using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  public static class AssetDatabaseExt {
    public static void DeleteNestedAsset(this Object parent, Object child) {
      // destroy child
      Object.DestroyImmediate(child, true);

      // set dirty
      EditorUtility.SetDirty(parent);

      // save
      AssetDatabase.SaveAssets();
    }

    public static void DeleteAllNestedAssets(this Object parent) {
      // get path of parent object
      var path = AssetDatabase.GetAssetPath(parent);

      // LoadAllAssetsAtPath() returns the parent asset AND all of its nested (chidren)
      var assets = AssetDatabase.LoadAllAssetsAtPath(path);
      foreach (var asset in assets) {

        // keep main (parent) asset
        if (AssetDatabase.IsMainAsset(asset))
          continue;

        // delete nested assets
        parent.DeleteNestedAsset(asset);
      }
    }

    public static Object CreateNestedScriptableObjectAsset(this Object parent, System.Type type, System.String name, HideFlags hideFlags = HideFlags.None) {
      // create new asset in memory
      Object asset;

      asset = ScriptableObject.CreateInstance(type);
      asset.name = name;
      asset.hideFlags = hideFlags;

      // add to parent asset
      AssetDatabase.AddObjectToAsset(asset, parent);

      // set dirty
      EditorUtility.SetDirty(parent);

      // save
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      return asset;
    }

    public static Object FindNestedObjectParent(this Object asset) {
      var assetPath = AssetDatabase.GetAssetPath(asset);
      if (string.IsNullOrEmpty(assetPath)) {
        return null;
      }

      return AssetDatabase.LoadMainAssetAtPath(assetPath);
    }

    public static int DeleteMissingNestedScriptableObjects(string path) {

      var yamlObjectHeader = new Regex("^--- !u!", RegexOptions.Multiline);
     
      // 114 - class id (see https://docs.unity3d.com/Manual/ClassIDReference.html)
      var monoBehaviourRegex = new Regex(@"^114 &(\d+)");

      // if a script is missing, then it will load as null
      List<long> validFileIds = new List<long>();
      foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path)) {
        if (asset == null)
          continue;

        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset.GetInstanceID(), out var guid, out long fileId)) {
          validFileIds.Add(fileId);
        }
      }

      var yamlObjects = yamlObjectHeader.Split(System.IO.File.ReadAllText(path)).ToList();

      // now remove all that's missing
      int initialCount = yamlObjects.Count;
      for (int i = 0; i < yamlObjects.Count; ++i) {
        var part = yamlObjects[i];
        var m = monoBehaviourRegex.Match(part);
        if (!m.Success)
          continue;

        var assetFileId = long.Parse(m.Groups[1].Value);
        if (!validFileIds.Remove(assetFileId)) {
          yamlObjects.RemoveAt(i--);
        }
      }

      Debug.Assert(initialCount >= yamlObjects.Count);
      if (initialCount != yamlObjects.Count) {
        System.IO.File.WriteAllText(path, string.Join("--- !u!", yamlObjects));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return initialCount - yamlObjects.Count;
      } else {
        return 0;
      }
    }
  }
}