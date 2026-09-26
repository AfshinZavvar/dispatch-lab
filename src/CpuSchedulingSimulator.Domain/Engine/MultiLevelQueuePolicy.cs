using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Maintains three fixed strict-priority queues with Round Robin in Q0/Q1 and FCFS in Q2.
/// Use it through <see cref="CpuSchedulingSimulator.Domain.Algorithms.MultiLevelQueueAlgorithm"/>.
/// </summary>
internal sealed class MultiLevelQueuePolicy(SchedulingOptions options) : ISchedulingPolicy
{
    private readonly LinkedList<JobRuntime>[] queues = [new LinkedList<JobRuntime>(), new LinkedList<JobRuntime>(), new LinkedList<JobRuntime>()];
    private int quantumUsed;

    public void Admit(JobRuntime job, int time) => queues[job.Definition.QueueLevel].AddLast(job);

    public JobRuntime? DequeueNext(int time)
    {
        foreach (var queue in queues)
        {
            if (queue.First is null)
            {
                continue;
            }

            var job = queue.First.Value;
            queue.RemoveFirst();
            return job;
        }

        return null;
    }

    public IReadOnlyList<JobRuntime> GetReadyQueue(int time) => queues.SelectMany(queue => queue).ToArray();

    public string GetQueueLabel(JobRuntime job) => $"fixed queue Q{job.Definition.QueueLevel}";

    public string ExplainSelection(JobRuntime job, int time)
    {
        var policy = job.Definition.QueueLevel switch
        {
            0 => $"Round Robin, quantum {options.MultiLevelQueueHighQuantum}",
            1 => $"Round Robin, quantum {options.MultiLevelQueueNormalQuantum}",
            _ => "FCFS",
        };
        return $"{job.Definition.Id} is first in the highest non-empty fixed queue Q{job.Definition.QueueLevel} ({policy}).";
    }

    public bool ShouldPreempt(JobRuntime running, int time) => queues.Take(running.Definition.QueueLevel).Any(queue => queue.Count > 0);

    public string ExplainPreemption(JobRuntime running, int time)
    {
        var higher = Array.FindIndex(queues, queue => queue.Count > 0);
        return $"{running.Definition.Id} in Q{running.Definition.QueueLevel} was preempted because higher fixed queue Q{higher} became ready.";
    }

    public void RequeuePreempted(JobRuntime job, int time) => queues[job.Definition.QueueLevel].AddFirst(job);

    public void OnDispatched(JobRuntime job, int time) => quantumUsed = 0;

    public void OnTickExecuted(JobRuntime job) => quantumUsed++;

    public bool HasQuantumExpired(JobRuntime running) => running.Definition.QueueLevel switch
    {
        0 => quantumUsed >= options.MultiLevelQueueHighQuantum,
        1 => quantumUsed >= options.MultiLevelQueueNormalQuantum,
        _ => false,
    };

    public string ExplainQuantumExpiration(JobRuntime running, int time) => $"{running.Definition.Id} used its Q{running.Definition.QueueLevel} quantum and rotates to that queue's tail.";

    public void RequeueAfterQuantum(JobRuntime job, int time) => queues[job.Definition.QueueLevel].AddLast(job);

    public bool ShouldBoost(int time) => false;

    public void ApplyBoost(JobRuntime? running, int time)
    {
    }

    public string ExplainBoost(int time) => string.Empty;
}
