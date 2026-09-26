namespace CpuSchedulingSimulator.Domain.Models;

/// <summary>
/// Explains why an execution or idle slice ended at its recorded boundary.
/// </summary>
public enum SliceEndReason
{
    Completed,
    Preempted,
    QuantumExpired,
    PriorityBoost,
    Idle,
}
