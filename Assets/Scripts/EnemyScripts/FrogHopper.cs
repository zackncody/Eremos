using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FrogHopper : MonoBehaviour, ISlowable
{
    // ── Sprites ──────────────────────────────────────────────────────────
    [Header("Sprites (4-direction)")]
    public SpriteRenderer bodyRenderer;
    public Sprite spriteDown;
    public Sprite spriteUp;
    public Sprite spriteLeft;
    public Sprite spriteRight;

    // ── Hop Settings ─────────────────────────────────────────────────────
    [Header("Hop Settings")]
    public float minHopDistance = 1.5f;
    public float maxHopDistance = 3.5f;
    public float hopDuration = 0.4f;
    public float hopHeight = 0.6f;
    public float minWaitBetweenHops = 1f;
    public float maxWaitBetweenHops = 3f;
    public LayerMask obstacleMask; // optional, leave empty to skip landing checks

    [Header("Roam Settings")]
    public float roamRadius = 8f;

    [Header("Detection Settings")]
    public string playerTag = "Player";
    public float detectionRadius = 8f;
    public bool requireLineOfSight = true;
    public LayerMask obstacleLayers;

    [Header("Chase Settings")]
    public float stopChaseDistance = 3.5f; // sits still (and can tongue-attack) inside this range
    public float loseTargetTime = 3f;

    [Header("Peace Settings")]
    [Tooltip("Tick this if this enemy should stop chasing when the area boss dies.")]
    public bool tiedToBoss = false;

    [Header("Debug")]
    public bool showGizmos = true;

    public bool IsHopping { get; private set; }

    public enum FacingDir { Down, Up, Left, Right }
    public FacingDir CurrentFacing { get; private set; } = FacingDir.Down;

    // ── Private State ───────────────────────────────────────────────────
    private enum State { Roaming, Chasing }
    private State currentState = State.Roaming;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 spawnPoint;

    private float waitTimer;
    private float loseTargetTimer;
    private Coroutine hopRoutine;

    // Knockback
    private bool isKnockedBack;
    private float knockbackTimer;

    // Slow (ISlowable)
    private bool isSlowed;
    private float speedMultiplier = 1f;

    // ── Unity Lifecycle ─────────────────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Kinematic; // movement is fully scripted (hops + knockback)
    }

    void Start()
    {
        spawnPoint = transform.position;
        waitTimer = Random.Range(minWaitBetweenHops, maxWaitBetweenHops);
        UpdateFacingSprite(Vector2.down);

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null) player = playerObj.transform;
        else Debug.LogWarning($"[FrogHopper] No GameObject tagged '{playerTag}' found.");
    }

    void Update()
    {
        if (IsPeaceful())
        {
            currentState = State.Roaming;
        }
        else
        {
            bool canSee = CanSeePlayer();
            if (currentState == State.Roaming && canSee)
            {
                currentState = State.Chasing;
                loseTargetTimer = 0f;
            }
            else if (currentState == State.Chasing)
            {
                if (canSee) loseTargetTimer = 0f;
                else
                {
                    loseTargetTimer += Time.deltaTime;
                    if (loseTargetTimer >= loseTargetTime) currentState = State.Roaming;
                }
            }
        }

        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f) isKnockedBack = false;
            return; // no hopping while being knocked back
        }

        if (IsHopping) return;

        waitTimer -= Time.deltaTime;
        if (waitTimer > 0f) return;

        if (currentState == State.Chasing && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist > stopChaseDistance)
            {
                Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
                // aim to land at half of stopChaseDistance from the player - no artificial
                // minimum floor here, since one that's too high can overshoot past the player
                float desiredRemaining = stopChaseDistance * 0.5f;
                float hopDist = Mathf.Clamp(dist - desiredRemaining, 0.1f, maxHopDistance);
                hopRoutine = StartCoroutine(DoHop(dir, hopDist));
            }
            else
            {
                waitTimer = 0.2f; // sit still and re-check (lets tongue attack fire)
            }
        }
        else
        {
            Vector2 dir = GetRoamDirection();
            float hopDist = Random.Range(minHopDistance, maxHopDistance);
            hopRoutine = StartCoroutine(DoHop(dir, hopDist));
        }
    }

    // ── Peace Check ─────────────────────────────────────────────────────
    private bool IsPeaceful() => tiedToBoss && BossController.IsDead;

    // ── Hop ─────────────────────────────────────────────────────────────
    Vector2 GetRoamDirection()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector2 candidate = (Vector2)transform.position + dir * maxHopDistance;
        if (Vector2.Distance(candidate, spawnPoint) > roamRadius)
            dir = (spawnPoint - (Vector2)transform.position).normalized; // bias back toward spawn
        return dir;
    }

    System.Collections.IEnumerator DoHop(Vector2 dir, float distance)
    {
        if (obstacleMask.value != 0)
        {
            Vector2 landing = (Vector2)transform.position + dir * distance;
            if (Physics2D.OverlapCircle(landing, 0.2f, obstacleMask))
            {
                waitTimer = 0.2f;
                yield break;
            }
        }

        IsHopping = true;
        UpdateFacingSprite(dir);

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + (Vector3)(dir * distance);
        float duration = hopDuration / Mathf.Max(speedMultiplier, 0.01f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float pct = t / duration;

            Vector3 flatPos = Vector3.Lerp(startPos, targetPos, pct);
            float arc = Mathf.Sin(pct * Mathf.PI) * hopHeight;
            transform.position = flatPos + new Vector3(0f, arc, 0f);

            float stretch = Mathf.Sin(pct * Mathf.PI);
            transform.localScale = new Vector3(1f - stretch * 0.15f, 1f + stretch * 0.2f, 1f);

            yield return null;
        }

        transform.position = targetPos;
        transform.localScale = Vector3.one;

        IsHopping = false;
        hopRoutine = null;
        waitTimer = Random.Range(minWaitBetweenHops, maxWaitBetweenHops) / Mathf.Max(speedMultiplier, 0.01f);
    }

    void UpdateFacingSprite(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            CurrentFacing = dir.x > 0 ? FacingDir.Right : FacingDir.Left;
        }
        else
        {
            CurrentFacing = dir.y > 0 ? FacingDir.Up : FacingDir.Down;
        }
        bodyRenderer.sprite = CurrentFacing switch
        {
            FacingDir.Right => spriteRight,
            FacingDir.Left => spriteLeft,
            FacingDir.Up => spriteUp,
            _ => spriteDown,
        };
    }

    // ── Detection ───────────────────────────────────────────────────────
    private bool CanSeePlayer()
    {
        if (player == null || IsPeaceful()) return false;

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

    // ── Knockback ───────────────────────────────────────────────────────
    public void ApplyKnockback(Vector2 force, float duration)
    {
        if (IsPeaceful()) return;
        if (hopRoutine != null) { StopCoroutine(hopRoutine); hopRoutine = null; }
        IsHopping = false;
        transform.localScale = Vector3.one;
        StartCoroutine(KnockbackCoroutine(force, duration));
    }

    System.Collections.IEnumerator KnockbackCoroutine(Vector2 force, float duration)
    {
        isKnockedBack = true;
        knockbackTimer = duration;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position += (Vector3)(force * Time.deltaTime);
            yield return null;
        }
        isKnockedBack = false;
    }

    // ── Slow (ISlowable) ────────────────────────────────────────────────
    public void ApplySlowMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
        isSlowed = true;
    }

    public void RemoveSlowMultiplier()
    {
        speedMultiplier = 1f;
        isSlowed = false;
    }

    // ── Gizmos ──────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        Gizmos.color = currentState == State.Chasing ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)spawnPoint : transform.position, roamRadius);
    }
}