using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to an altar world object. When the player interacts, opens the Fusion UI.
/// Pair with your existing Altar.cs patterns — this is a separate component focused
/// purely on the fusion interaction.
/// </summary>
public class FusionAltar : MonoBehaviour
{
    [Header("Recipes this altar knows")]
    [Tooltip("Populate with FusionRecipe assets in the Inspector.")]
    public List<FusionRecipe> availableRecipes = new List<FusionRecipe>();

    [Header("References")]
    public FusionUI fusionUI;   // Drag the FusionUI panel here
    public Transform vfxSpawnPoint; // Where fusion VFX appears (defaults to altar centre)

    [Header("Interaction")]
    public float interactRange = 1.5f;
    private bool _playerInRange = false;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[FusionAltar] E pressed in range — opening UI");
            OpenFusionUI();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[FusionAltar] OnTriggerEnter2D fired by: {other.gameObject.name}, tag: {other.tag}");
        if (other.CompareTag("Player"))
        {
            Debug.Log("[FusionAltar] Player entered range — press E to open");
            _playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[FusionAltar] OnTriggerExit2D fired by: {other.gameObject.name}, tag: {other.tag}");
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            fusionUI?.Close();
        }
    }

    // ── Public ───────────────────────────────────────────────────────────────

    public void OpenFusionUI()
    {
        fusionUI?.Open(availableRecipes, this);
    }

    /// <summary>
    /// Called by FusionUI once the player confirms a fusion.
    /// Handles VFX/SFX and delivers the result item to the player.
    /// </summary>
    public void ExecuteFusion(FusionRecipe recipe, InventoryManager inventory)
    {
        if (!recipe.CanFuse(inventory))
        {
            Debug.LogWarning($"[FusionAltar] Cannot fuse {recipe.fusionName} — missing fragments.");
            return;
        }

        // Consume fragments, receive result
        Item result = recipe.ExecuteFusion(inventory);

        // VFX
        Vector3 spawnPos = vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position;
        if (recipe.fusionVFX != null)
            Instantiate(recipe.fusionVFX, spawnPos, Quaternion.identity);

        // SFX
        if (recipe.fusionSound != null)
            AudioSource.PlayClipAtPoint(recipe.fusionSound, spawnPos);

        // Give item to player
        inventory.AddItem(result);

        Debug.Log($"[FusionAltar] Fused → {result.itemName}");
        fusionUI?.ShowResult(result);
    }

    // ── Gizmos ───────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.2f, 0.8f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}