using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    private SpriteRenderer playerSprite;
    private Animator animator;
    private bool flip;
    public bool Flip { get { return flip; } }

    [SerializeField] private Material whiteMaterial;
    bool flashing;

    private static PlayerRenderer instance;
    public static PlayerRenderer Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }

    void Start()
    {
        playerSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (PlayerMovement.Instance.Moving) flip = playerSprite.flipX = PlayerMovement.Instance.PreviousMovment.x < 0;
        else playerSprite.flipX = flip = CameraManager.Instance.MousePos.x < PlayerMovement.Instance.PlayerPosition.x;

        animator.SetBool("walking", PlayerMovement.Instance.Moving);
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
        }
    }
}
