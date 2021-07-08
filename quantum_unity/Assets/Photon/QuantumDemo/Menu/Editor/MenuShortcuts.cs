using UnityEditor;
using UnityEditor.SceneManagement;

namespace Quantum.Demo {
  public static class MenuShortcuts {
    [MenuItem("Quantum/Demo/Open Menu Scene", false, 2)]
    public static void OpenMenuScene() {
      EditorSceneManager.OpenScene("Assets/Photon/QuantumDemo/Menu/Menu.unity");
    }

    [MenuItem("Quantum/Demo/Open Auto Menu Scene", false, 2)]
    public static void OpenAutoMenuScene() {
      EditorSceneManager.OpenScene("Assets/Photon/QuantumDemo/Menu/MenuAuto.unity");
    }
  }
}