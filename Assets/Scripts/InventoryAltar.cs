using UnityEngine;

public class AltarInteract : MonoBehaviour
{
    [Header("Expansion Settings")]
    [SerializeField] private int rowsToAdd = 2;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string promptMessage = "Press E to expand inventory";

    [Header("One Time Use")]
    [SerializeField] private bool oneTimeUse = true;
    private bool hasBeenUsed = false;

    private bool playerInRange = false;
    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = FindAnyObjectByType<InventoryManager>();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
            TryExpand();
    }

    private void TryExpand()
    {
        if (oneTimeUse && hasBeenUsed)
        {
            Debug.Log("Altar already used");
            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning("No InventoryManager found!");
            return;
        }

        inventoryManager.ExpandInventory(rowsToAdd);
        hasBeenUsed = true;

        if (oneTimeUse)
        {
            Debug.Log("Altar used — inventory expanded!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log(promptMessage);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}