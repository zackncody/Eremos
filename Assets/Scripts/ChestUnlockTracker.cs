using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent singleton tracking which ItemDrop ("chest") instances
/// have already been used/opened, keyed by each ItemDrop's unique
/// chestID. Implements ISaveable so this state survives save/load.
/// </summary>
public class ChestUnlockTracker : MonoBehaviour, ISaveable
{
    public static ChestUnlockTracker Instance { get; private set; }

    private HashSet<string> usedChestIDs = new HashSet<string>();

    public string SaveKey => "chestUnlocks";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

   

    public bool IsUsed(string chestID)
    {
        if (string.IsNullOrEmpty(chestID)) return false;
        return usedChestIDs.Contains(chestID);
    }

    public void MarkUsed(string chestID)
    {
        if (string.IsNullOrEmpty(chestID))
        {
            Debug.LogWarning("ChestUnlockTracker: tried to mark a chest used with no chestID assigned.");
            return;
        }

        usedChestIDs.Add(chestID);
    }

    [ContextMenu("Admin: Clear All Used Chests")]
    private void AdminClearAll()
    {
        usedChestIDs.Clear();
        Debug.Log("ChestUnlockTracker: cleared all used-chest data.");
    }

    // ── ISaveable ─────────────────────────────────────────────

    public object CaptureState()
    {
        var data = new ChestSaveDTO();
        data.usedChestIDs = new List<string>(usedChestIDs);
        return data;
    }

    public void RestoreState(string json)
    {
        ChestSaveDTO data = JsonUtility.FromJson<ChestSaveDTO>(json);
        if (data == null) return;

        usedChestIDs = new HashSet<string>(data.usedChestIDs);
        Debug.Log("ChestUnlockTracker: restored " + usedChestIDs.Count + " used chest(s).");
    }
}

[System.Serializable]
public class ChestSaveDTO
{
    public List<string> usedChestIDs = new List<string>();
}