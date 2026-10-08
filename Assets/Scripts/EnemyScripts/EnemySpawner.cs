using UnityEngine;
public class EnemySpawner : MonoBehaviour
{
    public GameObject prefab;
    public Transform spawnPoint;
    public int maxEnemies = 10;
    public float timeBetweenSpawns = 5f;
    private int currentEnemies = 0;

    void Start()
    {
        InvokeRepeating("SpawnPrefab", 0f, timeBetweenSpawns);
    }

    void SpawnPrefab()
    {
        if (prefab == null)
        {
            Debug.Log("No prefab assigned!");
            return;
        }
        if (currentEnemies >= maxEnemies)
        {
            return;
        }
        Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
        currentEnemies++;
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += () => currentEnemies--;
        }
    }
}