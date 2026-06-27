using Unity.Entities;
using UnityEngine;

public class ECSEventBootstrap : MonoBehaviour
{
  void Start()
  {
    print("ECSEventBootstrap ");
    var world = World.DefaultGameObjectInjectionWorld;
    var entityManager = world.EntityManager;

    // Create singleton event holder entity
    var entity = entityManager.CreateEntity();
    entityManager.AddBuffer<TouchDownEvent>(entity); // init a container of TouchDownEvent
  }
}
