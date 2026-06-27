using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
  RaycastHit hitInfo = new RaycastHit();
  NavMeshAgent agent;

  void Start()
  {
    agent = GetComponent<NavMeshAgent>();
  }

  void Update()
  {
    if (Input.GetMouseButtonDown(0))
    {
      var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray.origin, ray.direction, out hitInfo))
      {
        agent.destination = hitInfo.point;
      }
    }
  }
}
