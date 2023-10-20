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
        StartCoroutine(nameof(LateStart));
    }

    private IEnumerator LateStart()
    {
        yield return null;
        for (int i = 0; i < playerHealth; i++)
        {
            currentPositions[i] = targetPositions[i];
        }
    }

    void LateUpdate()
    {
        counter += Time.deltaTime;
        var i = 0;
        foreach (var b in healthBots)
        {
            targetPositions[i] = PlayerMovement.Instance.PreviousPositions[i] +
                /*(0.75f * (i + 2) * (PlayerRenderer.Instance.Flip ? Vector2.right : Vector2.left) +*/
                (0.35f * Vector2.up * Mathf.Sin((2.5f * counter) + 30 * i));
            if (Vector2.Distance(currentPositions[i],PlayerMovement.Instance.PlayerPosition) > 0.75f) b.transform.position = currentPositions[i] = Vector2.Lerp(currentPositions[i], targetPositions[i], Time.deltaTime * 2f);
            b.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = currentPositions[i].x > targetPositions[i].x;
            i++;
        }
    }

    public void Damage(int damage)
    {
        PlayerRenderer.Instance.FlashWhite();

        playerHealth -= damage;

        var lastBot = healthBots[healthBots.Count - 1];
        healthBots.Remove(lastBot);
        Destroy(lastBot);
        currentPositions.RemoveAt(currentPositions.Count - 1);
        targetPositions.RemoveAt(targetPositions.Count - 1);

        if (playerHealth <= 0)
        {
            GameManager.Instance.Die();
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
        b.GetComponent<Animator>().speed = Random.Range(0.8f, 1.2f);
    }
}
