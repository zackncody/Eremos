using TMPro;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 5;
    public GameObject popUpPrefab;

    [Header("Peace Settings")]
    [Tooltip("Tick this if enemies in this area should go peaceful when the boss dies.")]
    public bool tiedToBoss = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (tiedToBoss && BossController.IsDead) return;

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(damage);
    }
}