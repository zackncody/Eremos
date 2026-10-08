using UnityEngine;

public class Heal : MonoBehaviour
{
    [Header("Heal Settings")]
    public KeyCode healKey = KeyCode.H;
    public int healAmount = 20;
    public float healCooldown = 10f;

    [Header("Damage Settings")]
    public KeyCode damageKey = KeyCode.J;
    public int damageAmount = 20;

    public PlayerHealth playerHealth;
    public EyeOfHealing eye;

    private float cooldownTimer;

    void Start()
    {
        eye.SetReady(true);
    }

    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                eye.SetReady(true);
            }
        }

        if (Input.GetKeyDown(healKey) && cooldownTimer <= 0f)
        {
            playerHealth.Heal(healAmount);
            cooldownTimer = healCooldown;
            eye.SetReady(false);
        }

        if (Input.GetKeyDown(damageKey))
        {
            playerHealth.TakeDamage(damageAmount);
        }
    }
}