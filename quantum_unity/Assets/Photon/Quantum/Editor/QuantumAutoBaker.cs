using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Quantum.Editor {
  [InitializeOnLoad]
  internal class QuantumAutoBaker : IProcessSceneWithReport {

    private enum BuildTrigger {
      SceneSave,
      PlaymodeChange,
      Build
    }

    const int MenuItemPriority = 60;
    const string BakeMapDataAndNavMeshSuffix      = "MapData";
    const string BakeWithNavMeshSuffix            = "MapData with NavMesh";
    const string BakeWithUnityNavMeshImportSuffix = "MapData with Unity NavMesh Import";

    [MenuItem("Quantum/Bake/" + BakeMapDataAndNavMeshSuffix, false, MenuItemPriority)]
    public static void BakeCurrentScene_MapData() => BakeLoadedScenes(QuantumMapDataBakeFlags.BakeMapData | QuantumMapDataBakeFlags.GenerateAssetDB);
    [MenuItem("Quantum/Bake/" + BakeWithNavMeshSuffix, false, MenuItemPriority)]
    public static void BakeCurrentScene_NavMesh() => BakeLoadedScenes(QuantumMapDataBakeFlags.BakeMapData | QuantumMapDataBakeFlags.GenerateAssetDB | QuantumMapDataBakeFlags.BakeNavMesh);
    [MenuItem("Quantum/Bake/" + BakeWithUnityNavMeshImportSuffix, false, MenuItemPriority)]
    public static void BakeCurrentScene_ImportNavMesh() => BakeLoadedScenes(QuantumMapDataBakeFlags.BakeMapData | QuantumMapDataBakeFlags.GenerateAssetDB | QuantumMapDataBakeFlags.BakeNavMesh | QuantumMapDataBakeFlags.ImportUnityNavMesh | QuantumMapDataBakeFlags.BakeUnityNavMesh);

    [MenuItem("Quantum/Bake/All Scenes/" + BakeMapDataAndNavMeshSuffix, false, MenuItemPriority + 11)]
    public static void BakeAllScenes_MapData() => BakeAllScenes(QuantumMapDataBakeFlags.BakeMapData);
    [MenuItem("Quantum/Bake/All Scenes/" + BakeWithNavMeshSuffix, false, MenuItemPriority + 11)]
    public static void BakeAllScenes_NavMesh() => BakeAllScenes(QuantumMapDataBakeFlags.BakeMapData | QuantumMapDataBakeFlags.GenerateAssetDB | QuantumMapDataBakeFlags.BakeNavMesh);
    [MenuItem("Quantum/Bake/All Scenes/" + BakeWithUnityNavMeshImportSuffix, false, MenuItemPriority + 11)]
    public static void BakeAllScenes_ImportNavMesh() => BakeAllScenes(QuantumMapDataBakeFlags.BakeMapData | QuantumMapDataBakeFlags.GenerateAssetDB | QuantumMapDataBakeFlags.BakeNavMesh | QuantumMapDataBakeFlags.ImportUnityNavMesh | QuantumMapDataBakeFlags.BakeUnityNavMesh);

    [MenuItem("Quantum/Bake/All Enabled Scenes/" + BakeMapDataAndNavMeshSuffix, false, MenuItemPriority + 12)]
    public static void BakeEnabledScenes_MapData() => BakeEnabledScenes(QuantumMapDataBakeFlags.BakeMapData | QuantumMapDataBakeFlags.GenerateAssetDB);
    [MenuItem("Quantum/Bake/All Enabled Scenes/" + BakeWithNavMeshSuffix, false, MenuItemPriority + 12)]
    public static void BakeEnabledScenes_NavMesh() => BakeEnabledScenes(QuantumMapDataBakeFlags.BakeMapData | QuantumMapDataBakeFlags.GenerateAssetDB | QuantumMapDataBakeFlags.BakeNavMesh);
    [MenuItem("Quantum/Bake/All Enabled Scenes/" + BakeWithUnityNavMeshImportSuffix, false, MenuItemPriority + 12)]
    public static void BakeEnabledScenes_ImportNavMesh() => BakeEnabledScenes(QuantumMapDataBakeFlags.BakeMapData | QuantumMapDataBakeFlags.GenerateAssetDB | QuantumMapDataBakeFlags.BakeNavMesh | QuantumMapDataBakeFlags.ImportUnityNavMesh | QuantumMapDataBakeFlags.BakeUnityNavMesh);

    private static void BakeLoadedScenes(QuantumMapDataBakeFlags flags) {
      for (int i = 0; i < EditorSceneManager.sceneCount; ++i) {
        BakeScene(EditorSceneManager.GetSceneAt(i), flags);
      }

      if (flags.HasFlag(QuantumMapDataBakeFlags.GenerateAssetDB)) {
        AssetDBGeneration.Generate();
      }
    }

    private static void BakeAllScenes(QuantumMapDataBakeFlags flags) {
      var scenes = AssetDatabase.FindAssets("t:scene")
        .Select(x => AssetDatabase.GUIDToAssetPath(x));
      BakeScenes(scenes, flags);

      if (flags.HasFlag(QuantumMapDataBakeFlags.GenerateAssetDB)) {
        AssetDBGeneration.Generate();
      }
    }

    private static void BakeEnabledScenes(QuantumMapDataBakeFlags flags) {
      var enabledScenes = EditorBuildSettings.scenes
          .Where(x => x.enabled)
          .Select(x => x.path);
      BakeScenes(enabledScenes, flags);

      if (flags.HasFlag(QuantumMapDataBakeFlags.GenerateAssetDB)) {
        AssetDBGeneration.Generate();
      }
    }


    static QuantumAutoBaker() {
      EditorSceneManager.sceneSaving += OnSceneSaving;
      EditorApplication.playModeStateChanged += OnPlaymodeChange;
    }

    private static void OnPlaymodeChange(PlayModeStateChange change) {
      if (change != PlayModeStateChange.ExitingEditMode) {
        return;
      }
      for (int i = 0; i < EditorSceneManager.sceneCount; ++i) {
        AutoBakeMapData(EditorSceneManager.GetSceneAt(i), BuildTrigger.PlaymodeChange);
      }
    }

    private static void OnSceneSaving(Scene scene, string path) {
      AutoBakeMapData(scene, BuildTrigger.SceneSave);
    }

    private static void AutoBakeMapData(Scene scene, BuildTrigger buildTrigger) {
      var settings = QuantumEditorSettings.Instance;
      if (settings == null)
        return;

      Debug.LogFormat("Auto baking {0}", scene.path);

      switch (buildTrigger) {
        case BuildTrigger.Build:
          BakeScene(scene, settings.AutoBuildOnBuild);
          break;
        case BuildTrigger.SceneSave:
          BakeScene(scene, settings.AutoBuildOnSceneSave);
          break;
        case BuildTrigger.PlaymodeChange:
          BakeScene(scene, settings.AutoBuildOnPlaymodeChanged);
          break;
      }
    }

    private static void BakeScenes(IEnumerable<string> scenes, QuantumMapDataBakeFlags mode) {
      if (mode == QuantumMapDataBakeFlags.None)
        return;

      if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
        return;
      }

      var currentScenes = Enumerable.Range(0, EditorSceneManager.sceneCount)
        .Select(x => EditorSceneManager.GetSceneAt(x))
        .Where(x => x.IsValid())
        .Select(x => new { Path = x.path, IsLoaded = x.isLoaded })
        .ToList();

      try {
        var mapDataAssets = AssetDatabase.FindAssets($"t:{nameof(MapAsset)}")
            .Select(x => AssetDatabase.GUIDToAssetPath(x))
            .Select(x => AssetDatabase.LoadAssetAtPath<MapAsset>(x));

        var lookup = scenes
          .ToLookup(x => Path.GetFileNameWithoutExtension(x));

        foreach (var mapData in mapDataAssets) {

          var path = lookup[mapData.Settings.Scene].FirstOrDefault();
          if (string.IsNullOrEmpty(path))
            continue;

          var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
          if (!scene.IsValid())
            continue;

          BakeScene(scene, mode);
          EditorSceneManager.SaveOpenScenes();
        }
      } finally {
        var sceneLoadMode = OpenSceneMode.Single;
        foreach (var sceneInfo in currentScenes) {
          var scene = EditorSceneManager.OpenScene(sceneInfo.Path, sceneLoadMode);
          if (scene.isLoaded && !sceneInfo.IsLoaded)
            EditorSceneManager.CloseScene(scene, false);
          sceneLoadMode = OpenSceneMode.Additive;
        }
      }
    }

    private static void BakeScene(Scene scene, QuantumMapDataBakeFlags mode) {
      if (mode == QuantumMapDataBakeFlags.None)
        return;

      var mapsData = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<MapData>()).ToList();

      if (mapsData.Count == 1) {
        BakeMap(mapsData[0], mode);
      } else if (mapsData.Count > 1) {
        Debug.LogError($"There are multiple {nameof(MapData)} components on scene {scene.name}. This is not supported.");
      }

      AssetDatabase.Refresh();
      
      if (mode.HasFlag(QuantumMapDataBakeFlags.GenerateAssetDB)) {
        AssetDBGeneration.Generate();
      }
    }

    public static void BakeMap(MapData data, QuantumMapDataBakeFlags mode) {
      if (data.Asset == null)
        return;

      if (mode.HasFlag(QuantumMapDataBakeFlags.BakeMapData)) {
        MapDataBaker.BakeMapData(data, true);
      }
      
      if (mode.HasFlag(QuantumMapDataBakeFlags.BakeUnityNavMesh)) {
        foreach (var navmeshDefinition in data.GetComponentsInChildren<MapNavMeshDefinition>()) {
          if (MapNavMeshEditor.BakeUnityNavmesh(navmeshDefinition.gameObject)) {
            break;
          }
        }

        foreach (var navmesh in data.GetComponentsInChildren<MapNavMeshUnity>()) {
          if (MapNavMeshEditor.BakeUnityNavmesh(navmesh.gameObject)) {
            break;
          }
        }
      }

      MapNavMeshEditor.UpdateDefaultMinAgentRadius();

      if (mode.HasFlag(QuantumMapDataBakeFlags.ImportUnityNavMesh)) {
        foreach (var navmeshDefinition in data.GetComponentsInChildren<MapNavMeshDefinition>()) {
          MapNavMeshDefinitionEditor.ImportFromUnity(navmeshDefinition);
        }
      }

      if (mode.HasFlag(QuantumMapDataBakeFlags.BakeNavMesh)) {
        MapDataBaker.BakeNavMeshes(data, true);
      }

      EditorUtility.SetDirty(data);
      EditorUtility.SetDirty(data.Asset);
    }

    int IOrderedCallback.callbackOrder => 0;

    void IProcessSceneWithReport.OnProcessScene(Scene scene, BuildReport report) {
      if (report == null)
        return;

      AutoBakeMapData(scene, BuildTrigger.Build);
    }
  }
}