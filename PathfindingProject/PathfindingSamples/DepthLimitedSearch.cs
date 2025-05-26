namespace PathfindingProject.PathfindingSamples;

/// <summary>
/// Depth-Limited Search explores deeply along each branch, but only up to a defined maximum depth.
/// It prevents infinite traversal in deep or cyclic graphs, but does not guarantee the shortest path.
/// </summary>
public class DLS
{
    private readonly Dictionary<Point, Node> _nodeMap = new();  // Stores nodes and their states for efficient tracking.

    /// <summary>
    /// Executes the Depth-Limited Search algorithm to find a path from start to goal within a given depth.
    /// </summary>
    /// <param name="start">Starting position on the grid.</param>
    /// <param name="goal">Target position on the grid.</param>
    /// <param name="gridSize">Dimensions of the grid.</param>
    /// <param name="depthLimit">Maximum allowed depth for exploration.</param>
    /// <returns>Returns a stack containing the nodes forming the path if found within the depth limit; otherwise, an empty path.</returns>
    public Stack<Node> FindPath(Point start, Point goal, Point gridSize, int depthLimit)
    {
        var frontier = new Stack<Node>();           // Stack to manage the exploration order (LIFO structure for DFS).
        var startNode = new Node(start);            // Create start node.
        
        frontier.Push(startNode);                   // Add start node to exploration stack.
        _nodeMap[start] = startNode;                // Save start node to node map.

        // Continue exploration until no more nodes are left in the frontier.
        while (frontier.Count > 0)
        {
            var currentNode = frontier.Pop();       // Pop the next node from the stack.
            var currentDepth = currentNode.DepthLimit;  // Track current node's depth.

            // Check if goal has been reached.
            if (currentNode.Point == goal)
            {
                return SampleUtils.ReconstructPath(currentNode, _nodeMap);  // Goal found; reconstruct and return the path.
            }

            currentNode.State = NodeState.Closed;       // Mark current node as explored/closed.
            _nodeMap[currentNode.Point] = currentNode;  // Update current node in the map.

            // Iterate through each valid neighbor of the current node.
            foreach (var neighbor in SampleUtils.GetNeighbors(currentNode.Point, gridSize, NeighborSearchType.FourWay))
            {
                // Try to retrieve or create the neighbor node.
                if (!_nodeMap.TryGetValue(neighbor, out var neighborNode))
                {
                    neighborNode = new Node(neighbor);  // Create new node for neighbor position.
                }

                var neighborDepth = currentDepth + 1;  // Calculate neighbor depth relative to current node.

                // Skip if neighbor is blocked, already explored, or exceeds depth limit.
                if (!neighborNode.Walkable || neighborNode.State != NodeState.Unvisited || neighborDepth > depthLimit)
                {
                    continue;
                }

                neighborNode.DepthLimit = neighborDepth;                // Store computed depth for neighbor.
                neighborNode.ParentPoint = currentNode.Point;           // Set parent for path reconstruction.
                neighborNode.State = NodeState.Open;                    // Mark neighbor as open for exploration.
                _nodeMap[neighbor] = neighborNode;                      // Save neighbor node to node map.
                frontier.Push(neighborNode);                            // Push neighbor to the frontier stack.
            }
        }

        return new Stack<Node>();  // No path found within the depth constraint; return empty path.
    }
}
