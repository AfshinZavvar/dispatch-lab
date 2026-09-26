using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Domain.Validation;

/// <summary>
/// Validates workloads and scheduler options at the domain boundary before any simulation begins.
/// </summary>
public static class SchedulingValidator
{
    public const int MaximumJobs = 100;
    public const int MaximumArrivalTime = 10_000;
    public const int MaximumBurstTime = 1_000;
    public const int MaximumTotalBurstTime = 100_000;
    public const int MaximumQuantum = 1_000;
    public const int MaximumBoostInterval = 100_000;

    public static IReadOnlyList<string> GetErrors(IReadOnlyCollection<JobDefinition>? jobs, SchedulingOptions? options)
    {
        var errors = new List<string>();

        if (jobs is null)
        {
            errors.Add("Jobs are required.");
            return errors.AsReadOnly();
        }

        if (options is null)
        {
            errors.Add("Scheduling options are required.");
        }
        else
        {
            ValidateOptions(options, errors);
        }

        if (jobs.Count > MaximumJobs)
        {
            errors.Add($"At most {MaximumJobs} jobs can be simulated.");
        }

        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        long totalBurst = 0;
        var index = 0;

        foreach (var job in jobs)
        {
            var label = string.IsNullOrWhiteSpace(job.Id) ? $"Job at position {index + 1}" : $"Job {job.Id}";
            if (string.IsNullOrWhiteSpace(job.Id))
            {
                errors.Add($"{label} must have a non-empty ID.");
            }
            else
            {
                if (job.Id.Length > 20)
                {
                    errors.Add($"{label} ID cannot exceed 20 characters.");
                }

                if (!seenIds.Add(job.Id))
                {
                    errors.Add($"Job ID '{job.Id}' must be unique.");
                }
            }

            if (job.ArrivalTime is < 0 or > MaximumArrivalTime)
            {
                errors.Add($"{label} arrival time must be from 0 through {MaximumArrivalTime}.");
            }

            if (job.BurstTime is < 1 or > MaximumBurstTime)
            {
                errors.Add($"{label} burst time must be from 1 through {MaximumBurstTime}.");
            }

            if (job.Priority is < 1 or > 99)
            {
                errors.Add($"{label} priority must be from 1 (highest) through 99 (lowest).");
            }

            if (job.QueueLevel is < 0 or > 2)
            {
                errors.Add($"{label} queue level must be 0, 1, or 2.");
            }

            totalBurst += Math.Max(0, job.BurstTime);
            index++;
        }

        if (totalBurst > MaximumTotalBurstTime)
        {
            errors.Add($"Total burst time cannot exceed {MaximumTotalBurstTime} ticks.");
        }

        return errors.AsReadOnly();
    }

    public static void ValidateAndThrow(IReadOnlyCollection<JobDefinition>? jobs, SchedulingOptions? options)
    {
        var errors = GetErrors(jobs, options);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(Environment.NewLine, errors));
        }
    }

    private static void ValidateOptions(SchedulingOptions options, List<string> errors)
    {
        ValidateQuantum(options.RoundRobinQuantum, "Round Robin quantum", errors);
        ValidateQuantum(options.MultiLevelQueueHighQuantum, "MLQ high-queue quantum", errors);
        ValidateQuantum(options.MultiLevelQueueNormalQuantum, "MLQ normal-queue quantum", errors);
        ValidateQuantum(options.MlfqHighQuantum, "MLFQ high-queue quantum", errors);
        ValidateQuantum(options.MlfqNormalQuantum, "MLFQ normal-queue quantum", errors);
        ValidateQuantum(options.MlfqLowQuantum, "MLFQ low-queue quantum", errors);

        if (options.MlfqBoostInterval is < 1 or > MaximumBoostInterval)
        {
            errors.Add($"MLFQ boost interval must be from 1 through {MaximumBoostInterval}.");
        }
    }

    private static void ValidateQuantum(int quantum, string name, List<string> errors)
    {
        if (quantum is < 1 or > MaximumQuantum)
        {
            errors.Add($"{name} must be from 1 through {MaximumQuantum}.");
        }
    }
}
