using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct GridNode
{
  public int Index;
  public int ParentIndex;
  public int2 GridPosition;
  public float HCost;
  public float GCost;
  public float FCost { get { return HCost + GCost; } }
}

/// <summary>
/// PathFinding
/// </summary>
public partial class GridWorld
{
  [Header("Path Finding")]
  NativeArray<GridNode> _nodeMap;
  public NativeArray<GridNode> NodeMap { get { return _nodeMap; } }

  public void BakingPathFinding()
  {
    _nodeMap = new NativeArray<GridNode>(GridResolution.x * GridResolution.y, Allocator.Persistent);
    for (int x = 0; x < GridResolution.x; ++x)
    {
      for (int y = 0; y < GridResolution.y; ++y)
      {
        var gridPos = new int2(x, y);
        var index = MapGridToIndex(gridPos);
        var node = new GridNode()
        {
          Index = index,
          ParentIndex = -1,
          GridPosition = gridPos,
        };

        _nodeMap[index] = node;
      }
    }
  }

  public void DisposePathFinding()
  {
    _nodeMap.Dispose();
  }

  /// <summary>
  /// excludes is an array that contain all of positions that is not consider 
  /// as obstacles. Use it to disable some obstacles temporary
  /// </summary>
  /// <param name="destination"></param>
  /// <param name="startPos"></param>
  /// <param name="excludes"></param>
  /// <returns></returns>
  public NativeList<int2> PathFindingTo(
    in float3 destination,
    in float3 startPos,
    in NativeArray<int2> excludes
  )
  {
    ResetPathFindingNodeData();

    using var availableNodes = new NativeList<GridNode>(_nodeMap.Length, Allocator.Temp);
    using var visitedNodes = new NativeList<GridNode>(_nodeMap.Length, Allocator.Temp);

    if (IsPosOutsideAt(destination)) return new NativeList<int2>(0, Allocator.Temp);
    if (IsPosOutsideAt(startPos)) return new NativeList<int2>(0, Allocator.Temp);

    var destNode = _nodeMap[MapWorldToIndex(destination)];
    var startNode = _nodeMap[MapWorldToIndex(startPos)];
    availableNodes.Add(startNode);

    while (availableNodes.Length > 0)
    {
      // standing in a suitable node
      var currentNode = availableNodes[^1];
      var currentIndex = availableNodes.Length - 1;
      for (int i = 0; i < availableNodes.Length; ++i)
      {
        if (availableNodes[i].FCost < currentNode.FCost)
        {
          currentNode = availableNodes[i];
          currentIndex = i;
        }
      }
      availableNodes.RemoveAt(currentIndex);
      visitedNodes.Add(currentNode);

      var neighbors = FindNeighborNodesFor(currentNode, excludes);
      for (int i = 0; i < neighbors.Length; ++i)
      {
        // we should skip the neighbor node that has already placed in visitedNodes
        var hasVisited = false;
        for (int j = 0; j < visitedNodes.Length; ++j)
        {
          if (neighbors[i].Index != visitedNodes[j].Index) continue;
          hasVisited = true;
          break;
        }
        for (int j = 0; j < availableNodes.Length; ++j)
        {
          if (availableNodes[j].Index != neighbors[i].Index) continue;
          hasVisited = true;
          break;
        }
        if (hasVisited) continue;

        var distanceToDest = math.distancesq(destNode.GridPosition, neighbors[i].GridPosition);
        var distanceFromParent = math.distancesq(neighbors[i].GridPosition, currentNode.GridPosition);
        // update new value for neighbor node
        var neighborData = _nodeMap[neighbors[i].Index];
        neighborData.Index = neighbors[i].Index;
        neighborData.ParentIndex = currentNode.Index;
        neighborData.GridPosition = neighbors[i].GridPosition;
        neighborData.HCost = distanceToDest;
        neighborData.GCost = currentNode.GCost + distanceFromParent;

        neighbors[i] = neighborData;
        _nodeMap[neighborData.Index] = neighborData;

        if (distanceToDest == 0) return FindTracingPathFor(neighborData);

        availableNodes.Add(neighborData);
      }

      neighbors.Dispose();
    }

    return new NativeList<int2>(0, Allocator.Temp);
  }

  public void ResetPathFindingNodeData()
  {
    for (int x = 0; x < GridResolution.x; ++x)
    {
      for (int y = 0; y < GridResolution.y; ++y)
      {
        var gridPos = new int2(x, y);
        var index = MapGridToIndex(gridPos);
        var nodeData = _nodeMap[index];
        nodeData.Index = index;
        nodeData.ParentIndex = -1;
        nodeData.GridPosition = gridPos;
        nodeData.HCost = 0;
        nodeData.GCost = 0;

        _nodeMap[index] = nodeData;
      }
    }
  }

  NativeList<int2> FindTracingPathFor(in GridNode node)
  {
    var maxLength = 1024;
    var listCapacity = 32;
    using NativeList<int2> foundList = new(listCapacity, Allocator.Temp)
    {
      node.GridPosition
    };
    if (node.ParentIndex == -1) return foundList;

    var parentNode = _nodeMap[node.ParentIndex];
    var count = 0;
    while (
      parentNode.Index < _nodeMap.Length && parentNode.Index > -1 ||
      count > maxLength
    )
    {
      foundList.Add(parentNode.GridPosition);
      if (parentNode.ParentIndex == -1) break;
      parentNode = _nodeMap[parentNode.ParentIndex];
      count++;
      if (count > maxLength) print("FindTracingPaths function reach its max capacity!");
    }

    NativeList<int2> foundListReserved = new(listCapacity, Allocator.Temp);
    for (int i = foundList.Length - 1; i >= 0; --i) foundListReserved.Add(foundList[i]);

    return foundListReserved;
  }

  NativeList<GridNode> FindNeighborNodesFor(in GridNode node, in NativeArray<int2> excludes)
  {
    var directions = new NativeArray<int2>(4, Allocator.Temp);
    directions[0] = new(1, 0);
    directions[1] = new(0, 1);
    directions[2] = new(-1, 0);
    directions[3] = new(0, -1);

    NativeList<GridNode> found = new(directions.Length, Allocator.Temp);

    for (int i = 0; i < directions.Length; ++i)
    {
      var neighborPos = node.GridPosition + directions[i];
      if (IsGridPosOutsideAt(neighborPos)) continue;
      var _index = MapGridToIndex(neighborPos);
      var neighbor = _nodeMap[_index];

      if (excludes.Contains(neighborPos))
      {
        found.Add(neighbor);
        continue;
      }

      found.Add(neighbor);
    }
    directions.Dispose();
    return found;
  }
}