namespace CpuSchedulingSimulator.Domain.Models;

/// <summary>
/// Defines the immutable workload input for one CPU-bound job.
/// </summary>
public sealed record JobDefinition(string Id, int ArrivalTime, int BurstTime, int Priority, int QueueLevel);
