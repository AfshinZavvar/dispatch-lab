using CpuSchedulingSimulator.Domain.Engine;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Algorithms;

/// <summary>
/// Selects the highest static-priority ready job and runs it to completion.
/// Use it when teaching non-preemptive priority ordering and starvation risk.
/// </summary>
public sealed class PriorityNonPreemptiveAlgorithm : SchedulingAlgorithmBase
{
    public override SchedulingAlgorithmMetadata Metadata => SchedulingMetadata.PriorityNonPreemptive;

    private protected override ISchedulingPolicy CreatePolicy(SchedulingOptions options) => new PriorityPolicy(false);
}
