namespace CpuSchedulingSimulator.Domain.Engine;

/// <summary>
/// Defines the queueing and preemption hooks consumed by the shared tick-based scheduling engine.
/// Implement this interface when an algorithm needs policy-specific dispatch behavior without duplicating the engine.
/// </summary>
internal interface ISchedulingPolicy
{
    void Admit(JobRuntime job, int time);

    JobRuntime? DequeueNext(int time);

    IReadOnlyList<JobRuntime> GetReadyQueue(int time);

    string GetQueueLabel(JobRuntime job);

    string ExplainSelection(JobRuntime job, int time);

    bool ShouldPreempt(JobRuntime running, int time);

    string ExplainPreemption(JobRuntime running, int time);

    void RequeuePreempted(JobRuntime job, int time);

    void OnDispatched(JobRuntime job, int time);

    void OnTickExecuted(JobRuntime job);

    bool HasQuantumExpired(JobRuntime running);

    string ExplainQuantumExpiration(JobRuntime running, int time);

    void RequeueAfterQuantum(JobRuntime job, int time);

    bool ShouldBoost(int time);

    void ApplyBoost(JobRuntime? running, int time);

    string ExplainBoost(int time);
}
