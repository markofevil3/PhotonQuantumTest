using System.Diagnostics;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Quantum.Editor {
  [InitializeOnLoad]
  internal static class QuantumCodeIntegrationQtnWatcher {
    static QuantumCodeIntegrationQtnWatcher() {
      EditorApplication.delayCall += () => {

        if (QuantumEditorSettings.InstanceFailSilently?.AutoRunQtnCodeGen != true)
          return;

        var solutionPath = QuantumEditorSettings.Instance.QuantumSolutionPath;

        var quantumCodePath = Path.Combine(Path.GetDirectoryName(solutionPath), "quantum.code");
        var quantumCodeProjectPath = Path.Combine(quantumCodePath, "quantum.code.csproj");
        var quantumCodegenPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(solutionPath), "../tools/codegen/quantum.codegen.host.exe"));

        var watcher = new FileSystemWatcher() {
          Path = quantumCodePath,
          Filter = "*.qtn",
          NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size,
          EnableRaisingEvents = true,
          IncludeSubdirectories = true,
        };

        bool needsRefresh = false;
        Process currentProcess = null;

        FileSystemEventHandler handler = (sender, e) => {
          needsRefresh = true;
        };

        watcher.Changed += handler;
        watcher.Created += handler;

        EditorApplication.update += () => {

          if (currentProcess != null) {
            if (currentProcess.HasExited) {
              var p = currentProcess;
              currentProcess = null;
              if (p.ExitCode != 0) {
                Debug.LogErrorFormat("Qtn compile failed: {0}", p.StandardError.ReadToEnd());
              }
            } else {
              return;
            }
          }

          if (!needsRefresh)
            return;

          needsRefresh = false;

          currentProcess = Process.Start(new ProcessStartInfo() {
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = false,
            Arguments = $"\"{quantumCodeProjectPath}\"",
            FileName = $"\"{quantumCodegenPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true
          });
        };
      };
    }
  }
}