using UnityEngine;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour
{
    [Header("Grid Config")]
    public int columns = 10;
    public int rows = 8;
    public float cellSize = 50f;

    private string[,] cells;
    private Dictionary<string, Vector2Int> placedItems = new Dictionary<string, Vector2Int>();
    private Dictionary<int, Vector2Int> placedByID = new Dictionary<int, Vector2Int>();
    private Dictionary<int, Vector2Int> itemSizes = new Dictionary<int, Vector2Int>();

    void Awake()
    {
        cells = new string[columns, rows];
    }

    void OnEnable()
    {
        if (cells == null)
            cells = new string[columns, rows];
    }

    public bool CanPlace(Vector2Int size, Vector2Int origin)
    {
        if (cells == null) cells = new string[columns, rows];

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                if (!InBounds(cell)) return false;
                if (cells[cell.x, cell.y] != null) return false;
            }
        }
        return true;
    }

    public bool TryPlace(Vector2Int size, Vector2Int origin, int instanceID)
    {
        if (!CanPlace(size, origin)) return false;

        string id = instanceID.ToString();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                cells[cell.x, cell.y] = id;
            }
        }
        placedByID[instanceID] = origin;
        itemSizes[instanceID] = size;
        return true;
    }

    public bool TryPlace(Item item, Vector2Int origin)
    {
        return TryPlace(item.gridSize, origin, item.GetInstanceID());
    }

    public void RemoveItem(Item item)
    {
        RemoveByID(item.GetInstanceID());
    }

    public void RemoveByID(int instanceID)
    {
        if (!placedByID.ContainsKey(instanceID)) return;

        Vector2Int origin = placedByID[instanceID];
        Vector2Int size = itemSizes.ContainsKey(instanceID) ? itemSizes[instanceID] : Vector2Int.one;
        string id = instanceID.ToString();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                if (InBounds(cell) && cells[cell.x, cell.y] == id)
                    cells[cell.x, cell.y] = null;
            }
        }

        placedByID.Remove(instanceID);
        itemSizes.Remove(instanceID);
    }

    public bool InBounds(Vector2Int cell)
        => cell.x >= 0 && cell.x < columns && cell.y >= 0 && cell.y < rows;

    public Vector2Int WorldToCell(Vector2 localPos)
    {
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(-localPos.y / cellSize);
        return new Vector2Int(x, y);
    }

    public void ExpandGrid(int extraRows)
    {
        int newRows = rows + extraRows;

        string[,] newCells = new string[columns, newRows];

        for (int x = 0; x < columns; x++)
            for (int y = 0; y < rows; y++)
                newCells[x, y] = cells[x, y];

        cells = newCells;
        rows = newRows;

        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
            rt.sizeDelta = new Vector2(columns * cellSize, rows * cellSize);

        Debug.Log($"Grid expanded to {columns}x{rows}");

    }

    public void ForcePlace(Vector2Int size, Vector2Int origin, int instanceID)
    {
        if (cells == null) cells = new string[columns, rows];
        string id = instanceID.ToString();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                if (InBounds(cell))
                    cells[cell.x, cell.y] = id;
            }
        }
        placedByID[instanceID] = origin;
        itemSizes[instanceID] = size;
    }
    public bool TryGetPosition(int instanceID, out Vector2Int position)
    {
        if (placedByID.ContainsKey(instanceID))
        {
            position = placedByID[instanceID];
            return true;
        }
        position = Vector2Int.zero;
        return false;
    }
}