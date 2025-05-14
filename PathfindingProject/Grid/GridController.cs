using PathfindingProject.Core;
using PathfindingProject.Pathfinding;
using PathfindingProject.Pathfinding.Algorithms;
using PathfindingProject.Pathfinding.Enums;
using PathfindingProject.Rendering;
using PathfindingProject.Scene;
using PathfindingProject.UI;

namespace PathfindingProject.Grid;

/// <summary>
/// Controls grid interaction, rendering, input handling, and communication between UI, pathfinding, and visualization layers.
/// </summary>
public class GridController : SceneBehaviour
{

    #region Public Fields & Events

    public readonly GridModel Model = new();

    public event Action<Node>? OnCellHover;
    public event Action? OnCellHoverOut;
    public event Action? OnGridUpdate;

    #endregion

    #region Dependencies

    private UIController _uiController = null!;
    private Pathfinder _pathfinder = null!;
    private FloodFill _floodFill = null!;

    #endregion

    #region Grid & Viewport State

    private Point _mousePos;
    private Point _lastMousePos;
    private Point _hoveredCell;

    private Point _baseCellSize = new(50, 50);
    private Point _cellSize = new(50, 50);
    private Size _panOffset;

    private float _zoomFactor = 1.0f;
    private const float _zoomStep = 0.1f;
    private const float _minScale = 0.1f;
    private const float _maxScale = 5.0f;
    private const float _scrollSpeed = 0.1f;

    private bool _isHovering;
    private bool _isDraggingView;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the clamped cell size (min 10x10).
    /// </summary>
    public Point CellSize
    {
        get => _cellSize;
        set
        {
            var clampedX = Math.Max(10, value.X);
            var clampedY = Math.Max(10, value.Y);
            _cellSize = new Point(clampedX, clampedY);
        }
    }

    #endregion

    #region SceneBehaviour Methods

    public override void Awake()
    {
        // Resolve shared components from the scene
        _uiController = SceneRegistry.Resolve<UIController>();
        _pathfinder = SceneRegistry.Resolve<Pathfinder>();
        _floodFill = new FloodFill();
    }

    public override void Start()
    {
        // Reactively update zoomed cell size on window resize
        Form.Resize += (_, _) => UpdateCellSize();
    }

    public override void Update()
    {
        #region Grid Size Sync from UI

        if (Model.GridSize.X != _uiController.GridSizeXInputUI.Value ||
            Model.GridSize.Y != _uiController.GridSizeYInputUI.Value)
        {
            Model.GridSize.X = _uiController.GridSizeXInputUI.Value;
            Model.GridSize.Y = _uiController.GridSizeYInputUI.Value;

            FixNodeMap(); // Clamp start/end + remove out-of-bounds overrides
            OnGridUpdate?.Invoke();
            GenerateMap(_uiController.ObstacleSlider.Value); // Re-generate walls
        }

        #endregion

        #region Hovering & Mouse Position Detection

        _mousePos = InputManager.MousePosition;
        var adjustedMouse = _mousePos - _panOffset;
        var gridPos = Extensions.ScreenToGrid(adjustedMouse, Model.GridSize, CellSize, Form.ClientSize);

        if (gridPos == _hoveredCell)
        {
            var exists = _pathfinder.PathfindingResult.NodeMap.TryGetValue(gridPos, out var hoveredNode);
            if (!exists) hoveredNode.Point = gridPos;
            OnCellHover?.Invoke(hoveredNode);
            _isHovering = true;
        }
        else if (_isHovering)
        {
            OnCellHoverOut?.Invoke();
            _isHovering = false;
        }

        // Snap to valid grid cell for drawing
        _hoveredCell = Model.SnapToNearestValidCell(gridPos);

        #endregion

        #region Drag-to-Pan with Ctrl + Left Mouse

        if (InputManager.GetKeyDown(Keys.K))
        {
            foreach (var control in Form.Controls)
            {
                if (control is Control uiElement)
                {
                    uiElement.Visible = !uiElement.Visible;
                    // or if you want to actually block interaction:
                    // uiElement.Enabled = enabled;
                }
            }
        }
        
        if (InputManager.GetKey(Keys.ControlKey) && InputManager.GetMouseButton(MouseButtons.Left))
        {
            if (!_isDraggingView)
            {
                _lastMousePos = _mousePos;
                _isDraggingView = true;
            }

            var delta = new Point(_mousePos.X - _lastMousePos.X, _mousePos.Y - _lastMousePos.Y);
            _panOffset += new Size(delta);
            _lastMousePos = _mousePos;

            ClampPanOffset();
        }
        else
        {
            _isDraggingView = false;
        }

        #endregion

        #region Zoom In/Out with Scroll Wheel

        if (InputManager.ScrollDelta > 0)
        {
            _zoomFactor = Math.Min(_zoomFactor + _zoomStep, _maxScale);
            UpdateCellSize();
        }
        else if (InputManager.ScrollDelta < 0)
        {
            _zoomFactor = Math.Max(_zoomFactor - _zoomStep, _minScale);
            UpdateCellSize();
        }

        #endregion

        #region Placement: Left/Right Click + Shift Key

        // Left click: Toggle start point or clear override
        if (IsMouseOverGrid() && InputManager.GetMouseButton(MouseButtons.Left, true) && _hoveredCell != Model.StartPoint)
        {
            if (Model.GridOverrides.ContainsKey(_hoveredCell)) Model.GridOverrides.Remove(_hoveredCell);
            else Model.StartPoint = _hoveredCell;

            OnGridUpdate?.Invoke();
        }
        
        // Shift + Left click: Toggle end point
        if (IsMouseOverGrid() && InputManager.GetKey(Keys.ShiftKey) && InputManager.GetMouseButton(MouseButtons.Left) && _hoveredCell != Model.EndPoint)
        {
            if (Model.GridOverrides.ContainsKey(_hoveredCell))
                Model.GridOverrides.Remove(_hoveredCell);
            else
                Model.EndPoint = _hoveredCell;

            OnGridUpdate?.Invoke();
        }

        // Right click: Set obstacle
        if (IsMouseOverGrid() && InputManager.GetMouseButton(MouseButtons.Right, true))
        {
            Model.SetCellObstacle(_hoveredCell);
            OnGridUpdate?.Invoke();
        }

        // Shift + Right click: Set traversal cost
        if (IsMouseOverGrid() && InputManager.GetKey(Keys.ShiftKey) && InputManager.GetMouseButton(MouseButtons.Right))
        {
            Model.SetCellCost(_hoveredCell, _uiController.CostSlider.Value);
            OnGridUpdate?.Invoke();
        }

        #endregion
    }

    #endregion

    public override void Draw(Graphics g)
    {
        #region Draw grid visuals and core data overlays

        DrawGridFrame(g); // Outer border
        DrawBaseGrid(g); // Base cell tiles
        DrawGridOverrides(g); // Obstacles and costs
        DrawNodeMap(g, _pathfinder.PathfindingResult.NodeMap); // Open/closed states
        DrawPath(g, _pathfinder.PathfindingResult.Path); // Highlighted path
        DrawStartEnd(g); // Start/end points
        if (IsMouseOverGrid()) DrawGridHighlighter(g); // Mouse hover cell highlight

        #endregion

        #region Debug Gizmos (selected from dropdown)

        switch (_uiController.GizmosDropdown.SelectedValue)
        {
            case Gizmos.Costs:
                DrawNodeCosts(g, _pathfinder.PathfindingResult.NodeMap);
                break;
            case Gizmos.Arrows:
                DrawArrows(g, _pathfinder.PathfindingResult.NodeMap);
                break;
            case Gizmos.Positions:
                DrawNodePositions(g, _pathfinder.PathfindingResult.NodeMap);
                break;
            case Gizmos.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        #endregion
    }

    private void DrawGridFrame(Graphics g)
    {
        // Draws the outer border around the grid area
        var penThickness = (int)(10 * _zoomFactor);

        var gridSizePixels = new Size(
            Model.GridSize.X * CellSize.X,
            Model.GridSize.Y * CellSize.Y
        );

        var topLeft = (Form.ClientSize - gridSizePixels) / 2 + _panOffset;
        var adjustedTopLeft = new Point(
            topLeft.Width - penThickness / 2,
            topLeft.Height - penThickness / 2
        );

        var adjustedSize = new Size(
            gridSizePixels.Width + penThickness,
            gridSizePixels.Height + penThickness
        );

        var rect = new Rectangle(adjustedTopLeft, adjustedSize - new Size(2, 2));
        var pen = PenPool.Get(Color.DarkSlateGray, penThickness);
        g.DrawRectangle(pen, rect);
    }

    // Draws default background for each cell
    private void DrawBaseGrid(Graphics g)
    {
        for (int y = 0; y < Model.GridSize.Y; y++)
        {
            for (int x = 0; x < Model.GridSize.X; x++)
            {
                var cell = new Point(x, y);
                var topLeft = Extensions.GridToScreen(cell, Model.GridSize, CellSize, Form.ClientSize) + _panOffset;
                var brush = BrushPool.Get(Color.LightGray);
                g.FillRectangle(brush, topLeft.X, topLeft.Y, CellSize.X - 2, CellSize.Y - 2);
            }
        }
    }

    // Draws walls (black) and weighted cells (scaled brown)
    private void DrawGridOverrides(Graphics g)
    {
        foreach (var (point, node) in Model.GridOverrides)
        {
            var screenPos = Extensions.GridToScreen(point, Model.GridSize, CellSize, Form.ClientSize) + _panOffset;
            var normalizedCost = Math.Clamp(node.CellCost / 10f, 0f, 1f);
            var scale = 0.2f + (1f - normalizedCost) * 0.8f;

            var brush = !node.Walkable
                ? BrushPool.Get(Color.Black)
                : BrushPool.Get(Color.SaddleBrown.ScaleColor(scale));

            g.FillRectangle(brush, screenPos.X, screenPos.Y, CellSize.X - 2, CellSize.Y - 2);
            
            if (!node.Walkable) continue;
            var w = CellSize.X;
            var h = CellSize.Y;
            var costFont = FontPool.Get("Segoe UI", MathF.Min(w, h) * 0.36f, FontStyle.Bold);
            var costText = node.CellCost.ToString();
            var costSize = TextRenderer.MeasureText(costText, costFont);

            g.DrawString(costText, costFont, BrushPool.Get(Color.White),
                screenPos.X + (w / 2f) - (costSize.Width / 2f),
                screenPos.Y + (h / 2f) - (costSize.Height / 2f));
        }
    }

    // Highlights cell currently under mouse
    private void DrawGridHighlighter(Graphics g)
    {
        var screenPos = Extensions.GridToScreen(_hoveredCell, Model.GridSize, CellSize, Form.ClientSize) + _panOffset;
        var brush = BrushPool.Get(Color.FromArgb(128, Color.DarkGray));
        g.FillRectangle(brush, screenPos.X, screenPos.Y, CellSize.X - 2, CellSize.Y - 2);
    }

    // Draws start and end points
    private void DrawStartEnd(Graphics g)
    {
        var startScreen = Extensions.GridToScreen(Model.StartPoint, Model.GridSize, CellSize, Form.ClientSize) +
                          _panOffset;
        var endScreen = Extensions.GridToScreen(Model.EndPoint, Model.GridSize, CellSize, Form.ClientSize) + _panOffset;

        var brush = BrushPool.Get(Color.CornflowerBlue);
        g.FillRectangle(brush, startScreen.X, startScreen.Y, CellSize.X - 2, CellSize.Y - 2);
        g.FillRectangle(brush, endScreen.X, endScreen.Y, CellSize.X - 2, CellSize.Y - 2);
    }

    // Draws the current found path if one exists
    private void DrawPath(Graphics g, Stack<Node>? path)
    {
        if (path == null) return;

        foreach (var node in path)
        {
            var screenPos = Extensions.GridToScreen(node.Point, Model.GridSize, CellSize, Form.ClientSize) + _panOffset;
            var brush = BrushPool.Get(Color.LightBlue);
            g.FillRectangle(brush, screenPos.X, screenPos.Y, CellSize.X - 2, CellSize.Y - 2);
        }
    }

    // Colors the grid based on pathfinding state (open/closed)
    private void DrawNodeMap(Graphics g, Dictionary<Point, Node>? nodeMap)
    {
        if (nodeMap == null || nodeMap.Count == 0) return;

        var minCost = _pathfinder.SearchMode != SearchMode.FlowField ? nodeMap[Model.StartPoint].FCost : 0;
        var maxCost = 0;

        // Gets max cost set if search has cost included
        if (_pathfinder.SearchMode != SearchMode.BreadthFirstSearch && _pathfinder.SearchMode != SearchMode.DepthFirstSearch)
        {
            foreach (var (_, node) in nodeMap)
            {
                if (maxCost < node.FCost) maxCost = node.FCost;
            }
        }

        foreach (var (_, node) in nodeMap)
        {
            if (!node.Walkable || node.State == Node.NodeState.Unvisited) continue;

            var screenPos = Extensions.GridToScreen(node.Point, Model.GridSize, CellSize, Form.ClientSize) + _panOffset;
            var factor = maxCost > 0 && maxCost != minCost ? (float)(node.FCost - minCost) / (maxCost - minCost) : 1; // Used for color shading across costs
            var minBrightness = maxCost > 0 && maxCost != minCost ? 0.5f : 0.8f;
            
            var brush = node.State switch
            {
                Node.NodeState.Open => BrushPool.Get(Color.ForestGreen.ScaleColor(factor, minBrightness, true)),
                Node.NodeState.Closed => BrushPool.Get(Color.Red.ScaleColor(factor, minBrightness, true)),
                _ => BrushPool.Get(Color.DarkGray)
            };

            g.FillRectangle(brush, screenPos.X, screenPos.Y, CellSize.X - 2, CellSize.Y - 2);

            // Mini overlay cost marker
            if (node.CellCost > 0)
            {
                var normalizedCost = Math.Clamp(node.CellCost / 10f, 0f, 1f);
                var scale = 0.2f + (1f - normalizedCost) * 0.8f;
                var brownBrush = BrushPool.Get(Color.SaddleBrown.ScaleColor(scale));
                var size = new Size(CellSize.X, CellSize.Y) / 4;
                g.FillRectangle(brownBrush,
                    screenPos.X + CellSize.X - size.Width - 2,
                    screenPos.Y + CellSize.Y - size.Height - 2,
                    size.Width, size.Height);
                
                var w = CellSize.X;
                var h = CellSize.Y;
                var costFont = FontPool.Get("Segoe UI", MathF.Min(w, h) * 0.12f, FontStyle.Bold);
                var costText = node.CellCost.ToString();
                var costSize = TextRenderer.MeasureText(costText, costFont);

                g.DrawString(costText, costFont, BrushPool.Get(Color.White),
                    screenPos.X + CellSize.X - size.Width - 2,
                    screenPos.Y + CellSize.Y - size.Height - 2);
            }
        }
    }

    // Draws the cost values based on pathfinding state
    private void DrawNodeCosts(Graphics g, Dictionary<Point, Node>? nodeMap)
    {
        if (nodeMap == null || nodeMap.Count == 0) return;

        var w = CellSize.X;
        var h = CellSize.Y;

        // Choose font sizes based on cell dimensions
        var gFont = FontPool.Get("Segoe UI", MathF.Min(w, h) * 0.15f, FontStyle.Bold);
        var hFont = FontPool.Get("Segoe UI", MathF.Min(w, h) * 0.15f, FontStyle.Bold);
        var fFont = FontPool.Get("Segoe UI", MathF.Min(w, h) * 0.36f, FontStyle.Bold);
        var brush = BrushPool.Get(Color.Black);

        foreach (var (point, node) in nodeMap)
        {
            if (!node.Walkable || node.State == Node.NodeState.Unvisited) continue;
            var screenPos = Extensions.GridToScreen(point, Model.GridSize, CellSize, Form.ClientSize) + _panOffset;

            var gText = node.GCost.ToString();
            var hText = node.HCost.ToString();
            var fText = node.FCost.ToString();

            var hSize = TextRenderer.MeasureText(hText, hFont);
            var fSize = TextRenderer.MeasureText(fText, fFont);

            g.DrawString(gText, gFont, brush, screenPos.X + 2, screenPos.Y + 2);
            g.DrawString(hText, hFont, brush, screenPos.X + w - hSize.Width - 2, screenPos.Y + 2);
            g.DrawString(fText, fFont, brush,
                screenPos.X + (w / 2f) - (fSize.Width / 2f),
                screenPos.Y + (h / 2f) - (fSize.Height / 3f));
        }
    }

    // Draws arrows based on node parent direction
    private void DrawArrows(Graphics g, Dictionary<Point, Node>? nodeMap)
    {
        if (nodeMap == null || nodeMap.Count == 0) return;

        var w = CellSize.X;
        var h = CellSize.Y;
        var halfLen = MathF.Min(w, h) * 0.2f;

        var fFont = FontPool.Get("Segoe UI", MathF.Min(w, h) * 0.25f, FontStyle.Bold);
        var brush = BrushPool.Get(Color.Black);

        foreach (var (gridPos, node) in nodeMap)
        {
            if (!node.Walkable || node.State == Node.NodeState.Unvisited) continue;

            var screenPos = Extensions.GridToScreen(gridPos, Model.GridSize, CellSize, Form.ClientSize) + _panOffset;
            var center = new PointF(screenPos.X + w / 2f, screenPos.Y + h / 2f);

            // Optional F-cost number display
            if (node.FCost > 0)
            {
                var fText = node.FCost.ToString();
                var fSize = TextRenderer.MeasureText(fText, fFont);
                var textPos = new PointF(center.X - fSize.Width / 2f,
                    node.ParentPoint != null ? screenPos.Y + 2 : center.Y - fSize.Height / 2f);
                g.DrawString(fText, fFont, brush, textPos);
            }

            if (node.ParentPoint == null) continue;

            // Draw arrow from this node to its parent
            var parentScreen =
                Extensions.GridToScreen(node.ParentPoint.Value, Model.GridSize, CellSize, Form.ClientSize) + _panOffset;
            var parentCenter = new PointF(parentScreen.X + w / 2f, parentScreen.Y + h / 2f);

            var dx = parentCenter.X - center.X;
            var dy = parentCenter.Y - center.Y;
            var len = MathF.Sqrt(dx * dx + dy * dy);
            if (len == 0) continue;

            dx /= len;
            dy /= len;

            var offset = node.FCost > 0 ? new Size(0, 5) : new Size();
            var arrowStart = new PointF(center.X - dx * halfLen, center.Y - dy * halfLen) + offset;
            var arrowEnd = new PointF(center.X + dx * halfLen, center.Y + dy * halfLen) + offset;

            Extensions.DrawArrowFromPosition(g, arrowStart, new Size(1, 1), arrowEnd, Color.Orange, 2f);
        }
    }

    // Draws the grid index based on node position
    private void DrawNodePositions(Graphics g, Dictionary<Point, Node>? nodeMap)
    {
        if (nodeMap == null || nodeMap.Count == 0) return;

        var w = CellSize.X;
        var h = CellSize.Y;
        var font = FontPool.Get("Segoe UI", MathF.Min(w, h) * 0.2f, FontStyle.Bold);
        var brush = BrushPool.Get(Color.Black);

        foreach (var (point, node) in nodeMap)
        {
            if (!node.Walkable || node.State == Node.NodeState.Unvisited) continue;

            var screenPos = Extensions.GridToScreen(point, Model.GridSize, CellSize, Form.ClientSize) + _panOffset;
            var text = $"{point.X},{point.Y}";
            var textSize = TextRenderer.MeasureText(text, font);

            g.DrawString(text, font, brush,
                screenPos.X + (w / 2f) - (textSize.Width / 2f),
                screenPos.Y + (h / 2f) - (textSize.Height / 2f));
        }
    }

    
    /// <summary>
    /// Generates a random obstacle map based on flood fill, while preserving connectivity of empty cells.
    /// </summary>
    public void GenerateMap(int obstaclePercent)
    {
        Model.GridOverrides.Clear();

        _floodFill.Initialize(Model.GridSize);

        // Clamp to 0–100 to ensure safety during division
        var clampedPercent = Math.Clamp(obstaclePercent, 0, 100);
        var walls = _floodFill.DefineObstacles(clampedPercent / 100f);

        foreach (var point in walls)
        {
            var node = new Node(point) { Walkable = false };
            Model.GridOverrides.TryAdd(point, node);
        }

        _pathfinder.UpdateNodeMap();
    }

    
    // Recalculates cell size based on form resolution and zoom scale. Maintains 16:9 aspect ratio.
    private void UpdateCellSize()
    {
        const float targetAspect = 16f / 9f;
        var currentAspect = (float)Form.ClientSize.Width / Form.ClientSize.Height;

        float aspectFactor = currentAspect > targetAspect
            ? Form.ClientSize.Height / 1080f
            : Form.ClientSize.Width / 1920f;

        var totalScale = aspectFactor * _zoomFactor;

        CellSize = new Point(
            (int)(_baseCellSize.X * totalScale),
            (int)(_baseCellSize.Y * totalScale)
        );
    }

    // Prevents panning beyond the edge of the grid visualization.
    private void ClampPanOffset()
    {
        var halfGridSizePx = new Size(
            Model.GridSize.X * CellSize.X,
            Model.GridSize.Y * CellSize.Y
        ) / 2;

        _panOffset.Width = Math.Clamp(_panOffset.Width, -halfGridSizePx.Width, halfGridSizePx.Width);
        _panOffset.Height = Math.Clamp(_panOffset.Height, -halfGridSizePx.Height, halfGridSizePx.Height);
    }

    
    // Clamps start/end points and override entries to remain within grid bounds.
    private void FixNodeMap()
    {
        Model.StartPoint = ClampToGrid(Model.StartPoint, Model.GridSize);
        Model.EndPoint = ClampToGrid(Model.EndPoint, Model.GridSize);

        var removalList = new List<Point>();
        foreach (var (point, _) in Model.GridOverrides)
        {
            if (!point.IsInBounds(Model.GridSize))
                removalList.Add(point);
        }

        foreach (var point in removalList)
        {
            Model.GridOverrides.Remove(point);
        }
    }

    // Clamps a point to the nearest valid grid cell.
    private Point ClampToGrid(Point point, Point gridSize)
    {
        return new Point(
            Math.Clamp(point.X, 0, gridSize.X - 1),
            Math.Clamp(point.Y, 0, gridSize.Y - 1)
        );
    }
    
    // Determines if the current mouse position is over a valid grid cell.
    private bool IsMouseOverGrid()
    {
        var mouse = InputManager.MousePosition;
        var adjustedMouse = mouse - _panOffset;
        var gridPos = Extensions.ScreenToGrid(adjustedMouse, Model.GridSize, CellSize, Form.ClientSize);
        return gridPos.IsInBounds(Model.GridSize);
    }
}