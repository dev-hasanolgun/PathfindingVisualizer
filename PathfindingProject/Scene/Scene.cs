using PathfindingProject.Core;

namespace PathfindingProject.Scene;

/// <summary>
/// Global scene manager that handles events for registered behaviours.
/// </summary>
public static class Scene
{
    /// <summary>
    /// The application’s main form.
    /// </summary>
    public static Form Form = null!;

    /// <summary>
    /// All active scene behaviours.
    /// </summary>
    public static readonly List<SceneBehaviour> Behaviours = new();

    /// <summary>
    /// Calls Awake() on all registered behaviours. Runs once before Start().
    /// </summary>
    public static void AwakeAll()
    {
        for (int i = 0; i < Behaviours.Count; i++)
        {
            Behaviours[i].Awake();
        }
    }

    /// <summary>
    /// Calls Start() on all registered behaviours. Runs once after Awake().
    /// </summary>
    public static void StartAll()
    {
        for (int i = 0; i < Behaviours.Count; i++)
        {
            Behaviours[i].Start();
        }
    }

    /// <summary>
    /// Updates all behaviours each frame. Also updates time and input state.
    /// </summary>
    public static void UpdateAll()
    {
        Time.Update();

        for (int i = 0; i < Behaviours.Count; i++)
        {
            Behaviours[i].Update();
        }

        InputManager.ResetFrameState();
    }

    /// <summary>
    /// Renders all behaviours by calling Draw() on each.
    /// </summary>
    public static void DrawAll(Graphics g)
    {
        for (int i = 0; i < Behaviours.Count; i++)
        {
            Behaviours[i].Draw(g);
        }
    }
}