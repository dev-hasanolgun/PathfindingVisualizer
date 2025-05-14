using System.Diagnostics;
using System.Text;
using PathfindingProject.Core;
using PathfindingProject.Grid;
using PathfindingProject.Pathfinding.Algorithms;
using PathfindingProject.Pathfinding.Enums;
using PathfindingProject.Pathfinding.Frontiers;
using PathfindingProject.Scene;
using PathfindingProject.UI;

namespace PathfindingProject.Pathfinding;

/// <summary>
/// Executes grid based pathfinding algorithms on runtime.
/// Supports step based visualization and full search execution.
/// </summary>
public class Pathfinder : SceneBehaviour
{
    public readonly PathfindingResult PathfindingResult = new();

    public SearchMode SearchMode = SearchMode.AStarSearch;
    public HeuristicMode HeuristicMode = HeuristicMode.Manhattan;
    public NeighborMode NeighborMode = NeighborMode.FourWay;
    public float HeuristicWeight;
    public int SearchSpeed = 5;

    private GridController _gridController = null!;
    private UIController _uiController = null!;
    private PathSearchBase _pathSearch = null!;

    private float _searchTimer;
    private float _currentStepsPercentage;
    private float _holdInterval = 0.05f;
    private float _holdDelay = 0.25f;
    private float _holdTimer;
    private float _computationTime;
    private int _currentStep;
    private int _totalSteps;
    private bool _isSearching;
    private bool _pathFound;

    public override void Awake()
    {
        _gridController = SceneRegistry.Resolve<GridController>();
        _uiController = SceneRegistry.Resolve<UIController>();

        _gridController.OnGridUpdate += UpdateNodeMap;
        _pathSearch = new GraphSearch(new HeapFrontier());
    }

    public override void Start()
    {
        SaveTotalSteps();
    }

    public override void Update()
    {
        // Step input with hold support (Right = forward, Left = backward)
        if (InputManager.GetKey(Keys.Right))
        {
            if (_holdTimer > _holdDelay + _holdInterval)
            {
                ExecuteStepInput(1);
                _holdTimer = _holdDelay;
            }

            _holdTimer += Time.DeltaTime;
        }
        else if (InputManager.GetKey(Keys.Left))
        {
            if (_holdTimer > _holdDelay + _holdInterval)
            {
                ExecuteStepInput(-1);
                _holdTimer = _holdDelay;
            }

            _holdTimer += Time.DeltaTime;
        }

        // Initial key press
        if (InputManager.GetKeyDown(Keys.Right))
        {
            ExecuteStepInput(1);
            _holdTimer = 0f;
        }
        else if (InputManager.GetKeyDown(Keys.Left))
        {
            ExecuteStepInput(-1);
            _holdTimer = 0f;
        }

        // Auto-step forward during active search
        if (_isSearching)
        {
            if (_searchTimer > 1f)
            {
                MovePathStep(_currentStep + 1);
                _searchTimer = 0f;
            }

            _searchTimer += Time.DeltaTime * SearchSpeed;
        }
    }

    /// <summary>
    /// Starts or stops automatic search execution.
    /// </summary>
    public bool ToggleSearch()
    {
        _isSearching = !_isSearching;
        return _isSearching;
    }
    
    /// <summary>
    /// Completes the search immediately and stores the final path.
    /// </summary>
    public void CompletePathSearch()
    {
        var pathFound = _pathSearch.CompleteSearch();
        if (pathFound)
        {
            PathfindingResult.Path = _pathSearch.GetPath();
        }

        _currentStep = _pathSearch.TotalStepsTaken;
        _uiController.UpdateTrackers(_pathSearch.CurrentOpenNodes, _pathSearch.CurrentClosedNodes, PathfindingResult.Path.Count, GetTotalPathCost(),_computationTime);
    }

    /// <summary>
    /// Advances or resets the search to a target step.
    /// </summary>
    public void MovePathStep(int targetStep)
    {
        if (targetStep > _totalSteps) targetStep = _totalSteps;

        // Restart search if going backwards
        if (targetStep <= _currentStep)
        {
            ResetResult();

            var gridModel = _gridController.Model;
            PathfindingResult.NodeMap.MergeWith(gridModel.GridOverrides);

            Initialize(gridModel.StartPoint, gridModel.EndPoint, PathfindingResult.NodeMap, recordSteps: true);
            _currentStep = 0;
        }

        for (int i = _currentStep; i < targetStep; i++)
        {
            _pathSearch.PerformStep();
            _currentStep = i + 1;

            if (_pathSearch.PathFound && !_pathFound)
            {
                _pathFound = true;
                PathfindingResult.Path = _pathSearch.GetPath();
                if (_isSearching)
                {
                    _isSearching = false;
                    _uiController.UpdateSearchButton(_isSearching);
                }

                // Stop early for non flow field searches
                if (SearchMode != SearchMode.FlowField) break;
            }
        }

        UpdatePathStepLog();

        _currentStepsPercentage = _totalSteps != 0 ? (float)_currentStep / _totalSteps : 0f;
        _uiController.UpdateTrackers(_pathSearch.CurrentOpenNodes, _pathSearch.CurrentClosedNodes, PathfindingResult.Path.Count, GetTotalPathCost(),_computationTime);
        _uiController.UpdateStepSlider(_currentStep, _totalSteps);
    }

    /// <summary>
    /// Called when the grid is changed (e.g. resizing, obstacle editing). Rebuilds the search.
    /// </summary>
    public void UpdateNodeMap()
    {
        SaveTotalSteps();
        ResetResult();

        var gridModel = _gridController.Model;
        PathfindingResult.NodeMap.MergeWith(gridModel.GridOverrides);

        Initialize(gridModel.StartPoint, gridModel.EndPoint, PathfindingResult.NodeMap, recordSteps: true);
        _currentStep = (int)(_totalSteps * _currentStepsPercentage);

        for (int i = 1; i <= _currentStep; i++)
        {
            _pathSearch.PerformStep();

            if (_pathSearch.PathFound && !_pathFound)
            {
                _pathFound = true;
                PathfindingResult.Path = _pathSearch.GetPath();
                _currentStep = i;

                if (SearchMode != SearchMode.FlowField)
                    break;
            }
        }

        UpdatePathStepLog();
        _uiController.UpdateTrackers(_pathSearch.CurrentOpenNodes, _pathSearch.CurrentClosedNodes, PathfindingResult.Path.Count, GetTotalPathCost(),_computationTime);
        _uiController.UpdateSearchButton(_isSearching = false);
        _uiController.UpdateStepSlider(_currentStep, _totalSteps);
    }
    
    /// <summary>
    /// Clears the entire node map except for the start and end point overrides.
    /// </summary>
    public void ClearNodeMapExceptEndpoints()
    {
        _gridController.Model.GridOverrides.Clear();
        ResetResult();
        _currentStep = 0;

        var gridModel = _gridController.Model;
        PathfindingResult.NodeMap.MergeWith(gridModel.GridOverrides);

        Initialize(gridModel.StartPoint, gridModel.EndPoint, PathfindingResult.NodeMap, recordSteps: true);
    }

    /// <summary>
    /// Advances or reverses the step count in the search manually.
    /// </summary>
    private void ExecuteStepInput(int direction)
    {
        MovePathStep(_currentStep + direction);
        _isSearching = false;
        _uiController.UpdateSearchButton(_isSearching);
    }

    /// <summary>
    /// Applies the current UI configured search mode options.
    /// </summary>
    private void UpdateSearchModes(HeuristicMode heuristicMode, NeighborMode neighborMode)
    {
        _pathSearch.HeuristicMode = heuristicMode;
        _pathSearch.NeighborMode = neighborMode;
    }

    /// <summary>
    /// Runs a complete search to calculate the max number of steps.
    /// </summary>
    private void SaveTotalSteps()
    {
        var gridModel = _gridController.Model;
        var tempNodeMap = new Dictionary<Point, Node>();
        tempNodeMap.MergeWith(gridModel.GridOverrides);

        UpdateSearchModes(HeuristicMode, NeighborMode);
        Initialize(gridModel.StartPoint, gridModel.EndPoint, tempNodeMap);

        var st = new Stopwatch();
        st.Start();
        _pathSearch.CompleteSearch();
        st.Stop();
        _computationTime = (float) st.Elapsed.TotalMilliseconds;
        st.Reset();
        _totalSteps = _pathSearch.TotalStepsTaken + 1;
    }

    /// <summary>
    /// Initializes the search algorithm with the current state.
    /// </summary>
    private void Initialize(Point startPoint, Point endPoint, Dictionary<Point, Node> nodeMap, bool recordSteps = false)
    {
        UpdateSearchModes(HeuristicMode, NeighborMode);
        _pathSearch.Initialize(_gridController.Model.GridSize, startPoint, endPoint, nodeMap, HeuristicWeight, recordSteps);
    }

    /// <summary>
    /// Clears all visual/path state.
    /// </summary>
    private void ResetResult()
    {
        PathfindingResult.Path.Clear();
        PathfindingResult.NodeMap.Clear();
        _pathFound = false;
    }

    /// <summary>
    /// Updates the text explanation panel for the current step.
    /// </summary>
    private void UpdatePathStepLog()
    {
        if (_pathSearch.StepLog != null && _currentStep > 0 && _currentStep <= _pathSearch.StepLog.Count)
        {
            var sb = new StringBuilder();
            var logList = _pathSearch.StepLog[_currentStep - 1];

            for (int i = 0; i < logList.Count; i++)
            {
                sb.Append(logList[i].Message);
                if (i < logList.Count - 1) sb.AppendLine();
            }

            _uiController.StepExplanationUI.TextContent = sb.ToString();
        }
    }
    
    /// <summary>
    /// Calculates the total cost of the current path found.
    /// For BFS and DFS, uses 10 for straight and 14 for diagonal steps based on NeighborMode.
    /// For other algorithms, uses the GCost of the end node.
    /// </summary>
    public int GetTotalPathCost()
    {
        if (PathfindingResult.Path.Count == 0) return 0;
        
        // Cost-unaware algorithms (uniform movement cost)
        if (SearchMode == SearchMode.BreadthFirstSearch || SearchMode ==  SearchMode.DepthFirstSearch)
        {
            var totalCost = 0;
            
            // Reverse the stack to get correct traversal order
            var pathPoints = PathfindingResult.Path.Reverse();

            foreach (var node in pathPoints)
            {
                if (node.ParentPoint != null)
                {
                    var dx = Math.Abs(node.Point.X - node.ParentPoint.Value.X);
                    var dy = Math.Abs(node.Point.Y - node.ParentPoint.Value.Y);
                    var isDiagonal = dx == 1 && dy == 1;
                    totalCost += isDiagonal && NeighborMode == NeighborMode.EightWay ? 14 : 10;
                }
            }

            return totalCost;
        }
        
        var gridModel = _gridController.Model;
        return PathfindingResult.NodeMap[gridModel.EndPoint].FCost;
    }

    /// <summary>
    /// Instantiates the selected pathfinding algorithm and frontier strategy.
    /// </summary>
    public void SetPathSearch()
    {
        _pathSearch = SearchMode switch
        {
            SearchMode.BreadthFirstSearch => new GraphSearch(new QueueFrontier())
            {
                SearchMode = SearchMode.BreadthFirstSearch,
                HeuristicMode = HeuristicMode.Zero,
                NeighborMode = NeighborMode.FourWay
            },
            SearchMode.DepthFirstSearch => new GraphSearch(new StackFrontier())
            {
                SearchMode = SearchMode.DepthFirstSearch,
                HeuristicMode = HeuristicMode.Zero,
                NeighborMode = NeighborMode.FourWay
            },
            SearchMode.UniformCostSearch => new GraphSearch(new HeapFrontier())
            {
                SearchMode = SearchMode.UniformCostSearch,
                HeuristicMode = HeuristicMode.Zero,
                NeighborMode = NeighborMode.FourWay
            },
            SearchMode.GreedyBestFirstSearch => new GraphSearch(new HeapFrontier())
            {
                SearchMode = SearchMode.GreedyBestFirstSearch,
                HeuristicMode = HeuristicMode,
                NeighborMode = NeighborMode.FourWay
            },
            SearchMode.AStarSearch => new GraphSearch(new HeapFrontier())
            {
                SearchMode = SearchMode.AStarSearch,
                HeuristicMode = HeuristicMode,
                NeighborMode = NeighborMode.FourWay
            },
            SearchMode.GeneralizedAStarSearch => new GraphSearch(new HeapFrontier())
            {
                SearchMode = SearchMode.GeneralizedAStarSearch,
                HeuristicMode = HeuristicMode,
                NeighborMode = NeighborMode.FourWay
            },
            SearchMode.FlowField => new FlowField(new QueueFrontier()),
            _ => throw new NotSupportedException("Search algorithm " + SearchMode + " is not supported.")
        };
    }
}