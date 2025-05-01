namespace PathfindingProject.Pathfinding.Enums;

/// <summary>
/// Defines which directions are considered neighbors when evaluating adjacent cells.
/// </summary>
public enum NeighborMode
{
    /// <summary>
    /// Cardinal directions only (↑ ↓ ← →).
    /// </summary>
    FourWay,

    /// <summary>
    /// Cardinal and diagonal directions (↑ ↓ ← → ↖ ↗ ↘ ↙).
    /// </summary>
    EightWay
}