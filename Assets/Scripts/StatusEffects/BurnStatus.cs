using UnityEngine;

public class BurnStatus : MonoBehaviour
{
    [Header("Burn Settings")]
    public float burnDamage = 2f;
    public float burnInterval = 0.5f;
    public float burnDuration = 3f;

    [Header("Burn Indicator")]
    public Sprite burnSprite;                                     
    public Vector3 indicatorOffset = new Vector3(0f, 2f, 0f); 
    public float indicatorScale = 0.4f;                          

    private float burnTimer = 0f;
    private float tickTimer = 0f;
    private bool isBurning = false;

    private EnemyHealth enemyHealth;
    private GameObject indicator;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        CreateIndicator();
    }

    private void CreateIndicator()
    {
        indicator = new GameObject("BurnIndicator");
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = indicatorOffset;
        indicator.transform.localScale = Vector3.one * indicatorScale;

        SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
        sr.sprite = burnSprite;
        sr.sortingLayerName = "Default"; 
        sr.sortingOrder = 10;

        indicator.SetActive(false); 
    }

    
    public void Apply()
    {
        burnTimer = burnDuration; 
        tickTimer = 0f;
        isBurning = true;
        indicator.SetActive(true);
    }

    private void Update()
    {
        if (!isBurning) return;

        burnTimer -= Time.deltaTime;
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            if (enemyHealth != null)
                enemyHealth.TakeDamage((int)burnDamage);

            tickTimer = burnInterval;
        }

        if (burnTimer <= 0f)
        {
            isBurning = false;
            indicator.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (indicator != null)
            Destroy(indicator);
    }
}