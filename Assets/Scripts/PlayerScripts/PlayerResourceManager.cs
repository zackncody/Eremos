using UnityEngine;

public class PlayerResourceManager : MonoBehaviour
{
    public static PlayerResourceManager Instance { get; private set; }

    public float maxResource = 3f;
    public float currentResource;
    public float regenPerSecond = 0.5f;
    public float regenDelayAfterUse = 1.5f; 

    private float regenPauseTimer;

    public event System.Action OnResourceChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        currentResource = maxResource;
    }

    void Update()
    {
        if (currentResource >= maxResource) return;

        if (regenPauseTimer > 0f)
        {
            regenPauseTimer -= Time.deltaTime;
            return;
        }

        float before = currentResource;
        currentResource = Mathf.Min(maxResource, currentResource + regenPerSecond * Time.deltaTime);

        if (!Mathf.Approximately(before, currentResource))
            OnResourceChanged?.Invoke();
    }

    public bool TrySpend(float amount)
    {
        if (currentResource < amount) return false;
        currentResource -= amount;
        regenPauseTimer = regenDelayAfterUse;
        Debug.Log($"Spent {amount}, currentResource now {currentResource}");
        OnResourceChanged?.Invoke();
        return true;
    }
    public void Refill(float amount)
    {
        currentResource = Mathf.Min(maxResource, currentResource + amount);
        OnResourceChanged?.Invoke();
    }

    public void ExpandMax(float amount)
    {
        maxResource += amount;
        currentResource += amount;
        OnResourceChanged?.Invoke();
    }
}