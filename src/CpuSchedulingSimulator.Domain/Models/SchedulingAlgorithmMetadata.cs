namespace CpuSchedulingSimulator.Domain.Models;

/// <summary>
/// Supplies the stable identity and educational copy used to present a scheduling algorithm.
/// </summary>
public sealed record SchedulingAlgorithmMetadata(SchedulingAlgorithmId Id, string ShortName, string Name, string Description, string SelectionRule, string TradeOff, bool IsPreemptive);
