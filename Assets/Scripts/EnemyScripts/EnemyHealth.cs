using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] GameObject Bar;
    public Slider slider;
    public int health;
    public int maxHealth = 10;
    public event System.Action OnDeath;

    void Start()
    {
        health = maxHealth;
        slider.maxValue = maxHealth;
        slider.value = health;
    }

    public void TakeDamage(int amount)
    {
        MarkedEnemy mark = GetComponent<MarkedEnemy>();
        if (mark != null)
            amount = Mathf.CeilToInt(amount * mark.Multiplier);

        health -= amount;
        slider.value = health;

        if (health > maxHealth)
            Burned();

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        OnDeath?.Invoke();

        LootTable loot = GetComponent<LootTable>();
        if (loot != null)
            loot.SpawnLoot(transform.position);

        Destroy(Bar);
        Destroy(gameObject);
    }

    public void Burned()
    {
    }
}