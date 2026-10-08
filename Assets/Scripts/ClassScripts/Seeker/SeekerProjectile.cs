using UnityEngine;

public enum SeekerEffect { Mark, Poison }

public class SeekerProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    private Vector2 direction;
    private float speed;
    private SeekerEffect effect;
    private float multiplier;
    private float duration;

    public void Init(Vector2 dir, float projectileSpeed, SeekerEffect effectType, float damageMultiplier, float markDuration)
    {
        direction = dir;
        speed = projectileSpeed;
        effect = effectType;
        multiplier = damageMultiplier;
        duration = markDuration;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Item") || other.CompareTag("EnemyProjectile")) return;
        if (other.GetComponentInParent<PlayerHealth>() != null) return;
        if (other.GetComponentInParent<Weapon>() != null) return;

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null) enemy = other.GetComponentInChildren<EnemyHealth>();

        if (enemy != null)
        {
            if (effect == SeekerEffect.Mark)
            {
                MarkedEnemy.Apply(enemy.gameObject, multiplier, duration);
            }
            else
            {
                StatusEffects status = enemy.GetComponentInParent<StatusEffects>();
                if (status == null) status = enemy.GetComponentInChildren<StatusEffects>();

                if (status != null)
                {
                    status.ApplyPoison();
                    if (SeekerSystem.Instance != null) SeekerSystem.Instance.RegisterPoisonHit();
                    Debug.Log($"SeekerProjectile: poisoned {enemy.name}");
                }
                else
                {
                    Debug.LogWarning("SeekerProjectile: no StatusEffects on " + enemy.name);
                }
            }

            Destroy(gameObject);
            return;
        }

        // Solid things like walls stop the projectile.
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}