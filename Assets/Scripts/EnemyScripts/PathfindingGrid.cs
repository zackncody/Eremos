using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a 2D A* grid by scanning for colliders on the obstacle layer.
/// Place this on an empty GameObject in your scene (e.g. "Pathfinding").
/// The grid is rebuilt automatically at Start; call BuildGrid() manually to refresh.
/// </summary>
public class PathfindingGrid : MonoBehaviour
{
    public static PathfindingGrid Instance { get; private set; }

    [Header("Grid Settings")]
    [Tooltip("World-space centre of the grid.")]
    public Vector2 gridCenter = Vector2.zero;

    [Tooltip("Total size of the grid in world units (width x height).")]
    public Vector2 gridWorldSize = new Vector2(30f, 20f);

    [Tooltip("Size of each node in world units. Match your tile size (usually 1).")]
    public float nodeRadius = 0.5f;

    [Header("Obstacle Detection")]
    [Tooltip("Layers treated as impassable (walls, etc.).")]
    public LayerMask obstacleLayers;

    // ── Internal ───────────────────────────────────────────────────────────

    private Node[,] grid;
    private int gridSizeX, gridSizeY;
    private float nodeDiameter;

    public int MaxSize => gridSizeX * gridSizeY;

    // ── Unity ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildGrid();
    }



    // ── Grid Building ──────────────────────────────────────────────────────

    public void BuildGrid()
    {
        nodeDiameter = nodeRadius * 2f;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        grid = new Node[gridSizeX, gridSizeY];

        Vector2 bottomLeft = gridCenter - new Vector2(gridWorldSize.x / 2f, gridWorldSize.y / 2f);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = bottomLeft + new Vector2(
                    x * nodeDiameter + nodeRadius,
                    y * nodeDiameter + nodeRadius);

                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.9f, obstacleLayers);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public Node NodeFromWorldPoint(Vector2 worldPos)
    {
        float percentX = Mathf.Clamp01((worldPos.x - gridCenter.x + gridWorldSize.x / 2f) / gridWorldSize.x);
        float percentY = Mathf.Clamp01((worldPos.y - gridCenter.y + gridWorldSize.y / 2f) / gridWorldSize.y);
        int x = Mathf.Clamp(Mathf.FloorToInt(gridSizeX * percentX), 0, gridSizeX - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(gridSizeY * percentY), 0, gridSizeY - 1);
        return grid[x, y];
    }

    public List<Node> GetNeighbours(Node node)
    {
        var neighbours = new List<Node>();
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                // Prevent diagonal movement through wall corners
                if (Mathf.Abs(dx) == 1 && Mathf.Abs(dy) == 1)
                {
                    if (!grid[node.GridX + dx, node.GridY].Walkable) continue;
                    if (!grid[node.GridX, node.GridY + dy].Walkable) continue;
                }
                int nx = node.GridX + dx, ny = node.GridY + dy;
                if (nx >= 0 && nx < gridSizeX && ny >= 0 && ny < gridSizeY)
                    neighbours.Add(grid[nx, ny]);
            }
        return neighbours;
    }

    /// <summary>Finds a path from start to end using A*. Returns null if no path exists.</summary>
    public List<Vector2> FindPath(Vector2 startPos, Vector2 endPos)
    {
        Node startNode = NodeFromWorldPoint(startPos);
        Node targetNode = NodeFromWorldPoint(endPos);

        if (!startNode.Walkable || !targetNode.Walkable) return null;

        var openSet = new List<Node>();
        var closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        // Reset nodes
        foreach (var n in grid) { n.GCost = int.MaxValue; n.Parent = null; }

        startNode.GCost = 0;
        startNode.HCost = Heuristic(startNode, targetNode);

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
                if (openSet[i].FCost < current.FCost ||
                   (openSet[i].FCost == current.FCost && openSet[i].HCost < current.HCost))
                    current = openSet[i];

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbour in GetNeighbours(current))
            {
                if (!neighbour.Walkable || closedSet.Contains(neighbour)) continue;

                int moveCost = current.GCost + Heuristic(current, neighbour);
                if (moveCost < neighbour.GCost)
                {
                    neighbour.GCost = moveCost;
                    neighbour.HCost = Heuristic(neighbour, targetNode);
                    neighbour.Parent = current;
                    if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
                }
            }
        }
        return null; // no path
    }

    private List<Vector2> RetracePath(Node start, Node end)
    {
        var path = new List<Vector2>();
        Node current = end;
        while (current != start)
        {
            path.Add(current.WorldPosition);
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }

    private int Heuristic(Node a, Node b)
    {
        int dx = Mathf.Abs(a.GridX - b.GridX);
        int dy = Mathf.Abs(a.GridY - b.GridY);
        // Octile distance
        return 14 * Mathf.Min(dx, dy) + 10 * Mathf.Abs(dx - dy);
    }

    // ── Gizmos ─────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube((Vector3)(Vector2)gridCenter, (Vector3)(Vector2)gridWorldSize);

        if (grid == null) return;
        foreach (Node n in grid)
        {
            Gizmos.color = n.Walkable ? new Color(1, 1, 1, 0.05f) : new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube((Vector3)n.WorldPosition, Vector3.one * (nodeRadius * 2f - 0.05f));
        }
    }
}

// ── Node ───────────────────────────────────────────────────────────────────

public class Node
{
    public bool Walkable;
    public Vector2 WorldPosition;
    public int GridX, GridY;
    public int GCost, HCost;
    public Node Parent;

    public int FCost => GCost + HCost;

    public Node(bool walkable, Vector2 worldPos, int gridX, int gridY)
    {
        Walkable = walkable;
        WorldPosition = worldPos;
        GridX = gridX;
        GridY = gridY;
    }
}