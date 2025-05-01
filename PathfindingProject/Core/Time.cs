namespace PathfindingProject.Core;

/// <summary>
/// Provides frame-based timing information and time scaling functionality for the simulation.
/// Call Update() once per frame to maintain accurate timing.
/// </summary>
public static class Time
{
    /// <summary>
    /// Scaled time (in seconds) since the last frame.
    /// </summary>
    public static float DeltaTime { get; private set; }

    /// <summary>
    /// Unaffected by time scale. Currently returns the same as DeltaTime.
    /// </summary>
    public static float UnscaledDeltaTime => DeltaTime;

    /// <summary>
    /// Total scaled time elapsed since the simulation started (in seconds).
    /// </summary>
    public static float ElapsedTime => _elapsedTimeSinceStart;

    /// <summary>
    /// Total number of frames that have elapsed since start.
    /// </summary>
    public static int FrameCount => _frameCount;

    /// <summary>
    /// Multiplier applied to DeltaTime. Set to 0 to pause time.
    /// </summary>
    public static float TimeScale { get; set; } = 1f;

    private static DateTime _lastFrameTime = DateTime.Now;
    private static float _elapsedTimeSinceStart = 0f;
    private static int _frameCount = 0;

    /// <summary>
    /// Updates time measurements. Must be called once per frame.
    /// </summary>
    public static void Update()
    {
        var now = DateTime.Now;
        var rawDelta = (float)(now - _lastFrameTime).TotalSeconds;
        _lastFrameTime = now;

        DeltaTime = rawDelta * TimeScale;
        _elapsedTimeSinceStart += DeltaTime;
        _frameCount++;
    }
}