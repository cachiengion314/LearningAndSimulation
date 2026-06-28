using UnityEngine;
using UnityEngine.UI;

public class UpdateWorld : MonoBehaviour
{
  public Text states;

  // Update is called once per frame
  void LateUpdate()
  {
    var worldstates = GWorld.Instance.GetWorld().GetStates();
    states.text = "";
    foreach (var s in worldstates)
    {
      states.text += s.Key + ", " + s.Value + "\n";
    }
  }
}
