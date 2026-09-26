using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Application.Algorithms;

/// <summary>
/// Exposes the available scheduling strategies without coupling callers to their registration mechanism.
/// </summary>
public interface IAlgorithmCatalog
{
    IReadOnlyList<ICpuSchedulingAlgorithm> All { get; }

    ICpuSchedulingAlgorithm Resolve(SchedulingAlgorithmId id);
}
