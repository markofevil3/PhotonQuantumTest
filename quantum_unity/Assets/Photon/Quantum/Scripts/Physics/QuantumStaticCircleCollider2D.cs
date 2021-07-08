using Photon.Deterministic;
using Quantum;
using System;
using UnityEngine;

public class QuantumStaticCircleCollider2D : MonoBehaviour {
  public FP Radius;
  public FP Height;
  public QuantumStaticColliderSettings Settings;

  void OnDrawGizmos() {
    DrawGizmo(false);
  }

  void OnDrawGizmosSelected() {
    DrawGizmo(true);
  }

  void DrawGizmo(Boolean selected) {

    var height = Height.AsFloat;
#if QUANTUM_XY
    height *= -1.0f;
    height *= transform.localScale.z;
#else
    height *= transform.localScale.y;
#endif

    GizmoUtils.DrawGizmosCircle(transform.position, Radius.AsFloat * Mathf.Max(transform.localScale.x, transform.localScale.y), height, selected, QuantumEditorSettings.Instance.StaticColliderColor);
  }
}
