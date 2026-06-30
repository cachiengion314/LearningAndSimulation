using UnityEngine;
using System.Collections.Generic;

public class ResourceQueue
{
  public Queue<GameObject> que = new();
  public string tag;
  public string modState;
  public ResourceQueue(string t, string ms, WorldStates w)
  {
    tag = t;
    modState = ms;
    if (tag != "")
    {
      var resources = GameObject.FindGameObjectsWithTag(tag);
      foreach (var r in resources)
        que.Enqueue(r);
    }

    if (modState != "")
      w.ModifyState(modState, que.Count);
  }

  public void AddResource(GameObject r)
  {
    que.Enqueue(r);
  }

  public GameObject RemoveResource()
  {
    if (que.Count == 0) return null;
    return que.Dequeue();
  }
}

public class GWorld : MonoBehaviour
{
  static GWorld _instance;
  public static GWorld Instance
  {
    get { return _instance; }
  }
  WorldStates world;
  ResourceQueue patients;
  ResourceQueue cubicles;
  ResourceQueue offices;
  ResourceQueue toilets;
  ResourceQueue puddles;
  Dictionary<string, ResourceQueue> resources = new();

  void Awake()
  {
    if (_instance == null)
    {
      _instance = this;

      world = new WorldStates();

      patients = new ResourceQueue("", "", world);
      resources.Add("patients", patients);

      cubicles = new ResourceQueue("Cubicle", "FreeCubicles", world);
      resources.Add("cubicles", cubicles);

      offices = new ResourceQueue("Office", "FreeOffices", world);
      resources.Add("offices", offices);

      toilets = new ResourceQueue("Toilet", "FreeToilets", world);
      resources.Add("toilets", toilets);

      puddles = new ResourceQueue("Puddle", "FreePuddles", world);
      resources.Add("puddles", puddles);
    }
    else
      Destroy(gameObject);
  }

  public ResourceQueue GetQueue(string type)
  {
    return resources[type];
  }

  public WorldStates GetWorld()
  {
    return world;
  }
}
