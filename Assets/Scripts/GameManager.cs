using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject lower, upper;
    [HideInInspector] public GameObject Lower { get { return lower; } }
    [HideInInspector] public GameObject Upper { get { return upper; } }

    public event Action PlayerDeath;
    public Vector2 bottomLeft, topRight;

    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        lower.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.Left)) Cursor.visible = false;
    }

    public void Die()
    {
        PlayerDeath?.Invoke();
    }
}
