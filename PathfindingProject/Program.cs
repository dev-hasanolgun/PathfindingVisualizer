namespace PathfindingProject;

/// <summary>
/// Application entry point. Bootstraps the main form and configures environment settings.
/// </summary>
internal static class Program
{
    /// <summary>
    /// The main entry point for the pathfinding visualizer application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var form = new MainForm();
        Scene.Scene.Form = form;

        Application.Run(form);
    }
}