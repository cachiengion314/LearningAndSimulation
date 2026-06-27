using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TerrainGenerator : MonoBehaviour
{
  [Header("Elements")]
  [SerializeField] MeshFilter filter;

  [Header("Brush Settings")]
  [SerializeField] int brushRadius;
  [SerializeField] float brushStrength;
  [SerializeField] float brushFallback;

  [Header("Data")]
  [SerializeField] int gridSize;
  [Range(0, 5)]
  [SerializeField] float gridScale;
  [SerializeField] float isoValue;
  SquareGrid squareGrid;

  Mesh mesh;
  List<Vector3> vertices = new();
  List<int> triangles = new();

  float[,] grid;

  private void Awake()
  {
    InputManager.onTouching += TouchingCallback;
  }

  void OnDestroy()
  {
    InputManager.onTouching -= TouchingCallback;
  }

  void Start()
  {
    grid = new float[gridSize, gridSize];

    for (int y = 0; y < gridSize; ++y)
      for (int x = 0; x < gridSize; ++x)
        grid[x, y] = isoValue + .1f;

    squareGrid = new SquareGrid();
    squareGrid.InitSquareGrid(gridSize - 1, gridScale, isoValue);
    GenerateMesh();
  }

  void TouchingCallback(Vector3 worldPos)
  {
    worldPos.z = 0;
    worldPos = transform.InverseTransformPoint(worldPos);
    var gridPos = GetGridPosFromWorldPos(worldPos);

    bool shouldGenerate = false;

    for (int y = gridPos.y - brushRadius; y < gridPos.y + brushRadius; ++y)
    {
      for (int x = gridPos.x - brushRadius; x < gridPos.x + brushRadius; ++x)
      {
        var currentGridPos = new Vector2Int(x, y);
        if (!IsValidGridPos(currentGridPos))
        {
          // Invalid grid position!
          continue;
        }

        var distance = Vector2.Distance(currentGridPos, gridPos);
        var factor = brushStrength * Mathf.Exp(-distance * brushFallback / brushRadius);

        grid[currentGridPos.x, currentGridPos.y] -= factor;
        shouldGenerate = true;
      }
    }

    if (shouldGenerate)
      GenerateMesh();
  }

  void GenerateMesh()
  {
    vertices.Clear();
    triangles.Clear();

    squareGrid.Update(grid);

    mesh = new Mesh
    {
      vertices = squareGrid.GetVertices(),
      triangles = squareGrid.GetTriangles(),
      uv = squareGrid.GetUVs(),
    };
    filter.mesh = mesh;

    GenerateCollider();
  }

  void GenerateCollider()
  {
    if (filter.TryGetComponent(out MeshCollider meshCollider))
      meshCollider.sharedMesh = mesh;
    else
      filter.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
  }

  bool IsValidGridPos(Vector2Int gridPos)
  {
    return gridPos.x >= 0 && gridPos.x < gridSize && gridPos.y >= 0 && gridPos.y < gridSize;
  }

  Vector2 GetWorldPosFromGridPos(int x, int y)
  {
    var worldPos = new Vector2(x, y) * gridScale;
    worldPos.x -= gridSize * gridScale / 2 - gridScale / 2;
    worldPos.y -= gridSize * gridScale / 2 - gridScale / 2;
    return worldPos;
  }

  Vector2Int GetGridPosFromWorldPos(Vector2 worldPos)
  {
    var gridPos = new Vector2Int
    {
      x = Mathf.FloorToInt(worldPos.x / gridScale + gridSize / 2 - gridScale / 2),
      y = Mathf.FloorToInt(worldPos.y / gridScale + gridSize / 2 - gridScale / 2)
    };
    return gridPos;
  }

#if UNITY_EDITOR
  void OnDrawGizmos()
  {
    if (!EditorApplication.isPlaying) return;

    Gizmos.color = Color.red;
    for (int y = 0; y < grid.GetLength(1); ++y)
    {
      for (int x = 0; x < grid.GetLength(0); ++x)
      {
        var worldPos = GetWorldPosFromGridPos(x, y);
        Gizmos.DrawSphere(worldPos, gridScale / 4f);
        Handles.Label(worldPos + Vector2.up * gridScale / 3f, grid[x, y].ToString());
      }
    }
  }
#endif
}
