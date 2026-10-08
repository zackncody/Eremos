using System.Collections.Generic;
using UnityEngine;

public class InventoryPersistence : MonoBehaviour, ISaveable
{
    public static InventoryPersistence Instance { get; private set; }

    public List<PersistedItem> savedInventoryItems = new List<PersistedItem>();
    public ItemData[] savedHotbarItems;
    public List<PersistedEquipment> savedEquipment = new List<PersistedEquipment>();
    public int expandedRows = 0;

    public string SaveKey => "inventoryPersistence";

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

    

    [ContextMenu("Admin: Clear All Saved Inventory Data")]
    private void AdminClearAll()
    {
        savedInventoryItems.Clear();
        savedHotbarItems = null;
        savedEquipment.Clear();
        Debug.Log("InventoryPersistence: cleared all saved data.");
    }

    // ── ISaveable ─────────────────────────────────────────────

    public object CaptureState()
    {
        var data = new InventorySaveDTO();

        foreach (var entry in savedInventoryItems)
        {
            data.items.Add(entry.itemData);
            data.itemCells.Add(entry.cell);
        }

        if (savedHotbarItems != null)
        {
            foreach (var item in savedHotbarItems)
                data.hotbarItems.Add(item); // null entries preserved for empty slots
        }

        foreach (var entry in savedEquipment)
        {
            data.equipItems.Add(entry.itemData);
            data.equipSlots.Add((int)entry.slot);
            data.equipSlotIndexes.Add(entry.slotIndex);
        }

        data.expandedRows = expandedRows;

        return data;
    }

    public void RestoreState(string json)
    {
        InventorySaveDTO data = JsonUtility.FromJson<InventorySaveDTO>(json);
        if (data == null) return;

        savedInventoryItems.Clear();
        for (int i = 0; i < data.items.Count; i++)
        {
            ItemData item = data.items[i];
            if (item == null) continue;

            item.ReloadReferences(); // repopulate icon/prefab refs from resourcePath

            savedInventoryItems.Add(new PersistedItem
            {
                itemData = item,
                cell = data.itemCells[i]
            });
        }

        savedHotbarItems = new ItemData[data.hotbarItems.Count];
        for (int i = 0; i < data.hotbarItems.Count; i++)
        {
            ItemData item = data.hotbarItems[i];
            item?.ReloadReferences();
            savedHotbarItems[i] = item;
        }

        savedEquipment.Clear();
        for (int i = 0; i < data.equipItems.Count; i++)
        {
            ItemData item = data.equipItems[i];
            if (item == null) continue;

            item.ReloadReferences();

            savedEquipment.Add(new PersistedEquipment
            {
                itemData = item,
                slot = (EquipSlot)data.equipSlots[i],
                slotIndex = data.equipSlotIndexes[i]
            });
        }

        expandedRows = data.expandedRows;

        Debug.Log("InventoryPersistence: restored from save data.");
        // NOTE: whatever code currently applies these lists to
        // InventoryManager/EquipmentManager on scene load should
        // pick this up automatically the next time that runs.
    }
}

[System.Serializable]
public class PersistedItem
{
    public ItemData itemData;
    public Vector2Int cell;
}

[System.Serializable]
public class PersistedEquipment
{
    public ItemData itemData;
    public EquipSlot slot;
    public int slotIndex;
}

[System.Serializable]
public class InventorySaveDTO
{
    public List<ItemData> items = new List<ItemData>();
    public List<Vector2Int> itemCells = new List<Vector2Int>();

    public List<ItemData> hotbarItems = new List<ItemData>();

    public List<ItemData> equipItems = new List<ItemData>();
    public List<int> equipSlots = new List<int>();
    public List<int> equipSlotIndexes = new List<int>();

    public int expandedRows;
}