namespace PathfindingProject.Pathfinding.Frontiers;

/// <summary>
/// Represents a frontier used during pathfinding. Determines the order in which nodes are visited.
/// </summary>
public interface IFrontier
{
    /// <summary>
    /// Adds a node to the frontier with an optional priority.
    /// Priority may be ignored in non-priority-based implementations (e.g. stacks or queues).
    /// </summary>
    void Add(Node node, int priority = 0);

    /// <summary>
    /// Retrieves and removes the next node to process.
    /// </summary>
    Node GetNext();

    /// <summary>
    /// Clears the frontier, removing all nodes.
    /// </summary>
    void Reset();

    /// <summary>
    /// Returns true if the frontier is empty.
    /// </summary>
    bool IsEmpty { get; }
}