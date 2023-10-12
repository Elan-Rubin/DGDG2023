using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    private static GunManager instance;
    public static GunManager Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    private Vector2 gunPosition;
    [HideInInspector] public Vector2 GunPosition { get { return gunPosition; } }
    [SerializeField] private GameObject crosshair;
    [SerializeField] private SpriteRenderer gunRenderer;
    [HideInInspector] public SpriteRenderer GunRenderer { get { return gunRenderer; } }
    void Start()
    {

    }

    void Update()
    {
        gunRenderer.flipY = PlayerRenderer.Instance.PlayerPosition.x > gunPosition.x;
        gunPosition = gunRenderer.transform.position;
    }

    private void FixedUpdate()
    {
        crosshair.transform.position = CameraManager.Instance.LaggedMousePos;
    }
}
