using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityHUD : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image cooldownFill;   // Image Type = Filled, Fill Method = Radial 360
    [SerializeField] private TMP_Text keyLabel;    // optional

    private ClassAbility shown;

    private void Update()
    {
        ClassManager cm = GameServices.Classes;
        ClassAbility current = cm != null ? cm.CurrentAbility : null;

        if (current != shown)
        {
            shown = current;
            bool has = shown != null;

            if (icon != null)
            {
                icon.enabled = has;
                if (has) icon.sprite = shown.icon;
            }
            if (keyLabel != null)
                keyLabel.text = has ? shown.keyHint : "";
        }

        if (cooldownFill != null)
        {
            cooldownFill.enabled = shown != null;
            cooldownFill.fillAmount = shown != null ? cm.GetCooldownNormalized(shown) : 0f;
        }
    }
}