using UnityEngine;
using Unity.Mathematics;
using DG.Tweening;

public class Movementum : MonoBehaviour
{
  [Header("Settings")]
  [Tooltip("Enable if you want movement of the obj itself is auto controlled by component's algorithms")]
  [SerializeField] float3 gravity;
  [SerializeField][Range(.01f, 100)] float mass = 1;
  public float Mass { get { return mass; } }
  public bool AutoUpdate;
  public bool UseGravity;
  [Header("Physic's material")]
  [Range(.0f, 1.0f)]
  public float Bounciness;
  [Range(.0f, 1.0f)]
  public float StaticFriction;
  [Range(.0f, 1.0f)]
  public float KineticFriction;
  /// <summary>
  /// internal physic's datas
  /// </summary>
  float3 _instantForce;
  float3 _accelerate;
  float3 _lastFrameVelocity;
  float3 _lastFramePosition;
  [SerializeField] Transform _pivotPosition;

  void Update()
  {
    /// TEST
    if (Input.GetKeyDown(KeyCode.Alpha1))
      ZRotateAboutPivot(math.PI / 4f, _pivotPosition.position);
    if (Input.GetKeyDown(KeyCode.Alpha2))
      ZRotateAboutPivot(-math.PI / 4f, _pivotPosition.position);
  }

  void ZRotateAboutPivot(float rotRad, float3 pivotPosition)
  {
    /// Move along with tangent direction each press
    var lastFramePos = (float3)transform.position;
    var r = lastFramePos - pivotPosition;
    var x = math.cos(rotRad) * r.x - math.sin(rotRad) * r.y;
    var y = math.sin(rotRad) * r.x + math.cos(rotRad) * r.y;
    var targetRPos = new float3(x, y, 0);
    var targetPos = pivotPosition + targetRPos;
    transform.position = targetPos;
    /// Rotate each press
    var targetZRad = rotRad / 2f;
    var targetQuad = new Quaternion(0, 0, math.sin(targetZRad), math.cos(targetZRad));
    transform.rotation *= targetQuad;
  }

  void FixedUpdate()
  {
    if (!AutoUpdate) return;

    _lastFramePosition = transform.position;
    transform.position = UpdatePosition();
  }

  float3 CalculateAccelerate()
  {
    var gravityForce = gravity * mass;
    if (!UseGravity) gravityForce = 0;
    var totalForce = _instantForce + gravityForce;
    _accelerate = totalForce / mass;
    _instantForce = .0f;
    return _accelerate;
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

  float3 CalculateContactNormalFrom(Collider other)
  {
    var n = (float3)other.transform.up;
    return n;
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


  public void AddInstantForce(float3 externalForce)
  {
    _instantForce += externalForce;
  }

  public void ApplyVelocity(float3 velocity)
  {
    _lastFrameVelocity = velocity;
  }

  void OnTriggerEnter(Collider other)
  {
    var e = Bounciness;
    var n = CalculateContactNormalFrom(other);
    var v_before = _lastFrameVelocity;

    // --- restitution impulse ---
    /// v_after = v_before - k * vn * n where k = 1 + e
    var v_after = v_before - (1 + e) * (float3)math.dot(v_before, n) * n;
    _lastFrameVelocity = v_after;
  }

  /// <summary>
  /// When the obj is finished its translation in FixedUpdate function,
  /// the callback OnTriggerStay will invoke right after that
  /// </summary>
  /// <param name="other"></param>
  void OnTriggerStay(Collider other)
  {
    var n = CalculateContactNormalFrom(other);
    var mu_s = StaticFriction;
    var mu_k = KineticFriction;
    var g = gravity;

    var vn = math.dot(_lastFrameVelocity, n);
    var normalVelocity = vn * n;
    var vt = _lastFrameVelocity - normalVelocity;

    if (vn < 0) // vn < 0 duo to the gravity force
    {
      var gn = math.dot(g, n);
      var normalAccelerate = gn * n;
      var tangentAccelerate = g - normalAccelerate;
      var Gt = mass * tangentAccelerate;
      var Gn = normalAccelerate * mass;

      AddInstantForce(-Gn); // cancel normal gravity force to prevent penetrating
      _lastFrameVelocity -= normalVelocity;
      transform.position -= (Vector3)normalVelocity * Time.fixedDeltaTime;

      if (math.lengthsq(tangentAccelerate) > 1e-6f)
      {
        if (math.length(Gt) <= math.length(mu_s * Gn))
        {
          // --- static friction (tangent correction) ---
          // formula: Ft <= μs*Fn 
          // where Ft: tangent force, mu: coefficient, Fn: normal force
          AddInstantForce(-Gt); // cancel tangent gravity force to make obj stand still
          _lastFrameVelocity -= vt;
          transform.position -= (Vector3)vt * Time.fixedDeltaTime;
        }
        else
        {
          // --- kinetic friction ---
          // original formula: fk = μk*Fn
          // therefore we have vector formula
          // fk = μk*length(Gn)*normalize(Gt)
          // noticed that μk <= μs as always
          mu_k = math.min(mu_k, mu_s);
          var fkLength = mu_k * math.length(Gn);
          var fk = -1 * fkLength * math.normalizesafe(Gt);
          AddInstantForce(fk);
        }
      }
    }
  }
}
