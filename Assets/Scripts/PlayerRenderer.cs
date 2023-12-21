using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerRenderer : MonoBehaviour
{
    private SpriteRenderer playerSprite;
    [HideInInspector] public SpriteRenderer PlayerSprite { get { return playerSprite; } }
    private Animator animator;
    private bool flip;
    public bool Flip { get { return flip; } }

    [SerializeField] private Material whiteMaterial;
    bool flashing;

    private static PlayerRenderer instance;
    public static PlayerRenderer Instance { get { return instance; } }
    [SerializeField] private GameObject playerCorpse;
    [SerializeField] private Transform compass;
    private Vector2 compassTarget;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }

    void Start()
    {
        playerSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        GameManager.Instance.PlayerDeath += SwitchToDead;
        GameManager.Instance.PlayerReborn += SwitchToAlive;
    }

    void Update()
    {
        if (PlayerMovement.Instance.Moving) flip = playerSprite.flipX = PlayerMovement.Instance.PreviousMovment.x < 0;
        else playerSprite.flipX = flip = CameraManager.Instance.MousePos.x < PlayerMovement.Instance.PlayerPosition.x;

        transform.GetChild(1).GetComponent<SpriteRenderer>().flipX = flip;

        animator.SetBool("walking", PlayerMovement.Instance.Moving);

        compass.LookAt((Vector3)compassTarget);
        compass.right = compassTarget - (Vector2)compass.position;
        var dist = Vector2.Distance(compass.position, compassTarget);
        var targetColor = Color.white;
        if (!LevelGenerator.Instance.AllEnemiesDead())
        {
            targetColor = Color.clear;
        }
        else if (dist < 15)
        {
            targetColor = Color.Lerp(Color.clear, Color.white, 0.0067f * Mathf.Pow(dist, 2));
        }
        //else white
        compass.GetChild(0).GetComponent<SpriteRenderer>().color = targetColor;
    }

    public void FlashWhite()
    {
        StartCoroutine(nameof(FlashWhiteCoroutine));
    }
    private IEnumerator FlashWhiteCoroutine()
    {
        if (!flashing)
        {
            flashing = true;
            var mat = playerSprite.material;
            playerSprite.material = whiteMaterial;
            yield return new WaitForSeconds(0.1f);
            playerSprite.material = mat;
            flashing = false;
            PlayerHealth.Instance.CoolingDown = true;
        }
    }

    private void SwitchToDead()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(true);
    }
    private void SwitchToAlive()
    {
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(4).gameObject.SetActive(false);
    }
    public void SpawnCorpse(Vector2 location)
    {
        var p = Instantiate(playerCorpse, location, Quaternion.identity).GetComponent<SpriteRenderer>();
        p.flipX = Random.value > 0.5f;
    }

    public void AssignTarget(Vector2 targetPos)
    {
        compassTarget = targetPos;
    }
}
