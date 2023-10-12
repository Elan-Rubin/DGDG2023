using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private float movementSpeed;
    [Range(0,1)]
    [SerializeField] private float mousePull = 0.4f;
    private static CameraManager instance;
    private Camera cam;
    public static CameraManager Instance { get { return instance; } }
    private Vector2 currentPos, targetPos;
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        cam = GetComponent<Camera>();
        currentPos = targetPos = transform.position;
    }

    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        targetPos = Vector2.Lerp(PlayerMovement.Instance.transform.position, cam.ScreenToWorldPoint(Input.mousePosition), mousePull);
        currentPos = Vector2.Lerp(currentPos, targetPos, 0.01f * movementSpeed);
        transform.position = new Vector3(currentPos.x, currentPos.y, -10);
    }
}
