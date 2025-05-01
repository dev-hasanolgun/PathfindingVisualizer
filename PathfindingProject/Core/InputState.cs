namespace PathfindingProject.Core;

/// <summary>
/// Describes the current state of a key or mouse button.
/// </summary>
public enum InputState
{
    /// <summary>The input is not pressed.</summary>
    Up,

    /// <summary>The input was pressed during this frame.</summary>
    Pressed,

    /// <summary>The input is held down across frames.</summary>
    Held,

    /// <summary>The input was released during this frame.</summary>
    Released
}