using System.Reflection;

namespace PathfindingProject;

/// <summary>
/// A non-blocking popup form that displays an embedded image containing keybinding information.
/// The form auto-centers over its owner and only allows a single instance to be opened.
/// </summary>
public partial class KeybindInfoPopupForm : Form
{
    /// <summary>
    /// Initializes the popup and loads the embedded keybind image.
    /// </summary>
    public KeybindInfoPopupForm()
    {
        InitializeComponent();
        ShowEmbeddedImage();
    }

    /// <summary>
    /// Loads and displays an embedded image inside the popup.
    /// Sets form size and appearance based on the image.
    /// </summary>
    private void ShowEmbeddedImage()
    {
        var image = LoadEmbeddedImage("KeybindInfo.png");

        if (image == null)
        {
            this.Close();
            return;
        }

        var pic = new PictureBox
        {
            Image = image,
            SizeMode = PictureBoxSizeMode.Zoom,
            Width = 350,
            Height = 500,
            Location = new Point(10, 10)
        };

        Controls.Add(pic);

        ClientSize = new Size(pic.Width + 20, pic.Height + 20);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Text = "Keybind Info";
    }

    /// <summary>
    /// Loads an embedded image from the application's compiled resources.
    /// </summary>
    /// <param name="shortName">The short file name of the embedded image (e.g., "KeybindInfo.png").</param>
    /// <returns>The loaded Image if found; otherwise, null.</returns>
    private Image? LoadEmbeddedImage(string shortName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fullName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(r => r.EndsWith(shortName, StringComparison.OrdinalIgnoreCase));

        if (fullName == null)
        {
            MessageBox.Show($"Embedded image '{shortName}' not found.");
            return null;
        }

        using var stream = assembly.GetManifestResourceStream(fullName)!;
        return Image.FromStream(stream);
    }

    /// <summary>
    /// Ensures the popup is centered over its owner form after loading.
    /// </summary>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (Owner != null)
        {
            int x = Owner.Left + (Owner.Width - Width) / 2;
            int y = Owner.Top + (Owner.Height - Height) / 2;
            Location = new Point(x, y);
        }
    }
}