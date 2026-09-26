namespace CpuSchedulingSimulator.Domain.Models;

/// <summary>
/// Captures aggregate performance measurements for one completed scheduling result.
/// </summary>
public sealed record AlgorithmMetrics(double AverageWaitingTime, double AverageTurnaroundTime, double AverageResponseTime, double CpuUtilizationPercent, double ThroughputPerTick, int ContextSwitches, int BusyTime, int Makespan);
