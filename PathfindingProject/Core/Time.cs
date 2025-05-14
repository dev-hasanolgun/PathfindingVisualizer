namespace PathfindingProject.Core;

/// <summary>
/// Provides frame-based timing information and time scaling functionality.
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
    public static float ElapsedTime => s_elapsedTimeSinceStart;

    /// <summary>
    /// Total number of frames that have elapsed since start.
    /// </summary>
    public static int FrameCount => s_frameCount;

    /// <summary>
    /// Multiplier applied to DeltaTime. Set to 0 to pause time.
    /// </summary>
    public static float TimeScale { get; set; } = 1f;

    private static DateTime s_lastFrameTime = DateTime.Now;
    private static float s_elapsedTimeSinceStart;
    private static int s_frameCount;

    /// <summary>
    /// Updates time measurements. Must be called once per frame.
    /// </summary>
    public static void Update()
    {
        var now = DateTime.Now;
        var rawDelta = (float)(now - s_lastFrameTime).TotalSeconds;
        s_lastFrameTime = now;

        DeltaTime = rawDelta * TimeScale;
        s_elapsedTimeSinceStart += DeltaTime;
        s_frameCount++;
    }
}