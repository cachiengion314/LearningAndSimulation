using UnityEngine;

public class Doctor : GAgent
{
  public new void Start()
  {
    base.Start();
    var s1 = new SubGoal("research", 1, false);
    goals.Add(s1, 1);

    var s2 = new SubGoal("relief", 1, false);
    goals.Add(s2, 2);

    var s3 = new SubGoal("rested", 1, false);
    goals.Add(s3, 3);

    Invoke("GetTired", Random.Range(6, 6));
    Invoke("NeedRelief", Random.Range(8, 8));
  }

  void GetTired()
  {
    beliefs.ModifyState("exhausted", 0);
    Invoke("GetTired", Random.Range(20, 40));
  }

  void NeedRelief()
  {
    beliefs.ModifyState("bursting", 1);
    Invoke("NeedRelief", Random.Range(12, 24));
  }
}
