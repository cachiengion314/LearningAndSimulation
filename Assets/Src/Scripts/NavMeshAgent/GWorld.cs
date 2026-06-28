using UnityEngine;
using System.Collections.Generic;

public class GWorld : MonoBehaviour
{
  static GWorld _instance;
  public static GWorld Instance
  {
    get { return _instance; }
  }
  WorldStates world;
  GWorld()
  {
    world = new WorldStates();
    patients = new Queue<GameObject>();
  }
  Queue<GameObject> patients;

  void Awake()
  {
    if (_instance == null)
      _instance = new GWorld();
    else
      Destroy(gameObject);
  }

  public void AddPatient(GameObject p)
  {
    patients.Enqueue(p);
  }

  public GameObject RemovePatient()
  {
    if (patients.Count == 0) return null;
    return patients.Dequeue();
  }

  public WorldStates GetWorld()
  {
    return world;
  }
}
