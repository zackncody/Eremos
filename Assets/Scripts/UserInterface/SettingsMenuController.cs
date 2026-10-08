using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    
    [SerializeField] private Button resetPuzzlesButton;

    private void Awake()
    {
        resetPuzzlesButton.onClick.AddListener(OnResetPuzzlesClicked);
    }

    private void OnResetPuzzlesClicked()
    {
        InteractablePuzzleManager.ResetAllPuzzles();
    }
}