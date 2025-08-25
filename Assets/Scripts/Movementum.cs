using UnityEngine;
using Unity.Mathematics;

public class Movementum : MonoBehaviour
{
  [Header("Settings")]
  [Tooltip("Enable if you want movement of the obj itself is auto controlled by component's algorithms")]
  [SerializeField] bool enableAutoUpdate;
  [SerializeField] bool useGravity;
  [SerializeField] float3 gravity;
  [SerializeField][Range(.01f, 100)] float mass = 1;
  public float Mass { get { return mass; } }
  public float3 ConstantForce = 0;
  float3 _lastFrameVelocity = 0;
  float3 _lastFramePosition = 0;

  void FixedUpdate()
  {
    if (!enableAutoUpdate) return;
    if (_lastFrameVelocity.Equals(0))
      _lastFramePosition = transform.position;
    transform.position = UpdatePosition();
    _lastFramePosition = transform.position;
  }

  float3 CalculateAccelerateBy()
  {
    var accelerate = ConstantForce / mass;
    return accelerate + gravity;
  }

  float3 CalculateVelocityBy()
  {
    var accelerate = CalculateAccelerateBy();
    var v = _lastFrameVelocity + accelerate * Time.fixedDeltaTime;
    return v;
  }

  float3 CalculatePositionBy(float3 v)
  {
    var x = _lastFramePosition + v * Time.fixedDeltaTime;
    return x;
  }

  /// <summary>
  /// Manual controlling obj movement for an advanced use case
  /// </summary>
  public float3 UpdatePosition()
  {
    var currentVelocity = CalculateVelocityBy();
    var position = CalculatePositionBy(currentVelocity);
    _lastFrameVelocity = currentVelocity;
    return position;
  }

  public void AddForce(float3 F)
  {
    ConstantForce += F;
  }

  public void ApplyVelocity(float3 velocity)
  {
    _lastFrameVelocity = velocity;
  }

  public void SetAutoUpdate(bool shouldAuto)
  {
    enableAutoUpdate = shouldAuto;
  }
}
