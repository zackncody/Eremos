using UnityEngine;
using System.Collections.Generic;

public class InteractablePuzzleManager : MonoBehaviour
{
    public static InteractablePuzzleManager Instance { get; private set; }

    // Tracks every puzzle manager across all scenes
    private static readonly List<string> allPuzzleSaveKeys = new List<string>();

    [Header("Cave to reveal")]
    public GameObject caveObject;

    [Header("Required clicks")]
    public int totalRequired = 3;
    private int clickedCount = 0;
    private bool[] clicked;

    [Header("Save Key")]
    public string puzzleSaveKey = "CavePuzzleSolved";

    void Awake()
    {
        Instance = this;
        clicked = new bool[totalRequired];

        // Register this puzzle's key globally
        if (!allPuzzleSaveKeys.Contains(puzzleSaveKey))
            allPuzzleSaveKeys.Add(puzzleSaveKey);

        bool alreadySolved = PlayerPrefs.GetInt(puzzleSaveKey, 0) == 1;
        if (caveObject != null)
            caveObject.SetActive(alreadySolved);
        if (alreadySolved)
            clickedCount = totalRequired;
    }

    public void RegisterClick(int objectIndex)
    {
        if (clickedCount >= totalRequired) return;
        if (objectIndex < 0 || objectIndex >= totalRequired) return;
        if (clicked[objectIndex]) return;

        clicked[objectIndex] = true;
        clickedCount++;

        if (clickedCount >= totalRequired)
            RevealCave();
    }

    void RevealCave()
    {
        if (caveObject != null)
            caveObject.SetActive(true);
        PlayerPrefs.SetInt(puzzleSaveKey, 1);
        PlayerPrefs.Save();
    }

    // Called by the settings button — resets every puzzle ever registered this session
    public static void ResetAllPuzzles()
    {
        foreach (string key in allPuzzleSaveKeys)
            PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();

        if (Instance != null)
            Instance.ResetPuzzle();

        ClickToDisappear.ResetAll();

        Debug.Log($"All puzzles reset ({allPuzzleSaveKeys.Count} keys cleared).");
    }

    [ContextMenu("Reset Puzzle (Dev)")]
    public void ResetPuzzle()
    {
        PlayerPrefs.DeleteKey(puzzleSaveKey);
        PlayerPrefs.Save();
        clickedCount = 0;
        if (clicked != null)
        {
            for (int i = 0; i < clicked.Length; i++)
                clicked[i] = false;
        }
        if (caveObject != null)
            caveObject.SetActive(false);

        // Reset all pebble animations
        foreach (InteractablePuzzleTrigger trigger in FindObjectsOfType<InteractablePuzzleTrigger>())
            trigger.ResetState();

        Debug.Log($"Puzzle '{puzzleSaveKey}' reset.");
    }
}