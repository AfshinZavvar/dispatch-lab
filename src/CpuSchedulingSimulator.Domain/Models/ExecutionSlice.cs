namespace CpuSchedulingSimulator.Domain.Models;

/// <summary>
/// Describes one contiguous interval of CPU work or idle time in a completed schedule.
/// </summary>
public sealed record ExecutionSlice(int StartTime, int EndTime, string? JobId, SliceEndReason EndReason)
{
    public int Duration => EndTime - StartTime;

    public bool IsIdle => JobId is null;
}
