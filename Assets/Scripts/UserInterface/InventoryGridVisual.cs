using UnityEngine;
using UnityEngine.UI;

public class InventoryGridVisual : MonoBehaviour
{
    [SerializeField] private InventoryGrid grid;
    [SerializeField] private Sprite cellSprite;
    [SerializeField] private Color cellColor = new Color(1f, 1f, 1f, 0.1f);
    [SerializeField] private Color cellOutlineColor = new Color(1f, 1f, 1f, 0.4f);

    private void Start()
    {
        DrawGrid();
    }

    private void DrawGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        for (int y = 0; y < grid.rows; y++)
        {
            for (int x = 0; x < grid.columns; x++)
            {
                GameObject cell = new GameObject($"Cell_{x}_{y}", typeof(RectTransform));
                cell.transform.SetParent(transform, false);

                Image bg = cell.AddComponent<Image>();
                bg.color = cellColor;
                bg.raycastTarget = false;
                if (cellSprite != null) bg.sprite = cellSprite;

                RectTransform rt = cell.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.sizeDelta = new Vector2(grid.cellSize - 1, grid.cellSize - 1);
                rt.anchoredPosition = new Vector2(
                    x * grid.cellSize,
                    -y * grid.cellSize
                );
            }
        }
    }

    public void RefreshGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        DrawGrid();
    }
}