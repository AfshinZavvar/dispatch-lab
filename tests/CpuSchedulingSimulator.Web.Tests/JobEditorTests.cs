using Bunit;
using CpuSchedulingSimulator.Web.Components;
using CpuSchedulingSimulator.Web.State;

namespace CpuSchedulingSimulator.Web.Tests;

/// <summary>
/// Verifies editable workload fields and their domain conversion behavior.
/// </summary>
public sealed class JobEditorTests
{
    [Fact]
    public void Render_WhenFiveJobsProvided_ShouldShowEditableLabeledRows()
    {
        using var context = new BunitContext();
        var jobs = Enumerable.Range(1, 5)
            .Select(index => new EditableJob
            {
                Id = $"P{index}",
                ArrivalTime = index - 1,
                BurstTime = index,
                Priority = index,
                QueueLevel = index % 3,
            })
            .ToArray();

        var cut = context.Render<JobEditor>(parameters => parameters.Add(component => component.Jobs, jobs));

        Assert.Equal(5, cut.FindAll("tbody tr").Count);
        Assert.Equal(5, cut.FindAll("input[type=number]").Count / 3);
        cut.Find("input[aria-label='P1 burst time']").Change("7");
        Assert.Equal(7, jobs[0].BurstTime);
        Assert.Contains("Q2 · background", cut.Markup, StringComparison.Ordinal);
    }
}
