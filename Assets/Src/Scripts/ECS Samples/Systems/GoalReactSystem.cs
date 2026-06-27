using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

namespace TestECS
{
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  public partial class GoalEventSystemGroup : ComponentSystemGroup { }

  /// <summary>
  /// GoalReactionSystem
  /// </summary>
  [UpdateInGroup(typeof(GoalEventSystemGroup))]
  [UpdateBefore(typeof(MovingBlockSystem))]
  public partial struct GoalReactionSystem : ISystem
  {
    public void OnCreate(ref SystemState state)
    {
      state.RequireForUpdate<Spawner>();

      if (!SystemAPI.HasSingleton<TouchSample>())
        state.EntityManager.CreateSingletonBuffer<TouchSample>();
    }

    public void OnUpdate(ref SystemState state)
    {
      using var ecb = new EntityCommandBuffer(Allocator.Temp);
      var touchSampleBuffer = SystemAPI.GetSingletonBuffer<TouchSample>();

      for (int i = 0; i < touchSampleBuffer.Length; ++i)
      {
        var touchSample = touchSampleBuffer[i];
        touchSample.CurrentTime += SystemAPI.Time.DeltaTime;
        touchSampleBuffer[i] = touchSample;
      }

      foreach (var (buffer, bufferEntity) in
        SystemAPI
          .Query<DynamicBuffer<ReachingGoalEvent>>()
          .WithEntityAccess()
      )
      {
        if (buffer.Length > 0)
        {
          var currentTouchPos = buffer[^1].Position;
          var latestTouchPos = float3.zero;
          if (touchSampleBuffer.Length > 0)
            latestTouchPos = touchSampleBuffer[^1].TouchPosition;
          var r = latestTouchPos - currentTouchPos;
          if (math.lengthsq(r) > .125f)
          {
            touchSampleBuffer.Add(
              new TouchSample
              {
                TouchPosition = currentTouchPos,
                CurrentTime = .0f
              }
            );
          }
          if (touchSampleBuffer.Length > 64) touchSampleBuffer.RemoveAt(0);

          /// demonstrate how to safely remove block component from entity
          foreach (var (transform, moveable, blockEntity) in
                   SystemAPI.Query<RefRW<LocalTransform>, RefRW<Moveable>>()
                            .WithAll<Block, Moveable>()
                            .WithEntityAccess())
          {
            ecb.RemoveComponent<Block>(blockEntity);
          }

          buffer.Clear(); // Clear events after processing
        }
      }

      ecb.Playback(state.EntityManager);
    }
  }
}