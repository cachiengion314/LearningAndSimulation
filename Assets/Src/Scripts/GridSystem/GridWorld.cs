using HoangNam;
using UnityEngine;
using Unity.Mathematics;

public partial class GridWorld : MonoBehaviour
{
  [Header("General Setting")]
  [SerializeField] bool shouldDrawMap;
  [Range(0, 10)]
  [SerializeField] int colorIndex;
  public int ColorIndex { get { return colorIndex; } }
  float3x3 _originalMatrix;
  public float3x3 OriginalMatrix { get { return _originalMatrix; } }
  float3x3 rotatedMatrix;
  public float3x3 RotatedMatrix { get { return rotatedMatrix; } }
  public float3 Rotation;
  public float3x3 Scale = float3x3.identity;
  public int2 GridResolution = 5;
  public float2 GridScale = 1;
  [Range(.0f, 1.0f)]
  [SerializeField] float gizmosQuadScale;
  [Range(.0f, 1.0f)]
  [SerializeField] float gizmosQuadAlpha;

  public float3 MapGridToWorld(int2 gridPos)
  {
    return MapGridToWorld(gridPos, GridResolution, GridScale, (float3)transform.position, rotatedMatrix);
  }

  public int2 MapWorldToGrid(float3 worldPos)
  {
    return MapWorldToGrid(worldPos, GridResolution, GridScale, (float3)transform.position, rotatedMatrix);
  }

  public int MapGridToIndex(int2 gridPos)
  {
    return MapGridToIndex(gridPos, GridResolution);
  }

  public int2 MapIndexToGrid(int index)
  {
    return MapIndexToGrid(index, GridResolution);
  }

  public float3 MapIndexToWorld(int index)
  {
    return MapIndexToWorld(index, GridResolution, GridScale, (float3)transform.position, rotatedMatrix);
  }

  public int MapWorldToIndex(float3 worldPos)
  {
    return MapWorldToIndex(worldPos, GridResolution, GridScale, (float3)transform.position, rotatedMatrix);
  }

  static float3 CalculateOrigin(
    int2 gridResolution,
    float2 gridScale,
    float3 coordinatePos
  )
  {
    var x = gridResolution.x * gridScale.x;
    var y = gridResolution.y * gridScale.y;
    var leftDir = new float3(-x / 2f, 0, 0);
    var bottomDir = new float3(0, -y / 2f, 0);

    var bottomLeftDir = bottomDir + leftDir;
    var bottomLeftPos = coordinatePos + bottomLeftDir;
    var centerBottomLeftPos = bottomLeftPos + new float3(gridScale.x / 2f, gridScale.y / 2f, 0);
    return new float3(centerBottomLeftPos.x, centerBottomLeftPos.y, 0);
  }

  static public float3 MapGridToWorld(
    int2 gridPos,
    int2 gridResolution,
    float2 gridScale,
    float3 coordinatePos,
    float3x3 rotatedMatrix
  )
  {
    var r = new float3(gridPos.x * gridScale.x, gridPos.y * gridScale.y, 0f);
    var o = CalculateOrigin(gridResolution, gridScale, float3.zero);
    return math.mul(o + r, rotatedMatrix) + coordinatePos;
  }

  static public int2 MapWorldToGrid(
    float3 worldPos,
    int2 gridResolution,
    float2 gridScale,
    float3 coordinatePos,
    float3x3 rotatedMatrix
  )
  {
    var localPos = math.mul(worldPos - coordinatePos, math.inverse(rotatedMatrix));
    var o = CalculateOrigin(gridResolution, gridScale, float3.zero);
    var local = localPos - o;
    return new int2(
      (int)math.round(local.x / gridScale.x),
      (int)math.round(local.y / gridScale.y)
    );
  }

  static public int MapGridToIndex(
    int2 gridPos,
    int2 gridResolution
  )
  {
    var index = gridResolution.y * gridPos.x + gridPos.y;
    return index;
  }

  static public int2 MapIndexToGrid(
    int index,
    int2 gridResolution
  )
  {
    int x = (int)(uint)math.floor(index / gridResolution.y);
    int y = index - (x * gridResolution.y);
    return new int2(x, y);
  }

  static public float3 MapIndexToWorld(
    int index,
    int2 gridResolution,
    float2 gridScale,
    float3 coordinatePos,
    float3x3 rotatedMatrix
  )
  {
    var grid = MapIndexToGrid(index, gridResolution);
    return MapGridToWorld(grid, gridResolution, gridScale, coordinatePos, rotatedMatrix);
  }

  static public int MapWorldToIndex(
    float3 worldPos,
    int2 gridResolution,
    float2 gridScale,
    float3 coordinatePos,
    float3x3 rotatedMatrix
  )
  {
    var grid = MapWorldToGrid(worldPos, gridResolution, gridScale, coordinatePos, rotatedMatrix);
    return MapGridToIndex(grid, gridResolution);
  }


  public static float3x3 CalculateRotatedMatrixBy(float3 degAround, float3x3 scale)
  {
    // since (only) unity editor's camera use left-hand coordinate system, it inverse in term of 
    // z-axis (its actually a forward direction point to far away from the screen),
    // therefore the positive degrees in user's 'POV is clockwise not counter-clockwise.
    // We inverse degAround sign for the correct right-hand coordinate system.
    var degAroundX = -degAround.x;
    var degAroundY = -degAround.y;
    var degAroundZ = -degAround.z;

    var phiX = degAroundX * math.PI / 180;
    var c1x = new float3(1, 0, 0);
    var c2x = new float3(0, math.cos(phiX), -math.sin(phiX));
    var c3x = new float3(0, math.sin(phiX), math.cos(phiX));
    var Mx = new float3x3(c1x, c2x, c3x);

    var phiY = degAroundY * math.PI / 180;
    var c1y = new float3(math.cos(phiY), 0, math.sin(phiY));
    var c2y = new float3(0, 1, 0);
    var c3y = new float3(-math.sin(phiY), 0, math.cos(phiY));
    var My = new float3x3(c1y, c2y, c3y);

    var phiZ = degAroundZ * math.PI / 180;
    var c1z = new float3(math.cos(phiZ), math.sin(phiZ), 0);
    var c2z = new float3(-math.sin(phiZ), math.cos(phiZ), 0);
    var c3z = new float3(0, 0, 1);
    var Mz = new float3x3(c1z, c2z, c3z);

    var MxMy = math.mul(Mx, My);
    var MxMyMz = math.mul(MxMy, Mz);
    var MxMyMzScale = math.mul(MxMyMz, scale);
    return MxMyMzScale;
  }

  public void InitConvertedComponents()
  {
    _originalMatrix = CalculateRotatedMatrixBy(0, Scale);
    rotatedMatrix = CalculateRotatedMatrixBy(Rotation, Scale);
  }

  public bool IsPosOutsideAt(float3 worldPos)
  {
    var gridPos = MapWorldToGrid(worldPos);
    return IsGridPosOutsideAt(gridPos);
  }

  static public bool IsPosOutsideAt(
    in float3 worldPos,
    in int2 GridResolution,
    in float2 GridScale,
    in float3 coordPosition,
    in float3x3 rotatedMatrix
  )
  {
    int2 gridPos = MapWorldToGrid(worldPos, GridResolution, GridScale, coordPosition, rotatedMatrix);
    return IsGridPosOutsideAt(gridPos, GridResolution);
  }

  public bool IsGridPosOutsideAt(int2 gridPos)
  {
    if (gridPos.x > GridResolution.x - 1 || gridPos.x < 0) return true;
    if (gridPos.y > GridResolution.y - 1 || gridPos.y < 0) return true;
    return false;
  }

  static public bool IsGridPosOutsideAt(int2 gridPos, int2 GridResolution)
  {
    if (gridPos.x > GridResolution.x - 1 || gridPos.x < 0) return true;
    if (gridPos.y > GridResolution.y - 1 || gridPos.y < 0) return true;
    return false;
  }

  public float3 ConvertToRotated(in float3 worldPos)
  {
    return math.mul(worldPos, rotatedMatrix) + (float3)transform.position;
  }

  static public float3 ConvertToRotated(
    in float3 worldPos,
    in float3x3 rotatedMatrix,
    in float3 coordPosition
  )
  {
    return math.mul(worldPos, rotatedMatrix) + coordPosition;
  }

  /// <summary>
  /// return a world position without any rotating processes.
  /// </summary>
  /// <returns></returns>
  public float3 ConvertToNonRotated(in float3 worldPos)
  {
    return math.mul(worldPos - (float3)transform.position, math.inverse(rotatedMatrix));
  }

  static public float3 ConvertToNonRotated(
    in float3 worldPos,
    float3 coordPosition,
    float3x3 rotatedMatrix
  )
  {
    return math.mul(worldPos - coordPosition, math.inverse(rotatedMatrix));
  }

  void OnDrawGizmos()
  {
    var color = Utility.GetColorFrom((ColorIndex)colorIndex);
    color.a = gizmosQuadAlpha;
    Gizmos.color = color;

    var gizmoScale = new Vector3(GridScale.x * gizmosQuadScale, GridScale.y * gizmosQuadScale, GridScale.x * gizmosQuadScale);
    var matrix = CalculateRotatedMatrixBy(Rotation, Scale);
    var pos = (float3)transform.position;

    if (shouldDrawMap)
    {
      for (int x = 0; x < GridResolution.x; ++x)
        for (int y = 0; y < GridResolution.y; ++y)
          Gizmos.DrawCube(MapGridToWorld(new int2(x, y), GridResolution, GridScale, pos, matrix), gizmoScale);
    }

    DebugSpatialMapOnDrawGizmos();
  }
}