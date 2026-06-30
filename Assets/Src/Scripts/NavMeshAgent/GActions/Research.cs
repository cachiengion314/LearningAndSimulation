using UnityEngine;

public class Research : GAction
{
  public override bool PrePerform()
  {
    target = GWorld.Instance.GetQueue("offices").RemoveResource();
    if (target == null)
      return false;

    inventory.AddItem(target);
    GWorld.Instance.GetWorld().ModifyState("FreeOffices", -1);
    return true;
  }

  public override bool PostPerform()
  {
    GWorld.Instance.GetQueue("offices").AddResource(target);
    inventory.RemoveItem(target);
    GWorld.Instance.GetWorld().ModifyState("FreeOffices", 1);
    return true;
  }
}
