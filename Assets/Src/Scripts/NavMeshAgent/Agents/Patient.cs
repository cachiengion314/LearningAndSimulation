using UnityEngine;

public class Patient : GAgent
{
  new void Start()
  {
    base.Start();
    var s1 = new SubGoal("isWaiting", 1, true);
    goals.Add(s1, 1);

    var s2 = new SubGoal("isTreated", 1, true);
    goals.Add(s2, 5);

    var s3 = new SubGoal("isHome", 1, true);
    goals.Add(s3, 2);

    var s4 = new SubGoal("relief", 1, true);
    goals.Add(s4, 3);

    Invoke("NeedRelief", Random.Range(2, 5));
  }

  void NeedRelief()
  {
    beliefs.ModifyState("bursting", 1);
    Invoke("NeedRelief", Random.Range(12, 24));
  }
}
