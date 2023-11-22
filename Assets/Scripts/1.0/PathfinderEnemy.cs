using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfinderEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap map;
    [SerializeField] private GameObject target;
    [HideInInspector] public GameObject Target { get { return target; } }
    
    [Header("Enemy")]
    [SerializeField] private float desiredDistanceToTarget = 1f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private bool pathfindWhenTargetOutOfSight = true;
    [SerializeField] private bool chargeWhenTargetInSight = false;
    
    [Header("Performance")]
    [SerializeField] private int width = 64;
    [SerializeField] private int height = 64;
    // The pathfinder only recalculates the path every X seconds
    [SerializeField] private float recalculationDelay = 1f;
    [SerializeField] private float raycastDelay = 1f;

    private Vector3Int lastTargetCell;
    private Vector3Int lastPathCell;
    private bool targetVisible;
    public bool TargetVisible { get { return targetVisible; } }

    private Rigidbody2D rb;

    List<Waypoint> path = new();
    PathCalculator pathCalculator;

    private Enemy enemy;
    private EnemyRenderer enemyRenderer;

    private float targetSightedTime = 0;

    private Vector3 targetPosition;

    private Vector3Int randomPosToWalkTo = Vector3Int.zero;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        enemyRenderer = GetComponent<EnemyRenderer>();
        rb = enemy.Rb;
        map = GameObject.FindGameObjectWithTag("MainTileMap").GetComponent<Tilemap>();
        target = PlayerMovement.Instance.gameObject;

        pathCalculator = new PathCalculator(width, height, map);

        InvokeRepeating(nameof(CalculatePath), 0f, recalculationDelay);
        if (chargeWhenTargetInSight)
            InvokeRepeating(nameof(IsTargetVisible), 0f, raycastDelay);

        SetupGridFromTilemap();
    }

    void Update()
    {
        enemyRenderer.Flip = target.transform.position.x < transform.position.x;


        if (targetVisible && targetSightedTime == 0)
            targetSightedTime = Time.time;
        else if (!targetVisible)
            targetSightedTime = 0;

        

        if (Vector3.Distance(transform.position, target.transform.position) < 10)
            targetPosition = target.transform.position;
        else
            targetPosition = transform.position + new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 0);

        // If close enough to player
        if (Vector3.Distance(transform.position, target.transform.position) < desiredDistanceToTarget)
        {
            if (!rb.bodyType.Equals(RigidbodyType2D.Static))
                rb.velocity = Vector3.zero;
        }
        // If we can see the player and we're supposed to charge when we see the player
        else if (chargeWhenTargetInSight && targetSightedTime != 0 && Time.time - targetSightedTime > 0.25f && enemy.DontMove == false)
        {
            if (!rb.bodyType.Equals(RigidbodyType2D.Static))
            {
                Vector3 velocityToUse = (target.transform.position - transform.position).normalized;
                // Potentially miss the player slightly by 10 degrees on either side
                velocityToUse = Quaternion.Euler(0, 0, Random.Range(-10, 10)) * velocityToUse;
                rb.velocity = velocityToUse * speed;
            }
        }
        // Otherwise, pathfind if supposed to
        else if (pathfindWhenTargetOutOfSight)
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

    private void IsTargetVisible()
    {
        RaycastHit2D rayToTarget = Physics2D.Raycast(transform.position, target.transform.position - transform.position);
        targetVisible = (rayToTarget.collider == null || rayToTarget.collider.gameObject.CompareTag("Player"));
    }

    private void CalculatePath()
    {
        if (enemy.DontMove)
            return;

        if ((Vector3.Distance(transform.position, targetPosition) < desiredDistanceToTarget || (targetVisible && chargeWhenTargetInSight)))
            return;


        Vector3Int currentCell = map.WorldToCell(transform.position);
        Vector3Int targetCell = map.WorldToCell(targetPosition);

        // If this enemy is on the correct path and the target hasn't changed cells
        if (path != null && path.Count > 0 && (currentCell == lastPathCell || currentCell == path[0].GetMapPosition()) && targetCell == lastTargetCell)
            return;

        lastTargetCell = targetCell;

        if (currentCell == null || targetCell == null)
            return;

        /*Debug.Log("current cell: " + currentCell);
        Debug.Log("target cell: " + targetCell);*/
        if (currentCell == null) Debug.LogError("1!!");
        if (targetCell == null) Debug.LogError("2!!");

        IEnumerator pathRoutine = pathCalculator.FindPath(new Vector2Int(currentCell.x, currentCell.y), new Vector2Int(targetCell.x, targetCell.y));
        if (GameManager.Instance.Debug.Equals(DebugMode.Pathfinding)) Debug.Log($"{currentCell} --> {targetCell}");
        if (pathRoutine != null) StartCoroutine(pathRoutine);
    }

    private void FollowPath()
    {
        if (enemy.DontMove)
            return;

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
