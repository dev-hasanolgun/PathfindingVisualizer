﻿using Point = System.Drawing.Point;

namespace PathfindingProject.Pathfinding;

/// <summary>
/// Represents a single cell/node used in pathfinding computations.
/// </summary>
public struct Node
{
    /// <summary>
    /// Position of the node on the grid.
    /// </summary>
    public Point Point;

    /// <summary>
    /// The parent node's position in the path (used for backtracking).
    /// </summary>
    public Point? ParentPoint;

    /// <summary>Cost from start to this node (g).</summary>
    public int GCost;

    /// <summary>Estimated cost from this node to goal (h).</summary>
    public int HCost;

    /// <summary>Extra movement cost for this cell (terrain weight).</summary>
    public int CellCost;
    
    /// <summary> Depth of the node from the start node.</summary>
    public int Depth;

    /// <summary>True if the node is walkable.</summary>
    public bool Walkable;

    /// <summary>Current state of the node in the search process.</summary>
    public NodeState State;

    /// <summary>Total cost (G + H).</summary>
    public int FCost => GCost + HCost;

    /// <summary>Creates a new node with default values and a specified position.</summary>
    public Node(Point p)
    {
        Point = p;
        ParentPoint = null;
        GCost = 0;
        HCost = 0;
        CellCost = 0;
        Walkable = true;
        State = NodeState.Unvisited;
    }

    /// <summary>Search state of the node.</summary>
    public enum NodeState
    {
        Unvisited,
        Open,
        Closed
    }
}