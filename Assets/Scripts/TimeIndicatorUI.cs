using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeIndicatorUI : MonoBehaviour
{
    [Header("References")]
    public Image periodIcon;
    public TextMeshProUGUI periodLabel;

    [Header("Icons (assign in Inspector)")]
    public Sprite dawnIcon;
    public Sprite dayIcon;
    public Sprite duskIcon;
    public Sprite nightIcon;

    void Start()
    {
        StartCoroutine(WaitForDayNightCycle());
    }

    System.Collections.IEnumerator WaitForDayNightCycle()
    {
        while (DayNightCycle.Instance == null)
            yield return null;

        DayNightCycle.Instance.OnPeriodChanged += HandlePeriodChanged;
        HandlePeriodChanged(DayNightCycle.Instance.CurrentPeriod);
    }

    void OnDestroy()
    {
        if (DayNightCycle.Instance != null)
            DayNightCycle.Instance.OnPeriodChanged -= HandlePeriodChanged;
    }
    

    void HandlePeriodChanged(DayNightCycle.TimeOfDay period)
    {
        switch (period)
        {
            case DayNightCycle.TimeOfDay.Dawn:
                periodIcon.sprite = dawnIcon;
                periodLabel.text = "Dawn";
                break;
            case DayNightCycle.TimeOfDay.Day:
                periodIcon.sprite = dayIcon;
                periodLabel.text = "Day";
                break;
            case DayNightCycle.TimeOfDay.Dusk:
                periodIcon.sprite = duskIcon;
                periodLabel.text = "Dusk";
                break;
            case DayNightCycle.TimeOfDay.Night:
                periodIcon.sprite = nightIcon;
                periodLabel.text = "Night";
                break;
        }
    }
}