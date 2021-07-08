using UnityEngine;
using Photon.Deterministic;
using System;
using System.Linq;

namespace Quantum {
  public static class GizmoUtils {
    public static readonly Color Blue = new Color(0.0f, 0.7f, 1f);
    public static Color Alpha(this Color color, Single a) { color.a = a; return color; }

    private static float GetAlphaFace(this bool selected) { return selected ? 0.5f : 0.4f; }
    private static float GetAlphaWire(this bool selected) { return selected ? 0.7f : 0.5f; }

    public static void DrawGizmosBox(Transform transform, Vector3 size) {
#if UNITY_EDITOR
      DrawGizmosBox(transform, size, false);
#endif
    }

    public static void DrawGizmosBox(Transform transform, Vector3 size, bool selected) {
#if UNITY_EDITOR
      DrawGizmosBox(transform, size, selected, Blue);
#endif
    }

    public static void DrawGizmosBox(Transform transform, Vector3 size, Color color) {
#if UNITY_EDITOR
      DrawGizmosBox(transform, size, false, color);
#endif
    }

    public static void DrawGizmosBox(Vector3 center, Vector3 size, Color color) {
#if UNITY_EDITOR
      DrawGizmosBox(center, Quaternion.identity, size, false, color);
#endif
    }

    public static void DrawGizmosBoxWire(Vector3 center, Vector3 size, Color color) {
#if UNITY_EDITOR
      DrawGizmosBoxWire(center, Quaternion.identity, size, false, color);
#endif
    }
    
    public static void DrawGizmosBox(FPVector2 center, FPVector2 size, Color color) {
#if UNITY_EDITOR
      DrawGizmosBox(center.ToUnityVector3(), Quaternion.identity, size.ToUnityVector3(), false, color);
#endif
    }

    public static void DrawGizmosBox(Transform transform, Vector3 size, bool selected, Color color) {
#if UNITY_EDITOR
      DrawGizmosBox(transform, size, selected, color, Vector3.zero);
#endif
    }

    public static void DrawGizmosBox(Transform transform, Vector3 size, bool selected, Color color, Vector3 offset) {
#if UNITY_EDITOR
      var matrix = transform.localToWorldMatrix * Matrix4x4.Translate(offset);
      DrawGizmosBox(matrix, size, selected, color);
#endif
    }

    public static void DrawGizmosBox(Vector3 center, Quaternion rotation, Vector3 size, bool selected, Color color) {
#if UNITY_EDITOR
      var matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
      DrawGizmosBox(matrix, size, selected, color);
#endif
    }
    
    public static void DrawGizmosBoxWire(Vector3 center, Quaternion rotation, Vector3 size, bool selected, Color color) {
#if UNITY_EDITOR
      var matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
      DrawGizmosBoxWire(matrix, size, selected, color);
#endif
    }

    public static void DrawGizmosBox(Matrix4x4 matrix, Vector3 size, bool selected, Color color) {
#if UNITY_EDITOR
      Gizmos.matrix = matrix;
      Gizmos.color = color.Alpha(selected.GetAlphaFace());
      Gizmos.DrawCube(Vector3.zero, size);

      Gizmos.color = color.Alpha(selected.GetAlphaWire());
      Gizmos.DrawWireCube(Vector3.zero, size);
      Gizmos.matrix = Matrix4x4.identity;
      Gizmos.color = Color.white;
#endif
    }
    
    public static void DrawGizmosBoxWire(Matrix4x4 matrix, Vector3 size, bool selected, Color color) {
#if UNITY_EDITOR
      Gizmos.matrix = matrix;
      Gizmos.color = color.Alpha(selected.GetAlphaWire());
      Gizmos.DrawWireCube(Vector3.zero, size);
      Gizmos.matrix = Matrix4x4.identity;
      Gizmos.color  = Color.white;
#endif
    }

    public static void DrawGizmosBox_NoLines(Vector3 center, Vector3 size, Color color) {
#if UNITY_EDITOR
      Gizmos.color = color;
      Gizmos.DrawCube(center, size);
      Gizmos.color = Color.white;
#endif
    }

    public static void DrawGizmosCircle(Vector3 position, Single radius) {
#if UNITY_EDITOR
      DrawGizmosCircle(position, radius, false);
#endif
    }

    public static void DrawGizmosCircle(Vector3 position, Single radius, Single height) {
#if UNITY_EDITOR
      DrawGizmosCircle(position, radius, height, false);
#endif
    }

    public static void DrawGizmosCircle(Vector3 position, Single radius, Boolean selected) {
#if UNITY_EDITOR
      DrawGizmosCircle(position, radius, selected, Blue);
#endif
    }

    public static void DrawGizmosCircle(Vector3 position, Single radius, Single height, Boolean selected) {
#if UNITY_EDITOR
      DrawGizmosCircle(position, radius, height, selected, Blue);
#endif
    }

    public static void DrawGizmosCircle(Vector3 position, Single radius, Color color) {
#if UNITY_EDITOR
      DrawGizmosCircle(position, radius, 0.0f, false, color);
#endif
    }

    public static void DrawGizmosCircle(Vector3 position, Single radius, Single height, Color color) {
#if UNITY_EDITOR
      DrawGizmosCircle(position, radius, height, false, color);
#endif
    }

    public static void DrawGizmosCircle(Vector3 position, Single radius, Boolean selected, Color color) {
#if UNITY_EDITOR
      DrawGizmosCircle(position, radius, 0.0f, selected, color);
#endif
    }

    public static void DrawGizmosCircle(Vector3 position, Single radius, Single height, Boolean selected, Color color) {
#if UNITY_EDITOR
      var s = Vector3.one;
      Quaternion rot;

#if QUANTUM_XY
      rot = Quaternion.Euler(0, 0, 0);
      s = new Vector3(radius + radius, radius + radius, height);
#else
      rot = Quaternion.Euler(-90, 0, 0);
      s = new Vector3(radius + radius, radius + radius, height);
#endif

      var mesh = height != 0.0f ? DebugMesh.CylinderMesh : DebugMesh.CircleMesh;

      Gizmos.color = color.Alpha(selected.GetAlphaFace());
      Gizmos.DrawMesh(mesh, 0, position, rot, s);

      Gizmos.color = color.Alpha(selected.GetAlphaFace()); // Wire color looks too bright for meshes
      Gizmos.DrawWireMesh(mesh, 0, position, rot, s);
      Gizmos.color = Color.white;
#endif
    }

    public static void DrawGizmosSphere(Vector3 position, Single radius, Boolean selected, Color color) {
#if UNITY_EDITOR
      Gizmos.color = color.Alpha(selected ? 1f : 0.75f);
      Gizmos.DrawSphere(position, radius);
      Gizmos.color = Color.white;
#endif
    }

    public static void DrawGizmosSphere(Vector3 position, Single radius, Color color) {
#if UNITY_EDITOR
      Gizmos.color = color;
      Gizmos.DrawSphere(position, radius);
      Gizmos.color = Color.white;
#endif
    }
    public static void DrawGizmosTriangle(Vector3 A, Vector3 B, Vector3 C, bool selected, Color color) {
#if UNITY_EDITOR
      Gizmos.color = color.Alpha(selected ? 0.5f : 0.3f);
      Gizmos.DrawLine(A, B);
      Gizmos.DrawLine(B, C);
      Gizmos.DrawLine(C, A);
      Gizmos.color = Color.white;
#endif
    }

    public static void DrawGizmoGrid(FPVector2 bottomLeft, Int32 width, Int32 height, Int32 nodeSize, Color color) {
#if UNITY_EDITOR
      DrawGizmoGrid(bottomLeft.ToUnityVector3(), width, height, nodeSize, nodeSize, color);
#endif
    }

    public static void DrawGizmoGrid(Vector3 bottomLeft, Int32 width, Int32 height, Int32 nodeSize, Color color) {
#if UNITY_EDITOR
      DrawGizmoGrid(bottomLeft, width, height, nodeSize, nodeSize, color);
#endif
    }

    public static void DrawGizmoGrid(Vector3 bottomLeft, Int32 width, Int32 height, float nodeWidth, float nodeHeight, Color color) {
#if UNITY_EDITOR
        Gizmos.color = color;

#if QUANTUM_XY
        for (Int32 z = 0; z <= height; ++z) {
            Gizmos.DrawLine(bottomLeft + new Vector3(0.0f, nodeHeight * z, 0.0f), bottomLeft + new Vector3(width * nodeWidth, nodeHeight * z, 0.0f));
        }

        for (Int32 x = 0; x <= width; ++x) {
            Gizmos.DrawLine(bottomLeft + new Vector3(nodeWidth * x, 0.0f, 0.0f), bottomLeft + new Vector3(nodeWidth * x, height * nodeHeight, 0.0f));
        }
#else
        for (Int32 z = 0; z <= height; ++z) {
            Gizmos.DrawLine(bottomLeft + new Vector3(0.0f, 0.0f, nodeHeight * z), bottomLeft + new Vector3(width * nodeWidth, 0.0f, nodeHeight * z));
        }

        for (Int32 x = 0; x <= width; ++x) {
            Gizmos.DrawLine(bottomLeft + new Vector3(nodeWidth * x, 0.0f, 0.0f), bottomLeft + new Vector3(nodeWidth * x, 0.0f, height * nodeHeight));
        }
#endif
        
        Gizmos.color = Color.white;
#endif
    }

    public static void DrawGizmoPolygon2D(Vector3 position, Quaternion rotation, FPVector2[] vertices, Single height, Color color) {
#if UNITY_EDITOR
      var matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
      DrawGizmoPolygon2D(matrix, vertices, height, false, false, color);
#endif
    }

    public static void DrawGizmoPolygon2D(Vector3 position, Quaternion rotation, FPVector2[] vertices, Single height, bool drawNormals, bool selected, Color color) {
#if UNITY_EDITOR
      var matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
      DrawGizmoPolygon2D(matrix, vertices, height, drawNormals, selected, color);
#endif
    }

    public static void DrawGizmoPolygon2D(Transform transform, FPVector2[] vertices, Single height, bool drawNormals, bool selected, Color color) {
#if UNITY_EDITOR
      var matrix = transform.localToWorldMatrix;
      DrawGizmoPolygon2D(matrix, vertices, height, drawNormals, selected, color);
#endif
    }

    public static void DrawGizmoPolygon2D(Matrix4x4 matrix, FPVector2[] vertices, Single height, bool drawNormals, bool selected, Color color) {
#if UNITY_EDITOR
      if (vertices.Length < 3) return;

      FPMathUtils.LoadLookupTables();

      color = FPVector2.IsPolygonConvex(vertices) ? color : Color.red;

      var transformedVertices = vertices.Select(x => matrix.MultiplyPoint(x.ToUnityVector3())).ToArray();
      DrawGizmoPolygon2DInternal(transformedVertices, height, drawNormals, selected, color);
#endif
    }

    private static void DrawGizmoPolygon2DInternal(Vector3[] vertices, Single height, Boolean drawNormals, Boolean selected, Color color) {
#if UNITY_EDITOR
      Gizmos.color = color.Alpha(selected.GetAlphaWire());
      UnityEditor.Handles.color = color.Alpha(selected.GetAlphaFace());
      UnityEditor.Handles.DrawAAConvexPolygon(vertices);

#if QUANTUM_XY
      var upVector = Vector3.forward;
#else
      var upVector = Vector3.up;
#endif

      if (height != 0.0f) {
        UnityEditor.Handles.matrix = Matrix4x4.Translate(upVector * height);
        UnityEditor.Handles.DrawAAConvexPolygon(vertices);
        UnityEditor.Handles.matrix = Matrix4x4.identity;
      }

      for (Int32 i = 0; i < vertices.Length; ++i) {
        var v1 = vertices[i];
        var v2 = vertices[(i + 1) % vertices.Length];

        Gizmos.DrawLine(v1, v2);

        if (height != 0.0f) {
          Gizmos.DrawLine(v1 + upVector * height, v2 + upVector * height);
          Gizmos.DrawLine(v1, v1 + upVector * height);
        }

        if (drawNormals) {
#if QUANTUM_XY
          var normal = Vector3.Cross(v2 - v1, upVector).normalized;
#else
          var normal = Vector3.Cross(v1 - v2, upVector).normalized;
#endif

          var center = Vector3.Lerp(v1, v2, 0.5f);
          DrawGizmoVector(center, center + (normal * 0.25f));
        }
      }

      Gizmos.color = UnityEditor.Handles.color = Color.white;
#endif
    }

    public static void DrawGizmoDiamond(Vector3 center, Vector2 size) {
#if UNITY_EDITOR
      var DiamondWidth = size.x * 0.5f;
      var DiamondHeight = size.y * 0.5f;

#if QUANTUM_XY
      Gizmos.DrawLine(center + Vector3.right * DiamondWidth, center + Vector3.up * DiamondHeight);
      Gizmos.DrawLine(center + Vector3.left * DiamondWidth, center + Vector3.up * DiamondHeight);
      Gizmos.DrawLine(center + Vector3.right * DiamondWidth, center + Vector3.down * DiamondHeight);
      Gizmos.DrawLine(center + Vector3.left * DiamondWidth, center + Vector3.down * DiamondHeight);
#else 
      Gizmos.DrawLine(center + Vector3.right * DiamondWidth, center + Vector3.forward * DiamondHeight);
      Gizmos.DrawLine(center + Vector3.left * DiamondWidth, center + Vector3.forward * DiamondHeight);
      Gizmos.DrawLine(center + Vector3.right * DiamondWidth, center + Vector3.back * DiamondHeight);
      Gizmos.DrawLine(center + Vector3.left * DiamondWidth, center + Vector3.back * DiamondHeight);
#endif
#endif
    }

    public static void DrawGizmoVector(Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 25.0f) {
#if UNITY_EDITOR
      Gizmos.DrawLine(start, end);

      var l = (start - end).magnitude;

      if (l < arrowHeadLength * 2) {
        arrowHeadLength = l / 2;
      }

      var d = (start - end).normalized;

      float cos = Mathf.Cos(arrowHeadAngle * Mathf.Deg2Rad);
      float sin = Mathf.Sin(arrowHeadAngle * Mathf.Deg2Rad);

      Vector3 left = Vector3.zero;
#if QUANTUM_XY
      left.x = d.x * cos - d.y * sin;
      left.y = d.x * sin + d.y * cos;
#else
      left.x = d.x * cos - d.z * sin;
      left.z = d.x * sin + d.z * cos;
#endif

      sin = -sin;

      Vector3 right = Vector3.zero;
#if QUANTUM_XY
      right.x = d.x * cos - d.y * sin;
      right.y = d.x * sin + d.y * cos;
#else
      right.x = d.x * cos - d.z * sin;
      right.z = d.x * sin + d.z * cos;
#endif

      Gizmos.DrawLine(end, end + left * arrowHeadLength);
      Gizmos.DrawLine(end, end + right * arrowHeadLength);
#endif
    }

    public static void DrawGizmosEdge(Vector3 start, Vector3 end, float height, bool selected, Color color) {
#if UNITY_EDITOR
        Gizmos.color = color;

        if (Math.Abs(height) > float.Epsilon) {
            var startToEnd = end - start;
            var edgeSize = startToEnd.magnitude;
            var size = new Vector3(edgeSize, 0);
            var center = start + startToEnd / 2;
#if QUANTUM_XY
            size.z = -height;
            center.z -= height / 2;
#else
            size.y = height;
            center.y += height / 2;
#endif
            DrawGizmosBox(center, Quaternion.FromToRotation(Vector3.right, startToEnd), size, selected, color);
        }
        else {
            Gizmos.DrawLine(start, end);
        }

        Gizmos.color = Color.white;
#endif
    }
  }
}