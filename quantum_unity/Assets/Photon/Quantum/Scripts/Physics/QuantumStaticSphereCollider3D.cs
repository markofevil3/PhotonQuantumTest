using Photon.Deterministic;
using Quantum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuantumStaticSphereCollider3D : MonoBehaviour {
  public FP Radius;
  public QuantumStaticColliderSettings Settings;

  void OnDrawGizmos() {
    DrawGizmo(false);
  }

  void OnDrawGizmosSelected() {
    DrawGizmo(true);
  }

  void DrawGizmo(Boolean selected) {
    // the radius with which the sphere with be baked into the map
    var radius = Radius.AsFloat * transform.localScale.x;
    
    GizmoUtils.DrawGizmosSphere(transform.position, radius, selected, QuantumEditorSettings.Instance.StaticColliderColor);
  }
}
