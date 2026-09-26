using CpuSchedulingSimulator.Domain.Engine;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Algorithms;

/// <summary>
/// Adapts job queue levels from CPU consumption and periodically boosts unfinished jobs.
/// Use it to demonstrate dynamic priority, demotion, and starvation prevention.
/// </summary>
public sealed class MultiLevelFeedbackQueueAlgorithm : SchedulingAlgorithmBase
{
    public override SchedulingAlgorithmMetadata Metadata => SchedulingMetadata.MultiLevelFeedbackQueue;

    private protected override ISchedulingPolicy CreatePolicy(SchedulingOptions options) => new MultiLevelFeedbackQueuePolicy(options);
}
