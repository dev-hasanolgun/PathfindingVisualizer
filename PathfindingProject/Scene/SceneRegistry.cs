namespace PathfindingProject.Scene;

/// <summary>
/// Provides global lookup and registration for SceneBehaviour instances.
/// Acts as a service locator within the scene system.
/// </summary>
public static class SceneRegistry
{
    private static readonly Dictionary<Type, SceneBehaviour> s_instances = new();

    /// <summary>
    /// Registers a SceneBehaviour instance by its type.
    /// </summary>
    public static void Register<T>(T instance) where T : SceneBehaviour
    {
        Register(typeof(T), instance);
    }

    /// <summary>
    /// Registers a SceneBehaviour instance with the specified type.
    /// </summary>
    public static void Register(Type type, SceneBehaviour instance)
    {
        s_instances[type] = instance;
    }

    /// <summary>
    /// Resolves a registered instance of the specified SceneBehaviour type.
    /// </summary>
    public static T Resolve<T>() where T : SceneBehaviour
    {
        return (T)s_instances[typeof(T)];
    }

    /// <summary>
    /// Attempts to resolve a SceneBehaviour instance. Returns true if found.
    /// </summary>
    public static bool TryResolve<T>(out T result) where T : SceneBehaviour
    {
        if (s_instances.TryGetValue(typeof(T), out var behavior))
        {
            result = (T)behavior;
            return true;
        }

        result = null!;
        return false;
    }
}