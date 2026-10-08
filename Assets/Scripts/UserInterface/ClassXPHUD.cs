using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassXPHUD : MonoBehaviour
{
    [Header("XP bar (fades in on gain)")]
    [SerializeField] private CanvasGroup barGroup;
    [SerializeField] private Image barFill;          // Image Type = Filled, Fill Method = Horizontal
    [SerializeField] private TMP_Text barLabel;      // optional, e.g. "Vessel Lv 3"
    [SerializeField] private float visibleSeconds = 2.5f;
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Level-up toast")]
    [SerializeField] private CanvasGroup toastGroup;
    [SerializeField] private TMP_Text toastText;
    [SerializeField] private float toastSeconds = 3f;

    private ClassManager manager;
    private float barHideTime;
    private float toastHideTime;

    private void Start()
    {
        if (barGroup != null) barGroup.alpha = 0f;
        if (toastGroup != null) toastGroup.alpha = 0f;
    }

    private void Update()
    {
        if (manager == null)
        {
            manager = GameServices.Classes;
            if (manager != null)
            {
                manager.OnXPChanged += HandleXPChanged;
                manager.OnLevelUp += HandleLevelUp;
            }
        }

        Fade(barGroup, Time.unscaledTime < barHideTime);
        Fade(toastGroup, Time.unscaledTime < toastHideTime);
    }

    private void OnDestroy()
    {
        if (manager == null) return;
        manager.OnXPChanged -= HandleXPChanged;
        manager.OnLevelUp -= HandleLevelUp;
    }

    private void Fade(CanvasGroup g, bool visible)
    {
        if (g == null) return;
        g.alpha = Mathf.MoveTowards(g.alpha, visible ? 1f : 0f, fadeSpeed * Time.unscaledDeltaTime);
    }

    private void HandleXPChanged(ClassData c, float gained)
    {
        barHideTime = Time.unscaledTime + visibleSeconds;

        bool max = manager.IsMaxLevel(c);
        float needed = manager.GetXPToNext(c);

        if (barFill != null)
            barFill.fillAmount = max ? 1f : (needed > 0f ? Mathf.Clamp01(manager.GetXP(c) / needed) : 0f);

        if (barLabel != null)
            barLabel.text = $"{c.displayName} Lv {manager.GetLevel(c)}";
    }

    private void HandleLevelUp(ClassData c, int level)
    {
        string text = $"{c.displayName} Level {level}";

        foreach (ClassAbility a in c.abilities)
        {
            if (a != null && a.unlockLevel == level)
                text += $"\nUnlocked: {a.displayName}";
        }

        if (toastText != null) toastText.text = text;
        toastHideTime = Time.unscaledTime + toastSeconds;
    }
}