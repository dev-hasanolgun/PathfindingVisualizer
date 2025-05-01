namespace PathfindingProject.Pathfinding.Frontiers;

/// <summary>
/// A LIFO (last-in-first-out) frontier for depth-first search (DFS).
/// </summary>
public class StackFrontier : IFrontier
{
    private readonly Stack<Node> _stack = new();

    /// <summary>
    /// Pushes a node onto the frontier. Priority is ignored.
    /// </summary>
    public void Add(Node node, int priority = 0) => _stack.Push(node);

    /// <summary>
    /// Pops and returns the next node from the frontier.
    /// </summary>
    public Node GetNext() => _stack.Pop();

    /// <summary>
    /// Clears all nodes from the frontier.
    /// </summary>
    public void Reset() => _stack.Clear();

    /// <summary>
    /// Returns true if the frontier is empty.
    /// </summary>
    public bool IsEmpty => _stack.Count == 0;
}