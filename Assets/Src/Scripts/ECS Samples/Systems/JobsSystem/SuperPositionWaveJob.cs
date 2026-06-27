using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct SuperPositionWaveJob : IJobEntity
{
  [ReadOnly] public NativeArray<TouchSample> TouchSamples;

  public static bool IsTouchChange(float3 TouchPosition, float3 PreviousTouchPosition)
  {
    return math.lengthsq(TouchPosition - PreviousTouchPosition) > .1f;
  }

  static float CalculateHeight(float3 x0, float3 x, float t)
  {
    var TWO_PI = math.PI * 2;
    var r = x - x0;
    var R = -math.length(r);

    var H = 2.0f * math.exp(-.1f * math.abs(R));
    var a = .5f;
    var b = .0f;
    var X = math.clamp(R - 7.5f + t, -TWO_PI, TWO_PI);

    var y = H * math.sin(a * (float)X + b);
    return y;
  }

  readonly void Execute(ref LocalTransform transform, ref Moveable moveable)
  {
    var currPos = transform.Position;

    var y = .0f;
    for (int i = TouchSamples.Length - 1; i >= 0; --i)
    {
      var touchPos = TouchSamples[i].TouchPosition;
      var t = 20 * TouchSamples[i].CurrentTime;
      y += CalculateHeight(touchPos, currPos, t);
    }

    var nextPos = new float3(currPos.x, y, currPos.z);
    transform.Position = nextPos;
  }
}
