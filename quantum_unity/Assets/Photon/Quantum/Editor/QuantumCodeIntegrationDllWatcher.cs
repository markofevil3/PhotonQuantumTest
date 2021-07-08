using System.IO;
using UnityEditor;

namespace Quantum.Editor {
  [InitializeOnLoad]
  internal static class QuantumCodeIntegrationDllWatcher {
    static QuantumCodeIntegrationDllWatcher() {
      EditorApplication.delayCall += () => {
        if (QuantumEditorSettings.InstanceFailSilently?.ImportQuantumLibrariesImmediately != true)
          return;

        var watcher = new FileSystemWatcher() {
          Path = "Assets/Plugins",
          Filter = "quantum.*.dll",
          NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size,
          EnableRaisingEvents = true
        };

        bool needsRefresh = false;

        FileSystemEventHandler handler = (sender, e) => needsRefresh = true;
        watcher.Changed += handler;
        watcher.Created += handler;

        EditorApplication.update += () => {
          if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

          if (!needsRefresh)
            return;
          needsRefresh = false;
          AssetDatabase.Refresh();
        };
      };
    }
  }
}