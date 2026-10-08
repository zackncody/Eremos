using UnityEngine;

public class AoEWave : MonoBehaviour
{
    [Header("Size")]
    public float initialRadius = 0.1f;
    public float finalRadius = 5f;

    [Header("Expansion")]
    public float expandSpeed = 5f;

    [Header("Damage")]
    public float damage = 40f;

    private bool hasHitPlayer = false;
    private CircleCollider2D col;
    private float currentRadius;

    void Awake()
    {
        col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1f; // keep at 1 — scale drives the visual size

        currentRadius = initialRadius;
        transform.localScale = Vector3.one * initialRadius;
    }

    void Update()
    {
        if (currentRadius >= finalRadius)
        {
            Destroy(gameObject);
            return;
        }

        currentRadius = Mathf.MoveTowards(currentRadius, finalRadius, expandSpeed * Time.deltaTime);
        transform.localScale = Vector3.one * currentRadius;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitPlayer) return;
        if (other.CompareTag("Player"))
        {
            hasHitPlayer = true;
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage((int)damage);
        }
    }
}