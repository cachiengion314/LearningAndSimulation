using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CheckInternet : MonoBehaviour
{
  public static CheckInternet Instance { get; private set; }

  private void Start()
  {
    Instance = this;
  }

  static public bool HasNetwork()
  {
    //Check if the device cannot reach the internet at all (that means if the "cable", "WiFi", etc. is connected or not)
    //if not, don't waste your time.
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
      return false;
    }
    return true;
  }

  public void CheckInternetConnection(Action failAction = null, Action successAction = null)
  {
    if (!HasNetwork())
    {
      // GameManager.Instance.ShowNotify("No internet connection");
      return;
    }
    StartCoroutine(IECheckInternetConnection((isConnected) =>
     {
       if (isConnected)
       {
         successAction?.Invoke();
       }
       else
       {
         failAction?.Invoke();
       }
     }));
  }

  IEnumerator IECheckInternetConnection(Action<bool> action)
  {
    UnityWebRequest request = new("https://google.com");
    yield return request.SendWebRequest();
    if (request.error != null)
    {
      Debug.Log("CheckInternetConnection.Error");
      // GameManager.Instance.ShowNotify("No internet connection");
      action(false);
    }
    else
    {
      Debug.Log("CheckInternetConnection.Success");
      action(true);
    }
  }
}