using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance { get; private set; }
    public int TotalDeaths { get; private set; }
    private const string DeathCountKey = "TotalDeaths";

    [Header("Death Screen")]
    [SerializeField] private DeathScreenUI deathScreenUIPrefab;
    public DeathScreenUI ScreenUI { get; private set; }

    void Awake()
    {
        GameServices.RegisterDeath(this);
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        TotalDeaths = PlayerPrefs.GetInt(DeathCountKey, 0);

        if (deathScreenUIPrefab != null)
        {
            ScreenUI = Instantiate(deathScreenUIPrefab);
            ScreenUI.transform.SetParent(null);
            DontDestroyOnLoad(ScreenUI.gameObject);
            ScreenUI.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("DeathManager: deathScreenUIPrefab not assigned in Inspector.");
        }
    }

    public void RegisterDeath()
    {
        TotalDeaths++;
        PlayerPrefs.SetInt(DeathCountKey, TotalDeaths);
        PlayerPrefs.Save();

        if (ScreenUI != null)
            ScreenUI.Show();
        else
            Debug.LogWarning("DeathManager: no DeathScreenUI instance to show.");

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
    }

    [Header("Debug / Admin")]
    [SerializeField] private int setDeathCountTo = 0;

    [ContextMenu("Admin: Set Death Count To Field Above")]
    private void AdminSetDeathCount()
    {
        TotalDeaths = setDeathCountTo;
        PlayerPrefs.SetInt(DeathCountKey, TotalDeaths);
        PlayerPrefs.Save();
        Debug.Log("DeathManager: TotalDeaths set to " + TotalDeaths);
    }

    [ContextMenu("Admin: Reset Death Count To 0")]
    private void AdminResetDeathCount()
    {
        TotalDeaths = 0;
        PlayerPrefs.SetInt(DeathCountKey, 0);
        PlayerPrefs.Save();
        Debug.Log("DeathManager: TotalDeaths reset to 0");
    }

    [ContextMenu("Admin: Log Current Death Count")]
    private void AdminLogDeathCount()
    {
        Debug.Log("DeathManager: TotalDeaths = " + TotalDeaths);
    }
}