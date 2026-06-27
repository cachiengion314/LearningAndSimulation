using Unity.Entities;
using Unity.Mathematics;

public struct Spawner : IComponentData
{
  public Entity GridCube;
  public Entity Agent;
  public float2 GridScale;
  public int2 GridResolution;
  public float3 GridRotation;
}