using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public string spawnID = "entrance";

    private void Start()
    {
        // Skip normal spawn-point positioning while a save file is being
        // loaded — SaveManager already placed the player at the saved
        // position, and this would otherwise overwrite it.
        if (SaveManager.IsLoadingSave) return;

        string targetSpawn = PlayerPrefs.GetString("SpawnPoint", "entrance");
        if (targetSpawn == spawnID)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                player.transform.position = transform.position;
        }
    }
}