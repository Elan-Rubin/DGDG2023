using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfinderEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap map;
    [SerializeField] private GameObject target;
    [SerializeField] private Material whiteMaterial;
    [Header("Enemy")]
    [SerializeField] private float desiredDistanceToTarget = 1f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private int startHealth = 1;
    // The pathfinder only recalculates the path every X seconds
    [SerializeField] private bool pathfindWhenTargetOutOfSight = true;
    [SerializeField] private bool chargeWhenTargetInSight = false;
    [SerializeField] private GameObject enemyBullet;
    [SerializeField] private bool shootBullets = false;
    [SerializeField] private float bulletCooldownBase = 1f;
    [SerializeField] private float meleeCooldown = 1f;
    private Transform bulletPos;
    private float bulletCooldown;
    [Header("Performance")]
    [SerializeField] private int width = 64;
    [SerializeField] private int height = 64;
    [SerializeField] private float recalculationDelay = 1f;
    [SerializeField] private float raycastDelay = 1f;

    private Vector3Int lastTargetCell;
    private Vector3Int lastPathCell;
    private bool targetVisible;

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D coll;
    private Rigidbody2D rb;

    List<Waypoint> path = new List<Waypoint>();
    PathCalculator pathCalculator;


    private bool flashing;
    private bool stopPathfinding;
    private bool flip;

    private bool playerInMeleeRange;
    private float lastMeleeTime;

    // TODO: Implement random walk when can't see player
    private Vector3Int randomPosToWalkTo = Vector3Int.zero;
    private int lastHealth;
    private int health;

    // Start is called before the first frame update
    void Start()
    {
        bulletCooldown = bulletCooldownBase;
        bulletPos = transform.GetChild(1);

        health = startHealth;

        GameManager.Instance.PlayerDeath += PlayerDeadDisappear;
        GameManager.Instance.PlayerReborn += PlayerRebornReappear;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        coll = GetComponent<CircleCollider2D>();
        pathCalculator = new PathCalculator(width, height, map);

        InvokeRepeating("CalculatePath", 0f, recalculationDelay);
        if (chargeWhenTargetInSight)
            InvokeRepeating("IsTargetVisible", 0f, raycastDelay);

        SetupGridFromTilemap();
        SelectSpriteForHealth();
    }

    void Update()
    {
        int i = 0;
        foreach (Transform child in transform)
        {
            if (i != 0 && i == health)
            {
                flip = target.transform.position.x < transform.position.x;
                child.gameObject.GetComponent<SpriteRenderer>().flipX = flip;
            }
            i++;
        }

        TryMeleeAttackPlayer();


        if (shootBullets && targetVisible && !stopPathfinding) bulletCooldown -= Time.deltaTime;
        if (bulletCooldown <= 0)
        {
            //this is copied code should be simplified later
            var spawnPos = (Vector2)transform.position + new Vector2(bulletPos.localPosition.x * (flip ? 1 : -1), bulletPos.localPosition.y);
            var b = Instantiate(enemyBullet, spawnPos, Quaternion.identity).GetComponent<Rigidbody2D>();
            var bullet = b.GetComponent<Bullet>();
            bullet.StartLifetime(0.5f);
            var dif = ((Vector2)target.transform.position - spawnPos).normalized;
            bullet.Velocity = dif.normalized;
            b.AddForce(500 * dif);

            bulletCooldown = bulletCooldownBase;
        }

        // If close enough to player
        if (Vector3.Distance(transform.position, target.transform.position) < desiredDistanceToTarget)
            rb.velocity = Vector3.zero;
        // If we can see the player and we're supposed to charge when we see the player
        else if (chargeWhenTargetInSight && targetVisible)
        {
            //Debug.Log("Charging");
            rb.velocity = (target.transform.position - transform.position).normalized * speed;
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
        if (stopPathfinding)
            return;

        if ((Vector3.Distance(transform.position, target.transform.position) < desiredDistanceToTarget || (targetVisible && chargeWhenTargetInSight)))
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

    private void SelectSpriteForHealth()
    {
        int i = 0;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(i == health);
            i++;
        }
    }

    private void FollowPath()
    {
        if (stopPathfinding)
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && !stopPathfinding)
        {
            playerInMeleeRange = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            playerInMeleeRange = false;
        }
    }

    private void TryMeleeAttackPlayer()
    {
        if (playerInMeleeRange && Time.time - lastMeleeTime > meleeCooldown)
        {
            Health.Instance.Damage(1);
            lastMeleeTime = Time.time;
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

    private IEnumerator FlashWhiteCoroutine()
    {
        if (!flashing)
        {
            flashing = true;
            var initialMat = spriteRenderer.material;
            foreach (Transform child in transform)
            {
                SpriteRenderer sr = new SpriteRenderer();
                if (child.gameObject.TryGetComponent<SpriteRenderer>(out sr))
                    sr.material = whiteMaterial;
            }
            yield return new WaitForSeconds(0.1f);
            foreach (Transform child in transform)
            {
                SpriteRenderer sr = new SpriteRenderer();
                if (child.gameObject.TryGetComponent<SpriteRenderer>(out sr))
                    sr.material = initialMat;
            }
            flashing = false;
        }
    }

    public void TakeDamage(int damage = 1)
    {
        health -= damage;
        SelectSpriteForHealth();
        if (health <= 0)
        {
            Death();
        }
        else
            StartCoroutine(nameof(FlashWhiteCoroutine));
    }

    private void Death()
    {
        coll.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        stopPathfinding = true;
    }

    public void PlayerDeadDisappear()
    {
        coll.enabled = false;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        rb.bodyType = RigidbodyType2D.Static;
        stopPathfinding = true;
    }

    public void PlayerRebornReappear()
    {
        SelectSpriteForHealth();
        coll.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        stopPathfinding = false;
    }
}
