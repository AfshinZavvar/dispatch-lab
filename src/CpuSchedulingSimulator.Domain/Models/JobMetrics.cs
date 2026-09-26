namespace CpuSchedulingSimulator.Domain.Models;

/// <summary>
/// Captures lifecycle times and derived performance measurements for one completed job.
/// </summary>
public sealed record JobMetrics(string JobId, int ArrivalTime, int BurstTime, int FirstStartTime, int CompletionTime, int CpuExecutionTime, int TurnaroundTime, int WaitingTime, int ResponseTime);
