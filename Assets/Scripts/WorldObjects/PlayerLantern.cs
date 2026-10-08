using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerLantern : MonoBehaviour
{
    public static PlayerLantern Instance { get; private set; }

    [SerializeField] private Light2D lanternLight;
    [SerializeField] private float lanternIntensity = 1.2f;
    [SerializeField] private float fadeSpeed = 2f;

    private bool isEquipped = false;
    private float targetIntensity;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        DayNightCycle.Instance.OnPeriodChanged += HandlePeriodChanged;
    }

    void OnDisable()
    {
        if (DayNightCycle.Instance == null) return;
        DayNightCycle.Instance.OnPeriodChanged -= HandlePeriodChanged;
    }

    void Start()
    {
        RefreshTarget();
        lanternLight.intensity = targetIntensity;
    }

    void Update()
    {
        lanternLight.intensity = Mathf.MoveTowards(lanternLight.intensity, targetIntensity, fadeSpeed * Time.deltaTime);
    }

    // Called by EquipmentManager when the lantern accessory is equipped/unequipped
    public void SetLanternEquipped(bool equipped)
    {
        isEquipped = equipped;
        RefreshTarget();
    }

    private void HandlePeriodChanged(DayNightCycle.TimeOfDay newPeriod)
    {
        RefreshTarget();
    }

    private void RefreshTarget()
    {
        bool isNight = !DayNightCycle.Instance.IsDay;
        targetIntensity = (isEquipped && isNight) ? lanternIntensity : 0f;
    }
}