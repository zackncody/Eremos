using UnityEngine;

public class Title : MonoBehaviour
{
    [SerializeField] private GameObject Titletext;
    [SerializeField] private GameObject scrollTitle;

    private void Awake()
    {
        Titletext.SetActive(false);
        scrollTitle.SetActive(false);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        Titletext.SetActive(true);
        scrollTitle.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        Titletext.SetActive(false);
        scrollTitle.SetActive(false);

        
    }
}