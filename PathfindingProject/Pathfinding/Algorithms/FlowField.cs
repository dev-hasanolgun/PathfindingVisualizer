using PathfindingProject.Core;
using PathfindingProject.Pathfinding.Enums;
using PathfindingProject.Pathfinding.Frontiers;

namespace PathfindingProject.Pathfinding.Algorithms;

/// <summary>
/// A pathfinding algorithm that builds a flow field from the goal back to all reachable points.
/// </summary>
public class FlowField : PathSearchBase
{
    public FlowField(IFrontier frontier) : base(frontier) { }

    /// <summary>
    /// Initializes the flow field search from the goal point.
    /// </summary>
    public override void Initialize(Point gridSize, Point start, Point end, Dictionary<Point, Node> nodeMap, float weight = 0, int depthLimit = 0, bool recordSteps = false)
    {
        base.Initialize(gridSize, start, end, nodeMap, weight, depthLimit, recordSteps);

        // Start the flow from the goal
        var node = new Node(_endPoint);
        _frontier.Add(node);
    }

    /// <summary>
    /// Performs a single step of the flow field algorithm.
    /// </summary>
    /// <returns>True if a step was performed; false if search is complete or no nodes remain.</returns>
    public override bool PerformStep()
    {
        if (SearchComplete)
        {
            LogStep("Search is already complete — skipping step.");
            return false;
        }

        if (_frontier.IsEmpty)
        {
            LogStep("Frontier is empty. No more nodes to process.", StepType.Info);
            SearchComplete = true;
            return false;
        }

        var currentNode = _frontier.GetNext();
        var currentDepth = currentNode.Depth;

        if (_nodeMap.TryGetValue(currentNode.Point, out var knownNode) && knownNode.State == Node.NodeState.Closed)
        {
            LogStep($"Node {currentNode.Point} is already closed — skipping.", StepType.Skipped, currentNode.Point);
            return PerformStep(); // Try next node
        }

        LogStep($"Processing node {currentNode.Point}", StepType.Visited, currentNode.Point);

        currentNode.State = Node.NodeState.Closed;
        _nodeMap[currentNode.Point] = currentNode;
        CurrentClosedNodes--;
        CurrentOpenNodes--;

        var currentCost = currentNode.GCost;
        var neighbors = Extensions.GetNeighbors(currentNode.Point, _gridSize, NeighborMode);

        for (int i = 0; i < neighbors.Count; i++)
        {
            var neighborPoint = neighbors[i];

            if (!_nodeMap.TryGetValue(neighborPoint, out var neighborNode)) neighborNode = new Node(neighborPoint);

            var neighborDepth = currentDepth + 1;
            
            if (neighborNode.State == Node.NodeState.Unvisited && _depthLimit > 0 && neighborDepth > _depthLimit) continue;
            
            if (!neighborNode.Walkable)
            {
                LogStep($"Skipped neighbor {neighborPoint} — not walkable.", StepType.Skipped, neighborPoint);
                continue;
            }

            if (neighborNode.State == Node.NodeState.Closed)
            {
                LogStep($"Skipped neighbor {neighborPoint} — already closed.", StepType.Skipped, neighborPoint);
                continue;
            }

            var movementCost = neighborNode.CellCost == 0 ? 1 : neighborNode.CellCost;
            var totalCost = currentCost + movementCost;

            if (neighborNode.State == Node.NodeState.Unvisited || totalCost < _nodeMap[neighborPoint].GCost)
            {
                neighborNode.GCost = totalCost; // Used as FlowField cost value to adapt existing Node struct instead of creating separate field
                neighborNode.ParentPoint = currentNode.Point;
                neighborNode.State = Node.NodeState.Open;
                neighborNode.Depth = neighborDepth;
                _nodeMap[neighborPoint] = neighborNode;
                _frontier.Add(neighborNode);
                CurrentOpenNodes++;
                LogStep($"Added neighbor {neighborNode.Point}, Cost={neighborNode.FCost}", StepType.AddedToOpen, neighborPoint);
            }
        }

        // If we've successfully closed the start node, the flow field can now guide a path.
        if (_nodeMap.TryGetValue(_startPoint, out var startNode) && startNode.State == Node.NodeState.Closed && startNode.ParentPoint != null)
        {
            PathFound = true;
            LogStep("Path to start point has been established via flow field.", StepType.GoalReached, _startPoint);
        }

        TotalStepsTaken++;
        return true;
    }

    /// <summary>
    /// Builds and returns the path by tracing the flow field from start to goal.
    /// </summary>
    /// <returns>A stack of nodes forming the path, or an empty stack if no path was found.</returns>
    public override Stack<Node> GetPath()
    {
        var path = new Stack<Node>();
        if (!PathFound || !_nodeMap.TryGetValue(_startPoint, out var currentNode))
            return path;

        var visited = new HashSet<Point>();

        // Follow the flow field from start to goal (by parent pointers)
        while (currentNode.Point != _endPoint)
        {
            if (visited.Contains(currentNode.Point)) break; // Safety: prevent infinite loops
            visited.Add(currentNode.Point);
            path.Push(currentNode);

            if (currentNode.ParentPoint == null) break;
            if (!_nodeMap.TryGetValue(currentNode.ParentPoint.Value, out currentNode)) break;
        }

        // Include the end point
        if (currentNode.Point == _endPoint) path.Push(currentNode);

        return path;
    }

    /// <summary>
    /// Completes the flow field search in one go, setting PathFound if the start is reachable.
    /// </summary>
    /// <returns>True if the start point was reached; otherwise, false.</returns>
    public override bool CompleteSearch()
    {
        base.CompleteSearch();

        // Mark success if the start has been reached
        if (_nodeMap.TryGetValue(_startPoint, out var startNode) &&
            startNode.State == Node.NodeState.Closed &&
            startNode.ParentPoint != null)
        {
            PathFound = true;
        }

        return PathFound;
    }
}