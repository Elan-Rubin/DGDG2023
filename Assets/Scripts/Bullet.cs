using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bulletRenderer;
    private List<int> uids = new();

    void Start()
    {
        
    }

    void Update()
    {
        
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
        Destroy(GetComponent<Collider2D>());
        Destroy(GetComponent<Rigidbody2D>());
        var sequence = DOTween.Sequence();
        uids.Add(sequence.intId);
        sequence.Append(bulletRenderer.DOColor(Color.white, 0.1f));
        sequence.Append(bulletRenderer.transform.DOScale(Vector2.zero, 0.1f)).OnComplete(() => {
            foreach (var u in uids) DOTween.Kill(u);
            Destroy(gameObject);
            });
    }

    public void StartLifetime(float time) => StartCoroutine(nameof(StartLifetimeCoroutine), time);
    private IEnumerator StartLifetimeCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        DestroyBullet();
    }
}
