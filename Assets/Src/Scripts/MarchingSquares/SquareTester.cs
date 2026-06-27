using UnityEngine;
using System.Collections.Generic;

public class SquareTester : MonoBehaviour
{
  Vector2 topRight;
  Vector2 bottomRight;
  Vector2 bottomLeft;
  Vector2 topLeft;

  Vector2 topCenter;
  Vector2 rightCenter;
  Vector2 bottomCenter;
  Vector2 leftCenter;

  [Header("Elements")]
  [SerializeField] MeshFilter filter;

  [Header("Settings")]
  [Range(1 / 64f, 1 / 4f)]
  [SerializeField] float gizmosScale = 1 / 16f;
  [SerializeField] float isoValue;
  [SerializeField] float gridScale;
  List<Vector3> vertices = new List<Vector3>();
  List<int> triangles = new List<int>();

  [Header("Configuration")]
  [SerializeField] float topRightValue;
  [SerializeField] float bottomRightValue;
  [SerializeField] float bottomLeftValue;
  [SerializeField] float topLeftValue;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    topRight = Vector2.one / 2f * gridScale;
    bottomRight = topRight + Vector2.down * gridScale;
    bottomLeft = bottomRight + Vector2.left * gridScale;
    topLeft = bottomLeft + Vector2.up * gridScale;

    topCenter = topRight + Vector2.left * gridScale / 2f;
    rightCenter = bottomRight + Vector2.up * gridScale / 2f;
    bottomCenter = bottomLeft + Vector2.right * gridScale / 2f;
    leftCenter = topLeft + Vector2.down * gridScale / 2f;


  }

  private void Update()
  {
    vertices.Clear();
    triangles.Clear();

    var square = new Square();
    square.InitSqaure(Vector3.zero, gridScale);
    square.Triangulate(
      isoValue,
      new float[] { topRightValue, bottomRightValue, bottomLeftValue, topLeftValue }
    );

    var mesh = new Mesh
    {
      vertices = square.GetVertices(),
      triangles = square.GetTriangles()
    };
    filter.mesh = mesh;
  }

  void OnDrawGizmos()
  {
    Gizmos.color = Color.red;

    Gizmos.DrawSphere(topRight, gridScale * gizmosScale);
    Gizmos.DrawSphere(bottomRight, gridScale * gizmosScale);
    Gizmos.DrawSphere(bottomLeft, gridScale * gizmosScale);
    Gizmos.DrawSphere(topLeft, gridScale * gizmosScale);

    Gizmos.color = Color.green;
    Gizmos.DrawSphere(topCenter, gridScale * gizmosScale * .5f);
    Gizmos.DrawSphere(rightCenter, gridScale * gizmosScale * .5f);
    Gizmos.DrawSphere(bottomCenter, gridScale * gizmosScale * .5f);
    Gizmos.DrawSphere(leftCenter, gridScale * gizmosScale * .5f);
  }
}
