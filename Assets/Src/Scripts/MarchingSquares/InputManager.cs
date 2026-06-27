using System;
using Lean.Touch;
using UnityEngine;

public class InputManager : MonoBehaviour
{
  [Header("Actions")]
  public static Action<Vector3> onTouching;

  private void Start()
  {
    LeanTouch.OnFingerDown += Clicking;
    LeanTouch.OnFingerUpdate += Clicking;
  }

  private void OnDestroy()
  {
    LeanTouch.OnFingerDown -= Clicking;
    LeanTouch.OnFingerUpdate -= Clicking;
  }

  void Clicking(LeanFinger leanFinger)
  {
    RaycastHit hit;
    Physics.Raycast(Camera.main.ScreenPointToRay(leanFinger.ScreenPosition), out hit, 50);
    if (hit.collider == null) return;

    onTouching?.Invoke(hit.point);
  }
}