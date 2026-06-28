using UnityEngine;

public class Patient : GAgent
{
  public new void Start()
  {
    base.Start();
    SubGoal s1 = new SubGoal("isWaiting", 1, true);
    goals.Add(s1, 3);
  }
}
