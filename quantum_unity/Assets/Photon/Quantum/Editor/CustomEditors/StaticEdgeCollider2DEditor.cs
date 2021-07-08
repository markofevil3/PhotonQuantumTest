using Photon.Deterministic;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(QuantumStaticEdgeCollider2D))]
  public class StaticEdgeCollider2DEditor : UnityEditor.Editor {
    public static float HandlesSize = 0.075f;
    public static float DistanceToReduceHandleSize = 30.0f;

    private bool _wereToolsHidden;

    private void OnEnable() {
      _wereToolsHidden = Tools.hidden;
    }

    private void OnDisable() {
      Tools.hidden = _wereToolsHidden;
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      var collider = (QuantumStaticEdgeCollider2D)target;

      EditorGUILayout.Space();

      if (GUILayout.Button("Recenter", EditorStyles.miniButton)) {
        var center = collider.VertexA + (collider.VertexB - collider.VertexA) / 2;
        collider.VertexA -= center;
        collider.VertexB -= center;
      }
    }

    public void OnSceneGUI() {
      if (EditorApplication.isPlaying)
        return;

      Tools.hidden = _wereToolsHidden;

      DrawMovementHandles((QuantumStaticEdgeCollider2D)target);
    }

    private void DrawMovementHandles(QuantumStaticEdgeCollider2D collider) {
      var handlesColor = Handles.color;

      Handles.color = Color.white;
      Handles.matrix = collider.transform.localToWorldMatrix;

      { // vertex A
        var handleSize = HandlesSize * HandleUtility.GetHandleSize(collider.VertexA.ToUnityVector3());
        var cameraDistance = Vector3.Distance(SceneView.currentDrawingSceneView.camera.transform.position, collider.VertexA.ToUnityVector3());
        if (cameraDistance > DistanceToReduceHandleSize) {
          handleSize *= DistanceToReduceHandleSize / cameraDistance;
        }
        var newPosition = Handles.FreeMoveHandle(collider.VertexA.ToUnityVector3(), Quaternion.identity, handleSize, Vector3.zero, Handles.DotHandleCap);
        if (newPosition != collider.VertexA.ToUnityVector3()) {
          Undo.RegisterCompleteObjectUndo(collider, "Moving edge vertex");
          collider.VertexA = newPosition.ToFPVector2();
        }
      }
      
      { // vertex B
        var handleSize = HandlesSize * HandleUtility.GetHandleSize(collider.VertexB.ToUnityVector3());
        var cameraDistance = Vector3.Distance(SceneView.currentDrawingSceneView.camera.transform.position, collider.VertexB.ToUnityVector3());
        if (cameraDistance > DistanceToReduceHandleSize) {
          handleSize *= DistanceToReduceHandleSize / cameraDistance;
        }
        var newPosition = Handles.FreeMoveHandle(collider.VertexB.ToUnityVector3(), Quaternion.identity, handleSize, Vector3.zero, Handles.DotHandleCap);
        if (newPosition != collider.VertexB.ToUnityVector3()) {
          Undo.RegisterCompleteObjectUndo(collider, "Moving edge vertex");
          collider.VertexB = newPosition.ToFPVector2();
        }
      }
      
      Handles.color = handlesColor;
      Handles.matrix = Matrix4x4.identity;
    }
  }
}