using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class Altar : MonoBehaviour
{
    [Header("Guaranteed Item")]
    public GameObject guaranteedItemPrefab;

    [System.Serializable]
    public class BonusDrop
    {
        public GameObject itemPrefab;
        [Range(0f, 100f)] public float dropChance = 50f;
    }

    [Header("Bonus Drops")]
    public List<BonusDrop> bonusDrops = new List<BonusDrop>();

    [Header("Pop Animation")]
    public float spawnRadiusMin = 0.5f;
    public float spawnRadiusMax = 1.5f;
    public float popHeight = 1.5f;
    public float popDuration = 0.4f;
    public float fallDuration = 0.3f;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRadius = 1.5f;
    public bool hasBeenUsed = false;

    [Header("Altar Settings")]
    public string animalName = "Wolf";

    private Transform playerTransform;
    private bool playerInRange = false;
    private GameObject promptObject;
    private TMP_Text promptText;

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        BuildPromptUI();
        promptObject.SetActive(false);
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
        if (guaranteedItemPrefab == null)
        {
            Debug.LogWarning("Altar has no guaranteed item assigned!");
            return;
        }

        SpawnItem(guaranteedItemPrefab);

        foreach (BonusDrop drop in bonusDrops)
        {
            if (drop.itemPrefab == null) continue;
            if (Random.Range(0f, 100f) <= drop.dropChance)
                SpawnItem(drop.itemPrefab);
        }

        hasBeenUsed = true;
        if (promptObject != null) promptObject.SetActive(false);

        Debug.Log($"Will of the {animalName} has been retrieved.");
    }

    private void SpawnItem(GameObject prefab)
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(spawnRadiusMin, spawnRadiusMax);
        Vector3 landPos = transform.position + new Vector3(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius,
            0f
        );

        GameObject item = Instantiate(prefab, transform.position, Quaternion.identity);

        foreach (var col in item.GetComponentsInChildren<Collider2D>())
            col.enabled = false;
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        ItemPopAnimation anim = item.AddComponent<ItemPopAnimation>();
        anim.Play(transform.position, landPos, popHeight, popDuration, fallDuration);
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
        promptText.text = $"Press E — Will of the {animalName}";
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