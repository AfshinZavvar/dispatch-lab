using CpuSchedulingSimulator.Domain.Models;
using CpuSchedulingSimulator.Domain.Validation;

namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Executes deterministic, discrete-time CPU simulations by combining shared lifecycle rules with an <see cref="ISchedulingPolicy"/>.
/// </summary>
internal static class TickSchedulingEngine
{
    public static SchedulingResult Run(SchedulingAlgorithmMetadata metadata, IReadOnlyCollection<JobDefinition> jobs, SchedulingOptions options, ISchedulingPolicy policy)
    {
        SchedulingValidator.ValidateAndThrow(jobs, options);
        if (jobs.Count == 0)
        {
            return SchedulingResult.Empty(metadata, options);
        }

        var canonicalJobs = jobs
            .OrderBy(job => job.ArrivalTime)
            .ThenBy(job => job.Id, StringComparer.Ordinal)
            .ToArray();
        var runtimes = canonicalJobs.Select(job => new JobRuntime(job)).ToArray();
        var recorder = new SimulationRecorder(runtimes);
        var time = 0;
        var nextArrival = 0;
        var completed = 0;
        JobRuntime? running = null;
        JobRuntime? pendingQuantumExpiration = null;
        var sliceStart = 0;

        while (completed < runtimes.Length)
        {
            while (nextArrival < runtimes.Length && runtimes[nextArrival].Definition.ArrivalTime == time)
            {
                var arrived = runtimes[nextArrival++];
                recorder.Record(
                    time,
                    SimulationEventKind.JobArrived,
                    arrived,
                    $"{arrived.Definition.Id} arrived with {arrived.Definition.BurstTime} ticks of CPU work.",
                    policy,
                    null,
                    running);
                policy.Admit(arrived, time);
                recorder.Record(
                    time,
                    SimulationEventKind.JobQueued,
                    arrived,
                    $"{arrived.Definition.Id} joined {policy.GetQueueLabel(arrived)}.",
                    policy,
                    null,
                    running);
            }

            if (pendingQuantumExpiration is not null)
            {
                var expired = pendingQuantumExpiration;
                var explanation = policy.ExplainQuantumExpiration(expired, time);
                policy.RequeueAfterQuantum(expired, time);
                recorder.Record(
                    time,
                    SimulationEventKind.JobPreempted,
                    expired,
                    explanation,
                    policy,
                    null,
                    null);
                pendingQuantumExpiration = null;
            }

            if (policy.ShouldBoost(time))
            {
                if (running is not null)
                {
                    CloseRunningSlice(recorder, policy, running, sliceStart, time, SliceEndReason.PriorityBoost);
                }

                var boostedRunning = running;
                running = null;
                policy.ApplyBoost(boostedRunning, time);
                recorder.Record(
                    time,
                    SimulationEventKind.PriorityBoost,
                    null,
                    policy.ExplainBoost(time),
                    policy,
                    null,
                    null);
            }

            if (running is not null && policy.ShouldPreempt(running, time))
            {
                var preempted = running;
                var explanation = policy.ExplainPreemption(preempted, time);
                CloseRunningSlice(recorder, policy, preempted, sliceStart, time, SliceEndReason.Preempted);
                running = null;
                policy.RequeuePreempted(preempted, time);
                recorder.Record(
                    time,
                    SimulationEventKind.JobPreempted,
                    preempted,
                    explanation,
                    policy,
                    null,
                    null);
            }

            if (running is null)
            {
                var selected = policy.DequeueNext(time);
                if (selected is null)
                {
                    var nextTime = runtimes[nextArrival].Definition.ArrivalTime;
                    recorder.Record(
                        time,
                        SimulationEventKind.CpuIdle,
                        null,
                        $"No job is ready. The CPU is idle until time {nextTime}.",
                        policy,
                        null,
                        null);
                    recorder.AddSlice(time, nextTime, null, SliceEndReason.Idle);
                    time = nextTime;
                    continue;
                }

                policy.OnDispatched(selected, time);
                recorder.Record(
                    time,
                    SimulationEventKind.JobSelected,
                    selected,
                    policy.ExplainSelection(selected, time),
                    policy,
                    selected,
                    null);

                var hasStarted = selected.HasStarted;
                selected.HasStarted = true;
                running = selected;
                sliceStart = time;
                recorder.Record(
                    time,
                    hasStarted ? SimulationEventKind.JobResumed : SimulationEventKind.JobStarted,
                    selected,
                    hasStarted
                        ? $"{selected.Definition.Id} resumed with {selected.RemainingTime} ticks remaining."
                        : $"{selected.Definition.Id} started its first CPU execution.",
                    policy,
                    null,
                    running);
            }

            running.RemainingTime--;
            policy.OnTickExecuted(running);
            time++;

            if (running.RemainingTime == 0)
            {
                var finished = running;
                CloseRunningSlice(recorder, policy, finished, sliceStart, time, SliceEndReason.Completed);
                finished.IsCompleted = true;
                running = null;
                completed++;
                recorder.MarkCompleted(finished);
                recorder.Record(
                    time,
                    SimulationEventKind.JobCompleted,
                    finished,
                    $"{finished.Definition.Id} completed all {finished.Definition.BurstTime} ticks of CPU work.",
                    policy,
                    null,
                    null);
            }
            else if (policy.HasQuantumExpired(running))
            {
                var expired = running;
                CloseRunningSlice(recorder, policy, expired, sliceStart, time, SliceEndReason.QuantumExpired);
                running = null;
                pendingQuantumExpiration = expired;
            }
        }

        return SchedulingResultFactory.Create(metadata, canonicalJobs, options, recorder.Slices, recorder.Events);
    }

    private static void CloseRunningSlice(SimulationRecorder recorder, ISchedulingPolicy policy, JobRuntime running, int sliceStart, int time, SliceEndReason endReason)
    {
        recorder.AddSlice(sliceStart, time, running, endReason);
        recorder.Record(
            time,
            SimulationEventKind.JobExecuted,
            running,
            $"{running.Definition.Id} ran from {sliceStart} to {time}; {running.RemainingTime} ticks remain.",
            policy,
            null,
            running);
    }
}
