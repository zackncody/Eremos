using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Put on the Player (next to RageSystem). Finds your hand-made HUD rage bar by tag
// after each scene load, fills it, and hides it unless the required class is selected.
public class RageBarUI : MonoBehaviour
{
    [SerializeField] private string requiredClassID = "vessel";
    [SerializeField] private string barTag = "RageBar";

    private GameObject barRoot;
    private Slider slider;
    private TMP_Text rageText;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(RelinkNextFrame());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RelinkNextFrame());
    }

    private IEnumerator RelinkNextFrame()
    {
        yield return null; // wait one frame so scene objects finish initializing
        Relink();
    }

    private void Relink()
    {
        barRoot = GameObject.FindGameObjectWithTag(barTag);
        Debug.Log("RageBarUI relink. Found bar: " + (barRoot != null));

        if (barRoot == null)
        {
            slider = null;
            rageText = null;
            return;
        }

        slider = barRoot.GetComponentInChildren<Slider>(true);
        rageText = barRoot.GetComponentInChildren<TMP_Text>(true);
        UpdateBar();
    }

    private void Update()
    {
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (barRoot == null) return;

        RageSystem rage = GameServices.Rage;
        ClassManager classes = GameServices.Classes;

        bool show = rage != null
            && classes != null
            && classes.Current != null
            && classes.Current.classID == requiredClassID;

        if (barRoot.activeSelf != show) barRoot.SetActive(show);
        if (!show) return;

        if (slider != null)
        {
            slider.maxValue = rage.MaxRage;
            slider.value = rage.Rage;
        }

        if (rageText != null)
        {
            int percent = rage.MaxRage > 0f ? Mathf.RoundToInt(rage.Rage / rage.MaxRage * 100f) : 0;
            rageText.SetText($"{percent}%");
        }
    }
}