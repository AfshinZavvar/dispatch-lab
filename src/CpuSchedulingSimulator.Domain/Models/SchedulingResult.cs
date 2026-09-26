namespace CpuSchedulingSimulator.Domain.Models;

/// <summary>
/// Contains the complete immutable output of one algorithm run, including its timeline, events, and metrics.
/// </summary>
public sealed record SchedulingResult(SchedulingAlgorithmMetadata Algorithm, IReadOnlyList<JobDefinition> Jobs, SchedulingOptions Options, IReadOnlyList<ExecutionSlice> ExecutionSlices, IReadOnlyList<SimulationEvent> Events, IReadOnlyList<JobMetrics> JobMetrics, AlgorithmMetrics Metrics)
{
    public static SchedulingResult Empty(SchedulingAlgorithmMetadata algorithm, SchedulingOptions options) => new(
            algorithm,
            Array.Empty<JobDefinition>(),
            options,
            Array.Empty<ExecutionSlice>(),
            Array.Empty<SimulationEvent>(),
            Array.Empty<JobMetrics>(),
            new AlgorithmMetrics(0, 0, 0, 0, 0, 0, 0, 0));
}
