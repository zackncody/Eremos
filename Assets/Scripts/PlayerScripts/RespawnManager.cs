using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [Header("Default Spawn")]
    [SerializeField] private string defaultSpawnID = "entrance";

    private string currentSpawnID;
    private string currentScene;

    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentSpawnID = defaultSpawnID;
        currentScene = SceneManager.GetActiveScene().name;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;

        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth == null)
            Debug.LogWarning("RespawnManager: No PlayerHealth found in scene!");

        DeathScreenUI deathUI = FindAnyObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        if (deathUI == null)
            Debug.LogWarning("RespawnManager: No DeathScreenUI found in scene!");
        else
            deathUI.gameObject.SetActive(false);
    }

    public void SetSpawnPoint(string spawnID)
    {
        currentSpawnID = spawnID;
        currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"Spawn point updated: {spawnID} in {currentScene}");
    }

    public void OnPlayerDied()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        DeathScreenUI[] allDeathScreens = Resources.FindObjectsOfTypeAll<DeathScreenUI>();

        foreach (DeathScreenUI deathUI in allDeathScreens)
        {
            if (deathUI.gameObject.scene.isLoaded)
            {
                deathUI.gameObject.SetActive(true);
            }
        }
    }

    public void Respawn()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        InventoryLoader.Save();
        PlayerPrefs.SetString("SpawnPoint", currentSpawnID);
        PlayerPrefs.Save();

        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.Respawn();
        else
            Debug.LogWarning("RespawnManager: No PlayerHealth found to reset on respawn!");

        SceneManager.LoadScene(currentScene);
    }
}