using AutoFixture.Xunit3;
using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;
using CpuSchedulingSimulator.Domain.Validation;

namespace CpuSchedulingSimulator.Domain.Tests;

/// <summary>
/// Verifies workload and policy-option validation limits and diagnostic messages.
/// </summary>
public sealed class SchedulingValidatorTests
{
    [Theory]
    [InlineAutoData(0)]
    [InlineAutoData(-1)]
    public void Validate_WhenBurstIsNotPositive_ShouldReturnSpecificError(int burst, string id)
    {
        var job = new JobDefinition(id[..Math.Min(id.Length, 20)], 0, burst, 1, 0);

        var errors = SchedulingValidator.GetErrors([job], new SchedulingOptions());

        Assert.Contains(errors, error => error.Contains("burst time", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_WhenIdsDuplicate_ShouldUseOrdinalUniqueness()
    {
        var jobs = new[]
        {
            GoldenScheduleTests.Job("P1", 0, 1),
            GoldenScheduleTests.Job("P1", 1, 1),
            GoldenScheduleTests.Job("p1", 2, 1),
        };

        var errors = SchedulingValidator.GetErrors(jobs, new SchedulingOptions());

        Assert.Single(errors, error => error.Contains("unique", StringComparison.Ordinal));
    }

    [Fact]
    public void Schedule_WhenQuantumIsInvalid_ShouldRejectBeforeSimulation()
    {
        var action = () => new RoundRobinAlgorithm().Schedule(
            [GoldenScheduleTests.Job("P1", 0, 1)],
            new SchedulingOptions(RoundRobinQuantum: 0));

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("Round Robin quantum", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_WhenTotalBurstExceedsBound_ShouldPreventResourceExhaustion()
    {
        var jobs = Enumerable.Range(1, 101)
            .Select(index => new JobDefinition($"P{index}", 0, 1_000, 1, 0))
            .ToArray();

        var errors = SchedulingValidator.GetErrors(jobs, new SchedulingOptions());

        Assert.Contains(errors, error => error.Contains("At most", StringComparison.Ordinal));
        Assert.Contains(errors, error => error.Contains("Total burst", StringComparison.Ordinal));
    }

    [Fact]
    public void Schedule_WhenJobIdContainsMarkup_ShouldTreatItAsOpaqueText()
    {
        const string id = "<script>";

        var result = new FirstComeFirstServedAlgorithm().Schedule(
            [GoldenScheduleTests.Job(id, 0, 1)],
            new SchedulingOptions());

        Assert.Equal(id, Assert.Single(result.JobMetrics).JobId);
    }
}
