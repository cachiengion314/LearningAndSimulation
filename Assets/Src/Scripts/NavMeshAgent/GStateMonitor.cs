using UnityEngine;

public class GStateMonitor : MonoBehaviour
{
  public string state;
  public float stateStrength;
  public float stateDecayRate;
  public WorldStates beliefs;
  public GameObject resourcePrefab;
  public string queueName;
  public string worldState;
  public GAction action;

  public bool stateFound;
  float initialStrength;

  void Awake()
  {
    beliefs = GetComponent<GAgent>().beliefs;
    initialStrength = stateStrength;
  }

  public void ResetStateStrength()
  {
    stateFound = false;
    stateStrength = initialStrength;
  }

  void LateUpdate()
  {
    if (!stateFound && beliefs.HasState(state))
      stateFound = true;

    if (stateFound)
    {
      stateStrength -= stateDecayRate * Time.deltaTime;
      if (stateStrength <= 0)
      {
        var location = new Vector3(
          transform.position.x,
          resourcePrefab.transform.position.y,
          transform.position.z);
        var p = Instantiate(resourcePrefab, location, resourcePrefab.transform.rotation);
        stateFound = false;
        stateStrength = initialStrength;
        beliefs.RemoveState(state);
        GWorld.Instance.GetQueue(queueName).AddResource(p);
        GWorld.Instance.GetWorld().ModifyState(worldState, 1);
      }
    }
  }
}
