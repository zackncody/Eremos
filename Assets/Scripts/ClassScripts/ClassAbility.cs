using UnityEngine;

[CreateAssetMenu(menuName = "Classes/Class Ability", fileName = "NewAbility")]
public class ClassAbility : ScriptableObject
{
    public string abilityID;
    public string classID;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public int unlockLevel = 1;
    public float cooldown;
    public float cost;
    public string keyHint;
}