using UnityEditor;

[InitializeOnLoad]
public class QuantumEditorSessionShutdown {
  static QuantumEditorSessionShutdown() {
    EditorApplication.update += EditorUpdate;
  }

  static void EditorUpdate() {
    if (EditorApplication.isPlaying == false)
      QuantumRunner.ShutdownAll(true);
  }
}
