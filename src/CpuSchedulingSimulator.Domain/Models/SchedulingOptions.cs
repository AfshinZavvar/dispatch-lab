namespace CpuSchedulingSimulator.Domain.Models;

/// <summary>
/// Groups the tunable quantum and boost settings consumed by configurable scheduling policies.
/// </summary>
public sealed record SchedulingOptions(int RoundRobinQuantum = 2, int MultiLevelQueueHighQuantum = 2, int MultiLevelQueueNormalQuantum = 4, int MlfqHighQuantum = 1, int MlfqNormalQuantum = 2, int MlfqLowQuantum = 4, int MlfqBoostInterval = 20)
{
    public int GetMlfqQuantum(int level) => level switch
    {
        0 => MlfqHighQuantum,
        1 => MlfqNormalQuantum,
        2 => MlfqLowQuantum,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "MLFQ level must be from 0 through 2."),
    };
}
