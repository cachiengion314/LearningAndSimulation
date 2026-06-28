using UnityEngine;

namespace NavMeshAgentAI
{
  public class Spawner : MonoBehaviour
  {
    public GameObject patientPrefab;
    public int numPatients;

    void Start()
    {
      for (int i = 0; i < numPatients; ++i)
      {
        Instantiate(patientPrefab, transform.position, Quaternion.identity);
      }

      Invoke("SpawnPatient", 5);
    }

    void SpawnPatient()
    {
      Instantiate(patientPrefab, transform.position, Quaternion.identity);
      Invoke("SpawnPatient", Random.Range(2, 10));
    }
  }
}

