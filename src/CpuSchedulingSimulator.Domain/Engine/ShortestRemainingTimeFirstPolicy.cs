namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Orders jobs by remaining work and requests preemption only for a strictly shorter ready job.
/// It is the engine policy used by <see cref="CpuSchedulingSimulator.Domain.Algorithms.ShortestRemainingTimeFirstAlgorithm"/>.
/// </summary>
internal sealed class ShortestRemainingTimeFirstPolicy : OrderedPolicyBase
{
    public override string ExplainSelection(JobRuntime job, int time) => $"{job.Definition.Id} has the least remaining work ({job.RemainingTime} ticks).";

    public override bool ShouldPreempt(JobRuntime running, int time)
    {
        var ready = GetReadyQueue(time);
        var candidate = ready.Count == 0 ? null : ready[0];
        return candidate is not null && candidate.RemainingTime < running.RemainingTime;
    }

    public override string ExplainPreemption(JobRuntime running, int time)
    {
        var candidate = GetReadyQueue(time)[0];
        return $"{running.Definition.Id} was preempted because {candidate.Definition.Id} has only {candidate.RemainingTime} ticks remaining, fewer than {running.RemainingTime}.";
    }

    protected override int Compare(JobRuntime left, JobRuntime right, int time)
    {
        var remaining = left.RemainingTime.CompareTo(right.RemainingTime);
        return remaining != 0 ? remaining : CompareGlobalTies(left, right);
    }
}
