
using UnityEngine;

public class ShadowTrigger : MonoBehaviour
{

    [SerializeField] private GameObject Shadow;
    [SerializeField] private GameObject GlobalShadow;
    [SerializeField] private GameObject HiderShadow;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        Shadow.SetActive(true);
        GlobalShadow.SetActive(false);
        HiderShadow.SetActive(false);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        Shadow.SetActive(false);
        GlobalShadow.SetActive(true);
        HiderShadow.SetActive(true);
    }
}