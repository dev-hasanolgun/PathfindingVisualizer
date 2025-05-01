using PathfindingProject.Pathfinding.Enums;
using PathfindingProject.Pathfinding.Frontiers;

namespace PathfindingProject.Pathfinding;

/// <summary>
/// Abstract base class for implementing different pathfinding algorithms. Manages search state, frontier, and step logging.
/// </summary>
public abstract class PathSearchBase
{
    public int TotalStepsTaken;
    public int CurrentOpenNodes;
    public int CurrentClosedNodes;

    public bool PathFound;
    public bool SearchComplete;

    public HeuristicMode HeuristicMode;
    public NeighborMode NeighborMode;

    protected readonly IFrontier _frontier;
    protected Dictionary<Point, Node> _nodeMap = new();
    protected Dictionary<int, List<PathStepLog>>? _explanations;

    protected Point _gridSize;
    protected Point _startPoint;
    protected Point _endPoint;
    protected bool _recordExplanations;

    /// <summary>
    /// Optional step-by-step log of search activity (for debugging or visualization).
    /// </summary>
    public Dictionary<int, List<PathStepLog>>? StepLog => _explanations;

    public PathSearchBase(IFrontier frontier)
    {
        _frontier = frontier;
    }

    /// <summary>
    /// Resets the search state and clears all previous results.
    /// </summary>
    public virtual void Reset()
    {
        _frontier.Reset();
        _explanations?.Clear();

        PathFound = false;
        SearchComplete = false;
        CurrentOpenNodes = 0;
        CurrentClosedNodes = 0;
        TotalStepsTaken = 0;
    }

    /// <summary>
    /// Prepares a new search instance with the given configuration.
    /// </summary>
    public virtual void Initialize(Point gridSize, Point start, Point end, Dictionary<Point, Node> nodeMap, bool recordSteps = false)
    {
        Reset();

        _gridSize = gridSize;
        _startPoint = start;
        _endPoint = end;
        _nodeMap = nodeMap;

        _recordExplanations = recordSteps;

        if (_recordExplanations)
        {
            var initLog = new PathStepLog
            {
                Message = $"Initializing search from {_startPoint} to {_endPoint}.",
                Node = _startPoint,
                Type = StepType.Info
            };

            _explanations = new Dictionary<int, List<PathStepLog>>
            {
                { -1, new List<PathStepLog> { initLog } }
            };
        }
    }

    /// <summary>
    /// Executes one iteration (step) of the pathfinding process.
    /// </summary>
    public abstract bool PerformStep();

    /// <summary>
    /// Completes the search by executing steps until the frontier is empty or the path is found.
    /// </summary>
    public virtual bool CompleteSearch()
    {
        if (SearchComplete) return PathFound;

        while (!SearchComplete)
        {
            PerformStep();
        }

        if (_recordExplanations)
            LogStep("Search completed", StepType.Info);

        return PathFound;
    }

    /// <summary>
    /// Reconstructs and returns the final path, if found.
    /// </summary>
    public virtual Stack<Node> GetPath()
    {
        var path = new Stack<Node>();

        if (!PathFound || !_nodeMap.TryGetValue(_endPoint, out var currentNode))
            return path;

        var visited = new HashSet<Point>();

        while (currentNode.Point != _startPoint)
        {
            // Prevent infinite loops in case of broken parent chains
            if (visited.Contains(currentNode.Point))
                break;

            visited.Add(currentNode.Point);
            path.Push(currentNode);

            if (currentNode.ParentPoint == null)
                break;

            if (!_nodeMap.TryGetValue(currentNode.ParentPoint.Value, out currentNode))
                break;
        }

        return path;
    }

    /// <summary>
    /// Tries to reconstruct a valid path and returns true if successful.
    /// </summary>
    public virtual bool TryGetPath(out Stack<Node> path)
    {
        path = new Stack<Node>();

        if (!_nodeMap.TryGetValue(_endPoint, out var currentNode))
            return false;

        while (currentNode.ParentPoint != null && currentNode.Point != _startPoint)
        {
            path.Push(currentNode);

            if (!_nodeMap.TryGetValue(currentNode.ParentPoint.Value, out currentNode))
                return false;
        }

        return currentNode.Point == _startPoint;
    }

    /// <summary>
    /// Returns the current node map used during the search.
    /// </summary>
    public Dictionary<Point, Node> GetNodeMap() => _nodeMap;

    /// <summary>
    /// Logs a message at the current step if explanation recording is enabled.
    /// </summary>
    protected void LogStep(string message, StepType type = StepType.Info, Point? node = null)
    {
        if (!_recordExplanations) return;

        if (!_explanations!.ContainsKey(TotalStepsTaken))
            _explanations[TotalStepsTaken] = new List<PathStepLog>();

        _explanations[TotalStepsTaken].Add(new PathStepLog
        {
            Message = message,
            Node = node,
            Type = type
        });
    }
}