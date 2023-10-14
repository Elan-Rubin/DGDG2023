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
    [Range(1,3)]
    public int GunLevel;
    [Header("Stats")]
    [Range(0,20)]
    public int ShootForce;
    [Range(0,1)]
    public float ReloadTime;
    public GameObject Bullet;
    [Range(0,2)]
    public int Repetitions;
    [Range(0,1)]
    public float DelayBetween;
    [Range(1,10)]
    public int BulletsPerShot;
    [Range(0,90)]
    public float Inaccuracy;
    [Header("Abilities")]
    public bool Laser;
    public bool Machinegun;
    [Header("References")]
    public Sprite GunSprite;
    public Vector2 GunOffset;
    public Vector2 TipOffset;
}
