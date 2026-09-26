using CpuSchedulingSimulator.Application.Algorithms;
using CpuSchedulingSimulator.Application.Models;
using CpuSchedulingSimulator.Domain.Models;
using CpuSchedulingSimulator.Domain.Validation;

namespace CpuSchedulingSimulator.Application.Simulation;

/// <summary>
/// Validates one request and executes every selected algorithm against the same immutable workload and options.
/// </summary>
public sealed class SimulationRunner(IAlgorithmCatalog catalog)
{
    public SimulationComparison Run(SimulationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Jobs);
        ArgumentNullException.ThrowIfNull(request.Algorithms);
        ArgumentNullException.ThrowIfNull(request.Options);

        SchedulingValidator.ValidateAndThrow(request.Jobs, request.Options);
        var selectedAlgorithms = request.Algorithms.ToArray();
        if (selectedAlgorithms.Length == 0)
        {
            throw new ArgumentException("Select at least one scheduling algorithm.", nameof(request));
        }

        if (selectedAlgorithms.Distinct().Count() != selectedAlgorithms.Length)
        {
            throw new ArgumentException("Each scheduling algorithm can be selected only once.", nameof(request));
        }

        var jobs = Array.AsReadOnly(request.Jobs.ToArray());
        var results = selectedAlgorithms
            .Select(id => catalog.Resolve(id).Schedule(jobs, request.Options))
            .ToArray();
        return new SimulationComparison(jobs, Array.AsReadOnly(results));
    }
}
