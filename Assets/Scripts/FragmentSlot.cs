using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to your FragmentSlotPrefab.
/// Shows the fragment icon, how many are required, and how many the player owns.
/// Turns red when the player doesn't have enough.
/// </summary>
public class FragmentSlot : MonoBehaviour
{
    public Image fragmentIcon;
    public TextMeshProUGUI countText;
    public Image background;

    [Header("Colours")]
    public Color sufficientColor = Color.white;
    public Color insufficientColor = new Color(1f, 0.3f, 0.3f);

    public void Setup(Item item, int required, int owned)
    {
        if (fragmentIcon != null && item.icon != null)
            fragmentIcon.sprite = item.icon;

        if (countText != null)
            countText.text = $"{owned} / {required}";

        bool sufficient = owned >= required;
        if (background != null)
            background.color = sufficient ? sufficientColor : insufficientColor;
    }
}