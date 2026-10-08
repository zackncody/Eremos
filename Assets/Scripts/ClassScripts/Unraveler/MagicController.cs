using UnityEngine;

public class MagicController : MonoBehaviour
{
    [Header("Stats")]
    public int damage = 6;
    public MagicCastType castType = MagicCastType.Projectile;
    public float castTime = 0.3f;
    public float rotationOffset = 0f;
    public float holdDistance = 0.4f;
    public float resourceCost = 1f;

    [Header("Projectile")]
    public GameObject magicProjectilePrefab;
    public float magicProjectileSpeed = 14f;

    [Header("AOE Burst")]
    public GameObject magicAOEPrefab;
    public float magicAOERadius = 2.5f;
    public Vector2 aoeSpawnOffset = Vector2.zero;

    [Header("References")]
    [SerializeField] private Animator animator;

    private Transform playerTransform;
    private SpriteRenderer sr;

    private bool isCasting = false;
    private bool hasReleased = false;
    private float castTimer = 0f;

    private static readonly int CastParam = Animator.StringToHash("Cast");
    private static readonly int ReleaseParam = Animator.StringToHash("Release");

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        if (playerTransform == null)
            Debug.LogError("MagicController: No GameObject tagged 'Player' found.", this);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        AimAtMouse();

        if (Input.GetMouseButtonDown(0) && !isCasting)
            BeginCast();

        if (isCasting)
        {
            castTimer += Time.deltaTime;

            if (castType == MagicCastType.Projectile)
            {
                if (Input.GetMouseButtonUp(0))
                    ReleaseProjectile();
            }
            else
            {
                if (castTimer >= castTime)
                    ReleaseAOE();
            }
        }
    }

    private void AimAtMouse()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - playerTransform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.position = playerTransform.position + (Vector3)(dir * holdDistance);
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);

        if (sr != null)
            sr.flipY = (angle > 90f || angle < -90f);
    }

    private void BeginCast()
    {
        isCasting = true;
        hasReleased = false;
        castTimer = 0f;
        if (animator != null)
            animator.SetTrigger(CastParam);
    }

    private void ReleaseProjectile()
    {
        if (hasReleased) return;
        hasReleased = true;
        isCasting = false;

        if (!PlayerResourceManager.Instance.TrySpend(resourceCost))
            return;

        if (animator != null)
            animator.SetTrigger(ReleaseParam);

        if (magicProjectilePrefab == null)
        {
            Debug.LogError("MagicController: magicProjectilePrefab is not assigned.", this);
            return;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - playerTransform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector3 spawnPos = playerTransform.position + (Vector3)(dir * 0.6f);

        GameObject proj = Instantiate(magicProjectilePrefab, spawnPos, Quaternion.Euler(0f, 0f, angle - 90f));
        MagicProjectile projScript = proj.GetComponent<MagicProjectile>();
        if (projScript != null)
            projScript.Init(dir, damage, magicProjectileSpeed);
        else
            Debug.LogError("MagicController: magicProjectilePrefab is missing a MagicProjectile component.", this);
    }

    private void ReleaseAOE()
    {
        if (hasReleased) return;
        hasReleased = true;
        isCasting = false;

        if (!PlayerResourceManager.Instance.TrySpend(resourceCost))
            return;

        if (animator != null)
            animator.SetTrigger(ReleaseParam);

        if (magicAOEPrefab == null)
        {
            Debug.LogError("MagicController: magicAOEPrefab is not assigned.", this);
            return;
        }

        GameObject aoe = Instantiate(magicAOEPrefab, playerTransform.position + (Vector3)aoeSpawnOffset, Quaternion.identity);
        MagicAOE aoeScript = aoe.GetComponent<MagicAOE>();
        if (aoeScript != null)
            aoeScript.Init(damage, magicAOERadius);
        else
            Debug.LogError("MagicController: magicAOEPrefab is missing a MagicAOE component.", this);
    }

    public void ApplyWeaponData(WeaponData data)
    {
        if (data.weaponType != WeaponType.Magic) return;

        EquipmentManager eq = EquipmentManager.Instance;
        damage = data.damage + (eq != null ? eq.TotalBonusDamage : 0);
        castType = data.magicCastType;
        castTime = data.castTime;
        resourceCost = data.resourceCost;

        magicProjectilePrefab = data.magicProjectilePrefab;
        magicProjectileSpeed = data.magicProjectileSpeed;

        magicAOEPrefab = data.magicAOEPrefab;
        magicAOERadius = data.magicAOERadius;
    }
}