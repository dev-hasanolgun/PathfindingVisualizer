using System.Drawing.Drawing2D;
using PathfindingProject.Pathfinding.Enums;
using PathfindingProject.Rendering;

namespace PathfindingProject.Core;

/// <summary>
/// A collection of helper and extension methods for grid-based math, color manipulation, and rendering.
/// </summary>
public static class Extensions
{
    // ────────────────────────────────
    // Grid Direction Definitions
    // ────────────────────────────────
    
    private static readonly Point[] s_fourWay =
    {
        new(0, -1),
        new(-1, 0), new(1, 0),
        new(0, 1)
    };

    private static readonly Point[] s_eightWay =
    {
        new(-1, -1), new(0, -1), new(1, -1),
        new(-1,  0),             new(1,  0),
        new(-1,  1), new(0,  1), new(1,  1)
    };

    // ────────────────────────────────
    // Grid/Pathfinding Helpers
    // ────────────────────────────────

    public static int GetStepCost(Point from, Point to, int straightCost = 10, int diagonalCost = 14)
    {
        var dx = Math.Abs(from.X - to.X);
        var dy = Math.Abs(from.Y - to.Y);
        return (dx == 1 && dy == 1) ? diagonalCost : straightCost;
    }

    public static int Evaluate(this HeuristicMode mode, Point a, Point b)
    {
        var dx = Math.Abs(a.X - b.X);
        var dy = Math.Abs(a.Y - b.Y);

        return mode switch
        {
            HeuristicMode.Manhattan => 10 * (dx + dy),
            HeuristicMode.Chebyshev => 10 * Math.Max(dx, dy),
            HeuristicMode.Octile    => 14 * Math.Min(dx, dy) + 10 * Math.Abs(dx - dy),
            HeuristicMode.Euclidean => (int)(10 * MathF.Sqrt(dx * dx + dy * dy)),
            HeuristicMode.Zero      => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public static int GridDistance(Point a, Point b, int straightCost = 10, int diagonalCost = 14)
    {
        var dx = Math.Abs(b.X - a.X);
        var dy = Math.Abs(b.Y - a.Y);
        return diagonalCost * Math.Min(dx, dy) + straightCost * Math.Abs(dx - dy);
    }

    public static int DistanceSquared(Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }

    public static float DistanceTo(this Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public static bool IsInBounds(this Point point, Point bounds) =>
        point.X >= 0 && point.X < bounds.X && point.Y >= 0 && point.Y < bounds.Y;

    public static Point Clamp(this Point p, Point min, Point max) =>
        new(Math.Clamp(p.X, min.X, max.X), Math.Clamp(p.Y, min.Y, max.Y));

    public static List<Point> GetNeighbors(Point point, Point bounds, NeighborMode neighborMode = NeighborMode.EightWay)
    {
        var directions = neighborMode == NeighborMode.FourWay ? s_fourWay : s_eightWay;
        return GetNeighbors(directions, point, bounds);
    }

    private static List<Point> GetNeighbors(Point[] directions, Point point, Point bounds)
    {
        var neighbors = new List<Point>();
        foreach (var dir in directions)
        {
            var neighbor = new Point(point.X + dir.X, point.Y + dir.Y);
            if (neighbor.IsInBounds(bounds))
                neighbors.Add(neighbor);
        }
        return neighbors;
    }

    public static IEnumerable<Point> GetNeighborsNonAlloc(Point point, Point bounds, NeighborMode neighborMode = NeighborMode.EightWay)
    {
        var directions = neighborMode == NeighborMode.FourWay ? s_fourWay : s_eightWay;
        foreach (var dir in directions)
        {
            var neighbor = new Point(point.X + dir.X, point.Y + dir.Y);
            if (neighbor.IsInBounds(bounds))
                yield return neighbor;
        }
    }

    public static int GetNeighborsNonAlloc(Point point, Point bounds, Span<Point> buffer, NeighborMode mode = NeighborMode.FourWay)
    {
        int count = 0;
        var directions = mode == NeighborMode.FourWay ? s_fourWay : s_eightWay;

        foreach (var dir in directions)
        {
            var neighbor = new Point(point.X + dir.X, point.Y + dir.Y);
            if (neighbor.IsInBounds(bounds))
                buffer[count++] = neighbor;
        }

        return count;
    }

    // ────────────────────────────────
    // Rendering Helpers
    // ────────────────────────────────

    public static void DrawArrowFromPosition(Graphics g, PointF position, Size size, PointF arrowEnd, Color arrowColor, float thickness = 2f)
    {
        using var pen = (Pen)PenPool.Get(arrowColor, thickness).Clone();
        pen.CustomEndCap = new AdjustableArrowCap(5, 5);

        var arrowStart = new PointF(position.X + size.Width / 2f, position.Y + size.Height / 2f);
        g.DrawLine(pen, arrowStart, arrowEnd);
    }

    // ────────────────────────────────
    // Grid ↔ Screen Coordinate Conversion
    // ────────────────────────────────

    public static Point GridToScreen(Point gridIndex, Point gridSize, Point cellSize, Size formSize)
    {
        var gridWidth = gridSize.X * cellSize.X;
        var gridHeight = gridSize.Y * cellSize.Y;

        var offsetX = (formSize.Width - gridWidth) / 2;
        var offsetY = (formSize.Height - gridHeight) / 2;

        var drawX = offsetX + gridIndex.X * cellSize.X;
        var drawY = offsetY + (gridSize.Y - 1 - gridIndex.Y) * cellSize.Y;

        return new Point(drawX, drawY);
    }

    public static Point ScreenToGrid(Point screenPos, Point gridSize, Point cellSize, Size formSize)
    {
        var gridWidth = gridSize.X * cellSize.X;
        var gridHeight = gridSize.Y * cellSize.Y;

        var offsetX = (formSize.Width - gridWidth) / 2;
        var offsetY = (formSize.Height - gridHeight) / 2;

        var localX = screenPos.X - offsetX;
        var localY = screenPos.Y - offsetY;

        var gridX = localX / cellSize.X;
        var gridY = gridSize.Y - 1 - (localY / cellSize.Y);

        return new Point(gridX, gridY);
    }

    // ────────────────────────────────
    // Math Operators (Point Extensions)
    // ────────────────────────────────

    public static Point Add(this Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
    public static Point Sub(this Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    public static Point Mul(this Point a, int scalar) => new(a.X * scalar, a.Y * scalar);
    public static Point Div(this Point a, int scalar) => new(a.X / scalar, a.Y / scalar);
    public static PointF Div(this Point a, float scalar) => new(a.X / scalar, a.Y / scalar);
    public static PointF ToPointF(this Point p) => new(p.X, p.Y);

    // ────────────────────────────────
    // Dictionary and Color Utilities
    // ────────────────────────────────

    public static void MergeWith<TKey, TValue>(this Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> other, bool overwrite = true)
    {
        foreach (var kvp in other)
        {
            if (overwrite || !source.ContainsKey(kvp.Key))
                source[kvp.Key] = kvp.Value;
        }
    }

    public static Color ScaleColor(this Color color, float factor, float minBrightness = 0f, bool darken = false)
    {
        factor = Math.Clamp(factor, 0f, 1f);
        minBrightness = Math.Clamp(minBrightness, 0f, 1f);

        var scale = darken ? 1f - (1f - minBrightness) * factor : minBrightness + (1f - minBrightness) * factor;

        var r = (int)(color.R * scale);
        var g = (int)(color.G * scale);
        var b = (int)(color.B * scale);

        return Color.FromArgb(color.A, r, g, b);
    }

    public static Color SetAlpha(this Color color, int alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);
}