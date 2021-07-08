using Photon.Deterministic;
using UnityEditor;

namespace Quantum.Editor {

  [CustomEditor(typeof(NavMeshAgentConfigAsset))]
  public class NavMeshAgentConfigAssetEditor : AssetBaseEditor {

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      var data = (NavMeshAgentConfigAsset)target;

      if (data.Settings.AvoidanceRadius <= FP._0) {
        data.Settings.AvoidanceRadius = data.Settings.Radius;
      }

      if (data.Settings.StoppingDistance < Navigation.Constants.MinStoppingDistance) {
        data.Settings.StoppingDistance = Navigation.Constants.MinStoppingDistance;
      }

      if (data.Settings.CachedWaypointCount < 3) {
        data.Settings.CachedWaypointCount = 3;
      }

      if (data.Settings.CachedWaypointCount > 255) {
        data.Settings.CachedWaypointCount = 255;
      }
    }
  }
}
