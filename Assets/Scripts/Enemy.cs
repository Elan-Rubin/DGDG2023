using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Enemy : MonoBehaviour
{
    private int health;
    [SerializeField] private int startHealth = 1;

    public int Health { get { return health; } }
    private bool playerInMeleeRange;
    private float lastMeleeTime;
    private int lastHealth;
    [SerializeField] private GameObject armorParticle, bloodParticle; 
    [SerializeField] private GameObject enemyBullet;
    [SerializeField] private bool shootBullets = false;
    [SerializeField] private float bulletCooldownBase = 1f;
    [SerializeField] private float meleeCooldown = 1f;
    private Transform bulletPos;
    private float bulletCooldown;
    private CircleCollider2D coll;
    private Rigidbody2D rb;
    public Rigidbody2D Rb { get { return rb; } }
    private bool dontMove;
    public bool DontMove { get { return dontMove; } set { dontMove = value; } }
    EnemyRenderer enemyRenderer;
    bool ragdoll;

    void Start()
    {
        enemyRenderer = GetComponent<EnemyRenderer>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CircleCollider2D>();
        bulletCooldown = bulletCooldownBase;
        bulletPos = transform.GetChild(1);
        health = startHealth;
        GameManager.Instance.PlayerDeath += PlayerDeadDisappear;
        GameManager.Instance.PlayerReborn += PlayerRebornReappear;
        enemyRenderer.SelectSpriteForHealth();

    }

    void Update()
    {
        TryRangedAttackPlayer();
        TryMeleeAttackPlayer();
    }

    private void TryMeleeAttackPlayer()
    {
        if (playerInMeleeRange && Time.time - lastMeleeTime > meleeCooldown)
        {
            PlayerHealth.Instance.Damage(1);
            lastMeleeTime = Time.time;
        }
    }

    private void TryRangedAttackPlayer()
    {
        var p = GetComponent<PathfinderEnemy>();
        if (shootBullets && p.TargetVisible && !dontMove)
            bulletCooldown -= Time.deltaTime;

        if (bulletCooldown <= 0)
        {
            //this is copied code should be simplified later
            var spawnPos = (Vector2)transform.position + new Vector2(bulletPos.localPosition.x * (enemyRenderer.Flip ? 1 : -1), bulletPos.localPosition.y);
            var b = Instantiate(enemyBullet, spawnPos, Quaternion.identity).GetComponent<Rigidbody2D>();
            var bullet = b.GetComponent<Bullet>();
            bullet.StartLifetime(0.5f);
            var dif = ((Vector2)GetComponent<PathfinderEnemy>().Target.transform.position - spawnPos).normalized;
            bullet.Velocity = Quaternion.Euler(0, 0, Random.Range(-45, 45)) * dif.normalized;
            b.AddForce(500 * dif);

            bulletCooldown = bulletCooldownBase;
        }
    }

    public void TakeDamage(int damage = 1)
    {
        SoundManager.Instance.PlaySoundEffect("enemydamage");
        health -= damage;
        enemyRenderer.SelectSpriteForHealth();
        if (health <= 0)
        {
            Death();
        }
        else
        {
            StartCoroutine(enemyRenderer.FlashWhiteCoroutine());
            var p = Instantiate(armorParticle, transform.position, Quaternion.identity);
            Destroy(p, 1f);
        }
    }

    public bool IsSlime()
    {
        return shootBullets;
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    private void Death()
    {
        var p = Instantiate(bloodParticle, transform.position, Quaternion.identity);
        //Destroy(p, 20f);

        var dir = (Vector2)transform.position - PlayerMovement.Instance.PlayerPosition;
        rb.AddForce(dir.normalized * 50);
        ragdoll = true;
        dontMove = true;
        StartCoroutine(nameof(RestBody));
    }
    IEnumerator RestBody()
    {
        yield return new WaitForSeconds(0.25f);
        coll.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void PlayerDeadDisappear()
    {
        coll.enabled = false;
        if (IsDead()) enemyRenderer.ToggleCorpseSprite(false);
        else
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        rb.bodyType = RigidbodyType2D.Static;
        dontMove = true;
    }

    public void PlayerRebornReappear()
    {
        if (IsDead()) enemyRenderer.ToggleCorpseSprite(true);
        else
        {
            dontMove = false;
            coll.enabled = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            enemyRenderer.SelectSpriteForHealth();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && !dontMove)
        {
            if (ragdoll) Physics2D.IgnoreCollision(collision.collider, collision.otherCollider);

            playerInMeleeRange = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (ragdoll) Physics2D.IgnoreCollision(collision.collider, collision.otherCollider);

            playerInMeleeRange = false;
        }
    }
}
