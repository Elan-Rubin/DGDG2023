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
        int ghostsSpawned = 0;

        // Loop over enemies, check if dead, spawn ghosts
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            PathfinderEnemy enemyBrain = enemy.GetComponent<PathfinderEnemy>();
            if (enemyBrain.IsDead())
            {
                // Spawn ghost based on enemy type
                GameObject newGhost;
                if (enemyBrain.IsSlime())
                    newGhost = Instantiate(slimeGhost, transform);
                else
                    newGhost = Instantiate(ratGhost, transform);
                newGhost.transform.position = enemy.transform.position;
                ghosts.Add(newGhost);
                ghostsSpawned++;
            }
        }

        // Spawn additional ghosts up to ghostsNeeded (maybe +1)
        for (int i = ghostsSpawned; i < RevivalScript.Instance.GhostThreshold * 3; i++)
        {
            GameObject newGhost;
            if (i % 2 == 0)
                newGhost = Instantiate(slimeGhost, transform);
            else
                newGhost = Instantiate(ratGhost, transform);
            newGhost.transform.position = PlayerMovement.Instance.transform.position + new Vector3(Random.Range(-15, 15), Random.Range(-15, 15), 0);
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
