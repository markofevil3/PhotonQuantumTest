using System;
using System.Diagnostics;
using Photon.Deterministic;
using UnityEditor;

namespace Quantum.Editor {
  [CustomEditor(typeof(MapAsset), true)]
  public class MapAssetEditor : AssetBaseEditor {
    public override void OnInspectorGUI() {
      if (!QuantumEditorSettings.Instance.UseQuantumAssetInspector) {
        // Soft deactivate the Quantum asset editor
        base.OnInspectorGUI();
      }
      else {
        CustomEditorsHelper.DrawDefaultInspector(serializedObject, new string[] { nameof(MapAsset.Settings), nameof(MapAsset.Prototypes) });
        using (new CustomEditorsHelper.BoxScope("Map")) {
          // This draws all fields except the "Settings" and script.
          CustomEditorsHelper.DrawDefaultInspector(serializedObject, "Settings", new string[] {"Settings.Prototypes" }, false);
          CustomEditorsHelper.DrawDefaultInspector(serializedObject.FindPropertyOrThrow(nameof(MapAsset.Prototypes)), skipRoot: false);
        }
      }

      var data = target as MapAsset;

      if (data.Settings.BucketsCount < 1) {
        data.Settings.BucketsCount = 1;
      }
      
      if (data.Settings.BucketsSubdivisions < 1) {
        data.Settings.BucketsSubdivisions = 1;
      }
      
      if (data.Settings.TriangleMeshCellSize < 2) {
        data.Settings.TriangleMeshCellSize = 2;
      }
      
      if ((data.Settings.TriangleMeshCellSize & 1) == 1) {
        data.Settings.TriangleMeshCellSize += 1;
      }

      if (data.Settings.WorldSize < 4) {
        data.Settings.WorldSize = 4;
      } else if(data.Settings.WorldSize > FP.UseableMax / 2){
        if (
          (data.Settings.BucketingAxis == PhysicsCommon.BucketAxis.X && data.Settings.SortingAxis == PhysicsCommon.SortAxis.X) || 
          (data.Settings.BucketingAxis == PhysicsCommon.BucketAxis.Y && data.Settings.SortingAxis == PhysicsCommon.SortAxis.Y)) {
          data.Settings.WorldSize = (FP.UseableMax / 2).AsInt;
        }
        else if(data.Settings.WorldSize > FP.UseableMax){
          data.Settings.WorldSize = FP.UseableMax.AsInt;
        }
      }
      
      if (data.Settings.GridSizeX <  2) {
        data.Settings.GridSizeX = 2;
      }

      if (data.Settings.GridSizeY < 2) {
        data.Settings.GridSizeY = 2;
      }

      if ((data.Settings.GridSizeX & 1) == 1) {
        data.Settings.GridSizeX += 1;
      }

      if ((data.Settings.GridSizeY & 1) == 1) {
        data.Settings.GridSizeY += 1;
      }

      if (data.Settings.GridNodeSize < 2) {
        data.Settings.GridNodeSize = 2;
      }

      if ((data.Settings.GridNodeSize & 1) == 1) {
        data.Settings.GridNodeSize += 1;
      }
    }
  }
}