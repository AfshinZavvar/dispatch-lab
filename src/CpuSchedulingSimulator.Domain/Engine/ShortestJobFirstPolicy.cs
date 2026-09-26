namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Orders ready jobs by original burst length and then global deterministic ties.
/// It is the non-preemptive engine policy used by <see cref="CpuSchedulingSimulator.Domain.Algorithms.ShortestJobFirstAlgorithm"/>.
/// </summary>
internal sealed class ShortestJobFirstPolicy : OrderedPolicyBase
{
    public override string ExplainSelection(JobRuntime job, int time) => $"{job.Definition.Id} has the shortest ready burst ({job.Definition.BurstTime} ticks). SJF will not preempt it.";

    protected override int Compare(JobRuntime left, JobRuntime right, int time)
    {
        var burst = left.Definition.BurstTime.CompareTo(right.Definition.BurstTime);
        return burst != 0 ? burst : CompareGlobalTies(left, right);
    }
}
