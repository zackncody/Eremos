using System;
using System.Collections.Generic;
using UnityEngine;

public class ClassManager : MonoBehaviour, ISaveable
{
    [Serializable]
    public class ClassState
    {
        public string classID;
        public int level = 1;
        public float xp;
        public string selectedAbilityID;
    }

    [Serializable]
    public class SaveBlob
    {
        public List<ClassState> states = new List<ClassState>();
        public string currentClassID;
    }

    [SerializeField] private List<ClassData> classes = new List<ClassData>();

    private readonly Dictionary<string, ClassState> states = new Dictionary<string, ClassState>();
    private ClassData current;
    private bool abilityActive;

    public IReadOnlyList<ClassData> Classes => classes;
    public ClassData Current => current;
    public bool AbilityActive => abilityActive;
    public ClassAbility CurrentAbility => GetSelectedAbility(current);

    public event Action<ClassData, int> OnLevelUp;
    public event Action<ClassData> OnClassChanged;
    public event Action<ClassData, ClassAbility> OnAbilityChanged;
    public event Action<ClassData, float> OnXPChanged;

    public string SaveKey => "classes";

    private void Awake()
    {
        if (GameServices.Classes != null && GameServices.Classes != this)
        {
            Destroy(this);
            return;
        }

        GameServices.RegisterClasses(this);

        foreach (ClassData c in classes)
        {
            if (c != null && !states.ContainsKey(c.classID))
                states[c.classID] = new ClassState { classID = c.classID };
        }

        if (classes.Count > 0) current = classes[0];
    }

    private void OnDestroy()
    {
        if (GameServices.Classes == this)
            GameServices.ClearClasses();
    }

    // ── Class selection ───────────────────────────────────────
    public void SetClass(ClassData c)
    {
        if (c == null || c == current || !states.ContainsKey(c.classID)) return;

        abilityActive = false;
        current = c;
        OnClassChanged?.Invoke(c);
        Debug.Log($"ClassManager: switched to {c.displayName}");
    }

    // Each class's ability system (e.g. RageSystem for Vessel) calls this
    // to say whether damage dealt right now should earn class XP.
    public void SetAbilityActive(bool active)
    {
        abilityActive = active;
    }

    public int GetLevel(ClassData c) => states[c.classID].level;
    public float GetXP(ClassData c) => states[c.classID].xp;
    public float GetXPToNext(ClassData c) => c.XPToNext(states[c.classID].level);
    public bool IsMaxLevel(ClassData c) => states[c.classID].level >= c.levelCap;

    // ── Ability selection ─────────────────────────────────────
    public ClassAbility GetSelectedAbility(ClassData c)
    {
        if (c == null || !states.ContainsKey(c.classID)) return null;
        ClassState s = states[c.classID];

        ClassAbility firstUnlocked = null;
        foreach (ClassAbility a in c.abilities)
        {
            if (a == null || s.level < a.unlockLevel) continue;
            if (firstUnlocked == null) firstUnlocked = a;
            if (a.abilityID == s.selectedAbilityID) return a;
        }

        // Nothing valid selected: default to the first unlocked ability.
        if (firstUnlocked != null) s.selectedAbilityID = firstUnlocked.abilityID;
        return firstUnlocked;
    }

    public bool SelectAbility(ClassData c, ClassAbility a)
    {
        if (c == null || a == null || !states.ContainsKey(c.classID)) return false;
        if (!c.abilities.Contains(a)) return false;

        ClassState s = states[c.classID];
        if (s.level < a.unlockLevel) return false;
        if (s.selectedAbilityID == a.abilityID) return true;

        if (c == current) abilityActive = false;
        s.selectedAbilityID = a.abilityID;
        OnAbilityChanged?.Invoke(c, a);
        Debug.Log($"ClassManager: {c.displayName} ability set to {a.displayName}");
        return true;
    }
    // ── Cooldowns (read by the HUD) ───────────────────────────
    private readonly Dictionary<string, (float end, float duration)> cooldowns
        = new Dictionary<string, (float, float)>();

    public void StartCooldown(ClassAbility a, float duration)
    {
        if (a == null || duration <= 0f) return;
        cooldowns[a.abilityID] = (Time.time + duration, duration);
    }

    // 1 = just used, drains to 0 = ready.
    public float GetCooldownNormalized(ClassAbility a)
    {
        if (a == null || !cooldowns.TryGetValue(a.abilityID, out var cd)) return 0f;
        float remaining = cd.end - Time.time;
        return remaining <= 0f ? 0f : Mathf.Clamp01(remaining / cd.duration);
    }
    public float GetCooldownRemaining(ClassAbility a)
    {
        if (a == null || !cooldowns.TryGetValue(a.abilityID, out var cd)) return 0f;
        return Mathf.Max(0f, cd.end - Time.time);
    }

    public bool IsOnCooldown(ClassAbility a) => GetCooldownRemaining(a) > 0f;
    // ── XP ────────────────────────────────────────────────────
    // Call this every time the player deals damage. XP is only
    // granted while the current class's ability is active.
    public void ReportDamage(float amount)
    {
        Debug.Log($"ClassManager.ReportDamage({amount}): class={(current != null ? current.classID : "NONE")}, abilityActive={abilityActive}");

        if (current == null || !abilityActive) return;

        ClassState s = states[current.classID];
        if (s.level >= current.levelCap) return;

        float gained = amount * current.xpPerDamage;
        s.xp += gained;

        float needed = current.XPToNext(s.level);
        while (s.xp >= needed && s.level < current.levelCap)
        {
            s.xp -= needed;
            s.level++;
            Debug.Log($"ClassManager: {current.displayName} reached level {s.level}");
            OnLevelUp?.Invoke(current, s.level);
            needed = current.XPToNext(s.level);
        }

        if (s.level >= current.levelCap) s.xp = 0f;

        OnXPChanged?.Invoke(current, gained);
        Debug.Log($"ClassManager: {current.displayName} L{s.level} XP {s.xp:F1}/{needed:F1}");
    }

    // ── Save / Load (ISaveable) ───────────────────────────────
    public object CaptureState()
    {
        SaveBlob blob = new SaveBlob { currentClassID = current != null ? current.classID : null };
        foreach (ClassState s in states.Values)
            blob.states.Add(s);
        return blob;
    }

    public void RestoreState(string json)
    {
        SaveBlob blob = JsonUtility.FromJson<SaveBlob>(json);
        if (blob == null) return;

        foreach (ClassState s in blob.states)
        {
            if (states.ContainsKey(s.classID))
                states[s.classID] = s;
        }

        foreach (ClassData c in classes)
        {
            if (c != null && c.classID == blob.currentClassID)
                current = c;
        }

        abilityActive = false;
        OnClassChanged?.Invoke(current);
    }
}