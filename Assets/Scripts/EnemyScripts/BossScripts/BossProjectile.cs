using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float damage = 15f;
    public float knockbackForce = 12f;

    void Start()
    {
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss != null)
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), boss.GetComponent<Collider2D>());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage((int)damage);

            PlayerCode pc = other.GetComponent<PlayerCode>();
            if (pc != null)
            {
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                pc.StartCoroutine(pc.ApplyKnockback(knockbackDir * knockbackForce));
            }

            Destroy(gameObject);
        }
        else if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            return;
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}