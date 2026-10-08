using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    public TMP_Text percentText;

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float healthRatio = maxHealth > 0f ? currentHealth / maxHealth : 0f;
        healthRatio = Mathf.Clamp01(healthRatio);

        fillImage.fillAmount = healthRatio;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(healthRatio * 100f) + "%";
    }
}