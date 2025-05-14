using PathfindingProject.Rendering;

namespace PathfindingProject.UI.Components;

/// <summary>
/// A styled float input box with optional min/max clamping and title label.
/// </summary>
public class FloatInputUI : Panel
{
    /// <summary>
    /// The label displaying the input title.
    /// </summary>
    public Label TitleLabel { get; }

    /// <summary>
    /// The text box used for float input.
    /// </summary>
    public TextBox InputBox { get; }

    /// <summary>Optional minimum value to enforce on input.</summary>
    public float? Min { get; set; }

    /// <summary>Optional maximum value to enforce on input.</summary>
    public float? Max { get; set; }

    /// <summary>Gets or sets the current float value, applying min/max clamping.</summary>
    public float Value
    {
        get
        {
            float.TryParse(InputBox.Text, out var value);
            return ClampValue(value);
        }
        set
        {
            var clamped = ClampValue(value);
            InputBox.Text = clamped.ToString("0.###");
        }
    }

    public FloatInputUI(string title, float defaultValue = 0f, float? min = default, float? max = default)
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
            Text = defaultValue.ToString("0.###"),
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

        InputBox.TextChanged += (_, _) =>
        {
            Value = Value;
        };
    }

    /// <summary>
    /// Handles key press events to restrict input to valid float characters.
    /// </summary>
    private void HandleKeyPress(object? sender, KeyPressEventArgs e)
    {
        var c = e.KeyChar;
        var isMinus = c == '-';
        var isDot = c == '.';

        if (!char.IsControl(c) && !char.IsDigit(c) && !isMinus && !isDot)
        {
            e.Handled = true;
        }

        if (isMinus)
        {
            if (InputBox.SelectionStart != 0 || InputBox.Text.Contains('-'))
            {
                e.Handled = true;
            }
        }

        if (isDot)
        {
            if (InputBox.Text.Contains('.') && !InputBox.SelectedText.Contains('.'))
            {
                e.Handled = true;
            }
        }

        if (c == (char)Keys.Enter || c == (char)Keys.Escape)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// Clamps the given float value within optional min and max bounds.
    /// </summary>
    private float ClampValue(float value)
    {
        if (Min.HasValue && Max.HasValue) return Math.Clamp(value, Min.Value, Max.Value);
        if (Min.HasValue) return Math.Max(value, Min.Value);
        if (Max.HasValue) return Math.Min(value, Max.Value);

        return value;
    }
}