using UnityEngine;
using System.Collections.Generic;

public class SummonController : MonoBehaviour
{
    [Header("Stats")]
    public GameObject summonPrefab;
    public int maxActiveSummons = 1;
    public float summonCooldown = 1f;   // starts when a summon ENDS, not when it's cast
    public int damage = 4;
    public float attackRange = 1f;
    public float attackInterval = 1f;
    public float moveSpeed = 3f;
    public float followDistance = 2f;
    public float aggroRange = 5f;
    public float summonDuration = 15f;
    public float summonExtendPerHit = 0f;
    public float resourceCost = 1f;

    [Header("Spawn")]
    public Vector2 spawnOffset = Vector2.zero;

    private Transform playerTransform;
    private float cooldownEndTime = 0f;
    private List<GameObject> activeSummons = new List<GameObject>();

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        if (playerTransform == null)
            Debug.LogError("SummonController: No GameObject tagged 'Player' found.", this);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Clean up references to summons that died/despawned on their own.
        activeSummons.RemoveAll(s => s == null);

        if (Input.GetMouseButtonDown(0))
            TrySummon();
    }

    private void TrySummon()
    {
        if (Time.time < cooldownEndTime)
        {
            Debug.Log($"Summon on cooldown: {cooldownEndTime - Time.time:F1}s left.");
            return;
        }
        if (activeSummons.Count >= maxActiveSummons)
        {
            Debug.Log("Max active summons reached.");
            return;
        }
        if (summonPrefab == null)
        {
            Debug.LogError("SummonController: summonPrefab is not assigned.", this);
            return;
        }
        if (!PlayerResourceManager.Instance.TrySpend(resourceCost))
            return;

        Vector3 spawnPos = playerTransform.position + (Vector3)spawnOffset;
        GameObject summon = Instantiate(summonPrefab, spawnPos, Quaternion.identity);
        SummonedAlly ally = summon.GetComponent<SummonedAlly>();
        if (ally != null)
        {
            ally.Init(playerTransform, damage, attackRange, attackInterval,
                      moveSpeed, followDistance, aggroRange, summonDuration, summonExtendPerHit);
            ally.OnEnded += HandleSummonEnded;
        }
        else
        {
            Debug.LogError("SummonController: summonPrefab is missing a SummonedAlly component.", this);
        }
        activeSummons.Add(summon);
    }

    private void HandleSummonEnded(SummonedAlly ally)
    {
        cooldownEndTime = Time.time + summonCooldown;
        Debug.Log($"SummonController: summon ended, cooldown {summonCooldown}s");
    }

    public void ApplyWeaponData(WeaponData data)
    {
        if (data.weaponType != WeaponType.Summon) return;
        summonPrefab = data.summonPrefab;
        maxActiveSummons = data.maxActiveSummons;
        summonCooldown = data.summonCooldown;
        damage = data.summonDamage;
        attackRange = data.summonAttackRange;
        attackInterval = data.summonAttackInterval;
        moveSpeed = data.summonMoveSpeed;
        followDistance = data.summonFollowDistance;
        aggroRange = data.summonAggroRange;
        summonDuration = data.summonDuration;
        summonExtendPerHit = data.summonExtendPerHit;
        resourceCost = data.resourceCost;
    }
}