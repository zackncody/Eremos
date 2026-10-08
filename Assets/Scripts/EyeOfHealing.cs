using UnityEngine;

public class EyeOfHealing : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void SetReady(bool ready)
    {
        animator.SetBool("CanHeal", ready);
    }
}