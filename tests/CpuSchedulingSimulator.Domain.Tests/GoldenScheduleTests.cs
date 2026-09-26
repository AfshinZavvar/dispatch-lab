using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Tests;

/// <summary>
/// Verifies canonical execution slices and metrics for every supported scheduling strategy.
/// </summary>
public sealed class GoldenScheduleTests
{
    private static readonly SchedulingOptions DefaultOptions = new();

    [Fact]
    public void Fcfs_WhenArrivalsAreStaggered_ShouldRunInArrivalOrderWithExpectedMetrics()
    {
        var jobs = new[]
        {
            Job("P1", 0, 5),
            Job("P2", 1, 3),
            Job("P3", 2, 1),
        };

        var result = new FirstComeFirstServedAlgorithm().Schedule(jobs, DefaultOptions);

        AssertSlices(result, (0, 5, "P1"), (5, 8, "P2"), (8, 9, "P3"));
        AssertMetrics(result, "P1", first: 0, completion: 5, waiting: 0, response: 0, turnaround: 5);
        AssertMetrics(result, "P2", first: 5, completion: 8, waiting: 4, response: 4, turnaround: 7);
        AssertMetrics(result, "P3", first: 8, completion: 9, waiting: 6, response: 6, turnaround: 7);
        Assert.Equal(10.0 / 3, result.Metrics.AverageWaitingTime, 10);
        Assert.Equal(2, result.Metrics.ContextSwitches);
        Assert.Equal(100, result.Metrics.CpuUtilizationPercent);
    }

    [Fact]
    public void Sjf_WhenShortJobsArriveDuringExecution_ShouldWaitThenChooseShortestReady()
    {
        var jobs = new[]
        {
            Job("P1", 0, 5),
            Job("P2", 1, 3),
            Job("P3", 2, 1),
        };

        var result = new ShortestJobFirstAlgorithm().Schedule(jobs, DefaultOptions);

        AssertSlices(result, (0, 5, "P1"), (5, 6, "P3"), (6, 9, "P2"));
        AssertMetrics(result, "P1", 0, 5, 0, 0, 5);
        AssertMetrics(result, "P2", 6, 9, 5, 5, 8);
        AssertMetrics(result, "P3", 5, 6, 3, 3, 4);
    }

    [Fact]
    public void Srtf_WhenSuccessivelyShorterJobsArrive_ShouldPreemptTwice()
    {
        var jobs = new[]
        {
            Job("P1", 0, 8),
            Job("P2", 1, 4),
            Job("P3", 2, 2),
        };

        var result = new ShortestRemainingTimeFirstAlgorithm().Schedule(jobs, DefaultOptions);

        AssertSlices(result,
            (0, 1, "P1"),
            (1, 2, "P2"),
            (2, 4, "P3"),
            (4, 7, "P2"),
            (7, 14, "P1"));
        AssertMetrics(result, "P1", 0, 14, 6, 0, 14);
        AssertMetrics(result, "P2", 1, 7, 2, 0, 6);
        AssertMetrics(result, "P3", 2, 4, 0, 0, 2);
        Assert.Equal(4, result.Metrics.ContextSwitches);
        Assert.Equal(2, result.Events.Count(item => item.Kind == SimulationEventKind.JobPreempted));
    }

    [Fact]
    public void RoundRobin_WhenJobsArriveAtQuantumBoundary_ShouldAdmitBeforeRequeue()
    {
        var jobs = new[]
        {
            Job("P1", 0, 5),
            Job("P2", 1, 3),
            Job("P3", 2, 1),
        };

        var result = new RoundRobinAlgorithm().Schedule(jobs, DefaultOptions);

        AssertSlices(result,
            (0, 2, "P1"),
            (2, 4, "P2"),
            (4, 5, "P3"),
            (5, 7, "P1"),
            (7, 8, "P2"),
            (8, 9, "P1"));
        AssertMetrics(result, "P1", 0, 9, 4, 0, 9);
        AssertMetrics(result, "P2", 2, 8, 4, 1, 7);
        AssertMetrics(result, "P3", 4, 5, 2, 2, 3);
        Assert.Equal(5, result.Metrics.ContextSwitches);
    }

    [Fact]
    public void PriorityNonPreemptive_WhenHigherPrioritiesArrive_ShouldFinishCurrentThenUsePriority()
    {
        var jobs = new[]
        {
            Job("P1", 0, 4, priority: 3),
            Job("P2", 1, 2, priority: 1),
            Job("P3", 1, 1, priority: 2),
        };

        var result = new PriorityNonPreemptiveAlgorithm().Schedule(jobs, DefaultOptions);

        AssertSlices(result, (0, 4, "P1"), (4, 6, "P2"), (6, 7, "P3"));
        AssertMetrics(result, "P1", 0, 4, 0, 0, 4);
        AssertMetrics(result, "P2", 4, 6, 3, 3, 5);
        AssertMetrics(result, "P3", 6, 7, 5, 5, 6);
    }

    [Fact]
    public void PriorityPreemptive_WhenHigherPrioritiesArrive_ShouldRepeatedlyPreempt()
    {
        var jobs = new[]
        {
            Job("P1", 0, 5, priority: 3),
            Job("P2", 1, 3, priority: 2),
            Job("P3", 2, 1, priority: 1),
        };

        var result = new PriorityPreemptiveAlgorithm().Schedule(jobs, DefaultOptions);

        AssertSlices(result,
            (0, 1, "P1"),
            (1, 2, "P2"),
            (2, 3, "P3"),
            (3, 5, "P2"),
            (5, 9, "P1"));
        AssertMetrics(result, "P1", 0, 9, 4, 0, 9);
        AssertMetrics(result, "P2", 1, 5, 1, 0, 4);
        AssertMetrics(result, "P3", 2, 3, 0, 0, 1);
    }

    [Fact]
    public void Hrrn_WhenRatiosTie_ShouldUseArrivalTieBreakInsteadOfShortestBurst()
    {
        var jobs = new[]
        {
            Job("P1", 0, 3),
            Job("P2", 0, 6),
            Job("P3", 2, 2),
        };

        var result = new HighestResponseRatioNextAlgorithm().Schedule(jobs, DefaultOptions);

        AssertSlices(result, (0, 3, "P1"), (3, 9, "P2"), (9, 11, "P3"));
        AssertMetrics(result, "P1", 0, 3, 0, 0, 3);
        AssertMetrics(result, "P2", 3, 9, 3, 3, 9);
        AssertMetrics(result, "P3", 9, 11, 7, 7, 9);
    }

    [Fact]
    public void Mlq_WhenHigherFixedQueuesArrive_ShouldPreemptAndResumeQueueHeads()
    {
        var jobs = new[]
        {
            Job("P1", 0, 5, queue: 2),
            Job("P2", 1, 3, queue: 1),
            Job("P3", 2, 2, queue: 0),
        };

        var result = new MultiLevelQueueAlgorithm().Schedule(jobs, DefaultOptions);

        AssertSlices(result,
            (0, 1, "P1"),
            (1, 2, "P2"),
            (2, 4, "P3"),
            (4, 6, "P2"),
            (6, 10, "P1"));
        AssertMetrics(result, "P1", 0, 10, 5, 0, 10);
        AssertMetrics(result, "P2", 1, 6, 2, 0, 5);
        AssertMetrics(result, "P3", 2, 4, 0, 0, 2);
    }

    [Fact]
    public void Mlfq_WhenJobsConsumeFullQuanta_ShouldDemoteAndRotate()
    {
        var jobs = new[]
        {
            Job("P1", 0, 5),
            Job("P2", 0, 3),
        };

        var result = new MultiLevelFeedbackQueueAlgorithm().Schedule(jobs, DefaultOptions);

        AssertSlices(result,
            (0, 1, "P1"),
            (1, 2, "P2"),
            (2, 4, "P1"),
            (4, 6, "P2"),
            (6, 8, "P1"));
        AssertMetrics(result, "P1", 0, 8, 3, 0, 8);
        AssertMetrics(result, "P2", 1, 6, 3, 1, 6);
    }

    [Fact]
    public void EveryAlgorithm_WhenInputIsEmpty_ShouldReturnEmptyZeroResult()
    {
        foreach (var algorithm in AllAlgorithms())
        {
            var result = algorithm.Schedule(Array.Empty<JobDefinition>(), DefaultOptions);

            Assert.Empty(result.ExecutionSlices);
            Assert.Empty(result.Events);
            Assert.Empty(result.JobMetrics);
            Assert.Equal(0, result.Metrics.Makespan);
            Assert.Equal(0, result.Metrics.CpuUtilizationPercent);
        }
    }

    internal static IReadOnlyList<ICpuSchedulingAlgorithm> AllAlgorithms() =>
    [
        new FirstComeFirstServedAlgorithm(),
        new ShortestJobFirstAlgorithm(),
        new ShortestRemainingTimeFirstAlgorithm(),
        new RoundRobinAlgorithm(),
        new PriorityNonPreemptiveAlgorithm(),
        new PriorityPreemptiveAlgorithm(),
        new HighestResponseRatioNextAlgorithm(),
        new MultiLevelQueueAlgorithm(),
        new MultiLevelFeedbackQueueAlgorithm(),
    ];

    internal static JobDefinition Job(string id, int arrival, int burst, int priority = 5, int queue = 1) => new(id, arrival, burst, priority, queue);

    internal static void AssertSlices(SchedulingResult result, params (int Start, int End, string? Job)[] expected) =>
        Assert.Equal(
            expected,
            result.ExecutionSlices.Select(slice => (slice.StartTime, slice.EndTime, slice.JobId)).ToArray());

    internal static void AssertMetrics(SchedulingResult result, string jobId, int first, int completion, int waiting, int response, int turnaround)
    {
        var metrics = Assert.Single(result.JobMetrics, metric => metric.JobId == jobId);
        Assert.Equal(first, metrics.FirstStartTime);
        Assert.Equal(completion, metrics.CompletionTime);
        Assert.Equal(waiting, metrics.WaitingTime);
        Assert.Equal(response, metrics.ResponseTime);
        Assert.Equal(turnaround, metrics.TurnaroundTime);
        Assert.Equal(metrics.BurstTime, metrics.CpuExecutionTime);
    }
}
