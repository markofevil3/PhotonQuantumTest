using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quantum.Editor {
  public partial class MapNavMeshDefinitionEditor {

    [Obsolete("Use MapNavMeshEditor.BakeUnityNavmesh()")]
    public static bool BakeUnityNavmesh(MapNavMeshDefinition data) {
      return MapNavMeshEditor.BakeUnityNavmesh(data.gameObject);
    }

    [Obsolete("Use MapNavMesh.FindSmallestAgentRadius()")]
    public static float FindSmallestAgentRadius(MapNavMeshDefinition data) {
      return MapNavMesh.FindSmallestAgentRadius(data.NavMeshSurfaces).AsFloat;
    }

    public static void ImportFromUnity(MapNavMeshDefinition data) {


      var scene = data.gameObject.scene;
      Debug.Assert(scene.IsValid());

      data.InvalidateGizmoMesh();

      var importSettings = MapNavMeshDefinition.CreateImportSettings(data);
      // Get the agent radius that the navmesh was generated with. Use the smallest one from surfaces.
      data.AgentRadius = importSettings.MinAgentRadius;

      // If NavMeshSurface installed, this will deactivate non linked surfaces 
      // to make the CalculateTriangulation work only with the selected Unity navmesh.
      List<GameObject> deactivatedObjects = new List<GameObject>();
      if (data.NavMeshSurfaces != null && data.NavMeshSurfaces.Length > 0) {
        if (NavMeshSurfaceType != null) {
          var surfaces = MapDataBaker.FindLocalObjects(scene, NavMeshSurfaceType);
          foreach (MonoBehaviour surface in surfaces) {
            if (data.NavMeshSurfaces.Contains(surface.gameObject) == false) {
              surface.gameObject.SetActive(false);
              deactivatedObjects.Add(surface.gameObject);
            }
          }
        }
      }

      try {
        var bakeDate = MapNavMesh.ImportFromUnity(scene, importSettings, data.name);
        data.Links = bakeDate.Links;
        data.Regions = bakeDate.Regions;
        data.Triangles = bakeDate.Triangles;
        data.Vertices = bakeDate.Vertices;
      } catch (Exception e) {
        Log.Exception(e);
        throw e;
      } finally {
        foreach (var go in deactivatedObjects) {
          go.SetActive(true);
        }
      }
    }
  }
}
