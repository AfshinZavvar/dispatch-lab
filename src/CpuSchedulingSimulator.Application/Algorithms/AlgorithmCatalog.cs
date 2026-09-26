using CpuSchedulingSimulator.Domain.Algorithms;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Application.Algorithms;

/// <summary>
/// Owns the registered scheduling strategies and resolves them by stable algorithm identifier for application workflows.
/// </summary>
public sealed class AlgorithmCatalog : IAlgorithmCatalog
{
    private readonly Dictionary<SchedulingAlgorithmId, ICpuSchedulingAlgorithm> algorithms;

    public AlgorithmCatalog(IEnumerable<ICpuSchedulingAlgorithm> algorithms)
    {
        ArgumentNullException.ThrowIfNull(algorithms);
        var registered = algorithms.ToArray();
        var duplicate = registered
            .GroupBy(algorithm => algorithm.Metadata.Id)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new ArgumentException($"Algorithm {duplicate.Key} is registered more than once.", nameof(algorithms));
        }

        this.algorithms = registered.ToDictionary(algorithm => algorithm.Metadata.Id);
        All = Array.AsReadOnly(registered);
    }

    public IReadOnlyList<ICpuSchedulingAlgorithm> All { get; }

    public ICpuSchedulingAlgorithm Resolve(SchedulingAlgorithmId id) =>
        algorithms.TryGetValue(id, out var algorithm)
            ? algorithm
            : throw new KeyNotFoundException($"Scheduling algorithm {id} is not registered.");
}
