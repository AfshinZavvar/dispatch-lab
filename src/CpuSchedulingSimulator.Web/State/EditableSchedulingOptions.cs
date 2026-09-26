using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Web.State;

/// <summary>
/// Provides mutable, UI-bindable policy settings and converts them to immutable domain options before simulation.
/// </summary>
public sealed class EditableSchedulingOptions
{
    public int RoundRobinQuantum { get; set; } = 2;

    public int MultiLevelQueueHighQuantum { get; set; } = 2;

    public int MultiLevelQueueNormalQuantum { get; set; } = 4;

    public int MlfqHighQuantum { get; set; } = 1;

    public int MlfqNormalQuantum { get; set; } = 2;

    public int MlfqLowQuantum { get; set; } = 4;

    public int MlfqBoostInterval { get; set; } = 20;

    public SchedulingOptions ToDomain() => new(
        RoundRobinQuantum,
        MultiLevelQueueHighQuantum,
        MultiLevelQueueNormalQuantum,
        MlfqHighQuantum,
        MlfqNormalQuantum,
        MlfqLowQuantum,
        MlfqBoostInterval);
}
