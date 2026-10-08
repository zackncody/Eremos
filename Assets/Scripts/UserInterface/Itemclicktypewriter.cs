using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Attach to any 2D clickable GameObject (requires a Collider2D) OR a UI Button.
/// When clicked, a UI panel fades in, text types out, and any assigned
/// GameObjects are activated/deactivated.
/// </summary>
public class ItemClickTypewriter2D : MonoBehaviour
{
    [Header("Tutorial (optional)")]
    [Tooltip("Assign if clicking this item should start a tutorial")]
    public TutorialManager tutorialManager;

    [Header("Trigger Button (optional)")]
    [Tooltip("A UI button that triggers this — use if the script is on a separate GameObject")]
    public Button triggerButton;

    [Header("UI References")]
    [Tooltip("The panel/window GameObject to show when clicked")]
    public CanvasGroup panel;

    [Tooltip("TMP_Text inside the panel for the typewriter effect")]
    public TMP_Text displayText;

    [Tooltip("Optional close button to hide the panel")]
    public Button closeButton;

    [Header("Objects To Activate On Click")]
    [Tooltip("These GameObjects will be SET ACTIVE when the item is clicked")]
    public GameObject[] objectsToActivate;

    [Tooltip("These GameObjects will be SET INACTIVE when the item is clicked")]
    public GameObject[] objectsToDeactivate;

    [Tooltip("Re-apply activation/deactivation when the panel is closed")]
    public bool resetObjectsOnClose = false;

    [Header("Message")]
    [TextArea(3, 8)]
    public string message = "This world is not what it seems. Beneath its surface lies something ancient...";

    [Header("Typewriter Settings")]
    [Range(0.01f, 0.2f)]
    public float charDelay = 0.04f;

    [Header("Panel Fade Settings")]
    [Range(0.1f, 2f)]
    [Tooltip("How long the panel takes to fade in/out")]
    public float fadeDuration = 0.4f;

    // ── private state ──────────────────────────────────────────────
    private Coroutine _typeCoroutine;
    private Coroutine _fadeCoroutine;
    private Camera _mainCam;
    private bool _isUIButton;

    void Start()
    {
        if (triggerButton != null)
            triggerButton.onClick.AddListener(OnItemClicked);
        _mainCam = Camera.main;

        // If a Button component is on this GameObject, wire it up automatically
        Button selfButton = GetComponent<Button>();
        if (selfButton != null)
        {
            _isUIButton = true;
            selfButton.onClick.AddListener(OnItemClicked);
        }

        if (panel != null)
        {
            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }

        if (displayText != null)
            displayText.text = "";

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    // ── input detection ────────────────────────────────────────────

    void Update()
    {
        // Skip physics-based click detection if this is a UI Button —
        // the Button's onClick listener handles it instead.
        if (_isUIButton) return;

        if (Input.GetMouseButtonDown(0))
            TryHandleClick(Input.mousePosition);

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            TryHandleClick(Input.GetTouch(0).position);
    }

    void TryHandleClick(Vector2 screenPos)
    {
        // Ignore clicks that land on a UI element to avoid double-firing
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 worldPos = _mainCam.ScreenToWorldPoint(screenPos);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit != null && hit.transform == transform)
            OnItemClicked();
    }

    // ── main logic ─────────────────────────────────────────────────

    void OnItemClicked()
    {
        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        SetObjects(true);

        // Start tutorial if assigned
        if (tutorialManager != null)
        {
            tutorialManager.StartTutorial();
            return; // skip the panel/typewriter logic on this script
        }

        if (panel != null)
        {
            _fadeCoroutine = StartCoroutine(FadePanel(1f, () =>
            {
                if (displayText != null)
                    _typeCoroutine = StartCoroutine(TypewriterRoutine(message));
            }));
        }
        else if (displayText != null)
        {
            _typeCoroutine = StartCoroutine(TypewriterRoutine(message));
        }
    }

    void SetObjects(bool clicked)
    {
        if (clicked)
        {
            foreach (var obj in objectsToDeactivate)
            {
                if (obj != null)
                {
                    Debug.Log($"Deactivating: {obj.name}");
                    obj.SetActive(false);
                }
            }

            foreach (var obj in objectsToActivate)
            {
                if (obj != null)
                {
                    Debug.Log($"Activating: {obj.name}");
                    obj.SetActive(true);
                }
            }
        }
        else if (resetObjectsOnClose)
        {
            foreach (var obj in objectsToActivate)
                if (obj != null) obj.SetActive(false);

            foreach (var obj in objectsToDeactivate)
                if (obj != null) obj.SetActive(true);
        }
    }

    // ── coroutines ─────────────────────────────────────────────────

    IEnumerator FadePanel(float targetAlpha, System.Action onComplete = null)
    {
        panel.interactable = true;
        panel.blocksRaycasts = true;

        float startAlpha = panel.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            panel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        panel.alpha = targetAlpha;
        onComplete?.Invoke();
    }

    IEnumerator TypewriterRoutine(string fullText)
    {
        displayText.text = "";

        foreach (char c in fullText)
        {
            displayText.text += c;
            yield return new WaitForSeconds(charDelay);
        }
    }

    // ── close panel ────────────────────────────────────────────────

    public void ClosePanel()
    {
        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        SetObjects(false);

        if (panel != null)
        {
            _fadeCoroutine = StartCoroutine(FadePanel(0f, () =>
            {
                panel.interactable = false;
                panel.blocksRaycasts = false;
                if (displayText != null) displayText.text = "";
            }));
        }
    }

    // ── public helpers ─────────────────────────────────────────────

    public void ShowMessage(string customMessage)
    {
        message = customMessage;
        OnItemClicked();
    }
}