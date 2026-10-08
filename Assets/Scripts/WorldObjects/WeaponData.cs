using UnityEngine;

public enum WeaponType { Sword, Bow, Magic, Summon }
public enum MagicCastType { Projectile, AOEBurst }

[System.Serializable]
public class WeaponData
{
    public float resourceCost = 1f;
    public WeaponType weaponType = WeaponType.Sword;
    public int damage = 5;
    public float attackSpeed = 1f;
    public float swingDuration = 0.2f;
    public float swingArc = 90f;
    public float damageInterval = 0.5f;
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.15f;
    public float rotationOffset = 0f;
    [Range(0f, 1f)] public float burnChance = 0f;
    [Range(0f, 1f)] public float poisonChance = 0f;
    [Range(0f, 1f)] public float slowChance = 0f;
    [Range(0f, 1f)] public float bleedChance = 0f;

    [Header("Bow Only")]
    public GameObject arrowPrefab;
    public float arrowSpeed = 18f;
    public float pullBackDuration = 0.4f;

    [Header("Magic - General")]
    public MagicCastType magicCastType = MagicCastType.Projectile;
    public float castTime = 0.3f;
    public float manaCost = 10f; // Not enforced yet — hook up to your mana system if/when you add one.

    [Header("Magic - Projectile")]
    public GameObject magicProjectilePrefab;
    public float magicProjectileSpeed = 14f;

    [Header("Magic - AOE Burst")]
    public GameObject magicAOEPrefab;
    public float magicAOERadius = 2.5f;

    [Header("Summon Only")]
    public GameObject summonPrefab;
    public int maxActiveSummons = 1;
    public float summonDuration = 15f;      // 0 = lasts until killed, no auto-despawn
    public float summonCooldown = 1f;
    public int summonDamage = 4;
    public float summonAttackRange = 1f;
    public float summonAttackInterval = 1f;
    public float summonMoveSpeed = 3f;
    public float summonFollowDistance = 2f;
    public float summonAggroRange = 5f;
    public float summonExtendPerHit = 0f;   // seconds the summon's own hits add back to its timer (0 = off)
}