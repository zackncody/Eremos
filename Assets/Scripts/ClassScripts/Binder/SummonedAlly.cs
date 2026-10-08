using System;
using UnityEngine;

public class SummonedAlly : MonoBehaviour
{
    // Most recently summoned ally. The HUD timer reads this.
    public static SummonedAlly Current { get; private set; }

    public event Action<SummonedAlly> OnEnded;

    [Header("Facing")]
    [SerializeField] private bool spriteFacesRight = true;
    [SerializeField] private bool flipToFace = true;      // mirror the sprite left/right
    [SerializeField] private bool rotateToFace = false;   // also rotate toward the target (top-down sprites)
    [SerializeField] private float rotationOffset = 0f;
    private SpriteRenderer sr;

    private Transform playerTransform;
    private int damage;
    private float attackRange;
    private float attackInterval;
    private float moveSpeed;
    private float followDistance;
    private float aggroRange;
    private float duration;
    private float extendPerHit;

    private Transform currentTarget;
    private EnemyHealth targetEnemy;
    private BossController targetBoss;
    private float attackTimer = 0f;
    private float lifeTimer = 0f;
    private float nextDebugTime;

    public float RemainingSeconds => duration > 0f ? Mathf.Max(0f, duration - lifeTimer) : Mathf.Infinity;
    public float RemainingNormalized => duration > 0f ? Mathf.Clamp01(1f - lifeTimer / duration) : 1f;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void Init(Transform player, int dmg, float atkRange, float atkInterval,
                      float moveSpd, float followDist, float aggro, float summonDuration,
                      float extendPerHitSeconds = 0f)
    {
        playerTransform = player;
        damage = dmg;
        attackRange = atkRange;
        attackInterval = atkInterval;
        moveSpeed = moveSpd;
        followDistance = followDist;
        aggroRange = aggro;
        duration = summonDuration;
        extendPerHit = extendPerHitSeconds;
        lifeTimer = 0f;
        Current = this;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;   // physics can't shove or spin it
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector2.zero;
#else
            rb.velocity = Vector2.zero;
#endif
            rb.angularVelocity = 0f;
        }

        string scripts = "";
        foreach (MonoBehaviour m in GetComponentsInChildren<MonoBehaviour>())
            scripts += m.GetType().Name + ", ";
        Debug.Log("SummonedAlly scripts on summon: " + scripts);
    }

    private void FaceDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;

        if (flipToFace && sr != null && Mathf.Abs(dir.x) > 0.01f)
            sr.flipX = spriteFacesRight ? dir.x < 0f : dir.x > 0f;

        if (rotateToFace)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        }
    }

    private void OnDestroy()
    {
        if (Current == this) Current = null;
        OnEnded?.Invoke(this);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        if (duration > 0f)
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= duration)
            {
                Despawn();
                return;
            }
        }

        FindTargetIfNeeded();

        if (currentTarget != null)
            HandleCombat();
        else
            FollowPlayer();
    }

    private void FindTargetIfNeeded()
    {
        if (currentTarget != null)
        {
            // Drop the target if it got too far from the player.
            float distFromPlayer = Vector2.Distance(playerTransform.position, currentTarget.position);
            if (distFromPlayer <= aggroRange * 1.5f) return;
            ClearTarget();
        }

        // Scan around the PLAYER, so the summon engages anything threatening you.
        Collider2D[] nearby = Physics2D.OverlapCircleAll(playerTransform.position, aggroRange);
        float closestDist = Mathf.Infinity;

        foreach (Collider2D col in nearby)
        {
            EnemyHealth eh = col.GetComponentInParent<EnemyHealth>();
            if (eh == null) eh = col.GetComponentInChildren<EnemyHealth>();

            BossController boss = null;
            if (eh == null)
            {
                boss = col.GetComponentInParent<BossController>();
                if (boss == null) boss = col.GetComponentInChildren<BossController>();
            }
            if (eh == null && boss == null) continue;

            Transform t = eh != null ? eh.transform : boss.transform;
            float d = Vector2.Distance(transform.position, t.position);
            if (d >= closestDist) continue;

            closestDist = d;
            currentTarget = t;
            targetEnemy = eh;
            targetBoss = boss;
        }

        if (currentTarget != null)
        {
            attackTimer = attackInterval;   // first hit lands as soon as it's in range
            Debug.Log("SummonedAlly: target acquired " + currentTarget.name);
        }
    }

    private void ClearTarget()
    {
        currentTarget = null;
        targetEnemy = null;
        targetBoss = null;
    }

    

    private void DealDamage()
    {
        int before = targetEnemy != null ? targetEnemy.health : -1;

        if (targetEnemy != null) targetEnemy.TakeDamage(damage);
        else if (targetBoss != null) targetBoss.TakeDamage(damage);
        else return;

        GameServices.ReportPlayerDamage(currentTarget, damage);

        // Landing hits refills the timer, never beyond the full duration.
        if (duration > 0f && extendPerHit > 0f)
            lifeTimer = Mathf.Max(0f, lifeTimer - extendPerHit);

        int after = targetEnemy != null ? targetEnemy.health : -1;
        Debug.Log($"SummonedAlly: hit {currentTarget.name} for {damage} (health {before} -> {after})");
    }

    private void HandleCombat()
    {
        Vector2 toTarget = currentTarget.position - transform.position;
        FaceDirection(toTarget);

        if (toTarget.magnitude > attackRange)
        {
            transform.position += (Vector3)(toTarget.normalized * moveSpeed * Time.deltaTime);
        }
        else
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                DealDamage();
            }
        }
    }

    private void FollowPlayer()
    {
        Vector2 toPlayer = playerTransform.position - transform.position;
        float dist = toPlayer.magnitude;
        if (dist <= followDistance) return;

        FaceDirection(toPlayer);
        float speed = dist > followDistance * 3f ? moveSpeed * 2f : moveSpeed;   // catch up when far
        transform.position += (Vector3)(toPlayer.normalized * speed * Time.deltaTime);
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }
}