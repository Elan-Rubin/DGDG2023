using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    [SerializeField] private DebugMode debug = DebugMode.None;
    [HideInInspector] public DebugMode Debug { get { return debug; } }
    [SerializeField] private GameObject lower, upper;
    [HideInInspector] public GameObject Lower { get { return lower; } }
    [HideInInspector] public GameObject Upper { get { return upper; } }

    [HideInInspector] public event Action PlayerDeath, PlayerReborn;
    [HideInInspector] public Vector2 BottomLeft, TopRight;

    private int enemiesRemaining;
    [SerializeField] private GameObject health, ammo;
    public GameObject Health { get { return health; } }
    public GameObject Ammo { get { return ammo; } }

    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        if (lower != null) lower.SetActive(false);
    }

    void Update()
    {

    }

    public void Die()
    {
        PlayerDeath?.Invoke();
    }

    public void Reborn(int newHealth)
    {
        var mask = RevivalScript.Instance.Mask;
        PlayerReborn?.Invoke();
        GetComponent<PlayerHealth>().ResetHealth(newHealth);

        var t2 = Lower;
        var t1 = Upper;

        t1.SetActive(true);

        var t1tm = t1.transform.GetChild(0).GetComponent<TilemapRenderer>();
        var t2tm = t2.transform.GetChild(0).GetComponent<TilemapRenderer>();

        t1tm.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        t2tm.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

        mask.SetActive(true);
        mask.transform.localScale = Vector2.zero;

        mask.transform.DOScale(Vector2.one * 50, 1.25f).SetEase(Ease.InCirc);

        var sr = mask.transform.GetChild(0).GetComponent<SpriteRenderer>();
        sr.color = Color.clear;
        var sequence = DOTween.Sequence();
        sequence.Append(sr.DOColor(Color.white, .5f)).OnComplete(() => sr.DOColor(new Color(97 / 255f, 224 / 255f, 135 / 255f), .5f));
    }

    public void GameOver() => StartCoroutine(nameof(GameOverCoroutine));
    private IEnumerator GameOverCoroutine()
    {
        CameraManager.Instance.transform.parent.GetChild(2).GetChild(1).gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void NextLevel() => StartCoroutine(nameof(NextLevelCoroutine));
    private IEnumerator NextLevelCoroutine()
    {
        var go = CameraManager.Instance.transform.parent.GetChild(2).GetChild(1).gameObject;
        go.SetActive(true);
        go.transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex == 8 ? 0 : SceneManager.GetActiveScene().buildIndex + 1); 
    }
}

public enum DebugMode
{
    None,
    Pathfinding,
    CameraMovement,
    Invincible,
}