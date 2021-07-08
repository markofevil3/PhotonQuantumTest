using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(QuantumRunnerLocalReplay))]
  public class QuantumRunnerLocalReplayEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {
      var data = (QuantumRunnerLocalReplay)target;

      var oldReplayFile = data.ReplayFile;

      if (DrawDefaultInspector() && oldReplayFile != data.ReplayFile) {
        data.DatabaseFile = null;
        data.ChecksumFile = null;
        data.DatabasePath = string.Empty;

        if (data.ReplayFile != null && data.DatabaseFile == null) {
          var assetPath        = AssetDatabase.GetAssetPath(data.ReplayFile);
          var databaseFilepath = Path.Combine(Path.GetDirectoryName(assetPath), "db.json");
          data.DatabaseFile = AssetDatabase.LoadAssetAtPath<TextAsset>(databaseFilepath);
        }

        if (data.ReplayFile != null && data.ChecksumFile == null) {
          var assetPath        = AssetDatabase.GetAssetPath(data.ReplayFile);
          var checksumFilepath = Path.Combine(Path.GetDirectoryName(assetPath), "checksum.json");
          data.ChecksumFile = AssetDatabase.LoadAssetAtPath<TextAsset>(checksumFilepath);
        }

        if (data.DatabaseFile != null) {
          var assetPath = AssetDatabase.GetAssetPath(data.DatabaseFile);
          data.DatabasePath = Path.GetDirectoryName(assetPath);
        }
        else
          data.DatabasePath = string.Empty;
      }

      // Toggle the debug runner if on the same game object.
      var debugRunner   = data.gameObject.GetComponent<QuantumRunnerLocalDebug>();
      var debugSavegame = data.gameObject.GetComponent<QuantumRunnerLocalSavegame>();
      if (debugRunner != null) {
        GUI.backgroundColor = data.enabled ? Color.red : Color.white;
        if (GUILayout.Button(new GUIContent(data.enabled ? "Disable" : "Enable", "Enable this script and disable QuantumRunnerLocalDebug on the same game object."))) {
          debugRunner.enabled = data.enabled;
          data.enabled        = !data.enabled;
          if (debugSavegame != null)
            debugSavegame.enabled = false;
        }

        GUI.backgroundColor = Color.white;
      }
    }
  }
}