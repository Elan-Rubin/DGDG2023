using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.U2D;
using UnityEngine.Windows.Speech;

public class CameraManager : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private GameObject crosshair;
    [HideInInspector] public GameObject Crosshair { get { return crosshair; } }

    [SerializeField] private bool menuCamera; 
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
    [SerializeField] private Sprite crosshair1, crosshair2;
    [SerializeField] private Color color1, color2;
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        if(menuCamera) crosshair.SetActive(false);
        Cursor.visible = false;

        mainCamera = GetComponent<Camera>();
        currentPos = targetPos = transform.position;
        cameraContainer = transform.parent;

        var pp = GetComponent<PixelPerfectCamera>();
        var ratio1 = pp.refResolutionX / pp.refResolutionY;
        var ratio2 = pp.refResolutionY / pp.refResolutionX;
        if (menuCamera) return;
        GetComponent<PostProcessVolume>().isGlobal = true;
        bottomLeft = GameManager.Instance.bottomLeft + Vector2.right * ratio1 + Vector2.up * ratio2;
        topRight = GameManager.Instance.topRight - Vector2.right * ratio1 - Vector2.up * ratio2;

        /*GameManager.Instance.PlayerDeath += SwitchToDead;
        GameManager.Instance.PlayerReborn += SwitchToAlive;*/
    }
    private void LateUpdate()
    {
        Cursor.visible = false;
        var sr = crosshair.GetComponent<SpriteRenderer>();
        if(!menuCamera) sr.sprite = RevivalScript.Instance.Dead ? crosshair2 : crosshair1;
        if(!menuCamera) sr.color = RevivalScript.Instance.Dead ? color2 : color1;
    }
    void Update()
    {
        laggedMousePos = Vector2.Lerp(laggedMousePos, mousePos, Time.deltaTime * mouseLagSpeed);
        crosshair.transform.position = laggedMousePos;

    }
    private void FixedUpdate()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.Left))
        {
            crosshair.SetActive(true);
            Cursor.visible = false;
        }

        var size = GetComponent<Camera>().orthographicSize;

        if (!menuCamera) targetPos = Vector2.Lerp(PlayerMovement.Instance.transform.position, mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition), mousePull);
        else targetPos = Vector2.Lerp(Vector2.zero, mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition), mousePull);

        var pp = GetComponent<PixelPerfectCamera>();
        var ratio = pp.refResolutionX / pp.refResolutionY;

        if (!menuCamera)
        {
            if (targetPos.x < bottomLeft.x + size) targetPos.x = bottomLeft.x + (size / ratio);
            else if (targetPos.x > topRight.x - size) targetPos.x = topRight.x - (size / ratio);
            if (targetPos.y < bottomLeft.y + size) targetPos.y = bottomLeft.y + size;
            else if (targetPos.y > topRight.y - size) targetPos.y = topRight.y - size;
        }

        currentPos = Vector2.Lerp(currentPos, targetPos, 0.01f * movementSpeed);
        cameraContainer.position = new Vector3(currentPos.x, currentPos.y, -10);
    }

    public void ShakeCamera()
    {
        transform.DOShakePosition(0.15f, 0.25f, 50);
    }
}
