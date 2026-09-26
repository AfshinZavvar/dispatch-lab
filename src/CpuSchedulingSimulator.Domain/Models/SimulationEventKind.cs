namespace CpuSchedulingSimulator.Domain.Models;

/// <summary>
/// Classifies the lifecycle and scheduling decisions emitted into a recorded trace.
/// </summary>
public enum SimulationEventKind
{
    JobArrived,
    JobQueued,
    JobSelected,
    JobStarted,
    JobExecuted,
    JobPreempted,
    JobResumed,
    JobCompleted,
    QuantumExpired,
    CpuIdle,
    PriorityBoost,
}
