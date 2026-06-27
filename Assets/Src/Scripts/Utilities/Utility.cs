using System;
using Unity.Mathematics;
using UnityEngine;

namespace HoangNam
{
  public enum ColorIndex
  {
    None,
    White,
    Black,
    Gray,
    Blue,
    Red,
    Green,
    Yellow,
    Cyan,
    Magenta,
    Orange,
    Lime,
    Purple,
    Pink,
    Azure,
    Beige,
    Chocolate,
    Coral
  }


  public partial class Utility
  {
    public static string GetCurrentDate() => DateTime.Now.Date.ToString("dd/MM/yyyy");

    public static string FormatIndexToStringXXX(int index, string _stringFormat = "000")
    {
      return index.ToString(_stringFormat);
    }

    public static string FormatDateFrom(float seconds)
    {
      TimeSpan time = TimeSpan.FromSeconds(seconds);
      return time.ToString(@"mm\:ss");
    }

    public static int GetEpochTime() =>
      (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

    public static bool IsPointInBox(float2 p, float2 center, float2 halfSize)
    {
      return math.all(math.abs(p - center) <= halfSize);
    }

    public static float3 MapRange(float3 value, float3 fromMin, float3 fromMax, float3 toMin, float3 toMax)
    {
      return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }

    public static float MapRange(float value, float oldMin, float oldMax, float newMin, float newMax)
    {
      return newMin + (value - oldMin) * (newMax - newMin) / (oldMax - oldMin);
    }

    public static bool OverlapsAABB(
      float2 posA, float2 halfA,
      float2 posB, float2 halfB,
      out float2 normal,            // collision normal
      out float2 penetration        // penetration vector
    )
    {
      normal = float2.zero;
      penetration = float2.zero;

      float2 delta = posB - posA;
      float2 totalHalf = halfA + halfB;
      float2 overlap = totalHalf - math.abs(delta);

      // No overlap
      if (overlap.x < 0 || overlap.y < 0)
        return false;

      // choose smallest penetration axis
      if (overlap.x < overlap.y)
      {
        // horizontal collision
        normal = new float2(math.sign(delta.x), 0);
        penetration = new float2(overlap.x * normal.x, 0);
      }
      else
      {
        // vertical collision
        normal = new float2(0, math.sign(delta.y));
        penetration = new float2(0, overlap.y * normal.y);
      }

      return true;
    }

    public static bool IsPointInLine(float3 givenPoint, float3 lineOrigin, float3 lineDirection)
    {
      /// Nr + d = 0 is a line equation with lineOrigin r and its direction v
      /// N = (-v.y, v.x) => d = -N * r
      var r = lineOrigin;
      var v = lineDirection;
      var N = new float3(-v.y, v.x, 0);
      var d = -math.dot(N, r);
      var val = math.dot(N, givenPoint) + d;
      if (val == 0) return true;
      return false;
    }

    public static void SplitIntoAxisDirection(
      float3 givenPos,
      float3 origin,
      out float3 xDirection,
      out float3 yDirection
    )
    {
      var r = givenPos - origin;
      var projectedX = math.dot(r, Vector3.right);
      var projectedY = math.dot(r, Vector3.up);
      xDirection = projectedX * Vector3.right;
      yDirection = projectedY * Vector3.up;
    }

    public static float3 ProjectPointToLine(
      float3 givenPoint,
      float3 lineOrigin,
      float3 lineUnitDirection
    )
    {
      var r = givenPoint - lineOrigin;
      var n = new float3(-lineUnitDirection.y, lineUnitDirection.x, 0);
      float d = math.dot(r, n);
      return lineOrigin + (r - d * n);
    }


    public static float3 Lerp(float3 start, float3 end, float t) =>
      (1 - t) * start + t * end;

    public static void LinearMoveInUpdate(
      float3 targetPos,
      float3 currPos,
      float moveSpeed,
      out float3 nextPos
    )
    {
      var remaining = targetPos - currPos;
      var velocity = moveSpeed * math.normalizesafe(remaining);
      var delta = velocity * Time.fixedDeltaTime;

      if (math.lengthsq(delta) >= math.lengthsq(remaining))
      {
        nextPos = targetPos;
        return;
      }
      nextPos = currPos + delta;
    }

    public static void InterpolateMove(
      float3 startPos,
      float3 targetPos,
      float t,
      out float3 nextPos
    )
    {
      nextPos = Lerp(startPos, targetPos, t);
    }

    public static void InterpolateMoveInUpdate(
      in float3 currentPos,
      in float3 startPos,
      in float3 targetPos,
      in float speed,
      in float dt,
      out float t,
      out float3 nextPos
    )
    {
      var distanceFromStart = math.length(currentPos - startPos);
      var totalDistance = math.length(targetPos - startPos);
      var _t = distanceFromStart / totalDistance + speed * dt / totalDistance;
      t = math.min(_t, 1);
      nextPos = Lerp(startPos, targetPos, t);
    }

    public static void InterpolatePathInUpdate(
      in float3 currentPos,
      in int currentIdx,
      in int endIdx,
      in float3[] path,
      in float speed,
      in float dt,
      out float t,
      out float3 nextPos,
      out int nextIdx
    )
    {
      t = 1;
      nextPos = currentPos;
      nextIdx = currentIdx;
      var maxIdx = math.min(endIdx, path.Length - 1);
      if (currentIdx > maxIdx) return;

      var startPos = path[currentIdx];
      var targetIdx = currentIdx + 1;
      if (targetIdx > maxIdx) return;

      var targetPos = path[targetIdx];
      InterpolateMoveInUpdate(
        currentPos, startPos, targetPos, speed, dt,
        out var percent, out var nextPosition
      );
      t = (currentIdx + percent) / math.max(maxIdx, 1);
      nextPos = nextPosition;
      if (percent < 1)
      {
        nextIdx = currentIdx;
        return;
      }
      nextIdx = currentIdx + 1;
    }

    public static bool HasSameDirection(Vector3 a, Vector3 b, float _tolerance = 1e-4f)
    {
      if (a.sqrMagnitude == 0f || b.sqrMagnitude == 0f) return false;
      float dot = math.dot(a.normalized, b.normalized);
      return dot >= 1f - _tolerance;
    }

    public static float4 RandomColor(ref Unity.Mathematics.Random random)
    {
      var hue = (random.NextFloat() + 0.618034005f) % 1;
      return (Vector4)Color.HSVToRGB(hue, 1.0f, 1.0f);
    }

    public static int FindGreatestCommonDivisor(int a, int b)
    {
      var smaller = math.min(a, b);
      var bigger = math.max(a, b);
      var remain = bigger % smaller;
      while (remain > 0)
      {
        bigger = smaller;
        smaller = remain;
        remain = bigger % smaller;
      }
      return smaller;
    }

    public static void Print(in object o)
    {
      if (!Debug.isDebugBuild) return;
      Debug.Log(o);
    }

    public static Color GetColorFrom(in ColorIndex colorIndex = 0)
    {
      return colorIndex switch
      {
        ColorIndex.Black => Color.black,
        ColorIndex.Gray => Color.gray,
        ColorIndex.Blue => Color.blue,
        ColorIndex.Red => Color.red,
        ColorIndex.Green => Color.green,
        ColorIndex.Yellow => Color.yellow,
        ColorIndex.Cyan => Color.cyan,
        ColorIndex.Magenta => Color.magenta,
        ColorIndex.Orange => new Color(1.0f, 0.55f, 0.0f),
        ColorIndex.Lime => new Color(0.5f, 1.0f, 0.0f),
        ColorIndex.Purple => new Color(0.6f, 0.3f, 0.8f),
        ColorIndex.Pink => new Color(1.0f, 0.41f, 0.71f),
        ColorIndex.Azure => new Color(0.0f, 0.5f, 1.0f),
        ColorIndex.Beige => new Color(0.96f, 0.96f, 0.86f),
        ColorIndex.Chocolate => new Color(0.48f, 0.25f, 0.0f),
        ColorIndex.Coral => new Color(1.0f, 0.5f, 0.31f),
        _ => Color.white,
      };
    }


    public static float3 FindRandomPointOnCircle(float3 center, float r)
    {
      var phi = UnityEngine.Random.Range(0, 2 * math.PI);
      return new float3(center.x + math.cos(phi) * r, center.y + math.sin(phi) * r, 0);
    }

    public static void DrawLine(in float3 start, in float3 end, in ColorIndex colorIndex = 0, float duration = 0f)
    {
#if UNITY_EDITOR
      Debug.DrawLine(start, end, GetColorFrom(colorIndex), duration);
#endif
    }

    public static void DrawRay(in float3 start, in float3 dir, in ColorIndex colorIndex = 0, float duration = 0f)
    {
#if UNITY_EDITOR
      Debug.DrawRay(start, dir, GetColorFrom(colorIndex), duration);
#endif
    }

    public static void DrawCircle(in float3 center, float radius, in ColorIndex colorIndex = 0, int segments = 24, float duration = 0f)
    {
#if UNITY_EDITOR
      var color = GetColorFrom(colorIndex);
      if (segments < 3) segments = 3;
      if (radius <= 0f)
      {
        Debug.DrawLine(center, center + new float3(0f, 0.01f, 0f), color, duration);
        return;
      }

      float angleStep = 2f * math.PI / segments;
      float3 prev = center + new float3(radius, 0f, 0f);
      for (int i = 1; i <= segments; ++i)
      {
        float angle = angleStep * i;
        float3 next = center + new float3(math.cos(angle) * radius, math.sin(angle) * radius, 0f);
        Debug.DrawLine(prev, next, color, duration);
        prev = next;
      }
#endif
    }

    public static void DrawCircle(in float2 center, float radius, in ColorIndex colorIndex = 0, int segments = 24, float duration = 0f)
    {
#if UNITY_EDITOR
      DrawCircle(new float3(center.x, center.y, 0f), radius, colorIndex, segments, duration);
#endif
    }

    public static void DrawCircle(in float3 center, float radius, in float3 normal, in ColorIndex colorIndex = 0, int segments = 24, float duration = 0f)
    {
#if UNITY_EDITOR
      var color = GetColorFrom(colorIndex);
      if (segments < 3) segments = 3;

      float3 n = math.normalizesafe(normal);
      if (radius <= 0f)
      {
        Debug.DrawLine(center, center + n * 0.01f, color, duration);
        return;
      }

      float3 tangent = math.cross(n, new float3(0f, 1f, 0f));
      if (math.lengthsq(tangent) < 1e-6f)
        tangent = math.cross(n, new float3(1f, 0f, 0f));
      tangent = math.normalizesafe(tangent);
      float3 bitangent = math.cross(n, tangent);

      float angleStep = 2f * math.PI / segments;
      float3 prev = center + (tangent * math.cos(0f) + bitangent * math.sin(0f)) * radius;
      for (int i = 1; i <= segments; ++i)
      {
        float angle = angleStep * i;
        float3 next = center + (tangent * math.cos(angle) + bitangent * math.sin(angle)) * radius;
        Debug.DrawLine(prev, next, color, duration);
        prev = next;
      }
#endif
    }

    public static float3x3 CalculateRotatedMatrixBy(float3 degAround, float3x3 scale)
    {
      var degX = -degAround.x * math.PI / 180f;
      var degY = -degAround.y * math.PI / 180f;
      var degZ = -degAround.z * math.PI / 180f;

      var Mx = new float3x3(
        new float3(1, 0, 0),
        new float3(0, math.cos(degX), -math.sin(degX)),
        new float3(0, math.sin(degX), math.cos(degX))
      );

      var My = new float3x3(
        new float3(math.cos(degY), 0, math.sin(degY)),
        new float3(0, 1, 0),
        new float3(-math.sin(degY), 0, math.cos(degY))
      );

      var Mz = new float3x3(
        new float3(math.cos(degZ), math.sin(degZ), 0),
        new float3(-math.sin(degZ), math.cos(degZ), 0),
        new float3(0, 0, 1)
      );

      return math.mul(math.mul(Mx, My), math.mul(Mz, scale));
    }

    public static void DrawQuad(
      in float3 worldPos,
      in float2 size,
      in float3 degAround,
      in float3x3 scale,
      in ColorIndex colorIndex = 0,
      float duration = 0f
    )
    {
#if UNITY_EDITOR
      var color = GetColorFrom(colorIndex);
      var x = size.x / 2;
      var y = size.y / 2;

      var rot = CalculateRotatedMatrixBy(degAround, scale);
      var p1 = worldPos + math.mul(new float3(-x, -y, 0), rot);
      var p2 = worldPos + math.mul(new float3(-x, y, 0), rot);
      var p3 = worldPos + math.mul(new float3(x, y, 0), rot);
      var p4 = worldPos + math.mul(new float3(x, -y, 0), rot);

      Debug.DrawLine(p1, p2, color, duration);
      Debug.DrawLine(p2, p3, color, duration);
      Debug.DrawLine(p3, p4, color, duration);
      Debug.DrawLine(p4, p1, color, duration);
#endif
    }

    public static void DrawText(string text, Vector3 worldPos, Color? colour = null)
    {
#if UNITY_EDITOR
      UnityEditor.Handles.BeginGUI();
      var restoreColor = GUI.color;
      if (colour.HasValue) GUI.color = colour.Value;

      var view = UnityEditor.SceneView.currentDrawingSceneView;
      var screenPos = view.camera.WorldToScreenPoint(worldPos);
      if (screenPos.z < 0 || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height)
      {
        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();
        return;
      }

      Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
      GUI.Label(new Rect(screenPos.x - size.x / 2, -screenPos.y + view.position.height - 35, size.x, size.y), text);
      GUI.color = restoreColor;
      UnityEditor.Handles.EndGUI();
#endif
    }
    public static string ConvertTimeSpanToTimeStr(TimeSpan elapsedTime)
    {
      int days = elapsedTime.Days;
      int hours = elapsedTime.Hours;
      int minutes = elapsedTime.Minutes;
      int seconds = elapsedTime.Seconds;

      if (days > 0) return $"{days}d{hours}h";
      else if (hours > 0) return $"{hours}h{minutes}m";
      else if (minutes > 0) return $"{minutes}m{seconds}s";
      else return $"{seconds}s";
    }

    public static string FormatTimeClockFrom(TimeSpan timeSpan)
    {
      if (timeSpan <= TimeSpan.Zero || timeSpan.Seconds <= 0) return "Finished";
      if (timeSpan.Days > 0) return timeSpan.Days + "d" + timeSpan.Hours + "h";
      if (timeSpan.Hours > 0) return timeSpan.Hours + "h" + timeSpan.Minutes + "m";
      return timeSpan.Minutes + "m" + timeSpan.Seconds + "s";
    }
  }
}