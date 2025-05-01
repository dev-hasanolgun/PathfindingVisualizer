namespace PathfindingProject.Pathfinding.Frontiers;

/// <summary>
/// Priority-based frontier using a binary heap. Ideal for A* and Dijkstra search strategies.
/// </summary>
public class HeapFrontier : IFrontier
{
    private readonly PriorityQueue<Node, int> _heap = new();

    public void Add(Node node, int priority) => _heap.Enqueue(node, priority);
    public Node GetNext() => _heap.Dequeue();
    public void Reset() => _heap.Clear();
    public bool IsEmpty => _heap.Count == 0;
}