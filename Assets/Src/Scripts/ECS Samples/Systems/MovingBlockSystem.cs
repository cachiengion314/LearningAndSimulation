using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace TestECS
{
  [UpdateInGroup(typeof(EventReactionSystemGroup))]
  public partial struct MovingBlockSystem : ISystem
  {
    static NativeArray<float> d;
    static NativeArray<float3> vel;
    static NativeArray<float> p;
    static NativeArray<float> pNext;
    static NativeArray<bool> obstaclesMask;

    public void OnCreate(ref SystemState state)
    {
      state.RequireForUpdate<Spawner>();
    }

    public void OnDestroy(ref SystemState state)
    {
      if (d.IsCreated) d.Dispose();
      if (vel.IsCreated) vel.Dispose();
      if (p.IsCreated) p.Dispose();
      if (pNext.IsCreated) pNext.Dispose();
      if (obstaclesMask.IsCreated) obstaclesMask.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
      var touchBuffer = SystemAPI.GetSingletonBuffer<TouchSample>();
      var touchSamples = touchBuffer.AsNativeArray();

      var gridWeightBuffer = SystemAPI.GetSingletonBuffer<GridWeight>();
      var gridWeights = gridWeightBuffer.AsNativeArray();

      var spawner = SystemAPI.GetSingleton<Spawner>();
      var spawnerEntity = SystemAPI.GetSingletonEntity<Spawner>();
      var spawnerTransformRO = SystemAPI.GetComponentRO<LocalTransform>(spawnerEntity);
      var gridResolution = spawner.GridResolution;
      var gridScale = spawner.GridScale;
      var rotatedMatrix = GridWorld.CalculateRotatedMatrixBy(spawner.GridRotation, float3x3.identity);
      var coordinatePos = math.float3(spawnerTransformRO.ValueRO.Position);

      // ── Allocate once, initialise once ─────────────────────────────────
      var freestream = new float3(10, 0f, 0f);
      int targetSize = gridResolution.x * gridResolution.y;

      if (!d.IsCreated || d.Length != targetSize)
      {
        if (d.IsCreated) d.Dispose();
        d = new NativeArray<float>(targetSize, Allocator.Persistent);
      }
      if (!vel.IsCreated || vel.Length != targetSize)
      {
        if (vel.IsCreated) vel.Dispose();
        vel = new NativeArray<float3>(targetSize, Allocator.Persistent);
        for (int i = 0; i < targetSize; ++i) vel[i] = freestream;
      }
      if (!p.IsCreated || p.Length != targetSize)
      {
        if (p.IsCreated) p.Dispose();
        p = new NativeArray<float>(targetSize, Allocator.Persistent);
      }
      if (!pNext.IsCreated || pNext.Length != targetSize)
      {
        if (pNext.IsCreated) pNext.Dispose();
        pNext = new NativeArray<float>(targetSize, Allocator.Persistent);
      }
      if (!obstaclesMask.IsCreated)
      {
        if (obstaclesMask.IsCreated) obstaclesMask.Dispose();
        obstaclesMask = new NativeArray<bool>(targetSize, Allocator.Persistent);
      }

      // Clean arrays-------------------------
      for (int i = 0; i < targetSize; ++i)
      {
        // vel[i] = freestream;
        p[i] = 0f;
        pNext[i] = 0f;
        obstaclesMask[i] = false;
      }

      foreach (var (transform, post, bounds)
        in SystemAPI.Query<
          RefRO<LocalTransform>,
          RefRO<PostTransformMatrix>,
          RefRO<RenderBounds>>()
        .WithAll<Obstacle>())
      {
        var obsCenterPos = transform.ValueRO.Position;

        float4x4 m = post.ValueRO.Value;
        float3 scale;
        scale.x = math.length(m.c0.xyz);
        scale.y = math.length(m.c1.xyz);
        scale.z = math.length(m.c2.xyz);

        AABB box = bounds.ValueRO.Value;
        float3 min = obsCenterPos + box.Min * scale;
        float3 max = obsCenterPos + box.Max * scale;

        int i0 = GridWorld.MapWorldToGrid(min, gridResolution, gridScale, coordinatePos, rotatedMatrix).x;
        int i1 = GridWorld.MapWorldToGrid(max, gridResolution, gridScale, coordinatePos, rotatedMatrix).x;

        int j0 = GridWorld.MapWorldToGrid(min, gridResolution, gridScale, coordinatePos, rotatedMatrix).y;
        int j1 = GridWorld.MapWorldToGrid(max, gridResolution, gridScale, coordinatePos, rotatedMatrix).y;

        for (int i = i0; i <= i1; i++)
          for (int j = j0; j <= j1; j++)
          {
            var gridPos = new int2(i, j);
            if (GridWorld.IsGridPosOutsideAt(gridPos, gridResolution)) continue;
            var idx = GridWorld.MapGridToIndex(gridPos, gridResolution);
            obstaclesMask[idx] = true;
            vel[idx] = 0f;
          }
      }

      // Jobs -------------------
      var dependency = state.Dependency;
      dependency = new SuperPositionWaveJob
      {
        TouchSamples = touchSamples
      }
        .ScheduleParallel(dependency);
      const int N_OUTER = 1;
      const int N_JACOBI = 30;
      for (int i = 0; i < N_OUTER; ++i)
      {
        // Step 1: divergence of current vel
        dependency = new PoissonDivergenceJob
        {
          gridResolution = gridResolution,
          gridScale = gridScale,
          coordinatePos = coordinatePos,
          rotatedMatrix = rotatedMatrix,
          vel = vel,
          d = d,
        }.ScheduleParallel(dependency);

        for (int j = 0; j < N_JACOBI; ++j)
        {
          dependency = new PoissonJacobiJob
          {
            gridResolution = gridResolution,
            gridScale = gridScale,
            coordinatePos = coordinatePos,
            rotatedMatrix = rotatedMatrix,
            obstaclesMask = obstaclesMask,
            h = gridScale.x,
            d = d,
            p = p,
            pNext = pNext,
          }.ScheduleParallel(dependency);
          (p, pNext) = (pNext, p);
        }

        // Step 3 + 4: correct vel in place, stamp BCs
        dependency = new PoissonCorrectionJob
        {
          gridResolution = gridResolution,
          gridScale = gridScale,
          coordinatePos = coordinatePos,
          rotatedMatrix = rotatedMatrix,
          freestream = freestream,
          obstaclesMask = obstaclesMask,
          p = p,
          vel = vel,
        }.ScheduleParallel(dependency);
      }

      // Agents advect through the converged field
      uint frameSeed = (uint)(SystemAPI.Time.ElapsedTime * 1000.0) + 1u;
      dependency = new PoissonAgentMoveJob
      {
        dt = SystemAPI.Time.DeltaTime,
        freestream = freestream,
        frameSeed = frameSeed,
        gridResolution = gridResolution,
        gridScale = gridScale,
        coordinatePos = coordinatePos,
        rotatedMatrix = rotatedMatrix,
        vel = vel,
        obstaclesMask = obstaclesMask
      }.ScheduleParallel(dependency);

      state.Dependency = dependency;
    }
  }
}
