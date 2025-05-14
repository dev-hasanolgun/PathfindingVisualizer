namespace PathfindingProject.Rendering;

/// <summary>
/// Provides cached brush instances for reuse to reduce GDI+ resource usage.
/// </summary>
public static class BrushPool
{
    private static readonly Dictionary<Color, Brush> s_brushCache = new();

    /// <summary>
    /// Gets a brush for the specified color. Caches the brush for future reuse.
    /// </summary>
    public static Brush Get(Color color)
    {
        if (s_brushCache.TryGetValue(color, out var brush)) return brush;

        brush = new SolidBrush(color);
        s_brushCache[color] = brush;
        return brush;
    }

    /// <summary>
    /// Disposes and clears all cached brushes.
    /// </summary>
    public static void Clear()
    {
        foreach (var brush in s_brushCache.Values) brush.Dispose();

        s_brushCache.Clear();
    }
}