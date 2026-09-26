using System.Collections.ObjectModel;

namespace CpuSchedulingSimulator.Domain.Models;

/// <summary>
/// Freezes the observable scheduler state attached to a recorded simulation event for deterministic playback.
/// </summary>
public sealed record SchedulerSnapshot(IReadOnlyList<string> ReadyQueue, string? SelectedJobId, string? RunningJobId, IReadOnlyList<string> CompletedJobIds, IReadOnlyDictionary<string, int> RemainingTimes)
{
    public static SchedulerSnapshot Empty { get; } = new(
        Array.Empty<string>(),
        null,
        null,
        Array.Empty<string>(),
        new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(StringComparer.Ordinal)));
}
