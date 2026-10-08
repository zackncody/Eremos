using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on the inventory panel (same object as ClassSelectorUI is fine), NOT on the AbilityPanel itself.
// All UI elements are built by hand in the editor.
public class AbilityPanelUI : MonoBehaviour
{
    [System.Serializable]
    public class Opener
    {
        public ClassData classData;
        public Button button;
    }

    [System.Serializable]
    public class AbilitySlot
    {
        public Button button;
        public Image icon;
        public TMP_Text nameLabel;
        public GameObject lockedOverlay;
        public GameObject selectedMarker;
    }

    [Header("Open buttons (one per class)")]
    [SerializeField] private List<Opener> openers = new List<Opener>();

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private List<AbilitySlot> slots = new List<AbilitySlot>();

    [Header("Details")]
    [SerializeField] private TMP_Text detailName;
    [SerializeField] private TMP_Text detailDescription;
    [SerializeField] private TMP_Text detailStats;
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text selectButtonLabel;

    [Header("Class XP (optional)")]
    [SerializeField] private TMP_Text levelLabel;
    [SerializeField] private Image xpFill;   // Image Type = Filled, Fill Method = Horizontal

    private ClassManager manager;
    private ClassData viewedClass;
    private ClassAbility viewedAbility;
    private bool wired;

    private void OnEnable()
    {
        manager = GameServices.Classes;
        if (manager == null)
        {
            Debug.LogWarning("AbilityPanelUI: no ClassManager found.");
            return;
        }

        if (!wired) Wire();

        manager.OnLevelUp += HandleLevelUp;
        manager.OnClassChanged += HandleClassChanged;
        manager.OnAbilityChanged += HandleAbilityChanged;
        manager.OnXPChanged += HandleXPChanged;
        Close();
    }

    private void OnDisable()
    {
        if (manager == null) return;

        manager.OnLevelUp -= HandleLevelUp;
        manager.OnClassChanged -= HandleClassChanged;
        manager.OnAbilityChanged -= HandleAbilityChanged;
        manager.OnXPChanged -= HandleXPChanged;
    }
    private void HandleXPChanged(ClassData c, float gained)
    {
        if (viewedClass == c) RefreshXP();
    }

    private void RefreshXP()
    {
        if (manager == null || viewedClass == null) return;

        int level = manager.GetLevel(viewedClass);
        bool max = manager.IsMaxLevel(viewedClass);
        float xp = manager.GetXP(viewedClass);
        float needed = manager.GetXPToNext(viewedClass);

        if (levelLabel != null)
            levelLabel.text = max ? $"Level {level} (MAX)"
                                  : $"Level {level}   {Mathf.FloorToInt(xp)} / {Mathf.CeilToInt(needed)} XP";

        if (xpFill != null)
            xpFill.fillAmount = max ? 1f : (needed > 0f ? Mathf.Clamp01(xp / needed) : 0f);
    }

    private void HandleLevelUp(ClassData c, int level) => Refresh();
    private void HandleClassChanged(ClassData c) => Refresh();
    private void HandleAbilityChanged(ClassData c, ClassAbility a) => Refresh();

    private void Wire()
    {
        wired = true;

        foreach (Opener o in openers)
        {
            if (o.button == null || o.classData == null) continue;
            ClassData captured = o.classData;
            o.button.onClick.AddListener(() => Open(captured));
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].button == null) continue;
            int index = i;
            slots[i].button.onClick.AddListener(() => ViewSlot(index));
        }

        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (selectButton != null) selectButton.onClick.AddListener(EquipViewed);
    }

    public void Open(ClassData c)
    {
        if (manager == null || c == null) return;

        viewedClass = c;
        viewedAbility = manager.GetSelectedAbility(c);
        if (panelRoot != null) panelRoot.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        viewedClass = null;
        viewedAbility = null;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void ViewSlot(int index)
    {
        if (viewedClass == null || index >= viewedClass.abilities.Count) return;

        viewedAbility = viewedClass.abilities[index];
        Refresh();
    }

    private void EquipViewed()
    {
        if (manager == null || viewedClass == null || viewedAbility == null) return;

        if (manager.SelectAbility(viewedClass, viewedAbility))
            Refresh();
    }

    private void Refresh()
    {
        if (manager == null || viewedClass == null) return;

        int level = manager.GetLevel(viewedClass);
        ClassAbility selected = manager.GetSelectedAbility(viewedClass);

        if (titleLabel != null)
            titleLabel.text = $"{viewedClass.displayName} Abilities";

        for (int i = 0; i < slots.Count; i++)
        {
            AbilitySlot s = slots[i];
            if (s.button == null) continue;

            ClassAbility a = i < viewedClass.abilities.Count ? viewedClass.abilities[i] : null;
            s.button.gameObject.SetActive(a != null);
            if (a == null) continue;

            if (s.icon != null) s.icon.sprite = a.icon;
            if (s.nameLabel != null) s.nameLabel.text = a.displayName;
            if (s.lockedOverlay != null) s.lockedOverlay.SetActive(level < a.unlockLevel);
            if (s.selectedMarker != null) s.selectedMarker.SetActive(a == selected);
        }

        RefreshDetails(level, selected);
        RefreshXP();
    }

    private void RefreshDetails(int level, ClassAbility selected)
    {
        bool has = viewedAbility != null;

        if (detailName != null) detailName.text = has ? viewedAbility.displayName : "";
        if (detailDescription != null) detailDescription.text = has ? viewedAbility.description : "";
        if (detailStats != null)
            detailStats.text = has
                ? $"Cost {viewedAbility.cost}   Cooldown {viewedAbility.cooldown}s   Unlocks at level {viewedAbility.unlockLevel}"
                : "";

        if (selectButton == null) return;

        selectButton.gameObject.SetActive(has);
        if (!has) return;

        bool locked = level < viewedAbility.unlockLevel;
        bool equipped = viewedAbility == selected;

        selectButton.interactable = !locked && !equipped;

        if (selectButtonLabel != null)
            selectButtonLabel.text = locked ? $"Unlocks at level {viewedAbility.unlockLevel}"
                                  : equipped ? "Selected"
                                  : "Select";
    }
}