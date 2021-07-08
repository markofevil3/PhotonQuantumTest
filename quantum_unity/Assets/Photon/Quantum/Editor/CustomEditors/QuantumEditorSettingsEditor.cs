using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Quantum.Editor {
  [CustomEditor(typeof(QuantumEditorSettings), true)]
  public class QuantumEditorSettingsEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {

      CustomEditorsHelper.DrawScript(target);
      using (new CustomEditorsHelper.BoxScope("Quantum Editor Settings")) {
        CustomEditorsHelper.DrawDefaultInspector(serializedObject, new string[] {"m_Script"});

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Build Features (Current Platform Only)", EditorStyles.boldLabel);

        DrawScriptingDefineToggle(new GUIContent("Use XY as 2D Plane"), "QUANTUM_XY");

        EditorGUI.BeginChangeCheck();
        DrawScriptingDefineToggle(new GUIContent("Remote Profiler"), "QUANTUM_REMOTE_PROFILER");
        if ( EditorGUI.EndChangeCheck()) {
          // need to reimport the libs, at least in 2018.4; otherwise there'd be compile errors
          var dlls = AssetDatabase.FindAssets("LiteNetLib")
            .Select(x => AssetDatabase.GUIDToAssetPath(x))
            .Where(x => string.Equals(System.IO.Path.GetFileName(x), "LiteNetLib.dll", System.StringComparison.OrdinalIgnoreCase))
            .ToList();

          foreach (var dll in dlls) {
            AssetDatabase.ImportAsset(dll);
          }
        }

        {
          var content = EditorGUIUtility.TrTextContentWithIcon("Addressables (Experimental)", "Enables Quantum AssetDB to use addressable assets. You need to have the Addressable package installed.", MessageType.Warning);
          DrawScriptingDefineToggle(content, "QUANTUM_ADDRESSABLES");
        }
      }

      var settings = target as QuantumEditorSettings;

      // Replace slashes, trim start and end
      for (int i = 0; i < settings.AssetSearchPaths.Length; i++) {
        settings.AssetSearchPaths[i] = PathUtils.MakeSane(settings.AssetSearchPaths[i]);

        string pathRelativeToAssets;
        if (!PathUtils.MakeRelativeToFolder(settings.AssetSearchPaths[i], "Assets", out pathRelativeToAssets))
          EditorGUILayout.HelpBox($"Asset path '{settings.AssetSearchPaths[i]}' needs to be inside the Unity Assets folder.", MessageType.Error);

        if (!System.IO.Directory.Exists(System.IO.Path.Combine(Application.dataPath, pathRelativeToAssets)))
          EditorGUILayout.HelpBox($"Asset path '{settings.AssetSearchPaths[i]}' does not exist.", MessageType.Error);
      }

      settings.QuantumSolutionPath = Quantum.PathUtils.MakeSane(settings.QuantumSolutionPath);

      if (!System.IO.File.Exists(settings.QuantumSolutionPath))
        EditorGUILayout.HelpBox("Quantum solution file not found at '" + settings.QuantumSolutionPath + "'", MessageType.Error);

      if (!settings.QuantumSolutionPath.EndsWith(".sln"))
        EditorGUILayout.HelpBox("Quantum solution file path has to end with .sln", MessageType.Error);
    }

    private static void DrawScriptingDefineToggle(GUIContent label, string define) {
      var buildTarget = EditorUserBuildSettings.activeBuildTarget;
      var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
      var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';');
      bool hasProfiler = System.Array.IndexOf(defines, define) >= 0;

      EditorGUI.BeginChangeCheck();
      bool value = EditorGUILayout.Toggle(label, hasProfiler);
      if (EditorGUI.EndChangeCheck()) {
        var list = defines.ToList();
        // in case there are duplicates, make sure all get removed
        list.RemoveAll(x => x == define);
        if (value) {
          list.Add(define);
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", list));
      }
    }
  }
}