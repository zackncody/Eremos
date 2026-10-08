using UnityEngine;
using UnityEngine.UI;

public class RageSystem : MonoBehaviour
{
    [Header("Class")]
    [SerializeField] private string requiredClassID = "vessel";
    [SerializeField] private string requiredAbilityID = "rage";

    [Header("Rage")]
    [SerializeField] private float maxRage = 100f;
    [SerializeField] private float fillPerHit = 10f;

    [Header("Passive Decay")]
    [SerializeField] private float decayDelay = 4f;       // seconds after last hit before decay starts
    [SerializeField] private float decayPerSecond = 5f;

    [Header("Spending (press key once, burns rage down to zero)")]
    [SerializeField] private KeyCode unleashKey = KeyCode.Q;
    [SerializeField] private float burnPerSecond = 25f;
    [SerializeField] private float maxDamageBonus = 1f;   // +100% damage at full rage while burning
    [SerializeField] private float speedBonus = 0.2f;     // +20% move speed while burning

    [Header("Vision")]
    [SerializeField] private float clearScale = 4.5f;     // overlay scale at 0 rage (vignette off-screen). Keep >= 4
    [SerializeField] private float fullRageScale = 1f;    // overlay scale at full rage. Keep >= 1
    [SerializeField] private float holeInner = 0.35f;     // clear radius of the vignette texture
    [SerializeField] private float holeOuter = 0.9f;      // radius where it becomes fully black
    [SerializeField] private int overlaySortingOrder = 50;

    private float rage;
    private float lastHitTime = -999f;
    private bool burning;

    private RawImage overlay;
    private RectTransform overlayRect;

    public float Rage => rage;
    public float MaxRage => maxRage;
    public float RageNormalized => maxRage > 0f ? rage / maxRage : 0f;
    public bool IsBurning => burning;
    public float DamageMultiplier => burning ? 1f + RageNormalized * maxDamageBonus : 1f;
    public float SpeedMultiplier => burning ? 1f + speedBonus : 1f;

    private bool ClassOK
    {
        get
        {
            ClassManager cm = GameServices.Classes;
            if (cm == null || cm.Current == null || cm.Current.classID != requiredClassID) return false;

            ClassAbility a = cm.CurrentAbility;
            return a != null && a.abilityID == requiredAbilityID;
        }
    }

    private void Awake()
    {
        if (GameServices.Rage != null && GameServices.Rage != this)
        {
            Destroy(this);
            return;
        }

        GameServices.RegisterRage(this);
        BuildOverlay();
    }

    private void OnDestroy()
    {
        if (GameServices.Rage == this)
            GameServices.ClearRage();
    }

    // ── Public API ────────────────────────────────────────────
    public void OnPlayerHit()
    {
        if (!ClassOK) return;

        rage = Mathf.Min(maxRage, rage + fillPerHit);
        lastHitTime = Time.time;
        Debug.Log($"RageSystem: rage {rage:F0}/{maxRage:F0}");
    }

    public void ResetRage()
    {
        rage = 0f;
        SetBurning(false);
        UpdateVision();
    }

    // ── Update ────────────────────────────────────────────────
    private void Update()
    {
        if (!ClassOK)
        {
            if (rage > 0f || burning) ResetRage();
            return;
        }

        // One press starts the burn; it always runs to zero.
        if (!burning && Input.GetKeyDown(unleashKey) && rage > 0f && !GameServices.InputBlocked)
            SetBurning(true);

        if (burning)
        {
            rage = Mathf.Max(0f, rage - burnPerSecond * Time.deltaTime);
            if (rage <= 0f) SetBurning(false);
        }
        else if (rage > 0f && Time.time - lastHitTime >= decayDelay)
        {
            rage = Mathf.Max(0f, rage - decayPerSecond * Time.deltaTime);
        }

        UpdateVision();
    }

    private void SetBurning(bool value)
    {
        if (burning == value) return;

        burning = value;
        if (GameServices.Classes != null)
            GameServices.Classes.SetAbilityActive(burning);

        Debug.Log("RageSystem: burning " + (burning ? "started" : "ended"));
    }

    // ── Vision overlay ────────────────────────────────────────
    private void BuildOverlay()
    {
        GameObject canvasGO = new GameObject("RageVisionCanvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = overlaySortingOrder;

        GameObject imgGO = new GameObject("Overlay");
        imgGO.transform.SetParent(canvasGO.transform, false);

        overlay = imgGO.AddComponent<RawImage>();
        overlay.raycastTarget = false;
        overlay.texture = BuildVignetteTexture();
        overlay.enabled = false;

        overlayRect = overlay.rectTransform;
        overlayRect.anchorMin = new Vector2(0.5f, 0.5f);
        overlayRect.anchorMax = new Vector2(0.5f, 0.5f);
        overlayRect.pivot = new Vector2(0.5f, 0.5f);
        overlayRect.anchoredPosition = Vector2.zero;
    }

    private Texture2D BuildVignetteTexture()
    {
        const int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color32[] px = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(Mathf.InverseLerp(holeInner, holeOuter, r));
                px[y * size + x] = new Color32(0, 0, 0, (byte)(a * 255f));
            }
        }

        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    private void UpdateVision()
    {
        if (overlay == null) return;

        if (rage <= 0f)
        {
            overlay.enabled = false;
            return;
        }

        overlay.enabled = true;

        // Interpolate the inverse of the scale so the visible hole shrinks evenly.
        float inv = Mathf.Lerp(1f / clearScale, 1f / fullRageScale, RageNormalized);
        float scale = 1f / inv;

        overlayRect.sizeDelta = new Vector2(Screen.width, Screen.height) * scale;
    }
}