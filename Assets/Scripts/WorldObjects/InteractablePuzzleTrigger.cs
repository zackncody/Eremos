using UnityEngine;

public class InteractablePuzzleTrigger : MonoBehaviour
{
    public int objectIndex;
    public Animator animator;
    public string animationTriggerName = "Activate";

    private void Start()
    {
        bool puzzleSolved = PlayerPrefs.GetInt(
            InteractablePuzzleManager.Instance.puzzleSaveKey, 0) == 1;
        if (puzzleSolved && animator != null)
            animator.SetBool(animationTriggerName, true);
    }

    void OnMouseDown()
    {
        InteractablePuzzleManager.Instance.RegisterClick(objectIndex);
        if (animator != null)
            animator.SetBool(animationTriggerName, true);
    }

    public void ResetState()
    {
        if (animator != null)
            animator.SetBool(animationTriggerName, false);
    }
}