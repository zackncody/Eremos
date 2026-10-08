using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    public InventoryGrid grid;
    private List<(Item item, Vector2Int cell)> pendingVisuals = new List<(Item, Vector2Int)>();
    private bool inventoryOpen = false;

    private List<(ItemData itemData, Vector2Int cell)> spawnedItems
        = new List<(ItemData, Vector2Int)>();

    private static Vector2Int SafeGridSize(Vector2Int size)
    {
        return new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
    }

    public void SetInventoryOpen(bool open)
    {
        GameServices.InventoryOpen = open;
        inventoryOpen = open;
        if (open)
            SpawnPendingVisuals();
    }
    private void OnDestroy()
    {
        GameServices.InventoryOpen = false;
    }
    public bool AddItem(Item item)
    {
        for (int y = 0; y < grid.rows; y++)
        {
            for (int x = 0; x < grid.columns; x++)
            {
                Vector2Int origin = new Vector2Int(x, y);
                if (grid.TryPlace(item, origin))
                {
                    if (inventoryOpen)
                        SpawnItemVisual(item, origin);
                    else
                        pendingVisuals.Add((item, origin));

                    if (PickupNotificationUI.Instance != null)
                        PickupNotificationUI.Instance.ShowPickup(item.icon, item.itemName);
                    else
                        Debug.LogWarning("PickupNotificationUI.Instance is null");
                    return true;
                }
            }
        }
        Debug.Log("Inventory Full");
        return false;
    }

    public bool AddItemDataToInventory(ItemData itemData)
    {
        Vector2Int size = SafeGridSize(itemData.gridSize);
        Debug.Log("AddItemDataToInventory: grid=" + (grid != null) +
                   " columns=" + (grid != null ? grid.columns.ToString() : "N/A") +
                   " rows=" + (grid != null ? grid.rows.ToString() : "N/A") +
                   " size=" + size);
        for (int y = 0; y < grid.rows; y++)
        {
            for (int x = 0; x < grid.columns; x++)
            {
                Vector2Int origin = new Vector2Int(x, y);
                if (grid.CanPlace(size, origin))
                {
                    if (inventoryOpen)
                        SpawnItemVisualFromData(itemData, origin);
                    else
                        pendingDataVisuals.Add((itemData, origin));

                    if (PickupNotificationUI.Instance != null)
                        PickupNotificationUI.Instance.ShowPickup(itemData.icon, itemData.itemName);
                    else
                        Debug.LogWarning("PickupNotificationUI.Instance is null");

                    return true;
                }
            }
        }
        Debug.Log("Inventory Full");
        return false;
    }

    public void AddItemData(ItemData itemData, Vector2Int cell)
    {
        Vector2Int size = SafeGridSize(itemData.gridSize);
        if (grid.CanPlace(size, cell))
        {
            pendingDataVisuals.Add((itemData, cell));
        }
        else
        {
            for (int y = 0; y < grid.rows; y++)
            {
                for (int x = 0; x < grid.columns; x++)
                {
                    Vector2Int origin = new Vector2Int(x, y);
                    if (grid.CanPlace(size, origin))
                    {
                        pendingDataVisuals.Add((itemData, origin));
                        return;
                    }
                }
            }
            Debug.LogWarning("Could not restore item: " + itemData.itemName);
        }
    }

    private List<(ItemData itemData, Vector2Int cell)> pendingDataVisuals
    = new List<(ItemData, Vector2Int)>();

    public void FlushPendingDataVisuals()
    {
        StartCoroutine(FlushNextFrame());
    }

    private IEnumerator FlushNextFrame()
    {
        yield return null;
        yield return null;
        foreach (var pending in pendingDataVisuals)
            SpawnItemVisualFromData(pending.itemData, pending.cell);
        pendingDataVisuals.Clear();
    }

    public List<(ItemData itemData, Vector2Int cell)> GetAllItems()
    {
        var all = new List<(ItemData, Vector2Int)>(spawnedItems);

        foreach (var pending in pendingVisuals)
            all.Add((ItemData.FromItem(pending.item), pending.cell));

        foreach (var pending in pendingDataVisuals)
            all.Add((pending.itemData, pending.cell));

        return all;
    }

    private void SpawnPendingVisuals()
    {
        StartCoroutine(SpawnPendingVisualsNextFrame());
    }

    private IEnumerator SpawnPendingVisualsNextFrame()
    {
        yield return null;
        foreach (var pending in pendingVisuals)
            SpawnItemVisual(pending.item, pending.cell);
        pendingVisuals.Clear();
    }

    private void SpawnItemVisual(Item item, Vector2Int cell)
    {
        GameObject prefabToUse = item.inventoryPrefab != null ? item.inventoryPrefab : item.itemPrefab;
        if (prefabToUse == null) { Debug.LogWarning("No prefab for: " + item.itemName); return; }

        GameObject visual = Instantiate(prefabToUse, grid.transform);
        visual.transform.SetAsLastSibling();

        RectTransform rt = visual.GetComponent<RectTransform>();
        if (rt == null) { Debug.LogWarning("Not a UI prefab!"); Destroy(visual); return; }

        Vector2Int size = SafeGridSize(item.gridSize);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(size.x * grid.cellSize, size.y * grid.cellSize);
        rt.anchoredPosition = new Vector2(cell.x * grid.cellSize, -cell.y * grid.cellSize);

        InventoryDragItem drag = visual.GetComponent<InventoryDragItem>();
        if (drag == null) drag = visual.AddComponent<InventoryDragItem>();

        Canvas canvas = grid.GetComponentInParent<Canvas>();
        drag.Initialize(grid, FindAnyObjectByType<HotbarManager>(), canvas);
        drag.itemData = ItemData.FromItem(item);
        drag.image = visual.GetComponent<UnityEngine.UI.Image>();

        grid.RemoveItem(item);
        grid.TryPlace(item.gridSize, cell, drag.GetInstanceID());

        spawnedItems.Add((ItemData.FromItem(item), cell));
    }

    private void SpawnItemVisualFromData(ItemData itemData, Vector2Int cell)
    {
        GameObject prefabToUse = itemData.inventoryPrefab != null ? itemData.inventoryPrefab : itemData.itemPrefab;

        if (prefabToUse == null) { Debug.LogWarning("No prefab for: " + itemData.itemName); return; }

        GameObject visual = Instantiate(prefabToUse, grid.transform);

        RectTransform rt = visual.GetComponent<RectTransform>();

        if (rt == null) { Debug.LogWarning("Not a UI prefab!"); Destroy(visual); return; }

        Vector2Int size = SafeGridSize(itemData.gridSize);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(size.x * grid.cellSize, size.y * grid.cellSize);
        rt.anchoredPosition = new Vector2(cell.x * grid.cellSize, -cell.y * grid.cellSize);

        InventoryDragItem drag = visual.GetComponent<InventoryDragItem>();
        if (drag == null) drag = visual.AddComponent<InventoryDragItem>();

        Canvas canvas = grid.GetComponentInParent<Canvas>();

        drag.Initialize(grid, FindAnyObjectByType<HotbarManager>(), canvas);
        drag.itemData = itemData;
        drag.image = visual.GetComponent<UnityEngine.UI.Image>();

        grid.TryPlace(size, cell, drag.GetInstanceID());
        spawnedItems.Add((itemData, cell));
    }

    public void RestoreItem(ItemData itemData, Vector2Int cell)
    {
        int tempID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        Vector2Int size = SafeGridSize(itemData.gridSize);
        if (!grid.TryPlace(size, cell, tempID)) return;

        GameObject prefabToUse = itemData.inventoryPrefab != null
            ? itemData.inventoryPrefab : itemData.itemPrefab;
        if (prefabToUse == null) return;

        GameObject visual = Instantiate(prefabToUse, grid.transform);
        visual.transform.SetAsLastSibling();

        RectTransform rt = visual.GetComponent<RectTransform>();
        if (rt == null) { Destroy(visual); return; }

        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(
            size.x * grid.cellSize,
            size.y * grid.cellSize);
        rt.anchoredPosition = new Vector2(
            cell.x * grid.cellSize,
            -cell.y * grid.cellSize);

        InventoryDragItem drag = visual.GetComponent<InventoryDragItem>();
        if (drag == null) drag = visual.AddComponent<InventoryDragItem>();

        Canvas canvas = grid.GetComponentInParent<Canvas>();
        drag.Initialize(grid, FindAnyObjectByType<HotbarManager>(), canvas);
        drag.itemData = itemData;
        drag.image = visual.GetComponent<UnityEngine.UI.Image>();

        grid.RemoveByID(tempID);
        grid.TryPlace(size, cell, drag.GetInstanceID());
    }

    public List<(ItemData item, Vector2Int cell)> GetSavedItems()
    {
        return pendingVisuals.ConvertAll(p => (ItemData.FromItem(p.item), p.cell));
    }

    public bool RemoveItemData(ItemData itemData, Vector2Int cell)
    {
        foreach (Transform child in grid.transform)
        {
            InventoryDragItem drag = child.GetComponent<InventoryDragItem>();
            if (drag == null || drag.itemData == null) continue;

            if (grid.TryGetPosition(drag.GetInstanceID(), out Vector2Int origin) && origin == cell)
            {
                if (drag.itemData.itemName != itemData.itemName) continue;

                grid.RemoveByID(drag.GetInstanceID());
                spawnedItems.RemoveAll(e => e.cell == cell);
                Destroy(child.gameObject);
                return true;
            }
        }

        for (int i = 0; i < pendingDataVisuals.Count; i++)
        {
            if (pendingDataVisuals[i].cell == cell && pendingDataVisuals[i].itemData.itemName == itemData.itemName)
            {
                pendingDataVisuals.RemoveAt(i);
                return true;
            }
        }

        for (int i = 0; i < pendingVisuals.Count; i++)
        {
            if (pendingVisuals[i].cell == cell && pendingVisuals[i].item.itemName == itemData.itemName)
            {
                pendingVisuals.RemoveAt(i);
                return true;
            }
        }

        Debug.LogWarning("RemoveItemData: could not find matching item at cell " + cell);
        return false;
    }

    public void ExpandInventory(int extraRows)
    {
        grid.ExpandGrid(extraRows);

        InventoryGridVisual visual = grid.GetComponent<InventoryGridVisual>();
        if (visual != null)
            visual.RefreshGrid();

        Debug.Log($"Inventory expanded by {extraRows} rows");
    }
}