using UnityEngine;
using UnityEngine.UI;

public class HealthVignette : MonoBehaviour
{
    [Header("References")]
    public Image overlayImage;
    private PlayerHealth playerHealth;

    [Header("Settings")]
    [Range(0f, 1f)] public float healthThreshold = 0.5f;
    public float maxAlpha = 0.7f;
    public float pulseSpeed = 2f;
    public float pulseMagnitude = 0.15f;

    private float _currentAlpha = 0f;
    private float _pulseTimer = 0f;

    void Awake()
    {
        if (overlayImage == null)
            overlayImage = GetComponent<Image>();

        playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    void Update()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null || playerHealth.health <= 0)
        {
            _currentAlpha = Mathf.Lerp(_currentAlpha, 0f, Time.unscaledDeltaTime * 3f);
            Color c = overlayImage.color;
            c.a = _currentAlpha;
            overlayImage.color = c;
            return;
        }

        UpdateVignette(playerHealth.GetHealthPercent());
    }

    void UpdateVignette(float healthPercent)
    {
        if (healthPercent < healthThreshold)
        {
            float danger = 1f - (healthPercent / healthThreshold);
            _pulseTimer += Time.unscaledDeltaTime * pulseSpeed * (1f + danger * 2f);
            float pulse = (Mathf.Sin(_pulseTimer) * 0.5f + 0.5f) * pulseMagnitude;
            float targetAlpha = maxAlpha * danger + pulse;
            _currentAlpha = Mathf.Lerp(_currentAlpha, targetAlpha, Time.unscaledDeltaTime * 5f);
        }
        else
        {
            _currentAlpha = Mathf.Lerp(_currentAlpha, 0f, Time.unscaledDeltaTime * 3f);
        }

        Color c = overlayImage.color;
        c.a = _currentAlpha;
        overlayImage.color = c;
    }
}