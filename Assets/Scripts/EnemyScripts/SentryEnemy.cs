using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class SentryEnemy : MonoBehaviour
{
    private Animator animator;

    [Header("References")]
    
    [SerializeField] private Transform firePoint;    // child of spriteHead, at mouth tip
    [SerializeField] private GameObject projectilePrefab;

    [Header("Detection")]
    public float detectionRadius = 8f;
    public bool requireLineOfSight = true;
    public LayerMask obstacleLayers;
    public string playerTag = "Player";

    [Header("Shooting")]
    public float fireRate = 2f;
    public float projectileSpeed = 6f;
    public float projectileLifetime = 5f;

    [Header("Peace Settings")]
    [Tooltip("Tick this if this enemy should stop shooting when the area boss dies.")]
    public bool tiedToBoss = false;

    [Header("Debug")]
    public bool showGizmos = true;

    // ── Private State ──────────────────────────────────────────────────────

    private Transform player;
    private float fireTimer = 0f;
    private bool playerInRange = false;

    // ── Unity Lifecycle ────────────────────────────────────────────────────

    private void Start()
    {
        animator = GetComponent<Animator>();
        // Lock Rigidbody2D — worm never moves
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("[WormEnemy] No GameObject tagged '" + playerTag + "' found.");
    }

    private void Update()
    {
        if (player == null) return;
        if (tiedToBoss && BossController.IsDead) return;

        playerInRange = CanSeePlayer();
        if (!playerInRange) return;

        FacePlayer();

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireRate;
        }
    }

    // ── Facing ─────────────────────────────────────────────────────────────

    private void FacePlayer()
    {
        if (animator == null) return;

        Vector2 dir = (Vector2)player.position - (Vector2)transform.position;

        // Snap to the dominant axis so you always get a clean 4-direction result
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            animator.SetFloat("FaceX", dir.x > 0 ? 1f : -1f);
            animator.SetFloat("FaceY", 0f);
        }
        else
        {
            animator.SetFloat("FaceX", 0f);
            animator.SetFloat("FaceY", dir.y > 0 ? 1f : -1f);
        }
    }

    // ── Shooting ───────────────────────────────────────────────────────────

    private void Shoot()
    {
        if (firePoint == null || projectilePrefab == null) return;

        Vector2 dir = ((Vector2)player.position - (Vector2)firePoint.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = dir * projectileSpeed;

        // EnemyProjectile handles its own rotation in Start() from the velocity
        Destroy(proj, projectileLifetime);
    }

    // ── Detection ──────────────────────────────────────────────────────────

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        if (toPlayer.magnitude > detectionRadius) return false;

        if (requireLineOfSight)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized,
                                                  toPlayer.magnitude, obstacleLayers);
            if (hit.collider != null) return false;
        }

        return true;
    }

    // ── Gizmos ─────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = playerInRange ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (firePoint != null && player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(firePoint.position, player.position);
        }
    }
}