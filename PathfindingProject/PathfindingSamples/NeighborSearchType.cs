namespace PathfindingProject.PathfindingSamples;

/// <summary>
/// Defines which directions are considered neighbors when evaluating adjacent cells.
/// </summary>
public enum NeighborSearchType
{
    /// <summary>
    /// Cardinal directions only.
    /// </summary>
    FourWay,

    /// <summary>
    /// Cardinal and diagonal directions.
    /// </summary>
    EightWay
}