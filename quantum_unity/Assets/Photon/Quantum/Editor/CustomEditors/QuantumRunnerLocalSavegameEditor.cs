using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(QuantumRunnerLocalSavegame))]
  public class QuantumRunnerLocalSavegameEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {
      var data = (QuantumRunnerLocalSavegame)target;

      var oldSavegameFile = data.SavegameFile;

      if (DrawDefaultInspector() && oldSavegameFile != data.SavegameFile) {
        data.DatabaseFile = null;
        data.DatabasePath = string.Empty;

        if (data.SavegameFile != null && data.DatabaseFile == null) {
          var assetPath        = AssetDatabase.GetAssetPath(data.SavegameFile);
          var databaseFilepath = Path.Combine(Path.GetDirectoryName(assetPath), "db.json");
          data.DatabaseFile = AssetDatabase.LoadAssetAtPath<TextAsset>(databaseFilepath);
        }

        if (data.DatabaseFile != null) {
          var assetPath = AssetDatabase.GetAssetPath(data.DatabaseFile);
          data.DatabasePath = Path.GetDirectoryName(assetPath);
        }
        else
          data.DatabasePath = string.Empty;
      }

      // Toggle the debug runner if on the same game object.
      var debugRunner = data.gameObject.GetComponent<QuantumRunnerLocalDebug>();
      var debugReplay = data.gameObject.GetComponent<QuantumRunnerLocalReplay>();
      if (debugRunner != null) {
        GUI.backgroundColor = data.enabled ? Color.red : Color.white;
        if (GUILayout.Button(new GUIContent(data.enabled ? "Disable" : "Enable", "Enable this script and disable QuantumRunnerLocalDebug on the same game object."))) {
          debugRunner.enabled = data.enabled;
          data.enabled        = !data.enabled;
          if (debugReplay != null)
            debugReplay.enabled = false;
        }

        GUI.backgroundColor = Color.white;
      }
    }
  }
}