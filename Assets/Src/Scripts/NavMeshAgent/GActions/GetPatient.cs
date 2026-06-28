using UnityEngine;

public class GetPatient : GAction
{
  public override bool PrePerform()
  {
    target = GWorld.Instance.RemovePatient();
    if (target == null)
      return false;
    return true;
  }

  public override bool PostPerform()
  {
    return true;
  }
}
