using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    private int slotIndex;
    private HotbarManager hotbar;

    public void Initialize(int index, HotbarManager manager)
    {
        slotIndex = index;
        hotbar = manager;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"OnDrop fired on hotbar slot {slotIndex}");

        InventoryDragItem drag = eventData.pointerDrag?.GetComponent<InventoryDragItem>();
        Debug.Log($"pointerDrag: {eventData.pointerDrag?.name}, drag component found: {drag != null}");

        if (drag == null) return;

        Debug.Log($"drag.itemData null: {drag.itemData == null}, item name: {drag.itemData?.itemName}");

        if (drag.itemData == null) return;

        hotbar.AssignItem(slotIndex, drag.itemData);
        Debug.Log($"AssignItem called for slot {slotIndex}");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            hotbar.SelectSlotPublic(slotIndex);
    }
}