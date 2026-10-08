using UnityEngine;
using System.Collections;

public class InventoryLoader : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private HotbarManager hotbarManager;
    [SerializeField] private EquipmentManager equipmentManager;
    [SerializeField] private EquipmentPanel equipmentPanel;
    [SerializeField] private InventoryGrid inventoryGrid;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private CanvasGroup inventoryCanvasGroup;

    public static bool IsRestoring { get; private set; } = false;

    private void Awake()
    {
        InventoryLoader.IsRestoring = true; // set immediately, before anything else runs
    }

    private void Start()
    {
        StartCoroutine(WaitThenRestore());
    }

    private IEnumerator WaitThenRestore()
    {
        IsRestoring = true;
        yield return null;
        yield return null;

        if (SaveManager.Instance != null)
        {
            while (SaveManager.IsLoadingSave)
                yield return null;
            yield return null;
        }

        Restore();
    }

    public static void Save()
    {
        if (InventoryPersistence.Instance == null) return;
        InventoryPersistence data = InventoryPersistence.Instance;
        data.savedInventoryItems.Clear();
        data.savedEquipment.Clear();

        InventoryManager manager = FindAnyObjectByType<InventoryManager>();
        if (manager != null)
        {
            data.expandedRows = manager.grid.rows - 8;
            foreach (var entry in manager.GetAllItems())
                data.savedInventoryItems.Add(new PersistedItem
                {
                    itemData = entry.itemData,
                    cell = entry.cell
                });
        }

        Debug.Log("InventoryLoader.Save: saved " + data.savedInventoryItems.Count + " items. manager found=" + (manager != null));

        HotbarManager hotbar = FindAnyObjectByType<HotbarManager>();
        if (hotbar != null)
        {
            ItemData[] allItems = hotbar.GetAllItems();
            ItemData[] cleanItems = new ItemData[allItems.Length];
            for (int i = 0; i < allItems.Length; i++)
            {
                if (allItems[i] != null && !string.IsNullOrEmpty(allItems[i].itemName))
                    cleanItems[i] = allItems[i];
                else
                    cleanItems[i] = null;
            }
            data.savedHotbarItems = cleanItems;
        }

        EquipmentManager equipment = FindAnyObjectByType<EquipmentManager>();
        if (equipment != null)
            foreach (var entry in equipment.GetAllEquipped())
                data.savedEquipment.Add(new PersistedEquipment
                {
                    itemData = entry.itemData,
                    slot = entry.slot,
                    slotIndex = entry.slotIndex
                });
    }

    private void Restore()
    {
        if (InventoryPersistence.Instance == null)
        {
            Debug.LogError("InventoryPersistence.Instance is null!");
            return;
        }
        InventoryPersistence data = InventoryPersistence.Instance;

        Debug.Log($"Restore called — items: {data.savedInventoryItems.Count}, hotbar: {data.savedHotbarItems?.Length}, equipment: {data.savedEquipment.Count}");

        // Log each item
        foreach (var entry in data.savedInventoryItems)
            Debug.Log($"  Item: {entry.itemData?.itemName ?? "NULL"} at cell {entry.cell}, resourcePath: {entry.itemData?.resourcePath ?? "EMPTY"}");

        //if (InventoryPersistence.Instance == null) return;
        //InventoryPersistence data = InventoryPersistence.Instance;

        Debug.Log("InventoryLoader.Restore: restoring " + data.savedInventoryItems.Count + " items.");

        if (data.expandedRows > 0)
            inventoryManager.ExpandInventory(data.expandedRows);

        if (data.savedInventoryItems.Count == 0 &&
            data.savedHotbarItems == null &&
            data.savedEquipment.Count == 0) return;

        foreach (var entry in data.savedInventoryItems)
            entry.itemData?.ReloadReferences();
        if (data.savedHotbarItems != null)
            foreach (var item in data.savedHotbarItems)
                item?.ReloadReferences();
        foreach (var entry in data.savedEquipment)
            entry.itemData?.ReloadReferences();

        inventoryPanel.SetActive(true);
        if (inventoryCanvasGroup != null)
        {
            inventoryCanvasGroup.alpha = 0f;
            inventoryCanvasGroup.blocksRaycasts = false;
        }
        inventoryManager.SetInventoryOpen(true);

        for (int i = 0; i < hotbarManager.GetAllItems().Length; i++)
            hotbarManager.ClearSlot(i);

        if (data.savedHotbarItems != null)
            for (int i = 0; i < data.savedHotbarItems.Length; i++)
                if (data.savedHotbarItems[i] != null)
                    hotbarManager.AssignItem(i, data.savedHotbarItems[i]);

        foreach (var entry in data.savedInventoryItems)
        {
            if (entry.itemData == null) continue;
            inventoryManager.AddItemData(entry.itemData, entry.cell);
        }

        inventoryManager.FlushPendingDataVisuals();

        foreach (var entry in data.savedEquipment)
        {
            if (entry.itemData == null) continue;
            equipmentManager.TryEquip(entry.itemData, entry.slotIndex);
        }

        equipmentPanel.RefreshAll();
        StartCoroutine(ClosePanelAfterRestore());
    }

    private IEnumerator ClosePanelAfterRestore()
    {
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();

        inventoryPanel.SetActive(false);
        inventoryManager.SetInventoryOpen(false);

        for (int i = 0; i < hotbarManager.GetAllItems().Length; i++)
            hotbarManager.ClearSlot(i);

        if (InventoryPersistence.Instance?.savedHotbarItems != null)
            for (int i = 0; i < InventoryPersistence.Instance.savedHotbarItems.Length; i++)
                if (InventoryPersistence.Instance.savedHotbarItems[i] != null)
                    hotbarManager.AssignItem(i, InventoryPersistence.Instance.savedHotbarItems[i]);

        hotbarManager.RefreshIcons();

        if (inventoryCanvasGroup != null)
        {
            inventoryCanvasGroup.alpha = 1f;
            inventoryCanvasGroup.blocksRaycasts = true;
        }

        IsRestoring = false;
    }
}