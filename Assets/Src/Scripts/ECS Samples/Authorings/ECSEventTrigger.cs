using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ECSEventTrigger : MonoBehaviour
{
  public static ECSEventTrigger Instance { get; private set; }

  void Start()
  {
    if (Instance == null)
      Instance = this;
    else Destroy(gameObject);
  }

  public void AddTouchDownEvent(float3 pos)
  {
    var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

    using var query = entityManager
      .CreateEntityQuery(ComponentType.ReadWrite<TouchDownEvent>());
    if (query.IsEmpty) return;

    var eventEntity = query.GetSingletonEntity();
    var buffer = entityManager.GetBuffer<TouchDownEvent>(eventEntity);

    buffer.Add(new TouchDownEvent
    {
      Position = pos
    });
  }
}
