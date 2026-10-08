using UnityEngine;

public class SeekerSystem : MonoBehaviour
{
    public static SeekerSystem Instance { get; private set; }

    [Header("Class")]
    [SerializeField] private string requiredClassID = "seeker";
    [SerializeField] private string markAbilityID = "seeker_mark";
    [SerializeField] private string poisonAbilityID = "seeker_poison";

    [Header("Input")]
    [SerializeField] private int mouseButton = 2;   // 0 left, 1 right, 2 middle

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 14f;
    [SerializeField] private float spawnDistance = 0.6f;
    [SerializeField] private float spriteRotationOffset = 0f;   // 0 if your sprite points right, -90 if it points up

    [Header("Mark ability")]
    [SerializeField] private float damageMultiplier = 1.5f;
    [SerializeField] private float markDuration = 10f;

    [Header("Poison ability")]
    [SerializeField] private float poisonXPWindow = 5f;   // seconds of class XP after a poison hit

    private Transform playerTransform;
    
    private float poisonXPEndTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterPoisonHit()
    {
        poisonXPEndTime = Time.time + poisonXPWindow;
    }

    private ClassAbility GetActiveAbility()
    {
        ClassManager cm = GameServices.Classes;
        if (cm == null || cm.Current == null || cm.Current.classID != requiredClassID) return null;

        ClassAbility a = cm.CurrentAbility;
        if (a == null) return null;
        if (a.abilityID != markAbilityID && a.abilityID != poisonAbilityID) return null;
        return a;
    }

    private void Update()
    {
        ClassAbility ability = GetActiveAbility();
        if (ability == null) return;

        // Class XP is earned while the current ability's effect is live.
        ClassManager cm = GameServices.Classes;
        bool earnXP = ability.abilityID == markAbilityID
            ? MarkedEnemy.ActiveCount > 0
            : Time.time < poisonXPEndTime;
        if (cm.AbilityActive != earnXP)
            cm.SetAbilityActive(earnXP);

        if (Input.GetMouseButtonDown(mouseButton) && !GameServices.InputBlocked && !cm.IsOnCooldown(ability))
            Fire(ability);
    }

    private void Fire(ClassAbility ability)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("SeekerSystem: projectilePrefab is not assigned.", this);
            return;
        }

        if (playerTransform == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p == null) return;
            playerTransform = p.transform;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - playerTransform.position).normalized;
        if (dir == Vector2.zero) dir = Vector2.right;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector3 spawnPos = playerTransform.position + (Vector3)(dir * spawnDistance);

        GameObject go = Instantiate(projectilePrefab, spawnPos, Quaternion.Euler(0f, 0f, angle + spriteRotationOffset));
        SeekerProjectile proj = go.GetComponent<SeekerProjectile>();
        if (proj == null)
        {
            Debug.LogError("SeekerSystem: projectilePrefab is missing a SeekerProjectile component.", this);
            Destroy(go);
            return;
        }

        SeekerEffect effect = ability.abilityID == poisonAbilityID ? SeekerEffect.Poison : SeekerEffect.Mark;
        proj.Init(dir, projectileSpeed, effect, damageMultiplier, markDuration);
        
        if (GameServices.Classes != null)
            GameServices.Classes.StartCooldown(ability, ability.cooldown);
        Debug.Log($"SeekerSystem: fired {ability.displayName}, cooldown {ability.cooldown}s");
    }
}