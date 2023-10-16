using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    private SpriteRenderer playerSprite;
    private Animator animator;

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
        playerSprite.flipX = CameraManager.Instance.MousePos.x < PlayerMovement.Instance.PlayerPosition.x;
        animator.SetBool("walking", PlayerMovement.Instance.Moving);
    }

    private void FixedUpdate()
    {

    }
}
