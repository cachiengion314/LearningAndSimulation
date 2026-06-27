using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ObstacleAuthoring : MonoBehaviour
{
  class Baker : Baker<ObstacleAuthoring>
  {
    public override void Bake(ObstacleAuthoring authoring)
    {
      var entity = GetEntity(
        TransformUsageFlags.Dynamic |
        TransformUsageFlags.NonUniformScale);

      AddComponent(entity, new Obstacle());

      float3 s = authoring.transform.localScale;

      AddComponent(entity,
        new PostTransformMatrix
        {
          Value = float4x4.Scale(s)
        });
    }
  }
}