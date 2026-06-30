using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class SubGoal
{
  public Dictionary<string, int> sgoals;
  public bool remove;
  public SubGoal(string s, int i, bool r)
  {
    sgoals = new Dictionary<string, int>
    {
      { s, i }
    };
    remove = r;
  }
}

public class GAgent : MonoBehaviour
{
  /// <summary>
  /// Storing GActions scripts thats are attached to the agent instance itself
  /// </summary>
  public List<GAction> actions = new();
  public Dictionary<SubGoal, int> goals = new();
  public GInventory inventory = new();
  public WorldStates beliefs = new();
  GPlanner planner;
  Queue<GAction> actionQueue;
  public GAction currentAction;
  SubGoal currentGoal;

  public void Start()
  {
    GAction[] acts = GetComponents<GAction>();
    foreach (GAction a in acts)
      actions.Add(a);
  }

  bool invoked = false;
  void CompleteAction()
  {
    currentAction.running = false;
    currentAction.PostPerform();
    invoked = false;
  }

  void LateUpdate()
  {
    if (currentAction != null && currentAction.running)
    {
      var distanceToTarget = Vector3.Distance(
        currentAction.target.transform.position, transform.position);
      if (distanceToTarget < 2.5f)
      {
        if (!invoked)
        {
          Invoke("CompleteAction", currentAction.duration);
          invoked = true;
        }
      }
      return;
    }

    if (planner == null || actionQueue == null)
    {
      planner = new GPlanner();
      /// It tries goals highest priority first, takes the first one that's even 
      /// plannable right now, and once that goal's plan empties out 
      /// (and remove=true deletes it from goals), the next LateUpdate re-sorts and 
      /// naturally falls through to the next goal in line. 
      /// So staging into isWaiting(3) → isTreated(5) → isHome(5) 
      /// isn't really about forcing an order through priority value — it's that 
      /// each smaller goal is individually a tiny, fast, low-branching search
      /// and the sequence of goals (combined with what preconditions are actually 
      /// achievable at each point) is what enforces the causal order, not the goal list 
      /// itself. Decomposing into checkpoints turns one expensive deep search into 
      /// several cheap shallow ones — and gives you natural milestones 
      /// (PostPerform side effects) to hook gameplay logic onto along the way.
      var sortedGoals = from entry in goals orderby entry.Value descending select entry;
      foreach (var sg in sortedGoals)
      {
        actionQueue = planner.plan(actions, sg.Key.sgoals, beliefs);
        if (actionQueue != null)
        {
          currentGoal = sg.Key;
          break;
        }
      }
    }

    if (actionQueue != null && actionQueue.Count == 0)
    {
      if (currentGoal.remove)
      {
        goals.Remove(currentGoal);
      }
      planner = null;
    }

    if (actionQueue != null && actionQueue.Count > 0)
    {
      currentAction = actionQueue.Dequeue();
      if (currentAction.PrePerform())
      {
        if (currentAction.target == null && currentAction.targetTag != "")
          currentAction.target = GameObject.FindWithTag(currentAction.targetTag);

        if (currentAction.target != null)
        {
          currentAction.running = true;
          currentAction.agent.SetDestination(currentAction.target.transform.position);
        }
      }
      else
      {
        actionQueue = null;
      }
    }
  }
}