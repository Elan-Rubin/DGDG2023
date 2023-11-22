using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint
{
    private Grid<Waypoint> grid;
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    public bool isWalkable;

    public Waypoint previousWaypoint;

    public Waypoint(Grid<Waypoint> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        isWalkable = true;
    }

    public void CalculateFCost()
    {
        fCost = hCost + gCost;
    }

    public Vector3Int GetMapPosition()
    {
        return new Vector3Int(x, y, 0);
    }

    public override string ToString()
    {
        return x + "," + y;
    }
}
