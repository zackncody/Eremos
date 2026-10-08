using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ZoneTrigger : MonoBehaviour
{
    public string targetScene;
    public string spawnPointID;
    public CanvasGroup fadePanel;
    public ZoneData zoneData;

    public enum ActiveTime { Day, Night, Both }
    [Header("Day/Night Availability")]
    public ActiveTime activeDuring = ActiveTime.Both;
    public DayNightCycle dayNightCycle;

    void Update()
    {
        if (activeDuring == ActiveTime.Both || dayNightCycle == null)
            return;

        bool shouldBeActive = activeDuring == ActiveTime.Day ? dayNightCycle.IsDay : !dayNightCycle.IsDay;

        if (gameObject.activeSelf != shouldBeActive)
            gameObject.SetActive(shouldBeActive);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger hit by: " + other.name + " tag: " + other.tag);
        if (other.CompareTag("Player"))
            StartCoroutine(TransitionToZone());
    }

    private IEnumerator TransitionToZone()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            fadePanel.alpha = t;
            yield return null;
        }
        InventoryLoader.Save();
        HotbarManager hotbar = FindAnyObjectByType<HotbarManager>();
        InventoryManager inventory = FindAnyObjectByType<InventoryManager>();
        if (GameManager.Instance != null && hotbar != null && inventory != null)
            GameManager.Instance.SaveState(hotbar, inventory);
        PlayerPrefs.SetString("SpawnPoint", spawnPointID);
        SceneManager.LoadScene(targetScene);
    }
}