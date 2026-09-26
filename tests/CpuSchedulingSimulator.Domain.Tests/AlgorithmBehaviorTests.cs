using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Tests;

/// <summary>
/// Verifies algorithm-specific dispatch, preemption, quantum, queue, and boost behavior.
/// </summary>
public sealed class AlgorithmBehaviorTests
{
    [Fact]
    public void Fcfs_WhenCpuHasIdleGaps_ShouldAccountForIdleTimeAndUtilization()
    {
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P1", 2, 1),
            GoldenScheduleTests.Job("P2", 5, 2),
        };

        var result = new FirstComeFirstServedAlgorithm().Schedule(jobs, new SchedulingOptions());

        GoldenScheduleTests.AssertSlices(result, (0, 2, null), (2, 3, "P1"), (3, 5, null), (5, 7, "P2"));
        Assert.Equal(3, result.Metrics.BusyTime);
        Assert.Equal(7, result.Metrics.Makespan);
        Assert.Equal(300.0 / 7, result.Metrics.CpuUtilizationPercent, 10);
        Assert.Equal(0, result.Metrics.ContextSwitches);
        Assert.Equal(2, result.Events.Count(item => item.Kind == SimulationEventKind.CpuIdle));
    }

    [Fact]
    public void Sjf_WhenBurstTimesTie_ShouldUseArrivalThenOrdinalId()
    {
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P3", 0, 2),
            GoldenScheduleTests.Job("P1", 0, 2),
            GoldenScheduleTests.Job("P2", 0, 2),
        };

        var result = new ShortestJobFirstAlgorithm().Schedule(jobs, new SchedulingOptions());

        GoldenScheduleTests.AssertSlices(result, (0, 2, "P1"), (2, 4, "P2"), (4, 6, "P3"));
    }

    [Fact]
    public void Srtf_WhenArrivingJobHasEqualRemainingTime_ShouldKeepCurrentJob()
    {
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P1", 0, 4),
            GoldenScheduleTests.Job("P2", 2, 2),
        };

        var result = new ShortestRemainingTimeFirstAlgorithm().Schedule(jobs, new SchedulingOptions());

        GoldenScheduleTests.AssertSlices(result, (0, 4, "P1"), (4, 6, "P2"));
        Assert.DoesNotContain(result.Events, item => item.Kind == SimulationEventKind.JobPreempted);
    }

    [Fact]
    public void RoundRobin_WhenJobFinishesBeforeQuantum_ShouldDispatchNextImmediately()
    {
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P1", 0, 1),
            GoldenScheduleTests.Job("P2", 0, 4),
        };

        var result = new RoundRobinAlgorithm().Schedule(jobs, new SchedulingOptions(RoundRobinQuantum: 3));

        GoldenScheduleTests.AssertSlices(result, (0, 1, "P1"), (1, 4, "P2"), (4, 5, "P2"));
        Assert.Equal(1, result.Metrics.ContextSwitches);
    }

    [Fact]
    public void RoundRobin_WhenOnlyOneJobExists_ShouldRenewQuantumWithoutContextSwitch()
    {
        var result = new RoundRobinAlgorithm().Schedule(
            [GoldenScheduleTests.Job("P1", 0, 5)],
            new SchedulingOptions(RoundRobinQuantum: 2));

        GoldenScheduleTests.AssertSlices(result, (0, 2, "P1"), (2, 4, "P1"), (4, 5, "P1"));
        Assert.Equal(0, result.Metrics.ContextSwitches);
        Assert.Equal(2, result.Events.Count(item => item.Kind == SimulationEventKind.JobPreempted));
    }

    [Fact]
    public void PriorityPreemptive_WhenEqualPriorityArrives_ShouldNotPreempt()
    {
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P1", 0, 4, priority: 2),
            GoldenScheduleTests.Job("P2", 1, 1, priority: 2),
        };

        var result = new PriorityPreemptiveAlgorithm().Schedule(jobs, new SchedulingOptions());

        GoldenScheduleTests.AssertSlices(result, (0, 4, "P1"), (4, 5, "P2"));
    }

    [Fact]
    public void Mlfq_WhenBoostBoundaryIsReached_ShouldRecordBoostAndResetToTopQueue()
    {
        var options = new SchedulingOptions(
            MlfqHighQuantum: 10,
            MlfqNormalQuantum: 10,
            MlfqLowQuantum: 10,
            MlfqBoostInterval: 3);
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P1", 0, 4),
            GoldenScheduleTests.Job("P2", 0, 2),
        };

        var result = new MultiLevelFeedbackQueueAlgorithm().Schedule(jobs, options);

        var boost = Assert.Single(result.Events, item => item.Kind == SimulationEventKind.PriorityBoost);
        Assert.Equal(3, boost.Time);
        Assert.Equal(["P1", "P2"], boost.Snapshot.ReadyQueue);
        Assert.Contains("prevent starvation", boost.Explanation, StringComparison.Ordinal);
    }
}
