using PathfindingProject.Pathfinding.Enums;

namespace PathfindingProject.Pathfinding;

/// <summary>
/// Represents a single explanation or debug entry during the pathfinding process.
/// Used for step-by-step visualization and logging.
/// </summary>
public class PathStepLog
{
    /// <summary>
    /// The main message or description for the step.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// The grid point this step relates to (optional).
    /// </summary>
    public Point? Node { get; init; }

    /// <summary>
    /// The type or category of the step (e.g. visited, skipped, goal reached).
    /// </summary>
    public StepType Type { get; init; } = StepType.Info;
}