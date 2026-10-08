using UnityEngine;

/// <summary>
/// Central registry for core persistent systems (player, managers, etc).
/// Scripts should access these via GameServices.X instead of holding
/// their own [SerializeField] references or separate static Instance fields.
/// </summary>
public static class GameServices
{
    public static PlayerCode Player { get; private set; }
    public static GameManager Game { get; private set; }
    public static DeathManager Death { get; private set; }
    public static DayNightCycle DayNight { get; private set; }
    public static ClassManager Classes { get; private set; }
    public static RageSystem Rage { get; private set; }

    public static void RegisterPlayer(PlayerCode player) => Player = player;
    public static void ClearPlayer() => Player = null;

    public static void RegisterGame(GameManager game) => Game = game;
    public static void ClearGame() => Game = null;

    public static void RegisterDeath(DeathManager death) => Death = death;
    public static void ClearDeath() => Death = null;

    public static void RegisterDayNight(DayNightCycle dayNight) => DayNight = dayNight;
    public static void ClearDayNight() => DayNight = null;

    public static void RegisterClasses(ClassManager classes) => Classes = classes;
    public static void ClearClasses() => Classes = null;

    public static void RegisterRage(RageSystem rage) => Rage = rage;
    public static void ClearRage() => Rage = null;

    public static bool InventoryOpen { get; set; }

    // True while a menu is open or the game is paused or the player is dead (timeScale 0).
    public static bool InputBlocked => InventoryOpen || Time.timeScale == 0f;

    // Reports player damage to class XP, but only if the target is an enemy or boss.
    public static void ReportPlayerDamage(Component target, float amount)
    {
        if (Classes == null || target == null) return;

        bool isEnemy = target.GetComponentInParent<EnemyHealth>() != null
                    || target.GetComponentInChildren<EnemyHealth>() != null
                    || target.GetComponentInParent<BossController>() != null
                    || target.GetComponentInChildren<BossController>() != null;

        Debug.Log($"ReportPlayerDamage: target={target.name}, isEnemy={isEnemy}, amount={amount}");

        if (isEnemy)
            Classes.ReportDamage(amount);
    }
}