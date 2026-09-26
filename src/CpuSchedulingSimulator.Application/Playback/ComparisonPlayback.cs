using CpuSchedulingSimulator.Application.Models;
using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Application.Playback;

/// <summary>
/// Advances multiple completed schedules on one absolute clock so their decisions can be observed side by side.
/// </summary>
public sealed class ComparisonPlayback
{
    private static readonly double[] SupportedSpeeds = [0.5, 1, 2, 4];
    private readonly IReadOnlyList<int> decisionTimes;

    public ComparisonPlayback(SimulationComparison comparison)
    {
        ArgumentNullException.ThrowIfNull(comparison);
        if (comparison.Results.Count < 2)
        {
            throw new ArgumentException("Comparison playback requires at least two scheduling results.", nameof(comparison));
        }

        Comparison = comparison;
        MaxTime = comparison.Results.Max(result => result.Metrics.Makespan);
        decisionTimes = comparison.Results
            .SelectMany(result => result.Events)
            .Select(simulationEvent => simulationEvent.Time)
            .Distinct()
            .Order()
            .ToArray();
    }

    public SimulationComparison Comparison { get; }

    public int CurrentTime { get; private set; } = -1;

    public int DisplayTime => Math.Max(0, CurrentTime);

    public int MaxTime { get; }

    public PlaybackStatus Status { get; private set; } = PlaybackStatus.Ready;

    public double Speed { get; private set; } = 1;

    public int TimerDelayMilliseconds => (int)(900 / Speed);

    public double ProgressPercent => MaxTime == 0 || CurrentTime < 0
        ? 0
        : CurrentTime * 100.0 / MaxTime;

    public bool CanStepForward => decisionTimes.Any(time => time > CurrentTime);

    public void Start()
    {
        if (Status == PlaybackStatus.Completed)
        {
            Restart();
        }

        Status = MaxTime == 0 ? PlaybackStatus.Completed : PlaybackStatus.Playing;
    }

    public void Pause()
    {
        if (Status == PlaybackStatus.Playing)
        {
            Status = PlaybackStatus.Paused;
        }
    }

    public void Resume()
    {
        if (Status is PlaybackStatus.Paused or PlaybackStatus.Ready)
        {
            Start();
        }
    }

    public void StepToNextDecision()
    {
        var next = decisionTimes.FirstOrDefault(time => time > CurrentTime, -1);
        if (next < 0)
        {
            CurrentTime = MaxTime;
            Status = PlaybackStatus.Completed;
            return;
        }

        CurrentTime = next;
        Status = CurrentTime >= MaxTime ? PlaybackStatus.Completed : PlaybackStatus.Paused;
    }

    public void AdvanceTimer()
    {
        if (Status != PlaybackStatus.Playing)
        {
            return;
        }

        CurrentTime = Math.Min(MaxTime, CurrentTime + 1);
        if (CurrentTime >= MaxTime)
        {
            Status = PlaybackStatus.Completed;
        }
    }

    public void Restart()
    {
        CurrentTime = -1;
        Status = PlaybackStatus.Ready;
    }

    public void SetSpeed(double speed)
    {
        if (!SupportedSpeeds.Contains(speed))
        {
            throw new ArgumentOutOfRangeException(nameof(speed), speed, "Speed must be 0.5, 1, 2, or 4.");
        }

        Speed = speed;
    }

    public SimulationEvent? GetCurrentEvent(SchedulingResult result) => CurrentTime < 0
        ? null
        : result.Events.LastOrDefault(simulationEvent => simulationEvent.Time <= CurrentTime);

    public SchedulerSnapshot GetSnapshot(SchedulingResult result) =>
        GetCurrentEvent(result)?.Snapshot ?? SchedulerSnapshot.Empty;

    public string? GetRunningJobId(SchedulingResult result)
    {
        if (CurrentTime < 0)
        {
            return null;
        }

        return result.ExecutionSlices.FirstOrDefault(slice =>
            !slice.IsIdle &&
            slice.StartTime <= CurrentTime &&
            CurrentTime < slice.EndTime)?.JobId;
    }

    public int GetRemainingTime(SchedulingResult result, string jobId)
    {
        var job = result.Jobs.Single(item => StringComparer.Ordinal.Equals(item.Id, jobId));
        if (CurrentTime <= job.ArrivalTime)
        {
            return job.BurstTime;
        }

        var executed = result.ExecutionSlices
            .Where(slice => StringComparer.Ordinal.Equals(slice.JobId, jobId))
            .Sum(slice => Math.Max(0, Math.Min(CurrentTime, slice.EndTime) - slice.StartTime));
        return Math.Max(0, job.BurstTime - executed);
    }
}
