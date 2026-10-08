using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public TutorialManager tutorialManager;

    private Camera _mainCam;

    void Start()
    {
        _mainCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryClick(Input.mousePosition);

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            TryClick(Input.GetTouch(0).position);
    }

    void TryClick(Vector2 screenPos)
    {
        if (_mainCam == null)
        {
            Debug.LogError("No Main Camera found! Make sure your camera is tagged 'Main Camera'");
            return;
        }

        Vector2 worldPos = _mainCam.ScreenToWorldPoint(screenPos);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        Debug.Log($"Click at world pos: {worldPos}, hit: {(hit != null ? hit.name : "nothing")}");

        if (hit != null && hit.transform == transform)
            tutorialManager.StartTutorial();
    }
}