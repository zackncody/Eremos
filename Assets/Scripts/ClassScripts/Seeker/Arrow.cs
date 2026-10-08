using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 18f;
    public float maxLifetime = 5f;

    private Vector2 direction;
    private int damage;
    private bool hasHit = false;

    public void Init(Vector2 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        Destroy(gameObject, maxLifetime);
    }

    private void Update()
    {
        if (hasHit) return;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (other.CompareTag("Player")) return;
        if (other.CompareTag("Arrow")) return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>()
            ?? other.GetComponentInParent<EnemyHealth>()
            ?? other.GetComponentInChildren<EnemyHealth>();

        if (enemy != null)
        {
            hasHit = true;
            enemy.TakeDamage(damage);
            if (GameServices.Classes != null) GameServices.Classes.ReportDamage(damage);
            Destroy(gameObject);
            return;
        }

        BossController boss = other.GetComponent<BossController>()
            ?? other.GetComponentInParent<BossController>()
            ?? other.GetComponentInChildren<BossController>();

        if (boss != null)
        {
            hasHit = true;
            boss.TakeDamage(damage);
            if (GameServices.Classes != null) GameServices.Classes.ReportDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }
}