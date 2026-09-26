using CpuSchedulingSimulator.Application.Models;
using CpuSchedulingSimulator.Application.Playback;
using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Application.Tests;

/// <summary>
/// Verifies synchronized absolute-time playback across multiple scheduling results.
/// </summary>
public sealed class ComparisonPlaybackTests
{
    [Fact]
    public void StepToNextDecision_ShouldRevealAllAlgorithmsAtTheNextSharedTimestamp()
    {
        var playback = CreatePlayback();

        playback.StepToNextDecision();

        Assert.Equal(0, playback.CurrentTime);
        Assert.Equal("P1", playback.GetRunningJobId(playback.Comparison.Results[0]));
        Assert.Equal("P1", playback.GetRunningJobId(playback.Comparison.Results[1]));
        Assert.Equal(PlaybackStatus.Paused, playback.Status);
    }

    [Fact]
    public void AdvanceTimer_ShouldUseOneGlobalClockAndLeaveCompletedAlgorithmsFinished()
    {
        var playback = CreatePlayback();
        playback.Start();

        while (playback.Status == PlaybackStatus.Playing)
        {
            playback.AdvanceTimer();
        }

        Assert.Equal(playback.MaxTime, playback.CurrentTime);
        Assert.All(playback.Comparison.Results, result =>
            Assert.Equal(result.Jobs.Count, playback.GetSnapshot(result).CompletedJobIds.Count));
        Assert.Equal(100, playback.ProgressPercent);
    }

    [Fact]
    public void GetRemainingTime_BetweenEvents_ShouldProjectExecutedTicksFromRecordedSlices()
    {
        var playback = CreatePlayback();
        playback.Start();
        playback.AdvanceTimer();
        playback.AdvanceTimer();
        playback.AdvanceTimer();

        var fcfs = playback.Comparison.GetResult(SchedulingAlgorithmId.FirstComeFirstServed);
        Assert.Equal(2, playback.CurrentTime);
        Assert.Equal(3, playback.GetRemainingTime(fcfs, "P1"));
    }

    [Fact]
    public void Constructor_WhenOnlyOneResultProvided_ShouldRejectNonComparison()
    {
        var jobs = new[] { new JobDefinition("P1", 0, 1, 1, 0) };
        var result = new FirstComeFirstServedAlgorithm().Schedule(jobs, new SchedulingOptions());

        var action = () => new ComparisonPlayback(new SimulationComparison(jobs, [result]));

        Assert.Throws<ArgumentException>(action);
    }

    private static ComparisonPlayback CreatePlayback()
    {
        var jobs = new[]
        {
            new JobDefinition("P1", 0, 5, 2, 0),
            new JobDefinition("P2", 1, 1, 1, 1),
        };
        var options = new SchedulingOptions(RoundRobinQuantum: 2);
        var comparison = new SimulationComparison(
            jobs,
            [
                new FirstComeFirstServedAlgorithm().Schedule(jobs, options),
                new ShortestRemainingTimeFirstAlgorithm().Schedule(jobs, options),
            ]);
        return new ComparisonPlayback(comparison);
    }
}
