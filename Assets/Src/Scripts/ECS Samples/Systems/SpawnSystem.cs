using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace TestECS
{
  [UpdateInGroup(typeof(EventReactionSystemGroup))]
  public partial struct SpawnSystem : ISystem
  {
    public void OnCreate(ref SystemState state)
    {
      state.RequireForUpdate<Spawner>();
    }

    public void OnUpdate(ref SystemState state)
    {
      state.Enabled = false;

      var ecbSingleton =
        SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
      var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

      var spawner = SystemAPI.GetSingleton<Spawner>();
      var spawnerEntity = SystemAPI.GetSingletonEntity<Spawner>();
      var spawnerTransformRO =
        SystemAPI.GetComponentRO<LocalTransform>(spawnerEntity);

      var gridResolution = spawner.GridResolution;
      var gridScale = spawner.GridScale;
      var rotationMatrix =
        GridWorld.CalculateRotatedMatrixBy(spawner.GridRotation, float3x3.identity);
      var coordinatePos =
        spawnerTransformRO.ValueRO.Position;

      var amountOfCubeInstances =
        gridResolution.x * gridResolution.y;

      var rand = new Unity.Mathematics.Random(1);

      // -------------------------
      // Spawn Grid Cubes
      // -------------------------

      for (int i = 0; i < amountOfCubeInstances; ++i)
      {
        var entity = ecb.Instantiate(spawner.GridCube);

        var worldPos = GridWorld.MapIndexToWorld(
          i,
          gridResolution,
          gridScale,
          coordinatePos,
          rotationMatrix
        );

        ecb.SetComponent(entity,
          new LocalTransform
          {
            Position = worldPos,
            Rotation = quaternion.identity,
            Scale = gridScale.x
          });

        ecb.AddComponent(entity,
          new Block { Index = i });

        ecb.AddComponent(entity,
          new Moveable
          {
            RandomSeed =
              math.hash(new uint2((uint)i, 0x9E3779B9))
          });

        var color = new float4(
          rand.NextFloat() * .5f,
          .1f,
          1.0f,
          1f
        );

        ecb.AddComponent(entity,
          new MatColor { Value = color });
      }

      // -------------------------
      // Spawn Agents
      // -------------------------

      for (int x = 2; x < 18; ++x)
      {
        for (int y = 8; y < 45; ++y)
        {
          var agent = ecb.Instantiate(spawner.Agent);
          var agentGridPos = new int2(x, y);
          var agentWorldPos =
            GridWorld.MapGridToWorld(
              agentGridPos,
              gridResolution,
              gridScale,
              coordinatePos,
              rotationMatrix
            );

          ecb.SetComponent(agent,
            new LocalTransform
            {
              Position = agentWorldPos,
              Rotation = quaternion.identity,
              Scale = gridScale.x
            });
          ecb.AddComponent(agent,
            new Agent { Index = GridWorld.MapGridToIndex(agentGridPos, gridResolution) });
        }
      }
    }
  }
}