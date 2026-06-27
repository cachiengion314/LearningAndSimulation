using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

namespace TestECS
{
  [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
  public partial class EventReactionSystemGroup : ComponentSystemGroup { }

  /// <summary>
  /// EventReactionSystem
  /// </summary>
  [UpdateInGroup(typeof(EventReactionSystemGroup))]
  [UpdateBefore(typeof(MovingBlockSystem))]
  public partial struct EventReactionSystem : ISystem
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
          .Query<DynamicBuffer<TouchDownEvent>>()
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

            foreach (var (request, requestEntity) in
            SystemAPI.Query<RefRW<RecalculateGridRequest>>()
              .WithDisabled<RecalculateGridRequest>()
              .WithEntityAccess())
              ecb.SetComponentEnabled<RecalculateGridRequest>(requestEntity, true);
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