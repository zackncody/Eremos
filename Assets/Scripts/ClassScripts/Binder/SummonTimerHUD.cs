using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonTimerHUD : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Image fill;          // Image Type = Filled, Fill Method = Horizontal
    [SerializeField] private TMP_Text timeLabel;  // optional, e.g. "12s"
    [SerializeField] private float fadeSpeed = 6f;

    private void Start()
    {
        if (group != null) group.alpha = 0f;
    }

    private void Update()
    {
        SummonedAlly s = SummonedAlly.Current;
        bool show = s != null;

        if (show)
        {
            if (fill != null) fill.fillAmount = s.RemainingNormalized;
            if (timeLabel != null)
                timeLabel.text = float.IsInfinity(s.RemainingSeconds) ? "" : Mathf.CeilToInt(s.RemainingSeconds) + "s";
        }

        if (group != null)
            group.alpha = Mathf.MoveTowards(group.alpha, show ? 1f : 0f, fadeSpeed * Time.unscaledDeltaTime);
    }
}