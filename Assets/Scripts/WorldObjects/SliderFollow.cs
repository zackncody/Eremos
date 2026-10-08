using UnityEngine;
using UnityEngine.UI;

public class SliderFollow : MonoBehaviour
{
    [Header("References")]
    private Transform target;
    public Slider slider;
    public Vector3 offset = new Vector3(0, 3f, 0);
    private Camera mainCamera;
    private Canvas canvas;

    void Awake()
    {
        // Find Franklin automatically
        GameObject franklin = GameObject.FindWithTag("Franklin");
        if (franklin != null)
        {
            target = franklin.transform;
            Debug.Log("Franklin found at: " + franklin.transform.position);
        }
        else
        {
            Debug.LogError("SliderFollow: Franklin not found! Is the tag set?");
            enabled = false;
            return;
        }

        if (slider == null)
        {
            Debug.LogError("SliderFollow: Slider is not assigned.");
            enabled = false;
            return;
        }

        mainCamera = Camera.main;
        canvas = slider.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("SliderFollow: Slider must be inside a Canvas.");
            enabled = false;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position + offset);

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            slider.transform.position = screenPos;
        }
        else
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceCamera ? mainCamera : null,
                out localPos
            );
            slider.transform.localPosition = localPos;
        }
    }
}