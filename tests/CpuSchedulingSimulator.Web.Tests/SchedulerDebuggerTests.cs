using Bunit;
using CpuSchedulingSimulator.Application.Playback;
using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;
using CpuSchedulingSimulator.Web.Components;

namespace CpuSchedulingSimulator.Web.Tests;

/// <summary>
/// Verifies recorded scheduler playback controls and event-state presentation.
/// </summary>
public sealed class SchedulerDebuggerTests
{
    [Fact]
    public void Render_WhenSteppingRecordedEvents_ShouldShowReadyRunningAndCompletedStates()
    {
        using var context = new BunitContext();
        var result = new RoundRobinAlgorithm().Schedule(
            [
                new JobDefinition("P1", 0, 2, 1, 0),
                new JobDefinition("P2", 0, 1, 2, 1),
            ],
            new SchedulingOptions(RoundRobinQuantum: 1));
        var playback = new SimulationPlayback(result);
        var cut = context.Render<SchedulerDebugger>(parameters => parameters
            .Add(component => component.Result, result)
            .Add(component => component.Playback, playback));

        Assert.Contains("Ready to trace", cut.Markup, StringComparison.OrdinalIgnoreCase);
        StepUntil(playback, SimulationEventKind.JobQueued);
        cut.Render();
        Assert.NotEmpty(cut.FindAll(".queue-job"));
        StepUntil(playback, SimulationEventKind.JobStarted);
        cut.Render();
        Assert.Equal("P1", cut.Find("[data-testid='running-job'] > strong").TextContent);
        StepUntil(playback, SimulationEventKind.JobCompleted);
        cut.Render();
        Assert.NotEmpty(cut.FindAll(".completed-job"));
        Assert.Equal(result.ExecutionSlices.Count, cut.FindAll(".gantt-segment").Count);
        Assert.Equal(result.JobMetrics.Count, cut.FindAll(".metrics-detail tbody tr").Count);
    }

    [Fact]
    public void Render_WhenJobIdContainsMarkup_ShouldHtmlEncodeIt()
    {
        using var context = new BunitContext();
        var result = new FirstComeFirstServedAlgorithm().Schedule(
            [new JobDefinition("<script>", 0, 1, 1, 0)],
            new SchedulingOptions());
        var playback = new SimulationPlayback(result);
        playback.StepForward();

        var cut = context.Render<SchedulerDebugger>(parameters => parameters
            .Add(component => component.Result, result)
            .Add(component => component.Playback, playback));

        Assert.DoesNotContain("<script>", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;script&gt;", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    private static void StepUntil(SimulationPlayback playback, SimulationEventKind kind)
    {
        while (playback.CanStepForward && playback.CurrentEvent?.Kind != kind)
        {
            playback.StepForward();
        }

        Assert.Equal(kind, playback.CurrentEvent?.Kind);
    }
}
