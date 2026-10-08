using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PickupNotificationUI : MonoBehaviour
{
    public static PickupNotificationUI Instance;

    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public Image iconImage;
    public TextMeshProUGUI itemNameText;

    [Header("Timing")]
    public float fadeInTime = 0.15f;
    public float holdTime = 1.2f;
    public float fadeOutTime = 0.3f;

    private struct PickupData
    {
        public Sprite icon;
        public string name;
        public int quantity;
    }

    private Queue<PickupData> queue = new Queue<PickupData>();
    private bool isShowing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        canvasGroup.alpha = 0f;
    }

   
    public void ShowPickup(Sprite icon, string itemName, int quantity = 1)
    {
        queue.Enqueue(new PickupData { icon = icon, name = itemName, quantity = quantity });
        if (!isShowing)
            StartCoroutine(ProcessQueue());
    }


    public void ShowPickup(ItemData item, int quantity = 1)
    {
        ShowPickup(item.icon, item.itemName, quantity);
    }

    private string FormatDisplayName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;
        string[] words = raw.Split('_');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
        }
        return string.Join(" ", words);
    }

    private IEnumerator ProcessQueue()
    {
        isShowing = true;

        while (queue.Count > 0)
        {
            PickupData data = queue.Dequeue();

            iconImage.sprite = data.icon;
            string displayName = FormatDisplayName(data.name);
            itemNameText.text = data.quantity > 1 ? $"{displayName} x{data.quantity}" : displayName;

            yield return StartCoroutine(Fade(0f, 1f, fadeInTime));
            yield return new WaitForSecondsRealtime(holdTime);
            yield return StartCoroutine(Fade(1f, 0f, fadeOutTime));
        }

        isShowing = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}