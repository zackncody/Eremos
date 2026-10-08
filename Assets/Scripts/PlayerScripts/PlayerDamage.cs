using TMPro;
using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    public int damage = 5;
    public GameObject popUpPrefab;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Hit: {collision.gameObject.name} | Tag: {collision.gameObject.tag}");
        bool isEnemy = collision.gameObject.CompareTag("Enemy");
        bool isBoss = collision.gameObject.CompareTag("Boss");

        if (isEnemy || isBoss)
        {
            // Try boss first
            BossController boss = collision.gameObject.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                return;
            }

            // Fall back to regular enemy
            EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }
    }
}