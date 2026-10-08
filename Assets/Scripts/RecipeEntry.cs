using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to your RecipeEntryPrefab.
/// Shows the recipe name, icon, and whether the player can currently fuse it.
/// </summary>
public class RecipeEntry : MonoBehaviour
{
    public Image recipeIcon;
    public TextMeshProUGUI recipeName;
    public GameObject canFuseIndicator;
    public GameObject missingIndicator;
    public Button selectButton;

    public void Setup(FusionRecipe recipe, bool canFuse, Action onSelect)
    {
        if (recipeIcon != null && recipe.resultItem?.icon != null)
            recipeIcon.sprite = recipe.resultItem.icon;

        if (recipeName != null)
            recipeName.text = recipe.fusionName;

        canFuseIndicator?.SetActive(canFuse);
        missingIndicator?.SetActive(!canFuse);

        selectButton?.onClick.RemoveAllListeners();
        selectButton?.onClick.AddListener(() => onSelect?.Invoke());
    }
}