using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class BossController : MonoBehaviour
{
    // ─── Health ────────────────────────────────────────────────────────────────
    [Header("Health")]
    public float maxHealth = 500f;
    private float currentHealth;
    public static bool IsDead = false;

    // ─── Phases ────────────────────────────────────────────────────────────────
    [Header("Phases")]
    [Tooltip("Define each phase in order. Phase 1 should have threshold 1.0, Phase 2 e.g. 0.6, Phase 3 e.g. 0.3")]
    public BossPhaseData[] phases = new BossPhaseData[]
    {
        new BossPhaseData { healthThreshold = 1.0f, moveSpeed = 3f, rangedCooldown = 3f, chargeCooldown = 5f, aoeCooldown = 7f, projectileBurstCount = 5 },
        new BossPhaseData { healthThreshold = 0.6f, moveSpeed = 3.9f, rangedCooldown = 2.4f, chargeCooldown = 4f, aoeCooldown = 5.6f, projectileBurstCount = 7 },
        new BossPhaseData { healthThreshold = 0.3f, moveSpeed = 4.7f, rangedCooldown = 1.7f, chargeCooldown = 2.8f, aoeCooldown = 3.9f, projectileBurstCount = 10, chargeSpeed = 17.5f }
    };

    // ─── Ranged Attack ─────────────────────────────────────────────────────────
    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    // ─── AoE Ground Slam ───────────────────────────────────────────────────────
    [Header("AoE Ground Slam")]
    public GameObject aoeWavePrefab;
    public float aoeRadius = 5f;

    // ─── References ────────────────────────────────────────────────────────────
    [Header("References")]
    public GameObject bossHealthBarUI;
    public Transform playerTransform;

    // ─── Altar ─────────────────────────────────────────────────────────────────
    [Header("Altar")]
    public GameObject altarPrefab;
    public Transform altarSpawnMarker;

    // ─── Private State ─────────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;
    private BossHUD bossHUD;

    private bool isActive = false;
    private bool isDead = false;
    private bool isAttacking = false;

    private float rangedTimer = 0f;
    private float chargeTimer = 0f;
    private float aoeTimer = 0f;

    private int currentPhaseIndex = 0;
    private BossPhaseData CurrentPhase => phases[currentPhaseIndex];

    // ───────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        currentHealth = maxHealth;
    }

    void Start()
    {
        IsDead = false;

        if (playerTransform != null) player = playerTransform;
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (bossHealthBarUI != null)
        {
            bossHUD = bossHealthBarUI.GetComponentInChildren<BossHUD>();
        }

        if (bossHUD == null)
        {
            BossHUD found = FindFirstObjectByType<BossHUD>();
            if (found != null)
            {
                bossHUD = found;
                bossHealthBarUI = found.gameObject;
            }
        }

        if (bossHealthBarUI != null)
            bossHealthBarUI.SetActive(false);

        Debug.Log($"BossController Start — bossHUD found: {bossHUD != null}");
    }

    void Update()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            else return;
        }

        if (!isActive || isDead) return;

        UpdatePhase();
        HandleAttackTimers();

        if (!isAttacking)
        {
            ChasePlayer();
            TrySelectAttack();
        }

        FacePlayer();
    }

    // ─── Activation ────────────────────────────────────────────────────────────

    public void ActivateBoss()
    {
        isActive = true;

        if (bossHealthBarUI != null)
            bossHealthBarUI.SetActive(true);

        if (bossHUD != null)
        {
            bossHUD.Show(maxHealth);
            bossHUD.UpdatePhase(currentPhaseIndex + 1);
        }
    }

    void OnTriggerEnter2D(Collider2D other) { }

    // ─── Phase Management ──────────────────────────────────────────────────────

    void UpdatePhase()
    {
        if (phases == null || phases.Length == 0) return;

        float healthPct = currentHealth / maxHealth;
        int newPhaseIndex = 0;

        for (int i = phases.Length - 1; i >= 0; i--)
        {
            if (healthPct <= phases[i].healthThreshold)
            {
                newPhaseIndex = i;
                break;
            }
        }

        if (newPhaseIndex != currentPhaseIndex)
        {
            currentPhaseIndex = newPhaseIndex;
            anim.SetInteger("phase", currentPhaseIndex + 1);

            if (bossHUD != null)
                bossHUD.UpdatePhase(currentPhaseIndex + 1);

            Debug.Log($"Boss entered Phase {currentPhaseIndex + 1}");
        }
    }

    // ─── Movement ──────────────────────────────────────────────────────────────

    void ChasePlayer()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < CurrentPhase.chaseRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + dir * CurrentPhase.moveSpeed * Time.deltaTime);
            anim.SetBool("isMoving", true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isMoving", false);
        }
    }

    void FacePlayer()
    {
        if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        else
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
    }

    // ─── Attack Timers ─────────────────────────────────────────────────────────

    void HandleAttackTimers()
    {
        rangedTimer -= Time.deltaTime;
        chargeTimer -= Time.deltaTime;
        aoeTimer -= Time.deltaTime;
    }

    void TrySelectAttack()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (aoeTimer <= 0f)
            StartCoroutine(AoEAttack());
        else if (chargeTimer <= 0f && dist <= CurrentPhase.chargeDetectRange)
            StartCoroutine(ChargeAttack());
        else if (rangedTimer <= 0f)
            StartCoroutine(RangedAttack());
    }

    // ─── Ranged Attack ─────────────────────────────────────────────────────────

    IEnumerator RangedAttack()
    {
        isAttacking = true;
        rangedTimer = CurrentPhase.rangedCooldown;
        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("ranged");
        yield return new WaitForSeconds(0.3f);

        float baseAngle = Mathf.Atan2(
            player.position.y - transform.position.y,
            player.position.x - transform.position.x) * Mathf.Rad2Deg;

        int burstCount = CurrentPhase.projectileBurstCount;
        float spread = CurrentPhase.projectileSpreadAngle;

        float step = burstCount > 1 ? spread / (burstCount - 1) : 0f;
        float startAngle = baseAngle - spread * 0.5f;

        for (int i = 0; i < burstCount; i++)
        {
            float angle = startAngle + step * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            if (projectilePrefab != null)
            {
                GameObject proj = Instantiate(projectilePrefab,
                    firePoint != null ? firePoint.position : transform.position,
                    Quaternion.identity);

                Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
                if (projRb != null) projRb.linearVelocity = dir * CurrentPhase.projectileSpeed;

                BossProjectile bp = proj.GetComponent<BossProjectile>();
                if (bp != null) bp.damage = CurrentPhase.projectileDamage;

                Destroy(proj, 5f);
            }

            yield return new WaitForSeconds(0.08f);
        }

        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }

    // ─── Charge Attack ─────────────────────────────────────────────────────────

    IEnumerator ChargeAttack()
    {
        isAttacking = true;
        chargeTimer = CurrentPhase.chargeCooldown;
        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("charge");
        yield return new WaitForSeconds(CurrentPhase.chargeWindupTime);

        Vector2 chargeDir = (player.position - transform.position).normalized;
        float elapsed = 0f;

        while (elapsed < CurrentPhase.chargeDuration)
        {
            rb.linearVelocity = chargeDir * CurrentPhase.chargeSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

    // ─── AoE Ground Slam ───────────────────────────────────────────────────────

    IEnumerator AoEAttack()
    {
        isAttacking = true;
        aoeTimer = CurrentPhase.aoeCooldown;
        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("aoe");
        yield return new WaitForSeconds(CurrentPhase.aoeWindupTime);

        if (aoeWavePrefab != null)
        {
            GameObject wave = Instantiate(aoeWavePrefab, transform.position, Quaternion.identity);
            AoEWave aoeWave = wave.GetComponent<AoEWave>();
            if (aoeWave != null)
            {
                aoeWave.damage = CurrentPhase.aoeDamage;
                aoeWave.initialRadius = CurrentPhase.aoeInitialRadius;
                aoeWave.finalRadius = CurrentPhase.aoeFinalRadius;
                aoeWave.expandSpeed = CurrentPhase.aoeExpandSpeed;
            }
        }

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    // ─── Damage & Death ────────────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (bossHUD != null)
            bossHUD.UpdateHealth(currentHealth);

        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        IsDead = true;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("death");

        if (bossHUD != null) bossHUD.Hide();
        if (bossHealthBarUI != null) bossHealthBarUI.SetActive(false);

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        StartCoroutine(SpawnAltarDelayed());
        Destroy(gameObject, 3f);
    }

    private IEnumerator SpawnAltarDelayed()
    {
        yield return new WaitForSeconds(3f);
        if (altarPrefab == null) yield break;
        Vector3 spawnPos = altarSpawnMarker != null ? altarSpawnMarker.position : transform.position;
        Instantiate(altarPrefab, spawnPos, Quaternion.identity);
    }

    // ─── Contact Damage ────────────────────────────────────────────────────────

    void OnCollisionStay2D(Collision2D col)
    {
        if (!isActive || isDead) return;

        if (col.gameObject.CompareTag("Player"))
        {
            PlayerHealth ph = col.gameObject.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage((int)(CurrentPhase.contactDamage * Time.deltaTime));
        }
    }

    // ─── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);

        if (phases != null && phases.Length > 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, phases[0].chargeDetectRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, phases[0].chaseRange);
        }
    }
}