using CpuSchedulingSimulator.Domain.Engine;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Algorithms;

/// <summary>
/// Preemptively schedules the ready job with strictly least remaining CPU work.
/// Use it to observe arrival-driven preemption and response-time trade-offs.
/// </summary>
public sealed class ShortestRemainingTimeFirstAlgorithm : SchedulingAlgorithmBase
{
    public override SchedulingAlgorithmMetadata Metadata => SchedulingMetadata.ShortestRemainingTimeFirst;

    private protected override ISchedulingPolicy CreatePolicy(SchedulingOptions options) => new ShortestRemainingTimeFirstPolicy();
}
