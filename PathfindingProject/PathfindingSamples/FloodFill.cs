namespace PathfindingProject.PathfindingSamples;

/// <summary>
/// Generates randomized obstacle maps ensuring connectivity of accessible cells from a start point.
/// </summary>
public class FloodFill
{
    private readonly Random _random = new();    // Random number generator instance for obstacle placement.
    
    private Point _gridSize;    // Stores grid dimensions for obstacle placement.

    /// <summary>
    /// Initializes the grid size for obstacle placement.
    /// </summary>
    /// <param name="gridSize">Dimensions of the grid.</param>
    public void Initialize(Point gridSize)
    {
        _gridSize = gridSize;   // Set the internal grid size to the provided dimensions.
    }

    /// <summary>
    /// Generates a random set of obstacles, ensuring all remaining cells are reachable.
    /// </summary>
    /// <param name="obstaclePercent">Percentage of grid cells to become obstacles.</param>
    /// <param name="neighborSearchType">Type of neighbor consideration (FourWay or EightWay).</param>
    /// <returns>A set of points representing obstacle positions.</returns>
    public HashSet<Point> DefineObstacles(float obstaclePercent, NeighborSearchType neighborSearchType = NeighborSearchType.FourWay)
    {
        var obstacleList = new HashSet<Point>();                        // Initialize an empty set to store obstacle points.
        var totalCells = _gridSize.X * _gridSize.Y - 1;              // Calculate the total number of cells available (excluding one for the start point).
        var targetObstacleCount = (int)(totalCells * obstaclePercent);  // Calculate how many obstacles we want based on the provided percentage.
        var points = GetShuffledPoints().ToList();            // Generate a shuffled list of all grid points.
        var startPoint = points[0];                                     // Select the first point as the guaranteed starting accessible cell.

        // Iterate through the remaining points to potentially place obstacles.
        foreach (var point in points.Skip(1))
        {
            // Stop placing obstacles once we reach the desired count.
            if (obstacleList.Count >= targetObstacleCount)
            {
                break;
            }
            
            obstacleList.Add(point);    // Add the current point as an obstacle.

            // Verify that adding this obstacle doesn't break grid connectivity.
            if (!IsGridConnected(obstacleList, startPoint, neighborSearchType))
            {
                obstacleList.Remove(point);  // Remove obstacle if connectivity is compromised.
            }
        }
        
        return obstacleList;    // Return the final set of obstacle points.
    }

    /// <summary>
    /// Determines if accessible cells remain fully connected after obstacle placement.
    /// </summary>
    /// <param name="obstacleList">Current obstacle set.</param>
    /// <param name="startPoint">Point to start connectivity check from.</param>
    /// <param name="neighborSearchType">Neighboring mode (FourWay or EightWay).</param>
    /// <returns>True if all non-obstacle cells are connected; otherwise false.</returns>
    private bool IsGridConnected(HashSet<Point> obstacleList, Point startPoint, NeighborSearchType neighborSearchType)
    {
        var visited = new HashSet<Point> { startPoint };    // Initialize visited set with the start point.
        var queue = new Queue<Point>();                     // Queue for breadth-first search traversal starting from the start point.
        
        queue.Enqueue(startPoint);  // Add start point to exploration queue.

        // Perform BFS to explore accessible cells.
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();  // Dequeue the next cell to process.

            // Iterate over each neighbor of the current cell.
            foreach (var neighbor in SampleUtils.GetNeighbors(current, _gridSize, neighborSearchType))
            {
                // Check if the neighbor hasn't been visited and isn't an obstacle.
                if (!visited.Contains(neighbor) && !obstacleList.Contains(neighbor))
                {
                    visited.Add(neighbor);      // Mark neighbor as visited.
                    queue.Enqueue(neighbor);    // Enqueue neighbor for further exploration
                }
            }
        }
        var expectedAccessibleCount = (_gridSize.X * _gridSize.Y) - obstacleList.Count; // Calculate the expected number of accessible cells.
        
        return visited.Count == expectedAccessibleCount;    // Check if the actual number of visited cells matches the expected count.
    }

    /// <summary>
    /// Generates points of the grid in random order (Fisher-Yates shuffle).
    /// </summary>
    /// <returns>Enumerable of points in random sequence.</returns>
    private IEnumerable<Point> GetShuffledPoints()
    {
        var points = new List<Point>(); // Create a list to store all points in the grid.

        // Fill the list with all grid points.
        for (int y = 0; y < _gridSize.Y; y++)
        {
            for (int x = 0; x < _gridSize.X; x++)
            {
                points.Add(new Point(x, y));
            }
        }

        // Shuffle points using the Fisher-Yates shuffle algorithm.
        for (int i = points.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);            // Select a random index from 0 to i.
            (points[i], points[j]) = (points[j], points[i]);    // Swap points[i] with points[j].
        }
        
        return points;  // Return the randomized sequence of points.
    }
}