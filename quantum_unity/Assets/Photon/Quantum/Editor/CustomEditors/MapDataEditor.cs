using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(MapData), true)]
  public class MapDataEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() {
      var data = target as MapData;
      if (data != null) {
        // Never move the map center
        data.transform.position = Vector3.zero;

        CustomEditorsHelper.DrawScript(target);
        using (new CustomEditorsHelper.BoxScope("Map Data")) {
          CustomEditorsHelper.DrawDefaultInspector(serializedObject, new string[] {"m_Script"});

          if (data.Asset) {
            using (new EditorGUI.DisabledGroupScope(EditorApplication.isPlayingOrWillChangePlaymode)) {
              using (new CustomEditorsHelper.BackgroundColorScope(Color.green)) {
                if (GUILayout.Button("Bake Map Only", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2))) {
                  Undo.RecordObject(target, "Bake Map Only");

                  MapDataBaker.BakeMapData(data, true);

                  EditorUtility.SetDirty(data.Asset);
                  AssetDatabase.Refresh();
                  AssetDBGeneration.Generate();
                  GUIUtility.ExitGUI();
                }

                if (GUILayout.Button("Bake All", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2))) {
                  Undo.RecordObject(target, "Bake All");

                  QuantumAutoBaker.BakeMap(data, data.BakeAllMode);

                  AssetDatabase.Refresh();

                  if (data.BakeAllMode.HasFlag(QuantumMapDataBakeFlags.GenerateAssetDB)) {
                    AssetDBGeneration.Generate();
                  }

                  GUIUtility.ExitGUI();
                }
              }

              using (var checkScope = new EditorGUI.ChangeCheckScope()) {
                data.BakeAllMode = (QuantumMapDataBakeFlags)EditorGUILayout.EnumFlagsField("Bake All Mode", data.BakeAllMode);
                if (checkScope.changed) {
                  EditorUtility.SetDirty(data);
                }
              }
            }
          }
        }

        if (data.Asset) {
          // Draw map asset inspector
          if (QuantumEditorSettings.Instance.UseQuantumAssetInspector) {
            var objectEditor = CreateEditor(data.Asset, typeof(MapAssetEditor));
            objectEditor.OnInspectorGUI();
          }
          else {
            CreateEditor(data.Asset).DrawDefaultInspector();
          }
        }
      }
    }
  }
}