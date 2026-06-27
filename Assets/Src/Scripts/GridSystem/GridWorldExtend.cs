using Unity.Mathematics;
using Unity.Collections;

public partial class GridWorld
{
  public int2[] directions = new int2[] { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };
  /// <summary>
  /// return world position of neighbor and float3(-1, -1, -1) for invalid neighbor.
  /// </summary>
  /// 
  public float3[] FindNeighborsAt(float3 worldPos)
  {
    float3[] neighbors = new float3[directions.Length];
    var gridPos = MapWorldToGrid(worldPos);
    for (int i = 0; i < directions.Length; ++i)
    {
      var dir = directions[i];
      var neighbor = gridPos + dir;
      if (IsGridPosOutsideAt(neighbor))
      {
        neighbors[i] = new float3(-1, -1, -1);
        continue;
      }
      float3 wPos = MapGridToWorld(neighbor);
      neighbors[i] = wPos;
    }
    return neighbors;
  }

  public NativeArray<int2> FindNeighberAt(int index)
  {
    using var directions = new NativeArray<int2>(
      new int2[4] { new(0, 1), new(0, -1), new(-1, 0), new(1, 0) }, Allocator.Temp);
    var neighbers = new NativeArray<int2>(directions.Length, Allocator.Temp);

    var gridPos = MapIndexToGrid(index);
    for (int i = 0; i < directions.Length; i++)
    {
      var direction = directions[i];
      var neighber = gridPos + direction;
      neighbers[i] = neighber;
    }
    return neighbers;
  }

  public NativeArray<int2> FindNeighber8DirAt(int index)
  {
    using var directions = new NativeArray<int2>(
      new int2[8] { new(0, 1), new(0, -1), new(-1, 0), new(1, 0), new(1, 1), new(-1, 1), new(1, -1), new(-1, -1) }, Allocator.Temp);
    var neighbers = new NativeArray<int2>(directions.Length, Allocator.Temp);

    var gridPos = MapIndexToGrid(index);
    for (int i = 0; i < directions.Length; i++)
    {
      var direction = directions[i];
      var neighber = gridPos + direction;
      neighbers[i] = neighber;
    }
    return neighbers;
  }
}