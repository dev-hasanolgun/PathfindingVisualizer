namespace PathfindingProject.PathfindingSamples;

/// <summary>
/// Generalized A* search algorithm efficiently finds the shortest path by considering both the cost
/// to reach the current node and the weighted estimated cost from the current node to the goal.
/// It can provide faster results by prioritizing the heuristic more aggressively, but does not guarantee the optimal path if the weight exceeds 1.
/// </summary>

public class GeneralizedAStar
{
    private readonly Dictionary<Point, Node> _nodeMap = new();  // Tracks nodes and their current exploration states.

    /// <summary>
    /// Executes the Weighted A* search algorithm to find the shortest path.
    /// </summary>
    /// <param name="start">Starting position on the grid.</param>
    /// <param name="goal">Target position on the grid.</param>
    /// <param name="gridSize">Dimensions of the grid.</param>
    /// <param name="heuristic">Heuristic function used to estimate distance.</param>
    /// <param name="weight">The multiplier applied to the heuristic function to control its influence during pathfinding.</param>
    /// <returns>Returns a stack containing the nodes forming the shortest path.</returns>
    public Stack<Node> FindPath(Point start, Point goal, Point gridSize, HeuristicFunction heuristic, float weight)
    {
        var frontier = new PriorityQueue<Node, int>();                             // Priority queue for selecting lowest-cost node first.
        var hCost = SampleUtils.EvaluateHeuristic(heuristic, start, goal);  // Evaluate H-Cost for start node
        var startNode = new Node(start) { HCost = hCost };                         // Create start node with initial parameters.

        frontier.Enqueue(startNode, 0 + (int)(weight * hCost));   // Add start node to exploration queue with weighted heuristic.
        _nodeMap[start] = startNode;          // Save start node to node map.

        // Continue exploration until no more nodes are left.
        while (frontier.Count > 0)
        {
            var currentNode = frontier.Dequeue();   // Dequeue the next node from the queue.

            // Check if goal is reached.
            if (currentNode.Point == goal)
            {
                return SampleUtils.ReconstructPath(currentNode, _nodeMap);  // Goal found, reconstruct and return the path.
            }

            currentNode.State = NodeState.Closed;       // Mark current node as explored/closed.
            _nodeMap[currentNode.Point] = currentNode;  // Update current node's state in the node map.

            // Iterate through each neighbor around current node.
            foreach (var neighbor in SampleUtils.GetNeighbors(currentNode.Point, gridSize, NeighborSearchType.FourWay))
            {
                // Try to retrieve or create neighbor node.
                if (!_nodeMap.TryGetValue(neighbor, out var neighborNode))
                {
                    neighborNode = new Node(neighbor);  // If neighbor not found in node map, create a new node at neighbor's position.
                }

                // Skip if neighbor is blocked or already visited.
                if (!neighborNode.Walkable || neighborNode.State == NodeState.Closed)
                {
                    continue;
                }

                var stepCost = SampleUtils.GetStepCost(currentNode.Point, neighbor);         // Step cost between current node and neighbor node
                var costToNeighbor = currentNode.GCost + stepCost + neighborNode.CellCost;   // Calculate new cost to reach neighbor.

                // Update neighbor if unvisited or found a cheaper path.
                if (neighborNode.State == NodeState.Unvisited || costToNeighbor < neighborNode.GCost)
                {
                    neighborNode.GCost = costToNeighbor;                                                 // Set new G-Cost for neighbor node
                    neighborNode.HCost = SampleUtils.EvaluateHeuristic(heuristic,neighbor, goal);    // Evaluate new H-Cost for neighbor node
                    neighborNode.ParentPoint = currentNode.Point;                                        // Set parent for path reconstruction.
                    neighborNode.State = NodeState.Open;                                                 // Mark neighbor node as open.
                    _nodeMap[neighbor] = neighborNode;                                                   // Save neighbor node to node map with updated information.
                    frontier.Enqueue(neighborNode, neighborNode.GCost + (int)(weight * neighborNode.HCost));                                  // Add neighbor node to the frontier queue with weighted heuristic to be explored next.
                }
            }
        }

        return new Stack<Node>();   // No path found; return empty path.
    }
}