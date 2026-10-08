using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class DayNightVisibility : MonoBehaviour
{
    [SerializeField] private Light2D sunLight;
    private string mainSceneName = "GameScene";

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
        if (sunLight != null)
            sunLight.enabled = scene.name == mainSceneName;
    }
}