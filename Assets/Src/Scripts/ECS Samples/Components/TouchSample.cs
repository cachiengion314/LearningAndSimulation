using Unity.Entities;
using Unity.Mathematics;

public struct TouchSample : IBufferElementData
{
  public float3 TouchPosition;
  public float CurrentTime;
}