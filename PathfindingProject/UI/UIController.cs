using System.Text;
using PathfindingProject.Core;
using PathfindingProject.Grid;
using PathfindingProject.Pathfinding;
using PathfindingProject.Pathfinding.Enums;
using PathfindingProject.Rendering;
using PathfindingProject.Scene;
using PathfindingProject.UI.Components;

namespace PathfindingProject.UI;

/// <summary>
/// Manages all UI components and their interactions for controlling pathfinding, grid settings, and step visualizations.
/// </summary>
public class UIController : SceneBehaviour
{
    #region UI Components — Core Controls

    public SliderUI CostSlider;
    public SliderUI ObstacleSlider;
    public SliderUI WeightSlider;
    public SliderUI DepthLimitSlider;
    public SliderUI StepSlider;
    public SliderUI SearchSpeedSlider;

    public IntInputUI GridSizeXInputUI;
    public IntInputUI GridSizeYInputUI;
    public IntInputUI OpenNodeCounter;
    public IntInputUI CloseNodeCounter;
    public IntInputUI PathLengthCounter;
    public IntInputUI PathCostCounter;
    public FloatInputUI ComputationTimeCounter;

    public ButtonUI ClearButton;
    public ButtonUI SearchButton;
    public ButtonUI KeybindInfoPopupButton;

    public ToggleUI StepExplanationToggle;

    public EnumDropdownUI<SearchMode> SearchModeDropdown;
    public EnumDropdownUI<HeuristicMode> HeuristicModeDropdown;
    public EnumDropdownUI<NeighborMode> NeighborModeDropdown;
    public EnumDropdownUI<Gizmos> GizmosDropdown;

    public TextUI GridCellTooltip;
    public TextUI StepExplanationUI;

    public bool ShowGridCellTooltip = true;

    #endregion

    #region Internal references

    private KeybindInfoPopupForm? _popupFormInstance;
    private GridController _gridController;
    private Pathfinder _pathfinder;

    #endregion

    #region Tooltip control

    private float _hoverDelay = 0.5f;
    private float _hoverTimer;

    #endregion

    public override void Awake()
    {
        _pathfinder = SceneRegistry.Resolve<Pathfinder>();
        _gridController = SceneRegistry.Resolve<GridController>();

        _gridController.OnCellHover += ShowCellTooltip;
        _gridController.OnCellHoverOut += HideCellTooltip;
    }

    public override void Start()
    {
        InitializeGridControls();
        InitializeSearchControls();
        InitializeExecutionControls();
        InitializeExplanationControls();
        InitializeTrackingDisplays();

        RegisterAllControls();
    }
    
    public override void Update()
    {
        if (InputManager.GetKeyDown(Keys.Space)) TogglePathSearch(null, null);
    }

    /// <summary>
    /// Updates the search counters on the UI.
    /// </summary>
    /// <param name="openNodes">The current number of open nodes.</param>
    /// <param name="closedNodes">The number of nodes already explored.</param>
    /// <param name="pathLength">The length of the found path. (0 if not found).</param>
    /// <param name="pathCost">The total cost of the found path.</param>
    /// <param name="computationTime">The computation time of the search algorithm.</param>
    public void UpdateTrackers(int openNodes, int closedNodes, int pathLength, int pathCost, float computationTime)
    {
        OpenNodeCounter.Value = openNodes;
        CloseNodeCounter.Value = closedNodes;
        PathLengthCounter.Value = pathLength;
        PathCostCounter.Value = pathCost;
        ComputationTimeCounter.Value = computationTime;
    }

    /// <summary>
    /// Updates the step slider's value and percentage display based on the current search progress.
    /// </summary>
    /// <param name="currentStep">The step currently being visualized.</param>
    /// <param name="stepAmount">The total number of steps in the search.</param>
    public void UpdateStepSlider(int currentStep, int stepAmount)
    {
        StepSlider.Slider.Maximum = stepAmount;
        StepSlider.Slider.Value = currentStep;

        // Show percentage (0–100) visually, based on step index
        StepSlider.ValueLabel.Text = ((int)(100f * currentStep / stepAmount)).ToString();
    }

    /// <summary>
    /// Updates the appearance of the search toggle button based on whether the search is running.
    /// </summary>
    /// <param name="toggle">True if searching, false if idle.</param>
    public void UpdateSearchButton(bool toggle)
    {
        SearchButton.Button.Text = toggle ? "Pause" : "Play";
        SearchButton.Button.BackColor = toggle ? Color.DarkRed : Color.DarkGreen;
    }
    
    /// <summary>
    /// Initializes UI elements related to grid editing and terrain layout.
    /// </summary>
    private void InitializeGridControls()
    {
        CostSlider = new SliderUI("Terrain Cost", 1, 10, 5, 100)
        {
            Location = new Point(10, 10),
            Slider = { TickStyle = TickStyle.BottomRight }
        };

        GridSizeXInputUI = new IntInputUI("Size X", 30, 1)
        {
            Location = new Point(10, 85)
        };

        GridSizeYInputUI = new IntInputUI("Size Y", 15, 1)
        {
            Location = new Point(95, 85)
        };

        ObstacleSlider = new SliderUI("Obstacle Percent", 0, 100, 0, 100, GenerateMap)
        {
            Location = new Point(10, 415)
        };
    }

    /// <summary>
    /// Initializes dropdowns and sliders for algorithm configuration.
    /// </summary>
    private void InitializeSearchControls()
    {
        SearchModeDropdown = new EnumDropdownUI<SearchMode>("Search Mode", SearchMode.AStarSearch, selectionChanged: ChangeSearchMode)
        {
            Location = new Point(10, 155)
        };

        HeuristicModeDropdown = new EnumDropdownUI<HeuristicMode>("Heuristic Mode", HeuristicMode.Manhattan, selectionChanged: ChangeHeuristicMode)
        {
            Location = new Point(10, 220)
        };

        NeighborModeDropdown = new EnumDropdownUI<NeighborMode>("Neighbor Mode", NeighborMode.FourWay, selectionChanged: ChangeNeighborMode)
        {
            Location = new Point(10, 285)
        };

        GizmosDropdown = new EnumDropdownUI<Gizmos>("Gizmos", Gizmos.Costs)
        {
            Location = new Point(10, 350)
        };
        
        WeightSlider = new SliderUI("Weight", 1, 10, 1, 100, UpdateWeight)
        {
            Location = new Point(10, 490)
        };
        
        DepthLimitSlider = new SliderUI("Depth Limit", 0, 100, 0, 100, UpdateDepthLimit)
        {
            Location = new Point(10, 565)
        };

        SearchSpeedSlider = new SliderUI("Search Speed", 1, 100, 50, 100, ChangeSearchSpeed)
        {
            Location = new Point(95, 900)
        };
    }

    /// <summary>
    /// Sets up the buttons for starting/stopping and clearing the search state.
    /// </summary>
    private void InitializeExecutionControls()
    {
        SearchButton = new ButtonUI("Start", TogglePathSearch, 60)
        {
            Location = new Point(10, 895)
        };

        ClearButton = new ButtonUI("Clear", ClearNodeMap, 60)
        {
            Location = new Point(10, 940)
        };
        
        KeybindInfoPopupButton = new ButtonUI("Show Keybinds", OpenPopup, 60, backColor: Color.RoyalBlue)
        {
            Location = new Point(10, 685)
        };

    }

    /// <summary>
    /// Initializes UI controls for toggling the step explanation panel.
    /// </summary>
    private void InitializeExplanationControls()
    {
        StepExplanationToggle = new ToggleUI("Show Explanations", false, toggled: ToggleExplanationPanel)
        {
            Location = new Point(10, 640)
        };
    }

    /// <summary>
    /// Initializes pathfinding diagnostics.
    /// </summary>
    private void InitializeTrackingDisplays()
    {
        StepSlider = new SliderUI("% Path Steps", 0, 100, 0, 500, MovePathStep)
        {
            Location = new Point(650, 900)
        };

        OpenNodeCounter = new IntInputUI("Open Nodes")
        {
            Location = new Point(1750, 10),
            Enabled = false
        };

        CloseNodeCounter = new IntInputUI("Closed Nodes")
        {
            Location = new Point(1750, 80),
            Enabled = false
        };
        
        PathLengthCounter = new IntInputUI("Path Length")
        {
            Location = new Point(1750, 150),
            Enabled = false
        };
        
        PathCostCounter = new IntInputUI("Path Cost")
        {
            Location = new Point(1750, 220),
            Enabled = false
        };
        
        ComputationTimeCounter = new FloatInputUI("Computation Time")
        {
            Location = new Point(1750, 290),
            Enabled = false
        };

        GridCellTooltip = new TextUI
        {
            BackColor = Color.FromArgb(24, 24, 24),
            Location = new Point(-100, -100),
            Enabled = false
        };

        StepExplanationUI = new TextUI("", 7f, multiline: false, scrollable: false, width: 500, height: 30)
        {
            BackColor = Color.FromArgb(24, 24, 24),
            Location = new Point(700, 10),
            Enabled = false,
            Visible = false
        };
    }
    /// <summary>
    /// Registers all initialized UI controls into the main application form.
    /// </summary>
    private void RegisterAllControls()
    {
        Form.Controls.Add(CostSlider);
        Form.Controls.Add(GridSizeXInputUI);
        Form.Controls.Add(GridSizeYInputUI);
        Form.Controls.Add(SearchModeDropdown);
        Form.Controls.Add(HeuristicModeDropdown);
        Form.Controls.Add(NeighborModeDropdown);
        Form.Controls.Add(GizmosDropdown);
        Form.Controls.Add(ObstacleSlider);
        Form.Controls.Add(WeightSlider);
        Form.Controls.Add(DepthLimitSlider);
        Form.Controls.Add(StepExplanationToggle);
        Form.Controls.Add(ClearButton);
        Form.Controls.Add(SearchButton);
        Form.Controls.Add(KeybindInfoPopupButton);
        Form.Controls.Add(SearchSpeedSlider);
        Form.Controls.Add(StepSlider);
        Form.Controls.Add(OpenNodeCounter);
        Form.Controls.Add(CloseNodeCounter);
        Form.Controls.Add(PathLengthCounter);
        Form.Controls.Add(PathCostCounter);
        Form.Controls.Add(ComputationTimeCounter);
        Form.Controls.Add(GridCellTooltip);
        Form.Controls.Add(StepExplanationUI);
    }

    /// <summary>
    /// Called when the obstacle slider value changes. Triggers obstacle map generation.
    /// </summary>
    public void GenerateMap(object? sender, EventArgs e)
    {
        _gridController.GenerateMap(ObstacleSlider.Value);
    }
    
    /// <summary>
    /// Updates the heuristic weight used by the pathfinding algorithm based on the current slider value,
    /// and refreshes the node map to reflect the new weighting in the path calculations.
    /// </summary>
    public void UpdateWeight(object? sender, EventArgs e)
    {
        _pathfinder.HeuristicWeight = WeightSlider.Value;
        _pathfinder.UpdateNodeMap();
    }
    
    /// <summary>
    /// Updates the pathfinder's depth limit based on the current slider value and refreshes the node map.
    /// </summary>
    public void UpdateDepthLimit(object? sender, EventArgs e)
    {
        _pathfinder.DepthLimit = DepthLimitSlider.Value;
        _pathfinder.UpdateNodeMap();
    }

    /// <summary>
    /// Clears the current node map while keeping the start and end points intact and resets current step back to 0
    /// </summary>
    private void ClearNodeMap(object? sender, EventArgs e)
    {
        _pathfinder.ClearNodeMapExceptEndpoints();
        _pathfinder.MovePathStep(0);
    }
    
    /// <summary>
    /// Opens the popup window if it's not already open. If it's open but not in focus, brings it to the front.
    /// Ensures only one instance of the popup is shown at a time.
    /// </summary>
    private void OpenPopup(object? sender, EventArgs e)
    {
        if (_popupFormInstance == null || _popupFormInstance.IsDisposed)
        {
            _popupFormInstance = new KeybindInfoPopupForm();
            _popupFormInstance.Show(Form);
        }
        else
        {
            _popupFormInstance.BringToFront();
            _popupFormInstance.Focus();
        }
    }
    
    /// <summary>
    /// Toggles automatic path search progression and updates the button state.
    /// </summary>
    private void TogglePathSearch(object? sender, EventArgs e)
    {
        var isSearching = _pathfinder.ToggleSearch();
        UpdateSearchButton(isSearching);
    }

    /// <summary>
    /// Updates the search speed value when the search speed slider is adjusted.
    /// </summary>
    private void ChangeSearchSpeed(object? sender, EventArgs e)
    {
        _pathfinder.SearchSpeed = SearchSpeedSlider.Value;
    }

    /// <summary>
    /// Advances the search step based on the step slider's percentage value.
    /// </summary>
    private void MovePathStep(object? sender, EventArgs e)
    {
        _pathfinder.MovePathStep(StepSlider.Value);
    }
    
    /// <summary>
    /// Handles changes to the selected search mode. 
    /// Adjusts heuristic/gizmo availability and updates the node map.
    /// </summary>
    private void ChangeSearchMode(object? sender, EventArgs e)
    {
        _pathfinder.SearchMode = SearchModeDropdown.SelectedValue;
        _pathfinder.SetPathSearch();

        switch (_pathfinder.SearchMode)
        {
            case SearchMode.UniformCostSearch:
                _pathfinder.HeuristicMode = HeuristicMode.Zero;
                HeuristicModeDropdown.ComboBox.SelectedItem = HeuristicMode.Zero;
                GizmosDropdown.ComboBox.SelectedItem = Gizmos.Arrows;
                HeuristicModeDropdown.Enabled = false;
                GizmosDropdown.Enabled = true;
                break;

            case SearchMode.BreadthFirstSearch:
            case SearchMode.DepthFirstSearch:
                _pathfinder.HeuristicMode = HeuristicMode.None;
                HeuristicModeDropdown.ComboBox.SelectedItem = HeuristicMode.None;
                GizmosDropdown.ComboBox.SelectedItem = Gizmos.Arrows;
                HeuristicModeDropdown.Enabled = false;
                GizmosDropdown.Enabled = true;
                break;

            case SearchMode.AStarSearch:
            case SearchMode.GeneralizedAStarSearch:
            case SearchMode.GreedyBestFirstSearch:
                _pathfinder.HeuristicMode = HeuristicMode.Manhattan;
                HeuristicModeDropdown.ComboBox.SelectedItem = HeuristicMode.Manhattan;
                GizmosDropdown.ComboBox.SelectedItem = Gizmos.Costs;
                HeuristicModeDropdown.Enabled = true;
                GizmosDropdown.Enabled = true;
                break;

            case SearchMode.FlowField:
                _pathfinder.HeuristicMode = HeuristicMode.None;
                HeuristicModeDropdown.ComboBox.SelectedItem = HeuristicMode.None;
                GizmosDropdown.ComboBox.SelectedItem = Gizmos.Arrows;
                HeuristicModeDropdown.Enabled = false;
                GizmosDropdown.Enabled = false;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        _pathfinder.UpdateNodeMap();
    }

    /// <summary>
    /// Applies a new heuristic mode when selected and updates the node map.
    /// </summary>
    private void ChangeHeuristicMode(object? sender, EventArgs e)
    {
        // Prevent invalid selection (None not allowed in some modes)
        if (HeuristicModeDropdown.ComboBox.SelectedItem?.ToString() == "None")
        {
            HeuristicModeDropdown.ComboBox.SelectedIndex = 0;
        }

        _pathfinder.HeuristicMode = HeuristicModeDropdown.SelectedValue;
        _pathfinder.UpdateNodeMap();
    }

    /// <summary>
    /// Updates the neighbor expansion mode (4-way or 8-way).
    /// </summary>
    private void ChangeNeighborMode(object? sender, EventArgs e)
    {
        _pathfinder.NeighborMode = NeighborModeDropdown.SelectedValue;
        _pathfinder.UpdateNodeMap();
    }

    /// <summary>
    /// Toggles the visibility of the step explanation panel in the UI.
    /// </summary>
    private void ToggleExplanationPanel(object? sender, EventArgs e)
    {
        StepExplanationUI.Visible = !StepExplanationUI.Visible;
    }
    
    /// <summary>
    /// Shows a cell tooltip with node data on hover.
    /// </summary>
    private void ShowCellTooltip(Node node)
    {
        if (!ShowGridCellTooltip)
            return;

        _hoverTimer += Time.DeltaTime;

        if (_hoverTimer > _hoverDelay)
        {
            var sb = new StringBuilder();
            sb.Append(node.Point.ToString());

            if (GizmosDropdown.SelectedValue != Gizmos.Costs)
            {
                sb.AppendLine();
                sb.Append($"F: {node.FCost}  G: {node.GCost}  H: {node.HCost}");
            }

            if (node.CellCost > 0)
            {
                sb.AppendLine();
                sb.Append("Cost: " + node.CellCost);
            }

            GridCellTooltip.TextContent = sb.ToString();

            var model = _gridController.Model;
            var pos = Extensions.GridToScreen(
                node.Point, 
                model.GridSize, 
                _gridController.CellSize, 
                Form.ClientSize
            ) + new Size(0, _gridController.CellSize.Y);

            GridCellTooltip.Location = pos;
            _hoverTimer = 0f;
        }
    }

    /// <summary>
    /// Hides the cell tooltip and resets the hover timer.
    /// </summary>
    private void HideCellTooltip()
    {
        GridCellTooltip.Location = new Point(-100, -100);
        _hoverTimer = 0f;
    }
}