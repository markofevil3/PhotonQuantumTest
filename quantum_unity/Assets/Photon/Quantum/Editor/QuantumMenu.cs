using System;
using UnityEditor;
using UnityEngine;

namespace Quantum {
  public static class QuantumMenu {
    [MenuItem("Assets/Open Quantum Project")]
    [MenuItem("Quantum/Open Quantum Project", false, 100)]
    private static void OpenQuantumProject() {
      var path = System.IO.Path.GetFullPath(QuantumEditorSettings.Instance.QuantumSolutionPath);

      if (!System.IO.File.Exists(path)) {
        EditorUtility.DisplayDialog("Open Quantum Project", "Solution file '" + path + "' not found. Check QuantumProjectPath in your QuantumEditorSettings.", "Ok");
      }

      var uri = new Uri(path);
      Application.OpenURL(uri.AbsoluteUri);
    }
  }
}