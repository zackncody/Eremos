using UnityEngine;
using UnityEngine.UI;

public class HotbarManager : MonoBehaviour
{
    [SerializeField] private Image[] slotHighlights;
    [SerializeField] private Image[] slotIcons;
    [SerializeField] private Sprite emptySlotSprite;
    private ItemData[] hotbarItems;
    private Sprite[] defaultSlotSprites;
    public int selectedIndex = 0;
    public event System.Action<ItemData> OnSelectionChanged;

    public ItemData[] GetAllItems() => hotbarItems;

    private void Awake()
    {
        int size = Mathf.Min(slotHighlights.Length, slotIcons.Length);
        hotbarItems = new ItemData[size];

        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (slotIcons[i] != null)
            {
                slotIcons[i].sprite = null;
                slotIcons[i].color = new Color(1f, 1f, 1f, 0f);
                slotIcons[i].enabled = false;
                slotIcons[i].raycastTarget = false;
            }

            if (slotHighlights[i] != null)
                slotHighlights[i].raycastTarget = false;

            Transform slotParent = slotIcons[i]?.transform.parent;
            if (slotParent != null)
            {
                Image slotImage = slotParent.GetComponent<Image>();
                if (slotImage == null)
                {
                    slotImage = slotParent.gameObject.AddComponent<Image>();
                    slotImage.color = new Color(0f, 0f, 0f, 0f);
                }
                slotImage.raycastTarget = true;

                HotbarSlotUI slotUI = slotParent.GetComponent<HotbarSlotUI>();
                if (slotUI == null) slotUI = slotParent.gameObject.AddComponent<HotbarSlotUI>();
                slotUI.Initialize(i, this);
            }
        }

        foreach (Image highlight in slotHighlights)
            if (highlight != null) highlight.enabled = false;
    }

    public void SelectSlotPublic(int index)
    {
        SelectSlot(index);
    }

    private void Update()
    {
        for (int i = 0; i < hotbarItems.Length; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i != selectedIndex)
                SelectSlot(i);

        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll > 0f) SelectSlot((selectedIndex - 1 + hotbarItems.Length) % hotbarItems.Length);
        else if (scroll < 0f) SelectSlot((selectedIndex + 1) % hotbarItems.Length);
    }

    private void SelectSlot(int index)
    {
        selectedIndex = index;
        for (int i = 0; i < slotHighlights.Length; i++)
            if (slotHighlights[i] != null)
                slotHighlights[i].enabled = (i == selectedIndex);

        OnSelectionChanged?.Invoke(hotbarItems != null ? hotbarItems[selectedIndex] : null);
    }

    public void AssignItem(int index, ItemData item)
    {
        if (index < 0 || index >= hotbarItems.Length) return;

        if (item != null && string.IsNullOrEmpty(item.itemName))
        {
            return;
        }

        hotbarItems[index] = item;

        if (slotIcons != null && index < slotIcons.Length && slotIcons[index] != null)
        {
            if (item != null && item.icon != null)
            {
                slotIcons[index].sprite = item.icon;
                slotIcons[index].color = new Color(1f, 1f, 1f, 1f);
                slotIcons[index].enabled = true;
            }
            else
            {
                slotIcons[index].sprite = null;
                slotIcons[index].color = new Color(1f, 1f, 1f, 0f);
                slotIcons[index].enabled = false;
            }
        }

        if (index == selectedIndex)
            OnSelectionChanged?.Invoke(hotbarItems[index]);
    }

    public void ClearSlot(int index)
    {
        if (index < 0 || index >= hotbarItems.Length) return;
        hotbarItems[index] = null;

        if (slotIcons != null && index < slotIcons.Length && slotIcons[index] != null)
        {
            slotIcons[index].sprite = null;
            slotIcons[index].color = new Color(1f, 1f, 1f, 0f);
            slotIcons[index].enabled = false;
        }

        if (index == selectedIndex)
            OnSelectionChanged?.Invoke(null);
    }

    public void RefreshIcons()
    {
        for (int i = 0; i < hotbarItems.Length; i++)
        {
            if (slotIcons[i] == null) continue;

            bool hasValidItem = hotbarItems[i] != null &&
                               !string.IsNullOrEmpty(hotbarItems[i].itemName) &&
                               hotbarItems[i].icon != null;

            if (hasValidItem)
            {
                slotIcons[i].sprite = hotbarItems[i].icon;
                slotIcons[i].color = new Color(1f, 1f, 1f, 1f);
                slotIcons[i].enabled = true;
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].color = new Color(1f, 1f, 1f, 0f);
                slotIcons[i].enabled = false;
                hotbarItems[i] = null;
            }
        }
    }

    public ItemData GetSelectedItem() => hotbarItems[selectedIndex];
}