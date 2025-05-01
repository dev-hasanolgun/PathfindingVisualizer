namespace PathfindingProject.Pathfinding.SearchSamples;

/// <summary>
/// Represents different heuristic functions for estimating cost to the goal.
/// </summary>
public enum HeuristicFunction
{
    /// <summary>Sum of horizontal and vertical distances. Best for 4-way movement.</summary>
    Manhattan,

    /// <summary>Max(dx, dy). Equivalent to diagonal movement where all steps are equal.</summary>
    Chebyshev,

    /// <summary>Diagonal movement with weighted corner cuts (approximate Euclidean).</summary>
    Octile,

    /// <summary>Straight-line distance (sqrt(dx² + dy²)).</summary>
    Euclidean
}