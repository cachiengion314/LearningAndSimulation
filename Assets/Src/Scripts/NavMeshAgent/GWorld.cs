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
  Queue<GameObject> patients;
  Queue<GameObject> cubicles;

  void Awake()
  {
    if (_instance == null)
    {
      _instance = this;

      world = new WorldStates();
      patients = new Queue<GameObject>();
      cubicles = new Queue<GameObject>();

      var cubes = GameObject.FindGameObjectsWithTag("Cubicle");
      foreach (var c in cubes)
        cubicles.Enqueue(c);

      if (cubes.Length > 0)
        world.ModifyState("FreeCubicle", cubes.Length);
    }
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

  public void AddCubicle(GameObject p)
  {
    cubicles.Enqueue(p);
  }

  public GameObject RemoveCubicle()
  {
    if (cubicles.Count == 0) return null;
    return cubicles.Dequeue();
  }

  public WorldStates GetWorld()
  {
    return world;
  }
}
