using UnityEngine;

public class ObeliskInteractable : MonoBehaviour
{
    [Header("Tip Content")]
    [TextArea(3, 6)]
    public string tipText = "The old stones remember what the living forget.";

    [Header("Prompt")]
    public GameObject promptIcon; // world-space "Press E" icon, optional

    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ObeliskUI.Instance.ShowTip(tipText);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptIcon != null) promptIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptIcon != null) promptIcon.SetActive(false);
            ObeliskUI.Instance.HideTip(); // close it if they walk away mid-read
        }
    }
}