using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Web.State;

/// <summary>
/// Provides mutable, UI-bindable job fields and converts them to the immutable domain model before simulation.
/// </summary>
public sealed class EditableJob
{
    public required string Id { get; init; }

    public int ArrivalTime { get; set; }

    public int BurstTime { get; set; }

    public int Priority { get; set; }

    public int QueueLevel { get; set; }

    public JobDefinition ToDomain() => new(Id, ArrivalTime, BurstTime, Priority, QueueLevel);

    public static EditableJob FromDomain(JobDefinition job) => new()
    {
        Id = job.Id,
        ArrivalTime = job.ArrivalTime,
        BurstTime = job.BurstTime,
        Priority = job.Priority,
        QueueLevel = job.QueueLevel,
    };
}
