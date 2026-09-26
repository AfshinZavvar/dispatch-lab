using CpuSchedulingSimulator.Domain.Models;

namespace CpuSchedulingSimulator.Application.Playback;

/// <summary>
/// Navigates the immutable event stream for one completed schedule without recalculating the algorithm.
/// </summary>
public sealed class SimulationPlayback
{
    private static readonly double[] SupportedSpeeds = [0.5, 1, 2, 4];

    public SimulationPlayback(SchedulingResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        Result = result;
    }

    public SchedulingResult Result { get; }

    public int CurrentEventIndex { get; private set; } = -1;

    public SimulationEvent? CurrentEvent => CurrentEventIndex < 0 ? null : Result.Events[CurrentEventIndex];

    public SchedulerSnapshot CurrentSnapshot => CurrentEvent?.Snapshot ?? SchedulerSnapshot.Empty;

    public PlaybackStatus Status { get; private set; } = PlaybackStatus.Ready;

    public double Speed { get; private set; } = 1;

    public int TimerDelayMilliseconds => (int)(900 / Speed);

    public double ProgressPercent => Result.Events.Count == 0
        ? 0
        : (CurrentEventIndex + 1) * 100.0 / Result.Events.Count;

    public bool CanStepForward => CurrentEventIndex + 1 < Result.Events.Count;

    public void Start()
    {
        if (Status == PlaybackStatus.Completed)
        {
            Restart();
        }

        Status = Result.Events.Count == 0 ? PlaybackStatus.Completed : PlaybackStatus.Playing;
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

    public void StepForward()
    {
        if (!CanStepForward)
        {
            Status = PlaybackStatus.Completed;
            return;
        }

        CurrentEventIndex++;
        Status = CanStepForward ? PlaybackStatus.Paused : PlaybackStatus.Completed;
    }

    public void AdvanceTimer()
    {
        if (Status != PlaybackStatus.Playing)
        {
            return;
        }

        if (!CanStepForward)
        {
            Status = PlaybackStatus.Completed;
            return;
        }

        CurrentEventIndex++;
        if (!CanStepForward)
        {
            Status = PlaybackStatus.Completed;
        }
    }

    public void Restart()
    {
        CurrentEventIndex = -1;
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
}
