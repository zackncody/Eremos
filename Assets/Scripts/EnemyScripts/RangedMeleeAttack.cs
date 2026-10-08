using UnityEngine;
public class RangedMeleeAttack : MonoBehaviour
{
    [Header("References")]
    public Transform tongueRoot;        // anchored at mouth, only ever rotates - never translates
    public Transform tongueSprite;      // child, pivot set to LEFT edge in Sprite Editor
    public Transform tongueHitPoint;    // child, holds the Collider2D + TongueHitbox
    public SpriteRenderer tongueRenderer;
    public Collider2D tongueCollider;
    public Transform player;
    public FrogHopper hopper;
    [Header("Mouth Anchors (local position of TongueRoot per facing)")]
    public Vector2 mouthOffsetDown = new Vector2(0f, -0.3f);
    public Vector2 mouthOffsetUp = new Vector2(0f, 0.3f);
    public Vector2 mouthOffsetLeft = new Vector2(-0.3f, 0f);
    public Vector2 mouthOffsetRight = new Vector2(0.3f, 0f);
    float tongueSpriteBaseLength = 1f; // auto-detected from the sprite asset in Start()
    [Header("Attack Settings")]
    public float attackRange = 4f;
    public float extendDistance = 3f;
    public float extendDuration = 0.15f;
    public float holdDuration = 0.1f;
    public float retractDuration = 0.2f;
    public float cooldown = 2f;
    public int damage = 1;
    bool isAttacking;
    bool hasHitThisAttack;
    float cooldownTimer;
    Coroutine attackRoutine;
    void Start()
    {
        if (tongueRenderer.sprite != null)
            tongueSpriteBaseLength = tongueRenderer.sprite.bounds.size.x; // native world-unit size, ignores current localScale
        else
           // Debug.LogWarning("[RangedMeleeAttack] tongueRenderer has no Sprite assigned - length will be wrong.");

        tongueRoot.localPosition = GetMouthAnchor();
        SetLength(0f);
        tongueCollider.enabled = false;
        tongueRenderer.enabled = false;
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }
    Vector3 GetMouthAnchor()
    {
        if (hopper == null) return mouthOffsetDown;
        return hopper.CurrentFacing switch
        {
            FrogHopper.FacingDir.Up => mouthOffsetUp,
            FrogHopper.FacingDir.Left => mouthOffsetLeft,
            FrogHopper.FacingDir.Right => mouthOffsetRight,
            _ => mouthOffsetDown,
        };
    }
    // length = current world-space extension distance, 0 = fully retracted
    // Works with a CENTERED sprite pivot: keeps the sprite's back edge glued to the
    // mouth by shifting its position forward by half its current length as it grows.
    void SetLength(float length)
    {
        float scaleX = length / tongueSpriteBaseLength;
        tongueSprite.localScale = new Vector3(scaleX, 1f, 1f);
        tongueSprite.localPosition = new Vector3(length / 2f, 0f, 0f);
        tongueHitPoint.localPosition = new Vector3(length, 0f, 0f);
    }
    void Update()
    {
        if (!isAttacking)
        {
            Vector3 mouth = GetMouthAnchor();
            if (tongueRoot.localPosition != mouth)
            {
                tongueRoot.localPosition = mouth;
                tongueRoot.localRotation = Quaternion.identity;
                SetLength(0f);
                tongueCollider.enabled = false;
                tongueRenderer.enabled = false;
            }
        }
        if (isAttacking) return;
        if (player == null) { Debug.Log("[RangedMeleeAttack] blocked: player reference is null"); return; }
        if (hopper != null && hopper.IsHopping) return; // this one is expected to block often, don't log it
        cooldownTimer -= Time.deltaTime;
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange && cooldownTimer <= 0f)
        {
            attackRoutine = StartCoroutine(DoTongueAttack());
        }
        else if (cooldownTimer <= 0f)
        {
            //Debug.Log($"[RangedMeleeAttack] in range check failed: dist={dist:F2}, attackRange={attackRange}, hopping={hopper?.IsHopping}");
        }
    }
    System.Collections.IEnumerator DoTongueAttack()
    {
        isAttacking = true;
        hasHitThisAttack = false;
        cooldownTimer = cooldown;
        tongueRenderer.enabled = true;
        Debug.Log("[RangedMeleeAttack] Attack started");

        tongueRoot.localPosition = GetMouthAnchor();
        Vector3 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        tongueRoot.localRotation = Quaternion.Euler(0f, 0f, angle);
        tongueCollider.enabled = true; // on for the whole extend/hold/retract window

        float t = 0f;
        while (t < extendDuration)
        {
            t += Time.deltaTime;
            SetLength(Mathf.Lerp(0f, extendDistance, t / extendDuration));
            yield return null;
        }
        SetLength(extendDistance);
        yield return new WaitForSeconds(holdDuration);

        t = 0f;
        while (t < retractDuration)
        {
            t += Time.deltaTime;
            SetLength(Mathf.Lerp(extendDistance, 0f, t / retractDuration));
            yield return null;
        }
        SetLength(0f);
        tongueCollider.enabled = false;
        tongueRoot.localRotation = Quaternion.identity;
        tongueRenderer.enabled = false;
        isAttacking = false;
        attackRoutine = null;
    }
    void OnDisable()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
        if (tongueRoot != null)
        {
            tongueRoot.localPosition = GetMouthAnchor();
            tongueRoot.localRotation = Quaternion.identity;
        }
        if (tongueSprite != null && tongueHitPoint != null) SetLength(0f);
        if (tongueCollider != null) tongueCollider.enabled = false;
        if (tongueRenderer != null) tongueRenderer.enabled = false;
        isAttacking = false;
    }
    public void OnTongueHit(Collider2D other)
    {
        if (!tongueCollider.enabled) return;
        if (hasHitThisAttack) return;
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
                hasHitThisAttack = true;
            }
        }
    }
}