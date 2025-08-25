using Unity.Mathematics;
using UnityEngine;

public class VectorFieldParticles : MonoBehaviour
{
  public ParticleSystem ps;
  public float speed = 1f;

  private ParticleSystem.Particle[] particles;

  void FixedUpdate()
  {
    if (ps == null) return;

    // Ensure array is large enough
    if (particles == null || particles.Length < ps.main.maxParticles)
      particles = new ParticleSystem.Particle[ps.main.maxParticles];

    int count = ps.GetParticles(particles);

    for (int i = 0; i < count; i++)
    {
      Vector3 pos = particles[i].position;
      float x = pos.x;
      float y = pos.y;

      var v = -new float3(x, y, 0) / math.pow(math.length(pos), 2) + 1 * new float3(-y, x, 0) / math.length(pos);
      Vector3 velocity = v * speed;

      particles[i].position += velocity * Time.deltaTime;
    }

    // Write changes back
    ps.SetParticles(particles, count);
  }
}
