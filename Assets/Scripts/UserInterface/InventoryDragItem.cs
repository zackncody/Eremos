using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDragItem : MonoBehaviour,
    IDragHandler, IEndDragHandler, IPointerClickHandler,
    IBeginDragHandler, IPointerDownHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public ItemData itemData;
    public Image image;
    private InventoryGrid grid;
    private Vector2Int originCell;
    private Transform originalParent;
    private Canvas rootCanvas;
    private HotbarManager hotbar;
    private int instanceID;

    public void Initialize(InventoryGrid inventoryGrid, HotbarManager hotbarManager, Canvas canvas)
    {
        grid = inventoryGrid;
        hotbar = hotbarManager;
        rootCanvas = canvas;
        instanceID = GetInstanceID();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData != null && TooltipUI.Instance != null)
            TooltipUI.Instance.Show(itemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
            TooltipUI.Instance.Hide();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("PointerDown on: " + gameObject.name);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //if (hotbar == null) { Debug.LogError("Hotbar null on right click"); return; }
            //if (itemData == null) { Debug.LogError("ItemData null on right click"); return; }

            //Debug.Log("Assigning " + itemData.itemName + " to hotbar slot " + hotbar.selectedIndex);
            hotbar.AssignItem(hotbar.selectedIndex, itemData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (grid == null || rootCanvas == null) return;

        if (TooltipUI.Instance != null)
            TooltipUI.Instance.Hide();

        if (grid.TryGetPosition(instanceID, out Vector2Int trackedCell))
            originCell = trackedCell;
        else
            originCell = grid.WorldToCell(GetLocalGridPos(eventData.position));

        grid.RemoveByID(instanceID);
        originalParent = transform.parent;
        transform.SetParent(rootCanvas.transform);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        transform.position = eventData.position;
    }

    //public void OnDrag(PointerEventData eventData)
    //{
    //    if (eventData.button != PointerEventData.InputButton.Left) return;

    //    Vector3[] corners = new Vector3[4];
    //    grid.GetComponent<RectTransform>().GetWorldCorners(corners);

    //    float itemHalfWidth = (itemData.gridSize.x * grid.cellSize) * 0.5f;
    //    float itemHalfHeight = (itemData.gridSize.y * grid.cellSize) * 0.5f;

    //    float clampedX = Mathf.Clamp(
    //        eventData.position.x,
    //        corners[0].x + itemHalfWidth,
    //        corners[2].x - itemHalfWidth);

    //    float clampedY = Mathf.Clamp(
    //        eventData.position.y,
    //        corners[0].y + itemHalfHeight,
    //        corners[2].y - itemHalfHeight);

    //    transform.position = new Vector2(clampedX, clampedY);
    //}

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        image.raycastTarget = true;
        transform.SetParent(grid.transform);

        bool overGrid = RectTransformUtility.RectangleContainsScreenPoint(
            grid.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera);

        if (overGrid)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                grid.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPos);

            Vector2Int targetCell = grid.WorldToCell(localPos);
            targetCell.x = Mathf.Clamp(targetCell.x, 0, grid.columns - itemData.gridSize.x);
            targetCell.y = Mathf.Clamp(targetCell.y, 0, grid.rows - itemData.gridSize.y);

            if (grid.TryPlace(itemData.gridSize, targetCell, instanceID))
            {
                SnapToGridParent(targetCell);
                return;
            }
        }

        ReturnToOrigin();
    }

    private void SnapToGridParent(Vector2Int cell)
    {
        transform.SetParent(grid.transform);
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(
            itemData.gridSize.x * grid.cellSize,
            itemData.gridSize.y * grid.cellSize);
        rt.anchoredPosition = new Vector2(
            cell.x * grid.cellSize,
            -cell.y * grid.cellSize);
        transform.SetAsLastSibling();
    }

    private void ReturnToOrigin()
    {
        grid.RemoveByID(instanceID);
        grid.ForcePlace(itemData.gridSize, originCell, instanceID);
        transform.SetParent(grid.transform);
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(
            itemData.gridSize.x * grid.cellSize,
            itemData.gridSize.y * grid.cellSize);
        rt.anchoredPosition = new Vector2(
            originCell.x * grid.cellSize,
            -originCell.y * grid.cellSize);
        transform.SetAsLastSibling();
    }

    private Vector2 GetLocalGridPos(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            grid.GetComponent<RectTransform>(),
            screenPos,
            rootCanvas.worldCamera,
            out Vector2 local);
        return local;
    }
}