using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private SpawnPoint spawnPoint;
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        activated = true;

        if (spawnPoint != null && RespawnManager.Instance != null)
            RespawnManager.Instance.SetSpawnPoint(spawnPoint.spawnID);

        Debug.Log("Checkpoint reached: " + gameObject.name);
    }
}