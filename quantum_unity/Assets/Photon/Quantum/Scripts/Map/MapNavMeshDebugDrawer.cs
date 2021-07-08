using Quantum;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapNavMeshDebugDrawer : MonoBehaviour {
  public TextAsset BinaryAsset;
  public bool DrawBorders = true;
  public bool DrawLinks = true;
  public bool DrawBorderNormals;
  public bool DrawVertexNormals;
  public bool DrawTriangleNeighbors;
  public bool DrawVertexIds;
  public bool DrawTrianglesIds;

#if UNITY_EDITOR

  void OnDrawGizmosSelected() {
    if (Selection.activeGameObject != gameObject) {
      return;
    }

    if (BinaryAsset == null) {
      return;
    }

    var originalColor = Gizmos.color;

    // TODO: make caching navmesh work (e.g. MapDataBakerCallback but playmode change still breaks)
    var navmesh = new NavMesh();
    var stream = new ByteStream(BinaryAsset.bytes);
    navmesh.Serialize(stream, false);
    navmesh.Name = BinaryAsset.name;

    MapNavMesh.CreateAndDrawGizmoMesh(navmesh, NavMeshRegionMask.Default);

    if (DrawTrianglesIds || DrawLinks || DrawTriangleNeighbors) {
      for (int i = 0; i < navmesh.Triangles.Length; i++) {
        if (DrawTrianglesIds) {
          Handles.color = Color.white;
          Handles.Label(ToUnityVector3(navmesh.Triangles[i].Center), i.ToString());
        }

        if (DrawLinks) {
          for (int j = 0; j < navmesh.Triangles[i].Links.Length; j++) {
            Gizmos.color = Color.blue;
            GizmoUtils.DrawGizmoVector(navmesh.Triangles[i].Links[j].Start.ToUnityVector3(), navmesh.Triangles[i].Links[j].End.ToUnityVector3());
          }
        }

        if (DrawTriangleNeighbors) {
          var t = navmesh.Triangles[i];
          if (t.Neighbors == null)
            continue;

          foreach (var p in t.Neighbors) {
            Gizmos.color = Color.green;
            var n = navmesh.Triangles[p.Neighbor];
            Gizmos.DrawLine(ToUnityVector3(t.Center), ToUnityVector3(n.Center));
          }
        }
      }
    }


    if (DrawVertexIds || DrawVertexNormals) {
      for (int i = 0; i < navmesh.Vertices.Length; i++) {
        if (DrawVertexIds) {
          Handles.color = Color.green;
          Handles.Label(ToUnityVector3(navmesh.Vertices[i].Point), i.ToString());
        }

        if (DrawVertexNormals) {
          FPMathUtils.LoadLookupTables();
          Gizmos.color = Color.blue;
          var p = ToUnityVector3(navmesh.Vertices[i].Point);
          var normal = ToUnityVector3(NavMeshVertex.CalculateNormal(i, navmesh, new NavMeshRegionMask()));
          GizmoUtils.DrawGizmoVector(p, p + normal * 0.33f);
        }
      }
    }

    if (DrawBorders || DrawBorderNormals) {
      Gizmos.color = Color.blue;
      for (int i = 0; i < navmesh.Borders.Length; i++) {
        if (DrawBorders) {
          Gizmos.DrawLine(ToUnityVector3(navmesh.Borders[i].V0), ToUnityVector3(navmesh.Borders[i].V1));
        }

        if (DrawBorderNormals) {
          var middle = (ToUnityVector3(navmesh.Borders[i].V0) + ToUnityVector3(navmesh.Borders[i].V1)) * 0.5f;
          GizmoUtils.DrawGizmoVector(middle, middle + ToUnityVector3(navmesh.Borders[i].Normal) * 0.33f);
        }
      }
    }

    Gizmos.color = originalColor;
  }

  static Vector3 ToUnityVector3(Photon.Deterministic.FPVector3 v) {
#if QUANTUM_XY
    return new Vector3(v.X.AsFloat, v.Z.AsFloat, v.Y.AsFloat);
#else
    return v.ToUnityVector3();
#endif
  }

  static void DrawTriangle(int i, NavMesh navmesh) {
    var t = navmesh.Triangles[i];
    var vertex0 = ToUnityVector3(navmesh.Vertices[t.Vertex0].Point);
    var vertex1 = ToUnityVector3(navmesh.Vertices[t.Vertex1].Point);
    var vertex2 = ToUnityVector3(navmesh.Vertices[t.Vertex2].Point);
    Gizmos.DrawLine(vertex0, vertex1);
    Gizmos.DrawLine(vertex1, vertex2);
    Gizmos.DrawLine(vertex2, vertex0);
  }

  static void DrawTriangleMesh(int i, NavMesh navmesh, Color color) {
    var t = navmesh.Triangles[i];
    var vertex0 = ToUnityVector3(navmesh.Vertices[t.Vertex0].Point);
    var vertex1 = ToUnityVector3(navmesh.Vertices[t.Vertex1].Point);
    var vertex2 = ToUnityVector3(navmesh.Vertices[t.Vertex2].Point);
    Handles.color = color;
    Handles.lighting = true;
    Handles.DrawAAConvexPolygon(vertex0, vertex1, vertex2);

  }
#endif
}
