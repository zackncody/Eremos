using UnityEngine;

public class BossArenaZone : MonoBehaviour
{
    public BossController boss;
    public BossHUD bossHUD;

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player") && boss != null)
        {
            triggered = true;

            boss.ActivateBoss();

            if (bossHUD != null)
            {
                
                bossHUD.Show(boss.maxHealth);
            }
            else
            {
                Debug.LogWarning("BossHUD not assigned!");
            }
        }
    }
}