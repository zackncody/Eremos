using UnityEngine;

public class PoisonStatus : MonoBehaviour
{
    [Header("Poison Settings")]
    public float poisonDamage = 2f;
    public float poisonInterval = 0.5f;
    public float poisonDuration = 3f;

    [Header("Poison Indicator")]
    public Sprite poisonSprite;
    public Vector3 indicatorOffset = new Vector3(0f, 2f, 0f);
    public float indicatorScale = 0.4f;

    private float poisonTimer = 0f;
    private float tickTimer = 0f;
    private bool isPoisoned = false;
    private EnemyHealth enemyHealth;
    private GameObject indicator;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        CreateIndicator();
    }

    private void CreateIndicator()
    {
        indicator = new GameObject("PoisonIndicator");
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = indicatorOffset;
        indicator.transform.localScale = Vector3.one * indicatorScale;
        SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
        sr.sprite = poisonSprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 10;
        indicator.SetActive(false);
    }

    public void Apply()
    {
        poisonTimer = poisonDuration;
        tickTimer = 0f;
        isPoisoned = true;
        indicator.SetActive(true);
    }

    private void Update()
    {
        if (!isPoisoned) return;

        poisonTimer -= Time.deltaTime;
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            if (enemyHealth != null)
                enemyHealth.TakeDamage((int)poisonDamage);
            tickTimer = poisonInterval;
        }

        if (poisonTimer <= 0f)
        {
            isPoisoned = false;
            indicator.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (indicator != null)
            Destroy(indicator);
    }
}