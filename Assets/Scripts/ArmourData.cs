using UnityEngine;
[System.Serializable]
public class ArmourData
{
    public EquipSlot slot;
    public int defense = 0;
    [Range(0f, 1f)] public float fireResistance = 0f;
    [Range(0f, 1f)] public float poisonResistance = 0f;
    [Range(0f, 1f)] public float slowResistance = 0f;
    [Range(0f, 1f)] public float bleedResistance = 0f;

    [Header("Visuals")]
    public Sprite equippedSprite;
}