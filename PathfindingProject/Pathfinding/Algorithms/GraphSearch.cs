﻿using System.Diagnostics;
using PathfindingProject.Core;
using PathfindingProject.Pathfinding.Enums;
using PathfindingProject.Pathfinding.Frontiers;

namespace PathfindingProject.Pathfinding.Algorithms;

/// <summary>
/// A flexible graph search implementation that adapts to both informed and uninformed searches with dynamic frontier.
/// </summary>
public class GraphSearch : PathSearchBase
{
    public GraphSearch(IFrontier frontier) : base(frontier) { }

    public override void Initialize(Point gridSize, Point start, Point end, Dictionary<Point, Node> nodeMap, float weight = 0, int depthLimit = 0, bool recordSteps = false)
    {
        base.Initialize(gridSize, start, end, nodeMap, weight, depthLimit, recordSteps);

        var startNode = new Node(_startPoint)
        {
            GCost = 0,
            HCost = HeuristicMode != HeuristicMode.None
                ? HeuristicMode.Evaluate(_startPoint, _endPoint)
                : 0,
            State = Node.NodeState.Open
        };

        _nodeMap[_startPoint] = startNode;
        var priority = GetPriority(startNode.GCost, startNode.HCost);
        _frontier.Add(startNode, priority);
        CurrentOpenNodes++;
    }

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
            return PerformStep(); // Try next node in frontier
        }

        LogStep($"Processing node {currentNode.Point}", StepType.Visited, currentNode.Point);

        currentNode.State = Node.NodeState.Closed;
        _nodeMap[currentNode.Point] = currentNode;
        CurrentClosedNodes++;
        CurrentOpenNodes--;

        // Goal check
        if (currentNode.Point == _endPoint)
        {
            LogStep($"Reached goal node {currentNode.Point}. Path found!", StepType.GoalReached, currentNode.Point);
            PathFound = true;
            SearchComplete = true;
            return true;
        }

        var neighbors = Extensions.GetNeighbors(currentNode.Point, _gridSize, NeighborMode);

        foreach (var neighborPoint in neighbors)
        {
            if (!_nodeMap.TryGetValue(neighborPoint, out var neighborNode)) neighborNode = new Node(neighborPoint);

            var neighborDepth = currentDepth + 1;
            // Debug.WriteLine(neighborDepth + " " + _depthLimit + " " + (_depthLimit > 0) + " " + (neighborDepth > _depthLimit));
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

            if (HeuristicMode == HeuristicMode.None)
            {
                LogStep($"Processing neighbor {neighborPoint} with no heuristic (BFS-style).", StepType.Info, neighborPoint);
                ProcessNeighborNoCost(currentNode, neighborNode, neighborDepth);
            }
            else
            {
                var oldGCost = neighborNode.GCost;
                ProcessNeighbor(currentNode, neighborNode, neighborDepth);
                var newGCost = neighborNode.GCost;

                if (oldGCost != newGCost || neighborNode.State == Node.NodeState.Open)
                {
                    LogStep($"Updated or added neighbor {neighborPoint} — G: {oldGCost} → {newGCost}, F: {neighborNode.FCost}", StepType.AddedToOpen, neighborPoint);
                }
            }

            if (neighborPoint == _endPoint)
            {
                LogStep($"Found goal {neighborPoint} during neighbor expansion.", StepType.GoalReached, neighborPoint);
                PathFound = true;
                SearchComplete = true;
                return true;
            }
        }

        TotalStepsTaken++;
        return true;
    }

    /// <summary>
    /// Handles neighbor processing using heuristic function (used for informed searches).
    /// </summary>
    private void ProcessNeighbor(Node currentNode, Node neighborNode, int depth)
    {
        var stepCost = Extensions.GetStepCost(currentNode.Point, neighborNode.Point);
        var costToNeighbor = currentNode.GCost + stepCost + neighborNode.CellCost;

        if (costToNeighbor < neighborNode.GCost || neighborNode.State == Node.NodeState.Unvisited)
        {
            neighborNode.GCost = costToNeighbor;
            neighborNode.HCost = HeuristicMode.Evaluate(_endPoint, neighborNode.Point);
            neighborNode.Depth = depth;
            neighborNode.ParentPoint = currentNode.Point;

            var priority = GetPriority(neighborNode.GCost, neighborNode.HCost);
            _frontier.Add(neighborNode, priority);

            if (neighborNode.State == Node.NodeState.Unvisited)
            {
                neighborNode.State = Node.NodeState.Open;
                CurrentOpenNodes++;
            }

            _nodeMap[neighborNode.Point] = neighborNode;

            LogStep($"Queued {neighborNode.Point} → G={neighborNode.GCost}, H={neighborNode.HCost}, F={neighborNode.FCost}, priority={priority}");
        }
    }

    /// <summary>
    /// Handles neighbor processing without heuristic function (used for uninformed searches).
    /// </summary>
    private void ProcessNeighborNoCost(Node currentNode, Node neighborNode, int depth)
    {
        neighborNode.Depth = depth;
        if (neighborNode.State == Node.NodeState.Unvisited)
        {
            neighborNode.ParentPoint = currentNode.Point;
            neighborNode.State = Node.NodeState.Open;
            _frontier.Add(neighborNode);
            _nodeMap[neighborNode.Point] = neighborNode;
            CurrentOpenNodes++;
        }
    }
}