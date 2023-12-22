using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static UnityEngine.GraphicsBuffer;

public class Enemy : MonoBehaviour
{
    private int health;
    [SerializeField] private int startHealth = 1;
    [SerializeField] private int regularSpeed = 3;
    [HideInInspector] public int RegularSpeed { get { return regularSpeed; } }
    [SerializeField] private int chargeSpeed = 6;
    [HideInInspector] public int ChargeSpeed { get { return chargeSpeed; } }
    [Range(0,5)]
    [SerializeField] private int distraction = 2;
    private int baseDistraction;
    [HideInInspector] public int Distraction { get { return distraction; } }
    public int Health { get { return health; } }
    private bool playerInMeleeRange;
    private float lastMeleeTime;
    private int lastHealth;
    [SerializeField] private GameObject armorParticle, bloodParticle;
    [SerializeField] private GameObject enemyBullet;
    [SerializeField] private LayerMask ignore;

    [SerializeField] private bool shootBullets = false;
    [SerializeField] private float bulletCooldownBase = 1f;
    [SerializeField] private float meleeCooldown = 1f;
    private Transform bulletPos;
    private float bulletCooldown;
    private CircleCollider2D coll;
    private Rigidbody2D rb;
    public Rigidbody2D Rb { get { return rb; } }
    private bool dontMove;
    private bool targetVisible = false;
    public bool DontMove { get { return dontMove; } set { dontMove = value; } }
    EnemyRenderer enemyRenderer;
    bool ragdoll;
    bool chargingUp;

    private EnemyBehavior eb = EnemyBehavior.Follow;
    public EnemyBehavior Behavior { get { return eb; } }

    void Start()
    {
        baseDistraction = distraction;
        enemyRenderer = GetComponent<EnemyRenderer>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CircleCollider2D>();
        bulletCooldown = bulletCooldownBase;
        bulletPos = transform.GetChild(1);
        health = startHealth;
        bulletCooldown = Random.Range(bulletCooldownBase * 0.5f, bulletCooldownBase * 2f);

        GameManager.Instance.PlayerDeath += PlayerDeadDisappear;
        GameManager.Instance.PlayerReborn += PlayerRebornReappear;
        enemyRenderer.SelectSpriteForHealth();

        InvokeRepeating(nameof(UpdateTargetVisible), 0f, 1f);

        SetSpeed(regularSpeed);
    }

    void Update()
    {
        TryRangedAttackPlayer();
        TryMeleeAttackPlayer();

        GetComponent<AIPath>().enabled = !dontMove;
    }
    IEnumerator ChargingUpCoroutine()
    {
        chargingUp = true;
        yield return new WaitForSeconds(1f);
        if (targetVisible)
        {
            StartCoroutine(nameof(ActivateCharge));
        }
        chargingUp = false;
    }
    IEnumerator ActivateCharge()
    {
        distraction = 0;
        eb = EnemyBehavior.Charge;
        SetSpeed(0);
        yield return new WaitForSeconds(0.25f);
        SetSpeed(chargeSpeed);
        //animation for charge!
        yield return new WaitForSeconds(2f);
        distraction = baseDistraction;
        SetSpeed(regularSpeed);
        yield return new WaitForSeconds(0.25f);
        eb = EnemyBehavior.Follow;
    }
    private void SetSpeed(int speed)
    {
        GetComponent<AIPath>().maxSpeed = speed;
    }
    private void TryMeleeAttackPlayer()
    {
        if (playerInMeleeRange && Time.time - lastMeleeTime > meleeCooldown)
        {
            PlayerHealth.Instance.Damage(1);
            lastMeleeTime = Time.time;
        }
    }
    private void UpdateTargetVisible()
    {
        targetVisible = UpdateTargetVisible(true);
        if (targetVisible && !chargingUp && IsRat()) StartCoroutine(nameof(ChargingUpCoroutine));
    }
    private bool UpdateTargetVisible(bool a)
    {
        var player = PlayerMovement.Instance.PlayerPosition;
        var self = transform.position;

        if (Vector2.Distance(player, self) > 17f)
            eb = EnemyBehavior.Wander;
        else if (!eb.Equals(EnemyBehavior.Charge))
            eb = EnemyBehavior.Follow;

        enemyRenderer.Flip = player.x < self.x;

        var dist = Vector2.Distance(self, player);
        if (dist > 20f) return false;
        if (dist < 2f) return false;

        var pointer = ((Vector2)self - player).normalized;
        var hit = Physics2D.Raycast(self, -pointer, 10, ~ignore);
        return hit.collider == null || hit.collider.gameObject.CompareTag("Player");
    }
    private void TryRangedAttackPlayer()
    {
        if (shootBullets && targetVisible && !dontMove)
            bulletCooldown -= Time.deltaTime;

        if (bulletCooldown <= 0)
        {
            bulletCooldown = bulletCooldownBase;
            //this is copied code should be simplified later
            var spawnPos = (Vector2)transform.position + new Vector2(bulletPos.localPosition.x * (enemyRenderer.Flip ? 1 : -1), bulletPos.localPosition.y);
            var b = Instantiate(enemyBullet, spawnPos, Quaternion.identity).GetComponent<Rigidbody2D>();
            var bullet = b.GetComponent<Bullet>();
            bullet.StartLifetime(1.5f);
            var dif = (PlayerMovement.Instance.PlayerPosition - spawnPos).normalized;
            bullet.Velocity = Quaternion.Euler(0, 0, Random.Range(-45, 45)) * dif.normalized;
            b.AddForce(325 * dif);
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
    public bool IsRat()
    {
        return !shootBullets;
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    private void Death()
    {
        var p = Instantiate(bloodParticle, transform.position, Quaternion.identity);
        //Destroy(p, 20f);

        if (Random.value > 0.9f)
        {
            Instantiate(GameManager.Instance.Health, transform.position, Quaternion.identity);
        }
        else if (Random.value > 0.75f)
        {
            Instantiate(GameManager.Instance.Ammo, transform.position, Quaternion.identity);
        }

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

public enum EnemyBehavior
{
    Still,
    Wander,
    Follow,
    Charge
}
