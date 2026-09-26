using CpuSchedulingSimulator.Application.Algorithms;
using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;
using NSubstitute;

namespace CpuSchedulingSimulator.Application.Tests;

/// <summary>
/// Verifies scheduling strategy registration, lookup, ordering, and duplicate protection.
/// </summary>
public sealed class AlgorithmCatalogTests
{
    [Fact]
    public void Constructor_WhenAlgorithmIdIsDuplicated_ShouldRejectAmbiguousRegistration()
    {
        var first = Substitute.For<ICpuSchedulingAlgorithm>();
        var second = Substitute.For<ICpuSchedulingAlgorithm>();
        first.Metadata.Returns(SchedulingMetadata.RoundRobin);
        second.Metadata.Returns(SchedulingMetadata.RoundRobin);

        var action = () => new AlgorithmCatalog([first, second]);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("more than once", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Get_WhenAlgorithmIsMissing_ShouldReturnSpecificFailure()
    {
        var catalog = new AlgorithmCatalog(Array.Empty<ICpuSchedulingAlgorithm>());

        var action = () => catalog.Resolve(SchedulingAlgorithmId.RoundRobin);

        var exception = Assert.Throws<KeyNotFoundException>(action);
        Assert.Contains(nameof(SchedulingAlgorithmId.RoundRobin), exception.Message, StringComparison.Ordinal);
    }
}
