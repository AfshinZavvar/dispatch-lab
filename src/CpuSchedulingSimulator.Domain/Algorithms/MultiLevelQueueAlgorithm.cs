using CpuSchedulingSimulator.Domain.Engine;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Algorithms;

/// <summary>
/// Schedules jobs through three strict fixed-priority queues with queue-specific policies.
/// Use it to examine fixed workload classes and lower-queue starvation.
/// </summary>
public sealed class MultiLevelQueueAlgorithm : SchedulingAlgorithmBase
{
    public override SchedulingAlgorithmMetadata Metadata => SchedulingMetadata.MultiLevelQueue;

    private protected override ISchedulingPolicy CreatePolicy(SchedulingOptions options) => new MultiLevelQueuePolicy(options);
}
