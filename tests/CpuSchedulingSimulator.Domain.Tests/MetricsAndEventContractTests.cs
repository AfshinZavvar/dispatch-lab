using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Tests;

/// <summary>
/// Verifies metric calculations and the stable event contract emitted by scheduling runs.
/// </summary>
public sealed class MetricsAndEventContractTests
{
    [Fact]
    public void Fcfs_WhenJobsHaveDifferentWaits_ShouldCalculateEveryAggregateMetric()
    {
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P1", 0, 4),
            GoldenScheduleTests.Job("P2", 1, 2),
            GoldenScheduleTests.Job("P3", 1, 1),
        };

        var result = new FirstComeFirstServedAlgorithm().Schedule(jobs, new SchedulingOptions());

        Assert.Equal(8.0 / 3, result.Metrics.AverageWaitingTime, 10);
        Assert.Equal(5, result.Metrics.AverageTurnaroundTime, 10);
        Assert.Equal(8.0 / 3, result.Metrics.AverageResponseTime, 10);
        Assert.Equal(100, result.Metrics.CpuUtilizationPercent, 10);
        Assert.Equal(3.0 / 7, result.Metrics.ThroughputPerTick, 10);
        Assert.Equal(2, result.Metrics.ContextSwitches);
        Assert.Equal(7, result.Metrics.BusyTime);
        Assert.Equal(7, result.Metrics.Makespan);
    }

    [Fact]
    public void RoundRobin_WhenOneJobCrossesTwoQuantumBoundaries_ShouldRecordStartThenResumes()
    {
        var result = new RoundRobinAlgorithm().Schedule(
            [GoldenScheduleTests.Job("P1", 0, 5)],
            new SchedulingOptions(RoundRobinQuantum: 2));

        var lifecycle = result.Events
            .Where(item => item.Kind is SimulationEventKind.JobStarted or SimulationEventKind.JobResumed)
            .ToArray();

        Assert.Equal(
            [SimulationEventKind.JobStarted, SimulationEventKind.JobResumed, SimulationEventKind.JobResumed],
            lifecycle.Select(item => item.Kind));
        Assert.StartsWith("P1 started its first CPU execution", lifecycle[0].Explanation, StringComparison.Ordinal);
        Assert.All(lifecycle[1..], item => Assert.Contains("resumed with", item.Explanation, StringComparison.Ordinal));
        Assert.Equal(3, result.Events.Count(item => item.Kind == SimulationEventKind.JobSelected));
        Assert.Equal(3, result.Events.Count(item => item.Kind == SimulationEventKind.JobExecuted));
    }

    [Fact]
    public void Mlq_WhenJobsShareHighQueue_ShouldRotateAtTheExactQuantumBoundary()
    {
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P1", 0, 3, queue: 0),
            GoldenScheduleTests.Job("P2", 0, 3, queue: 0),
        };
        var options = new SchedulingOptions(MultiLevelQueueHighQuantum: 2);

        var result = new MultiLevelQueueAlgorithm().Schedule(jobs, options);

        GoldenScheduleTests.AssertSlices(
            result,
            (0, 2, "P1"),
            (2, 4, "P2"),
            (4, 5, "P1"),
            (5, 6, "P2"));
        Assert.Equal(2, result.Events.Count(item =>
            item.Kind == SimulationEventKind.JobPreempted &&
            item.Explanation.Contains("rotates", StringComparison.Ordinal)));
    }

    [Fact]
    public void Mlq_WhenJobsShareNormalQueue_ShouldUseTheNormalQueueQuantum()
    {
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P1", 0, 4, queue: 1),
            GoldenScheduleTests.Job("P2", 0, 4, queue: 1),
        };
        var options = new SchedulingOptions(MultiLevelQueueNormalQuantum: 3);

        var result = new MultiLevelQueueAlgorithm().Schedule(jobs, options);

        GoldenScheduleTests.AssertSlices(
            result,
            (0, 3, "P1"),
            (3, 6, "P2"),
            (6, 7, "P1"),
            (7, 8, "P2"));
    }

    [Fact]
    public void Mlfq_WhenTopQueueJobArrivesDuringLowerQueueExecution_ShouldPreemptImmediately()
    {
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P1", 0, 5),
            GoldenScheduleTests.Job("P2", 2, 1),
        };
        var options = new SchedulingOptions(
            MlfqHighQuantum: 1,
            MlfqNormalQuantum: 4,
            MlfqLowQuantum: 8,
            MlfqBoostInterval: 100);

        var result = new MultiLevelFeedbackQueueAlgorithm().Schedule(jobs, options);

        GoldenScheduleTests.AssertSlices(result, (0, 1, "P1"), (1, 2, "P1"), (2, 3, "P2"), (3, 6, "P1"));
        var preemption = Assert.Single(result.Events, item =>
            item.Kind == SimulationEventKind.JobPreempted &&
            item.Explanation.Contains("higher feedback queue", StringComparison.Ordinal));
        Assert.Equal(2, preemption.Time);
    }

    [Fact]
    public void Mlfq_WhenJobsReachLowestQueue_ShouldRotateWithoutFurtherDemotion()
    {
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P1", 0, 5),
            GoldenScheduleTests.Job("P2", 0, 5),
        };
        var options = new SchedulingOptions(
            MlfqHighQuantum: 1,
            MlfqNormalQuantum: 1,
            MlfqLowQuantum: 2,
            MlfqBoostInterval: 100);

        var result = new MultiLevelFeedbackQueueAlgorithm().Schedule(jobs, options);

        GoldenScheduleTests.AssertSlices(
            result,
            (0, 1, "P1"),
            (1, 2, "P2"),
            (2, 3, "P1"),
            (3, 4, "P2"),
            (4, 6, "P1"),
            (6, 8, "P2"),
            (8, 9, "P1"),
            (9, 10, "P2"));
        Assert.Equal(2, result.Events.Count(item =>
            item.Kind == SimulationEventKind.JobPreempted &&
            item.Explanation.Contains("rotates within the lowest queue", StringComparison.Ordinal)));
    }
}
