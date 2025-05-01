using PathfindingProject.Core;

namespace PathfindingProject.Pathfinding.SearchSamples;

/// <summary>
/// Utility class providing common helper methods for grid-based pathfinding algorithms.
/// Includes neighbor finding, step cost calculation, heuristic evaluations, and path reconstruction.
/// </summary>
public static class SampleUtils
{
    // Directions for four-way neighbor search (up, down, left, right).
    private static readonly Point[] s_fourWay =
    {
                new(0, -1),
        new(-1, 0), new(1, 0),
                new(0, 1)
    };

    // Directions for eight-way neighbor search, including diagonals.
    private static readonly Point[] s_eightWay =
    {
        new(-1, -1), new(0, -1), new(1, -1),
        new(-1,  0),                 new(1,  0),
        new(-1,  1), new(0,  1), new(1,  1)
    };

    /// <summary>
    /// Returns a list of valid neighboring points based on the specified neighbor search type (4-way or 8-way).
    /// </summary>
    /// <param name="point">The current point to find neighbors for.</param>
    /// <param name="bounds">The grid boundaries.</param>
    /// <param name="neighborMode">The type of neighbor search (FourWay or EightWay).</param>
    /// <returns>A list of neighboring points within grid boundaries.</returns>
    public static List<Point> GetNeighbors(Point point, Point bounds, NeighborSearchType neighborMode = NeighborSearchType.EightWay)
    {
        var directions = neighborMode == NeighborSearchType.FourWay ? s_fourWay : s_eightWay;
        var neighbors = new List<Point>();

        // Iterate through potential neighbor directions.
        foreach (var dir in directions)
        {
            var neighbor = new Point(point.X + dir.X, point.Y + dir.Y);

            // Add neighbor only if it lies within grid bounds.
            if (IsInBounds(neighbor, bounds))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Calculates the movement cost between two adjacent points.
    /// Returns diagonalCost for diagonal moves and straightCost otherwise.
    /// </summary>
    /// <param name="from">Starting point.</param>
    /// <param name="to">Destination point.</param>
    /// <param name="straightCost">Cost for horizontal/vertical movement (default 10).</param>
    /// <param name="diagonalCost">Cost for diagonal movement (default 14).</param>
    /// <returns>The step cost between the two points.</returns>
    public static int GetStepCost(Point from, Point to, int straightCost = 10, int diagonalCost = 14)
    {
        var dx = Math.Abs(from.X - to.X);
        var dy = Math.Abs(from.Y - to.Y);

        // Check if movement is diagonal (dx and dy both equal 1).
        return (dx == 1 && dy == 1) ? diagonalCost : straightCost;
    }

    /// <summary>
    /// Evaluates heuristic cost based on selected heuristic function between two points.
    /// </summary>
    /// <param name="mode">Heuristic function type.</param>
    /// <param name="a">Starting point.</param>
    /// <param name="b">Goal point.</param>
    /// <returns>Calculated heuristic cost between points a and b.</returns>
    public static int EvaluateHeuristic(HeuristicFunction mode, Point a, Point b)
    {
        var dx = Math.Abs(a.X - b.X);
        var dy = Math.Abs(a.Y - b.Y);

        // Calculate heuristic based on selected mode.
        return mode switch
        {
            HeuristicFunction.Manhattan => 10 * (dx + dy),                                  // Best for 4-directional grids.
            HeuristicFunction.Chebyshev => 10 * Math.Max(dx, dy),                           // Good for diagonal grids, fast calculation.
            HeuristicFunction.Octile    => 14 * Math.Min(dx, dy) + 10 * Math.Abs(dx - dy),  // Accurate for diagonal movement.
            HeuristicFunction.Euclidean => (int)(10 * MathF.Sqrt(dx * dx + dy * dy)),     // Precise straight-line distance.
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    /// <summary>
    /// Reconstructs the shortest path from the goal node back to the start node using parent pointers.
    /// </summary>
    /// <param name="node">The goal node from which to reconstruct the path.</param>
    /// <param name="nodeMap">Dictionary storing all explored nodes.</param>
    /// <returns>A stack of nodes representing the path from start to goal.</returns>
    public static Stack<Node> ReconstructPath(Node node, Dictionary<Point, Node> nodeMap)
    {
        var path = new Stack<Node>();

        // Trace path back to start node using parent pointers.
        while (node.ParentPoint != null)
        {
            path.Push(node);
            node = nodeMap[node.ParentPoint.Value];
        }

        return path;
    }

    /// <summary>
    /// Determines whether a given point is within the defined grid boundaries.
    /// </summary>
    /// <param name="point">Point to check.</param>
    /// <param name="bounds">Grid dimensions.</param>
    /// <returns>True if point lies within bounds; otherwise, false.</returns>
    public static bool IsInBounds(Point point, Point bounds)
    {
        return point.X >= 0 && point.X < bounds.X && point.Y >= 0 && point.Y < bounds.Y;
    }
}