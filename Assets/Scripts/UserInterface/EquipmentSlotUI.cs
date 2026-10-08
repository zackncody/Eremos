using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [HideInInspector] public EquipSlot equipSlot;
    [HideInInspector] public int slotIndex;
    private Image iconImage;
    private Image backgroundImage;
    private Sprite emptySprite;

    public void Initialize(EquipSlot equipSlot, int index, Sprite background = null, Sprite empty = null)
    {
        this.equipSlot = equipSlot;
        slotIndex = index;
        emptySprite = empty;
        backgroundImage = GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(1f, 1f, 1f, 1f);
            if (background != null) backgroundImage.sprite = background;
        }
        GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(transform, false);
        RectTransform rt = iconObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(4f, 4f);
        rt.offsetMax = new Vector2(-4f, -4f);
        iconImage = iconObj.GetComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
        iconImage.enabled = false;
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (iconImage == null) return;
        ItemData item = EquipmentManager.Instance?.GetEquipped(equipSlot, slotIndex);
        if (item != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = new Color(1f, 1f, 1f, 1f);
            iconImage.enabled = true;
        }
        else if (emptySprite != null)
        {
            iconImage.sprite = emptySprite;
            iconImage.color = new Color(1f, 1f, 1f, 0.4f);
            iconImage.enabled = true;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Middle) return;
        if (EquipmentManager.Instance == null) return;
        ItemData unequipped = EquipmentManager.Instance.Unequip(equipSlot, slotIndex);
        if (unequipped == null) return;
        Debug.Log($"Unequipped {unequipped.itemName} from {equipSlot}");
        RefreshVisual();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (EquipmentManager.Instance == null)
        {
            Debug.LogError("EquipmentManager instance not found in scene!");
            return;
        }
        InventoryDragItem drag = eventData.pointerDrag?.GetComponent<InventoryDragItem>();
        if (drag == null) return;
        ItemData item = drag.itemData;
        if (item == null) return;

        EquipSlot itemSlot = EquipSlot.None;
        if (item.accessoryData != null && item.accessoryData.slot != EquipSlot.None)
            itemSlot = item.accessoryData.slot;
        else if (item.armourData != null && item.armourData.slot != EquipSlot.None)
            itemSlot = item.armourData.slot;

        if (itemSlot == EquipSlot.None)
        {
            Debug.Log($"{item.itemName} is not equippable.");
            return;
        }
        if (itemSlot != equipSlot)
        {
            Debug.Log($"{item.itemName} doesn't go in the {equipSlot} slot.");
            return;
        }
        EquipmentManager.Instance.TryEquip(item, slotIndex);
        RefreshVisual();
    }
}