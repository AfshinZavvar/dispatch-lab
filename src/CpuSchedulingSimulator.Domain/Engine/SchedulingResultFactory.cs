using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Verifies a completed timeline and derives immutable per-job and aggregate metrics from it.
/// </summary>
internal static class SchedulingResultFactory
{
    public static SchedulingResult Create(SchedulingAlgorithmMetadata metadata, IReadOnlyList<JobDefinition> jobs, SchedulingOptions options, IReadOnlyList<ExecutionSlice> slices, IReadOnlyList<SimulationEvent> events)
    {
        ValidateInvariants(jobs, slices, events);

        var jobMetrics = jobs.Select(job => CreateJobMetrics(job, slices)).ToArray();
        var busyTime = slices.Where(slice => !slice.IsIdle).Sum(slice => slice.Duration);
        var makespan = slices.Count == 0 ? 0 : slices[^1].EndTime;
        var contextSwitches = CountContextSwitches(slices);
        var count = jobMetrics.Length;
        var metrics = new AlgorithmMetrics(
            count == 0 ? 0 : jobMetrics.Average(metric => metric.WaitingTime),
            count == 0 ? 0 : jobMetrics.Average(metric => metric.TurnaroundTime),
            count == 0 ? 0 : jobMetrics.Average(metric => metric.ResponseTime),
            makespan == 0 ? 0 : busyTime * 100.0 / makespan,
            makespan == 0 ? 0 : count / (double)makespan,
            contextSwitches,
            busyTime,
            makespan);

        return new SchedulingResult(
            metadata,
            Array.AsReadOnly(jobs.ToArray()),
            options,
            Array.AsReadOnly(slices.ToArray()),
            Array.AsReadOnly(events.ToArray()),
            Array.AsReadOnly(jobMetrics),
            metrics);
    }

    private static JobMetrics CreateJobMetrics(JobDefinition job, IReadOnlyList<ExecutionSlice> slices)
    {
        var jobSlices = slices.Where(slice => StringComparer.Ordinal.Equals(slice.JobId, job.Id)).ToArray();
        if (jobSlices.Length == 0)
        {
            throw new InvalidOperationException($"Completed schedule contains no execution for {job.Id}.");
        }

        var firstStart = jobSlices[0].StartTime;
        var completion = jobSlices[^1].EndTime;
        var cpuTime = jobSlices.Sum(slice => slice.Duration);
        var turnaround = completion - job.ArrivalTime;
        return new JobMetrics(
            job.Id,
            job.ArrivalTime,
            job.BurstTime,
            firstStart,
            completion,
            cpuTime,
            turnaround,
            turnaround - job.BurstTime,
            firstStart - job.ArrivalTime);
    }

    private static int CountContextSwitches(IReadOnlyList<ExecutionSlice> slices)
    {
        ExecutionSlice? previousJobSlice = null;
        var switches = 0;

        foreach (var slice in slices.Where(slice => !slice.IsIdle))
        {
            if (previousJobSlice is not null &&
                previousJobSlice.EndTime == slice.StartTime &&
                !StringComparer.Ordinal.Equals(previousJobSlice.JobId, slice.JobId))
            {
                switches++;
            }

            previousJobSlice = slice;
        }

        return switches;
    }

    private static void ValidateInvariants(IReadOnlyList<JobDefinition> jobs, IReadOnlyList<ExecutionSlice> slices, IReadOnlyList<SimulationEvent> events)
    {
        if (jobs.Count == 0)
        {
            if (slices.Count != 0 || events.Count != 0)
            {
                throw new InvalidOperationException("An empty workload must produce an empty timeline.");
            }

            return;
        }

        var jobsById = jobs.ToDictionary(job => job.Id, StringComparer.Ordinal);
        var expectedStart = 0;
        foreach (var slice in slices)
        {
            if (slice.EndTime <= slice.StartTime)
            {
                throw new InvalidOperationException("Every execution slice must have positive length.");
            }

            if (slice.StartTime != expectedStart)
            {
                throw new InvalidOperationException("The timeline must account for every CPU tick without gaps or overlaps.");
            }

            expectedStart = slice.EndTime;
            if (slice.JobId is not null)
            {
                if (!jobsById.TryGetValue(slice.JobId, out var job))
                {
                    throw new InvalidOperationException($"Timeline references unknown job {slice.JobId}.");
                }

                if (slice.StartTime < job.ArrivalTime)
                {
                    throw new InvalidOperationException($"Job {job.Id} executes before it arrives.");
                }
            }
        }

        foreach (var job in jobs)
        {
            var executed = slices
                .Where(slice => StringComparer.Ordinal.Equals(slice.JobId, job.Id))
                .Sum(slice => slice.Duration);
            if (executed != job.BurstTime)
            {
                throw new InvalidOperationException($"Job {job.Id} executes for {executed} ticks instead of {job.BurstTime}.");
            }

            var completions = events.Count(simulationEvent =>
                simulationEvent.Kind == SimulationEventKind.JobCompleted &&
                StringComparer.Ordinal.Equals(simulationEvent.JobId, job.Id));
            if (completions != 1)
            {
                throw new InvalidOperationException($"Job {job.Id} must complete exactly once.");
            }
        }
    }
}
