using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Application.Models;

/// <summary>
/// Carries a workload, selected algorithms, and shared options into a multi-algorithm simulation run.
/// </summary>
public sealed record SimulationRequest(IReadOnlyCollection<JobDefinition> Jobs, IReadOnlyCollection<SchedulingAlgorithmId> Algorithms, SchedulingOptions Options);
