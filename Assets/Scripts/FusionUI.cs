using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Fusion UI panel.
///
/// Hierarchy suggestion:
///   FusionPanel (this script)
///   ├── RecipeList          ← parent for RecipeEntryPrefab instances
///   ├── FragmentSlotParent  ← shows required fragments for selected recipe
///   ├── ResultPreview
///   │   ├── ResultIcon (Image)
///   │   └── ResultName (TMP)
///   ├── LoreText (TMP)
///   ├── FuseButton (Button)
///   └── ResultRevealPanel
///       ├── RevealIcon (Image)
///       └── RevealName (TMP)
/// </summary>
public class FusionUI : MonoBehaviour
{
    [Header("Recipe List")]
    public Transform recipeListParent;
    public GameObject recipeEntryPrefab;

    [Header("Selected Recipe Info")]
    public Transform fragmentSlotParent;
    public GameObject fragmentSlotPrefab;
    public TextMeshProUGUI loreText;

    [Header("Result Preview")]
    public Image resultIcon;
    public TextMeshProUGUI resultNameText;

    [Header("Fuse Button")]
    public Button fuseButton;
    public TextMeshProUGUI fuseButtonText;

    [Header("Close Button")]
    public Button closeButton;

    [Header("Result Reveal")]
    public GameObject resultRevealPanel;
    public Image revealIcon;
    public TextMeshProUGUI revealNameText;
    public float revealDuration = 2.5f;

    [Header("Inventory Reference")]
    [Tooltip("Drag your InventoryManager GameObject here, or leave blank to find it automatically.")]
    public InventoryManager inventoryManager;

    // ── Private state ────────────────────────────────────────────────────────

    private List<FusionRecipe> _recipes;
    private FusionRecipe _selectedRecipe;
    private FusionAltar _altar;

    // ── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        gameObject.SetActive(false);
        resultRevealPanel?.SetActive(false);
        fuseButton.onClick.AddListener(OnFuseClicked);
        closeButton?.onClick.AddListener(Close);

        // Auto-find if not assigned in Inspector
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();
    }

    private void OnDestroy()
    {
        Debug.LogWarning($"[FusionUI] OnDestroy called on '{gameObject.name}'. Stack trace below shows what triggered it.");
        Debug.LogWarning(System.Environment.StackTrace);
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public void Open(List<FusionRecipe> recipes, FusionAltar altar)
    {
        _recipes = recipes;
        _altar = altar;

        gameObject.SetActive(true);
        BuildRecipeList();

        if (_recipes.Count > 0)
            SelectRecipe(_recipes[0]);
    }

    public void Close()
    {
        if (this == null) return;
        gameObject.SetActive(false);
    }

    public void ShowResult(Item item)
    {
        if (resultRevealPanel == null) return;

        if (revealIcon != null) revealIcon.sprite = item.icon;
        if (revealNameText != null) revealNameText.text = item.itemName;
        resultRevealPanel.SetActive(true);
        StartCoroutine(HideRevealAfterDelay());

        BuildRecipeList();
        if (_selectedRecipe != null)
            SelectRecipe(_selectedRecipe);
    }

    // ── Private ──────────────────────────────────────────────────────────────

    private void BuildRecipeList()
    {
        foreach (Transform child in recipeListParent)
            Destroy(child.gameObject);

        foreach (var recipe in _recipes)
        {
            var entry = Instantiate(recipeEntryPrefab, recipeListParent);
            var entryComp = entry.GetComponent<RecipeEntry>();
            if (entryComp != null)
            {
                bool canFuse = recipe.CanFuse(inventoryManager);
                entryComp.Setup(recipe, canFuse, () => SelectRecipe(recipe));
            }
            else
            {
                var btn = entry.GetComponent<Button>();
                var capturedRecipe = recipe;
                btn?.onClick.AddListener(() => SelectRecipe(capturedRecipe));

                var label = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = recipe.fusionName;
            }
        }
    }

    private void SelectRecipe(FusionRecipe recipe)
    {
        _selectedRecipe = recipe;

        if (loreText != null) loreText.text = recipe.loreText;
        if (resultIcon != null) resultIcon.sprite = recipe.resultItem.icon;
        if (resultNameText != null) resultNameText.text = recipe.resultItem.itemName;

        // Fragment slots
        foreach (Transform child in fragmentSlotParent)
            Destroy(child.gameObject);

        foreach (var fragment in recipe.requiredFragments)
        {
            var slot = Instantiate(fragmentSlotPrefab, fragmentSlotParent);
            var slotComp = slot.GetComponent<FragmentSlot>();
            if (slotComp != null)
            {
                int owned = inventoryManager != null
                    ? FusionRecipe.CountItem(inventoryManager, fragment.item)
                    : 0;
                slotComp.Setup(fragment.item, fragment.quantity, owned);
            }
        }

        bool canFuse = recipe.CanFuse(inventoryManager);
        fuseButton.interactable = canFuse;
        if (fuseButtonText != null)
            fuseButtonText.text = canFuse ? "Fuse" : "Missing Fragments";
    }

    private void OnFuseClicked()
    {
        if (_selectedRecipe == null || _altar == null) return;
        _altar.ExecuteFusion(_selectedRecipe, inventoryManager);
    }

    private IEnumerator HideRevealAfterDelay()
    {
        yield return new WaitForSeconds(revealDuration);
        resultRevealPanel?.SetActive(false);
    }
}