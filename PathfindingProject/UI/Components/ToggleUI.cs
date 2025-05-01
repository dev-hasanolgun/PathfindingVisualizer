using PathfindingProject.Rendering;

namespace PathfindingProject.UI.Components;

/// <summary>
/// A styled checkbox control with a label, consistent with the app's dark UI theme.
/// </summary>
public class ToggleUI : Panel
{
    /// <summary>
    /// The underlying checkbox control.
    /// </summary>
    public CheckBox CheckBox { get; private set; }

    /// <summary>
    /// The label shown next to the checkbox.
    /// </summary>
    public Label Label { get; private set; }

    /// <summary>
    /// The text displayed next to the checkbox.
    /// </summary>
    public string LabelText { get; private set; }

    /// <summary>
    /// Whether the checkbox is currently checked.
    /// </summary>
    public bool IsChecked
    {
        get => CheckBox.Checked;
        set => CheckBox.Checked = value;
    }

    /// <summary>
    /// Event fired when the checkbox is toggled.
    /// </summary>
    public event EventHandler? Toggled;

    /// <summary>
    /// Creates a new toggle switch UI with a label and optional default state.
    /// </summary>
    public ToggleUI(string label, bool defaultState = false, int size = 20, EventHandler? toggled = null)
    {
        const int padding = 5;
        var font = FontPool.Get("Segoe UI", 10f);
        LabelText = label;

        var labelOffset = size + padding * 4;
        var titleWidth = TextRenderer.MeasureText(label, font).Width + labelOffset;
        Size = new Size(titleWidth, size * 2);
        BackColor = Color.FromArgb(24, 24, 24);

        CheckBox = new CheckBox
        {
            Checked = defaultState,
            AutoSize = false,
            Size = new Size(size, size),
            Location = new Point(padding * 2, (Height - size) / 2),
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat
        };
        CheckBox.FlatAppearance.BorderSize = 1;
        CheckBox.FlatAppearance.BorderColor = Color.DimGray;

        Label = new Label
        {
            Text = label,
            Font = font,
            ForeColor = Color.Gainsboro,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(CheckBox.Right + padding, (Height - font.Height) / 2)
        };

        if (toggled != null)
            Toggled += toggled;

        CheckBox.CheckedChanged += (_, _) => Toggled?.Invoke(this, EventArgs.Empty);

        Controls.Add(CheckBox);
        Controls.Add(Label);
    }
}