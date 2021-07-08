using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace Quantum.Editor {
  public class AssetBasePostprocessor : AssetPostprocessor {

    private static int? _removeMonoBehaviourUndoGroup;
    private static int _reentryCount = 0;
    private const int MaxReentryCount = 3;

    [Flags]
    private enum ValidationResult {
      Ok,
      Dirty = 1,
      Invalidated = 2,
    }
    

    [InitializeOnLoadMethod]
    static void SetupVariantPrefabWorkarounds() {
      PrefabStage.prefabSaving += OnPrefabStageSaving;
      PrefabStage.prefabStageClosing += OnPrefabStageClosing;
    }

    static void OnPrefabStageClosing(PrefabStage stage) {

      var assetPath = stage.GetAssetPath();
      var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
      if (PrefabUtility.GetPrefabAssetType(prefabAsset) != PrefabAssetType.Variant)
        return;

      // restore references
      ValidateQuantumAsset(assetPath, ignoreVariantPrefabWorkaround: true);
    }

    static void OnPrefabStageSaving(GameObject obj) {
      var stage = PrefabStageUtility.GetCurrentPrefabStage();
      if (stage == null)
        return;

      var assetPath = stage.GetAssetPath();
      var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
      if (PrefabUtility.GetPrefabAssetType(prefabAsset) != PrefabAssetType.Variant)
        return;

      // nested assets of variant prefabs holding component references raise internal Unity error;
      // these references need to be cleared before first save
      var nestedAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<AssetBase>();
      foreach (var nestedAsset in nestedAssets) {
        if (nestedAsset is IQuantumPrefabNestedAsset == false)
          continue;
        NestedAssetBaseEditor.ClearParent(nestedAsset);
      }
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
      try {

        if (++_reentryCount > MaxReentryCount) {
          Debug.LogError("Exceeded max reentry count, possibly a bug");
          return;
        }

        if (QuantumEditorSettings.InstanceFailSilently?.UseAssetBasePostprocessor == true) {

#if UNITY_2020_1_OR_NEWER
          // this is a workaround for EditorUtility.SetDirty and AssetDatabase.SaveAssets not working nicely
          // with postprocessors; a dummy FindAssets call prior to SaveAssets seems to flush internal state and fix the issue
          AssetDatabase.FindAssets($"t:AssetThatDoesNotExist", QuantumEditorSettings.Instance.AssetSearchPaths);
#endif

          var result = ValidationResult.Ok;

          foreach (var importedAsset in importedAssets) {
            result |= ValidateQuantumAsset(importedAsset);
          }

          foreach (var movedAsset in movedAssets) {
            result |= ValidateQuantumAsset(movedAsset);
          }

          for (int i = 0; result == ValidationResult.Ok && i < deletedAssets.Length; i++) {
            if (QuantumEditorSettings.Instance.AssetSearchPaths.Any(p => deletedAssets[i].StartsWith(p))) {
              result |= ValidationResult.Invalidated;
            }
          }

#if !UNITY_2020_1_OR_NEWER
          if (result.HasFlag(ValidationResult.Dirty)) {
            AssetDatabase.SaveAssets();
          }
#endif

          if (result.HasFlag(ValidationResult.Invalidated) || result.HasFlag(ValidationResult.Dirty)) {
            AssetDBGeneration.OnAssetDBInvalidated?.Invoke();
          }

#if UNITY_2020_1_OR_NEWER
          if (result.HasFlag(ValidationResult.Dirty)) {
            AssetDatabase.SaveAssets();
          }
#endif
        }
      } finally {
        --_reentryCount;
      }
    }

    private static ValidationResult ValidateQuantumAsset(string path, bool ignoreVariantPrefabWorkaround = false) {

      var result = ValidationResult.Ok;

      for (int i = 0; i < QuantumEditorSettings.Instance.AssetSearchPaths.Length; i++) {
        if (path.StartsWith(QuantumEditorSettings.Instance.AssetSearchPaths[i])) {
          var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);


          if (mainAsset is AssetBase asset) {
            result |= ValidateAsset(asset, path);
          } else if (mainAsset is GameObject prefab) {

            if (!ignoreVariantPrefabWorkaround) {
              // there is some weirdness in how Unity handles variant prefabs; basically you can't reference any components
              // externally in that stage, or you'll get an internal error
              if (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Variant && PrefabStageUtility.GetCurrentPrefabStage()?.GetAssetPath() == path) {
                break;
              }
            }

            result |= ValidatePrefab(prefab, path);
          }

          var nestedAssets = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<AssetBase>()
            .Where(x => x != mainAsset);

          foreach (var nestedAsset in nestedAssets) {
            result |= ValidateAsset(nestedAsset, path);
          }
        }
      }

      return result;
    }

    private static ValidationResult ValidateAsset(AssetBase asset, string assetPath) {
      Debug.Assert(!string.IsNullOrEmpty(assetPath));

      var correctPath = asset.GenerateDefaultPath(assetPath);

      ValidationResult result = ValidationResult.Ok;

      if (string.IsNullOrEmpty(asset.AssetObject.Path)) {
        asset.AssetObject.Path = correctPath;
        result |= ValidationResult.Dirty;
      } else {
        if (!asset.AssetObject.Path.Equals(correctPath)) {
          // possible duplication
          var sourceAssetPath = asset.AssetObject.Path;

          // ditch everything after the separator
          var separatorIndex = sourceAssetPath.LastIndexOf(AssetBase.NestedPathSeparator);
          if (separatorIndex >= 0) {
            sourceAssetPath = sourceAssetPath.Substring(0, separatorIndex);
          }

          var wasCloned = AssetDatabase.LoadAllAssetsAtPath($"Assets/{sourceAssetPath}.asset")
            .Concat(AssetDatabase.LoadAllAssetsAtPath($"Assets/{sourceAssetPath}.prefab"))
            .OfType<AssetBase>()
            .Any(x => x.AssetObject?.Guid == asset.AssetObject.Guid);

          if (wasCloned) {
            var newGuid = AssetGuid.NewGuid();
            Debug.LogFormat(asset, "Asset {0} ({3}) appears to have been cloned, assigning new id: {1} (old id: {2})", assetPath, newGuid, asset.AssetObject.Guid, asset.GetType());
            asset.AssetObject.Guid = newGuid;
            result |= ValidationResult.Invalidated;
          }

          asset.AssetObject.Path = correctPath;
          result |= ValidationResult.Dirty;
        }
      }

      if (!asset.AssetObject.Guid.IsValid) {
        asset.AssetObject.Guid = AssetGuid.NewGuid();
        result |= ValidationResult.Dirty;
        result |= ValidationResult.Invalidated;
      }

      if (result.HasFlag(ValidationResult.Dirty)) {
        EditorUtility.SetDirty(asset);
      }

      return result;
    }

    private static ValidationResult ValidatePrefab(GameObject prefab, string prefabPath) {
      Debug.Assert(!string.IsNullOrEmpty(prefabPath));
      var result = ValidationResult.Ok;

      var existingNestedAssets = AssetDatabase.LoadAllAssetsAtPath(prefabPath).OfType<IQuantumPrefabNestedAsset>().ToList();

      foreach (var component in prefab.GetComponents<MonoBehaviour>()) {
        if (component == null) {
          Debug.LogWarning($"Asset {prefab} has a missing component", prefab);
          continue;
        }

        if ( component is IQuantumPrefabNestedAssetHost host ) {
          var nestedAssetType = host.NestedAssetType;

          if (nestedAssetType == null || nestedAssetType.IsAbstract) {
            Debug.LogError($"{component.GetType().FullName} component's NestedAssetType property is either null or abstract, unable to create.", prefab);
            continue;
          }

          if (NestedAssetBaseEditor.EnsureExists(component, nestedAssetType, out var nested, save: false)) {
            // saving will trigger the postprocessor again
            result |= ValidationResult.Dirty;
          }

          existingNestedAssets.Remove(nested);
        }
      }

      foreach (var orphaned in existingNestedAssets) {
        var obj = (AssetBase)orphaned;
        Debug.LogFormat("Deleting orphaned nested asset: {0} (in {1})", obj, prefabPath);
        if (Undo.GetCurrentGroupName() == "Remove MonoBehaviour" || _removeMonoBehaviourUndoGroup == Undo.GetCurrentGroup()) {
          // special case: when component gets removed with context menu, we want to be able to restore
          // asset with the original guid
          _removeMonoBehaviourUndoGroup = Undo.GetCurrentGroup();
          Undo.DestroyObjectImmediate(obj);
        } else {
          _removeMonoBehaviourUndoGroup = null;
          UnityEngine.Object.DestroyImmediate(obj, true);
        }
        result |= ValidationResult.Dirty;
      }

      if (result.HasFlag(ValidationResult.Dirty)) {
        EditorUtility.SetDirty(prefab);
      }

      return result;
    }
  }

  static class PrefabStageExtensions {
    public static string GetAssetPath(this PrefabStage stage) {
#if UNITY_2020_1_OR_NEWER
      return stage.assetPath;
#else
      return stage.prefabAssetPath;
#endif
    }
  }
}
