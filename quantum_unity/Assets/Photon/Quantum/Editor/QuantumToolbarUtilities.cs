using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace Quantum.Editor {
  [InitializeOnLoad]
  public static class QuantumToolbarUtilities {

    private static ScriptableObject _toolbar;
    private static string[]         _scenePaths;
    private static string[]         _sceneNames;

    static QuantumToolbarUtilities() {
      EditorApplication.update -= Update;
      EditorApplication.update += Update; 
    }

    private static void Update() {
      if (!QuantumEditorSettings.Instance.UseQuantumToolbarUtilities) {
        return;
      }

      if (_toolbar == null) {
        Assembly editorAssembly = typeof(UnityEditor.Editor).Assembly;

        UnityEngine.Object[] toolbars = UnityEngine.Resources.FindObjectsOfTypeAll(editorAssembly.GetType("UnityEditor.Toolbar"));
        _toolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
        if (_toolbar != null) {
#if UNITY_2020_1_OR_NEWER
          var windowBackendPropertyInfo = editorAssembly.GetType("UnityEditor.GUIView").GetProperty("windowBackend", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
          var windowBackend = windowBackendPropertyInfo.GetValue(_toolbar);
          var visualTreePropertyInfo = windowBackend.GetType().GetProperty("visualTree", BindingFlags.Public| BindingFlags.Instance); 
          var visualTree = (VisualElement)visualTreePropertyInfo.GetValue(windowBackend); 
#else
          PropertyInfo  visualTreePropertyInfo = editorAssembly.GetType("UnityEditor.GUIView").GetProperty("visualTree", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
          VisualElement visualTree             = (VisualElement)visualTreePropertyInfo.GetValue(_toolbar, null);
#endif

          IMGUIContainer container = (IMGUIContainer)visualTree[0];

          FieldInfo onGUIHandlerFieldInfo = typeof(IMGUIContainer).GetField("m_OnGUIHandler", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
          Action    handler               = (Action)onGUIHandlerFieldInfo.GetValue(container);
          handler -= OnGUI;
          handler += OnGUI;
          onGUIHandlerFieldInfo.SetValue(container, handler);
        }
      }

      if (_scenePaths == null || _scenePaths.Length != EditorBuildSettings.scenes.Length) {
        List<string> scenePaths = new List<string>();
        List<string> sceneNames = new List<string>();

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
          if (scene.path == null || scene.path.StartsWith("Assets") == false)
            continue;

          string scenePath = Application.dataPath + scene.path.Substring(6);

          scenePaths.Add(scenePath);
          sceneNames.Add(Path.GetFileNameWithoutExtension(scenePath));
        }

        _scenePaths = scenePaths.ToArray();
        _sceneNames = sceneNames.ToArray();
      }
    }

    private static void OnGUI() {
      if (!QuantumEditorSettings.Instance.UseQuantumToolbarUtilities) {
        return;
      }

      using (new EditorGUI.DisabledScope(Application.isPlaying)) {
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        rect.xMin = EditorGUIUtility.currentViewWidth * 0.5f + 100.0f;
        rect.xMax = EditorGUIUtility.currentViewWidth - 350.0f;
        rect.y    = 8.0f;

        using (new GUILayout.AreaScope(rect)) {
          string sceneName  = EditorSceneManager.GetActiveScene().name;
          int    sceneIndex = -1;

          for (int i = 0; i < _sceneNames.Length; ++i) {
            if (sceneName == _sceneNames[i]) {
              sceneIndex = i;
              break;
            }
          }

          int newSceneIndex = EditorGUILayout.Popup(sceneIndex, _sceneNames, GUILayout.Width(200.0f));
          if (newSceneIndex != sceneIndex) {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
              EditorSceneManager.OpenScene(_scenePaths[newSceneIndex], OpenSceneMode.Single);
            }
          }
        }
      }
    }
  }
}
