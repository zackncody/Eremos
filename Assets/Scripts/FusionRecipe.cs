using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a fusion: which fragments are consumed and what rare item is born.
/// Create via: Right-click > Create > RPG/Fusion Recipe
/// </summary>
[CreateAssetMenu(fileName = "NewFusionRecipe", menuName = "RPG/Fusion Recipe")]
public class FusionRecipe : ScriptableObject
{
    [Header("Identity")]
    public string fusionName;
    [TextArea] public string loreText;

    [Header("Fragments Required")]
    [Tooltip("All fragments that will be consumed. Order doesn't matter.")]
    public List<FusionFragment> requiredFragments = new List<FusionFragment>();

    [Header("Result")]
    public Item resultItem;
    [Tooltip("Optional VFX prefab spawned at the altar when fusion completes.")]
    public GameObject fusionVFX;
    [Tooltip("Optional sound played when fusion completes.")]
    public AudioClip fusionSound;

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Counts how many of a given item the player currently has
    /// by scanning InventoryDragItem components on the grid.
    /// </summary>
    public static int CountItem(InventoryManager inventory, Item item)
    {
        int count = 0;
        var dragItems = inventory.grid.GetComponentsInChildren<InventoryDragItem>();
        foreach (var drag in dragItems)
        {
            if (drag.itemData != null && drag.itemData.itemName == item.itemName)
                count++;
        }
        return count;
    }

    /// <summary>Returns true if the inventory holds all required fragments.</summary>
    public bool CanFuse(InventoryManager inventory)
    {
        if (inventory == null) return false;
        foreach (var fragment in requiredFragments)
        {
            if (CountItem(inventory, fragment.item) < fragment.quantity)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Consumes all fragments from the inventory grid and returns the result item.
    /// Call only after CanFuse() returns true.
    /// </summary>
    public Item ExecuteFusion(InventoryManager inventory)
    {
        foreach (var fragment in requiredFragments)
        {
            int toRemove = fragment.quantity;
            var dragItems = inventory.grid.GetComponentsInChildren<InventoryDragItem>();

            foreach (var drag in dragItems)
            {
                if (toRemove <= 0) break;
                if (drag.itemData != null && drag.itemData.itemName == fragment.item.itemName)
                {
                    inventory.grid.RemoveByID(drag.GetInstanceID());
                    Object.Destroy(drag.gameObject);
                    toRemove--;
                }
            }
        }

        return resultItem;
    }
}

[System.Serializable]
public class FusionFragment
{
    public Item item;
    public int quantity = 1;
}