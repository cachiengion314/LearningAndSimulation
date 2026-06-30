using UnityEngine;

public class GoToWaitingRoom : GAction
{
  public override bool PrePerform()
  {
    return true;
  }

  public override bool PostPerform()
  {
    GWorld.Instance.GetWorld().ModifyState("Waiting", 1);
    GWorld.Instance.GetQueue("patients").AddResource(gameObject);
    /// this patient believes atHospital: 1 (they personally 
    /// know they're at the hospital), while a different patient agent 
    /// has their own separate beliefs dict that doesn't have that key at all
    beliefs.ModifyState("atHospital", 1);
    return true;
  }
}
