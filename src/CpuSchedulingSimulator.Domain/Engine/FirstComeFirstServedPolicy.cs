namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Orders ready jobs by arrival time and ordinal identifier without preemption.
/// It is the engine policy used by <see cref="CpuSchedulingSimulator.Domain.Algorithms.FirstComeFirstServedAlgorithm"/>.
/// </summary>
internal sealed class FirstComeFirstServedPolicy : OrderedPolicyBase
{
    public override string ExplainSelection(JobRuntime job, int time) => $"{job.Definition.Id} arrived first among the ready jobs.";

    protected override int Compare(JobRuntime left, JobRuntime right, int time) => CompareGlobalTies(left, right);
}
