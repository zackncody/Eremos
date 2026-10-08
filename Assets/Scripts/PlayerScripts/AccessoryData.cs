using UnityEngine;
[System.Serializable]
public class AccessoryData
{
    public EquipSlot slot;
    public int bonusDamage = 0;
    public float attackSpeedBonus = 0f;
    [Range(0f, 1f)] public float burnChanceBonus = 0f;
    [Range(0f, 1f)] public float poisonChanceBonus = 0f;
    [Range(0f, 1f)] public float slowChanceBonus = 0f;
    [Range(0f, 1f)] public float bleedChanceBonus = 0f;
    public float knockbackBonus = 0f;
    public bool providesLight = false;
}