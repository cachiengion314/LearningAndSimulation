using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
  public GameObject gridCubePref;
  public GameObject agentPref;
  public GridWorld gridWorld;
}

/// <summary>
/// Create a Spawner entity in ECS world
/// This is actually a bridge that connect between ECS world and MonoBehavior world
/// </summary>
public class SpawnerBaker : Baker<SpawnerAuthoring>
{
  public override void Bake(SpawnerAuthoring authoring)
  {
    var entity = GetEntity(TransformUsageFlags.Dynamic);
    AddComponent(
      entity,
      new Spawner
      {
        GridCube = GetEntity(authoring.gridCubePref, TransformUsageFlags.Dynamic),
        Agent = GetEntity(authoring.agentPref, TransformUsageFlags.Dynamic),
        GridScale = authoring.gridWorld.GridScale,
        GridResolution = authoring.gridWorld.GridResolution,
        GridRotation = authoring.gridWorld.Rotation
      }
    );
  }
}
