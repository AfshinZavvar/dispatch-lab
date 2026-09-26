using CpuSchedulingSimulator.Domain.Engine;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Algorithms;

/// <summary>
/// Schedules ready jobs non-preemptively by arrival time and ordinal job identifier.
/// Use it to demonstrate baseline FIFO scheduling behavior.
/// </summary>
public sealed class FirstComeFirstServedAlgorithm : SchedulingAlgorithmBase
{
    public override SchedulingAlgorithmMetadata Metadata => SchedulingMetadata.FirstComeFirstServed;

    private protected override ISchedulingPolicy CreatePolicy(SchedulingOptions options) => new FirstComeFirstServedPolicy();
}
