using UnityEngine;
using UnityEngine.Serialization;

public class Item : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName;
    public Sprite icon;
    public int value;
    [TextArea] public string description;

    [Header("Resource Path")]
    public string resourcePath;

    [Header("Key Settings")]
    public bool isKey = false;
    public string keyID;
#if UNITY_EDITOR
    [ContextMenu("Auto Set Resource Path")]
    private void AutoSetResourcePath()
    {
        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(gameObject);
        int resourcesIndex = assetPath.IndexOf("/Resources/");
        if (resourcesIndex >= 0)
        {
            string relativePath = assetPath.Substring(resourcesIndex + "/Resources/".Length);
            relativePath = System.IO.Path.ChangeExtension(relativePath, null);
            resourcePath = relativePath;
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"Resource path set to: {resourcePath}");
        }
        else
        {
            Debug.LogWarning("Prefab is not inside a Resources folder! Move it to Assets/Resources/...");
        }
    }
#endif

    [Header("Prefabs")]
    public GameObject itemPrefab;
    public GameObject inventoryPrefab;

    [Header("Grid Shape")]
    public Vector2Int gridSize = new Vector2Int(1, 1);

    [Header("Weapon (leave null if not a weapon)")]
    public bool isWeapon = false;
    public WeaponData weaponData;

    [Header("Armour (leave null if not armour)")]
    public bool isArmour = false;
    [FormerlySerializedAs("armorData")] public ArmourData armourData;

    [Header("Accessory (leave null if not accessory)")]
    public bool isAccessory = false;
    public AccessoryData accessoryData;
}