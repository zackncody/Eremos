using System;
using System.Collections.Generic;

/// <summary>
/// The full save file contents. Core fields are strongly typed;
/// everything else (inventory, equipment, chests, quests, etc.)
/// is stored as raw JSON blobs keyed by ISaveable.SaveKey, so
/// SaveManager doesn't need to know their internal structure.
/// </summary>
[Serializable]
public class SaveData
{
    // ── Core Player State ──────────────────────────────────────
    public int health;
    public int maxHealth;
    public float posX;
    public float posY;
    public string sceneName;

    // ── Death Tracking ──────────────────────────────────────────
    public int totalDeaths;

    // ── Everything Else (inventory, equipment, chests, quests) ──
    // Key = ISaveable.SaveKey, Value = JSON string of that system's data
    public List<string> saveableKeys = new List<string>();
    public List<string> saveableBlobs = new List<string>();

    // ── Metadata ──────────────────────────────────────────────
    public string savedAtUtc;
}