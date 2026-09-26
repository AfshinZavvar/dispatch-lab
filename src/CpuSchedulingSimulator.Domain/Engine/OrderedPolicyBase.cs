namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Maintains a sortable ready set and supplies no-op defaults for non-queue-specific scheduling hooks.
/// Derive from it for policies whose next job can be selected by a deterministic comparison function.
/// </summary>
internal abstract class OrderedPolicyBase : ISchedulingPolicy
{
    private readonly List<JobRuntime> ready = [];

    protected IReadOnlyList<JobRuntime> Ready => ready;

    public void Admit(JobRuntime job, int time) => ready.Add(job);

    public JobRuntime? DequeueNext(int time)
    {
        var ordered = GetReadyQueue(time);
        var next = ordered.Count == 0 ? null : ordered[0];
        if (next is not null)
        {
            ready.Remove(next);
        }

        return next;
    }

    public IReadOnlyList<JobRuntime> GetReadyQueue(int time) => ready.Order(Comparer<JobRuntime>.Create((left, right) => Compare(left, right, time))).ToArray();

    public virtual string GetQueueLabel(JobRuntime job) => "the ready queue";

    public abstract string ExplainSelection(JobRuntime job, int time);

    public virtual bool ShouldPreempt(JobRuntime running, int time) => false;

    public virtual string ExplainPreemption(JobRuntime running, int time) => $"{running.Definition.Id} was preempted.";

    public virtual void RequeuePreempted(JobRuntime job, int time) => ready.Add(job);

    public virtual void OnDispatched(JobRuntime job, int time)
    {
    }

    public virtual void OnTickExecuted(JobRuntime job)
    {
    }

    public virtual bool HasQuantumExpired(JobRuntime running) => false;

    public virtual string ExplainQuantumExpiration(JobRuntime running, int time) => $"{running.Definition.Id} reached its time quantum.";

    public virtual void RequeueAfterQuantum(JobRuntime job, int time) => ready.Add(job);

    public virtual bool ShouldBoost(int time) => false;

    public virtual void ApplyBoost(JobRuntime? running, int time)
    {
    }

    public virtual string ExplainBoost(int time) => $"All unfinished jobs were boosted at time {time}.";

    protected static int CompareGlobalTies(JobRuntime left, JobRuntime right)
    {
        var arrival = left.Definition.ArrivalTime.CompareTo(right.Definition.ArrivalTime);
        return arrival != 0 ? arrival : StringComparer.Ordinal.Compare(left.Definition.Id, right.Definition.Id);
    }

    protected abstract int Compare(JobRuntime left, JobRuntime right, int time);
}
