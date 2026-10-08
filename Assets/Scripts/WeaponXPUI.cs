using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class WeaponXPUI : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text xpText; // optional

    private HotbarManager hotbar;
    private CanvasGroup group;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (hotbar == null)
            hotbar = FindAnyObjectByType<HotbarManager>(FindObjectsInactive.Include);

        ItemData item = hotbar != null ? hotbar.GetSelectedItem() : null;

        // Melee only for now. Remove the Sword check once other weapon types award XP.
        bool show = item != null
            && item.weaponData != null
            && item.weaponData.weaponType == WeaponType.Sword;

        group.alpha = show ? 1f : 0f;
        if (!show) return;

        float needed = item.XPToNextLevel;
        if (fill != null) fill.fillAmount = Mathf.Clamp01(item.currentXP / needed);
        if (levelText != null) levelText.text = "Lv " + item.level;
        if (xpText != null) xpText.text = Mathf.FloorToInt(item.currentXP) + " / " + Mathf.CeilToInt(needed);
    }
}