using UnityEngine;

public class Patient : GAgent
{
  new void Start()
  {
    base.Start();
    var s1 = new SubGoal("isWaiting", 1, true);
    goals.Add(s1, 3);

    var s2 = new SubGoal("isTreated", 1, true);
    goals.Add(s2, 5);

    var s3 = new SubGoal("isHome", 1, true);
    goals.Add(s3, 5);
  }
}
