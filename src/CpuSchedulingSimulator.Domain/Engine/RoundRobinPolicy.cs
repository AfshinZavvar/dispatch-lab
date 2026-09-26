namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Maintains the FIFO ready queue and quantum counter required by Round Robin scheduling.
/// Use it through <see cref="CpuSchedulingSimulator.Domain.Algorithms.RoundRobinAlgorithm"/> rather than directly.
/// </summary>
internal sealed class RoundRobinPolicy(int quantum) : ISchedulingPolicy
{
    private readonly LinkedList<JobRuntime> ready = [];
    private int quantumUsed;

    public void Admit(JobRuntime job, int time) => ready.AddLast(job);

    public JobRuntime? DequeueNext(int time)
    {
        if (ready.First is null)
        {
            return null;
        }

        var job = ready.First.Value;
        ready.RemoveFirst();
        return job;
    }

    public IReadOnlyList<JobRuntime> GetReadyQueue(int time) => ready.ToArray();

    public string GetQueueLabel(JobRuntime job) => "the Round Robin queue";

    public string ExplainSelection(JobRuntime job, int time) => $"{job.Definition.Id} is at the head of the FIFO queue and may run for {quantum} ticks.";

    public bool ShouldPreempt(JobRuntime running, int time) => false;

    public string ExplainPreemption(JobRuntime running, int time) => string.Empty;

    public void RequeuePreempted(JobRuntime job, int time) => ready.AddLast(job);

    public void OnDispatched(JobRuntime job, int time) => quantumUsed = 0;

    public void OnTickExecuted(JobRuntime job) => quantumUsed++;

    public bool HasQuantumExpired(JobRuntime running) => quantumUsed >= quantum;

    public string ExplainQuantumExpiration(JobRuntime running, int time) => ready.Count == 0 ? $"{running.Definition.Id} used its {quantum}-tick quantum. No competitor is waiting, so it will resume immediately." : $"{running.Definition.Id} used its {quantum}-tick quantum and returns to the end of the queue.";

    public void RequeueAfterQuantum(JobRuntime job, int time) => ready.AddLast(job);

    public bool ShouldBoost(int time) => false;

    public void ApplyBoost(JobRuntime? running, int time)
    {
    }

    public string ExplainBoost(int time) => string.Empty;
}
