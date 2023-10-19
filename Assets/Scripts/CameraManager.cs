using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class CameraManager : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private float movementSpeed;
    [Range(0,1)]
    [SerializeField] private float mousePull = 0.4f;
    [SerializeField] private int mouseLagSpeed = 5;
    private static CameraManager instance;
    private Camera mainCamera;
    private Transform cameraContainer;
    [HideInInspector] public Camera MainCamera { get { return mainCamera; } }
    private Vector2 mousePos;
    [HideInInspector] public Vector2 MousePos { get { return mousePos; } }
    private Vector2 laggedMousePos;
    [HideInInspector] public Vector2 LaggedMousePos { get { return laggedMousePos; } }
    public static CameraManager Instance { get { return instance; } }
    private Vector2 currentPos, targetPos;
    private Vector2 bottomLeft, topRight;
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        currentPos = targetPos = transform.position;
        cameraContainer = transform.parent;
        bottomLeft = GameManager.Instance.bottomLeft;
        topRight = GameManager.Instance.topRight;
    }

    void Update()
    {
        laggedMousePos = Vector2.Lerp(laggedMousePos, mousePos, Time.deltaTime * mouseLagSpeed);
    }
    private void FixedUpdate()
    {
        var size = GetComponent<Camera>().orthographicSize;

        targetPos = Vector2.Lerp(PlayerMovement.Instance.transform.position, mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition), mousePull);

        if (targetPos.x < bottomLeft.x + size) targetPos.x = bottomLeft.x + size;
        else if(targetPos.x > topRight.x - size) targetPos.x = topRight.x - size;
        if(targetPos.y < bottomLeft.y + size) targetPos.y = bottomLeft.y + size;
        else if(targetPos.y > topRight.y - size) targetPos.y = topRight.y - size;

        currentPos = Vector2.Lerp(currentPos, targetPos, 0.01f * movementSpeed);
        cameraContainer.position = new Vector3(currentPos.x, currentPos.y, -10);
    }

    public void ShakeCamera()
    {
        transform.DOShakePosition(0.15f, 0.25f, 50);
    }
}
