using Unity.Entities;
using Unity.Mathematics;

public struct TouchDownEvent : IBufferElementData
{
  public float3 Position;    // Where it happened
}
