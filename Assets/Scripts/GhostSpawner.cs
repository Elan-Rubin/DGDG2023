using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    private static GhostSpawner instance;
    public static GhostSpawner Instance { get { return instance; } }

    [SerializeField] private GameObject ratGhost;
    [SerializeField] private GameObject slimeGhost;
    public int ghostsNeeded = 2;
    private int ghostsCaught = 0;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.PlayerDeath += SpawnGhosts;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnGhosts()
    {
        ghostsCaught = 0;
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

                ghostsSpawned++;
            }
        }

        // Spawn additional ghosts up to ghostsNeeded (maybe +1)
        for (int i = ghostsSpawned; i < ghostsNeeded; i++)
        {
            if (i % 2 == 0)
                Instantiate(slimeGhost, transform);
            else
                Instantiate(ratGhost, transform);
        }
    }

    public void CaughtGhost()
    {
        ghostsCaught++;
    }

    public void RevivalPathEnded()
    {
        if (ghostsCaught >= ghostsNeeded)
        {
            GameManager.Instance.Reborn();
        }
        else
        {
            GameManager.Instance.GameOver();
        }
    }
}
