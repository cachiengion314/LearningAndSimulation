using UnityEngine;
using System.Collections.Generic;

public class Node
{
  public Node parent;
  public float cost;
  public Dictionary<string, int> state;
  public GAction action;

  public Node(Node parent, float cost, Dictionary<string, int> allStates, GAction action)
  {
    this.parent = parent;
    this.cost = cost;
    state = new Dictionary<string, int>(allStates);
    this.action = action;
  }
}

public class GPlanner
{
  public Queue<GAction> plan(
    /// Storing GActions scripts thats are attached to the agent instance itself
    List<GAction> actions,
    Dictionary<string, int> goal,
    WorldStates states
  )
  {
    var usuableActions = new List<GAction>();
    foreach (GAction a in actions)
    {
      if (a.IsAchievable())
        usuableActions.Add(a);
    }

    var leaves = new List<Node>();
    var start = new Node(null, 0, GWorld.Instance.GetWorld().GetStates(), null);

    var success = BuildGraph(start, leaves, usuableActions, goal);

    if (!success)
    {
      Debug.Log("No Plan");
      return null;
    }
    Node cheapest = null;
    foreach (Node leaf in leaves)
    {
      if (cheapest == null)
        cheapest = leaf;
      else
        if (leaf.cost < cheapest.cost)
          cheapest = leaf;
    }

    var result = new List<GAction>();
    var n = cheapest;
    while (n != null)
    {
      if (n.action != null)
      {
        result.Insert(0, n.action);
      }
      n = n.parent;
    }

    var queue = new Queue<GAction>();
    foreach (GAction a in result)
    {
      queue.Enqueue(a);
    }

    Debug.Log("The Plan is: ");
    foreach (GAction a in queue)
    {
      Debug.Log("Q: " + a.actionName);
    }

    return queue;
  }

  /// <summary>
  /// The preconditions' only job is gatekeeping ("am I allowed to run, 
  /// given what's true right now?"), checked once and then discarded. 
  /// They never get written anywhere. Only effects — the result of 
  /// running the action — get folded into the accumulating state 
  /// that carries forward to the next recursion level 
  /// and eventually gets diffed against the goal.
  /// A chain of accumulated effects, with preconditions acting only 
  /// as a yes/no gate at each step, never as data that flows forward. 
  /// That's a clean way to hold the mental model.
  /// </summary>
  private bool BuildGraph(
    Node parent,
    List<Node> leaves,
    List<GAction> usuableActions, // Storing GActions scripts thats are attached to the agent instance itself
    Dictionary<string, int> goal)
  {
    var foundPath = false;
    foreach (GAction action in usuableActions)
    {
      if (action.IsAchievableGiven(parent.state))
      {
        var currentState = new Dictionary<string, int>(parent.state);
        foreach (KeyValuePair<string, int> eff in action.effects)
        {
          if (!currentState.ContainsKey(eff.Key))
            currentState.Add(eff.Key, eff.Value);
        }
        var node = new Node(parent, parent.cost + action.cost, currentState, action);
        if (GoalAchieved(goal, currentState))
        {
          leaves.Add(node);
          foundPath = true;
        }
        else
        {
          var subset = ActionSubset(usuableActions, action);
          var found = BuildGraph(node, leaves, subset, goal);
          if (found)
            foundPath = true;
        }
      }
    }
    return foundPath;
  }

  bool GoalAchieved(Dictionary<string, int> goal, Dictionary<string, int> state)
  {
    foreach (KeyValuePair<string, int> g in goal)
    {
      if (!state.ContainsKey(g.Key)) return false;
    }
    return true;
  }

  List<GAction> ActionSubset(List<GAction> actions, GAction removeMe)
  {
    var subset = new List<GAction>();
    foreach (GAction a in actions)
    {
      if (!a.Equals(removeMe))
        subset.Add(a);
    }
    return subset;
  }
}