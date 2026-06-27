using System.Collections.Generic;
using Lean.Touch;
using Unity.Entities;
using UnityEngine;

namespace TestECS
{
  public partial class TouchControlSystem : SystemBase
  {
    protected override void OnCreate()
    {
      RequireForUpdate<Spawner>();
      LeanTouch.OnFingerDown += OnFingerDown;
      LeanTouch.OnGesture += OnFingerUpdate;
    }

    protected override void OnDestroy()
    {
      LeanTouch.OnFingerDown -= OnFingerDown;
      LeanTouch.OnGesture -= OnFingerUpdate;
    }

    protected override void OnUpdate()
    {
      // normal ECS update
    }

    void SampleTouch(LeanFinger finger)
    {
      Ray ray = Camera.main.ScreenPointToRay(finger.ScreenPosition);
      var groundPlane = new Plane(Vector3.up, Vector3.zero);
      if (groundPlane.Raycast(ray, out float distance))
      {
        Vector3 worldPos = ray.GetPoint(distance);
        ECSEventTrigger.Instance.AddTouchDownEvent(worldPos);
      }
    }

    private void OnFingerDown(LeanFinger finger)
    {
      SampleTouch(finger);
    }

    private void OnFingerUpdate(List<LeanFinger> fingers)
    {
      SampleTouch(fingers[0]);
    }
  }
}