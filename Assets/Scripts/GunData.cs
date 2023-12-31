using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "GunData")]
public class GunData : ScriptableObject
{
    [Header("Information")]
    public string GunName;
    public string GunDescription;
    public string SoundName;

    public Color BulletColor = Color.white;
    public Color GunColor = Color.white;

    [Header("Stats")]
    [Range(0,35)]
    public int ShootForce;
    public int ShootKnockback;
    [Range(0,1)]
    public float ReloadTime;
    public GameObject Bullet;
    [Range(0,2)]
    public int Repetitions;
    [Range(0,1)]
    public float DelayBetween;
    [Range(1,10)]
    public int BulletsPerShot;
    [Range(0, 1.5f)]
    public float BulletLifeTime;
    public bool LifetimeVariation;
    [Range(0,90)]
    public float Inaccuracy;
    [Header("Ammo")]
    [HideInInspector] public int Ammo = 100;
    [HideInInspector] public int MaxAmmo = 100;
    [Header("Abilities")]
    public bool Laser;
    public bool Machinegun;
    public bool Pierce;
    public bool PierceWall;
    [Header("References")]
    public Sprite GunSprite;
    public Vector2 GunOffset;
    public Vector2 TipOffset;
}
