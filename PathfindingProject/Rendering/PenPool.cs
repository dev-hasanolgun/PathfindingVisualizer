namespace PathfindingProject.Rendering;

/// <summary>
/// Provides cached pen instances for reuse to reduce GDI+ resource usage.
/// </summary>
public static class PenPool
{
    private static readonly Dictionary<(Color color, float width), Pen> s_penCache = new();

    /// <summary>
    /// Gets a pen for the given specified color and width. Caches the pen for future reuse.
    /// </summary>
    public static Pen Get(Color color, float width = 1f)
    {
        var key = (color, width);

        if (s_penCache.TryGetValue(key, out var pen))
            return pen;

        pen = new Pen(color, width);
        s_penCache[key] = pen;
        return pen;
    }

    /// <summary>
    /// Disposes and clears all cached pens.
    /// </summary>
    public static void Clear()
    {
        foreach (var pen in s_penCache.Values) pen.Dispose();

        s_penCache.Clear();
    }
}