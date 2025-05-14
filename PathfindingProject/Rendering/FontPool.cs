namespace PathfindingProject.Rendering;

/// <summary>
/// Provides cached font instances for reuse to reduce GDI+ resource usage.
/// </summary>
public static class FontPool
{
    private static readonly Dictionary<FontKey, Font> s_fontCache = new();

    /// <summary>
    /// Gets a font for the given family, size, and style. Caches the font for future reuse.
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
    /// Disposes and clears all cached fonts.
    /// </summary>
    public static void Clear()
    {
        foreach (var font in s_fontCache.Values) font.Dispose();

        s_fontCache.Clear();
    }

    /// <summary>
    /// Uniquely identifies a font by family, size, and style for use as a dictionary key.
    /// </summary>
    private readonly record struct FontKey(string Family, float Size, FontStyle Style);
}