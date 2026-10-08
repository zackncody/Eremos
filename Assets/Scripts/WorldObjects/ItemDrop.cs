using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemDrop : MonoBehaviour
{
    [Header("Item to Spawn")]
    public GameObject itemPrefab;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRadius = 1.5f;
    public bool hasBeenUsed = false;

    [Header("Persistence")]
    [Tooltip("Unique ID for this chest instance, used to remember if it's already been opened. Must be unique per chest — use the context menu to auto-generate one.")]
    public string chestID;

    [Header("Lock Settings")]
    public bool requiresKey = false;
    public string requiredKeyID;
    public bool consumeKeyOnUse = true;
    public string lockedMessage = "Locked - requires a key";
    public string unlockPromptMessage = "Press E to unlock";

    [Header("Spawn Ring")]
    public float spawnRadiusMin = 0.5f;
    public float spawnRadiusMax = 1.5f;

    [Header("Pop Animation")]
    public float popHeight = 1.5f;
    public float popDuration = 0.4f;
    public float fallDuration = 0.3f;

    [Header("Prompt Text")]
    [TextArea] public string promptMessage = "Press E to pick up";

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTriggerName = "Open";

    private Transform playerTransform;
    private bool playerInRange = false;
    private GameObject promptObject;
    private TMP_Text promptText;
    private InventoryManager inventoryManager;

#if UNITY_EDITOR
    [ContextMenu("Generate Chest ID")]
    private void GenerateChestID()
    {
        chestID = System.Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("Generated chestID: " + chestID);
    }
#endif

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        inventoryManager = FindAnyObjectByType<InventoryManager>();

        if (ChestUnlockTracker.Instance != null && ChestUnlockTracker.Instance.IsUsed(chestID))
            hasBeenUsed = true;

        BuildPromptUI();
        promptObject.SetActive(false);
        UpdatePromptMessage();
    }

    private void UpdatePromptMessage()
    {
        if (promptText == null) return;
        promptText.text = requiresKey ? unlockPromptMessage : promptMessage;
    }

    private void BuildPromptUI()
    {
        GameObject canvasObj = new GameObject("PromptCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRT = canvasObj.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(260f, 50f);

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObj.transform, false);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);

        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        GameObject border = new GameObject("Border");
        border.transform.SetParent(canvasObj.transform, false);
        border.transform.SetAsFirstSibling();

        Image borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(0.8f, 0.7f, 0.4f, 0.9f);

        RectTransform borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = new Vector2(-3f, -3f);
        borderRT.offsetMax = new Vector2(3f, 3f);

        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(panel.transform, false);

        promptText = textObj.AddComponent<TextMeshProUGUI>();
        promptText.text = promptMessage;
        promptText.fontSize = 18f;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = new Color(1f, 0.95f, 0.8f, 1f);

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(6f, 4f);
        textRT.offsetMax = new Vector2(-6f, -4f);

        promptObject = canvasObj;
    }

    private void Update()
    {
        if (hasBeenUsed) return;
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = dist <= interactRadius;

        if (promptObject != null)
            promptObject.SetActive(playerInRange);

        if (playerInRange && Input.GetKeyDown(interactKey))
            Interact();
    }

    private void Interact()
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning("ItemDrop has no item prefab assigned!");
            return;
        }

        if (requiresKey)
        {
            if (!TryConsumeKey())
            {
                Debug.Log(lockedMessage);
                return;
            }
        }

        hasBeenUsed = true;
        ChestUnlockTracker.Instance?.MarkUsed(chestID);

        if (promptObject != null) promptObject.SetActive(false);

        if (animator != null)
            animator.SetTrigger(openTriggerName);
        else
            SpawnItem(); // no animation set up — spawn immediately as before

        Debug.Log($"ItemDrop used: {promptMessage}");
    }

    /// <summary>
    /// Call this from an Animation Event on the open animation clip,
    /// at the exact frame the chest should actually release the item.
    /// </summary>
    public void SpawnItem()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(spawnRadiusMin, spawnRadiusMax);
        Vector3 landPos = transform.position + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

        GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);

        foreach (var col in item.GetComponentsInChildren<Collider2D>())
            col.enabled = false;
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        ItemPopAnimation anim = item.AddComponent<ItemPopAnimation>();
        anim.Play(transform.position, landPos, popHeight, popDuration, fallDuration);
    }

    private bool TryConsumeKey()
    {
        if (string.IsNullOrEmpty(requiredKeyID))
        {
            Debug.LogWarning("requiresKey is true but no requiredKeyID set!");
            return false;
        }
        if (inventoryManager == null)
        {
            Debug.LogWarning("No InventoryManager found in scene.");
            return false;
        }

        var allItems = inventoryManager.GetAllItems();
        foreach (var entry in allItems)
        {
            if (entry.itemData != null && entry.itemData.isKey && entry.itemData.keyID == requiredKeyID)
            {
                if (consumeKeyOnUse)
                    inventoryManager.RemoveItemData(entry.itemData, entry.cell);
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadiusMin);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadiusMax);
    }
}