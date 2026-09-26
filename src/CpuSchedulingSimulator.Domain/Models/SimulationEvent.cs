namespace CpuSchedulingSimulator.Domain.Models;

/// <summary>
/// Represents one ordered, human-readable scheduling decision and the state observed immediately afterward.
/// </summary>
public sealed record SimulationEvent(int Sequence, int Time, SimulationEventKind Kind, string? JobId, string Explanation, SchedulerSnapshot Snapshot);
