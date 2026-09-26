namespace CpuSchedulingSimulator.Domain.Models;

/// <summary>
/// Identifies every scheduling strategy supported by the simulator and its catalog.
/// </summary>
public enum SchedulingAlgorithmId
{
    FirstComeFirstServed,
    ShortestJobFirst,
    ShortestRemainingTimeFirst,
    RoundRobin,
    PriorityNonPreemptive,
    PriorityPreemptive,
    HighestResponseRatioNext,
    MultiLevelQueue,
    MultiLevelFeedbackQueue,
}
