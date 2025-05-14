using PathfindingProject.Core;
using PathfindingProject.Pathfinding.Enums;

namespace PathfindingProject.Pathfinding.Algorithms;

/// <summary>
/// Generates randomized obstacle maps while ensuring all empty cells are accessible from a start point.
/// </summary>
public class FloodFill
{
    private readonly Random _random = new();
    private Point _gridSize;

    /// <summary>
    /// Sets the size of the grid before generating obstacles.
    /// </summary>
    public void Initialize(Point gridSize)
    {
        _gridSize = gridSize;
    }

    /// <summary>
    /// Defines a set of obstacles randomly while ensuring the remaining grid is fully accessible.
    /// </summary>
    public HashSet<Point> DefineObstacles(float obstaclePercent, NeighborMode neighborMode = NeighborMode.FourWay)
    {
        var obstacleList = new HashSet<Point>();
        var totalCells = _gridSize.X * _gridSize.Y - 1;
        var totalObstacleAmount = (int)(totalCells * obstaclePercent);

        var isFirst = true;
        var firstPoint = new Point();

        foreach (var point in GetShuffledPoints())
        {
            if (isFirst)
            {
                firstPoint = point;
                isFirst = false;
                continue;
            }

            if (obstacleList.Count >= totalObstacleAmount)
                break;

            obstacleList.Add(point);

            // Only keep the obstacle if the remaining grid is still fully connected
            if (!IsGridConnected(obstacleList, firstPoint, totalCells, neighborMode))
            {
                obstacleList.Remove(point);
            }
        }

        return obstacleList;
    }

    /// <summary>
    /// Checks whether the remaining empty cells form a single connected region starting from the given point.
    /// </summary>
    public bool IsGridConnected(HashSet<Point> obstacleList, Point startPoint, int targetCount, NeighborMode neighborMode = NeighborMode.FourWay)
    {
        var visited = new HashSet<Point> { startPoint };
        var queue = new Queue<Point>();
        queue.Enqueue(startPoint);
        var accessibleCount = 0;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            
            // Allocate a fixed-size buffer on the stack for neighbor points (faster, avoids GC).
            Span<Point> neighborBuffer = stackalloc Point[8];
            var neighborCount = Extensions.GetNeighborsNonAlloc(current, _gridSize, neighborBuffer, neighborMode);

            for (int i = 0; i < neighborCount; i++)
            {
                var neighbor = neighborBuffer[i];

                if (!visited.Contains(neighbor) && !obstacleList.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                    accessibleCount++;
                }
            }
        }

        var expectedAccessible = targetCount - obstacleList.Count;
        return accessibleCount == expectedAccessible;
    }

    /// <summary>
    /// Yields all grid points in random order using a Fisher-Yates shuffle.
    /// </summary>
    private IEnumerable<Point> GetShuffledPoints()
    {
        var size = _gridSize.X * _gridSize.Y;
        var indices = new int[size];
        for (int i = 0; i < size; i++)
            indices[i] = i;

        for (int i = size - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        foreach (var index in indices)
        {
            var x = index % _gridSize.X;
            var y = index / _gridSize.X;
            yield return new Point(x, y);
        }
    }
}