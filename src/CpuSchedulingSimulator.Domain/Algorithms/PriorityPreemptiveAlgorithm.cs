using CpuSchedulingSimulator.Domain.Engine;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Algorithms;

/// <summary>
/// Preempts the running job when a strictly higher static-priority job becomes ready.
/// Use it to contrast responsive priority scheduling with its non-preemptive variant.
/// </summary>
public sealed class PriorityPreemptiveAlgorithm : SchedulingAlgorithmBase
{
    public override SchedulingAlgorithmMetadata Metadata => SchedulingMetadata.PriorityPreemptive;

    private protected override ISchedulingPolicy CreatePolicy(SchedulingOptions options) => new PriorityPolicy(true);
}
