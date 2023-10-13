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
        
    }
    private void FixedUpdate()
    {
        var movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (movement.magnitude > 1) movement /= movement.magnitude;
        rigidBody.velocity = movement * movementSpeed; 
    }
}
