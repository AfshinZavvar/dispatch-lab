using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Algorithms;

/// <summary>
/// Represents a user-selectable CPU scheduling strategy that turns a validated workload into an immutable result.
/// Implement this interface to register another algorithm through the application catalog.
/// </summary>
public interface ICpuSchedulingAlgorithm
{
    SchedulingAlgorithmMetadata Metadata { get; }

    SchedulingResult Schedule(IReadOnlyCollection<JobDefinition> jobs, SchedulingOptions options);
}
