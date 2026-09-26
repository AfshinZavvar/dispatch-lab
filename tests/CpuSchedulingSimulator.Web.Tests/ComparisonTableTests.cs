using Bunit;
using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;
using CpuSchedulingSimulator.Web.Components;

namespace CpuSchedulingSimulator.Web.Tests;

/// <summary>
/// Verifies side-by-side metric comparison rendering for completed algorithms.
/// </summary>
public sealed class ComparisonTableTests
{
    [Fact]
    public void Render_WhenMultipleResultsProvided_ShouldShowMetricsAndTradeOffs()
    {
        using var context = new BunitContext();
        var jobs = new[]
        {
            new JobDefinition("P1", 0, 4, 2, 0),
            new JobDefinition("P2", 1, 1, 1, 1),
        };
        var options = new SchedulingOptions();
        var results = new SchedulingResult[]
        {
            new FirstComeFirstServedAlgorithm().Schedule(jobs, options),
            new ShortestRemainingTimeFirstAlgorithm().Schedule(jobs, options),
        };

        var cut = context.Render<ComparisonTable>(parameters => parameters.Add(component => component.Results, results));

        Assert.Equal(2, cut.FindAll(".comparison-table tbody tr").Count);
        Assert.Equal(2, cut.FindAll(".tradeoff-grid article").Count);
        Assert.Contains("Same jobs. Different policy.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("not the only goal", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }
}
