using PathfindingProject.Rendering;

namespace PathfindingProject.UI.Components;

/// <summary>
/// A styled integer input box with optional min/max clamping and title label.
/// </summary>
public class IntInputUI : Panel
{
    public Label TitleLabel { get; }
    public TextBox InputBox { get; }

    /// <summary>Optional minimum value to enforce on input.</summary>
    public int? Min { get; set; }

    /// <summary>Optional maximum value to enforce on input.</summary>
    public int? Max { get; set; }

    /// <summary>Gets or sets the current integer value, applying min/max clamping.</summary>
    public int Value
    {
        get
        {
            int.TryParse(InputBox.Text, out var value);
            return ClampValue(value);
        }
        set
        {
            var clamped = ClampValue(value);
            InputBox.Text = clamped.ToString();
        }
    }

    /// <summary>
    /// Creates a new integer input UI panel.
    /// </summary>
    public IntInputUI(string title, int defaultValue = 0, int? min = default, int? max = default)
    {
        const int padding = 10;
        var font = FontPool.Get("Segoe UI", 10f);
        var inputSize = new Size(60, 24);

        var titleWidth = TextRenderer.MeasureText(title, font).Width;
        var defaultWidth = inputSize.Width + padding * 2;
        var usableWidth = Math.Max(defaultWidth, titleWidth + padding * 2);

        Size = new Size(usableWidth, 65);
        BackColor = Color.FromArgb(24, 24, 24);
        Min = min;
        Max = max;

        TitleLabel = new Label
        {
            Text = title,
            Font = font,
            AutoSize = true,
            ForeColor = Color.Gainsboro,
            BackColor = Color.Transparent,
            Location = new Point(padding, 5)
        };

        InputBox = new TextBox
        {
            Text = defaultValue.ToString(),
            Font = font,
            Size = inputSize,
            Location = new Point(padding, 30),
            TextAlign = HorizontalAlignment.Center,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(40, 40, 40),
            ForeColor = Color.White
        };

        Controls.Add(TitleLabel);
        Controls.Add(InputBox);

        InputBox.KeyPress += HandleKeyPress;

        // Re-apply clamping on every change
        InputBox.TextChanged += (_, _) =>
        {
            Value = Value; // Triggers clamp and updates if needed
        };
    }

    /// <summary>
    /// Prevents invalid characters (non-digit, disallowed minus signs).
    /// </summary>
    private void HandleKeyPress(object? sender, KeyPressEventArgs e)
    {
        // Allow digits and control characters (backspace, arrows)
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            // Allow minus only at beginning and not already present
            if (e.KeyChar != '-' || InputBox.SelectionStart != 0 || InputBox.Text.Contains('-'))
            {
                e.Handled = true;
            }
        }

        // Suppress Enter/Escape to avoid focus or form behavior
        if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// Enforces min/max bounds if defined.
    /// </summary>
    private int ClampValue(int value)
    {
        if (Min.HasValue && Max.HasValue)
            return Math.Clamp(value, Min.Value, Max.Value);

        if (Min.HasValue)
            return Math.Max(value, Min.Value);

        if (Max.HasValue)
            return Math.Min(value, Max.Value);

        return value;
    }
}