using Photon.Deterministic;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(QuantumStaticPolygonCollider2D))]
  public class StaticPolygonCollider2DEditor : UnityEditor.Editor {

    public static float ButtonOffset = 0.050f;
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

      var collider = (QuantumStaticPolygonCollider2D)target;

      EditorGUILayout.HelpBox("Press shift to activate add buttons.\nPress control to activate remove buttons.\nSet static variables like `ButtonOffset` to fine-tune the sizing to your need.", MessageType.Info);
      EditorGUILayout.Space();

      if (GUILayout.Button("Recenter", EditorStyles.miniButton))
        collider.Vertices = FPVector2.RecenterPolygon(collider.Vertices);
    }

    public void OnSceneGUI() {

      if (EditorApplication.isPlaying)
        return;

      var collider = (QuantumStaticPolygonCollider2D)base.target;

      Tools.hidden = _wereToolsHidden;

      if (Event.current.shift || Event.current.control) {
        Tools.hidden = true;
        DrawAddAndRemoveButtons(collider, Event.current.shift, Event.current.control);
      }
      else {
        DrawMovementHandles(collider);
        DrawMakeCCWButton(collider);
      }
    }

    private void AddVertex(QuantumStaticPolygonCollider2D collider, int index, FPVector2 position) {
      var newVertices = new List<FPVector2>(collider.Vertices);
      newVertices.Insert(index, position);
      Undo.RegisterCompleteObjectUndo(collider, "Adding polygon vertex");
      collider.Vertices = newVertices.ToArray();
    }

    private void RemoveVertex(QuantumStaticPolygonCollider2D collider, int index) {
      var newVertices = new List<FPVector2>(collider.Vertices);
      newVertices.RemoveAt(index);
      Undo.RegisterCompleteObjectUndo(collider, "Removing polygon vertex");
      collider.Vertices = newVertices.ToArray();
    }

    private void DrawMovementHandles(QuantumStaticPolygonCollider2D collider) {
      var isCW = FPVector2.IsClockWise(collider.Vertices);
      var handlesColor = Handles.color;
      var t = collider.transform;

      Handles.color = isCW ? Color.red : Color.white;
      Handles.matrix = Matrix4x4.TRS(t.position, t.rotation, t.localScale);

      for (int i = 0; i < collider.Vertices.Length; i++) {
        var handleSize = HandlesSize * HandleUtility.GetHandleSize(collider.Vertices[i].ToUnityVector3());
        var cameraDistance = Vector3.Distance(SceneView.currentDrawingSceneView.camera.transform.position, collider.Vertices[i].ToUnityVector3());
        if (cameraDistance > DistanceToReduceHandleSize) {
          handleSize = handleSize * (DistanceToReduceHandleSize / (cameraDistance));
        }
        var newPosition = Handles.FreeMoveHandle(collider.Vertices[i].ToUnityVector3(), Quaternion.identity, handleSize, Vector3.zero, Handles.DotHandleCap);
        if (newPosition != collider.Vertices[i].ToUnityVector3()) {
          Undo.RegisterCompleteObjectUndo(collider, "Moving polygon vertex");
          collider.Vertices[i] = newPosition.ToFPVector2();
        }
      }

      Handles.color = handlesColor;
      Handles.matrix = Matrix4x4.identity;
    }

    private void DrawMakeCCWButton(QuantumStaticPolygonCollider2D collider) {
      if (FPVector2.IsPolygonConvex(collider.Vertices) && FPVector2.IsClockWise(collider.Vertices)) {
        var center = FPVector2.CalculatePolygonCentroid(collider.Vertices);
        var view = SceneView.currentDrawingSceneView;
        var screenPos = view.camera.WorldToScreenPoint(collider.transform.position + center.ToUnityVector3());
        var size = GUI.skin.label.CalcSize(new GUIContent(" Make CCW "));
        Handles.BeginGUI();
        if (GUI.Button(new Rect(screenPos.x - size.x * 0.5f, view.position.height - screenPos.y - size.y, size.x, size.y), "Make CCW")) {
          Undo.RegisterCompleteObjectUndo(collider, "Making polygon CCW");
          FPVector2.MakeCounterClockWise(collider.Vertices);
        }
        Handles.EndGUI();
      }
    } 

    private void DrawAddAndRemoveButtons(QuantumStaticPolygonCollider2D collider, bool drawAddButton, bool drawRemoveButton) {
      var handlesColor = Handles.color;
      var t = collider.transform;
      Handles.matrix = Matrix4x4.TRS(t.position, t.rotation, t.localScale);

      for (int i = 0; i < collider.Vertices.Length; i++) {
        var facePosition_FP = (collider.Vertices[i] + collider.Vertices[(i + 1) % collider.Vertices.Length]) * FP._0_50;

        var handleSize     = HandlesSize * HandleUtility.GetHandleSize(collider.Vertices[i].ToUnityVector3());
        var cameraDistance = Vector3.Distance(SceneView.currentDrawingSceneView.camera.transform.position, collider.Vertices[i].ToUnityVector3());
        if (cameraDistance > DistanceToReduceHandleSize) {
          handleSize *= (DistanceToReduceHandleSize / (cameraDistance));
        }

        if (drawRemoveButton) {
          if (collider.Vertices.Length > 3) {

            Handles.color = Color.red;
            if (Handles.Button(collider.Vertices[i].ToUnityVector3(), Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap)) {
              RemoveVertex(collider, i);
              return;
            }
          }
        }

        if (drawAddButton) {
          Handles.color = Color.green;
          if (Handles.Button(facePosition_FP.ToUnityVector3(), Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap)) {
            AddVertex(collider, i + 1, facePosition_FP);
            return;
          }
        }
      }

      Handles.color  = handlesColor;
      Handles.matrix = Matrix4x4.identity;
    }
  }
}