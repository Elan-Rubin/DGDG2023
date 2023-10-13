using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerMovement : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private float movementSpeed;
    [Header("References")]
    private Rigidbody2D rigidBody;
    private static PlayerMovement instance;
    [SerializeField] private GameObject bullet;
    public static PlayerMovement Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var b = Instantiate(bullet, GunManager.Instance.GunTip.position, Quaternion.identity).GetComponent<Rigidbody2D>();
            var dif = (CameraManager.Instance.LaggedMousePos - (Vector2)GunManager.Instance.GunTip.position).normalized;
            //bodyRigid.AddForce(dif * multiplier * Time.deltatime);
            b.AddForce(dif * 800);
            CameraManager.Instance.ShakeCamera();
        }
    }
    private void FixedUpdate()
    {
        var movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (movement.magnitude > 1) movement /= movement.magnitude;
        rigidBody.velocity = movement * movementSpeed; 
    }
}
