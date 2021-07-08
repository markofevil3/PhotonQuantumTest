using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(MapNavMeshRegion))]
  public class MapNavMeshRegionEditor : UnityEditor.Editor {
    static bool _navMeshModifierSearched;
    static System.Type navMeshModifierType;

    System.Type NavMeshModifierType {
      get {
        // TypeUtils.FindType can be quite slow
        if (_navMeshModifierSearched == false) {
          _navMeshModifierSearched = true;
          navMeshModifierType = TypeUtils.FindType("NavMeshModifier");
        }
        return navMeshModifierType;
      }
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      var data = (MapNavMeshRegion)target;

      if (string.IsNullOrEmpty(data.Id)) {
        EditorGUILayout.HelpBox("Id is not set", MessageType.Error);
      }
      else if (data.Id == "Default") {
        EditorGUILayout.HelpBox("'Default' is not allowed", MessageType.Error);
      }

      if (data.CastRegion == MapNavMeshRegion.RegionCastType.CastRegion) {

        CustomEditorsHelper.DrawHeadline("NavMesh Helper");

        if (data.gameObject.GetComponent<MeshRenderer>() == null) {
          EditorGUILayout.HelpBox("MapNavMeshRegion requires a MeshRenderer to be able to cast a region onto the navmesh", MessageType.Error);
        }

        using (var change = new EditorGUI.ChangeCheckScope()) {
          var currentFlags = GameObjectUtility.GetStaticEditorFlags(data.gameObject);
          var currentNavigationStatic = (currentFlags & StaticEditorFlags.NavigationStatic) == StaticEditorFlags.NavigationStatic;
          var newNavigationStatic = EditorGUILayout.Toggle("Toggle Static Flag", currentNavigationStatic);
          if (currentNavigationStatic != newNavigationStatic) {
            if (newNavigationStatic)
              GameObjectUtility.SetStaticEditorFlags(data.gameObject, currentFlags | StaticEditorFlags.NavigationStatic);
            else
              GameObjectUtility.SetStaticEditorFlags(data.gameObject, currentFlags & ~StaticEditorFlags.NavigationStatic);
          }

          if (NavMeshModifierType != null) {
            var modifier = data.GetComponent(NavMeshModifierType) ?? data.GetComponentInParent(NavMeshModifierType);
            if (modifier != null) {
              EditorGUILayout.ObjectField("NavMesh Modifier GameObject", modifier.gameObject, typeof(GameObject), true);
              using (new EditorGUI.DisabledGroupScope(true)) {
                EditorGUILayout.Toggle("Override Area", (bool)NavMeshModifierType.GetField("m_OverrideArea", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(modifier));
                var areaId = (int)NavMeshModifierType.GetField("m_Area", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(modifier);
                var map = GameObjectUtility.GetNavMeshAreaNames();
                var currentIndex = GetNavMeshAreaIndex(areaId);
                var newIndex = EditorGUILayout.Popup("Area", currentIndex, map);
                if (currentIndex != newIndex) {
                  GameObjectUtility.SetNavMeshArea(data.gameObject, GameObjectUtility.GetNavMeshAreaFromName(map[newIndex]));
                }
              }
            }
            else {
              EditorGUILayout.LabelField("The NavMesh Area is defined by the NavMeshModifier script. No such script found in parents.");
            }
          }
          else {
            var areaId = GameObjectUtility.GetNavMeshArea(data.gameObject);

            var map = GameObjectUtility.GetNavMeshAreaNames();
            var currentIndex = GetNavMeshAreaIndex(areaId);
            var newIndex = EditorGUILayout.Popup("Unity NavMesh Area", currentIndex, map);
            if (currentIndex != newIndex) {
              GameObjectUtility.SetNavMeshArea(data.gameObject, GameObjectUtility.GetNavMeshAreaFromName(map[newIndex]));
            }

            if (newIndex < 3 || newIndex >= map.Length) {
              EditorGUILayout.HelpBox("Unity NavMesh Area not valid", MessageType.Error);
            }



            if (newNavigationStatic == false) {
              EditorGUILayout.HelpBox("Unity Navigation Static has to be enabled", MessageType.Error);
            }
          }

          if (change.changed) {
            EditorUtility.SetDirty(target);
          }
        }
      }
    }

    private static int GetNavMeshAreaIndex(int areaId) {
      var map = GameObjectUtility.GetNavMeshAreaNames();
      var index = 0;
      for (index = 0; index < map.Length;) {
        if (GameObjectUtility.GetNavMeshAreaFromName(map[index]) == areaId)
          break;
        index++;
      }

      return index;
    }
  }
}
