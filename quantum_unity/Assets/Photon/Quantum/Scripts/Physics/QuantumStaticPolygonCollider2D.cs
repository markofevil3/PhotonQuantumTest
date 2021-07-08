using Photon.Deterministic;
using UnityEngine;
using System;
using Quantum;

public class QuantumStaticPolygonCollider2D : MonoBehaviour {
  public bool BakeAsStaticEdges2D = false;
  public FPVector2[] Vertices = new FPVector2[3] {
    new FPVector2(0, 2),
    new FPVector2(-1, 0),
    new FPVector2(+1, 0)
  };
  public FP Height;
  public QuantumStaticColliderSettings Settings;

  void OnDrawGizmos() {
    DrawGizmo(false);
  }


  void OnDrawGizmosSelected() {
    DrawGizmo(true);
  }

  void DrawGizmo(Boolean selected) {
    float height = Height.AsFloat * transform.localScale.z;
#if QUANTUM_XY
    height *= -1.0f;
#endif

    var t = transform;
    var matrix = Matrix4x4.TRS(t.position, t.rotation, t.localScale);
    GizmoUtils.DrawGizmoPolygon2D(matrix, Vertices, height, selected, selected, QuantumEditorSettings.Instance.StaticColliderColor);
  }
}
