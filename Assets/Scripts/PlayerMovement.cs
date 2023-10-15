using DG.Tweening;
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
    [HideInInspector] public Vector2 PlayerPosition;
    [HideInInspector] public bool CanMove;
    private Vector2 currentPos, targetPos;
    private static PlayerMovement instance;
    public static PlayerMovement Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        CanMove = true;
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!CanMove)
        {
            transform.position = currentPos = Vector2.Lerp(currentPos, targetPos, Time.deltaTime * 5f);
        }
    }
    private void FixedUpdate()
    {
        PlayerPosition = transform.position;
        if (!CanMove) return;
        if (RevivalScript.Instance!=null && Vector2.Distance(PlayerPosition, RevivalScript.Instance.GetLatest()) > 3f) RevivalScript.Instance.AddPosition(PlayerPosition);
        var movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (movement.magnitude > 1) movement /= movement.magnitude;
        rigidBody.velocity = movement * movementSpeed; 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Chest":
                collision.GetComponent<GunChest>().SwitchGun(GunManager.Instance.SelectedGun);
                break;
        }
    }

    public void ForceMovePlayer(Vector2 newPosition)
    {
        //transform.DOMove(newPosition, 0.5f, true);
        targetPos = newPosition;
    }
}
