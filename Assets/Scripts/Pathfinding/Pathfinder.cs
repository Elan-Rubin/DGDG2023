using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathCalculator
{
    private const int STRAIGHT_COST = 10;
    private const int DIAG_COST = 14;

    public Grid<Waypoint> grid;
    private Tilemap map;

    private List<Waypoint> openList;
    private List<Waypoint> closedList;

    private bool ready = false;
    private List<Waypoint> currentPath;

    public PathCalculator(int width, int height, Tilemap map)
    {
        this.map = map;

        grid = new Grid<Waypoint>(width, height, 1f, Vector3.zero, (Grid<Waypoint> g, int x, int y) => new Waypoint(g, x, y) );
    }

    public IEnumerator FindPath(Vector2Int start, Vector2Int end)
    {
        ready = false;
        Waypoint startPoint = grid.GetGridObject(start.x, start.y);
        Waypoint endPoint = grid.GetGridObject(end.x, end.y);


        // About to search list
        openList = new List<Waypoint> { startPoint };
        // Done searching list
        closedList = new List<Waypoint>();

        // Prepare all waypoints
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                Waypoint waypoint = grid.GetGridObject(x, y);
                // Set gCost to infinite to start with and calculate fCost based on that (so tiles are only viable after we calculate them)
                waypoint.gCost = int.MaxValue;
                waypoint.CalculateFCost();
                // Reset previousWaypoint which might be set from previous pathfinding attempts
                waypoint.previousWaypoint = null;
            }
        }

        startPoint.gCost = 0;
        startPoint.hCost = CalculateDistanceCost(startPoint, endPoint);
        startPoint.CalculateFCost();

        while (openList.Count > 0)
        {
            Waypoint currentWaypoint = GetLowestCostWaypoint(openList);

            if (currentWaypoint == endPoint)
            {
                ready = true;
                currentPath = CalculateBackPath(endPoint);
                yield break;
            }

            openList.Remove(currentWaypoint);
            closedList.Add(currentWaypoint);

            foreach (Waypoint neighbor in GetValidEmptyNeighbors(currentWaypoint))
            {
                if (!neighbor.isWalkable)
                    closedList.Add(neighbor);
                if (closedList.Contains(neighbor))
                    continue;

                int tentativeGCost = currentWaypoint.gCost + CalculateDistanceCost(currentWaypoint, neighbor);
                if (tentativeGCost < neighbor.gCost)
                {
                    neighbor.previousWaypoint = currentWaypoint;
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = CalculateDistanceCost(neighbor, endPoint);
                    neighbor.CalculateFCost();

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }

            ready = false;
            currentPath = null;
            yield return null;
        }

        // No nodes left on the open list, entire map is searched, NO PATH
        ready = true;
        currentPath = null;
        yield break;
    }

    private int CalculateDistanceCost(Waypoint a, Waypoint b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);

        return Mathf.Min(xDistance, yDistance) * DIAG_COST + remaining * STRAIGHT_COST;
    }

    private Waypoint GetLowestCostWaypoint(List<Waypoint> waypoints)
    {
        Waypoint lowestCostWaypoint = waypoints[0];
        foreach (Waypoint waypoint in waypoints)
        {
            if (waypoint.fCost < lowestCostWaypoint.fCost)
                lowestCostWaypoint = waypoint;
        }
        return lowestCostWaypoint;
    }

    private List<Waypoint> CalculateBackPath(Waypoint pathEnd)
    {
        // Go through this point to it's previous point to that one's previous point until we reach one with a null previous point (the beginning) and return that list
        List<Waypoint> path = new List<Waypoint>();
        Waypoint currentPoint = pathEnd;

        while (currentPoint.previousWaypoint != null)
        {
            path.Insert(0, currentPoint.previousWaypoint);
            currentPoint = currentPoint.previousWaypoint;
        }

        return path;
    }

    private List<Waypoint> GetValidEmptyNeighbors(Waypoint waypoint)
    {
        List<Waypoint> validNeighbors = new List<Waypoint>();

        for (int x = waypoint.x - 1; x <= waypoint.x + 1; x++)
        {
            for (int y = waypoint.y - 1; y <= waypoint.y + 1; y++)
            {
                // If the waypoint is not the current waypoint and it's in bounds
                if (!(x == waypoint.x && y == waypoint.y) && x >= 0 && y >= 0 && x < grid.GetWidth() && y < grid.GetHeight()) {
                    // If the waypoint is not a horizontal move, make sure the sideways pieces are free too (so the enemy can actually fit
                    if (Vector2Int.Distance(new Vector2Int(waypoint.x, waypoint.y), new Vector2Int(x, y)) < 1 || (grid.GetGridObject(waypoint.x, y).isWalkable && grid.GetGridObject(x, waypoint.y).isWalkable))
                    {
                        validNeighbors.Add(grid.GetGridObject(x, y));
                    }
                }
            }
        }

        return validNeighbors;
    }

    public bool IsPathReady()
    {
        return ready;
    }
    public void PathReceived()
    {
        ready = false;
    }

    public List<Waypoint> Path()
    {
        return currentPath;
    }
}
