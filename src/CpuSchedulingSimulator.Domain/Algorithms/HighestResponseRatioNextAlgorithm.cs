using CpuSchedulingSimulator.Domain.Engine;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Algorithms;

/// <summary>
/// Selects the ready job with the highest response ratio without preemption.
/// Use it to demonstrate how waiting time can age longer jobs ahead of new short jobs.
/// </summary>
public sealed class HighestResponseRatioNextAlgorithm : SchedulingAlgorithmBase
{
    public override SchedulingAlgorithmMetadata Metadata => SchedulingMetadata.HighestResponseRatioNext;

    private protected override ISchedulingPolicy CreatePolicy(SchedulingOptions options) => new HighestResponseRatioNextPolicy();
}
