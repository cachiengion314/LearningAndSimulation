using UnityEngine;

public class CleanUpPuddle : GAction
{
  public override bool PrePerform()
  {
    target = GWorld.Instance.GetQueue("puddles").RemoveResource();
    if (target == null)
      return false;
    inventory.AddItem(target);
    GWorld.Instance.GetWorld().ModifyState("FreePuddles", -1);
    return true;
  }

  public override bool PostPerform()
  {
    inventory.RemoveItem(target);
    Destroy(target);
  
    return true;
  }
}
