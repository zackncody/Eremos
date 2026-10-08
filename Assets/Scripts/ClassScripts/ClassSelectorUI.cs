using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on the inventory panel (or any object). Buttons are built by hand in the editor.
public class ClassSelectorUI : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public ClassData classData;
        public Button button;
        public Image image;      // optional, defaults to the button's image
        public TMP_Text label;   // optional, leave empty to skip text
        public Image xpFill;     // optional, Image Type = Filled, Fill Method = Horizontal
        public TMP_Text xpLabel; // optional, shows "45 / 150 XP" or "MAX"
    }

    [SerializeField] private List<Slot> slots = new List<Slot>();
    [SerializeField] private Color normalColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private Color selectedColor = Color.white;

    private ClassManager manager;
    private bool wired;

    private void OnEnable()
    {
        manager = GameServices.Classes;
        if (manager == null)
        {
            Debug.LogWarning("ClassSelectorUI: no ClassManager found.");
            return;
        }

        if (!wired) Wire();

        manager.OnClassChanged += HandleClassChanged;
        manager.OnLevelUp += HandleLevelUp;
        manager.OnXPChanged += HandleXPChanged;
        Refresh();
    }

    private void OnDisable()
    {
        if (manager == null) return;

        manager.OnClassChanged -= HandleClassChanged;
        manager.OnLevelUp -= HandleLevelUp;
        manager.OnXPChanged -= HandleXPChanged;
    }

    private void HandleClassChanged(ClassData c) => Refresh();
    private void HandleLevelUp(ClassData c, int level) => Refresh();
    private void HandleXPChanged(ClassData c, float gained) => Refresh();

    private void Wire()
    {
        wired = true;

        foreach (Slot s in slots)
        {
            if (s.button == null || s.classData == null) continue;

            if (s.image == null) s.image = s.button.targetGraphic as Image;

            ClassData captured = s.classData;
            s.button.onClick.AddListener(() => manager.SetClass(captured));
        }
    }

    private void Refresh()
    {
        if (manager == null) return;

        foreach (Slot s in slots)
        {
            if (s.classData == null) continue;

            if (s.label != null)
                s.label.text = $"{s.classData.displayName} Lv {manager.GetLevel(s.classData)}";

            if (s.image != null)
                s.image.color = (s.classData == manager.Current) ? selectedColor : normalColor;

            bool max = manager.IsMaxLevel(s.classData);
            float xp = manager.GetXP(s.classData);
            float needed = manager.GetXPToNext(s.classData);

            if (s.xpFill != null)
                s.xpFill.fillAmount = max ? 1f : (needed > 0f ? Mathf.Clamp01(xp / needed) : 0f);

            if (s.xpLabel != null)
                s.xpLabel.text = max ? "MAX" : $"{Mathf.FloorToInt(xp)} / {Mathf.CeilToInt(needed)} XP";
        }
    }
}