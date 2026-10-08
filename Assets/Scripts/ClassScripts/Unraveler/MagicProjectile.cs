using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    private Vector2 direction;
    private int damage;
    private float speed;

    public void Init(Vector2 dir, int dmg, float projectileSpeed)
    {
        direction = dir.normalized;
        damage = dmg;
        speed = projectileSpeed;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        GameServices.ReportPlayerDamage(other, damage);

        Destroy(gameObject);
    }
}