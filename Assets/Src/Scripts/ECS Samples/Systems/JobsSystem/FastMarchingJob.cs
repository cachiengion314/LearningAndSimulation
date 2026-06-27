using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Entities;

[BurstCompile]
public struct FastMarchingJob : IJob
{
  public int2 GoalGridPos;
  public int2 GridResolution;
  public float2 GridScale;
  [ReadOnly] public NativeHashMap<int, bool> ObstaclePosIndexes;
  public NativeArray<float> WeightsMap;
  public DynamicBuffer<GridWeight> WeightsBuffer;

  public void Execute()
  {
    for (int i = 0; i < WeightsMap.Length; i++)
      WeightsMap[i] = float.PositiveInfinity;

    GridWorld.FastMarching(
      GoalGridPos,
      ObstaclePosIndexes,
      GridResolution,
      GridScale,
      ref WeightsMap
    );

    for (int i = 0; i < WeightsBuffer.Length; i++)
    {
      var weight = WeightsMap[i];
      WeightsBuffer[i] = new GridWeight { Value = weight };
    }
  }
}
