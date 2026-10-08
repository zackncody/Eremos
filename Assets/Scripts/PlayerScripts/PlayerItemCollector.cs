using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerItemCollector : MonoBehaviour
{
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private Transform handTransform;
    [SerializeField] private HotbarManager hotbar;
    [SerializeField] private Weapon playerWeapon;

    private GameObject heldItem;

    private void Awake()
    {
        Debug.Log("PlayerItemCollector.Awake on '" + gameObject.name + "' (instance ID " + GetInstanceID() + ")");
    }

    private void Start()
    {
        Debug.Log("PlayerItemCollector.Start on '" + gameObject.name + "' (instance ID " + GetInstanceID() + ")");
        RelinkManagers();

        if (playerWeapon != null)
            playerWeapon.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (hotbar != null)
            hotbar.OnSelectionChanged -= OnHotbarSelectionChanged;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RelinkManagersNextFrame());
    }

    private IEnumerator RelinkManagersNextFrame()
    {
        const int maxAttempts = 30; // ~0.5 seconds at 60fps, adjust if needed
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            yield return null;
            attempts++;

            InventoryManager foundInventory = FindAnyObjectByType<InventoryManager>(FindObjectsInactive.Include);
            HotbarManager foundHotbar = FindAnyObjectByType<HotbarManager>(FindObjectsInactive.Include);

            if (foundInventory != null && foundHotbar != null)
                break;
        }

        RelinkManagers();
    }

    private void RelinkManagers()
    {
         
        if (hotbar != null)
            hotbar.OnSelectionChanged -= OnHotbarSelectionChanged;

        inventory = FindAnyObjectByType<InventoryManager>(FindObjectsInactive.Include);
        hotbar = FindAnyObjectByType<HotbarManager>(FindObjectsInactive.Include);

        if (inventory == null)
            Debug.LogWarning("PlayerItemCollector: no InventoryManager found in this scene.");
        else
            Debug.Log("PlayerItemCollector: relinked to InventoryManager '" + inventory.name + "' (instance ID " + inventory.GetInstanceID() + ")");

        if (hotbar != null)
            hotbar.OnSelectionChanged += OnHotbarSelectionChanged;
        else
            Debug.LogWarning("PlayerItemCollector: no HotbarManager found in this scene.");
    }

    private void OnHotbarSelectionChanged(ItemData item)
    {
        if (item == null)
        {
            HideHeldItem();
            SetWeaponActive(false);
            return;
        }

        if (item.weaponData != null)
        {
            SetWeaponActive(true, item);
            playerWeapon.ApplyWeaponData(item.weaponData);
            HideHeldItem();
        }
        else
        {
            SetWeaponActive(false);
            ShowHeldItem(item);
        }
    }

    private void SetWeaponActive(bool active, ItemData item = null)
    {
        if (playerWeapon == null) return;
        playerWeapon.gameObject.SetActive(active);
        if (active && item != null)
        {
            SpriteRenderer sr = playerWeapon.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = item.icon;
        }
    }

    private void ShowHeldItem(ItemData item)
    {
        if (heldItem == null)
        {
            heldItem = new GameObject("HeldItem");
            heldItem.transform.SetParent(handTransform);
            heldItem.transform.localPosition = Vector3.zero;
            heldItem.transform.localRotation = Quaternion.identity;
            heldItem.transform.localScale = Vector3.one;
            heldItem.AddComponent<SpriteRenderer>().sortingOrder = 10;
        }
        heldItem.GetComponent<SpriteRenderer>().sprite = item.icon;
        heldItem.SetActive(true);
    }

    private void HideHeldItem()
    {
        if (heldItem != null)
            heldItem.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("PlayerItemCollector.OnTriggerEnter2D fired on '" + gameObject.name + "' (instance ID " + GetInstanceID() + "), inventory is " + (inventory == null ? "NULL" : "set") + ", collided with '" + collision.name + "'");

        if (!collision.CompareTag("Item")) return;

        Item item = collision.GetComponent<Item>();
        if (item == null)
        {
            Debug.LogWarning("Tagged as Item but missing Item component: " + collision.name);
            return;
        }
        if (item.itemPrefab == null)
        {
            Debug.LogWarning("itemPrefab not assigned on: " + collision.name);
            return;
        }
        if (inventory == null)
        {
            Debug.LogError("Inventory reference not assigned on PlayerItemCollector.");
            return;
        }

        if (inventory.AddItem(item))
            Destroy(collision.gameObject);
    }
}