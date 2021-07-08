using Quantum;
using System;
using Photon.Deterministic;
using UnityEngine;

public class QuantumStaticMeshCollider3D : MonoBehaviour {
  public Mesh                                     Mesh;
  public Quantum.MapStaticCollider3D.MutableModes Mode;

  public QuantumStaticColliderSettings Settings;

  [Header("Experimental")]
  public Boolean SmoothSphereMeshCollisions = false;

  [NonSerialized]
  public TriangleCCW[] Triangles;

  void Reset() {
    // default to mesh collider
    var meshCollider = GetComponent<MeshCollider>();
    if (meshCollider) {
      Mesh = meshCollider.sharedMesh;
    }

    // try mesh filter
    else {
      var meshFilter = GetComponent<MeshFilter>();
      if (meshFilter) {
        Mesh = meshFilter.sharedMesh;
      }
    }
  }

  public bool Bake(Int32 index) {
    FPMathUtils.LoadLookupTables(false);

    if (!Mesh) {
      Reset();

      if (!Mesh) {
        // log warning
        Debug.LogWarning($"No mesh for static mesh collider selected on {gameObject.name}");

        // clear triangles and return
        Triangles = new TriangleCCW[0];

        // don't do anything else
        return false;
      }
    }

    var localToWorld = transform.localToWorldMatrix;
    var degenerateCount = 0;
    var triIndex = 0;

    Triangles = new TriangleCCW[Mesh.triangles.Length / 3];

    for (int i = 0; i < Mesh.triangles.Length; i += 3) {
      TriangleCCW tri = new TriangleCCW();

      var vertexA = Mesh.triangles[i];
      var vertexB = Mesh.triangles[i + 1];
      var vertexC = Mesh.triangles[i + 2];

      tri.C = localToWorld.MultiplyPoint(Mesh.vertices[vertexA]).ToFPVector3();
      tri.B = localToWorld.MultiplyPoint(Mesh.vertices[vertexB]).ToFPVector3();
      tri.A = localToWorld.MultiplyPoint(Mesh.vertices[vertexC]).ToFPVector3();

      tri.ComputeNormal();

      if (tri.Normal == default(FPVector3)) {
        degenerateCount++;
        Debug.LogWarning($"Degenerate triangle on mesh {gameObject.name}");
      } else {
        tri.StaticDataIndex = index;
        Triangles[triIndex++] = tri;
      }
    }
    
    if (degenerateCount > 0) {
      Array.Resize(ref Triangles, triIndex);
    }

#if UNITY_EDITOR
    UnityEditor.EditorUtility.SetDirty(this);
#endif
    return Triangles.Length > 0;
  }
}