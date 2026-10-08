using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBarUI : MonoBehaviour
{
    public RectTransform barContainer;
    public Image fillImage;
    public TMP_Text percentText;

    public float widthPerUnit = 40f;
    public float baseWidth = 120f;
    public float baseMaxResource = 3f;

    void Start()
    {
        if (PlayerResourceManager.Instance != null)
        {
            PlayerResourceManager.Instance.OnResourceChanged += Refresh;
            Refresh();
        }
    }

    void OnDisable()
    {
        if (PlayerResourceManager.Instance != null)
            PlayerResourceManager.Instance.OnResourceChanged -= Refresh;
    }

    void Refresh()
    {
        float max = PlayerResourceManager.Instance.maxResource;
        float current = PlayerResourceManager.Instance.currentResource;

        float targetWidth = baseWidth + (max - baseMaxResource) * widthPerUnit;
        barContainer.sizeDelta = new Vector2(targetWidth, barContainer.sizeDelta.y);

        float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        fillImage.fillAmount = ratio;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(ratio * 100f) + "%";
    }
}