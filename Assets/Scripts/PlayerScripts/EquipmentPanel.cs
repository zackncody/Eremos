using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EquipSlotConfig
{
    public EquipSlot slot;
    public string label;
    public Sprite icon;
    public Sprite emptySlotSprite;
}

public class EquipmentPanel : MonoBehaviour
{
    [Header("Layout")]
    public float slotSize = 50f;
    public float padding = 8f;
    public Sprite slotBackground;

    [Header("Slot Config")]
    public List<EquipSlotConfig> slots = new List<EquipSlotConfig>
    {
        new EquipSlotConfig { slot = EquipSlot.Head,     label = "Head" },
        new EquipSlotConfig { slot = EquipSlot.Chest,    label = "Chest" },
        new EquipSlotConfig { slot = EquipSlot.Legs,     label = "Legs" },
        new EquipSlotConfig { slot = EquipSlot.Boots,    label = "Boots" },
        new EquipSlotConfig { slot = EquipSlot.Ring,     label = "Ring" },
        new EquipSlotConfig { slot = EquipSlot.Necklace, label = "Necklace" },
    };

    private List<EquipmentSlotUI> generatedSlots = new List<EquipmentSlotUI>();
    private Dictionary<EquipSlot, int> slotCounters = new Dictionary<EquipSlot, int>();

    private void Start()
    {
        GenerateSlots();
    }

    public void GenerateSlots()
    {
        foreach (var s in generatedSlots)
            if (s != null) Destroy(s.gameObject);
        generatedSlots.Clear();
        slotCounters.Clear();

        RectTransform panelRT = GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(1f, 1f);
        panelRT.anchorMax = new Vector2(1f, 1f);
        panelRT.pivot = new Vector2(0f, 1f);
        panelRT.anchoredPosition = new Vector2(10f, 40f);

        float totalHeight = padding + slots.Count * (slotSize + padding);
        float totalWidth = padding + slotSize + padding;
        panelRT.sizeDelta = new Vector2(totalWidth, totalHeight);

        Image panelBg = GetComponent<Image>();
        if (panelBg == null) panelBg = gameObject.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.4f);

        for (int i = 0; i < slots.Count; i++)
        {
            EquipSlotConfig config = slots[i];

            if (!slotCounters.ContainsKey(config.slot))
                slotCounters[config.slot] = 0;
            int slotIndex = slotCounters[config.slot]++;

            GameObject slotObj = new GameObject(config.label, typeof(RectTransform), typeof(Image));
            slotObj.transform.SetParent(transform, false);

            RectTransform rt = slotObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(slotSize, slotSize);
            rt.anchoredPosition = new Vector2(padding, -(padding + i * (slotSize + padding)));

            Image bg = slotObj.GetComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 1f);
            if (slotBackground != null) bg.sprite = slotBackground;

            EquipmentSlotUI slotUI = slotObj.AddComponent<EquipmentSlotUI>();
            slotUI.Initialize(config.slot, slotIndex, slotBackground, config.emptySlotSprite);

            generatedSlots.Add(slotUI);
        }
    }

    public void AddSlot(EquipSlotConfig config)
    {
        slots.Add(config);
        GenerateSlots();
    }

    public void RefreshAll()
    {
        foreach (var s in generatedSlots)
            if (s != null) s.RefreshVisual();
    }
}