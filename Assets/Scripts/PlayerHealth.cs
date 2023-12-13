using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int health = 1;
    [HideInInspector] public int Health { get { return health; } }
    private List<GameObject> healthBots = new();
    private List<Vector2> currentPositions = new(), targetPositions = new();
    [SerializeField] private GameObject healthBot;
    float counter;
    private static PlayerHealth instance;
    [HideInInspector] public static PlayerHealth Instance { get { return instance; } }

    private int highestHealth;

    private int startingHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        SetupHealth();
        highestHealth = startingHealth = health;
        StartCoroutine(nameof(LateStart));
    }
    private void Update()
    {
        //this should not be in the update
        UIManager.Instance.HealthSlider.value = health;
        UIManager.Instance.HealthSlider.maxValue = highestHealth;
        UIManager.Instance.HealthText.text = $"{health} of {highestHealth}";
    }
    public void ResetHealth() => ResetHealth(startingHealth);
    public void ResetHealth(int newHealth)
    {
        health = newHealth;
        SetupHealth();
    }

    private void SetupHealth()
    {
        for (int i = 0; i < health; i++) GenerateBot();
    }

    private IEnumerator LateStart()
    {
        yield return null;
        for (int i = 0; i < health; i++)
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
        SoundManager.Instance.PlaySoundEffect("playerdamage");

        PlayerRenderer.Instance.FlashWhite();

        health -= damage;

        var lastBot = healthBots[healthBots.Count - 1];
        healthBots.Remove(lastBot);
        Destroy(lastBot);
        currentPositions.RemoveAt(currentPositions.Count - 1);
        targetPositions.RemoveAt(targetPositions.Count - 1);

        if (health <= 0)
        {
            GameManager.Instance.Die();
            PlayerRenderer.Instance.SpawnCorpse(PlayerMovement.Instance.PlayerPosition);
        }
    }

    public void AddHealth()
    {
        health++;
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
