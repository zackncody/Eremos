using UnityEngine;


[System.Serializable]
public class BossPhaseData
{
    [Header("Phase Trigger")]
    [Tooltip("Boss enters this phase when HP drops below this % (0-1)")]
    public float healthThreshold = 1f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseRange = 12f;

    [Header("Ranged Attack")]
    public float rangedCooldown = 3f;
    public int projectileBurstCount = 5;
    public float projectileSpreadAngle = 30f;
    public float projectileSpeed = 6f;

    [Header("Charge Attack")]
    public float chargeCooldown = 5f;
    public float chargeSpeed = 14f;
    public float chargeDuration = 0.4f;
    public float chargeWindupTime = 0.6f;
    public float chargeDetectRange = 8f;

    [Header("AoE Ground Slam")]
    public float aoeCooldown = 7f;
    public float aoeDamage = 40f;
    public float aoeInitialRadius = 0.1f;
    public float aoeFinalRadius = 5f;
    public float aoeExpandSpeed = 5f;
    public float aoeWindupTime = 1f;

    [Header("Damage")]
    public float meleeDamage = 25f;
    public float projectileDamage = 15f;
    public float contactDamage = 10f;
}