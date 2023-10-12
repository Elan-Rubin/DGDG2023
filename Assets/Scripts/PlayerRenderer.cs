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
    [SerializeField] private Transform gunRotator;
    //[SerializeField] private float gunRotationSpeed = 90;
    [SerializeField] private SpriteRenderer gunRenderer;
    private SpriteRenderer playerSprite;
    private Vector2 playerPosition;
    [HideInInspector] private Vector2 PlayerPosition { get { return playerPosition; } }
    private Vector2 gunPosition;
    [HideInInspector] public Vector2 GunPosition { get { return gunPosition; } }

    void Start()
    {
        playerSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        playerPosition = transform.position;
        gunPosition = gunRenderer.transform.position;

        /*var target = CameraManager.Instance.MousePos;
        float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
        gunRotator.rotation = Quaternion.Euler(new Vector3(0, 0, angle));*/

        gunRotator.right = Vector3.RotateTowards(gunRotator.position, CameraManager.Instance.LaggedMousePos, Mathf.Infinity, Mathf.Infinity); ;

        gunRenderer.flipY = playerPosition.x > gunPosition.x;
        playerSprite.flipX = CameraManager.Instance.MousePos.x < playerPosition.x;
    }
}
