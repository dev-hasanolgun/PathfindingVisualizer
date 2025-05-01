namespace PathfindingProject.Rendering;

/// <summary>
/// Provides cached access to reusable Pen objects, reducing GDI allocations during drawing.
/// </summary>
public static class PenPool
{
    private static readonly Dictionary<(Color color, float width), Pen> s_penCache = new();

    /// <summary>
    /// Retrieves a cached Pen instance with the specified color and width, or creates one if not cached.
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
    /// Disposes all cached Pen instances and clears the pool.
    /// </summary>
    public static void Clear()
    {
        foreach (var pen in s_penCache.Values)
            pen.Dispose();

        s_penCache.Clear();
    }
}