using UnityEngine;

public class MarkedEnemy : MonoBehaviour
{
    public static int ActiveCount { get; private set; }

    public float Multiplier { get; private set; } = 1f;

    private static readonly Color markColor = new Color(1f, 0.25f, 0.25f, 1f);
    private const float tintStrength = 0.7f;

    private float endTime;
    private SpriteRenderer[] renderers;
    private Color[] originalColors;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ActiveCount = 0;
    }

    // Marks an enemy, or refreshes the timer if it's already marked.
    public static void Apply(GameObject enemy, float multiplier, float duration)
    {
        MarkedEnemy mark = enemy.GetComponent<MarkedEnemy>();
        if (mark == null) mark = enemy.AddComponent<MarkedEnemy>();

        mark.Multiplier = multiplier;
        mark.endTime = Time.time + duration;
        Debug.Log($"MarkedEnemy: {enemy.name} marked, x{multiplier} damage for {duration}s");
    }

    private void Awake()
    {
        ActiveCount++;

        renderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;
    }

    private void Update()
    {
        if (Time.time >= endTime)
            Destroy(this);
    }

    // LateUpdate so the red stays on top of any hit flash or status tint.
    private void LateUpdate()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = Color.Lerp(originalColors[i], markColor, tintStrength);
        }
    }

    private void OnDestroy()
    {
        ActiveCount--;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].color = originalColors[i];
        }
    }
}