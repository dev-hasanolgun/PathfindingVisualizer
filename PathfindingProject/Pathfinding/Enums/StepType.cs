namespace PathfindingProject.Pathfinding.Enums;

/// <summary>
/// Describes the role or significance of a step taken during pathfinding.
/// Used for explanation logging and visualization.
/// </summary>
public enum StepType
{
    /// <summary>
    /// General informational message or initialization.
    /// </summary>
    Info,

    /// <summary>
    /// Node was examined but skipped (e.g. already closed or not walkable).
    /// </summary>
    Skipped,

    /// <summary>
    /// Node is being processed in the current step.
    /// </summary>
    Visited,

    /// <summary>
    /// Node has been added to the frontier/open list.
    /// </summary>
    AddedToOpen,

    /// <summary>
    /// The goal node has been reached — search is complete.
    /// </summary>
    GoalReached
}