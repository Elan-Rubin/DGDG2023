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
    private Vector2 playerPosition;
    [HideInInspector] public Vector2 PlayerPosition { get { return playerPosition; } }
    

    void Start()
    {
        playerSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        playerPosition = transform.position;

        /*var target = CameraManager.Instance.MousePos;
        float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
        gunRotator.rotation = Quaternion.Euler(new Vector3(0, 0, angle));*/


        playerSprite.flipX = CameraManager.Instance.MousePos.x < playerPosition.x;
    }

    private void FixedUpdate()
    {

    }
}
