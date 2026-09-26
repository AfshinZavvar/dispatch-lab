using CpuSchedulingSimulator.Domain.Engine;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Algorithms;

/// <summary>
/// Provides the shared validation and tick-engine adapter used by concrete CPU scheduling algorithms.
/// Derive from this type when an algorithm can be expressed through an <see cref="ISchedulingPolicy"/>.
/// </summary>
public abstract class SchedulingAlgorithmBase : ICpuSchedulingAlgorithm
{
    public abstract SchedulingAlgorithmMetadata Metadata { get; }

    public SchedulingResult Schedule(IReadOnlyCollection<JobDefinition> jobs, SchedulingOptions options)
    {
        ArgumentNullException.ThrowIfNull(jobs);
        ArgumentNullException.ThrowIfNull(options);
        return TickSchedulingEngine.Run(Metadata, jobs, options, CreatePolicy(options));
    }

    private protected abstract ISchedulingPolicy CreatePolicy(SchedulingOptions options);
}
