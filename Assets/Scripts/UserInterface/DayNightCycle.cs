using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    public enum TimeOfDay { Dawn, Day, Dusk, Night }
    public TimeOfDay CurrentPeriod { get; private set; }
    public event System.Action<TimeOfDay> OnPeriodChanged;

    [Header("Time Settings")]
    public float dayLengthInMinutes = 10f;
    [Range(0, 24)] public float currentTime = 12f;

    [Header("Day/Night Thresholds")]
    [Range(0, 24)] public float dayStartTime = 6f;
    [Range(0, 24)] public float nightStartTime = 18f;

    [Header("Dawn/Dusk")]
    [Range(0, 4)] public float dawnDuskDuration = 1f;

    [Header("Spot Light (Sun)")]
    public Light2D spotLight;
    public AnimationCurve spotIntensityOverTime;
    public Gradient spotColorOverTime;

    public bool IsDay { get; private set; }

    void Awake()
    {
        GameServices.RegisterDayNight(this);
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject sunObj = GameObject.FindGameObjectWithTag("SunLight");
        spotLight = sunObj != null ? sunObj.GetComponent<Light2D>() : null;
    }

    void Update()
    {
        float secondsPerGameHour = (dayLengthInMinutes * 60f) / 24f;
        currentTime += Time.deltaTime / secondsPerGameHour;
        if (currentTime >= 24f) currentTime -= 24f;

        UpdateLighting();
        UpdateDayNightState();
    }

    void UpdateLighting()
    {
        if (spotLight == null) return;
        float t = currentTime / 24f;
        spotLight.intensity = spotIntensityOverTime.Evaluate(t);
        spotLight.color = spotColorOverTime.Evaluate(t);
    }

    void UpdateDayNightState()
    {
        if (dayStartTime < nightStartTime)
            IsDay = currentTime >= dayStartTime && currentTime < nightStartTime;
        else
            IsDay = currentTime >= dayStartTime || currentTime < nightStartTime;

        TimeOfDay newPeriod;
        if (IsInWindow(dayStartTime)) newPeriod = TimeOfDay.Dawn;
        else if (IsInWindow(nightStartTime)) newPeriod = TimeOfDay.Dusk;
        else if (IsDay) newPeriod = TimeOfDay.Day;
        else newPeriod = TimeOfDay.Night;

        if (newPeriod != CurrentPeriod)
        {
            CurrentPeriod = newPeriod;
            OnPeriodChanged?.Invoke(CurrentPeriod);
        }
    }

    bool IsInWindow(float startTime)
    {
        float diff = Mathf.Abs(currentTime - startTime);
        if (diff > 12f) diff = 24f - diff;
        return diff <= dawnDuskDuration / 2f;
    }

    public string GetTimeString()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
        return $"{hours:00}:{minutes:00}";
    }
}