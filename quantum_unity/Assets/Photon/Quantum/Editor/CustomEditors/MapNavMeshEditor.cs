using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  public static class MapNavMeshEditor {

    public static void UpdateDefaultMinAgentRadius() {
      // This can not be called by reflection, hence we need to set this by this ugly way.
      var settingsObject = new SerializedObject(UnityEditor.AI.NavMeshBuilder.navMeshSettingsObject);
      var radiusProperty = settingsObject.FindProperty("m_BuildSettings.agentRadius");
      MapNavMesh.DefaultMinAgentRadius = radiusProperty.floatValue;
    }

    public static bool BakeUnityNavmesh(GameObject go) {
      if (MapNavMesh.NavMeshSurfaceType == null) {
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        return true;
      }
      else {
        var unityNavmeshes = new List<MapNavMeshUnity>();
        go.GetComponents(unityNavmeshes);
        go.GetComponentsInChildren(unityNavmeshes);
        foreach (var unityNavmesh in unityNavmeshes) {
          foreach (var gameObject in unityNavmesh.NavMeshSurfaces) {
            var navMeshSurface = gameObject.GetComponent(MapNavMesh.NavMeshSurfaceType);
            var buildNavMeshMethod = MapNavMesh.NavMeshSurfaceType.GetMethod("BuildNavMesh");
            buildNavMeshMethod.Invoke(navMeshSurface, null);
          }
        }
      }

      return false;
    }

    #region Unity Editors

    [CustomEditor(typeof(MapNavMeshUnity))]
    public partial class MapNavMeshUnityEditor : UnityEditor.Editor {
      public override void OnInspectorGUI() {

        var data = ((MapNavMeshUnity)target).Settings;

#if QUANTUM_XY
        var filteredSettings = new List<string>() { "Settings.RegionAreaIds" };
#else 
        var filteredSettings = new List<string>() { "Settings.RegionAreaIds", "Settings.EnableQuantum_XY" };
#endif

        if (data.WeldIdenticalVertices == false) filteredSettings.Add("Settings.WeldVertexEpsilon");
        if (data.FixTrianglesOnEdges == false) filteredSettings.Add("Settings.FixTrianglesOnEdgesEpsilon");
        if (data.ImportRegions == false) filteredSettings.Add("Settings.RegionDetectionMargin");
        if (data.ClosestTriangleCalculation == MapNavMesh.FindClosestTriangleCalculation.BruteForce) filteredSettings.Add("Settings.ClosestTriangleCalculationDepth");

        using (new CustomEditorsHelper.BoxScope("Import Settings")) {
          if (MapNavMesh.NavMeshSurfaceType != null) {
            CustomEditorsHelper.DrawDefaultInspector(serializedObject, "NavMeshSurfaces", new string[0]);
          }

          CustomEditorsHelper.DrawDefaultInspector(serializedObject, "Settings", filteredSettings.ToArray(), false);

          if (data.WeldIdenticalVertices) {
            data.WeldVertexEpsilon = Mathf.Max(data.WeldVertexEpsilon, float.Epsilon);
          }

          if (data.FixTrianglesOnEdges) {
            data.FixTrianglesOnEdgesEpsilon = Mathf.Max(data.FixTrianglesOnEdgesEpsilon, float.Epsilon);
          }

          if (data.ImportRegions) {

            EditorGUILayout.LabelField(new GUIContent("Convert Unity Areas To Quantum Region:", "Select what Unity NavMesh areas are used to generated Quantum regions. At least one must be selected."));
            EditorGUI.indentLevel++;
            var names = new List<string>(GameObjectUtility.GetNavMeshAreaNames());
            if (data.RegionAreaIds == null) {
              data.RegionAreaIds = new List<int>();
            }

            for (int i = 0; i < data.RegionAreaIds.Count; i++) {
              var areaId = data.RegionAreaIds[i];
              var areaName = GameObjectUtility.GetNavMeshAreaNames().FirstOrDefault(name => GameObjectUtility.GetNavMeshAreaFromName(name) == areaId);
              if (string.IsNullOrEmpty(areaName)) {
                areaName = "missing Unity NavMesh area";
              }

              if (!EditorGUILayout.Toggle(areaName, true)) {
                data.RegionAreaIds.Remove(areaId);
              }
              else {
                names.Remove(areaName);
              }
            }

            if (names.Count > 0) {
              var newName = EditorGUILayout.Popup("Add New Area", -1, names.ToArray());
              if (newName >= 0) {
                var areaId = GameObjectUtility.GetNavMeshAreaFromName(names[newName]);
                data.RegionAreaIds.Add(areaId);
              }
            }

            EditorGUI.indentLevel--;
          }
        }

        // This is confusing, it only triggers the unity baking not the Quantum baking
        //using (new CustomEditorsHelper.BackgroundColorScope(Color.green)) {
        //  if (GUILayout.Button("Run Unity Navmesh Baker", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2))) {
        //    BakeUnityNavmesh(((MapNavMeshUnity)target).gameObject);
        //  }
        //}
      }

      #endregion
    }
  }
}