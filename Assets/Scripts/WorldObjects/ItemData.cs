using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public string description;
    public Sprite icon;
    public Vector2Int gridSize;
    public GameObject inventoryPrefab;
    public GameObject itemPrefab;
    public string resourcePath;
    public WeaponData weaponData;
    public ArmourData armourData;
    public AccessoryData accessoryData;
    public bool isKey;
    public string keyID;

    [Header("Leveling")]
    public int level = 1;
    public float currentXP = 0f;

    public float XPToNextLevel => 10f * Mathf.Pow(1.15f, level - 1);

    public float DamageMultiplier => 1f + 0.05f * (level - 1);

    // Call this on hit and on kill. Returns true if a level-up happened.
    public bool AddXP(float amount)
    {
        currentXP += amount;
        bool leveledUp = false;
        while (currentXP >= XPToNextLevel)
        {
            currentXP -= XPToNextLevel;
            level++;
            leveledUp = true;
        }
        return leveledUp;
    }

    public static ItemData FromItem(Item item)
    {
        return new ItemData
        {
            itemName = item.itemName,
            description = item.description,
            icon = item.icon,
            gridSize = item.gridSize,
            inventoryPrefab = item.inventoryPrefab,
            itemPrefab = item.itemPrefab,
            resourcePath = item.resourcePath,
            weaponData = item.isWeapon ? item.weaponData : null,
            armourData = item.isArmour ? item.armourData : null,
            accessoryData = item.isAccessory ? item.accessoryData : null,
            isKey = item.isKey,
            keyID = item.keyID
        };
    }

    public void ReloadReferences()
    {
        bool needsCoreReload = icon == null || inventoryPrefab == null;
        bool needsWeaponReload = weaponData != null && (
            (weaponData.arrowPrefab == null) ||
            (weaponData.magicProjectilePrefab == null) ||
            (weaponData.magicAOEPrefab == null) ||
            (weaponData.summonPrefab == null));
        bool needsArmourReload = armourData != null && armourData.equippedSprite == null;

        if (!needsCoreReload && !needsWeaponReload && !needsArmourReload) return;
        if (string.IsNullOrEmpty(resourcePath)) return;

        GameObject loaded = Resources.Load<GameObject>(resourcePath);
        if (loaded == null) return;

        Item item = loaded.GetComponent<Item>();
        if (item == null) return;

        // ── Core fields (existing behaviour) ──────────────────
        if (icon == null) icon = item.icon;
        if (inventoryPrefab == null) inventoryPrefab = item.inventoryPrefab;
        if (itemPrefab == null) itemPrefab = item.itemPrefab;
        if (string.IsNullOrEmpty(itemName)) itemName = item.itemName;
        if (string.IsNullOrEmpty(description)) description = item.description;
        if (string.IsNullOrEmpty(keyID)) keyID = item.keyID;

        // ── WeaponData object-reference fields ────────────────
        // Numeric fields (damage, speeds, chances) already survive
        // JsonUtility fine — only prefab refs need patching here.
        if (weaponData != null && item.weaponData != null)
        {
            if (weaponData.arrowPrefab == null) weaponData.arrowPrefab = item.weaponData.arrowPrefab;
            if (weaponData.magicProjectilePrefab == null) weaponData.magicProjectilePrefab = item.weaponData.magicProjectilePrefab;
            if (weaponData.magicAOEPrefab == null) weaponData.magicAOEPrefab = item.weaponData.magicAOEPrefab;
            if (weaponData.summonPrefab == null) weaponData.summonPrefab = item.weaponData.summonPrefab;
        }

        // ── ArmourData object-reference fields ────────────────
        if (armourData != null && item.armourData != null)
        {
            if (armourData.equippedSprite == null) armourData.equippedSprite = item.armourData.equippedSprite;
        }

        // AccessoryData has no object references — nothing to patch.
    }
}