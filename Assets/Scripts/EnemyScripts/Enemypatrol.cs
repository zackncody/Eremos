using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour, ISlowable
{
    // ── Inspector ──────────────────────────────────────────────────────────

    [Header("Roam Settings")]
    public float roamRadius = 8f;
    public float roamWaitTime = 2f;
    public float roamSpeed = 2f;

    [Header("Detection Settings")]
    public string playerTag = "Player";
    public float detectionRadius = 8f;
    public bool requireLineOfSight = true;
    public LayerMask obstacleLayers;

    [Header("Chase Settings")]
    public float chaseSpeed = 4f;
    public float loseTargetTime = 3f;
    public float pathRefreshRate = 0.25f;
    public float waypointTolerance = 0.15f;

    [Header("Peace Settings")]
    [Tooltip("Tick this if this enemy should stop chasing when the area boss dies.")]
    public bool tiedToBoss = false;

    [Header("Debug")]
    public bool showGizmos = true;

    // ── Private State ──────────────────────────────────────────────────────

    private enum State { Roaming, Chasing }
    private State currentState = State.Roaming;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;

    private Vector2 spawnPoint;
    private List<Vector2> currentPath = new List<Vector2>();
    private int pathIndex = 0;

    private float loseTargetTimer = 0f;
    private float pathRefreshTimer = 0f;
    private bool isWaiting = false;

    // ── Knockback State ────────────────────────────────────────────────────

    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;

    // ── Slow State (ISlowable) ─────────────────────────────────────────────

    private bool isSlowed = false;
    private float baseRoamSpeed = -1f;
    private float baseChaseSpeed = -1f;

    // ── Unity Lifecycle ────────────────────────────────────────────────────


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
    }

    private void Start()
    {

        spawnPoint = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"[EnemyPatrol] No GameObject tagged '{playerTag}' found.");

        StartCoroutine(RoamCoroutine());
    }

    private void Update()
    {
        if (IsPeaceful())
        {
            if (currentState == State.Chasing)
            {
                StopAllCoroutines();
                currentState = State.Roaming;
                currentPath.Clear();
                pathIndex = 0;
                rb.linearVelocity = Vector2.zero;
                StartCoroutine(RoamCoroutine());
            }
            MoveAlongPath();
            UpdateAnimator();
            return;
        }

        bool canSee = CanSeePlayer();

        switch (currentState)
        {
            case State.Roaming:
                if (canSee) EnterChase();
                break;

            case State.Chasing:
                if (canSee)
                {
                    loseTargetTimer = 0f;
                    pathRefreshTimer -= Time.deltaTime;
                    if (pathRefreshTimer <= 0f)
                    {
                        pathRefreshTimer = pathRefreshRate;
                        RefreshChasePath();
                    }
                }
                else
                {
                    loseTargetTimer += Time.deltaTime;
                    if (loseTargetTimer >= loseTargetTime) EnterRoam();
                }
                break;
        }

        MoveAlongPath();
        UpdateAnimator();
    }

    // ── Peace Check ────────────────────────────────────────────────────────

    private bool IsPeaceful()
    {
        return tiedToBoss && BossController.IsDead;
    }

    // ── Knockback ──────────────────────────────────────────────────────────

    public void ApplyKnockback(Vector2 force, float duration)
    {
        if (IsPeaceful()) return;
        rb.linearVelocity = force;
        isKnockedBack = true;
        knockbackTimer = duration;
    }

    // ── Slow (ISlowable) ───────────────────────────────────────────────────

    public void ApplySlowMultiplier(float multiplier)
    {
        if (!isSlowed)
        {
            baseRoamSpeed = roamSpeed;
            baseChaseSpeed = chaseSpeed;
        }

        // Always apply against the stored base speed, not the current
        // (possibly already-slowed) speed, so repeated calls don't stack.
        roamSpeed = baseRoamSpeed * multiplier;
        chaseSpeed = baseChaseSpeed * multiplier;
        isSlowed = true;
    }

    public void RemoveSlowMultiplier()
    {
        if (!isSlowed) return;

        if (baseRoamSpeed >= 0f) roamSpeed = baseRoamSpeed;
        if (baseChaseSpeed >= 0f) chaseSpeed = baseChaseSpeed;

        baseRoamSpeed = -1f;
        baseChaseSpeed = -1f;
        isSlowed = false;
    }

    // ── State Transitions ──────────────────────────────────────────────────

    private void EnterChase()
    {
        if (IsPeaceful()) return;
        currentState = State.Chasing;
        loseTargetTimer = 0f;
        isWaiting = false;
        StopAllCoroutines();
        RefreshChasePath();
    }

    private void EnterRoam()
    {
        currentState = State.Roaming;
        currentPath.Clear();
        pathIndex = 0;
        isWaiting = false;
        rb.linearVelocity = Vector2.zero;
        StopAllCoroutines();
        StartCoroutine(RoamCoroutine());
    }

    // ── Movement ───────────────────────────────────────────────────────────

    private void MoveAlongPath()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        if (isWaiting || currentPath == null || pathIndex >= currentPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 target = currentPath[pathIndex];
        Vector2 direction = (target - (Vector2)transform.position);

        if (direction.magnitude <= waypointTolerance)
        {
            pathIndex++;
            return;
        }

        float speed = currentState == State.Chasing ? chaseSpeed : roamSpeed;
        rb.linearVelocity = direction.normalized * speed;
    }

    // ── Roam Logic ─────────────────────────────────────────────────────────

    private IEnumerator RoamCoroutine()
    {
        while (currentState == State.Roaming)
        {
            Vector2? dest = GetRandomWalkablePoint();
            if (dest == null) { yield return new WaitForSeconds(0.5f); continue; }

            SetPath(PathfindingGrid.Instance.FindPath(transform.position, dest.Value));

            yield return new WaitUntil(() =>
                currentState != State.Roaming ||
                (currentPath != null && pathIndex >= currentPath.Count));

            if (currentState != State.Roaming) yield break;

            isWaiting = true;
            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(roamWaitTime);
            isWaiting = false;
        }
    }

    private Vector2? GetRandomWalkablePoint()
    {
        if (PathfindingGrid.Instance == null) return null;

        for (int i = 0; i < 15; i++)
        {
            Vector2 candidate = spawnPoint + Random.insideUnitCircle * roamRadius;
            Node node = PathfindingGrid.Instance.NodeFromWorldPoint(candidate);
            if (node.Walkable) return node.WorldPosition;
        }
        return null;
    }

    // ── Path Helpers ───────────────────────────────────────────────────────

    private void SetPath(List<Vector2> path)
    {
        currentPath = path ?? new List<Vector2>();
        pathIndex = 0;
    }

    private void RefreshChasePath()
    {
        if (player == null || PathfindingGrid.Instance == null) return;
        SetPath(PathfindingGrid.Instance.FindPath(transform.position, player.position));
    }

    // ── Detection ──────────────────────────────────────────────────────────

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        if (IsPeaceful()) return false;

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

    // ── Animator ───────────────────────────────────────────────────────────

    private void UpdateAnimator()
    {
        if (animator == null) return;

        Vector2 velocity = rb.linearVelocity;
        bool isMoving = velocity.magnitude > 0.1f;

        animator.SetFloat("Speed", velocity.magnitude);
        animator.SetBool("IsChasing", currentState == State.Chasing);
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("MoveX", velocity.normalized.x);
            animator.SetFloat("MoveY", velocity.normalized.y);
        }
    }

    // ── Gizmos ─────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = currentState == State.Chasing ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)(Vector2)spawnPoint : transform.position, roamRadius);

        if (currentPath == null || currentPath.Count == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = pathIndex; i < currentPath.Count - 1; i++)
            Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
    }
}