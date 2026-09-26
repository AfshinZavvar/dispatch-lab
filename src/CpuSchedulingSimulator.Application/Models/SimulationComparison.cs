using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Application.Models;

/// <summary>
/// Groups the common workload and algorithm results that are compared and replayed together.
/// </summary>
public sealed record SimulationComparison(IReadOnlyList<JobDefinition> Jobs, IReadOnlyList<SchedulingResult> Results)
{
    public SchedulingResult GetResult(SchedulingAlgorithmId id) =>
        Results.Single(result => result.Algorithm.Id == id);
}
