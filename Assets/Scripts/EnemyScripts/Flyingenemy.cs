using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyPatrol))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class FlyingEnemy : MonoBehaviour
{
    [Header("Hover & Shoot")]
    [Tooltip("How often the flying enemy stops to hover and shoot (seconds)")]
    public float shootRange = 10f;

    [Tooltip("How often the flying enemy stops to hover and shoot (seconds)")]
    public float hoverInterval = 3f;

    [Tooltip("How long the flying enemy hovers before moving again")]
    public float hoverDuration = 1.5f;

    [Tooltip("How long after hover starts before the projectile fires")]
    public float shootDelay = 0.6f;

    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 6f;
    public float projectileDamage = 10f;

    [Header("Animation Parameters")]
    [Tooltip("Animator bool set true while hovering")]
    public string hoverParam = "isHovering";

    [Tooltip("Animator trigger fired when shooting")]
    public string shootParam = "shoot";

    [Tooltip("Animator float for horizontal direction (-1 left, 1 right)")]
    public string facingXParam = "facingX";

    [Tooltip("Animator float for vertical direction (-1 down, 1 up)")]
    public string facingYParam = "facingY";

    [Tooltip("Animator bool for whether the flying enemy is moving")]
    public string movingParam = "isMoving";

    private Rigidbody2D rb;
    private Animator anim;
    private EnemyPatrol patrol;
    private Transform player;

    private bool isHovering = false;
    private float hoverTimer = 0f;

    private Collider2D contactCollider;  // non-trigger, disabled during hover
    private Collider2D hitCollider;      // trigger, always on so weapon can hit

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        patrol = GetComponent<EnemyPatrol>();

        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            if (col.isTrigger) hitCollider = col;
            else contactCollider = col;
        }
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        hoverTimer = Random.Range(0f, hoverInterval);
    }

    void Update()
    {
        if (isHovering) return;

        hoverTimer -= Time.deltaTime;
        if (hoverTimer <= 0f)
        {
            if (player != null && Vector2.Distance(transform.position, player.position) <= shootRange)
                StartCoroutine(HoverAndShoot());
            else
                hoverTimer = hoverInterval;
            return;
        }

        UpdateDirectionAnimation();
    }

    void UpdateDirectionAnimation()
    {
        Vector2 vel = rb.linearVelocity;
        bool moving = vel.magnitude > 0.1f;

        anim.SetBool(movingParam, moving);

        if (!moving) return;

        Vector2 dir = vel.normalized;

        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
        {
            anim.SetFloat(facingXParam, dir.x > 0 ? 1f : -1f);
            anim.SetFloat(facingYParam, 0f);
        }
        else
        {
            anim.SetFloat(facingXParam, 0f);
            anim.SetFloat(facingYParam, dir.y > 0 ? 1f : -1f);
        }
    }

    IEnumerator HoverAndShoot()
    {
        isHovering = true;
        hoverTimer = hoverInterval;
        rb.linearVelocity = Vector2.zero;

        if (contactCollider != null) contactCollider.enabled = false;

        anim.SetBool(hoverParam, true);
        anim.SetBool(movingParam, false);

        float elapsed = 0f;
        while (elapsed < hoverDuration)
        {
            rb.linearVelocity = Vector2.zero;
            elapsed += Time.deltaTime;

            if (elapsed >= shootDelay && elapsed - Time.deltaTime < shootDelay)
                FireProjectile();

            yield return null;
        }

        anim.SetBool(hoverParam, false);
        if (contactCollider != null) contactCollider.enabled = true;
        isHovering = false;
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || player == null) return;

        anim.SetTrigger(shootParam);

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Vector2 dir = (player.position - spawnPos).normalized;

        Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
        if (projRb != null) projRb.linearVelocity = dir * projectileSpeed;

        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null) ep.damage = projectileDamage;

        BossProjectile bp = proj.GetComponent<BossProjectile>();
        if (bp != null) bp.damage = projectileDamage;

        Destroy(proj, 5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);

        if (firePoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(firePoint.position, 0.15f);
        }
    }
    public void ApplyExternalKnockback(Vector2 force, float duration)
    {
        if (isHovering)
        {
            StopAllCoroutines();
            anim.SetBool(hoverParam, false);
            if (contactCollider != null) contactCollider.enabled = true;
            isHovering = false;
            hoverTimer = hoverInterval;
        }
        EnemyPatrol patrol = GetComponent<EnemyPatrol>();
        if (patrol != null) patrol.ApplyKnockback(force, duration);
    }
}