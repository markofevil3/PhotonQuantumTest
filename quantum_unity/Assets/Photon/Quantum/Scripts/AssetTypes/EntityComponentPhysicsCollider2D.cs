using Quantum;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class EntityComponentPhysicsCollider2D {
  public Component SourceCollider;

  private void OnValidate() {
    if (SourceCollider != null) {
      Prototype.ShapeConfig = EntityPrototypeUtils.ColliderToShape2D(transform, SourceCollider, out Prototype.IsTrigger);
      Prototype.Layer = SourceCollider.gameObject.layer;
    }
  }

  public override void Refresh() {
    if (SourceCollider != null) {
      Prototype.ShapeConfig = EntityPrototypeUtils.ColliderToShape2D(transform, SourceCollider, out Prototype.IsTrigger);
      Prototype.Layer = SourceCollider.gameObject.layer;
    }
  }

#if UNITY_EDITOR

  [ContextMenu("Migrate To EntityPrototype")]
  public void Migrate() {
    var parent = GetComponent<EntityPrototype>();
    UnityEditor.Undo.RecordObject(parent, "Migrate");
    parent.PhysicsCollider.IsEnabled = true;
    parent.PhysicsCollider.IsTrigger = Prototype.IsTrigger;
    parent.PhysicsCollider.Layer = Prototype.Layer;
    parent.PhysicsCollider.Material = Prototype.PhysicsMaterial;
    parent.PhysicsCollider.Shape2D = Prototype.ShapeConfig;
    parent.PhysicsCollider.SourceCollider = SourceCollider;
    UnityEditor.Undo.DestroyObjectImmediate(this);
  }

  public override void OnInspectorGUI(SerializedObject so, IQuantumEditorGUI editor) {
    var sourceCollider = so.FindPropertyOrThrow(nameof(EntityComponentPhysicsCollider2D.SourceCollider));

    editor.HandleMultiTypeField(sourceCollider, typeof(Collider), typeof(Collider2D));

    bool enterChildren = true;
    for (var p = so.FindPropertyOrThrow("Prototype"); p.Next(enterChildren) && p.depth >= 1; enterChildren = false) {
      if (p.name == nameof(Quantum.Prototypes.PhysicsCollider3D_Prototype.PhysicsMaterial)) {
        editor.DrawProperty(p, skipRoot: false);
      } else {
        using (new EditorGUI.DisabledGroupScope(sourceCollider.objectReferenceValue != null)) {
          editor.DrawProperty(p, skipRoot: false);
        }
      }
    }

    try {
      // sync with Unity collider, if set
      ((EntityComponentPhysicsCollider2D)so.targetObject).Refresh();
    } catch (System.Exception ex) {
      EditorGUILayout.HelpBox(ex.Message, MessageType.Error);
    }
  }

#endif
}