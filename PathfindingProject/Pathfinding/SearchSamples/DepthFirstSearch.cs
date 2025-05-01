namespace PathfindingProject.Pathfinding.SearchSamples;

/// <summary>
/// Depth-First Search explores deeply along each branch before backtracking.
/// Does not guarantee the shortest path, but uses less memory.
/// </summary>
public class DFS
{
    private readonly Dictionary<Point, Node> _nodeMap = new();    // Stores nodes and their states for efficient tracking.

    /// <summary>
    /// Executes the DFS algorithm to find the shortest path.
    /// </summary>
    /// <param name="start">Starting position on the grid.</param>
    /// <param name="goal">Target position on the grid.</param>
    /// <param name="gridSize">Dimensions of the grid.</param>
    /// <returns>Returns a stack containing the nodes forming the shortest path.</returns>
    public Stack<Node> FindPath(Point start, Point goal, Point gridSize)
    {
        var frontier = new Stack<Node>();   // Stack to manage the exploration order (LIFO structure for DFS).
        var startNode = new Node(start);    // Create start node.
        
        frontier.Push(startNode);       // Add start node to exploration queue.   
        _nodeMap[start] = startNode;    // Save start node to node map.

        // Continue exploration until no more nodes are left.
        while (frontier.Count > 0)
        {
            var currentNode = frontier.Pop();   // Pop the next node from the stack.

            // Check if goal is reached.
            if (currentNode.Point == goal)
            {
                return SampleUtils.ReconstructPath(currentNode, _nodeMap);  // Goal found, reconstruct and return the path.
            }
            
            currentNode.State = NodeState.Closed;           // Mark current node as explored/closed.
            _nodeMap[currentNode.Point] = currentNode;      // Update current node's state in the node map.

            // Iterate through each neighbor around current node.
            foreach (var neighbor in SampleUtils.GetNeighbors(currentNode.Point, gridSize, NeighborSearchType.FourWay))
            {
                // Try to retrieve or create neighbor node.
                if (!_nodeMap.TryGetValue(neighbor, out var neighborNode))
                {
                    neighborNode = new Node(neighbor);  // If neighbor not found in node map, create a new node at neighbor's position.
                }

                // Skip if neighbor is blocked or already visited.
                if (!neighborNode.Walkable || neighborNode.State != NodeState.Unvisited)
                {
                    continue;
                }

                neighborNode.ParentPoint = currentNode.Point;   // Set parent for path reconstruction.
                neighborNode.State = NodeState.Open;            // Mark neighbor node as open.
                _nodeMap[neighbor] = neighborNode;              // Save neighbor node to node map with updated information.
                frontier.Push(neighborNode);                    // Add neighbor node to the frontier queue to be explored next.
            }
        }
        
        return new Stack<Node>();   // No path found; return empty path.
    }
}