using PathfindingProject.Rendering;

namespace PathfindingProject.UI.Components;

/// <summary>
/// A custom panel that contains a styled button with consistent appearance and sizing.
/// </summary>
public class ButtonUI : Panel
{
    public event EventHandler? OnClick;

    public Button Button { get; }

    public ButtonUI(string text, EventHandler onClick, int width = 0, int height = 40)
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
            BackColor = Color.FromArgb(40, 40, 40),
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