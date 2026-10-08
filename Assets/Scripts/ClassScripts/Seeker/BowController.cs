using UnityEngine;
public class BowController : MonoBehaviour
{
    [Header("Stats")]
    public int damage = 8;
    public float arrowSpeed = 18f;
    public float pullBackDuration = 0.4f;
    public float holdDistance = 0.4f;
    public float rotationOffset = 0f;
    public float resourceCost = 1f;
    [Header("References")]
    public GameObject arrowPrefab;
    [SerializeField] private Animator animator;
    private Transform playerTransform;
    private SpriteRenderer sr;
    private bool isPullingBack = false;
    private bool hasReleased = false;
    private float pullTimer = 0f;
    private static readonly int PullBackParam = Animator.StringToHash("PullBack");
    private static readonly int ShootParam = Animator.StringToHash("Shoot");
    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        if (playerTransform == null)
            Debug.LogError("BowController: No GameObject tagged 'Player' found.", this);
    }
    public void ApplyFromPrefab(BowController source)
    {
        damage = source.damage;
        arrowSpeed = source.arrowSpeed;
        pullBackDuration = source.pullBackDuration;
        arrowPrefab = source.arrowPrefab;
        resourceCost = source.resourceCost;
    }
    private void Update()
    {
        if (playerTransform == null) return;
        AimAtMouse();
        if (Input.GetMouseButtonDown(0) && !isPullingBack)
            BeginPullBack();
        if (isPullingBack)
        {
            pullTimer += Time.deltaTime;
            if (Input.GetMouseButtonUp(0))
                Release();
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
        sr.flipY = (angle > 90f || angle < -90f);
    }
    private void BeginPullBack()
    {
        isPullingBack = true;
        hasReleased = false;
        pullTimer = 0f;
        if (animator != null)
            animator.SetTrigger(PullBackParam);
    }
    private void Release()
    {
        if (hasReleased) return;
        hasReleased = true;
        isPullingBack = false;

        if (!PlayerResourceManager.Instance.TrySpend(resourceCost))
        {
            // not enough resource — cancel the shot, skip animation/arrow
            return;
        }

        if (animator != null)
            animator.SetTrigger(ShootParam);
        FireArrow();
    }
    private void FireArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("BowController: arrowPrefab is not assigned.", this);
            return;
        }
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - playerTransform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector3 spawnPos = playerTransform.position + (Vector3)(dir * 0.6f);
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.Euler(0f, 0f, angle - 90f));
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null)
            arrowScript.Init(dir, damage);
        else
            Debug.LogError("BowController: arrowPrefab is missing an Arrow component.", this);
    }
    public void ApplyWeaponData(WeaponData data)
    {
        if (data.weaponType != WeaponType.Bow) return;
        EquipmentManager eq = EquipmentManager.Instance;
        damage = data.damage + (eq != null ? eq.TotalBonusDamage : 0);
        arrowSpeed = data.arrowSpeed;
        pullBackDuration = data.pullBackDuration;
        arrowPrefab = data.arrowPrefab;
        resourceCost = data.resourceCost;
    }
}