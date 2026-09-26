using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Holds the mutable execution state for one immutable job during a single scheduling run.
/// </summary>
internal sealed class JobRuntime(JobDefinition definition)
{
    public JobDefinition Definition { get; } = definition;

    public int RemainingTime { get; set; } = definition.BurstTime;

    public bool HasStarted { get; set; }

    public bool IsCompleted { get; set; }

    public int DynamicQueueLevel { get; set; } = definition.QueueLevel;

    public int QuantumUsed { get; set; }
}
