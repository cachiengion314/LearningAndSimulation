using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct VisualizeMatchFieldJob : IJobEntity
{
  [ReadOnly] public int2 GridResolution;
  [ReadOnly] public float2 GridScale;
  [ReadOnly] public float3 CoordinatePos;
  [ReadOnly] public float3x3 RotationMatrix;
  [ReadOnly] public NativeArray<GridWeight> GridWeights;

  static float3 VisualMarchingField(float3 currWorldPos, float currWeight)
  {
    var nextPos = new float3(currWorldPos.x, math.min(currWeight, 125), currWorldPos.z);
    return nextPos;
  }

  static float GetWeightAt(
    int2 gridPos,
    int2 gridResolution,
    float defaultWeight,
    NativeArray<GridWeight> gridWeights
  )
  {
    var posIndex = GridWorld.MapGridToIndex(gridPos, gridResolution);
    var weight = defaultWeight;
    if (!GridWorld.IsGridPosOutsideAt(gridPos, gridResolution))
      weight = math.min(gridWeights[posIndex].Value, 125);
    return weight;
  }

  static float3 MoveAgent(
    float3 currWorldPos,
    int2 currGridPos,
    int2 gridResolution,
    float2 gridScale,
    float dt,
    float currWeight,
    NativeArray<GridWeight> gridWeights
  )
  {
    var h = gridScale.x;

    var preXGridPos = new int2(currGridPos.x - 1, currGridPos.y);
    var preXWeight = GetWeightAt(preXGridPos, gridResolution, currWeight, gridWeights);

    var nextXGridPos = new int2(currGridPos.x + 1, currGridPos.y);
    var nextXWeight = GetWeightAt(nextXGridPos, gridResolution, currWeight, gridWeights);

    var preYGridPos = new int2(currGridPos.x, currGridPos.y - 1);
    var preYWeight = GetWeightAt(preYGridPos, gridResolution, currWeight, gridWeights);

    var nextYGridPos = new int2(currGridPos.x, currGridPos.y + 1);
    var nextYWeight = GetWeightAt(nextYGridPos, gridResolution, currWeight, gridWeights);

    var ddx = (nextXWeight - preXWeight) / (2 * h);
    var ddy = (nextYWeight - preYWeight) / (2 * h);
    var v = -new float3(ddx, 0, ddy) * 1.0f;
    var nextPos = currWorldPos + dt * v;
    return nextPos;
  }

  readonly void Execute(ref LocalTransform transform, ref Agent agent)
  {
    var amount = GridResolution.x * GridResolution.y;
    if (GridWeights.Length < amount) return;

    var currWorldPos = transform.Position;
    var currGridPos = GridWorld.MapWorldToGrid(
      currWorldPos, GridResolution, GridScale, CoordinatePos, RotationMatrix
    );
    if (GridWorld.IsGridPosOutsideAt(currGridPos, GridResolution)) return;

    var currWeight = GetWeightAt(currGridPos, GridResolution, .0f, GridWeights);
    var nextPos = VisualMarchingField(currWorldPos, currWeight);

    transform.Position = nextPos;
  }
}
