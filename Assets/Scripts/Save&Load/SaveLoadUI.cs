using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hook this up to Save/Load/Delete buttons in the UI (main menu,
/// pause menu, etc). Each public method maps directly to a Button's
/// OnClick() event in the Inspector — no code changes needed per button.
/// </summary>
public class SaveLoadUI : MonoBehaviour
{
    [Header("Optional — shown/hidden based on save file existing")]
    [SerializeField] private GameObject continueButton;

    private void Start()
    {
        RefreshContinueButton();
    }

    private void RefreshContinueButton()
    {
        if (continueButton == null) return;

        bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSaveFile();
        continueButton.SetActive(hasSave);
    }

    // ── Hook these to Button OnClick() events ──────────────────

    public void OnSaveButtonPressed()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("SaveLoadUI: no SaveManager in scene.");
            return;
        }

        SaveManager.Instance.SaveGame();
        RefreshContinueButton();
    }

    public void OnLoadButtonPressed()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("SaveLoadUI: no SaveManager in scene.");
            return;
        }

        if (!SaveManager.Instance.HasSaveFile())
        {
            Debug.LogWarning("SaveLoadUI: no save file to load.");
            return;
        }

        SaveManager.Instance.LoadGame();
    }

    public void OnDeleteSaveButtonPressed()
    {
        if (SaveManager.Instance == null) return;

        SaveManager.Instance.DeleteSave();
        RefreshContinueButton();
    }
}