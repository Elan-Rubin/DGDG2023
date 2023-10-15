using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bulletRenderer;
    private List<int> uids = new();
    private Rigidbody2D rb;
    private bool destroying;

    void Start()
    {
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
            case 8: //wall
                DestroyBullet();
                break;
            case 7: //player
                Physics2D.IgnoreCollision(collision.collider, collision.otherCollider);
                break;
        }
    }

    private void DestroyBullet()
    {
        if (destroying) return;
        destroying = true;

        Destroy(GetComponent<Collider2D>());
        Destroy(GetComponent<Rigidbody2D>());
        var sequence = DOTween.Sequence();
        uids.Add(sequence.intId);
        sequence.Append(bulletRenderer.DOColor(Color.white, 0.1f));
        sequence.Append(bulletRenderer.transform.DOScale(Vector2.zero, 0.1f));
        sequence.OnComplete(() => Destroy(gameObject));
    }

    public void StartLifetime(float time) => StartCoroutine(nameof(StartLifetimeCoroutine), time);
    private IEnumerator StartLifetimeCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        DestroyBullet();
    }
}
