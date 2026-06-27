using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// Boundary condition helper.
// Inside the cylinder: v = 0.
// Outer ring: v = freestream.
// Everywhere else: leave alone.
static class PoissonBC
{
  public static bool IsInsideObsAt(
    float3 worldPos,
    int2 gridResolution,
    float2 gridScale,
    float3 coordinatePos,
    float3x3 rotatedMatrix,
    NativeArray<bool> obstaclesMask
  )
  {
    var idx = GridWorld.MapWorldToIndex(
      worldPos, gridResolution, gridScale, coordinatePos, rotatedMatrix
    );
    return obstaclesMask[idx];
  }

  public static bool IsOuterBoundary(int x, int y, int2 gridResolution)
  {
    return x == 0 || x == gridResolution.x - 1 || y == 0 || y == gridResolution.y - 1;
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// STEP 1 — Divergence
//   d = ∇·v = ∂u/∂x + ∂w/∂z   (central differences)
//   Outer ring: d = 0 (uniform freestream has no divergence).
//   Inside cylinder: d = 0 (no fluid).
//   Fluid cell with a solid neighbour: mirror current velocity into the solid
//   so the difference contributes nothing in that direction. This keeps the
//   divergence stencil consistent with the Neumann pressure BC.
// ─────────────────────────────────────────────────────────────────────────────
[BurstCompile]
public partial struct PoissonDivergenceJob : IJobEntity
{
  [ReadOnly] public int2 gridResolution;
  [ReadOnly] public float2 gridScale;
  [ReadOnly] public float3 coordinatePos;
  [ReadOnly] public float3x3 rotatedMatrix;
  [ReadOnly] public NativeArray<float3> vel;
  [NativeDisableParallelForRestriction] public NativeArray<float> d;

  void Execute(ref Block block)
  {
    int idx = block.Index;
    int2 gridPos = GridWorld.MapIndexToGrid(idx, gridResolution);
    int x = gridPos.x;
    int y = gridPos.y;

    if (PoissonBC.IsOuterBoundary(x, y, gridResolution))
    {
      d[idx] = 0f;
      return;
    }

    // Interior cells always have valid neighbors (no clamping needed)
    var leftIdx = GridWorld.MapGridToIndex(new int2(x - 1, y), gridResolution);
    var rightIdx = GridWorld.MapGridToIndex(new int2(x + 1, y), gridResolution);
    var downIdx = GridWorld.MapGridToIndex(new int2(x, y - 1), gridResolution);
    var upIdx = GridWorld.MapGridToIndex(new int2(x, y + 1), gridResolution);

    var vL = vel[leftIdx];
    var vR = vel[rightIdx];
    var vD = vel[downIdx];
    var vU = vel[upIdx];

    d[idx] = (vR.x - vL.x) / (2f * gridScale.x) + (vU.z - vD.z) / (2f * gridScale.y);
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// STEP 2 — Pressure Poisson (one Jacobi sweep)
//   Δp = d   discretised on uniform grid (h = dx = dy):
//   p[i,j] = ¼ · ( p_L + p_R + p_D + p_U  −  d · h² )
//   Neumann at cylinder: mirror current pressure into solid neighbours.
//   Dirichlet p = 0 on outer ring.
// ─────────────────────────────────────────────────────────────────────────────
[BurstCompile]
public partial struct PoissonJacobiJob : IJobEntity
{
  [ReadOnly] public int2 gridResolution;
  [ReadOnly] public float2 gridScale;
  [ReadOnly] public float3 coordinatePos;
  [ReadOnly] public float3x3 rotatedMatrix;
  [ReadOnly] public float h;
  [ReadOnly] public NativeArray<float> d;
  [ReadOnly] public NativeArray<float> p;
  [ReadOnly] public NativeArray<bool> obstaclesMask;
  [NativeDisableParallelForRestriction] public NativeArray<float> pNext;

  void Execute(ref Block block)
  {
    var currIdx = block.Index;
    var gridPos = GridWorld.MapIndexToGrid(currIdx, gridResolution);
    var x = gridPos.x;
    var y = gridPos.y;

    // Outer boundary: Dirichlet p = 0 — free-stream pressure unchanged
    if (PoissonBC.IsOuterBoundary(x, y, gridResolution))
    {
      pNext[currIdx] = 0f;
      return;
    }

    var leftGridPos = new int2(x - 1, y);
    var leftPos = GridWorld.MapGridToWorld(
      leftGridPos, gridResolution, gridScale, coordinatePos, rotatedMatrix
    );
    var leftIdx = GridWorld.MapGridToIndex(leftGridPos, gridResolution);
    var pL = p[leftIdx];

    var rightGridPos = new int2(x + 1, y);
    var rightPos = GridWorld.MapGridToWorld(
      rightGridPos, gridResolution, gridScale, coordinatePos, rotatedMatrix
    );
    var rightIdx = GridWorld.MapGridToIndex(rightGridPos, gridResolution);
    var pR = p[rightIdx];

    var downGridPos = new int2(x, y - 1);
    var downPos = GridWorld.MapGridToWorld(
      downGridPos, gridResolution, gridScale, coordinatePos, rotatedMatrix
    );
    var downIdx = GridWorld.MapGridToIndex(downGridPos, gridResolution);
    var pD = p[downIdx];

    var upGridPos = new int2(x, y + 1);
    var upPos = GridWorld.MapGridToWorld(
     upGridPos, gridResolution, gridScale, coordinatePos, rotatedMatrix
   );
    var upIdx = GridWorld.MapGridToIndex(upGridPos, gridResolution);
    var pU = p[upIdx];

    // # --------------------------------
    // # Neumann ghost rule:
    // # if neighbor inside obstacle,
    // # mirror current pressure
    // # dp/dn=0 => p[neighbor]=p[current]
    // # --------------------------------
    if (PoissonBC.IsInsideObsAt(leftPos, gridResolution, gridScale, coordinatePos, rotatedMatrix, obstaclesMask))
      pL = p[currIdx];
    if (PoissonBC.IsInsideObsAt(rightPos, gridResolution, gridScale, coordinatePos, rotatedMatrix, obstaclesMask))
      pR = p[currIdx];
    if (PoissonBC.IsInsideObsAt(downPos, gridResolution, gridScale, coordinatePos, rotatedMatrix, obstaclesMask))
      pD = p[currIdx];
    if (PoissonBC.IsInsideObsAt(upPos, gridResolution, gridScale, coordinatePos, rotatedMatrix, obstaclesMask))
      pU = p[currIdx];

    // p[i,j] = .25 * (p_L + p_R + p_D + p_U  -  d·h²)
    var sigmaNeighbors = pL + pR + pD + pU;
    pNext[currIdx] = .25f * (sigmaNeighbors - d[currIdx] * gridScale.x * gridScale.y);
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// STEP 3 + 4 — Correct vel in place, stamp BCs
//   Outer ring:    vel = freestream
//   Inside solid:  vel = 0
//   Interior:      vel ← vel − ∇p
// ─────────────────────────────────────────────────────────────────────────────
[BurstCompile]
public partial struct PoissonCorrectionJob : IJobEntity
{
  [ReadOnly] public int2 gridResolution;
  [ReadOnly] public float2 gridScale;
  [ReadOnly] public float3 coordinatePos;
  [ReadOnly] public float3x3 rotatedMatrix;
  [ReadOnly] public float3 freestream;
  [ReadOnly] public NativeArray<float> p;
  [ReadOnly] public NativeArray<bool> obstaclesMask;
  [NativeDisableParallelForRestriction] public NativeArray<float3> vel;

  void Execute(ref Block block)
  {
    var currIdx = block.Index;
    var gridPos = GridWorld.MapIndexToGrid(currIdx, gridResolution);
    var x = gridPos.x;
    var y = gridPos.y;

    // Outer boundary: restore free-stream velocity — no pressure correction applied
    if (PoissonBC.IsOuterBoundary(x, y, gridResolution))
    {
      vel[currIdx] = freestream;
      return;
    }

    int leftIdx = GridWorld.MapGridToIndex(new int2(x - 1, y), gridResolution);
    int rightIdx = GridWorld.MapGridToIndex(new int2(x + 1, y), gridResolution);
    int downIdx = GridWorld.MapGridToIndex(new int2(x, y - 1), gridResolution);
    int upIdx = GridWorld.MapGridToIndex(new int2(x, y + 1), gridResolution);

    float3 worldPos = GridWorld.MapIndexToWorld(
      currIdx, gridResolution, gridScale, coordinatePos, rotatedMatrix
    );
    if (
      PoissonBC.IsInsideObsAt(
      worldPos, gridResolution, gridScale, coordinatePos, rotatedMatrix, obstaclesMask
      )
    )
    {
      vel[currIdx] = 0;
      return;
    }

    var dpDx = (p[rightIdx] - p[leftIdx]) / (2f * gridScale.x);
    var dpDz = (p[upIdx] - p[downIdx]) / (2f * gridScale.y);
    var nabla_p = 1.0f * new float3(dpDx, 0, dpDz);

    // pressure correction
    float3 corrected = vel[currIdx] - nabla_p;
    vel[currIdx] = corrected;
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// AGENT ADVECTION — sample the converged field at agent position.
// ─────────────────────────────────────────────────────────────────────────────
[BurstCompile]
public partial struct PoissonAgentMoveJob : IJobEntity
{
  [ReadOnly] public float dt;
  [ReadOnly] public float3 freestream;
  [ReadOnly] public uint frameSeed;
  [ReadOnly] public int2 gridResolution;
  [ReadOnly] public float2 gridScale;
  [ReadOnly] public float3 coordinatePos;
  [ReadOnly] public float3x3 rotatedMatrix;
  [ReadOnly] public NativeArray<float3> vel;
  [ReadOnly] public NativeArray<bool> obstaclesMask;

  static float3 RandomDirectionXZ(ref Random rng)
  {
    var angle = rng.NextFloat(0f, 2f * math.PI);
    return new float3(
      math.cos(angle),
      0f,
      math.sin(angle)
    );
  }

  void Execute([EntityIndexInQuery] int entityIdx, ref LocalTransform transform, ref Agent agent)
  {
    var snapPosIdx = GridWorld.MapWorldToIndex(
        transform.Position, gridResolution, gridScale, coordinatePos, rotatedMatrix
    );
    snapPosIdx = math.clamp(snapPosIdx, 0, gridResolution.x * gridResolution.y - 1);

    var v = vel[snapPosIdx];
    transform.Position += v * dt;
  }
}