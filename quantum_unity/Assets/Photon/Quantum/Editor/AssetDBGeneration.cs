using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Quantum.Editor {
  public static class AssetDBGeneration {
    // Reset this when other behaviour than an immediate AssetDB is desired after creating new or changing Quantum asset guids.
    public static Action OnAssetDBInvalidated = Generate;

    [MenuItem("Quantum/Generate Asset Resources", true, 21)]
    public static bool GenerateValidation() {
      return !Application.isPlaying;
    }

    [MenuItem("Quantum/Generate Asset Resources", false, 21)]
    public static void Generate() {

      if (Application.isPlaying) {
        return;
      }

      // This part will ensure that every prefab has a nested Quantum asset
      {
        var dirtyAssets = false;
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", QuantumEditorSettings.Instance.AssetSearchPaths);
        foreach (var prefabGuid in prefabGuids) {
          var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuid));
          Debug.Assert(prefab != null);
          // TODO: ensure for each component
        }

        if (dirtyAssets) {
          AssetDatabase.SaveAssets();
        }
      }

      var allAssets = new List<AssetBase>();
      {
        var assetGuids = AssetDatabase.FindAssets("t:AssetBase", QuantumEditorSettings.Instance.AssetSearchPaths);
        foreach (var assetGuid in assetGuids.Distinct()) {
          foreach (var assetBase in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(assetGuid)).OfType<AssetBase>()) {
            allAssets.Add(assetBase);
          }
        }

        foreach (var assetBase in allAssets) {
          if (assetBase != null) {

            var assetPath = AssetDatabase.GetAssetPath(assetBase);

            // Fix invalid guid ids.
            if (!assetBase.AssetObject.Guid.IsValid) {
              var assetFileName = Path.GetFileName(assetPath);

              // Recover default config settings
              switch (assetFileName) {
                case "DefaultCharacterController2D":
                  assetBase.AssetObject.Guid = (long)DefaultAssetGuids.CharacterController2DConfig;
                  break;
                case "DefaultCharacterController3D":
                  assetBase.AssetObject.Guid = (long)DefaultAssetGuids.CharacterController3DConfig;
                  break;
                case "DefaultNavMeshAgentConfig":
                  assetBase.AssetObject.Guid = (long)DefaultAssetGuids.NavMeshAgentConfig;
                  break;
                case "DefaultPhysicsMaterial":
                  assetBase.AssetObject.Guid = (long)DefaultAssetGuids.PhysicsMaterial;
                  break;
                case "SimulationConfig":
                  assetBase.AssetObject.Guid = (long)DefaultAssetGuids.SimulationConfig;
                  break;
                default:
                  assetBase.AssetObject.Guid = AssetGuid.NewGuid();
                  break;
              }

              Debug.LogWarning($"Generated a new guid {assetBase.AssetObject.Guid} for asset at path '{assetPath}'");
              EditorUtility.SetDirty(assetBase);
            }

            // Fix invalid paths
            var correctPath = assetBase.GenerateDefaultPath(assetPath);
            if (string.IsNullOrEmpty(assetBase.AssetObject.Path) || assetBase.AssetObject.Path != correctPath) {
              assetBase.AssetObject.Path = correctPath;

              Debug.LogWarning($"Generated a new path '{assetBase.AssetObject.Path}' for asset {assetBase.AssetObject.Guid}");

              if (string.IsNullOrEmpty(assetBase.AssetObject.Path)) {
                Debug.LogError($"Asset '{assetBase.AssetObject.Guid}' is not added to the Asset DB because it does not have a valid path");
                continue;
              } else {
                EditorUtility.SetDirty(assetBase);
              }
            }
          }
        }
      }

      allAssets.Sort((a, b) => a.AssetObject.Guid.CompareTo(b.AssetObject.Guid));

      // Overwrite the resource container and add found assets
      {
        var container = AssetDatabase.LoadAssetAtPath<AssetResourceContainer>(QuantumEditorSettings.Instance.AssetResourcesPath);
        if (container == null) {
          container = ScriptableObject.CreateInstance<AssetResourceContainer>();
          AssetDatabase.CreateAsset(container, QuantumEditorSettings.Instance.AssetResourcesPath);
        }

        var guidMap = new HashSet<AssetGuid>();
        var pathMap = new HashSet<string>();
        var loaders = new List<Type>();

        foreach (var group in container.Groups) {
          group.Clear();
        }

        foreach (var asset in allAssets) {
          if (asset != null) {

            // Check for duplicates
            if (guidMap.Contains(asset.AssetObject.Guid)) {
              Debug.LogError($"Duplicated guid {asset.AssetObject.Guid} found and skipping asset at path '{asset.AssetObject.Path}'");
              continue;
            }

            if (pathMap.Contains(asset.AssetObject.Path)) {
              Debug.LogError($"Duplicated path '{asset.AssetObject.Path}' found and skipping asset with guid {asset.AssetObject.Guid}");
              continue;
            }

            guidMap.Add(asset.AssetObject.Guid);
            pathMap.Add(asset.AssetObject.Path);

            bool found = false;
            foreach (var group in container.Groups) {
              var info = group.EditorTryCreateResourceInfo(asset);
              if ( info != null ) {
                info.Guid = asset.AssetObject.Guid;
                info.Path = asset.AssetObject.Path;
                group.Add(info);
                found = true;
                break;
              }
            }

            if (!found) {
              Debug.LogError($"Failed to find a resource group for {asset.AssetObject.Identifier}. " +
                $"Make sure this asset is either in Resources, has an AssetBundle assigned, is an Addressable (if QUANTUM_ADDRESSABLES is defined) " +
                $"or add your own custom group to handle it.", asset);
              continue;
            }
          }
        }

        EditorUtility.SetDirty(container);
      }

      UnityDB.Dispose();

      Debug.Log("Rebuild Quantum Asset DB");
    }
  }
}
