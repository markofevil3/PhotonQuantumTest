using Photon.Deterministic;
using Quantum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuantumStaticBoxCollider3D : MonoBehaviour {
  public FPVector3 Size;
  public QuantumStaticColliderSettings Settings;

  void OnDrawGizmos() {
    DrawGizmo(false);
  }

  void OnDrawGizmosSelected() {
    DrawGizmo(true);
  }

  void DrawGizmo(Boolean selected) {
    var t = transform;
    var matrix = Matrix4x4.TRS(t.position, t.rotation, t.localScale);
    GizmoUtils.DrawGizmosBox(matrix, Size.ToUnityVector3(), selected, QuantumEditorSettings.Instance.StaticColliderColor);
  }
}
