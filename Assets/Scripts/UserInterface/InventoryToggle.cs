using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private EquipmentPanel equipmentPanel;

    private bool isOpen = false;

    private void Start()
    {
        SetInventoryState(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetInventoryState(!isOpen);
    }

    private void SetInventoryState(bool open)
    {
        isOpen = open;
        inventoryPanel.SetActive(open);
    
        Time.timeScale = open ? 0f : 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (!open && TooltipUI.Instance != null)
            TooltipUI.Instance.Hide();

        if (inventoryManager != null)
            inventoryManager.SetInventoryOpen(open);

        if (equipmentPanel != null)
            equipmentPanel.gameObject.SetActive(open);
    }

}