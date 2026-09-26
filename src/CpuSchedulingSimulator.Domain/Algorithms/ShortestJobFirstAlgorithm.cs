using CpuSchedulingSimulator.Domain.Engine;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Algorithms;

/// <summary>
/// Schedules the ready job with the smallest original burst and runs it to completion.
/// Use it to compare non-preemptive shortest-work selection with FCFS and SRTF.
/// </summary>
public sealed class ShortestJobFirstAlgorithm : SchedulingAlgorithmBase
{
    public override SchedulingAlgorithmMetadata Metadata => SchedulingMetadata.ShortestJobFirst;

    private protected override ISchedulingPolicy CreatePolicy(SchedulingOptions options) => new ShortestJobFirstPolicy();
}
