using CpuSchedulingSimulator.Domain.Engine;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Algorithms;

/// <summary>
/// Rotates ready jobs through a FIFO queue using the configured time quantum.
/// Use it to demonstrate time slicing, queue rotation, and context-switch costs.
/// </summary>
public sealed class RoundRobinAlgorithm : SchedulingAlgorithmBase
{
    public override SchedulingAlgorithmMetadata Metadata => SchedulingMetadata.RoundRobin;

    private protected override ISchedulingPolicy CreatePolicy(SchedulingOptions options) => new RoundRobinPolicy(options.RoundRobinQuantum);
}
