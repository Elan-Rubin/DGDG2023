using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private CollectibleType type; 

    [SerializeField] private LayerMask ignore;
    void Start()
    {
        
    }

    void Update()
    {
        var player = PlayerMovement.Instance.PlayerPosition;
        var self = transform.position;
        if (Vector2.Distance(self, player) > 10f) return;
        else if (Vector2.Distance(self, player) < 0.5f) Collect();
        
        var pointer = ((Vector2)self - player).normalized;
        var hit = Physics2D.Raycast(self, -pointer, 10, ~ignore);
        transform.position = Vector3.Slerp(self, hit.point, Time.deltaTime * 2.5f);
        transform.position = Vector3.MoveTowards(self, player, Time.deltaTime * 5f);
        //transform.position = Vector2.MoveTowards(transform.position, hit.point, 1f);
        /*if (hit.collider != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, hit.point, Time.deltaTime * 10f);
        }*/
        /*laser.SetPosition(0, gunTip.position);
        laser.SetPosition(1, hit.point);*/
    }
    private void Collect()
    {
        switch (type)
        {
            case CollectibleType.Ammo:
                SoundManager.Instance.PlaySoundEffect("ammo");
                GunManager.Instance.AddAmmo();
                break;
            case CollectibleType.Health:
                SoundManager.Instance.PlaySoundEffect("health");
                PlayerHealth.Instance.AddHealth();
                break;
        }
        Destroy(gameObject);
    }
}
public enum CollectibleType
{
    Ammo,
    Health
}