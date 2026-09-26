using Bunit;
using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;
using CpuSchedulingSimulator.Web.Components;

namespace CpuSchedulingSimulator.Web.Tests;

/// <summary>
/// Verifies algorithm selection controls and their user-visible selection state.
/// </summary>
public sealed class AlgorithmSelectorTests
{
    [Fact]
    public void Toggle_WhenCardClicked_ShouldReportAlgorithmAndExposeSelectionText()
    {
        using var context = new BunitContext();
        SchedulingAlgorithmId? toggled = null;
        IReadOnlySet<SchedulingAlgorithmId> selected = new HashSet<SchedulingAlgorithmId>
        {
            SchedulingAlgorithmId.FirstComeFirstServed,
        };

        var cut = context.Render<AlgorithmSelector>(parameters => parameters
            .Add(component => component.Algorithms, SchedulingMetadata.All)
            .Add(component => component.Selected, selected)
            .Add(component => component.Toggled, id => toggled = id));

        Assert.Equal(9, cut.FindAll(".algorithm-card").Count);
        Assert.Contains("1 selected", cut.Markup, StringComparison.Ordinal);
        cut.Find($"#algorithm-{SchedulingAlgorithmId.RoundRobin}").Change(true);
        Assert.Equal(SchedulingAlgorithmId.RoundRobin, toggled);
        Assert.Contains("preemptive", cut.Markup, StringComparison.Ordinal);
    }
}
