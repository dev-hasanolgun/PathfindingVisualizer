using PathfindingProject.Rendering;

namespace PathfindingProject.UI.Components;

/// <summary>
/// A flexible text UI component with optional multiline, scrollable display, and padding control.
/// </summary>
public class TextUI : Panel
{
    public Label Label { get; private set; }

    private int _padding;
    private bool _multiline;
    private bool _scrollable;
    private int _explicitWidth;
    private int _explicitHeight;

    /// <summary>
    /// Gets or sets the text content displayed in the label.
    /// </summary>
    public string TextContent
    {
        get => Label.Text;
        set
        {
            Label.Text = value;
            UpdateLayout();
        }
    }

    /// <summary>
    /// Gets or sets the text alignment within the label.
    /// </summary>
    public ContentAlignment TextAlign
    {
        get => Label.TextAlign;
        set => Label.TextAlign = value;
    }

    /// <summary>
    /// Gets or sets the font of the label text.
    /// </summary>
    public Font Font
    {
        get => Label.Font;
        set
        {
            Label.Font = value;
            UpdateLayout();
        }
    }

    /// <summary>
    /// Initializes a new text display UI element.
    /// </summary>
    public TextUI(string text = "", float textSize = 10f, bool multiline = false, bool scrollable = false, int width = 0, int height = 0, int padding = 10)
    {
        var font = FontPool.Get("Segoe UI", textSize, FontStyle.Bold);

        _padding = padding;
        _multiline = multiline;
        _scrollable = scrollable;
        _explicitWidth = width;
        _explicitHeight = height;

        BackColor = Color.Transparent;
        Location = new Point(20, 300); // Optional: could be set by caller instead

        Label = new Label
        {
            Text = text,
            Font = font,
            ForeColor = Color.Gainsboro,
            BackColor = Color.Transparent,
            AutoSize = multiline,
            TextAlign = ContentAlignment.TopLeft,
            AutoEllipsis = !multiline
        };

        if (scrollable)
        {
            var scroll = new Panel
            {
                AutoScroll = true,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            scroll.Controls.Add(Label);
            Controls.Add(scroll);
        }
        else
        {
            Controls.Add(Label);
        }

        UpdateLayout();
    }

    /// <summary>
    /// Updates layout size based on current text, font, and padding.
    /// </summary>
    private void UpdateLayout()
    {
        var measuredSize = TextRenderer.MeasureText(Label.Text, Label.Font);
        var paddedSize = measuredSize + new Size(2 * _padding, 2 * _padding);

        int finalWidth = _explicitWidth > 0 ? Math.Max(_explicitWidth, paddedSize.Width) : paddedSize.Width;
        int finalHeight = _explicitHeight > 0 ? Math.Max(_explicitHeight, paddedSize.Height) : paddedSize.Height;

        Size = new Size(finalWidth, finalHeight);
        Label.Location = new Point(_padding, _padding);
        Label.Size = measuredSize;
    }
}