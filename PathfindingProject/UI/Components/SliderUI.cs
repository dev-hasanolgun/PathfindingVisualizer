using PathfindingProject.Rendering;

namespace PathfindingProject.UI.Components;

/// <summary>
/// A labeled integer slider control with real-time value display and change notification.
/// </summary>
public class SliderUI : Panel
{
    /// <summary>Triggered when the slider value changes.</summary>
    public event EventHandler? OnScroll;

    public Label TitleLabel { get; }
    public TrackBar Slider { get; }
    public Label ValueLabel { get; }

    protected override bool ShowFocusCues => false;

    /// <summary>
    /// Gets or sets the current slider value.
    /// </summary>
    public int Value
    {
        get => Slider.Value;
        set
        {
            Slider.Value = value;
            UpdateValueLabel();
        }
    }

    /// <summary>
    /// Creates a new labeled slider UI control.
    /// </summary>
    /// <param name="title">The label text to display above the slider.</param>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    /// <param name="initialValue">Starting slider value.</param>
    /// <param name="sliderWidth">Width of the slider track.</param>
    /// <param name="onScroll">Optional event callback for scroll.</param>
    public SliderUI(string title, int min, int max, int initialValue, int sliderWidth = 150, EventHandler? onScroll = null)
    {
        const int padding = 5;
        var font = FontPool.Get("Segoe UI", 10f);
        var sliderSize = new Size(sliderWidth, 30);
        var valueBoxSize = new Size(60, 24);

        var titleWidth = TextRenderer.MeasureText(title, font).Width;
        var defaultWidth = sliderSize.Width + valueBoxSize.Width + padding;
        var usableWidth = Math.Max(defaultWidth, titleWidth + padding);

        Size = new Size(usableWidth, 70);
        BackColor = Color.FromArgb(24, 24, 24);
        BorderStyle = BorderStyle.None;

        TitleLabel = new Label
        {
            Text = title,
            ForeColor = Color.Gainsboro,
            BackColor = Color.Transparent,
            Font = font,
            AutoSize = true,
            Location = new Point(padding * 2, 8)
        };

        Slider = new TrackBar
        {
            Minimum = min,
            Maximum = max,
            Value = initialValue,
            TickStyle = TickStyle.None,
            BackColor = Color.FromArgb(24, 24, 24),
            ForeColor = Color.SkyBlue,
            Size = sliderSize,
            Location = new Point(padding, 30)
        };

        ValueLabel = new Label
        {
            Text = initialValue.ToString(),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(40, 40, 40),
            Font = font,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(50, 24),
            Location = new Point(sliderSize.Width + padding, 28)
        };

        // Update label and call handler on scroll
        OnScroll += (_, _) => UpdateValueLabel();
        if (onScroll != null)
            OnScroll += onScroll;

        Slider.Scroll += (_, _) => OnScroll?.Invoke(this, EventArgs.Empty);

        Controls.Add(TitleLabel);
        Controls.Add(Slider);
        Controls.Add(ValueLabel);
    }

    /// <summary>
    /// Updates the label to reflect the current slider value.
    /// </summary>
    private void UpdateValueLabel()
    {
        ValueLabel.Text = Slider.Value.ToString();
    }
}