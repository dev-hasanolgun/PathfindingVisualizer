using PathfindingProject.Core;
using PathfindingProject.Scene;
using Timer = System.Windows.Forms.Timer;

namespace PathfindingProject;

/// <summary>
/// The main application form that handles game loop timing, input events, and delegates scene lifecycle updates.
/// </summary>
public partial class MainForm : Form
{
    private readonly Timer _gameTimer = new();

    public MainForm()
    {
        InitializeComponent();

        // Window & rendering config
        WindowState = FormWindowState.Maximized;
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.Gray;

        // Initialize scene components
        try
        {
            ScriptBootstrapper.InitializeSceneBehaviours();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error during bootstrap: " + ex.Message);
        }

        Scene.Scene.AwakeAll();

        // Frame timer setup (~60 FPS)
        _gameTimer.Interval = 16;
        _gameTimer.Tick += GameLoop;
        _gameTimer.Start();

        // Input event binding
        BindInputEvents();

        // Clear UI focus when clicking empty space
        InputManager.RequestFocusClear += () =>
        {
            var mouse = PointToClient(Cursor.Position);
            if (!IsMouseOverAnyControl(mouse))
                ActiveControl = null;
        };
    }

    /// <summary>
    /// Main game loop tick. Calls scene update and triggers repaint.
    /// </summary>
    private void GameLoop(object? sender, EventArgs e)
    {
        Scene.Scene.UpdateAll();
        Invalidate();
    }

    /// <summary>
    /// Called after the form is first shown. Triggers StartAll for scene objects.
    /// </summary>
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Scene.Scene.StartAll();
    }

    /// <summary>
    /// Called on each frame repaint. Triggers DrawAll with the current Graphics object.
    /// </summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        Scene.Scene.DrawAll(e.Graphics);
    }

    /// <summary>
    /// Returns true if the mouse is currently hovering any visible control.
    /// Used to decide whether focus should be cleared.
    /// </summary>
    private bool IsMouseOverAnyControl(Point mouse)
    {
        foreach (Control ctrl in Controls)
        {
            if (ctrl.Visible && ctrl.Bounds.Contains(mouse))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Wires up all WinForms input events to the InputManager.
    /// </summary>
    private void BindInputEvents()
    {
        MouseDown += (_, e) => InputManager.HandleMouseDown(e);
        MouseUp += (_, e) => InputManager.HandleMouseUp(e);
        MouseMove += (_, e) => InputManager.HandleMouseMove(e);
        MouseWheel += (_, e) => InputManager.HandleMouseScroll(e);

        KeyDown += (_, e) => InputManager.HandleKeyDown(e);
        KeyUp += (_, e) => InputManager.HandleKeyUp(e);
    }
}