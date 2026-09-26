using Bunit;
using CpuSchedulingSimulator.Application.Algorithms;
using CpuSchedulingSimulator.Application.Simulation;
using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Web.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace CpuSchedulingSimulator.Web.Tests;

/// <summary>
/// Verifies the primary simulation workflow, validation feedback, and comparison playback UI.
/// </summary>
public sealed class HomeTests
{
    [Fact]
    public void InitialSetup_ShouldPlaceBuildActionAfterPolicySelectionAndOutsideOptionsPanel()
    {
        using var context = CreateContext();
        var cut = context.Render<Home>();

        var policyPosition = cut.Markup.IndexOf("algorithm-panel", StringComparison.Ordinal);
        var buildPosition = cut.Markup.IndexOf("build-traces-button", StringComparison.Ordinal);

        Assert.True(policyPosition >= 0);
        Assert.True(buildPosition > policyPosition);
        Assert.Empty(cut.Find(".control-console").QuerySelectorAll(".build-traces-button"));
        Assert.Contains("Build 3 scheduler traces", cut.Find(".build-traces-button").TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildTraces_WithDefaultWorkload_ShouldCollapseSetupAndRenderSynchronizedComparison()
    {
        using var context = CreateContext();
        var cut = context.Render<Home>();

        cut.Find(".build-traces-button").Click();

        Assert.Empty(cut.FindAll(".setup-grid"));
        Assert.Contains("3 traces · 5 jobs", cut.Find(".experiment-summary").TextContent, StringComparison.Ordinal);
        Assert.Equal(3, cut.FindAll(".algorithm-lane").Count);
        Assert.Equal(3, cut.FindAll(".compact-debugger").Count);
        Assert.NotNull(cut.Find(".shared-clock"));
        Assert.Empty(cut.FindAll(".result-tabs"));
        Assert.Equal(3, cut.FindAll(".comparison-table tbody tr").Count);
        Assert.Contains("same absolute tick", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SharedStep_ShouldRevealEverySelectedAlgorithmAtTheSameDecisionTime()
    {
        using var context = CreateContext();
        var cut = context.Render<Home>();
        cut.Find(".build-traces-button").Click();

        cut.FindAll(".comparison-transport button")[1].Click();

        Assert.Equal("00", cut.Find(".shared-clock strong").TextContent);
        Assert.All(
            cut.FindAll(".compact-decision p"),
            explanation => Assert.DoesNotContain("Ready on the shared clock", explanation.TextContent, StringComparison.Ordinal));
    }

    [Fact]
    public void InspectEvents_ShouldOpenTheExistingDetailedDebugger()
    {
        using var context = CreateContext();
        var cut = context.Render<Home>();
        cut.Find(".build-traces-button").Click();

        cut.Find(".inspect-button").Click();

        Assert.NotNull(cut.Find(".deep-inspector"));
        Assert.NotNull(cut.Find(".deep-inspector .debugger"));
        Assert.Contains("event debugger", cut.Find("#deep-inspector-title").TextContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EditExperiment_ShouldRestoreTheInputsWithoutDiscardingSelection()
    {
        using var context = CreateContext();
        var cut = context.Render<Home>();
        cut.Find(".build-traces-button").Click();

        cut.Find(".edit-experiment-button").Click();

        Assert.NotNull(cut.Find(".setup-grid"));
        Assert.Equal(3, cut.FindAll(".algorithm-card.is-selected").Count);
        Assert.Equal(5, cut.FindAll(".job-table tbody tr").Count);
    }

    [Fact]
    public void BuildTraces_WhenBurstIsInvalid_ShouldRenderValidationAndKeepSetupVisible()
    {
        using var context = CreateContext();
        var cut = context.Render<Home>();
        cut.Find("input[aria-label='P1 burst time']").Change("0");

        cut.Find(".build-traces-button").Click();

        Assert.Contains("burst time", cut.Find("[role='alert']").TextContent, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(cut.Find(".setup-grid"));
        Assert.Empty(cut.FindAll(".comparison-theater"));
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var algorithms = new ICpuSchedulingAlgorithm[]
        {
            new FirstComeFirstServedAlgorithm(),
            new ShortestJobFirstAlgorithm(),
            new ShortestRemainingTimeFirstAlgorithm(),
            new RoundRobinAlgorithm(),
            new PriorityNonPreemptiveAlgorithm(),
            new PriorityPreemptiveAlgorithm(),
            new HighestResponseRatioNextAlgorithm(),
            new MultiLevelQueueAlgorithm(),
            new MultiLevelFeedbackQueueAlgorithm(),
        };
        var catalog = new AlgorithmCatalog(algorithms);
        context.Services.AddSingleton<IAlgorithmCatalog>(catalog);
        context.Services.AddSingleton(new SimulationRunner(catalog));
        return context;
    }
}
