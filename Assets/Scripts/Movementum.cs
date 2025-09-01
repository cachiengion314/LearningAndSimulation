using UnityEngine;
using Unity.Mathematics;

public class Movementum : MonoBehaviour
{
  [Header("Settings")]
  [Tooltip("Enable if you want movement of the obj itself is auto controlled by component's algorithms")]
  [SerializeField] float3 gravity;
  [SerializeField][Range(.01f, 100)] float mass = 1;
  public float Mass { get { return mass; } }
  public bool AutoUpdate;
  public bool UseGravity;
  public float3 ConstantForce = 0;
  [Range(.0f, 1.0f)]
  public float Bounce;

  float3 _lastFrameVelocity = 0;
  float3 _lastFramePosition = 0;

  void FixedUpdate()
  {
    if (!AutoUpdate) return;
    if (_lastFrameVelocity.Equals(0)) _lastFramePosition = transform.position;

    transform.position = UpdatePosition();
    _lastFramePosition = transform.position;
  }

  float3 CalculateAccelerate()
  {
    var accelerate = ConstantForce / mass;
    if (UseGravity) return accelerate + gravity;
    return accelerate;
  }

  float3 CalculateVelocity()
  {
    var accelerate = CalculateAccelerate();
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
    var currentVelocity = CalculateVelocity();
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

  void OnTriggerEnter(Collider other)
  {
    var e = Bounce;
    var v_after
      = _lastFrameVelocity - (1 + e) *
      (float3)math.dot(_lastFrameVelocity, other.transform.up) * other.transform.up;
    _lastFrameVelocity = v_after;
  }

  void OnTriggerStay(Collider other)
  {
    var vn = math.dot(_lastFrameVelocity, other.transform.up);
    if (vn < 0) // only cancel downward velocity
    {
      var normalVelocity = vn * (float3)other.transform.up;
      _lastFrameVelocity -= normalVelocity;

      transform.position -= (Vector3)normalVelocity * Time.fixedDeltaTime;
    }
  }
}
