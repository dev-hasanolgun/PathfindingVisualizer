using PathfindingProject.Rendering;

namespace PathfindingProject.UI.Components;

/// <summary>
/// A generic dropdown UI for selecting a value from an enum type.
/// </summary>
public class EnumDropdownUI<TEnum> : Panel where TEnum : Enum
{
    public event EventHandler? SelectionChanged;

    public ComboBox ComboBox { get; private set; }
    public Label Label { get; private set; }
    public string LabelText { get; private set; }

    public TEnum SelectedValue => (TEnum)ComboBox.SelectedItem!;

    public EnumDropdownUI(string label, TEnum defaultValue, int width = 0, int height = 60, EventHandler? selectionChanged = null)
    {
        const int padding = 10;
        const int labelToComboSpacing = 2;

        var font = FontPool.Get("Segoe UI", 10f);

        // Calculate width dynamically based on label text
        var titleWidth = TextRenderer.MeasureText(label, font).Width + padding * 2;
        var defaultWidth = width + padding * 2;
        var usableWidth = width > 0 ? Math.Max(defaultWidth, titleWidth + padding * 2) : titleWidth + padding * 2;

        LabelText = label;
        Size = new Size(usableWidth, height);
        BackColor = Color.FromArgb(24, 24, 24);

        // Create label
        Label = new Label
        {
            Text = label,
            Font = font,
            ForeColor = Color.Gainsboro,
            BackColor = Color.Transparent,
            Location = new Point(padding, 4),
            AutoSize = true
        };

        // Create ComboBox
        ComboBox = new ComboBox
        {
            Font = font,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(padding, Label.Bottom + labelToComboSpacing),
            Size = new Size(usableWidth - padding * 2, 20),
            BackColor = Color.FromArgb(40, 40, 40),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };

        PopulateComboBoxWithEnumValues(defaultValue);

        if (selectionChanged is not null)
            SelectionChanged += selectionChanged;

        ComboBox.SelectedIndexChanged += (_, _) => SelectionChanged?.Invoke(this, EventArgs.Empty);

        Controls.Add(Label);
        Controls.Add(ComboBox);
    }

    /// <summary>
    /// Populates the ComboBox with all values of the enum and sets the default selection.
    /// </summary>
    private void PopulateComboBoxWithEnumValues(TEnum defaultValue)
    {
        var values = Enum.GetValues(typeof(TEnum));
        foreach (var value in values)
        {
            ComboBox.Items.Add(value);
        }
        ComboBox.SelectedItem = defaultValue;
    }
}