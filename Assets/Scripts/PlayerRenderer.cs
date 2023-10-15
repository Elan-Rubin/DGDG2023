using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    private static PlayerRenderer instance;
    public static PlayerRenderer Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    //[SerializeField] private float gunRotationSpeed = 90;
    
    private SpriteRenderer playerSprite;
    

    void Start()
    {
        playerSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        playerSprite.flipX = CameraManager.Instance.MousePos.x < PlayerMovement.Instance.PlayerPosition.x;
    }

    private void FixedUpdate()
    {

    }
}
