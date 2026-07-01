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

  public Node(
    Node parent,
    float cost,
    Dictionary<string, int> allStates,
    Dictionary<string, int> beliefStates,
    GAction action
  )
  {
    this.parent = parent;
    this.cost = cost;
    state = new Dictionary<string, int>(allStates);
    foreach (var b in beliefStates)
      if (!state.ContainsKey(b.Key))
        state.Add(b.Key, b.Value);
    this.action = action;
  }
}

public class GPlanner
{
  public Queue<GAction> plan(
    /// Stored GActions scripts thats are attached to the agent instance itself
    List<GAction> actions,
    Dictionary<string, int> goal,
    /// beliefStates lets you model facts that are true for one agent but not another
    WorldStates beliefStates
  )
  {
    var usuableActions = new List<GAction>();
    foreach (GAction a in actions)
    {
      if (a.IsAchievable())
        usuableActions.Add(a);
    }

    var leaves = new List<Node>();
    var start = new Node(
      null,
      0,
      GWorld.Instance.GetWorld().GetStates(),
      beliefStates.GetStates(),
      null);

    var success = BuildGraph(start, leaves, usuableActions, goal);

    if (!success)
    {
      /// No plan
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
        result.Insert(0, n.action);
      n = n.parent;
    }

    var queue = new Queue<GAction>();
    foreach (GAction a in result)
      queue.Enqueue(a);

    return queue;
  }

  /// <summary>
  /// At the first time, collect world states as a starter points. 
  /// Now, check world states to see preConditions of every possible GActions 
  /// can be matched. If none basically nothing happen. 
  /// If yes at a GAction, then function build a currentState 
  /// that is the result of accumulating after effect states, 
  /// so we got an unified after effect states obj. And then create a new node 
  /// with combined cost (from injected node and current action cost) 
  /// and parent node is the injected one 
  /// (so we can trace to the first node in the future like A* pathfinding). 
  /// The check "if GoalAchieved" is basically a final matched 
  /// condition (its quite very alike to final condition of A* pathfinding) 
  /// so we collect this one to leaves array so we can use that array to 
  /// trace to the first node later on. And if the "if GoalAchieved" fail, 
  /// we continue recursion more into that new node by called again 
  /// BuildGraph function. But this time, we remove current action from the 
  /// usuableActions array so basically, 
  /// we iterator every single actions then if not matched the final condition, 
  /// we continue to explore more into a node that excluded that explored action. 
  /// Every actions have this kind of exploring so its quite literally a 
  /// heavy computation. 
  /// 
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