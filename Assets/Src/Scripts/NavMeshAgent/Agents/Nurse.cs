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

    var s3 = new SubGoal("relief", 1, false);
    goals.Add(s3, 2);

    Invoke("GetTired", Random.Range(20, 20));
    Invoke("NeedRelief", Random.Range(8, 8));
  }

  void GetTired()
  {
    beliefs.ModifyState("exhausted", 0);
    Invoke("GetTired", Random.Range(8, 16));
  }

  void NeedRelief()
  {
    beliefs.ModifyState("bursting", 1);
    Invoke("NeedRelief", Random.Range(12, 24));
  }
}
