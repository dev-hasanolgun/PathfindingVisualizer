﻿using PathfindingProject.Rendering;
using PathfindingProject.UI.Scaling;

namespace PathfindingProject.UI.Components;

/// <summary>
/// A generic dropdown UI for selecting a value from an enum type.
/// </summary>
public class EnumDropdownUI<TEnum> : ScalableControl where TEnum : Enum
{
    /// <summary>
    /// Occurs when the selected enum value changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// The ComboBox control displaying enum options.
    /// </summary>
    public ComboBox ComboBox { get; private set; }

    /// <summary>
    /// The label displayed above the dropdown.
    /// </summary>
    public Label Label { get; private set; }

    /// <summary>
    /// The text content of the label.
    /// </summary>
    public string LabelText { get; private set; }

    /// <summary>
    /// The currently selected enum value.
    /// </summary>
    public TEnum SelectedValue => (TEnum)ComboBox.SelectedItem!;

    public EnumDropdownUI(string label, TEnum defaultValue, int width = 0, int height = 60, EventHandler? selectionChanged = null)
    {
        const int padding = 10;
        const int labelToComboSpacing = 2;

        var font = FontPool.Get("Segoe UI", S(10f));

        // Calculate width dynamically based on label text
        var titleWidth = TextRenderer.MeasureText(label, font).Width + S(padding) * 2;
        var defaultWidth = S(width) + S(padding) * 2;
        var usableWidth = width > 0 ? Math.Max(defaultWidth, titleWidth + S(padding) * 2) : titleWidth + S(padding) * 2;

        LabelText = label;
        Size = new Size(usableWidth, S(height));
        BackColor = Color.FromArgb(24, 24, 24);

        // Create label
        Label = new Label
        {
            Text = label,
            Font = font,
            ForeColor = Color.Gainsboro,
            BackColor = Color.Transparent,
            Location = P(padding, 4),
            AutoSize = true
        };

        // Create ComboBox
        ComboBox = new ComboBox
        {
            Font = font,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(S(padding), Label.Bottom + S(labelToComboSpacing)),
            Size = new Size(usableWidth - S(padding) * 2, S(20)),
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
    /// Adds all enum values to the ComboBox and sets the default selection.
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