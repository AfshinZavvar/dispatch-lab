using System.Collections.ObjectModel;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Records execution slices and immutable scheduler snapshots while one simulation is running.
/// </summary>
internal sealed class SimulationRecorder(IReadOnlyList<JobRuntime> jobs)
{
    private readonly List<ExecutionSlice> slices = [];
    private readonly List<SimulationEvent> events = [];
    private readonly List<string> completedJobIds = [];

    public IReadOnlyList<ExecutionSlice> Slices => slices;

    public IReadOnlyList<SimulationEvent> Events => events;

    public void AddSlice(int start, int end, JobRuntime? job, SliceEndReason endReason)
    {
        if (end <= start)
        {
            throw new InvalidOperationException($"A slice must have positive length, but [{start}, {end}) was requested.");
        }

        slices.Add(new ExecutionSlice(start, end, job?.Definition.Id, endReason));
    }

    public void MarkCompleted(JobRuntime job) => completedJobIds.Add(job.Definition.Id);

    public void Record(int time, SimulationEventKind kind, JobRuntime? subject, string explanation, ISchedulingPolicy policy, JobRuntime? selected, JobRuntime? running)
    {
        var readyIds = policy.GetReadyQueue(time).Select(job => job.Definition.Id).ToArray();
        var remaining = jobs.ToDictionary(
            job => job.Definition.Id,
            job => job.RemainingTime,
            StringComparer.Ordinal);
        var snapshot = new SchedulerSnapshot(
            Array.AsReadOnly(readyIds),
            selected?.Definition.Id,
            running?.Definition.Id,
            Array.AsReadOnly(completedJobIds.ToArray()),
            new ReadOnlyDictionary<string, int>(remaining));

        events.Add(new SimulationEvent(
            events.Count,
            time,
            kind,
            subject?.Definition.Id,
            explanation,
            snapshot));
    }
}
