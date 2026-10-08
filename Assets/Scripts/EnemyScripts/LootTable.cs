using UnityEngine;

[System.Serializable]
public class LootEntry
{
    public GameObject itemPrefab;
    public float dropChance = 1f;
}

public class LootTable : MonoBehaviour
{
    [Header("Drops")]
    public LootEntry[] drops;

    [Header("Scatter Settings")]
    public float scatterRadius = 0.5f;

    public void SpawnLoot(Vector3 position)
    {
        if (drops == null || drops.Length == 0) return;

        foreach (var entry in drops)
        {
            if (entry.itemPrefab == null) continue;
            if (Random.value <= entry.dropChance)
            {
                Vector2 scatter = Random.insideUnitCircle * scatterRadius;
                Vector3 spawnPos = position + new Vector3(scatter.x, scatter.y, 0f);
                Instantiate(entry.itemPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}