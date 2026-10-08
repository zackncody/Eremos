using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Classes/Class Data", fileName = "NewClass")]
public class ClassData : ScriptableObject
{
    [Header("Identity")]
    public string classID = "vessel";
    public string displayName = "Vessel";

    [Header("Leveling")]
    public int levelCap = 10;
    public float baseXP = 100f;        // XP needed for level 1 -> 2
    public float xpGrowth = 1.5f;      // each level needs this much more than the last
    public float xpPerDamage = 1f;     // XP earned per point of damage dealt while ability is active

    [Header("Ability")]
    public float abilityCost = 2f;     // resource spent on activation
    public float baseDuration = 4f;    // active window at level 1 (seconds)
    public float durationPerLevel = 0.5f;
    public float baseCooldown = 10f;   // starts after the window ends
    public float cooldownReductionPerLevel = 0.5f;

    [Header("Abilities")]
    public List<ClassAbility> abilities = new List<ClassAbility>();

    public float XPToNext(int level)
    {
        return baseXP * Mathf.Pow(xpGrowth, Mathf.Max(0, level - 1));
    }
}