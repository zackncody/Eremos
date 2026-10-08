using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<(ItemData item, Vector2Int cell)> savedInventoryItems = new List<(ItemData, Vector2Int)>();
    public ItemData[] savedHotbarItems;
    public int savedHotbarIndex = 0;
    public int savedHealth = 0; // 0 means not yet set, PlayerHealth will use maxHealth

    private void Awake()
    {
        GameServices.RegisterGame(this);
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(InitializeNewScene());
    }

    private System.Collections.IEnumerator InitializeNewScene()
    {
        yield return null;
        HotbarManager hotbar = FindAnyObjectByType<HotbarManager>();
        InventoryManager inventory = FindAnyObjectByType<InventoryManager>();
        Weapon weapon = FindAnyObjectByType<Weapon>();

        if (hotbar != null && savedHotbarItems != null)
        {
            for (int i = 0; i < savedHotbarItems.Length && i < hotbar.GetAllItems().Length; i++)
            {
                if (savedHotbarItems[i] != null)
                    hotbar.AssignItem(i, savedHotbarItems[i]);
            }
            hotbar.SelectSlotPublic(savedHotbarIndex);
        }

        if (inventory != null)
        {
            inventory.SetInventoryOpen(false);
            foreach (var pending in savedInventoryItems)
                inventory.RestoreItem(pending.item, pending.cell);
        }

        if (weapon != null && hotbar != null)
        {
            ItemData selected = hotbar.GetSelectedItem();
            if (selected?.weaponData != null)
            {
                weapon.gameObject.SetActive(true);
                weapon.ApplyWeaponData(selected.weaponData);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
        }
    }

    public void SaveState(HotbarManager hotbar, InventoryManager inventory)
    {
        savedHotbarItems = hotbar.GetAllItems().Clone() as ItemData[];
        savedHotbarIndex = hotbar.selectedIndex;
        savedInventoryItems = inventory.GetSavedItems();
    }
}