using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 10f;
    public bool destroyOnHit = true;

    [Header("Rotation")]
    [Tooltip("Adjust this if the projectile sprite isn't pointing right by default. 0 = pointing right, 90 = pointing up, -90 = pointing down, 180 = pointing left")]
    public float rotationOffset = 0f;

    private void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.linearVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage((int)damage);

            if (destroyOnHit) Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            PlayerHealth ph = col.gameObject.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage((int)damage);

            if (destroyOnHit) Destroy(gameObject);
            return;
        }

        if (destroyOnHit) Destroy(gameObject);
    }
}