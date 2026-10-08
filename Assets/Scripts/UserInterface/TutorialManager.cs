using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public string title;
        [TextArea(3, 8)]
        public string message;
        public GameObject panel;
        public TMP_Text displayText;
        [Range(0f, 2f)]
        public float fadeInDuration = 0.4f;
        [Range(0f, 2f)]
        public float fadeOutDuration = 0.4f;
        [Tooltip("Overrides the global charDelay for this step. Set to 0 to use the global value.")]
        [Range(0f, 0.2f)]
        public float charDelayOverride = 0f;
        [Tooltip("These GameObjects will be SET ACTIVE when this step is shown")]
        public GameObject[] objectsToActivate;
        [Tooltip("These GameObjects will be SET INACTIVE when this step is shown")]
        public GameObject[] objectsToDeactivate;
    }

    [Header("Tutorial Steps")]
    public TutorialStep[] steps;

    [Header("Navigation Buttons")]
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;

    [Header("Scene Transition")]
    public Button sceneButton;
    public string sceneToLoad = "BattleTutorial";

    [Header("Step Indicator")]
    [Tooltip("Optional TMP_Text to display e.g. 'Step 2 of 5'")]
    public TMP_Text stepIndicatorText;
    [Tooltip("Format string — {0} = current step (1-based), {1} = total steps")]
    public string stepIndicatorFormat = "Step {0} of {1}";

    [Header("Typewriter Settings")]
    [Range(0.01f, 0.2f)]
    public float charDelay = 0.04f;

    [Header("On Close")]
    [Tooltip("These GameObjects will be SET INACTIVE when the tutorial is closed")]
    public GameObject[] objectsToDeactivateOnClose;

    [Header("Events")]
    public UnityEvent OnTutorialStarted;
    public UnityEvent OnTutorialCompleted;
    public UnityEvent<int> OnStepChanged;

    private int _currentStep = 0;
    private bool _typewriterFinished = false;
    private Coroutine _typeCoroutine;
    private Coroutine _fadeCoroutine;

    // ── lifecycle ──────────────────────────────────────────────────

    void Start()
    {
        if (nextButton != null) nextButton.onClick.AddListener(NextStep);
        if (prevButton != null) prevButton.onClick.AddListener(PrevStep);
        if (closeButton != null) closeButton.onClick.AddListener(CloseTutorial);
        if (sceneButton != null) sceneButton.onClick.AddListener(LoadNextScene);
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (prevButton != null) prevButton.gameObject.SetActive(false);
        if (closeButton != null) closeButton.gameObject.SetActive(false);
        if (sceneButton != null) sceneButton.gameObject.SetActive(false);

        foreach (var step in steps)
        {
            if (step.panel == null) continue;
            var cg = step.panel.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false; }
            else step.panel.SetActive(false);
        }
    }

    // ── public API ─────────────────────────────────────────────────

    public void StartTutorial()
    {
        if (steps == null || steps.Length == 0) return;

        _currentStep = 0;
        if (nextButton != null) nextButton.gameObject.SetActive(true);
        if (prevButton != null) prevButton.gameObject.SetActive(true);
        if (closeButton != null) closeButton.gameObject.SetActive(true);
        if (sceneButton != null) sceneButton.gameObject.SetActive(false);

        OnTutorialStarted?.Invoke();
        ShowStep(_currentStep);
    }

    public void GoToStep(int index)
    {
        if (steps == null || index < 0 || index >= steps.Length) return;
        HideStep(_currentStep, () =>
        {
            _currentStep = index;
            ShowStep(_currentStep);
        });
    }

    // ── navigation ─────────────────────────────────────────────────

    void NextStep()
    {
        if (!_typewriterFinished)
        {
            SkipTypewriter();
            return;
        }

        if (_currentStep < steps.Length - 1)
        {
            HideStep(_currentStep, () =>
            {
                _currentStep++;
                ShowStep(_currentStep);
            });
        }
        else
        {
            CloseTutorial();
            OnTutorialCompleted?.Invoke();
        }
    }

    void PrevStep()
    {
        if (_currentStep > 0)
        {
            HideStep(_currentStep, () =>
            {
                _currentStep--;
                ShowStep(_currentStep);
            });
        }
    }

    // ── typewriter skip ────────────────────────────────────────────

    void SkipTypewriter()
    {
        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);

        var step = steps[_currentStep];
        if (step.displayText != null) step.displayText.text = step.message;

        _typewriterFinished = true;
    }

    // ── step objects ───────────────────────────────────────────────

    void SetStepObjects(TutorialStep step)
    {
        foreach (var obj in step.objectsToDeactivate)
            if (obj != null) obj.SetActive(false);

        foreach (var obj in step.objectsToActivate)
            if (obj != null) obj.SetActive(true);
    }

    // ── step indicator ─────────────────────────────────────────────

    void UpdateStepIndicator(int index)
    {
        if (stepIndicatorText == null || steps == null) return;
        stepIndicatorText.text = string.Format(stepIndicatorFormat, index + 1, steps.Length);
    }

    // ── show / hide ────────────────────────────────────────────────

    void ShowStep(int index)
    {
        var step = steps[index];
        if (step.panel == null) return;

        _typewriterFinished = false;
        SetStepObjects(step);
        UpdateStepIndicator(index);
        OnStepChanged?.Invoke(index);

        if (prevButton != null) prevButton.gameObject.SetActive(index > 0);
        if (sceneButton != null) sceneButton.gameObject.SetActive(index == steps.Length - 1);
        if (nextButton != null) nextButton.gameObject.SetActive(index < steps.Length - 1);

        var cg = step.panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            step.panel.SetActive(true);
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadePanel(cg, 1f, step.fadeInDuration, () =>
            {
                if (step.displayText != null)
                {
                    if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
                    float delay = step.charDelayOverride > 0f ? step.charDelayOverride : charDelay;
                    _typeCoroutine = StartCoroutine(TypewriterRoutine(step.displayText, step.message, delay));
                }
            }));
        }
        else
        {
            step.panel.SetActive(true);
            if (step.displayText != null)
            {
                if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
                float delay = step.charDelayOverride > 0f ? step.charDelayOverride : charDelay;
                _typeCoroutine = StartCoroutine(TypewriterRoutine(step.displayText, step.message, delay));
            }
        }
    }

    void HideStep(int index, System.Action onComplete = null)
    {
        var step = steps[index];
        if (step.panel == null) { onComplete?.Invoke(); return; }

        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);

        var cg = step.panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadePanel(cg, 0f, step.fadeOutDuration, () =>
            {
                step.panel.SetActive(false);
                if (step.displayText != null) step.displayText.text = "";
                onComplete?.Invoke();
            }));
        }
        else
        {
            step.panel.SetActive(false);
            if (step.displayText != null) step.displayText.text = "";
            onComplete?.Invoke();
        }
    }

    public void CloseTutorial()
    {
        if (steps == null || steps.Length == 0) return;

        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        foreach (var step in steps)
        {
            if (step.panel == null) continue;

            var cg = step.panel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }

            step.panel.SetActive(false);
            if (step.displayText != null) step.displayText.text = "";
        }

        if (stepIndicatorText != null) stepIndicatorText.text = "";
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (prevButton != null) prevButton.gameObject.SetActive(false);
        if (closeButton != null) closeButton.gameObject.SetActive(false);
        if (sceneButton != null) sceneButton.gameObject.SetActive(false);

        foreach (var obj in objectsToDeactivateOnClose)
            if (obj != null) obj.SetActive(false);
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            CloseTutorial();
            OnTutorialCompleted?.Invoke();
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // ── coroutines ─────────────────────────────────────────────────

    IEnumerator FadePanel(CanvasGroup cg, float targetAlpha, float duration, System.Action onComplete = null)
    {
        cg.interactable = true;
        cg.blocksRaycasts = true;

        float startAlpha = cg.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        cg.alpha = targetAlpha;

        if (targetAlpha == 0f)
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        onComplete?.Invoke();
    }

    IEnumerator TypewriterRoutine(TMP_Text target, string fullText, float delay)
    {
        target.text = "";
        foreach (char c in fullText)
        {
            target.text += c;
            yield return new WaitForSeconds(delay);
        }
        _typewriterFinished = true;
    }
}