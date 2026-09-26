using CpuSchedulingSimulator.Application.Playback;
using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Application.Tests;

/// <summary>
/// Verifies event-by-event playback state, timing, speed, and restart behavior.
/// </summary>
public sealed class SimulationPlaybackTests
{
    [Fact]
    public void StepForward_WhenReady_ShouldAdvanceOneMeaningfulEventAndPause()
    {
        var playback = CreatePlayback();

        playback.StepForward();

        Assert.Equal(0, playback.CurrentEventIndex);
        Assert.Equal(SimulationEventKind.JobArrived, playback.CurrentEvent?.Kind);
        Assert.Equal(PlaybackStatus.Paused, playback.Status);
    }

    [Fact]
    public void StartAndAdvanceTimer_WhenFinalEventReached_ShouldComplete()
    {
        var playback = CreatePlayback();

        playback.Start();
        while (playback.Status == PlaybackStatus.Playing)
        {
            playback.AdvanceTimer();
        }

        Assert.Equal(PlaybackStatus.Completed, playback.Status);
        Assert.Equal(playback.Result.Events.Count - 1, playback.CurrentEventIndex);
        Assert.Equal(100, playback.ProgressPercent);
    }

    [Fact]
    public void PauseResumeRestart_ShouldPreserveThenResetEventPosition()
    {
        var playback = CreatePlayback();
        playback.Start();
        playback.AdvanceTimer();
        playback.Pause();
        var pausedIndex = playback.CurrentEventIndex;

        playback.AdvanceTimer();
        Assert.Equal(pausedIndex, playback.CurrentEventIndex);
        playback.Resume();
        Assert.Equal(PlaybackStatus.Playing, playback.Status);
        playback.Restart();
        Assert.Equal(-1, playback.CurrentEventIndex);
        Assert.Equal(PlaybackStatus.Ready, playback.Status);
    }

    [Theory]
    [InlineData(0.5, 1800)]
    [InlineData(1, 900)]
    [InlineData(2, 450)]
    [InlineData(4, 225)]
    public void SetSpeed_WhenSupported_ShouldOnlyChangePlaybackDelay(double speed, int expectedDelay)
    {
        var playback = CreatePlayback();
        var originalSlices = playback.Result.ExecutionSlices.ToArray();

        playback.SetSpeed(speed);

        Assert.Equal(expectedDelay, playback.TimerDelayMilliseconds);
        Assert.Equal(originalSlices, playback.Result.ExecutionSlices);
    }

    [Fact]
    public void SetSpeed_WhenUnsupported_ShouldRejectValue()
    {
        var playback = CreatePlayback();

        var action = () => playback.SetSpeed(3);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    private static SimulationPlayback CreatePlayback()
    {
        var result = new FirstComeFirstServedAlgorithm().Schedule(
            [new JobDefinition("P1", 0, 2, 1, 0)],
            new SchedulingOptions());
        return new SimulationPlayback(result);
    }
}
