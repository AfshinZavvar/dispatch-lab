namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Orders jobs by static priority and optionally requests strict higher-priority preemption.
/// Construct it in preemptive or non-preemptive mode for the matching priority algorithm adapter.
/// </summary>
internal sealed class PriorityPolicy(bool isPreemptive) : OrderedPolicyBase
{
    public override string ExplainSelection(JobRuntime job, int time) => $"{job.Definition.Id} has the highest ready priority ({job.Definition.Priority}; lower numbers are higher).";

    public override bool ShouldPreempt(JobRuntime running, int time)
    {
        if (!isPreemptive)
        {
            return false;
        }

        var ready = GetReadyQueue(time);
        var candidate = ready.Count == 0 ? null : ready[0];
        return candidate is not null && candidate.Definition.Priority < running.Definition.Priority;
    }

    public override string ExplainPreemption(JobRuntime running, int time)
    {
        var candidate = GetReadyQueue(time)[0];
        return $"{running.Definition.Id} was preempted because {candidate.Definition.Id} has higher priority ({candidate.Definition.Priority} vs {running.Definition.Priority}).";
    }

    protected override int Compare(JobRuntime left, JobRuntime right, int time)
    {
        var priority = left.Definition.Priority.CompareTo(right.Definition.Priority);
        return priority != 0 ? priority : CompareGlobalTies(left, right);
    }
}
