using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Quantum {

  public partial class AssetResourceContainer {
    public AssetResourceInfoGroup_Resources ResourcesGroup;

    [Serializable]
    public class AssetResourceInfo_Resources : AssetResourceInfo {
      public string ResourcePath;
    }

    [Serializable]
    public class AssetResourceInfoGroup_Resources : AssetResourceGroupInfo<AssetResourceInfo_Resources> {
      public override int SortOrder => 2000;

      public override UnityResourceLoader.ILoader CreateLoader() {
        return new Loader_Resources();
      }

#if UNITY_EDITOR
      public override AssetResourceInfo EditorTryCreateResourceInfo(AssetBase asset) {
        var assetPath = AssetDatabase.GetAssetPath(asset);
        if (PathUtils.MakeRelativeToFolder(assetPath, "Resources", out var resourcePath)) {
          // drop the extension
          return new AssetResourceInfo_Resources() {
            ResourcePath = PathUtils.GetPathWithoutExtension(resourcePath)
          };
        }
        return null;
      }

#endif
    }

    class Loader_Resources : UnityResourceLoader.LoaderBase<AssetResourceInfo_Resources, ResourceRequest> {

      protected override AssetBase GetAssetFromAsyncState(AssetResourceInfo_Resources info, ResourceRequest asyncState) {
        if (info.IsNestedAsset) {
          return FindAsset(UnityEngine.Resources.LoadAll<AssetBase>(info.ResourcePath), info.Guid);
        } else {
          return (AssetBase)asyncState.asset;
        }
      }

      protected override bool IsDone(ResourceRequest asyncState) {
        return asyncState.isDone;
      }

      protected override ResourceRequest LoadAsync(AssetResourceInfo_Resources info) {
        return UnityEngine.Resources.LoadAsync<AssetBase>(info.ResourcePath);
      }

      protected override AssetBase LoadSync(AssetResourceInfo_Resources info) {
        return info.IsNestedAsset
          ? FindAsset(UnityEngine.Resources.LoadAll<AssetBase>(info.ResourcePath), info.Guid)
          : UnityEngine.Resources.Load<AssetBase>(info.ResourcePath);
      }
    }
  }
}