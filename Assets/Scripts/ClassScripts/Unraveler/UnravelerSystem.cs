using UnityEngine;

public class UnravelerSystem : MonoBehaviour
{
    public static UnravelerSystem Instance { get; private set; }

    [Header("Class")]
    [SerializeField] private string requiredClassID = "unraveler";
    [SerializeField] private string healAbilityID = "unraveler_heal";
    [SerializeField] private string refillAbilityID = "unraveler_refill";

    [Header("Input")]
    [SerializeField] private KeyCode activateKey = KeyCode.Q;

    [Header("Heal ability (asset 'cost' = stamina spent)")]
    [SerializeField] private int healAmount = 30;

    [Header("Refill ability (asset 'cost' = health lost)")]
    [SerializeField] private float staminaRefillAmount = 3f;

    private PlayerHealth playerHealth;
    private bool windowActive;
    private float windowEndTime;
    

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

    private ClassAbility GetActiveAbility()
    {
        ClassManager cm = GameServices.Classes;
        if (cm == null || cm.Current == null || cm.Current.classID != requiredClassID) return null;

        ClassAbility a = cm.CurrentAbility;
        if (a == null) return null;
        if (a.abilityID != healAbilityID && a.abilityID != refillAbilityID) return null;
        return a;
    }

    private void Update()
    {
        ClassAbility ability = GetActiveAbility();
        if (ability == null)
        {
            if (windowActive) EndWindow();
            return;
        }

        if (windowActive && Time.time >= windowEndTime)
            EndWindow();

        if (Input.GetKeyDown(activateKey) && !GameServices.InputBlocked && !GameServices.Classes.IsOnCooldown(ability))
            TryActivate(ability);
    }

    private void TryActivate(ClassAbility ability)
    {
        bool ok = ability.abilityID == healAbilityID ? DoHeal(ability) : DoRefill(ability);
        if (!ok) return;

        ClassManager cm = GameServices.Classes;
        ClassData c = cm.Current;
        int level = cm.GetLevel(c);

        float duration = c.baseDuration + c.durationPerLevel * (level - 1);
        windowActive = true;
        windowEndTime = Time.time + duration;
        cm.SetAbilityActive(true);

        float cooldown = ability.cooldown > 0f
            ? ability.cooldown
            : Mathf.Max(0f, c.baseCooldown - c.cooldownReductionPerLevel * (level - 1));
        cm.StartCooldown(ability, windowEndTime + cooldown - Time.time);
        Debug.Log($"UnravelerSystem: {ability.displayName} used. Window {duration:F1}s, cooldown {cooldown:F1}s");
    }

    private void EndWindow()
    {
        windowActive = false;
        if (GameServices.Classes != null)
            GameServices.Classes.SetAbilityActive(false);
    }

    private PlayerHealth GetHealth()
    {
        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();
        return playerHealth;
    }

    private bool DoHeal(ClassAbility ability)
    {
        PlayerHealth hp = GetHealth();
        PlayerResourceManager res = PlayerResourceManager.Instance;
        if (hp == null || res == null) return false;

        if (hp.health >= hp.maxHealth)
        {
            Debug.Log("UnravelerSystem: heal skipped, health is full");
            return false;
        }

        if (!res.TrySpend(ability.cost))
        {
            Debug.Log("UnravelerSystem: heal failed, not enough stamina");
            return false;
        }

        hp.Heal(healAmount);
        return true;
    }

    private bool DoRefill(ClassAbility ability)
    {
        PlayerHealth hp = GetHealth();
        PlayerResourceManager res = PlayerResourceManager.Instance;
        if (hp == null || res == null) return false;

        if (res.currentResource >= res.maxResource)
        {
            Debug.Log("UnravelerSystem: refill skipped, stamina is full");
            return false;
        }

        if (!hp.LoseHealth(Mathf.RoundToInt(ability.cost)))
        {
            Debug.Log("UnravelerSystem: refill failed, not enough health");
            return false;
        }

        res.Refill(staminaRefillAmount);
        return true;
    }
}