using UnityEngine;
using System.Collections.Generic;

public class GInventory
{
  public List<GameObject> items = new();

  public void AddItem(GameObject i)
  {
    items.Add(i);
  }

  public GameObject FindItemWithTag(string tag)
  {
    foreach (GameObject i in items)
    {
      if (i.tag == tag)
      {
        return i;
      }
    }
    return null;
  }

  public void RemoveItem(GameObject i)
  {
    var indexToRemove = -1;
    foreach (var g in items)
    {
      indexToRemove++;
      if (g == i) break;
    }
    if (indexToRemove >= -1)
      items.RemoveAt(indexToRemove);
  }
}
