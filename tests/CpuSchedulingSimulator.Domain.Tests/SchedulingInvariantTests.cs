using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Tests;

/// <summary>
/// Verifies timeline, determinism, immutability, and completion invariants across all algorithms.
/// </summary>
public sealed class SchedulingInvariantTests
{
    [Fact]
    public void EveryAlgorithm_ForDeterministicRandomWorkloads_ShouldSatisfyCoreInvariants()
    {
        foreach (var algorithm in GoldenScheduleTests.AllAlgorithms())
        {
            for (var seed = 0; seed < 20; seed++)
            {
                var random = new Random(seed);
                var jobs = Enumerable.Range(1, 8)
                    .Select(index => new JobDefinition(
                        $"P{index}",
                        random.Next(0, 8),
                        random.Next(1, 10),
                        random.Next(1, 6),
                        random.Next(0, 3)))
                    .ToArray();

                var result = algorithm.Schedule(jobs, new SchedulingOptions());

                Assert.Equal(jobs.Sum(job => job.BurstTime), result.Metrics.BusyTime);
                Assert.Equal(jobs.Length, result.JobMetrics.Count);
                Assert.All(result.ExecutionSlices, slice => Assert.True(slice.EndTime > slice.StartTime));
                Assert.All(result.JobMetrics, metric =>
                {
                    Assert.Equal(metric.BurstTime, metric.CpuExecutionTime);
                    Assert.True(metric.CompletionTime >= metric.ArrivalTime);
                    Assert.True(metric.WaitingTime >= 0);
                    Assert.True(metric.ResponseTime >= 0);
                });
                Assert.Equal(
                    jobs.Length,
                    result.Events.Count(item => item.Kind == SimulationEventKind.JobCompleted));
            }
        }
    }

    [Fact]
    public void EveryAlgorithm_WhenInvokedTwice_ShouldProduceStructurallyIdenticalTimelineAndEvents()
    {
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P3", 2, 3, priority: 2, queue: 0),
            GoldenScheduleTests.Job("P1", 0, 5, priority: 3, queue: 2),
            GoldenScheduleTests.Job("P2", 1, 2, priority: 1, queue: 1),
        };

        foreach (var algorithm in GoldenScheduleTests.AllAlgorithms())
        {
            var first = algorithm.Schedule(jobs, new SchedulingOptions());
            var second = algorithm.Schedule(jobs.Reverse().ToArray(), new SchedulingOptions());

            Assert.Equal(first.ExecutionSlices, second.ExecutionSlices);
            Assert.Equal(
                first.Events.Select(EventShape),
                second.Events.Select(EventShape));
            Assert.Equal(first.JobMetrics, second.JobMetrics);
            Assert.Equal(first.Metrics, second.Metrics);
        }
    }

    private static string EventShape(SimulationEvent item) => string.Join(
        '|',
        item.Sequence,
        item.Time,
        item.Kind,
        item.JobId,
        item.Explanation,
        string.Join(',', item.Snapshot.ReadyQueue),
        item.Snapshot.SelectedJobId,
        item.Snapshot.RunningJobId,
        string.Join(',', item.Snapshot.CompletedJobIds),
        string.Join(',', item.Snapshot.RemainingTimes.OrderBy(pair => pair.Key)));
}
