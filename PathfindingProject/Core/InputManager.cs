namespace PathfindingProject.Core;

public static class InputManager
{
    /// <summary>
    /// Triggered on any mouse click to request keyboard focus clearing from focused UI.
    /// </summary>
    public static event Action? RequestFocusClear;

    private static readonly Dictionary<Keys, InputState> s_keyStates = new();
    private static readonly Dictionary<MouseButtons, InputState> s_mouseStates = new();

    private static int s_scrollDelta;

    /// <summary>
    /// Gets the current mouse position in screen coordinates.
    /// </summary>
    public static Point MousePosition { get; private set; }

    /// <summary>
    /// Gets the mouse scroll delta since the last frame.
    /// </summary>
    public static int ScrollDelta => s_scrollDelta;

    #region Input Queries — Keyboard

    /// <summary>
    /// Returns true if a key is held down (Pressed or Held). Can be exclusive.
    /// </summary>
    public static bool GetKey(Keys key, bool exclusive = false)
    {
        var isHeld = s_keyStates.GetValueOrDefault(key) is InputState.Pressed or InputState.Held;
        if (!exclusive) return isHeld;

        var onlyThis = s_keyStates.Count(kv => kv.Value is InputState.Pressed or InputState.Held) == 1;
        var noMouse = s_mouseStates.All(kv => kv.Value is InputState.Up or InputState.Released);
        return isHeld && onlyThis && noMouse;
    }

    /// <summary>
    /// Returns true only on the frame the key was pressed.
    /// </summary>
    public static bool GetKeyDown(Keys key, bool exclusive = false)
    {
        var isPressed = s_keyStates.GetValueOrDefault(key) == InputState.Pressed;
        if (!exclusive) return isPressed;

        var onlyThis = s_keyStates.Count(kv => kv.Value is InputState.Pressed or InputState.Held) == 1;
        var noMouse = s_mouseStates.All(kv => kv.Value is InputState.Up or InputState.Released);
        return isPressed && onlyThis && noMouse;
    }

    /// <summary>
    /// Returns true only on the frame the key was released.
    /// </summary>
    public static bool GetKeyUp(Keys key, bool exclusive = false)
    {
        var isReleased = s_keyStates.GetValueOrDefault(key) == InputState.Released;
        if (!exclusive) return isReleased;

        var onlyThis = s_keyStates.Count(kv => kv.Value == InputState.Released) == 1;
        var noMouse = s_mouseStates.All(kv => kv.Value is InputState.Up or InputState.Released);
        return isReleased && onlyThis && noMouse;
    }

    #endregion

    #region Input Queries — Mouse

    /// <summary>
    /// Returns true if the specified mouse button is currently held down (Pressed or Held). Can be exclusive.
    /// </summary>
    public static bool GetMouseButton(MouseButtons button, bool exclusive = false)
    {
        var isHeld = s_mouseStates.GetValueOrDefault(button) is InputState.Pressed or InputState.Held;
        if (!exclusive) return isHeld;

        var onlyThis = s_mouseStates.Count(kv => kv.Value is InputState.Pressed or InputState.Held) == 1;
        var noKeys = s_keyStates.All(kv => kv.Value is InputState.Up or InputState.Released);
        return isHeld && onlyThis && noKeys;
    }

    /// <summary>
    /// Returns true only on the frame the specified mouse button was pressed. Can be exclusive.
    /// </summary>
    public static bool GetMouseButtonDown(MouseButtons button, bool exclusive = false)
    {
        var isPressed = s_mouseStates.GetValueOrDefault(button) == InputState.Pressed;
        if (!exclusive) return isPressed;

        var onlyThis = s_mouseStates.Count(kv => kv.Value is InputState.Pressed or InputState.Held) == 1;
        var noKeys = s_keyStates.All(kv => kv.Value is InputState.Up or InputState.Released);
        return isPressed && onlyThis && noKeys;
    }

    /// <summary>
    /// Returns true only on the frame the specified mouse button was released. Can be exclusive.
    /// </summary>
    public static bool GetMouseButtonUp(MouseButtons button, bool exclusive = false)
    {
        var isReleased = s_mouseStates.GetValueOrDefault(button) == InputState.Released;
        if (!exclusive) return isReleased;

        var onlyThis = s_mouseStates.Count(kv => kv.Value == InputState.Released) == 1;
        var noKeys = s_keyStates.All(kv => kv.Value is InputState.Up or InputState.Released);
        return isReleased && onlyThis && noKeys;
    }

    #endregion

    #region Input Event Handlers

    /// <summary>
    /// Processes a keyboard key down event.
    /// </summary>
    public static void HandleKeyDown(KeyEventArgs e)
    {
        if (!s_keyStates.TryGetValue(e.KeyCode, out var state) || state == InputState.Up)
            s_keyStates[e.KeyCode] = InputState.Pressed;
    }

    /// <summary>
    /// Processes a keyboard key up event.
    /// </summary>
    public static void HandleKeyUp(KeyEventArgs e)
    {
        s_keyStates[e.KeyCode] = InputState.Released;
    }

    /// <summary>
    /// Processes a mouse button down event and requests focus clear.
    /// </summary>
    public static void HandleMouseDown(MouseEventArgs e)
    {
        if (!s_mouseStates.TryGetValue(e.Button, out var state) || state == InputState.Up)
            s_mouseStates[e.Button] = InputState.Pressed;

        RequestFocusClear?.Invoke();
    }

    /// <summary>
    /// Processes a mouse button up event.
    /// </summary>
    public static void HandleMouseUp(MouseEventArgs e)
    {
        s_mouseStates[e.Button] = InputState.Released;
    }

    /// <summary>
    /// Updates the mouse position.
    /// </summary>
    public static void HandleMouseMove(MouseEventArgs e)
    {
        MousePosition = e.Location;
    }

    /// <summary>
    /// Accumulates mouse scroll delta for the current frame.
    /// </summary>
    public static void HandleMouseScroll(MouseEventArgs e)
    {
        s_scrollDelta += e.Delta;
    }

    #endregion

    #region Disposal

    /// <summary>
    /// Transitions inputs between frames: Pressed->Held, Released->Up.
    /// Call once per frame at the end of input processing.
    /// </summary>
    public static void ResetFrameState()
    {
        s_scrollDelta = 0;

        foreach (var key in s_keyStates.Keys.ToList())
        {
            s_keyStates[key] = s_keyStates[key] switch
            {
                InputState.Pressed => InputState.Held,
                InputState.Released => InputState.Up,
                _ => s_keyStates[key]
            };
        }

        foreach (var button in s_mouseStates.Keys.ToList())
        {
            s_mouseStates[button] = s_mouseStates[button] switch
            {
                InputState.Pressed => InputState.Held,
                InputState.Released => InputState.Up,
                _ => s_mouseStates[button]
            };
        }
    }

    /// <summary>
    /// Immediately sets all key and mouse inputs to the Up state and resets scroll delta.
    /// </summary>
    public static void ForceReleaseAllInputs()
    {
        foreach (var key in s_keyStates.Keys.ToList())
        {
            s_keyStates[key] = InputState.Up;
        }

        foreach (var button in s_mouseStates.Keys.ToList())
        {
            s_mouseStates[button] = InputState.Up;
        }

        s_scrollDelta = 0;
    }

    #endregion
}