using Photon.Deterministic;
using UnityEngine;
using System;
using Quantum;

public class QuantumStaticEdgeCollider2D : MonoBehaviour {
  public FPVector2 VertexA = new FPVector2(2, 2);
  public FPVector2 VertexB = new FPVector2(-2, -2);
  public FP Height;
  public QuantumStaticColliderSettings Settings;

  void OnDrawGizmos() {
    DrawGizmos(false);
  }


  void OnDrawGizmosSelected() {
    DrawGizmos(true);
  }

  void DrawGizmos(Boolean selected) {
    var t = transform;
    var pos = t.position;
    var scale = t.localScale;

    var start = pos + transform.rotation * Vector3.Scale(scale, VertexA.ToUnityVector3());
    var end = pos + transform.rotation * Vector3.Scale(scale, VertexB.ToUnityVector3());

    var height = Height.AsFloat;
    
#if QUANTUM_XY
      height *= scale.z;
#else
      height *= scale.y;
#endif
    
    GizmoUtils.DrawGizmosEdge(start, end, height, selected, QuantumEditorSettings.Instance.StaticColliderColor);
  }
}
