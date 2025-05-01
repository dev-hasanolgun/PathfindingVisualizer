namespace PathfindingProject.Rendering;

/// <summary>
/// Provides cached SolidBrush instances for reuse to reduce GDI+ resource usage.
/// </summary>
public static class BrushPool
{
    // Caches SolidBrush objects based on color for reuse.
    private static readonly Dictionary<Color, Brush> s_brushCache = new();

    /// <summary>
    /// Gets a SolidBrush for the specified color. Caches the brush for future reuse.
    /// </summary>
    public static Brush Get(Color color)
    {
        if (s_brushCache.TryGetValue(color, out var brush))
            return brush;

        brush = new SolidBrush(color);
        s_brushCache[color] = brush;
        return brush;
    }

    /// <summary>
    /// Disposes all cached brushes and clears the cache.
    /// Call this when brushes are no longer needed (e.g., on shutdown).
    /// </summary>
    public static void Clear()
    {
        foreach (var brush in s_brushCache.Values)
            brush.Dispose();

        s_brushCache.Clear();
    }
}