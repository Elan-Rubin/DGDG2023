using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bulletRenderer;
    private List<int> uids = new();
    private Rigidbody2D rb;
    private bool destroying;
    [HideInInspector] public Vector2 Velocity;
    [SerializeField] private bool enemyBullet;
    [SerializeField] private GameObject particle;
    [SerializeField] private Material whiteMat;

    void Start()
    {
        Physics2D.IgnoreLayerCollision(7, 7);
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (rb == null) return;
        var v = rb.velocity;
        var angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        bulletRenderer.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.layer)
        {
            case 6: //player
                if(!enemyBullet) Physics2D.IgnoreCollision(collision.collider, collision.otherCollider);
                else
                {
                    DestroyBullet();
                    MakeParticle();
                }
                break;
            case 9: //enemy
                if(enemyBullet || GunManager.Instance.SelectedGun.Pierce) Physics2D.IgnoreCollision(collision.collider, collision.otherCollider);
                else
                {
                    DestroyBullet();
                    MakeParticle();
                    collision.rigidbody.AddForce(Velocity * 100f);
                }
                break;
            case 8: //wall
                SoundManager.Instance.PlaySoundEffect("bulletHitWall");
                MakeParticle();
                DestroyBullet();
                break;
        }
    }

    private void DestroyBullet()
    {
        if (destroying) return;
        destroying = true;

        Destroy(GetComponent<Collider2D>());
        Destroy(GetComponent<Rigidbody2D>());

        transform.GetChild(0).GetComponent<SpriteRenderer>().material = whiteMat;
        var sequence = DOTween.Sequence();
        uids.Add(sequence.intId);
        sequence.Append(bulletRenderer.DOColor(Color.white, 0.1f));
        sequence.Append(bulletRenderer.transform.DOScale(Vector2.zero, 0.1f));
        sequence.OnComplete(() => Destroy(gameObject));
    }
    private void MakeParticle()
    {
        var p = Instantiate(particle, transform.position, Quaternion.identity).gameObject;
        //Destroy(p, 1f);
    }

    public void StartLifetime(float time) => StartCoroutine(nameof(StartLifetimeCoroutine), time);
    private IEnumerator StartLifetimeCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        DestroyBullet();
    }
}
