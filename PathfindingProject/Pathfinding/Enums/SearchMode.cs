namespace PathfindingProject.Pathfinding.Enums;

/// <summary>
/// Defines the set of available search strategies for pathfinding.
/// </summary>
public enum SearchMode
{
    // ─── Tree-based (uninformed) ──────────────────────────────

    /// <summary>
    /// Breadth-First Search: explores all neighbors evenly. Unweighted.
    /// </summary>
    BreadthFirstSearch,

    /// <summary>
    /// Depth-First Search: explores deep paths before breadth. Unweighted.
    /// </summary>
    DepthFirstSearch,

    // ─── Graph-based (cost-aware) ─────────────────────────────

    /// <summary>
    /// Uniform Cost Search: expands lowest-cost path without using a heuristic.
    /// </summary>
    UniformCostSearch,

    /// <summary>
    /// Greedy Best-First Search: uses heuristic only (no cost so far).
    /// </summary>
    GreedyBestFirstSearch,

    /// <summary>
    /// A* Search: combines cost-so-far and heuristic. Widely used and optimal.
    /// </summary>
    AStarSearch,

    // ─── Grid field-based ─────────────────────────────────────

    /// <summary>
    /// FlowField: propagates cost from target to all cells (used for many-agent movement).
    /// </summary>
    FlowField
}