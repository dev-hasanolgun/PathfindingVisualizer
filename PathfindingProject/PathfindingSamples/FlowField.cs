namespace PathfindingProject.PathfindingSamples;

/// <summary>
/// A simplified Flow Field algorithm that calculates and propagates traversal costs from a goal position across the grid.
/// It doesn't use inheritance or step-by-step processing, making it easier for educational demonstrations.
/// </summary>
public class FlowField
{
    private readonly Dictionary<Point, Node> _nodeMap = new();   // Stores calculated traversal costs and direction information.

    /// <summary>
    /// Generates the flow field from the goal point outward.
    /// </summary>
    /// <param name="gridSize">The dimensions of the grid.</param>
    /// <param name="goal">The goal position from which costs propagate.</param>
    /// <param name="neighborSearchType">Determines how neighbors are evaluated (FourWay or EightWay).</param>
    public void GenerateFlowField(Point gridSize, Point goal, NeighborSearchType neighborSearchType = NeighborSearchType.FourWay)
    {
        var frontier = new Queue<Node>();                                       // Queue for spreading movement costs starting from the goal.
        var goalNode = new Node(goal) { GCost = 0, State = NodeState.Open };    // Create goal node with initial parameters.
        
        frontier.Enqueue(goalNode);   // Add goal node to exploration queue.
        _nodeMap[goal] = goalNode;    // Save goal node to node map.

        // Continue exploration until no more nodes are left.
        while (frontier.Count > 0)
        {
            var currentNode = frontier.Dequeue();   // Dequeue the next node from the queue.

            currentNode.State = NodeState.Closed;       // Mark current node as explored/closed.
            _nodeMap[currentNode.Point] = currentNode;  // Update current node's state in the node map.

            // Iterate through each neighbor around current node.
            foreach (var neighborPoint in SampleUtils.GetNeighbors(currentNode.Point, gridSize, neighborSearchType))
            {
                // Try to retrieve or create neighbor node.
                if (!_nodeMap.TryGetValue(neighborPoint, out var neighborNode))
                {
                    neighborNode = new Node(neighborPoint); // If neighbor not found in node map, create a new node at neighbor's position.
                }

                // Skip if neighbor is blocked or already visited.
                if (!neighborNode.Walkable || neighborNode.State == NodeState.Closed)
                {
                    continue;
                }

                var totalCost = currentNode.GCost + neighborNode.CellCost;  // Calculate new cost to reach neighbor.

                // Update neighbor if unvisited or found a cheaper path.
                if (neighborNode.State == NodeState.Unvisited || totalCost < neighborNode.GCost)
                {
                    neighborNode.GCost = totalCost;                 // Set new G-Cost for neighbor node
                    neighborNode.ParentPoint = currentNode.Point;   // Set parent for path reconstruction.
                    neighborNode.State = NodeState.Open;            // Mark neighbor node as open.
                    _nodeMap[neighborPoint] = neighborNode;         // Save neighbor node to node map with updated information.
                    frontier.Enqueue(neighborNode);                 // Add neighbor node to the frontier queue to be explored next.
                }
            }
        }
    }

    /// <summary>
    /// Constructs a path from the start position following the flow field back to the goal.
    /// </summary>
    /// <param name="start">The starting point from which to follow the flow field.</param>
    /// <param name="goal">The goal point to reach.</param>
    /// <returns>A stack of nodes representing the path.</returns>
    public Stack<Node> GetPath(Point start, Point goal)
    {
        var path = new Stack<Node>();   // Stack to store the resulting path.

        // Try to get the node at the start position.
        if (!_nodeMap.TryGetValue(start, out var currentNode))
        {
            return path;   // Return empty path if the start node is not found.
        }
        
        var visited = new HashSet<Point>();  // Set to keep track of visited points to avoid infinite loops.

        // Trace the path from the start to the goal following parent pointers.
        while (currentNode.Point != goal)
        {
            // Check if the current node has already been visited (infinite loop prevention).
            if (visited.Contains(currentNode.Point))
            {
                break;
            }
            
            visited.Add(currentNode.Point);  // Mark current node as visited.
            path.Push(currentNode);          // Add current node to the path stack.

            // Move to the parent node to continue path tracing.
            if (currentNode.ParentPoint == null || !_nodeMap.TryGetValue(currentNode.ParentPoint.Value, out currentNode))
            {
                break;
            }
        }

        // Check if the final node reached is indeed the goal node.
        if (currentNode.Point == goal)
        {
            path.Push(currentNode);  // Push the goal node onto the path stack.
        }
        
        return path;    // Return the completed path.
    }
}