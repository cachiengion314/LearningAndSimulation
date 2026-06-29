using UnityEngine;

public class Nurse : GAgent
{
  public new void Start()
  {
    base.Start();
    SubGoal s1 = new SubGoal("treatPatient", 1, false);
    goals.Add(s1, 3);

    SubGoal s2 = new SubGoal("rested", 1, false);
    goals.Add(s2, 1);

    Invoke("GetTired", Random.Range(20, 40));
  }

  void GetTired()
  {
    beliefs.ModifyState("exhausted", 0);
    Invoke("GetTired", Random.Range(20, 40));
  }
}
