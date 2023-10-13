using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "GunData")]
public class GunData : ScriptableObject
{
    /*[SerializeField] private string gunName;
    [HideInInspector] public string GunName { get { return gunName; } }

    [SerializeField] private float reloadTime;
    [HideInInspector] public float ReloadTime { get { return reloadTime; } }
    [SerializeField] private GameObject bullet;
    [HideInInspector] public GameObject Bullet { get { return bullet; } }

    [SerializeField] private Sprite gunSprite;
    [HideInInspector] public Sprite GunSprite { get { return gunSprite; } }
    [SerializeField] private Vector2 gunOffset;
    [HideInInspector] public Vector2 GunOffset { get {  return gunOffset; } }
    [SerializeField] private Vector2 tipOffset;
    [HideInInspector] public Vector2 TipOffset { get { return tipOffset; } }
    [SerializeField] private int gunLevel;
    [HideInInspector] public int GunLevel { get { return gunLevel; } }*/
    [Header("Information")]
    public string GunName;
    public int GunLevel;
    [Header("Stats")]
    public float ReloadTime;
    public GameObject Bullet;
    public int Repetitions;
    public float DelayBetween;
    public int BulletsPerShot;
    public float Inaccuracy;
    [Header("Abilities")]
    public bool Laser;
    [Header("References")]
    public Sprite GunSprite;
    public Vector2 GunOffset;
    public Vector2 TipOffset;
}
