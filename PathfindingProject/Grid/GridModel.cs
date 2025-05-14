using PathfindingProject.Core;
using PathfindingProject.Pathfinding;

namespace PathfindingProject.Grid;

/// <summary>
/// Maintains the current grid configuration, overrides, and start/end points for pathfinding.
/// </summary>
public class GridModel
{
    /// <summary>
    /// Cells with user-defined modifications (obstacles, costs, etc.).
    /// </summary>
    public readonly Dictionary<Point, Node> GridOverrides = new();

    /// <summary>
    /// Number of cells in X and Y directions.
    /// </summary>
    public Point GridSize = new(30, 15);

    /// <summary>
    /// Path start and end points.
    /// </summary>
    public Point StartPoint;
    public Point EndPoint;

    /// <summary>
    /// Marks a cell as a wall, unless it's the start or end point.
    /// </summary>
    public void SetCellObstacle(Point point)
    {
        if (point == StartPoint || point == EndPoint) return;

        var node = new Node(point) { Walkable = false };
        GridOverrides.Remove(point);
        GridOverrides.TryAdd(point, node);
    }

    /// <summary>
    /// Sets a custom cost for a cell, unless it's the start or end point.
    /// </summary>
    public void SetCellCost(Point point, int cellCost)
    {
        if (point == StartPoint || point == EndPoint) return;

        var node = new Node(point) { CellCost = cellCost };
        GridOverrides.Remove(point);
        GridOverrides.TryAdd(point, node);
    }

    /// <summary>
    /// Returns the closest override cell to a given raw index,
    /// or snaps to the clamped cell inside grid bounds.
    /// </summary>
    public Point SnapToNearestValidCell(Point rawIndex)
    {
        // Clamp the raw index to grid boundaries
        var clampedX = Math.Clamp(rawIndex.X, 0, GridSize.X - 1);
        var clampedY = Math.Clamp(rawIndex.Y, 0, GridSize.Y - 1);
        var gridClamped = new Point(clampedX, clampedY);

        // Check for closest override
        Point? closestOverride = null;
        var bestOverrideDist = int.MaxValue;

        foreach (var point in GridOverrides.Keys)
        {
            var dist = Extensions.DistanceSquared(rawIndex, point);
            if (dist < bestOverrideDist)
            {
                bestOverrideDist = dist;
                closestOverride = point;
            }
        }

        var distToGrid = Extensions.DistanceSquared(rawIndex, gridClamped);

        return closestOverride.HasValue && bestOverrideDist < distToGrid
            ? closestOverride.Value
            : gridClamped;
    }
}