using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using System.Reflection;
using Quantum;
using UnityEngine;
using Quantum.Editor;

[CustomEditor(typeof(EntityView), true)]
public class EntityViewEditor : Editor {

  private StateInspector.GUILayoutDrawer _stateInspector = new StateInspector.GUILayoutDrawer();

  private delegate Texture2D GetHelpIconDelegate(MessageType type);
  private static readonly GetHelpIconDelegate GetHelpIcon = typeof(EditorGUIUtility).CreateMethodDelegate<GetHelpIconDelegate>(nameof(GetHelpIcon), BindingFlags.NonPublic | BindingFlags.Static);

  public override unsafe void OnInspectorGUI() {

    if (!QuantumEditorSettings.Instance.UseQuantumAssetInspector) {
      base.OnInspectorGUI();
      return;
    }

    var target = (EntityView)base.target;
    CustomEditorsHelper.DrawScript(target);

    if (!EditorApplication.isPlaying) {
      bool isOnScene = target.gameObject.scene.IsValid() && PrefabStageUtility.GetPrefabStage(target.gameObject) == null;

      if (isOnScene) {
        bool hasPrototype = target.gameObject.GetComponent<EntityPrototype>();
        if (!hasPrototype) {
          using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
            EditorGUILayout.HelpBox($"This {nameof(EntityView)} will never be bound to any Entity. Add {nameof(EntityPrototype)} and bake map data.", MessageType.Warning);
            if (GUILayout.Button("Fix")) {
              Undo.AddComponent<EntityPrototype>(target.gameObject);
            }
          }
        }
      }
    }

    if (AssetDatabase.IsMainAsset(target.gameObject)) {
      if (NestedAssetBaseEditor.GetNested(target, out EntityViewAsset asset)) {
        using (new CustomEditorsHelper.BoxScope(nameof(EntityViewAsset))) {
          CustomEditorsHelper.DrawDefaultInspector(new SerializedObject(asset), "Settings", new[] { "Settings.Container" }, false);
        }
      }
    }

    CustomEditorsHelper.DrawDefaultInspector(serializedObject, new[] { "m_Script" } );

    if (QuantumRunner.Default == null)
      return;

    using (new EditorGUILayout.HorizontalScope()) {
      EditorGUILayout.PrefixLabel("Quantum Entity Id");
      EditorGUILayout.SelectableLabel(target.EntityRef.ToString(), EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
    }

    using (new GUILayout.VerticalScope()) {
      if (QuantumRunner.Default.IsRunning) {
        _stateInspector.DrawEntity(QuantumRunner.Default.Game.Frames.Predicted, target.EntityRef, "Quantum Entity Root");
      }
    }
  }
}
