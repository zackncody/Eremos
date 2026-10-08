using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObeliskUI : MonoBehaviour
{
    public static ObeliskUI Instance;

    [Header("References")]
    public CanvasGroup panelGroup;
    public TMP_Text tipTextDisplay;

    [Header("Fade")]
    public float fadeSpeed = 5f;

    private bool isShowing = false;

    private void Awake()
    {
        Instance = this;
        panelGroup.alpha = 0f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
    }

    public void ShowTip(string text)
    {
        tipTextDisplay.text = text;
        isShowing = true;
        panelGroup.blocksRaycasts = true;
    }

    public void HideTip()
    {
        isShowing = false;
        panelGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        float target = isShowing ? 1f : 0f;
        panelGroup.alpha = Mathf.MoveTowards(panelGroup.alpha, target, fadeSpeed * Time.deltaTime);

        // let them close it manually too
        if (isShowing && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)))
        {
            HideTip();
        }
    }
}