using CpuSchedulingSimulator.Application.Jobs;

namespace CpuSchedulingSimulator.Application.Tests;

/// <summary>
/// Verifies that seeded workloads are deterministic, valid, and sufficiently varied.
/// </summary>
public sealed class SeededJobGeneratorTests
{
    [Fact]
    public void Generate_WhenSeedIsRepeated_ShouldReturnSameFiveInstructionalJobs()
    {
        var first = SeededJobGenerator.Generate(42);
        var second = SeededJobGenerator.Generate(42);

        Assert.Equal(SeededJobGenerator.DefaultJobCount, first.Count);
        Assert.Equal(first, second);
        Assert.Contains(first, job => job.ArrivalTime == 0 && job.BurstTime >= 8);
        Assert.Contains(first, job => job.BurstTime <= 3);
        Assert.Equal(3, first.Select(job => job.QueueLevel).Distinct().Count());
        Assert.Equal(5, first.Select(job => job.Priority).Distinct().Count());
    }

    [Fact]
    public void Generate_WhenSeedChanges_ShouldChangeAtLeastOneSchedulingInput()
    {
        var first = SeededJobGenerator.Generate(1);
        var second = SeededJobGenerator.Generate(2);

        Assert.NotEqual(first, second);
    }
}
