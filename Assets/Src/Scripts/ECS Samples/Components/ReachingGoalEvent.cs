using Unity.Entities;
using Unity.Mathematics;

public struct ReachingGoalEvent : IBufferElementData
{
  public float3 Position;    // Where it happened
}
