using UnityEngine;
using System.Collections.Generic;

public struct SquareGrid
{
  public Square[,] squares;

  List<Vector3> vertices;
  List<int> triangles;
  List<Vector2> uvs;
  float isoValue;

  public void InitSquareGrid(int size, float gridScale, float isoValue)
  {
    squares = new Square[size, size];
    vertices = new List<Vector3>();
    triangles = new List<int>();
    uvs = new List<Vector2>();

    this.isoValue = isoValue;

    for (int y = 0; y < size; ++y)
    {
      for (int x = 0; x < size; ++x)
      {
        var squarePos = new Vector2(x, y) * gridScale;
        squarePos.x -= size * gridScale / 2 - gridScale / 2;
        squarePos.y -= size * gridScale / 2 - gridScale / 2;
        squares[x, y] = new Square();
        squares[x, y].InitSqaure(squarePos, gridScale);
      }
    }
  }

  public void Update(float[,] grid)
  {
    vertices.Clear();
    triangles.Clear();
    uvs.Clear();

    var triangleStartIndex = 0;

    for (int y = 0; y < squares.GetLength(1); ++y)
    {
      for (int x = 0; x < squares.GetLength(0); ++x)
      {
        var currentSquare = squares[x, y];
        var values = new float[4];

        values[0] = grid[x + 1, y + 1];
        values[1] = grid[x + 1, y];
        values[2] = grid[x, y];
        values[3] = grid[x, y + 1];

        currentSquare.Triangulate(isoValue, values);
        vertices.AddRange(currentSquare.GetVertices());

        int[] currentSquareTriangles = currentSquare.GetTriangles();
        for (int i = 0; i < currentSquareTriangles.Length; ++i)
          currentSquareTriangles[i] += triangleStartIndex;

        triangles.AddRange(currentSquareTriangles);
        triangleStartIndex += currentSquare.GetVertices().Length;
        uvs.AddRange(currentSquare.GetUVs());
      }
    }
  }

  public Vector3[] GetVertices()
  {
    return vertices.ToArray();
  }

  public int[] GetTriangles()
  {
    return triangles.ToArray();
  }

  public Vector2[] GetUVs()
  {
    return uvs.ToArray();
  }
}