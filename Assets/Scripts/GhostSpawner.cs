using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    private static GhostSpawner instance;
    public static GhostSpawner Instance { get { return instance; } }

    [SerializeField] private GameObject ratGhost;
    [SerializeField] private GameObject slimeGhost;

    List<GameObject> ghosts = new List<GameObject>();

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }

    void Start()
    {
        GameManager.Instance.PlayerDeath += SpawnGhosts;
        GameManager.Instance.PlayerReborn += DestroyGhosts;
    }

    public void SpawnGhosts()
    {
        RevivalScript.Instance.GhostCounter = 0;
        List<GameObject> deadEnemies = new List<GameObject>(); 
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            //i feel like this could be made a lot simpler
            PathfinderEnemy enemyBrain = enemy.GetComponent<PathfinderEnemy>();
            if (enemyBrain.GetComponent<Enemy>().IsDead())
                deadEnemies.Add(enemy);
        }
        // You have to collect half the enemies you killed, max 8, min 3
        RevivalScript.Instance.GhostThreshold = Mathf.Min(8, Mathf.Max(3, deadEnemies.Count/2));
        int ghostsSpawned = 0;

        // Loop over enemies, check if dead, spawn ghosts
        foreach (GameObject enemy in deadEnemies)
        {
            if (ghostsSpawned > RevivalScript.Instance.GhostThreshold * 2)
                break;

            // Spawn ghost based on enemy type
            GameObject newGhost;
            if (enemy.GetComponent<Enemy>().IsSlime())
                newGhost = Instantiate(slimeGhost, transform);
            else
                newGhost = Instantiate(ratGhost, transform);

            newGhost.transform.position = enemy.transform.position;
            ghosts.Add(newGhost);
            ghostsSpawned++;
        }

        // Spawn additional ghosts up to ghostsNeeded (maybe +1)
        var posList = new List<Vector2>();

        for (int i = ghostsSpawned; i < RevivalScript.Instance.GhostThreshold; i++)
        {
            GameObject newGhost;
            if (i % 2 == 0)
                newGhost = Instantiate(slimeGhost, transform);
            else
                newGhost = Instantiate(ratGhost, transform);
           /* var newPos = Vector2.zero;
            var condition = false;*/
            var newPos = PlayerMovement.Instance.transform.position + new Vector3(Random.Range(-25, 25), Random.Range(-25, 25), 0);

            /*do
            {
                newPos = PlayerMovement.Instance.transform.position + new Vector3(Random.Range(-25, 25), Random.Range(-25, 25), 0);
                foreach (var p in posList)
                {
                    var newc = Vector2.Distance(p, newPos) > 2.5f;
                    if()
                }
            } while (!condition);*/
            newGhost.transform.position = newPos;
            posList.Add(newPos);
        }
    }

    public void DestroyGhosts()
    {
        foreach (GameObject ghost in GameObject.FindGameObjectsWithTag("Ghost"))
        {
            Destroy(ghost);
        }
    }

    public void CaughtGhost()
    {
        RevivalScript.Instance.GhostCounter++;
    }

    public void RevivalPathEnded()
    {
        var rs = RevivalScript.Instance;
        if (rs.GhostCounter >= rs.GhostThreshold)
        {
            GameManager.Instance.Reborn(Mathf.FloorToInt((float)rs.GhostCounter/rs.GhostThreshold));
            foreach (GameObject ghost in ghosts)
            {
                Destroy(ghost);
            }
            ghosts.Clear();
        }
        else
        {
            GameManager.Instance.GameOver();
        }
    }
}
