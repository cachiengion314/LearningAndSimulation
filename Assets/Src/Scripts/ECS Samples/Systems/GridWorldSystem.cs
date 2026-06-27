using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;

namespace TestECS
{
  [UpdateInGroup(typeof(EventReactionSystemGroup))]
  [UpdateBefore(typeof(MovingBlockSystem))]
  public partial struct GridWorldSystem : ISystem
  {
    public void OnCreate(ref SystemState state)
    {
      state.RequireForUpdate<Spawner>();

      if (!SystemAPI.HasSingleton<GridWeight>())
      {
        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.AddBuffer<GridWeight>(entity);
      }

      var query = SystemAPI.QueryBuilder()
      .WithAll<RecalculateGridRequest>()
      .Build();

      if (query.IsEmptyIgnoreFilter)
      {
        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.SetName(entity, "RecalculateGridRequest Entity");
        state.EntityManager.AddComponent<RecalculateGridRequest>(entity);
        state.EntityManager
          .SetComponentEnabled<RecalculateGridRequest>(entity, false);
      }
    }

    public void OnUpdate(ref SystemState state)
    {
      var ecb = new EntityCommandBuffer(Allocator.Temp);

      foreach (var (request, requestEntity) in
      SystemAPI.Query<RefRW<RecalculateGridRequest>>()
        .WithEntityAccess())
      {
        // Disable FIRST before scheduling anything
        ecb.SetComponentEnabled<RecalculateGridRequest>(requestEntity, false);

        var entity = SystemAPI.GetSingletonEntity<GridWeight>();
        var weightsBuffer = state.EntityManager.GetBuffer<GridWeight>(entity);

        var spawner = SystemAPI.GetSingleton<Spawner>();
        var gridResolution = spawner.GridResolution;
        var gridScale = spawner.GridScale;
        var amountOfInstances = gridResolution.x * gridResolution.y;

        var goalGridPos = new int2(24, 24);
        var obstaclePosIndexes = new NativeHashMap<int, bool>(150, Allocator.TempJob);
        for (int x = 10; x < 15; ++x)
        {
          for (int y = 10; y < 40; ++y)
          {
            var gridPos = new int2(x, y);
            var posIndex = GridWorld.MapGridToIndex(gridPos, gridResolution);
            obstaclePosIndexes.Add(posIndex, true);
          }
        }
        var weightsMap = new NativeArray<float>(amountOfInstances, Allocator.TempJob);
        weightsBuffer.ResizeUninitialized(amountOfInstances);

        var fastMarchingJob = new FastMarchingJob
        {
          GoalGridPos = goalGridPos,
          GridResolution = gridResolution,
          GridScale = gridScale,
          ObstaclePosIndexes = obstaclePosIndexes,
          WeightsMap = weightsMap,
          WeightsBuffer = weightsBuffer
        };

        state.Dependency = fastMarchingJob.Schedule(state.Dependency);
        obstaclePosIndexes.Dispose(state.Dependency);
        weightsMap.Dispose(state.Dependency);
      }

      ecb.Playback(state.EntityManager);
      ecb.Dispose();
    }
  }
}
