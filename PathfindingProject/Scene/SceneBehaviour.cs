namespace PathfindingProject.Scene;

/// <summary>
/// Base class for all components that runs in the custom scene system.
/// Auto-registers itself to the global scene manager.
/// </summary>
public abstract class SceneBehaviour
{
    /// <summary>
    /// Shortcut to the main application form.
    /// </summary>
    protected Form Form => Scene.Form;

    protected SceneBehaviour()
    {
        Scene.Behaviours.Add(this);
    }

    /// <summary>
    /// Called once before Start(). Used for initial setup.
    /// </summary>
    public virtual void Awake() { }

    /// <summary>
    /// Called once after Awake(). Use for initialization that depends on other objects.
    /// </summary>
    public virtual void Start() { }

    /// <summary>
    /// Called every frame. Handles logic and updates.
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// Called every frame to render visuals.
    /// </summary>
    public virtual void Draw(Graphics g) { }
}