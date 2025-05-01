using System.Reflection;

namespace PathfindingProject.Scene;

/// <summary>
/// Automatically finds and instantiates all SceneBehaviour subclasses at startup.
/// Ensures they are registered with the SceneRegistry and participate in the lifecycle.
/// </summary>
public static class ScriptBootstrapper
{
    /// <summary>
    /// Scans the current assembly for non-abstract SceneBehaviour types and instantiates them.
    /// </summary>
    public static void InitializeSceneBehaviours()
    {
        var behaviourTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(SceneBehaviour).IsAssignableFrom(t));

        foreach (var type in behaviourTypes)
        {
            try
            {
                var instance = (SceneBehaviour?)Activator.CreateInstance(type);

                if (instance != null)
                {
                    SceneRegistry.Register(type, instance);
                }
                else
                {
                    throw new InvalidOperationException($"Failed to instantiate {type.FullName}");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error initializing {type.Name}: {ex.Message}", ex);
            }
        }
    }
}