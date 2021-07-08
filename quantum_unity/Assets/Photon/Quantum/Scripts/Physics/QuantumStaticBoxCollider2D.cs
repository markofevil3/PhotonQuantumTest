using Photon.Deterministic;
using Quantum;
using System;
using UnityEngine;

public class QuantumStaticBoxCollider2D : MonoBehaviour {
  public FPVector2 Size;
  public FP Height;
  public QuantumStaticColliderSettings Settings;

  void OnDrawGizmos() {
    DrawGizmo(false);
  }

  void OnDrawGizmosSelected() {
    DrawGizmo(true);
  }

  void DrawGizmo(Boolean selected) {

    var size = Size.ToUnityVector3();
    var offset = Vector3.zero;

#if QUANTUM_XY
    size.z = -Height.AsFloat;
    offset.z = size.z / 2.0f;
#else
    size.y = Height.AsFloat;
    offset.y = size.y / 2.0f;
#endif

    var t = transform;
    var matrix = Matrix4x4.TRS(t.position, t.rotation, t.localScale) * Matrix4x4.Translate(offset);
    GizmoUtils.DrawGizmosBox(matrix, size, selected, QuantumEditorSettings.Instance.StaticColliderColor);
  }
}
