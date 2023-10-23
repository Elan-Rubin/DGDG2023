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
    [HideInInspector] public bool Moving;
    public Vector2 currentPos, targetPos;
    float counter;
    private Vector2 previousMovement;
    public Vector2 PreviousMovment { get { return previousMovement; } }

    private List<Vector2> previousPositions = new();
    public List<Vector2> PreviousPositions { get { return previousPositions; } }

    public int PreviousPositionsLength;
    int iterator;

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

        for (int i = 0; i < Health.Instance.PlayerHealth; i++)
        {
            previousPositions.Add((Vector2)transform.position - Vector2.right * (i+1));
        }
    }

    void Update()
    {
        counter += Time.deltaTime;
        if (!CanMove)
        {
            transform.position = currentPos = Vector2.Lerp(currentPos, targetPos, Time.deltaTime * 5f);
        }
    }

    public void SnapPosition(Vector3 newPosition)
    {
        transform.position = currentPos = newPosition;
    }

    private void FixedUpdate()
    {
        PlayerPosition = transform.position;
        if (!CanMove) return;
        if (RevivalScript.Instance!=null && Vector2.Distance(PlayerPosition, RevivalScript.Instance.GetLatest()) > RevivalScript.Instance.MinimumDistance) RevivalScript.Instance.AddPosition(PlayerPosition);
        var movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        previousMovement = movement;
        Moving = movement.magnitude > 0;
        if (Moving && counter > 0.15f)
        {
            SoundManager.Instance.PlaySoundEffect("playerFootstep");
            counter = 0;
        }
        if (movement.magnitude > 1) movement /= movement.magnitude;
        rigidBody.velocity = movement * movementSpeed; 

        if(Vector2.Distance(PlayerPosition, previousPositions[0]) > 1f)
        {
            if (iterator <= 0) iterator = previousPositions.Count - 1;
            else iterator--;

            previousPositions.RemoveAt(previousPositions.Count-1);
            previousPositions.Insert(0, PlayerPosition);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Chest":
                collision.GetComponent<GunChest>().SwitchGun(GunManager.Instance.SelectedGun);
                break;
            case "Key":
                collision.GetComponent<Key>().Collect();
                break;
        }
    }

    public void ForceMovePlayer(Vector2 newPosition)
    {
        //transform.DOMove(newPosition, 0.5f, true);
        targetPos = newPosition;
    }
}
