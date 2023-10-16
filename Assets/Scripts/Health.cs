using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Health : MonoBehaviour
{
    [SerializeField] private int playerHealth = 1;
    [HideInInspector] public int PlayerHealth { get { return playerHealth; } }
    private List<GameObject> healthBots = new();
    private List<Vector2> currentPositions = new(), targetPositions = new();
    [SerializeField] private GameObject healthBot;
    float counter;
    private static Health instance;
    [HideInInspector] public static Health Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        for (int i = 0; i < playerHealth; i++) GenerateBot();
    }

    void Update()
    {
        counter += Time.deltaTime;
        var i = 0;
        foreach (var b in healthBots)
        {
            targetPositions[i] = PlayerMovement.Instance.PlayerPosition +
                (0.75f * (i + 2) * (PlayerRenderer.Instance.Flip ? Vector2.right : Vector2.left) +
                (0.35f * Vector2.up * Mathf.Sin((2.5f * counter) + 30 * i)));
            b.transform.position = currentPositions[i] = Vector2.Lerp(currentPositions[i], targetPositions[i], Time.deltaTime * 10f);
            i++;
        }
    }

    public void Damage(int damage)
    {
        playerHealth -= damage;

        if (playerHealth <= 0)
        {
            RevivalScript.Instance.Rewind();
        }
    }

    public void AddHealth()
    {
        playerHealth++;
    }

    private void GenerateBot()
    {
        Vector2 spawnPos = Vector2.zero;
        currentPositions.Add(spawnPos);
        targetPositions.Add(spawnPos);

        var b = Instantiate(healthBot, spawnPos, Quaternion.identity);
        healthBots.Add(b);
        b.transform.SetParent(transform);
    }
}
