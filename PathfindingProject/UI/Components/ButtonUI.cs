using PathfindingProject.Rendering;

namespace PathfindingProject.UI.Components;

/// <summary>
/// A custom panel that contains a styled button with consistent appearance and sizing.
/// </summary>
public class ButtonUI : Panel
{
    /// <summary>
    /// Event triggered when the button is clicked.
    /// </summary>
    public event EventHandler? OnClick;

    /// <summary>
    /// The internal button control.
    /// </summary>
    public Button Button { get; }

    /// <summary>
    /// Creates a new styled button wrapped in a panel with consistent layout and padding.
    /// </summary>
    /// <param name="text">The text displayed on the button.</param>
    /// <param name="onClick">The event handler to invoke on click.</param>
    /// <param name="width">Optional width override for the panel.</param>
    /// <param name="height">Height of the panel (default is 40).</param>
    /// <param name="backColor">Optional background color for the button. Defaults to dark gray if not specified.</param>
    public ButtonUI(string text, EventHandler onClick, int width = 0, int height = 40, Color? backColor = null)
    {
        const int padding = 10;
        const int verticalPadding = 6;

        var font = FontPool.Get("Segoe UI", 10f);

        // Calculate the required width based on text size
        var titleWidth = TextRenderer.MeasureText(text, font).Width + padding * 2;
        var defaultWidth = width + padding * 2;
        var usableWidth = width > 0 ? Math.Max(defaultWidth, titleWidth + padding * 2) : titleWidth + padding * 2;

        Size = new Size(usableWidth, height);
        BackColor = Color.FromArgb(24, 24, 24); // Dark background

        Button = new Button
        {
            Text = text,
            Font = font,
            Size = new Size(titleWidth, 28),
            Location = new Point(padding, verticalPadding),
            BackColor = backColor ?? Color.FromArgb(40, 40, 40), // Use provided color or default
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };

        Button.FlatAppearance.BorderSize = 1;
        Button.FlatAppearance.BorderColor = Color.DimGray;

        // Forward the button's click event through the OnClick event
        OnClick += onClick;
        Button.Click += (_, _) => OnClick?.Invoke(this, EventArgs.Empty);

        Controls.Add(Button);
    }
}