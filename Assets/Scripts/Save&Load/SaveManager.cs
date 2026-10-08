using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central save/load controller. Persistent singleton.
/// Core player stats (health, position, scene) are handled directly.
/// Everything else (inventory, equipment, chests, quests, etc.) goes
/// through ISaveable systems, discovered dynamically at save/load time
/// rather than relying on push-registration — this avoids a class of
/// bugs where Awake()/OnEnable() order between singletons is not
/// guaranteed, which can otherwise cause a saveable to silently never
/// register in time.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // SpawnPoint checks this to skip its own repositioning logic
    // while a save file's position is being applied.
    public static bool IsLoadingSave { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedAutosave;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedAutosave;
    }

    private void OnSceneLoadedAutosave(Scene scene, LoadSceneMode mode)
    {
        if (IsLoadingSave) return;
        StartCoroutine(WaitForRestoreThenSave());
    }

    private IEnumerator WaitForRestoreThenSave()
    {
        Debug.Log($"WaitForRestoreThenSave started — IsRestoring: {InventoryLoader.IsRestoring}");
        while (InventoryLoader.IsRestoring)
        {
            Debug.Log("Waiting for restore...");
            yield return null;
        }
        Debug.Log("Restore done, saving now");
        yield return null;
        SaveGame();
    }

    // ── Discovery ─────────────────────────────────────────────
    // Finds every active ISaveable in the game right now — including
    // DontDestroyOnLoad objects like InventoryPersistence, and any
    // scene-local saveables in whatever scene is currently loaded.

    private List<ISaveable> FindAllSaveables()
    {
        var result = new List<ISaveable>();
        MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var behaviour in allBehaviours)
        {
            if (behaviour is ISaveable saveable)
                result.Add(saveable);
        }

        return result;
    }

    // ── Save ──────────────────────────────────────────────────

    public void SaveGame()
    {
        // Force InventoryPersistence to sync with the current live
        // InventoryManager/HotbarManager/EquipmentManager state before
        // capturing it — otherwise it may still hold stale data from
        // whenever it was last synced (e.g. the last respawn).
        InventoryLoader.Save();

        SaveData data = new SaveData();

        PlayerCode player = GameServices.Player;
        if (player != null)
        {
            data.posX = player.transform.position.x;
            data.posY = player.transform.position.y;

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                data.health = health.health;
                data.maxHealth = health.maxHealth;
            }
        }

        data.sceneName = SceneManager.GetActiveScene().name;

        if (GameServices.Death != null)
            data.totalDeaths = GameServices.Death.TotalDeaths;

        List<ISaveable> saveables = FindAllSaveables();
        foreach (var saveable in saveables)
        {
            object state = saveable.CaptureState();
            string json = JsonUtility.ToJson(state);
            data.saveableKeys.Add(saveable.SaveKey);
            data.saveableBlobs.Add(json);
        }

        data.savedAtUtc = System.DateTime.UtcNow.ToString("o");

        string fullJson = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, fullJson);

        Debug.Log($"SaveManager: game saved to {SavePath} ({saveables.Count} saveable system(s) captured: " +
                  string.Join(", ", data.saveableKeys) + ")");
    }

    // ── Load ──────────────────────────────────────────────────

    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    public void LoadGame()
    {
        if (!HasSaveFile())
        {
            Debug.LogWarning("SaveManager: no save file found at " + SavePath);
            return;
        }

        string fullJson = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(fullJson);

        // Load the correct scene first — ApplyLoadedData runs after the
        // scene finishes loading, once the player/systems exist again.
        pendingLoadData = data;
        IsLoadingSave = true;
        SceneManager.sceneLoaded += OnSceneLoadedForRestore;
        SceneManager.LoadScene(data.sceneName);
    }

    private SaveData pendingLoadData;

    private void OnSceneLoadedForRestore(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedForRestore;
        ApplyLoadedData(pendingLoadData);
        pendingLoadData = null;
        StartCoroutine(ClearLoadingFlagNextFrame());
    }

    private IEnumerator ClearLoadingFlagNextFrame()
    {
        // Stay true through this frame's Start() calls (e.g. SpawnPoint.Start())
        // so they know to skip repositioning, then clear for future scene loads.
        yield return null;
        IsLoadingSave = false;
    }

    private void ApplyLoadedData(SaveData data)
    {
        PlayerCode player = GameServices.Player;
        if (player != null)
        {
            player.transform.position = new Vector3(data.posX, data.posY, player.transform.position.z);

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.health = data.health;
                health.maxHealth = data.maxHealth;
            }
        }

        // totalDeaths lives in DeathManager's own PlayerPrefs-backed counter;
        // only touch it here if you want save files to override that count.

        Dictionary<string, ISaveable> saveableLookup = new Dictionary<string, ISaveable>();
        foreach (var saveable in FindAllSaveables())
            saveableLookup[saveable.SaveKey] = saveable;

        for (int i = 0; i < data.saveableKeys.Count; i++)
        {
            string key = data.saveableKeys[i];
            string blob = data.saveableBlobs[i];

            if (saveableLookup.TryGetValue(key, out ISaveable saveable))
                saveable.RestoreState(blob);
            else
                Debug.LogWarning("SaveManager: no ISaveable found for key '" + key + "' — data not restored.");
        }

        Debug.Log("SaveManager: game loaded from " + SavePath);
    }

    // ── Delete ────────────────────────────────────────────────

    public void DeleteSave()
    {
        if (HasSaveFile())
        {
            File.Delete(SavePath);
            Debug.Log("SaveManager: save file deleted.");
        }
    }
}