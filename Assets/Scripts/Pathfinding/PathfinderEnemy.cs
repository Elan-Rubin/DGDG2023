using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfinderEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap map;
    [SerializeField] private GameObject target;
    [Header("Variables")]
    [SerializeField] private int width = 64;
    [SerializeField] private int height = 64;
    [SerializeField] private float desiredDistanceToTarget = 1f;
    [SerializeField] private float speed = 1f;
    // The pathfinder only recalculates the path every X seconds
    [SerializeField] private float recalculationDelay = 1f;

    private Vector3Int lastTargetCell;
    private Vector3Int lastPathCell;

    private float lastRecalcTime;
    private Rigidbody2D rb;

    List<Waypoint> path = new List<Waypoint>();
    PathCalculator pathCalculator;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        pathCalculator = new PathCalculator(width, height, map);
        SetupGridFromTilemap();
        InvokeRepeating("CalculatePath", 0f, recalculationDelay);
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, target.transform.position) < desiredDistanceToTarget)
            rb.velocity = Vector3.zero;
        else
        {
            if (pathCalculator.IsPathReady())
            {
                List<Waypoint> newPath = pathCalculator.Path();
                if (newPath == null || newPath.Count < 1)
                    return;

                path = newPath;

                if (newPath.Count <= 1)
                    return;

                Vector3 followingWaypointCenter = map.CellToWorld(path[1].GetMapPosition()) + new Vector3(pathCalculator.grid.GetCellSize(), pathCalculator.grid.GetCellSize()) * 0.5f;

                // If the following waypoint is closer than the next one, use it instead
                // (due to the time it takes to recalculate, sometimes we will have already passed the 'next waypoint' and be ready for the one after that)
                if (Vector3.Distance(transform.position, followingWaypointCenter) < 2)
                {
                    path.RemoveAt(0);
                }
                pathCalculator.PathReceived();
            }
            FollowPath();
        }
    }

    private void CalculatePath()
    {
        if (Vector3.Distance(transform.position, target.transform.position) < desiredDistanceToTarget)
            return;

        Vector3Int currentCell = map.WorldToCell(transform.position);
        Vector3Int targetCell = map.WorldToCell(target.transform.position);

        // If this enemy is on the correct path and the target hasn't changed cells
        if (path != null && path.Count > 0 && (currentCell == lastPathCell || currentCell == path[0].GetMapPosition()) && targetCell == lastTargetCell)
            return;

        lastTargetCell = targetCell;

        IEnumerator pathRoutine = pathCalculator.FindPath(new Vector2Int(currentCell.x, currentCell.y), new Vector2Int(targetCell.x, targetCell.y));
        StartCoroutine(pathRoutine);
    }

    private void FollowPath()
    {
        if (path != null && path.Count > 0)
        {
            Vector3 nextWaypointCenter = map.CellToWorld(path[0].GetMapPosition()) + new Vector3(pathCalculator.grid.GetCellSize(), pathCalculator.grid.GetCellSize()) * 0.5f;

            rb.velocity = Vector3.Normalize(nextWaypointCenter - transform.position) * speed;

            if (Vector3.Distance(transform.position, nextWaypointCenter) < desiredDistanceToTarget)
            {
                lastPathCell = path[0].GetMapPosition();
                path.RemoveAt(0);
            }
        }
    }

    private void SetupGridFromTilemap()
    {
        Grid<Waypoint> grid = pathCalculator.grid;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Waypoint currentPoint = grid.GetGridObject(x, y);
                if (map.GetTile(currentPoint.GetMapPosition()) != null)
                {
                    currentPoint.isWalkable = false;
                }
            }
        }
    }
}
