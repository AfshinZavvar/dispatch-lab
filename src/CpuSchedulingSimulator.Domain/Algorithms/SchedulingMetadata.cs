using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Algorithms;

/// <summary>
/// Provides the canonical names, explanations, trade-offs, and preemption flags displayed for every supported algorithm.
/// </summary>
public static class SchedulingMetadata
{
    public static SchedulingAlgorithmMetadata FirstComeFirstServed { get; } = new(
        SchedulingAlgorithmId.FirstComeFirstServed,
        "FCFS",
        "First Come First Served",
        "Runs the oldest ready job until it completes.",
        "Earliest arrival wins; job ID breaks simultaneous-arrival ties.",
        "Simple and low-overhead, but a long job can delay every job behind it.",
        false);

    public static SchedulingAlgorithmMetadata ShortestJobFirst { get; } = new(
        SchedulingAlgorithmId.ShortestJobFirst,
        "SJF",
        "Shortest Job First",
        "Runs the shortest burst currently available to completion.",
        "Smallest original burst wins; arrival and job ID break ties.",
        "Often lowers average waiting time, but later short jobs cannot interrupt and long jobs may wait.",
        false);

    public static SchedulingAlgorithmMetadata ShortestRemainingTimeFirst { get; } = new(
        SchedulingAlgorithmId.ShortestRemainingTimeFirst,
        "SRTF",
        "Shortest Remaining Time First",
        "Always favors the ready job with the least CPU work remaining.",
        "Strictly shorter remaining time preempts; equality keeps the current job.",
        "Responsive to short work, at the cost of more preemption and possible long-job starvation.",
        true);

    public static SchedulingAlgorithmMetadata RoundRobin { get; } = new(
        SchedulingAlgorithmId.RoundRobin,
        "RR",
        "Round Robin",
        "Rotates a FIFO ready queue after each time quantum.",
        "Queue head runs for at most the configured quantum, then returns to the tail.",
        "Fair and responsive; a small quantum increases context switches and turnaround overhead.",
        true);

    public static SchedulingAlgorithmMetadata PriorityNonPreemptive { get; } = new(
        SchedulingAlgorithmId.PriorityNonPreemptive,
        "Priority NP",
        "Priority Scheduling — Non-preemptive",
        "Runs the highest-priority ready job to completion.",
        "Lowest numeric priority wins; arrival and job ID break ties.",
        "Important work runs first, but static priorities can starve low-priority jobs.",
        false);

    public static SchedulingAlgorithmMetadata PriorityPreemptive { get; } = new(
        SchedulingAlgorithmId.PriorityPreemptive,
        "Priority P",
        "Priority Scheduling — Preemptive",
        "Immediately favors newly ready work with a strictly higher priority.",
        "Lower numeric priority preempts; equality keeps the current job.",
        "Fast response for urgent work, with switch overhead and starvation risk.",
        true);

    public static SchedulingAlgorithmMetadata HighestResponseRatioNext { get; } = new(
        SchedulingAlgorithmId.HighestResponseRatioNext,
        "HRRN",
        "Highest Response Ratio Next",
        "Balances short bursts with the amount of time each ready job has waited.",
        "Maximize (waiting + burst) / burst without floating-point comparisons.",
        "Aging reduces starvation, but burst lengths must be known and running jobs are not preempted.",
        false);

    public static SchedulingAlgorithmMetadata MultiLevelQueue { get; } = new(
        SchedulingAlgorithmId.MultiLevelQueue,
        "MLQ",
        "Multilevel Queue",
        "Uses strict fixed queues: interactive RR, standard RR, then background FCFS.",
        "Highest non-empty fixed queue wins; a higher queue preempts lower work.",
        "Workload classes are explicit, but strict queues can starve background jobs.",
        true);

    public static SchedulingAlgorithmMetadata MultiLevelFeedbackQueue { get; } = new(
        SchedulingAlgorithmId.MultiLevelFeedbackQueue,
        "MLFQ",
        "Multilevel Feedback Queue",
        "Adapts priority from observed CPU use and periodically boosts waiting work.",
        "New jobs start high; full quanta demote; the highest queue runs with RR.",
        "Approximates short-job responsiveness without burst estimates, but policy tuning is complex.",
        true);

    public static IReadOnlyList<SchedulingAlgorithmMetadata> All { get; } =
    [
        FirstComeFirstServed,
        ShortestJobFirst,
        ShortestRemainingTimeFirst,
        RoundRobin,
        PriorityNonPreemptive,
        PriorityPreemptive,
        HighestResponseRatioNext,
        MultiLevelQueue,
        MultiLevelFeedbackQueue,
    ];
}
