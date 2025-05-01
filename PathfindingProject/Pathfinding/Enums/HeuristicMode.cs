namespace PathfindingProject.Pathfinding.Enums;

/// <summary>
/// Represents different heuristic strategies for estimating cost to the goal.
/// </summary>
public enum HeuristicMode
{
    /// <summary>Disables heuristic cost (e.g. for BFS or greedy test modes).</summary>
    None,

    /// <summary>Sum of horizontal and vertical distances. Best for 4-way movement.</summary>
    Manhattan,

    /// <summary>Max(dx, dy). Equivalent to diagonal movement where all steps are equal.</summary>
    Chebyshev,

    /// <summary>Diagonal movement with weighted corner cuts (approximate Euclidean).</summary>
    Octile,

    /// <summary>Straight-line distance (sqrt(dx² + dy²)).</summary>
    Euclidean,

    /// <summary>Always returns 0 as heuristic. Forces uniform-cost search.</summary>
    Zero
}