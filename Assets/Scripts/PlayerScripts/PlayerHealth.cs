using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public TMP_Text HealthText;
    public Slider slider;
    public int health;
    public int maxHealth = 100;

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.savedHealth > 0)
            health = GameManager.Instance.savedHealth;
        else
            health = maxHealth;

        slider.maxValue = maxHealth;
        UpdateUI();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RelinkHudNextFrame());
    }

    private IEnumerator RelinkHudNextFrame()
    {
        yield return null; // wait one frame so scene objects finish initializing
        RelinkHud();
    }

    private void RelinkHud()
    {
        GameObject hudSliderObj = GameObject.FindGameObjectWithTag("HealthSlider");
        Debug.Log("RelinkHud fired. Found slider: " + (hudSliderObj != null));
        if (hudSliderObj != null)
            slider = hudSliderObj.GetComponent<Slider>();

        GameObject hudTextObj = GameObject.FindGameObjectWithTag("HealthText"); // adjust tag/name to match your HUD
        if (hudTextObj != null)
            HealthText = hudTextObj.GetComponent<TMP_Text>();

        if (slider != null)
            slider.maxValue = maxHealth;

        UpdateUI();
    }

    public float GetHealthPercent()
    {
        return (float)health / maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (health <= 0) return;

        int defense = EquipmentManager.Instance != null
            ? EquipmentManager.Instance.TotalDefense : 0;
        int reduced = Mathf.Max(1, amount - defense);
        health -= reduced;
        health = Mathf.Max(0, health);
        UpdateUI();

        if (GameServices.Rage != null)
            GameServices.Rage.OnPlayerHit();

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        DeathManager.Instance.RegisterDeath();

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.OnPlayerDied();
    }

    public void Respawn()
    {
        health = maxHealth;
        if (slider != null)
            slider.maxValue = maxHealth;
        UpdateUI();

        if (GameServices.Rage != null)
            GameServices.Rage.ResetRage();
    }

    private void UpdateUI()
    {
        if (slider != null)
            slider.value = health;

        if (HealthText != null)
        {
            int percent = maxHealth > 0 ? Mathf.RoundToInt((float)health / maxHealth * 100f) : 0;
            HealthText.SetText($"{percent}%");
        }

        if (GameManager.Instance != null)
            GameManager.Instance.savedHealth = health;
    }

    public void Heal(int amount)
    {
        if (health <= 0) return;
        health += amount;
        health = Mathf.Min(health, maxHealth);
        UpdateUI();
    }
    public bool LoseHealth(int amount)
    {
        if (health <= amount) return false;
        health -= amount;
        UpdateUI();
        return true;
    }
}