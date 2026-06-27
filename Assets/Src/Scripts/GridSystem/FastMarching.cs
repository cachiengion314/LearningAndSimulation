using Unity.Collections;
using Unity.Mathematics;

public struct NativeMinHeap<K>
  where K : unmanaged
{
  public struct HeapNode
  {
    public K Key;
    public float Priority;
  }

  private NativeList<HeapNode> data;

  public NativeMinHeap(int capacity, Allocator allocator)
  {
    data = new NativeList<HeapNode>(capacity, allocator);
  }

  public int Count => data.Length;

  public void Dispose()
  {
    if (data.IsCreated)
      data.Dispose();
  }

  private static int Parent(int i) => (i - 1) / 2;
  private static int Left(int i) => 2 * i + 1;
  private static int Right(int i) => 2 * i + 2;

  public void Add(K key, float priority)
  {
    data.Add(new HeapNode
    {
      Key = key,
      Priority = priority
    });

    HeapifyUp(data.Length - 1);
  }

  public bool TryPeek(out HeapNode node)
  {
    if (data.Length == 0)
    {
      node = default;
      return false;
    }

    node = data[0];
    return true;
  }

  public bool TryPop(out HeapNode node)
  {
    if (data.Length == 0)
    {
      node = default;
      return false;
    }

    node = data[0];

    var last = data[data.Length - 1];
    data.RemoveAt(data.Length - 1);

    if (data.Length > 0)
    {
      data[0] = last;
      HeapifyDown(0);
    }

    return true;
  }

  private void HeapifyUp(int i)
  {
    while (i > 0)
    {
      int p = Parent(i);

      if (data[p].Priority <= data[i].Priority)
        break;

      Swap(p, i);
      i = p;
    }
  }

  private void HeapifyDown(int i)
  {
    int n = data.Length;

    while (true)
    {
      int left = Left(i);
      int right = Right(i);
      int smallest = i;

      if (left < n &&
          data[left].Priority < data[smallest].Priority)
        smallest = left;

      if (right < n &&
          data[right].Priority < data[smallest].Priority)
        smallest = right;

      if (smallest == i)
        break;

      Swap(i, smallest);
      i = smallest;
    }
  }

  private void Swap(int a, int b)
  {
    var tmp = data[a];
    data[a] = data[b];
    data[b] = tmp;
  }
}

/// <summary>
/// Fast Marching
/// </summary>
public partial class GridWorld
{
  public static NativeList<int2> FindNeighborsAt(
    int posIdx,
    int2 gridResolution,
    NativeArray<int2> directions,
    NativeHashMap<int, bool> obstaclePosIndexes,
    NativeHashMap<int, bool> accepted
  )
  {
    NativeList<int2> found = new(directions.Length, Allocator.Temp);
    var currGridPos = MapIndexToGrid(posIdx, gridResolution);

    for (int i = 0; i < directions.Length; ++i)
    {
      var neighborGridPos = currGridPos + directions[i];
      if (IsGridPosOutsideAt(neighborGridPos, gridResolution)) continue;
      var neighborPosIdx = MapGridToIndex(neighborGridPos, gridResolution);
      if (obstaclePosIndexes.ContainsKey(neighborPosIdx)) continue;
      if (accepted.ContainsKey(neighborPosIdx)) continue;

      found.Add(neighborGridPos);
    }
    return found;
  }

  static void SetWeightAt(int posIdx, float weight, NativeArray<float> weightsMap)
  {
    if (posIdx < 0 || posIdx > weightsMap.Length - 1) return;
    weightsMap[posIdx] = weight;
  }

  static float GetWeightAt(int posIdx, NativeArray<float> weightsMap)
  {
    if (posIdx < 0 || posIdx > weightsMap.Length - 1) return float.PositiveInfinity;
    return weightsMap[posIdx];
  }

  public static void FastMarching(
    int2 goalGridPos,
    NativeHashMap<int, bool> obstaclePosIndexes,
    int2 gridResolution,
    float2 gridScale,
    ref NativeArray<float> weightsMap
  )
  {
    var totalAmount = gridResolution.x * gridResolution.y;
    var accepted = new NativeHashMap<int, bool>(totalAmount, Allocator.Temp);
    var trialExisted = new NativeHashMap<int, bool>(totalAmount, Allocator.Temp);
    var trial = new NativeMinHeap<int>(totalAmount, Allocator.Temp);
    var directions = new NativeArray<int2>(4, Allocator.Temp);
    directions[0] = new(1, 0);
    directions[1] = new(0, 1);
    directions[2] = new(-1, 0);
    directions[3] = new(0, -1);
    var h = gridScale.x;

    var goalPosIdx = MapGridToIndex(goalGridPos, gridResolution);
    SetWeightAt(goalPosIdx, 0, weightsMap);
    trial.Add(goalPosIdx, 0);

    while (trial.Count > 0)
    {
      var haveItem = trial.TryPop(out var node);
      if (!haveItem) break;
      var nodeIdx = node.Key;
      accepted.Add(nodeIdx, true);

      using var neighborGridPositions = FindNeighborsAt(
        nodeIdx, gridResolution, directions, obstaclePosIndexes, accepted
      );
      for (int i = 0; i < neighborGridPositions.Length; ++i)
      {
        var neighborGridPos = neighborGridPositions[i];
        var neighborPosIdx = MapGridToIndex(neighborGridPos, gridResolution);

        var preXGridPos = new int2(neighborGridPos.x - 1, neighborGridPos.y);
        var preXPosIdx = MapGridToIndex(preXGridPos, gridResolution);

        var nextXGridPos = new int2(neighborGridPos.x + 1, neighborGridPos.y);
        var nextXPosIdx = MapGridToIndex(nextXGridPos, gridResolution);

        var preYGridPos = new int2(neighborGridPos.x, neighborGridPos.y - 1);
        var preYPosIdx = MapGridToIndex(preYGridPos, gridResolution);

        var nextYGridPos = new int2(neighborGridPos.x, neighborGridPos.y + 1);
        var nextYPosIdx = MapGridToIndex(nextYGridPos, gridResolution);

        var preXGridPosWeight = GetWeightAt(preXPosIdx, weightsMap);
        if (IsGridPosOutsideAt(preXGridPos, gridResolution))
          preXGridPosWeight = float.PositiveInfinity;

        var nextXGridPosWeight = GetWeightAt(nextXPosIdx, weightsMap);
        if (IsGridPosOutsideAt(nextXGridPos, gridResolution))
          nextXGridPosWeight = float.PositiveInfinity;

        var preYGridPosWeight = GetWeightAt(preYPosIdx, weightsMap);
        if (IsGridPosOutsideAt(preYGridPos, gridResolution))
          preYGridPosWeight = float.PositiveInfinity;

        var nextYGridPosWeight = GetWeightAt(nextYPosIdx, weightsMap);
        if (IsGridPosOutsideAt(nextYGridPos, gridResolution))
          nextYGridPosWeight = float.PositiveInfinity;

        var a = math.min(preXGridPosWeight, nextXGridPosWeight);
        var b = math.min(preYGridPosWeight, nextYGridPosWeight);
        if (b < a)
        {
          (a, b) = (b, a);
        }
        var d = a + h;
        if (math.abs(a - b) <= h)
        {
          var c = math.sqrt(2 * h * h - (a - b) * (a - b));
          d = (a + b + c) / 2.0f;
        }
        SetWeightAt(neighborPosIdx, d, weightsMap);
        if (!trialExisted.ContainsKey(neighborPosIdx))
        {
          trialExisted.Add(neighborPosIdx, true);
          trial.Add(neighborPosIdx, d);
        }
      }
    }

    directions.Dispose();
    accepted.Dispose();
    trialExisted.Dispose();
    trial.Dispose();
  }
}