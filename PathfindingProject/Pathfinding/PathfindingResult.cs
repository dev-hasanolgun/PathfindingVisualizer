namespace PathfindingProject.Pathfinding;

/// <summary>
/// Stores the results of a pathfinding run, including node expansion state and the resulting path.
/// </summary>
public class PathfindingResult
{
    /// <summary>
    /// All visited and expanded nodes during the search, keyed by their grid position.
    /// </summary>
    public Dictionary<Point, Node> NodeMap { get; } = new();

    /// <summary>
    /// The final computed path from start to goal, if found.
    /// </summary>
    public Stack<Node> Path { get; set; } = new();
}