using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int damage = 5;
    public GameObject popUpPrefab;
    [SerializeField] private Animator animator;

    [Header("Hold Settings")]
    public float holdDistance = 0.4f;
    public float resourceCost = 1f;

    [Header("Swing Settings")]
    public float swingDuration = 0.2f;
    public float swingArc = 90f;
    public float rotationOffset = 45f;
    public float attackSpeed = 1f;

    [Header("Damage Settings")]
    public float damageInterval = 0.5f;

    [Header("Burn Settings")]
    [Range(0f, 1f)] public float burnChance = 0.5f;

    [Header("Poison Settings")]
    [Range(0f, 1f)] public float poisonChance = 0.5f;

    [Header("Slow Settings")]
    [Range(0f, 1f)] public float slowChance = 0.3f;

    [Header("Bleed Settings")]
    [Range(0f, 1f)] public float bleedChance = 0.4f;

    [Header("Knockback Settings")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.15f;

    [Header("Leveling")]
    public float hitXP = 1f;
    public float killXP = 5f;
    private ItemData itemData;

    [Header("Audio - Swing")]
    [SerializeField] private AudioClip swingSound;
    [Range(0f, 1f)][SerializeField] private float swingSoundVolume = 1f;
    [SerializeField] private AudioClip swingMissSound;
    [Range(0f, 1f)][SerializeField] private float swingMissSoundVolume = 1f;

    [Header("Audio - Status Effects")]
    [SerializeField] private AudioClip burnSound;
    [Range(0f, 1f)][SerializeField] private float burnSoundVolume = 1f;
    [SerializeField] private AudioClip poisonSound;
    [Range(0f, 1f)][SerializeField] private float poisonSoundVolume = 1f;
    [SerializeField] private AudioClip slowSound;
    [Range(0f, 1f)][SerializeField] private float slowSoundVolume = 1f;
    [SerializeField] private AudioClip bleedSound;
    [Range(0f, 1f)][SerializeField] private float bleedSoundVolume = 1f;

    private AudioSource audioSource;

    private bool isAttacking = false;
    private bool hasHitThisSwing = false;
    private EnemyHealth currentEnemy = null;
    private BossController currentBoss = null;
    private float damageTimer = 0f;
    private float swingTimer = 0f;
    private Transform playerTransform;
    private SpriteRenderer sr;
    private PolygonCollider2D col;

    // Weapon damage after the rage bonus (only > base damage while burning rage).
    private int FinalDamage
    {
        get
        {
            float levelMult = itemData != null ? itemData.DamageMultiplier : 1f;
            float rageMult = GameServices.Rage != null ? GameServices.Rage.DamageMultiplier : 1f;
            float raw = damage * levelMult * rageMult;
            int whole = Mathf.FloorToInt(raw);
            return whole + (Random.value < raw - whole ? 1 : 0);
        }
    }

    private enum FacingDir { Right, Up, Left, Down }
    private FacingDir facing = FacingDir.Right;

    private float swingStartAngle;
    private float swingEndAngle;

    private float EffectiveSwingDuration => swingDuration / Mathf.Max(attackSpeed, 0.01f);

    private float FacingAngle(FacingDir dir)
    {
        switch (dir)
        {
            case FacingDir.Right: return 0f;
            case FacingDir.Up: return 90f;
            case FacingDir.Left: return 180f;
            case FacingDir.Down: return 270f;
            default: return 0f;
        }
    }

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<PolygonCollider2D>();
        isAttacking = false;
        playerTransform = GameObject.FindWithTag("Player").transform;
        if (animator != null) animator.SetBool("SwordHit", false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        foreach (Collider2D playerCol in playerTransform.GetComponentsInChildren<Collider2D>())
        {
            Physics2D.IgnoreCollision(col, playerCol);
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, volume);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        if (!isAttacking)
            UpdateFacing();

        if (Input.GetMouseButtonDown(0) && !isAttacking)
            StartSwing();

        if (isAttacking)
            UpdateSwing();
        else
            UpdateIdle();

        if (isAttacking && (currentEnemy != null || currentBoss != null))
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                if (currentEnemy != null)
                {
                    currentEnemy.TakeDamage(FinalDamage);
                    ReportClassDamage();
                }

                if (currentBoss != null)
                {
                    currentBoss.TakeDamage(FinalDamage);
                    ReportClassDamage();
                }

                damageTimer = 0f;
            }
        }
    }

    private void UpdateFacing()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - playerTransform.position).normalized;

        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            facing = dir.x >= 0 ? FacingDir.Right : FacingDir.Left;
        else
            facing = dir.y >= 0 ? FacingDir.Up : FacingDir.Down;
    }

    private void StartSwing()
    {
        if (!PlayerResourceManager.Instance.TrySpend(resourceCost))
            return;

        isAttacking = true;
        hasHitThisSwing = false;
        swingTimer = 0f;
        col.enabled = true;

        PlaySound(swingSound, swingSoundVolume);

        float baseAngle = FacingAngle(facing);
        float half = swingArc * 0.5f;
        swingStartAngle = baseAngle + half;
        swingEndAngle = baseAngle - half;
    }

    private void UpdateSwing()
    {
        swingTimer += Time.deltaTime;
        float t = swingTimer / EffectiveSwingDuration;
        t = Mathf.Clamp01(t);

        float currentAngle = Mathf.Lerp(swingStartAngle, swingEndAngle, t);
        float thrust = Mathf.Sin(t * Mathf.PI) * 0.3f;

        Vector2 swordDir = new Vector2(
            Mathf.Cos(currentAngle * Mathf.Deg2Rad),
            Mathf.Sin(currentAngle * Mathf.Deg2Rad));

        transform.position = playerTransform.position
            + (Vector3)(swordDir * (holdDistance + thrust));

        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle + rotationOffset);
        sr.flipX = (facing == FacingDir.Left);

        if (swingTimer >= EffectiveSwingDuration)
        {
            if (!hasHitThisSwing)
                PlaySound(swingMissSound, swingMissSoundVolume);

            isAttacking = false;
            col.enabled = false;
            if (currentEnemy != null) currentEnemy.OnDeath -= HandleEnemyKilled;
            currentEnemy = null;
            currentBoss = null;
            damageTimer = 0f;
        }
    }

    private void OnDisable()
    {
        if (currentEnemy != null) currentEnemy.OnDeath -= HandleEnemyKilled;
        currentEnemy = null;
        currentBoss = null;
    }

    private void UpdateIdle()
    {
        float baseAngle = FacingAngle(facing);
        float idleAngle = baseAngle - swingArc * 0.5f;

        Vector2 idleDir = new Vector2(
            Mathf.Cos(idleAngle * Mathf.Deg2Rad),
            Mathf.Sin(idleAngle * Mathf.Deg2Rad));

        transform.position = playerTransform.position
            + (Vector3)(idleDir * holdDistance);

        transform.rotation = Quaternion.Euler(0f, 0f, idleAngle + rotationOffset);
        sr.flipX = (facing == FacingDir.Left);
    }

    private void ApplyKnockback(GameObject target)
    {
        FlyingEnemy flyingEnemy = target.GetComponent<FlyingEnemy>()
            ?? target.GetComponentInParent<FlyingEnemy>();
        if (flyingEnemy != null)
        {
            Vector2 knockbackDir = (target.transform.position - playerTransform.position).normalized;
            flyingEnemy.ApplyExternalKnockback(knockbackDir * knockbackForce, knockbackDuration);
            return;
        }

        EnemyPatrol enemyPatrol = target.GetComponent<EnemyPatrol>()
            ?? target.GetComponentInParent<EnemyPatrol>();
        if (enemyPatrol == null) return;
        Vector2 knockbackDir2 = (target.transform.position - playerTransform.position).normalized;
        enemyPatrol.ApplyKnockback(knockbackDir2 * knockbackForce, knockbackDuration);
    }

    private void TryApplyStatusEffects(GameObject target)
    {
        StatusEffects status = target.GetComponent<StatusEffects>()
            ?? target.GetComponentInParent<StatusEffects>();
        if (status == null) return;

        if (Random.value <= burnChance) { status.ApplyBurn(); PlaySound(burnSound, burnSoundVolume); }
        if (Random.value <= poisonChance) { status.ApplyPoison(); PlaySound(poisonSound, poisonSoundVolume); }
        if (Random.value <= slowChance) { status.ApplySlow(); PlaySound(slowSound, slowSoundVolume); }
        if (Random.value <= bleedChance) { status.ApplyBleed(); PlaySound(bleedSound, bleedSoundVolume); }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Weapon hit: " + other.gameObject.name + " | isAttacking: " + isAttacking);
        Debug.Log($"lvl {(itemData != null ? itemData.level : -1)} | mult {(itemData != null ? itemData.DamageMultiplier : 0f)} | base {damage} | final {FinalDamage}");
        if (!isAttacking) return;
        if (other.CompareTag("Player")) return;
        if (other.CompareTag("EnemyProjectile")) return;

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>()
            ?? other.GetComponentInParent<EnemyHealth>()
            ?? other.GetComponentInChildren<EnemyHealth>();

        if (enemyHealth != null)
        {
            if (currentEnemy != enemyHealth)
                enemyHealth.OnDeath += HandleEnemyKilled;

            currentEnemy = enemyHealth;
            currentEnemy.TakeDamage(FinalDamage);
            ReportClassDamage();
            damageTimer = 0f;
            hasHitThisSwing = true;
            ApplyKnockback(other.gameObject);
            TryApplyStatusEffects(other.gameObject);

            itemData?.AddXP(hitXP);
            return;
        }

        BossController boss = other.GetComponent<BossController>()
            ?? other.GetComponentInParent<BossController>()
            ?? other.GetComponentInChildren<BossController>();

        if (boss != null)
        {
            currentBoss = boss;
            currentBoss.TakeDamage(FinalDamage);
            ReportClassDamage();
            damageTimer = 0f;
            hasHitThisSwing = true;

            itemData?.AddXP(hitXP);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isAttacking) return;

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>()
            ?? other.GetComponentInParent<EnemyHealth>()
            ?? other.GetComponentInChildren<EnemyHealth>();

        if (enemyHealth != null && enemyHealth == currentEnemy)
        {
            enemyHealth.OnDeath -= HandleEnemyKilled;
            currentEnemy = null;
            damageTimer = 0f;
        }

        BossController boss = other.GetComponent<BossController>()
            ?? other.GetComponentInParent<BossController>()
            ?? other.GetComponentInChildren<BossController>();

        if (boss != null && boss == currentBoss)
        {
            currentBoss = null;
            damageTimer = 0f;
        }
    }

    private void HandleEnemyKilled()
    {
        itemData?.AddXP(killXP);
    }

    private void ReportClassDamage()
    {
        if (GameServices.Classes != null)
            GameServices.Classes.ReportDamage(FinalDamage);
    }

    public void ApplyWeaponData(WeaponData data, ItemData ownerItemData = null)
    {
        itemData = ownerItemData;

        if (data.weaponType == WeaponType.Bow)
        {
            gameObject.SetActive(false);
            return;
        }

        EquipmentManager eq = EquipmentManager.Instance;
        int baseDamage = data.damage + (eq != null ? eq.TotalBonusDamage : 0);
        damage = baseDamage;
           

        attackSpeed = data.attackSpeed + (eq != null ? eq.TotalAttackSpeedBonus : 0f);
        swingDuration = data.swingDuration;
        swingArc = data.swingArc;
        rotationOffset = data.rotationOffset;
        damageInterval = data.damageInterval;
        knockbackForce = data.knockbackForce + (eq != null ? eq.TotalKnockbackBonus : 0f);
        knockbackDuration = data.knockbackDuration;
        burnChance = Mathf.Clamp01(data.burnChance + (eq != null ? eq.TotalBurnChanceBonus : 0f));
        poisonChance = Mathf.Clamp01(data.poisonChance + (eq != null ? eq.TotalPoisonChanceBonus : 0f));
        slowChance = Mathf.Clamp01(data.slowChance + (eq != null ? eq.TotalSlowChanceBonus : 0f));
        bleedChance = Mathf.Clamp01(data.bleedChance + (eq != null ? eq.TotalBleedChanceBonus : 0f));
        resourceCost = data.resourceCost;
    }
}