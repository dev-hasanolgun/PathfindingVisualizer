namespace PathfindingProject.Rendering;

/// <summary>
/// Caches Font instances to prevent redundant allocations and reduce GDI+ resource usage.
/// </summary>
public static class FontPool
{
    private static readonly Dictionary<FontKey, Font> s_fontCache = new();

    /// <summary>
    /// Returns a cached Font instance for the given family, size, and style.
    /// </summary>
    public static Font Get(string family, float size, FontStyle style = FontStyle.Regular)
    {
        var key = new FontKey(family, size, style);

        if (s_fontCache.TryGetValue(key, out var font))
            return font;

        font = new Font(family, size, style);
        s_fontCache[key] = font;
        return font;
    }

    /// <summary>
    /// Disposes and clears all cached fonts. Call on shutdown or UI reset.
    /// </summary>
    public static void Clear()
    {
        foreach (var font in s_fontCache.Values)
            font.Dispose();

        s_fontCache.Clear();
    }

    /// <summary>
    /// Uniquely identifies a font by family, size, and style for use as a dictionary key.
    /// </summary>
    private readonly record struct FontKey(string Family, float Size, FontStyle Style);
}