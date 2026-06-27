using Unity.Mathematics;
using UnityEngine;

public class VectorFieldParticles : MonoBehaviour
{
  public ParticleSystem ps;
  public float speed = 1f;
  [Range(.01f, 50.0f)]
  [SerializeField] float cylinderRadius = 1.0f;
  private ParticleSystem.Particle[] particles;

  void FixedUpdate()
  {
    if (ps == null) return;

    // Ensure array is large enough
    if (particles == null || particles.Length < ps.main.maxParticles)
      particles = new ParticleSystem.Particle[ps.main.maxParticles];

    int count = ps.GetParticles(particles);

    ComplexFlow(count);

    // Write changes back
    ps.SetParticles(particles, count);
  }

  void GravityFlow(int count)
  {
    for (int i = 0; i < count; i++)
    {
      Vector3 pos = particles[i].position;
      float x = pos.x;
      float y = pos.y;

      var v = -new float3(x, y, 0) /
        math.pow(math.length(pos), 2) + 1 * new float3(-y, x, 0) / math.length(pos);
      Vector3 velocity = v * speed;

      particles[i].position += velocity * Time.deltaTime;
    }
  }

  /// <summary>
  /// w(z)=V(z + a^2/z)
  /// </summary>
  void ComplexFlow(int count)
  {
    for (int i = 0; i < count; i++)
    {
      Vector3 pos = particles[i].position;
      float x = pos.x;
      float y = pos.y;
      var square_length = math.pow(x, 2) + math.pow(y, 2);

      var u_0 = math.pow(cylinderRadius, 2) * (math.pow(x, 2) - math.pow(y, 2)) /
        math.pow(square_length, 2);
      var u = speed * (1 - u_0);
      var v = speed * (-1 * 2 * math.pow(cylinderRadius, 2) * x * y /
        math.pow(square_length, 2));

      var velocity = new float3(u, v, 0);

      particles[i].position += (Vector3)velocity * Time.deltaTime;
    }
  }
}
