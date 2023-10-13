using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

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
    [SerializeField] private Transform gunRotator;
    [SerializeField] private Transform gunTip;
    [HideInInspector] public Transform GunTip { get { return gunTip; } }

    [HideInInspector] public SpriteRenderer GunRenderer { get { return gunRenderer; } }
    void Start()
    {
    }

    void Update()
    {
        gunRenderer.flipY = PlayerRenderer.Instance.PlayerPosition.x > gunPosition.x;
        gunPosition = gunRenderer.transform.position;
        if (Vector2.Distance(CameraManager.Instance.LaggedMousePos, gunPosition) < 1) return;
        //Vector3 relativePos = CameraManager.Instance.LaggedMousePos - PlayerRenderer.Instance.PlayerPosition;
        //Quaternion rotation = Quaternion.LookRotation(relativePos);
        //Debug.Log(Quaternion.ToEulerAngles(rotation));
        //Debug.Log($"{gunPosition},{CameraManager.Instance.LaggedMousePos}");
        var test = Mathf.Atan2(gunPosition.y - CameraManager.Instance.LaggedMousePos.y, gunPosition.x - CameraManager.Instance.LaggedMousePos.x);
        //Debug.Log($"Degree: #{Mathf.Rad2Deg*test}");

        gunRotator.rotation = Quaternion.Euler(new Vector3(0, 0, 180f + Mathf.Rad2Deg * test));
        //rotation.x = 0; rotation.y = 0;
        //gunRotator.rotation = rotation;
        //var angle = Vector3.SignedAngle(gunRotator.position, CameraManager.Instance.LaggedMousePos, transform.forward);
        //gunRotator.Rotate(0.0f, 0.0f, angle);
        //gunRotator.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));

        /*Vector3 targetDirection = CameraManager.Instance.LaggedMousePos - gunPosition;
        Vector3 newDirection = Vector3.RotateTowards(gunRotator.transform.forward, targetDirection, Time.deltaTime, 0.0f);
        gunRotator.rotation = Quaternion.LookRotation(newDirection);*/

        //gunRotator.right = Vector3.RotateTowards(gunRotator.position, CameraManager.Instance.LaggedMousePos, 100f, Mathf.Infinity);
    }

    private void FixedUpdate()
    {
        crosshair.transform.position = CameraManager.Instance.LaggedMousePos;
    }
}
