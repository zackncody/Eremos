using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [Header("General")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;

    [Header("Weapon Stats Panel")]
    [SerializeField] private GameObject weaponStatsPanel;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI weaponLevelText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI burnChanceText;
    [SerializeField] private TextMeshProUGUI poisonChanceText;
    [SerializeField] private TextMeshProUGUI slowChanceText;
    [SerializeField] private TextMeshProUGUI bleedChanceText;
    [SerializeField] private TextMeshProUGUI knockbackText;

    [Header("Armour Stats Panel")]
    [FormerlySerializedAs("armorStatsPanel")]
    [SerializeField] private GameObject armourStatsPanel;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI fireResistText;
    [SerializeField] private TextMeshProUGUI poisonResistText;
    [SerializeField] private TextMeshProUGUI slowResistText;
    [SerializeField] private TextMeshProUGUI bleedResistText;

    [Header("Accessory Stats Panel")]
    [SerializeField] private GameObject accessoryStatsPanel;
    [SerializeField] private TextMeshProUGUI bonusDamageText;
    [SerializeField] private TextMeshProUGUI attackSpeedBonusText;
    [SerializeField] private TextMeshProUGUI burnBonusText;
    [SerializeField] private TextMeshProUGUI poisonBonusText;
    [SerializeField] private TextMeshProUGUI slowBonusText;
    [SerializeField] private TextMeshProUGUI bleedBonusText;
    [SerializeField] private TextMeshProUGUI knockbackBonusText;

    [Header("Offset from cursor")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(15f, -15f);

    private RectTransform rt;
    private Canvas rootCanvas;

    private void Awake()
    {
        Instance = this;
        rt = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        rt.sizeDelta = new Vector2(150f, rt.sizeDelta.y);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            Input.mousePosition,
            rootCanvas.worldCamera,
            out Vector2 localPos);

        Vector2 tooltipPos = localPos + new Vector2(10f, -10f);
        Vector2 canvasSize = rootCanvas.GetComponent<RectTransform>().sizeDelta;
        float halfW = canvasSize.x * 0.5f;
        float halfH = canvasSize.y * 0.5f;

        if (tooltipPos.x + rt.sizeDelta.x > halfW)
            tooltipPos.x = localPos.x - rt.sizeDelta.x - 10f;
        if (tooltipPos.y - rt.sizeDelta.y < -halfH)
            tooltipPos.y = localPos.y + rt.sizeDelta.y + 10f;

        rt.anchoredPosition = tooltipPos;
    }

    public void Show(ItemData item)
    {
        gameObject.SetActive(true);
        itemNameText.text = item.itemName;
        itemDescriptionText.text = string.IsNullOrEmpty(item.description) ? "" : item.description;

        if (weaponStatsPanel != null) weaponStatsPanel.SetActive(false);
        if (armourStatsPanel != null) armourStatsPanel.SetActive(false);
        if (accessoryStatsPanel != null) accessoryStatsPanel.SetActive(false);

        if (item.weaponData != null && item.weaponData.damage > 0)
            ShowWeaponStats(item);
        else if (item.armourData != null && item.armourData.defense > 0)
            ShowArmourStats(item.armourData);
        else if (item.accessoryData != null)
            ShowAccessoryStats(item.accessoryData);
    }

    private void ShowWeaponStats(ItemData item)
    {
        WeaponData data = item.weaponData;
        if (weaponStatsPanel == null) return;
        weaponStatsPanel.SetActive(true);

        float leveledDamage = data.damage * item.DamageMultiplier;
        damageText.text = item.level > 1
            ? $"Damage: {leveledDamage:F1} (base {data.damage})"
            : $"Damage: {data.damage}";

        if (weaponLevelText != null)
            weaponLevelText.text = $"Lv {item.level}  ({Mathf.FloorToInt(item.currentXP)} / {Mathf.CeilToInt(item.XPToNextLevel)} XP)";

        attackSpeedText.text = $"Attack Speed: {data.attackSpeed:F1}x";
        burnChanceText.text = $"Burn: {data.burnChance * 100f:F0}%";
        poisonChanceText.text = $"Poison: {data.poisonChance * 100f:F0}%";
        slowChanceText.text = $"Slow: {data.slowChance * 100f:F0}%";
        bleedChanceText.text = $"Bleed: {data.bleedChance * 100f:F0}%";
        knockbackText.text = $"Knockback: {data.knockbackForce:F1}";
    }

    private void ShowArmourStats(ArmourData data)
    {
        if (armourStatsPanel == null) return;
        armourStatsPanel.SetActive(true);

        defenseText.text = $"Defense: {data.defense}";

        if (fireResistText != null)
            fireResistText.gameObject.SetActive(data.fireResistance > 0f);
        if (fireResistText != null && data.fireResistance > 0f)
            fireResistText.text = $"Fire Resist: {data.fireResistance * 100f:F0}%";

        if (poisonResistText != null)
            poisonResistText.gameObject.SetActive(data.poisonResistance > 0f);
        if (poisonResistText != null && data.poisonResistance > 0f)
            poisonResistText.text = $"Poison Resist: {data.poisonResistance * 100f:F0}%";

        if (slowResistText != null)
            slowResistText.gameObject.SetActive(data.slowResistance > 0f);
        if (slowResistText != null && data.slowResistance > 0f)
            slowResistText.text = $"Slow Resist: {data.slowResistance * 100f:F0}%";

        if (bleedResistText != null)
            bleedResistText.gameObject.SetActive(data.bleedResistance > 0f);
        if (bleedResistText != null && data.bleedResistance > 0f)
            bleedResistText.text = $"Bleed Resist: {data.bleedResistance * 100f:F0}%";
    }

    private void ShowAccessoryStats(AccessoryData data)
    {
        if (accessoryStatsPanel == null) return;
        accessoryStatsPanel.SetActive(true);

        if (bonusDamageText != null)
        {
            bonusDamageText.gameObject.SetActive(data.bonusDamage > 0);
            if (data.bonusDamage > 0)
                bonusDamageText.text = $"+ {data.bonusDamage} Damage";
        }
        if (attackSpeedBonusText != null)
        {
            attackSpeedBonusText.gameObject.SetActive(data.attackSpeedBonus > 0f);
            if (data.attackSpeedBonus > 0f)
                attackSpeedBonusText.text = $"+ {data.attackSpeedBonus:F1}x Attack Speed";
        }
        if (burnBonusText != null)
        {
            burnBonusText.gameObject.SetActive(data.burnChanceBonus > 0f);
            if (data.burnChanceBonus > 0f)
                burnBonusText.text = $"+ {data.burnChanceBonus * 100f:F0}% Burn";
        }
        if (poisonBonusText != null)
        {
            poisonBonusText.gameObject.SetActive(data.poisonChanceBonus > 0f);
            if (data.poisonChanceBonus > 0f)
                poisonBonusText.text = $"+ {data.poisonChanceBonus * 100f:F0}% Poison";
        }
        if (slowBonusText != null)
        {
            slowBonusText.gameObject.SetActive(data.slowChanceBonus > 0f);
            if (data.slowChanceBonus > 0f)
                slowBonusText.text = $"+ {data.slowChanceBonus * 100f:F0}% Slow";
        }
        if (bleedBonusText != null)
        {
            bleedBonusText.gameObject.SetActive(data.bleedChanceBonus > 0f);
            if (data.bleedChanceBonus > 0f)
                bleedBonusText.text = $"+ {data.bleedChanceBonus * 100f:F0}% Bleed";
        }
        if (knockbackBonusText != null)
        {
            knockbackBonusText.gameObject.SetActive(data.knockbackBonus > 0f);
            if (data.knockbackBonus > 0f)
                knockbackBonusText.text = $"+ {data.knockbackBonus:F1} Knockback";
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}