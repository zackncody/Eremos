using System.Collections.Generic;
using UnityEngine;

public class PlayerArmourVisuals : MonoBehaviour
{
    public static PlayerArmourVisuals Instance { get; private set; }

    [System.Serializable]
    public class ArmourLayer
    {
        public EquipSlot slot;
        public SpriteRenderer spriteRenderer;
    }

    public List<ArmourLayer> layers; // only needs entries for Head, Chest, Legs, Boots

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        foreach (var layer in layers)
            layer.spriteRenderer.enabled = false;
    }

    public void EquipArmour(EquipSlot slot, ArmourData armour)
    {
        var layer = layers.Find(l => l.slot == slot);
        if (layer == null) return; // Ring/Necklace etc. have no visual layer, that's fine

        if (armour == null || armour.equippedSprite == null)
        {
            layer.spriteRenderer.enabled = false;
            return;
        }

        layer.spriteRenderer.enabled = true;
        layer.spriteRenderer.sprite = armour.equippedSprite;
    }
}