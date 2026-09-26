namespace CpuSchedulingSimulator.Application.Playback;

/// <summary>
/// Describes the user-controlled lifecycle of single-result and comparison playback sessions.
/// </summary>
public enum PlaybackStatus
{
    Ready,
    Playing,
    Paused,
    Completed,
}
