using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Maintains dynamic MLFQ levels, retained quanta, demotion, higher-level preemption, and periodic boosts.
/// Use it through <see cref="CpuSchedulingSimulator.Domain.Algorithms.MultiLevelFeedbackQueueAlgorithm"/>.
/// </summary>
internal sealed class MultiLevelFeedbackQueuePolicy(SchedulingOptions options) : ISchedulingPolicy
{
    private readonly LinkedList<JobRuntime>[] queues = [new LinkedList<JobRuntime>(), new LinkedList<JobRuntime>(), new LinkedList<JobRuntime>()];

    public void Admit(JobRuntime job, int time)
    {
        job.DynamicQueueLevel = 0;
        job.QuantumUsed = 0;
        queues[0].AddLast(job);
    }

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

    public string GetQueueLabel(JobRuntime job) => $"feedback queue Q{job.DynamicQueueLevel}";

    public string ExplainSelection(JobRuntime job, int time) => $"{job.Definition.Id} is first in the highest non-empty feedback queue Q{job.DynamicQueueLevel}; its quantum is {options.GetMlfqQuantum(job.DynamicQueueLevel)} ticks.";

    public bool ShouldPreempt(JobRuntime running, int time) => queues.Take(running.DynamicQueueLevel).Any(queue => queue.Count > 0);

    public string ExplainPreemption(JobRuntime running, int time)
    {
        var higher = Array.FindIndex(queues, queue => queue.Count > 0);
        return $"{running.Definition.Id} in Q{running.DynamicQueueLevel} was preempted because higher feedback queue Q{higher} became ready; its unused quantum is retained.";
    }

    public void RequeuePreempted(JobRuntime job, int time) => queues[job.DynamicQueueLevel].AddFirst(job);

    public void OnDispatched(JobRuntime job, int time)
    {
    }

    public void OnTickExecuted(JobRuntime job) => job.QuantumUsed++;

    public bool HasQuantumExpired(JobRuntime running) => running.QuantumUsed >= options.GetMlfqQuantum(running.DynamicQueueLevel);

    public string ExplainQuantumExpiration(JobRuntime running, int time)
    {
        var nextLevel = Math.Min(2, running.DynamicQueueLevel + 1);
        return nextLevel == running.DynamicQueueLevel ? $"{running.Definition.Id} used its Q2 quantum and rotates within the lowest queue." : $"{running.Definition.Id} used its full Q{running.DynamicQueueLevel} quantum and is demoted to Q{nextLevel}.";
    }

    public void RequeueAfterQuantum(JobRuntime job, int time)
    {
        job.DynamicQueueLevel = Math.Min(2, job.DynamicQueueLevel + 1);
        job.QuantumUsed = 0;
        queues[job.DynamicQueueLevel].AddLast(job);
    }

    public bool ShouldBoost(int time) => time > 0 && time % options.MlfqBoostInterval == 0;

    public void ApplyBoost(JobRuntime? running, int time)
    {
        var boostOrder = new List<JobRuntime>();
        if (running is not null)
        {
            boostOrder.Add(running);
        }

        boostOrder.AddRange(queues.SelectMany(queue => queue));
        foreach (var queue in queues)
        {
            queue.Clear();
        }

        foreach (var job in boostOrder)
        {
            job.DynamicQueueLevel = 0;
            job.QuantumUsed = 0;
            queues[0].AddLast(job);
        }
    }

    public string ExplainBoost(int time) => $"The {options.MlfqBoostInterval}-tick priority boost moved every unfinished job to Q0 to prevent starvation.";
}
