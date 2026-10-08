using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHUD : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthBar;
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI healthText;

    [Header("Settings")]
    public string bossName = "The Corrupted One";
    public float fadeInSpeed = 2f;

    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    private float maxHealth;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        if (bossNameText != null)
            bossNameText.text = bossName;

        UpdatePhase(1);
    }

    void Update()
    {
        float targetAlpha = isVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(
            canvasGroup.alpha,
            targetAlpha,
            fadeInSpeed * Time.deltaTime);
    }

    public void Show(float max)
    {
        isVisible = true;
        maxHealth = max;
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
        UpdateHealthText(maxHealth);
        UpdatePhase(1);
    }

    public void Hide()
    {
        isVisible = false;
    }

    public void UpdateHealth(float current)
    {
        healthBar.value = current;
        UpdateHealthText(current);
    }

    private void UpdateHealthText(float current)
    {
        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(maxHealth)}";
    }

    public void UpdatePhase(int phase)
    {
        if (phaseText == null)
            return;

        switch (phase)
        {
            case 1: phaseText.text = "Phase 1"; break;
            case 2: phaseText.text = "Phase 2"; break;
            case 3: phaseText.text = "Final Phase"; break;
            default: phaseText.text = $"Phase {phase}"; break;
        }
    }
}