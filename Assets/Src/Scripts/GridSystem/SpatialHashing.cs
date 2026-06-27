using HoangNam;
using Unity.Mathematics;
using UnityEngine;

public partial class GridWorld
{
  [Header("Spatial Hashing")]
  public int2 SpatialResolution;
  [SerializeField] bool shouldDrawSpatial;
  [Range(.0f, 1.0f)]
  [SerializeField] float gizmosSpatialScale;
  [Range(.0f, 1.0f)]
  [SerializeField] float gizmosSpatialAlpha;

  public float2 GetSpatialScale()
  {
    return GetSpatialScale(GridResolution, GridScale, SpatialResolution);
  }

  public void CollectOriginalGridPos(
    int2 spatialGridPos,
    out int2 xOriginalInterval,
    out int2 yOriginalInterval
  )
  {
    var spatialCellIdx = MapGridToIndex(spatialGridPos, SpatialResolution);
    CollectOriginalGridPos(
      spatialCellIdx, SpatialResolution, GridResolution,
      out var _xOriginalInterval, out var _yOriginalInterval
    );
    xOriginalInterval = _xOriginalInterval;
    yOriginalInterval = _yOriginalInterval;
  }

  public void CollectOriginalGridPos(
    int spatialCellIdx,
    out int2 xOriginalInterval,
    out int2 yOriginalInterval
  )
  {
    CollectOriginalGridPos(
      spatialCellIdx, SpatialResolution, GridResolution,
      out var _xOriginalInterval, out var _yOriginalInterval
    );
    xOriginalInterval = _xOriginalInterval;
    yOriginalInterval = _yOriginalInterval;
  }

  static public float2 GetSpatialScale(
    int2 originalResolution,
    float2 originalScale,
    int2 spatialResolution
  )
  {
    var xUnitAmount = originalResolution.x / math.max(spatialResolution.x, 1);
    var yUnitAmount = originalResolution.y / math.max(spatialResolution.y, 1);
    var xScale = xUnitAmount * originalScale.x;
    var yScale = yUnitAmount * originalScale.y;
    return new float2(xScale, yScale);
  }

  static public void CollectOriginalGridPos(
    int spatialCellIdx,
    int2 spatialResolution,
    int2 originalResolution,
    out int2 xOriginalInterval,
    out int2 yOriginalInterval
  )
  {
    var xUnitAmount = originalResolution.x / math.max(spatialResolution.x, 1);
    var yUnitAmount = originalResolution.y / math.max(spatialResolution.y, 1);
    var cellGridPos = MapIndexToGrid(spatialCellIdx, spatialResolution);
    var startX = cellGridPos.x * xUnitAmount;
    var endX = startX + xUnitAmount;
    xOriginalInterval = new int2(startX, endX);
    var startY = cellGridPos.y * yUnitAmount;
    var endY = startY + yUnitAmount;
    yOriginalInterval = new int2(startY, endY);
  }

  void DebugSpatialMapOnDrawGizmos()
  {
    if (!shouldDrawSpatial) return;

    var color = Utility.GetColorFrom((ColorIndex)(colorIndex + 1));
    color.a = gizmosSpatialAlpha;
    Gizmos.color = color;
    var spatialScale = GetSpatialScale();

    for (int x = 0; x < SpatialResolution.x; ++x)
    {
      for (int y = 0; y < SpatialResolution.y; ++y)
      {
        var spatialGridPos = new int2(x, y);
        var spatialWorldPos = MapGridToWorld(
          spatialGridPos,
          SpatialResolution,
          spatialScale,
          transform.position,
          rotatedMatrix
        );
        var spatialGridScale = new float3(
          spatialScale.x * gizmosSpatialScale, spatialScale.y * gizmosSpatialScale, 0
        );
        Gizmos.DrawWireCube(spatialWorldPos, spatialGridScale);
      }
    }
  }
}