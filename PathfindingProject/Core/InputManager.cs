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

    public static Point MousePosition { get; private set; }

    /// <summary>
    /// Gets the mouse scroll delta since the last frame.
    /// </summary>
    public static int ScrollDelta => s_scrollDelta;

    // ─────────────────────────────────────────────────────────────
    // Input Queries — Keyboard
    // ─────────────────────────────────────────────────────────────

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

    // ─────────────────────────────────────────────────────────────
    // Input Queries — Mouse
    // ─────────────────────────────────────────────────────────────

    public static bool GetMouseButton(MouseButtons button, bool exclusive = false)
    {
        var isHeld = s_mouseStates.GetValueOrDefault(button) is InputState.Pressed or InputState.Held;
        if (!exclusive) return isHeld;

        var onlyThis = s_mouseStates.Count(kv => kv.Value is InputState.Pressed or InputState.Held) == 1;
        var noKeys = s_keyStates.All(kv => kv.Value is InputState.Up or InputState.Released);
        return isHeld && onlyThis && noKeys;
    }

    public static bool GetMouseButtonDown(MouseButtons button, bool exclusive = false)
    {
        var isPressed = s_mouseStates.GetValueOrDefault(button) == InputState.Pressed;
        if (!exclusive) return isPressed;

        var onlyThis = s_mouseStates.Count(kv => kv.Value is InputState.Pressed or InputState.Held) == 1;
        var noKeys = s_keyStates.All(kv => kv.Value is InputState.Up or InputState.Released);
        return isPressed && onlyThis && noKeys;
    }

    public static bool GetMouseButtonUp(MouseButtons button, bool exclusive = false)
    {
        var isReleased = s_mouseStates.GetValueOrDefault(button) == InputState.Released;
        if (!exclusive) return isReleased;

        var onlyThis = s_mouseStates.Count(kv => kv.Value == InputState.Released) == 1;
        var noKeys = s_keyStates.All(kv => kv.Value is InputState.Up or InputState.Released);
        return isReleased && onlyThis && noKeys;
    }

    // ─────────────────────────────────────────────────────────────
    // Input Event Handlers (called externally by WinForms)
    // ─────────────────────────────────────────────────────────────

    public static void HandleKeyDown(KeyEventArgs e)
    {
        if (!s_keyStates.TryGetValue(e.KeyCode, out var state) || state == InputState.Up)
            s_keyStates[e.KeyCode] = InputState.Pressed;
    }

    public static void HandleKeyUp(KeyEventArgs e)
    {
        s_keyStates[e.KeyCode] = InputState.Released;
    }

    public static void HandleMouseDown(MouseEventArgs e)
    {
        if (!s_mouseStates.TryGetValue(e.Button, out var state) || state == InputState.Up)
            s_mouseStates[e.Button] = InputState.Pressed;

        RequestFocusClear?.Invoke();
    }

    public static void HandleMouseUp(MouseEventArgs e)
    {
        s_mouseStates[e.Button] = InputState.Released;
    }

    public static void HandleMouseMove(MouseEventArgs e)
    {
        MousePosition = e.Location;
    }

    public static void HandleMouseScroll(MouseEventArgs e)
    {
        s_scrollDelta += e.Delta;
    }

    // ─────────────────────────────────────────────────────────────
    // Frame Lifecycle
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Transitions inputs between frames: Pressed→Held, Released→Up.
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
}