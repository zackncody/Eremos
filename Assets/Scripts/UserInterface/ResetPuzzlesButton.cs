using UnityEngine;

public class ResetPuzzlesButton : MonoBehaviour
{
    public void OnClick()
    {
        InteractablePuzzleManager.ResetAllPuzzles();
    }
}