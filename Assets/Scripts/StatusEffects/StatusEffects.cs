using UnityEngine;
using UnityEngine.UI;


public class StatusEffects : MonoBehaviour
{
    [Header("Burn")]
    public bool canBurn = true;
    public float burnDamage = 2f;
    public float burnTickInterval = 0.5f;
    public float burnDuration = 3f;

    [Header("Poison")]
    public bool canPoison = true;
    public float poisonDamage = 1f;
    public float poisonTickInterval = 1f;
    public float poisonDuration = 5f;

    [Header("Slow")]
    public bool canBeSlow = true;
    [Range(0f, 1f)]
    [Tooltip("How much speed is removed. 0.5 = 50% slower.")]
    public float slowStrength = 0.5f;
    public float slowDuration = 2f;

    [Header("Bleed")]
    public bool canBleed = true;
    public float bleedDamagePerStack = 1f;
    public float bleedTickInterval = 0.5f;
    public float bleedDuration = 4f;
    public int maxBleedStacks = 5;

    [Header("Status Icons")]
    [Tooltip("The HorizontalLayoutGroup container sitting above the health bar.")]
    public RectTransform statusIconContainer;
    [Tooltip("Icon size in world-space canvas units.")]
    public float iconSize = 20f;
    public Sprite burnIcon;
    public Sprite poisonIcon;
    public Sprite slowIcon;
    public Sprite bleedIcon;

    // Runtime state — visible in Inspector for debugging
    [Header("Active Status (Runtime)")]
    [SerializeField] private bool isBurning = false;
    [SerializeField] private bool isPoisoned = false;
    [SerializeField] private bool isSlowed = false;
    [SerializeField] private int bleedStacks = 0;

    private float burnTimer = 0f;
    private float burnTickTimer = 0f;

    private float poisonTimer = 0f;
    private float poisonTickTimer = 0f;

    private float slowTimer = 0f;

    private float bleedTimer = 0f;
    private float bleedTickTimer = 0f;

    // Cached icon Image components, created once at Start
    private Image burnIconImage;
    private Image poisonIconImage;
    private Image slowIconImage;
    private Image bleedIconImage;

    private EnemyHealth enemyHealth;
    private ISlowable slowable;

    private void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        slowable = GetComponent<ISlowable>();

        if (enemyHealth == null)
            Debug.LogWarning(gameObject.name + ": StatusEffects requires an EnemyHealth component.");
        if (slowable == null && canBeSlow)
            Debug.LogWarning(gameObject.name + ": StatusEffects couldn't find an ISlowable component — Slow will not work.");

        InitIcons();
    }

    private void Update()
    {
        if (isBurning) TickBurn();
        if (isPoisoned) TickPoison();
        if (isSlowed) TickSlow();
        if (bleedStacks > 0) TickBleed();
    }

    // -------------------------------------------------------------------------
    // Icon Setup
    // -------------------------------------------------------------------------

    private void InitIcons()
    {
        if (statusIconContainer == null)
        {
            Debug.LogWarning(gameObject.name + ": StatusEffects — statusIconContainer is not assigned! Status icons will not show.");
            return;
        }

        burnIconImage = CreateIcon("BurnIcon", burnIcon);
        poisonIconImage = CreateIcon("PoisonIcon", poisonIcon);
        slowIconImage = CreateIcon("SlowIcon", slowIcon);
        bleedIconImage = CreateIcon("BleedIcon", bleedIcon);

        SetIconActive(burnIconImage, false);
        SetIconActive(poisonIconImage, false);
        SetIconActive(slowIconImage, false);
        SetIconActive(bleedIconImage, false);
    }

    private Image CreateIcon(string iconName, Sprite sprite)
    {
        if (sprite == null)
            Debug.LogWarning(gameObject.name + $": StatusEffects — no sprite assigned for '{iconName}'. Assign it in the Inspector.");

        GameObject obj = new GameObject(iconName, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(statusIconContainer, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(iconSize, iconSize);

        Image img = obj.GetComponent<Image>();
        img.sprite = sprite;
        img.color = Color.white;
        img.preserveAspect = true;

        return img;
    }

    private void SetIconActive(Image icon, bool active)
    {
        if (icon != null)
            icon.gameObject.SetActive(active);
    }

    // -------------------------------------------------------------------------
    // Public Apply Methods
    // -------------------------------------------------------------------------

    public void ApplyBurn()
    {
        if (!canBurn) return;
        isBurning = true;
        burnTimer = burnDuration;
        burnTickTimer = 0f;
        SetIconActive(burnIconImage, true);
    }

    public void ApplyPoison()
    {
        if (!canPoison) return;
        isPoisoned = true;
        poisonTimer = poisonDuration;
        poisonTickTimer = 0f;
        SetIconActive(poisonIconImage, true);
    }

    public void ApplySlow()
    {
        if (!canBeSlow || slowable == null) return;

        if (!isSlowed)
            slowable.ApplySlowMultiplier(1f - slowStrength);

        isSlowed = true;
        slowTimer = slowDuration;
        SetIconActive(slowIconImage, true);
    }

    public void ApplyBleed()
    {
        if (!canBleed) return;
        if (bleedStacks < maxBleedStacks)
            bleedStacks++;

        bleedTimer = bleedDuration;
        bleedTickTimer = 0f;
        SetIconActive(bleedIconImage, true);
    }

    // -------------------------------------------------------------------------
    // Internal Tick Logic
    // -------------------------------------------------------------------------

    private void TickBurn()
    {
        burnTimer -= Time.deltaTime;
        burnTickTimer -= Time.deltaTime;

        if (burnTickTimer <= 0f)
        {
            enemyHealth?.TakeDamage((int)burnDamage);
            burnTickTimer = burnTickInterval;
        }

        if (burnTimer <= 0f)
        {
            isBurning = false;
            burnTimer = 0f;
            SetIconActive(burnIconImage, false);
        }
    }

    private void TickPoison()
    {
        poisonTimer -= Time.deltaTime;
        poisonTickTimer -= Time.deltaTime;

        if (poisonTickTimer <= 0f)
        {
            enemyHealth?.TakeDamage((int)poisonDamage);
            poisonTickTimer = poisonTickInterval;
        }

        if (poisonTimer <= 0f)
        {
            isPoisoned = false;
            poisonTimer = 0f;
            SetIconActive(poisonIconImage, false);
        }
    }

    private void TickSlow()
    {
        slowTimer -= Time.deltaTime;

        if (slowTimer <= 0f)
        {
            slowable?.RemoveSlowMultiplier();

            isSlowed = false;
            slowTimer = 0f;
            SetIconActive(slowIconImage, false);
        }
    }

    private void TickBleed()
    {
        bleedTimer -= Time.deltaTime;
        bleedTickTimer -= Time.deltaTime;

        if (bleedTickTimer <= 0f)
        {
            int bleedDamage = Mathf.RoundToInt(bleedDamagePerStack * bleedStacks);
            enemyHealth?.TakeDamage(bleedDamage);
            bleedTickTimer = bleedTickInterval;
        }

        if (bleedTimer <= 0f)
        {
            bleedStacks = 0;
            bleedTimer = 0f;
            SetIconActive(bleedIconImage, false);
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    public bool IsBurning => isBurning;
    public bool IsPoisoned => isPoisoned;
    public bool IsSlowed => isSlowed;
    public int BleedStacks => bleedStacks;

    public void ClearAll()
    {
        if (isSlowed)
            slowable?.RemoveSlowMultiplier();

        isBurning = false;
        isPoisoned = false;
        isSlowed = false;
        bleedStacks = 0;

        burnTimer = burnTickTimer = 0f;
        poisonTimer = poisonTickTimer = 0f;
        slowTimer = 0f;
        bleedTimer = bleedTickTimer = 0f;

        SetIconActive(burnIconImage, false);
        SetIconActive(poisonIconImage, false);
        SetIconActive(slowIconImage, false);
        SetIconActive(bleedIconImage, false);
    }
}