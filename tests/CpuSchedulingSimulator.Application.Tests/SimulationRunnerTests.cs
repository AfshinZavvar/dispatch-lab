using CpuSchedulingSimulator.Application.Algorithms;
using CpuSchedulingSimulator.Application.Models;
using CpuSchedulingSimulator.Application.Simulation;
using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;
using NSubstitute;

namespace CpuSchedulingSimulator.Application.Tests;

/// <summary>
/// Verifies request validation and consistent multi-algorithm execution by the application runner.
/// </summary>
public sealed class SimulationRunnerTests
{
    [Fact]
    public void Run_WhenMultipleAlgorithmsSelected_ShouldUseTheSameImmutableDatasetInRequestedOrder()
    {
        var options = new SchedulingOptions();
        var jobs = new[] { new JobDefinition("P1", 0, 2, 1, 0) };
        var fcfs = CreateAlgorithm(SchedulingMetadata.FirstComeFirstServed, options);
        var roundRobin = CreateAlgorithm(SchedulingMetadata.RoundRobin, options);
        var catalog = Substitute.For<IAlgorithmCatalog>();
        catalog.Resolve(SchedulingAlgorithmId.FirstComeFirstServed).Returns(fcfs);
        catalog.Resolve(SchedulingAlgorithmId.RoundRobin).Returns(roundRobin);
        var capturedDatasets = new List<IReadOnlyCollection<JobDefinition>>();
        fcfs.When(algorithm => algorithm.Schedule(Arg.Any<IReadOnlyCollection<JobDefinition>>(), options))
            .Do(info => capturedDatasets.Add(info.ArgAt<IReadOnlyCollection<JobDefinition>>(0)));
        roundRobin.When(algorithm => algorithm.Schedule(Arg.Any<IReadOnlyCollection<JobDefinition>>(), options))
            .Do(info => capturedDatasets.Add(info.ArgAt<IReadOnlyCollection<JobDefinition>>(0)));
        var runner = new SimulationRunner(catalog);

        var comparison = runner.Run(new SimulationRequest(
            jobs,
            [SchedulingAlgorithmId.RoundRobin, SchedulingAlgorithmId.FirstComeFirstServed],
            options));

        Assert.Equal(
            [SchedulingAlgorithmId.RoundRobin, SchedulingAlgorithmId.FirstComeFirstServed],
            comparison.Results.Select(result => result.Algorithm.Id));
        Assert.Equal(2, capturedDatasets.Count);
        Assert.True(ReferenceEquals(capturedDatasets[0], capturedDatasets[1]));
        Assert.NotSame(jobs, comparison.Jobs);
    }

    [Fact]
    public void Run_WhenNoAlgorithmSelected_ShouldRejectRequest()
    {
        var runner = new SimulationRunner(Substitute.For<IAlgorithmCatalog>());
        var request = new SimulationRequest(
            [new JobDefinition("P1", 0, 1, 1, 0)],
            Array.Empty<SchedulingAlgorithmId>(),
            new SchedulingOptions());

        var action = () => runner.Run(request);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("at least one", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_WhenAlgorithmIsSelectedTwice_ShouldRejectDuplicateComparison()
    {
        var runner = new SimulationRunner(Substitute.For<IAlgorithmCatalog>());
        var request = new SimulationRequest(
            [new JobDefinition("P1", 0, 1, 1, 0)],
            [SchedulingAlgorithmId.RoundRobin, SchedulingAlgorithmId.RoundRobin],
            new SchedulingOptions());

        var action = () => runner.Run(request);

        Assert.Throws<ArgumentException>(action);
    }

    private static ICpuSchedulingAlgorithm CreateAlgorithm(SchedulingAlgorithmMetadata metadata, SchedulingOptions options)
    {
        var algorithm = Substitute.For<ICpuSchedulingAlgorithm>();
        algorithm.Metadata.Returns(metadata);
        algorithm.Schedule(Arg.Any<IReadOnlyCollection<JobDefinition>>(), options)
            .Returns(SchedulingResult.Empty(metadata, options));
        return algorithm;
    }
}
