using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    private Dictionary<(EquipSlot, int), ItemData> equipped
        = new Dictionary<(EquipSlot, int), ItemData>();

    public int TotalDefense { get; private set; }
    public int TotalBonusDamage { get; private set; }
    public float TotalAttackSpeedBonus { get; private set; }
    public float TotalBurnChanceBonus { get; private set; }
    public float TotalPoisonChanceBonus { get; private set; }
    public float TotalSlowChanceBonus { get; private set; }
    public float TotalBleedChanceBonus { get; private set; }
    public float TotalKnockbackBonus { get; private set; }
    public float TotalFireResistance { get; private set; }
    public float TotalPoisonResistance { get; private set; }
    public float TotalSlowResistance { get; private set; }
    public float TotalBleedResistance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool TryEquip(ItemData item, int slotIndex = 0)
    {
        EquipSlot slot = GetSlotForItem(item);
        if (slot == EquipSlot.None) return false;
        equipped[(slot, slotIndex)] = item;
        RecalculateStats();

        if (item.armourData != null && PlayerArmourVisuals.Instance != null)
            PlayerArmourVisuals.Instance.EquipArmour(slot, item.armourData);

        if (item.accessoryData != null && item.accessoryData.providesLight && PlayerLantern.Instance != null)
            PlayerLantern.Instance.SetLanternEquipped(true);

        return true;
    }

    public ItemData Unequip(EquipSlot slot, int slotIndex = 0)
    {
        var key = (slot, slotIndex);
        if (!equipped.ContainsKey(key)) return null;
        ItemData item = equipped[key];
        equipped.Remove(key);
        RecalculateStats();

        if (item.armourData != null && PlayerArmourVisuals.Instance != null)
            PlayerArmourVisuals.Instance.EquipArmour(slot, null);

        if (item.accessoryData != null && item.accessoryData.providesLight && PlayerLantern.Instance != null)
            PlayerLantern.Instance.SetLanternEquipped(false);

        return item;
    }

    public ItemData GetEquipped(EquipSlot slot, int slotIndex = 0)
    {
        equipped.TryGetValue((slot, slotIndex), out ItemData item);
        return item;
    }

    public bool IsSlotOccupied(EquipSlot slot, int slotIndex = 0)
        => equipped.ContainsKey((slot, slotIndex));

    private EquipSlot GetSlotForItem(ItemData item)
    {
        if (item.accessoryData != null && item.accessoryData.slot != EquipSlot.None)
            return item.accessoryData.slot;
        if (item.armourData != null && item.armourData.slot != EquipSlot.None)
            return item.armourData.slot;
        return EquipSlot.None;
    }

    private void RecalculateStats()
    {
        TotalDefense = 0;
        TotalBonusDamage = 0;
        TotalAttackSpeedBonus = 0f;
        TotalBurnChanceBonus = 0f;
        TotalPoisonChanceBonus = 0f;
        TotalSlowChanceBonus = 0f;
        TotalBleedChanceBonus = 0f;
        TotalKnockbackBonus = 0f;
        TotalFireResistance = 0f;
        TotalPoisonResistance = 0f;
        TotalSlowResistance = 0f;
        TotalBleedResistance = 0f;

        foreach (var item in equipped.Values)
        {
            if (item.armourData != null)
            {
                TotalDefense += item.armourData.defense;
                TotalFireResistance += item.armourData.fireResistance;
                TotalPoisonResistance += item.armourData.poisonResistance;
                TotalSlowResistance += item.armourData.slowResistance;
                TotalBleedResistance += item.armourData.bleedResistance;
            }
            if (item.accessoryData != null)
            {
                TotalBonusDamage += item.accessoryData.bonusDamage;
                TotalAttackSpeedBonus += item.accessoryData.attackSpeedBonus;
                TotalBurnChanceBonus += item.accessoryData.burnChanceBonus;
                TotalPoisonChanceBonus += item.accessoryData.poisonChanceBonus;
                TotalSlowChanceBonus += item.accessoryData.slowChanceBonus;
                TotalBleedChanceBonus += item.accessoryData.bleedChanceBonus;
                TotalKnockbackBonus += item.accessoryData.knockbackBonus;
            }
        }
    }

    public List<(ItemData itemData, EquipSlot slot, int slotIndex)> GetAllEquipped()
    {
        var result = new List<(ItemData, EquipSlot, int)>();
        foreach (var kvp in equipped)
            result.Add((kvp.Value, kvp.Key.Item1, kvp.Key.Item2));
        return result;
    }
}