namespace PathfindingProject.Pathfinding.Frontiers;

/// <summary>
/// A FIFO (first-in-first-out) frontier for unweighted breadth-first search (BFS).
/// </summary>
public class QueueFrontier : IFrontier
{
    private readonly Queue<Node> _queue = new();

    public void Add(Node node, int priority = 0) => _queue.Enqueue(node);
    public Node GetNext() => _queue.Dequeue();
    public void Reset() => _queue.Clear();
    public bool IsEmpty => _queue.Count == 0;
}