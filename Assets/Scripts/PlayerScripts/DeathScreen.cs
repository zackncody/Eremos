using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class DeathScreenUI : MonoBehaviour
{
    [Header("Core UI")]
    [SerializeField] private Button respawnButton;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private GameObject textBox;
    [SerializeField] private CanvasGroup textBoxCanvasGroup;
    [SerializeField] private GameObject beingSilhouette;
    [SerializeField] private CanvasGroup beingSilhouetteCanvasGroup;
    [SerializeField] private GameObject advanceClickCatcher;
    [SerializeField] private Image fadeOverlay;

    [Header("HUD (re-linked per scene)")]
    [SerializeField] private string hudTag = "HUD";
    private GameObject hudGroup;

    [Header("Dialogue")]
    [SerializeField] private AfterlifeDialogueDatabase dialogueDatabase;
    [SerializeField] private float typeSpeed = 0.03f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float silhouetteFadeDuration = 1.5f;

    [Header("Item Choice")]
    [SerializeField] private AfterlifeItemChoiceDatabase itemChoiceDatabase;
    [SerializeField] private GameObject itemChoicePanel;
    [SerializeField] private ItemChoiceButtonUI[] itemChoiceButtons;
    private InventoryManager inventoryManager;

    private string[] currentLines;
    private int lineIndex;
    private Coroutine typingCoroutine;
    private bool isTyping;

    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
        }

        if (respawnButton != null)
            respawnButton.onClick.AddListener(OnRespawnClicked);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RelinkHud();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RelinkHud();
    }

    private void RelinkHud()
    {
        GameObject found = GameObject.FindGameObjectWithTag(hudTag);
        hudGroup = found;

        inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager == null)
            Debug.LogWarning("DeathScreenUI: could not find InventoryManager in this scene.");
    }

    public void Show()
    {
        gameObject.SetActive(true);
        RelinkHud();
        BeginAfterlife();
    }

    private void BeginAfterlife()
    {
        if (hudGroup != null)
            hudGroup.SetActive(false);

        if (textBox != null)
            textBox.SetActive(false);

        if (beingSilhouette != null)
            beingSilhouette.SetActive(false);

        if (itemChoicePanel != null)
            itemChoicePanel.SetActive(false);

        int deaths = DeathManager.Instance.TotalDeaths;
        var set = dialogueDatabase.GetSetForDeathCount(deaths);
        currentLines = set.lines;
        lineIndex = 0;

        if (respawnButton != null)
            respawnButton.gameObject.SetActive(false);

        if (advanceClickCatcher != null)
            advanceClickCatcher.SetActive(false);

        deathText.text = "";

        if (fadeOverlay != null)
            StartCoroutine(FadeToBlackThenType());
        else
            StartCoroutine(RevealBeingAndTextBox());
    }

    private IEnumerator FadeToBlackThenType()
    {
        Color c = fadeOverlay.color;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            fadeOverlay.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeOverlay.color = c;

        yield return StartCoroutine(RevealBeingAndTextBox());
    }

    private IEnumerator RevealBeingAndTextBox()
    {
        if (beingSilhouette != null)
        {
            beingSilhouette.SetActive(true);
            if (beingSilhouetteCanvasGroup != null)
                yield return StartCoroutine(FadeCanvasGroup(beingSilhouetteCanvasGroup, 0f, 1f, silhouetteFadeDuration));
        }

        if (textBox != null)
        {
            textBox.SetActive(true);
            if (textBoxCanvasGroup != null)
                yield return StartCoroutine(FadeCanvasGroup(textBoxCanvasGroup, 0f, 1f, silhouetteFadeDuration));
        }

        if (advanceClickCatcher != null)
            advanceClickCatcher.SetActive(true);

        ShowCurrentLine();
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float t = 0f;
        group.alpha = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        group.alpha = to;
    }

    private void ShowCurrentLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentLines[lineIndex]));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        deathText.text = "";

        foreach (char c in line)
        {
            deathText.text += c;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        isTyping = false;
    }

    public void AdvanceDialogue()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            deathText.text = currentLines[lineIndex];
            isTyping = false;
            return;
        }

        lineIndex++;
        if (lineIndex >= currentLines.Length)
        {
            if (advanceClickCatcher != null)
                advanceClickCatcher.SetActive(false);

            TryShowItemChoiceOrRespawn();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    private void TryShowItemChoiceOrRespawn()
    {
        int deaths = DeathManager.Instance.TotalDeaths;
        AfterlifeItemChoiceSet choiceSet = itemChoiceDatabase != null
            ? itemChoiceDatabase.GetSetForDeathCount(deaths)
            : null;

        if (choiceSet != null && choiceSet.choices != null && choiceSet.choices.Length > 0)
            ShowItemChoices(choiceSet);
        else
            ShowRespawnButton();
    }

    private void ShowItemChoices(AfterlifeItemChoiceSet set)
    {
        if (itemChoicePanel != null)
            itemChoicePanel.SetActive(true);

        for (int i = 0; i < itemChoiceButtons.Length; i++)
        {
            if (i < set.choices.Length)
            {
                ItemData choice = set.choices[i];

                itemChoiceButtons[i].root.SetActive(true);
                if (itemChoiceButtons[i].label != null)
                    itemChoiceButtons[i].label.text = choice.itemName;
                if (itemChoiceButtons[i].icon != null)
                    itemChoiceButtons[i].icon.sprite = choice.icon;

                itemChoiceButtons[i].button.onClick.RemoveAllListeners();
                itemChoiceButtons[i].button.onClick.AddListener(() => OnItemChosen(choice));
            }
            else
            {
                itemChoiceButtons[i].root.SetActive(false);
            }
        }
    }

    private void OnItemChosen(ItemData chosen)
    {
        Debug.Log("OnItemChosen called. chosen=" + (chosen != null ? chosen.itemName : "NULL") +
                   " inventoryManager=" + (inventoryManager != null ? "found" : "NULL"));

        if (inventoryManager != null)
            inventoryManager.AddItemDataToInventory(chosen);

        if (itemChoicePanel != null)
            itemChoicePanel.SetActive(false);

        ShowRespawnButton();
    }

    private void ShowRespawnButton()
    {
        if (respawnButton != null)
            respawnButton.gameObject.SetActive(true);
    }

    private void OnRespawnClicked()
    {
        Time.timeScale = 1f;

        if (hudGroup != null)
            hudGroup.SetActive(true);

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.Respawn();

        gameObject.SetActive(false);
    }
}

[System.Serializable]
public class ItemChoiceButtonUI
{
    public GameObject root;
    public Button button;
    public Image icon;
    public TextMeshProUGUI label;
}