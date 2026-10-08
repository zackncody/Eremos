using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Name of the scene to load when Play is pressed (must be added to Build Settings).")]
    [SerializeField] private string sceneToLoad = "MainScene";

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(sceneToLoad);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}