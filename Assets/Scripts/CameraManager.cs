using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private float movementSpeed;
    [Range(0,1)]
    [SerializeField] private float mousePull = 0.4f;
    [SerializeField] private int mouseLagSpeed = 5;
    private static CameraManager instance;
    private Camera mainCamera;
    [HideInInspector] public Camera MainCamera { get { return mainCamera; } }
    private Vector2 mousePos;
    [HideInInspector] public Vector2 MousePos { get { return mousePos; } }
    private Vector2 laggedMousePos;
    [HideInInspector] public Vector2 LaggedMousePos { get { return laggedMousePos; } }
    public static CameraManager Instance { get { return instance; } }
    private Vector2 currentPos, targetPos;
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        currentPos = targetPos = transform.position;
    }

    void Update()
    {
        laggedMousePos = Vector2.Lerp(laggedMousePos, mousePos, Time.deltaTime * mouseLagSpeed);
    }
    private void FixedUpdate()
    {
        targetPos = Vector2.Lerp(PlayerMovement.Instance.transform.position, mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition), mousePull);
        currentPos = Vector2.Lerp(currentPos, targetPos, 0.01f * movementSpeed);
        transform.position = new Vector3(currentPos.x, currentPos.y, -10);
    }
}
