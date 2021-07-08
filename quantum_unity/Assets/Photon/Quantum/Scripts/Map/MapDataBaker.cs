using Photon.Deterministic;
using Quantum;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Assert = Quantum.Assert;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

#endif

public static class MapDataBaker {
  public static int NavMeshSerializationBufferSize = 1024 * 1024 * 60;

  public static void BakeMapData(MapData data, Boolean inEditor) {
    FPMathUtils.LoadLookupTables();

    if (inEditor == false && !data.Asset) {
      data.Asset          = ScriptableObject.CreateInstance<MapAsset>();
      data.Asset.Settings = new Quantum.Map();
    }

    BakeData(data, inEditor);
  }

  public static void BakeMeshes(MapData data, Boolean inEditor) {
    if (inEditor) {
#if UNITY_EDITOR
      var assetPath         = Path.GetDirectoryName(AssetDatabase.GetAssetPath(data.Asset));
      var meshDataFilenname = data.Asset.name + "_mesh.bytes";
      var dbAssetPath       = default(string);

      if (!Quantum.PathUtils.MakeRelativeToFolder(assetPath, QuantumEditorSettings.Instance.DatabasePath, out dbAssetPath)) {
        Debug.LogErrorFormat("Cannot convert '{0}' into a relative path using the db folder '{1}'.", assetPath, QuantumEditorSettings.Instance.DatabasePath);
        throw new Exception();
      }

      if (string.IsNullOrEmpty(dbAssetPath)) {
        data.Asset.Settings.StaticColliders3DTrianglesBinaryFile = meshDataFilenname;
      } else {
        data.Asset.Settings.StaticColliders3DTrianglesBinaryFile = Path.Combine(dbAssetPath, meshDataFilenname).Replace('\\', '/');
      }

      // Serialize to binary some of the data (max 20 megabytes for now)
      var bytestream = new ByteStream(new Byte[NavMeshSerializationBufferSize]);
      data.Asset.Settings.SerializeStaticColliderTriangles(bytestream, true);

      // write file to disk
      File.WriteAllBytes(Path.Combine(assetPath, meshDataFilenname), bytestream.ToArray());
#endif
    }
  }

  public static IEnumerable<NavMesh> BakeNavMeshes(MapData data, Boolean inEditor) {
    FPMathUtils.LoadLookupTables();

    data.Asset.Settings.NavMeshLinks = new AssetRefNavMesh[0];
    data.Asset.Settings.Regions      = new string[0];
    
    InvokeCallbacks("OnBeforeBakeNavMesh", data);

    var navmeshes = BakeNavMeshesLoop(data).ToList();

    if (inEditor) {
#if UNITY_EDITOR
      var assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(data.Asset));
      foreach (var navmesh in navmeshes) {
        var navmeshBinaryFilename = $"{data.Asset.name}_{navmesh.Name}.bytes";
        var navmeshAssetFilename  = $"{data.Asset.name}_{navmesh.Name}.asset";

        // Make navmesh data file path relative to the DB folder
        if (!Quantum.PathUtils.MakeRelativeToFolder(assetPath, QuantumEditorSettings.Instance.DatabasePath, out var dbAssetPath)) {
          Debug.LogErrorFormat("Cannot convert '{0}' into a relative path using the db folder '{1}'.", assetPath, QuantumEditorSettings.Instance.DatabasePath);
          throw new Exception();
        }

        if (string.IsNullOrEmpty(dbAssetPath))
          navmesh.DataFilepath = navmeshBinaryFilename;
        else
          navmesh.DataFilepath = Path.Combine(dbAssetPath, navmeshBinaryFilename).Replace('\\', '/');

        // Serialize to binary some of the data (max 20 megabytes for now)
        var bytestream = new ByteStream(new Byte[NavMeshSerializationBufferSize]);
        navmesh.Serialize(bytestream, true);
        File.WriteAllBytes(Path.Combine(assetPath, navmeshBinaryFilename), bytestream.ToArray());

        var navmeshAssetPath = Path.Combine(assetPath, navmeshAssetFilename);
        var oldAsset         = AssetDatabase.LoadAssetAtPath<NavMeshAsset>(navmeshAssetPath);
        if (oldAsset != null) {
          navmesh.Guid = oldAsset.Settings.Guid;
        }

        if (!navmesh.Guid.IsValid) {
          navmesh.Guid = AssetGuid.NewGuid();
        }

        if (oldAsset != null) {
          // Reuse the old one
          navmesh.Path      = oldAsset.Settings.Path;
          oldAsset.Settings = navmesh;
          EditorUtility.SetDirty(oldAsset);
        } else {
          // Create scriptable object
          var navMeshAsset = ScriptableObject.CreateInstance<NavMeshAsset>();
          navmesh.Path          = navMeshAsset.GenerateDefaultPath(navmeshAssetPath);
          navMeshAsset.Settings = navmesh;
          AssetDatabase.CreateAsset(navMeshAsset, navmeshAssetPath);
          AssetDatabase.ImportAsset(navmeshAssetPath, ImportAssetOptions.ForceUpdate);
        }

        ArrayUtils.Add(ref data.Asset.Settings.NavMeshLinks, (AssetRefNavMesh)navmesh);
      }
#endif
    } else {
      // When executing this during runtime the guids of the created navmesh are added to the map.
      // Binary navmesh files are not created because the fresh navmesh object has everything it needs.
      // Caveat: the returned navmeshes need to be added to the DB by either...
      // A) overwriting the navmesh inside an already existing NavMeshAsset ScriptableObject or
      // B) Creating new NavMeshAsset ScriptableObjects (see above) and inject them into the DB (use UnityDB.OnAssetLoad callback).
      foreach (var navmesh in navmeshes) {
        navmesh.Path = data.Asset.name + "_" + navmesh.Name;
        navmesh.Guid = AssetGuid.NewGuid();
        ArrayUtils.Add(ref data.Asset.Settings.NavMeshLinks, (AssetRefNavMesh)navmesh);
      }
    }
    
    InvokeCallbacks("OnBakeNavMesh", data);

    return navmeshes;
  }

  static StaticColliderData GetStaticData(GameObject gameObject, QuantumStaticColliderSettings settings, int colliderId) {
    return new StaticColliderData {
      Asset      = settings.Asset,
      Name       = gameObject.name,
      Tag        = gameObject.tag,
      Layer      = gameObject.layer,
      IsTrigger  = settings.Trigger,
      ColliderId = colliderId
    };
  }

  static void BakeData(MapData data, Boolean inEditor) {

    var scene = data.gameObject.scene;
    Debug.Assert(scene.IsValid());

#if UNITY_EDITOR
    if (inEditor) {
      // set scene name
      data.Asset.Settings.Scene = scene.name;
    }
#endif

    InvokeCallbacks("OnBeforeBake", data);

    // clear existing colliders
    data.Asset.Settings.StaticColliders2D = new MapStaticCollider2D[0];
    data.StaticCollider2DReferences       = new List<MonoBehaviour>();
    data.StaticCollider3DReferences       = new List<MonoBehaviour>();

    // circle colliders
    foreach (var collider in FindLocalObjects<QuantumStaticCircleCollider2D>(scene)) {
      var s = collider.transform.localScale;
      ArrayUtils.Add(ref data.Asset.Settings.StaticColliders2D, new MapStaticCollider2D {
        Position = collider.transform.position.ToFPVector2(),
        Rotation = collider.transform.rotation.ToFPRotation2D(),
#if QUANTUM_XY
        VerticalOffset = -collider.transform.position.z.ToFP(),
        Height = collider.Height * s.z.ToFP(),
#else
        VerticalOffset = collider.transform.position.y.ToFP(),
        Height         = collider.Height * s.y.ToFP(),
#endif
        PhysicsMaterial = collider.Settings.PhysicsMaterial,
        StaticData      = GetStaticData(collider.gameObject, collider.Settings, data.Asset.Settings.StaticColliders2D.Length),
        Layer           = collider.gameObject.layer,

        // circle
        ShapeType    = Quantum.Shape2DType.Circle,
        CircleRadius = FP.FromFloat_UNSAFE(collider.Radius.AsFloat * s.x)
      });

      data.StaticCollider2DReferences.Add(collider);
    }

    // polygon colliders
    foreach (var c in FindLocalObjects<QuantumStaticPolygonCollider2D>(scene)) {
      if (c.BakeAsStaticEdges2D) {
        for (var i = 0; i < c.Vertices.Length; i++) {
          var staticEdge = BakeStaticEdge2D(c.transform, c.Vertices[i], c.Vertices[(i + 1) % c.Vertices.Length], c.Height, c.Settings, data.Asset.Settings.StaticColliders2D.Length);
          ArrayUtils.Add(ref data.Asset.Settings.StaticColliders2D, staticEdge);
          data.StaticCollider2DReferences.Add(c);
        }

        continue;
      }

      var s = c.transform.localScale;
      var vertices = c.Vertices.Select(x => {
        var v = x.ToUnityVector3();
        return new Vector3(v.x * s.x, v.y * s.y, v.z * s.z);
      }).Select(x => x.ToFPVector2()).ToArray();
      if (FPVector2.IsClockWise(vertices)) {
        FPVector2.MakeCounterClockWise(vertices);
      }


      var normals = FPVector2.CalculatePolygonNormals(vertices);

      ArrayUtils.Add(ref data.Asset.Settings.StaticColliders2D, new MapStaticCollider2D {
        Position = c.transform.position.ToFPVector2(),
        Rotation = c.transform.rotation.ToFPRotation2D(),
#if QUANTUM_XY
        VerticalOffset = -c.transform.position.z.ToFP(),
        Height = c.Height * s.z.ToFP(),
#else
        VerticalOffset = c.transform.position.y.ToFP(),
        Height         = c.Height * s.y.ToFP(),
#endif
        PhysicsMaterial = c.Settings.PhysicsMaterial,
        StaticData      = GetStaticData(c.gameObject, c.Settings, data.Asset.Settings.StaticColliders2D.Length),
        Layer           = c.gameObject.layer,

        // polygon
        ShapeType = Quantum.Shape2DType.Polygon,
        PolygonCollider = new PolygonCollider {
          Vertices = vertices,
          Normals  = normals
        }
      });

      data.StaticCollider2DReferences.Add(c);
    }

    // edge colliders
    foreach (var c in FindLocalObjects<QuantumStaticEdgeCollider2D>(scene)) {
      ArrayUtils.Add(ref data.Asset.Settings.StaticColliders2D, BakeStaticEdge2D(c.transform, c.VertexA, c.VertexB, c.Height, c.Settings, data.Asset.Settings.StaticColliders2D.Length));
      data.StaticCollider2DReferences.Add(c);
    }

    // box colliders
    foreach (var collider in FindLocalObjects<QuantumStaticBoxCollider2D>(scene)) {
      var e = collider.Size.ToUnityVector3();
      var s = collider.transform.localScale;
      e.x *= s.x;
      e.y *= s.y;
      e.z *= s.z;

      ArrayUtils.Add(ref data.Asset.Settings.StaticColliders2D, new MapStaticCollider2D {
        Position = collider.transform.position.ToFPVector2(),
        Rotation = collider.transform.rotation.ToFPRotation2D(),
#if QUANTUM_XY
        VerticalOffset = -collider.transform.position.z.ToFP(),
        Height = collider.Height * s.z.ToFP(),
#else
        VerticalOffset = collider.transform.position.y.ToFP(),
        Height         = collider.Height * s.y.ToFP(),
#endif
        PhysicsMaterial = collider.Settings.PhysicsMaterial,
        StaticData      = GetStaticData(collider.gameObject, collider.Settings, data.Asset.Settings.StaticColliders2D.Length),
        Layer           = collider.gameObject.layer,

        // polygon
        ShapeType  = Quantum.Shape2DType.Box,
        BoxExtents = e.ToFPVector2() * FP._0_50
      });

      data.StaticCollider2DReferences.Add(collider);
    }

    // 3D statics

    // clear existing colliders
    var staticCollider3DList = new List<MapStaticCollider3D>();

    // clear on mono behaviour and assets
    data.Asset.Settings.StaticColliders3DTriangles = new Dictionary<int, TriangleCCW[]>();
    data.Asset.Settings.StaticColliders3D          = new MapStaticCollider3D[0];

    // initialize collider references, add default null on offset 0
    data.StaticCollider3DReferences = new List<MonoBehaviour>();

    // sphere colliders
    foreach (var collider in FindLocalObjects<QuantumStaticSphereCollider3D>(scene)) {
      staticCollider3DList.Add(new MapStaticCollider3D {
        Position        = collider.transform.position.ToFPVector3(),
        Rotation        = collider.transform.rotation.ToFPQuaternion(),
        PhysicsMaterial = collider.Settings.PhysicsMaterial,
        StaticData      = GetStaticData(collider.gameObject, collider.Settings, staticCollider3DList.Count),

        // circle
        ShapeType    = Quantum.Shape3DType.Sphere,
        SphereRadius = FP.FromFloat_UNSAFE(collider.Radius.AsFloat * collider.transform.localScale.x)
      });

      data.StaticCollider3DReferences.Add(collider);
    }

    // box colliders
    foreach (var collider in FindLocalObjects<QuantumStaticBoxCollider3D>(scene)) {
      var e = collider.Size.ToUnityVector3();
      var s = collider.transform.localScale;

      e.x *= s.x;
      e.y *= s.y;
      e.z *= s.z;

      staticCollider3DList.Add(new MapStaticCollider3D {
        Position        = collider.transform.position.ToFPVector3(),
        Rotation        = collider.transform.rotation.ToFPQuaternion(),
        PhysicsMaterial = collider.Settings.PhysicsMaterial,
        StaticData      = GetStaticData(collider.gameObject, collider.Settings, staticCollider3DList.Count),

        // box
        ShapeType  = Quantum.Shape3DType.Box,
        BoxExtents = e.ToFPVector3() * FP._0_50
      });

      data.StaticCollider3DReferences.Add(collider);
    }

    var meshes   = FindLocalObjects<QuantumStaticMeshCollider3D>(scene);
    var terrains = FindLocalObjects<QuantumStaticTerrainCollider3D>(scene);

    // static 3D mesh colliders
    foreach (var collider in meshes) {
      // our assumed static collider index
      var staticColliderIndex = staticCollider3DList.Count;

      // bake mesh
      if (collider.Bake(staticColliderIndex)) {
        Quantum.Assert.Check(staticColliderIndex == staticCollider3DList.Count);

        // add on list
        staticCollider3DList.Add(new MapStaticCollider3D {
          Position        = collider.transform.position.ToFPVector3(),
          Rotation        = collider.transform.rotation.ToFPQuaternion(),
          PhysicsMaterial = collider.Settings.PhysicsMaterial,
          MutableMode     = collider.Mode,
          SmoothSphereMeshCollisions = collider.SmoothSphereMeshCollisions,

          // mesh
          ShapeType = Quantum.Shape3DType.Mesh,

          // static data
          StaticData = new StaticColliderData() {
            Asset      = collider.Settings.Asset,
            IsTrigger  = collider.Settings.Trigger,
            Name       = collider.name,
            Layer      = collider.gameObject.layer,
            Tag        = collider.tag,
            ColliderId = staticColliderIndex
          }
        });

        // add to static collider lookup
        data.StaticCollider3DReferences.Add(collider);

        // add to static collider data
        data.Asset.Settings.StaticColliders3DTriangles.Add(staticColliderIndex, collider.Triangles);
      }
    }

    // terrain colliders
    foreach (var terrain in terrains) {
      // our assumed static collider index
      var staticColliderIndex = staticCollider3DList.Count;

      // bake terrain
      terrain.Bake();

      // add to 3d collider list
      staticCollider3DList.Add(new MapStaticCollider3D {
        Position        = default(FPVector3),
        Rotation        = FPQuaternion.Identity,
        PhysicsMaterial = terrain.Asset.Settings.PhysicsMaterial,
        MutableMode     = terrain.Mode,
        SmoothSphereMeshCollisions = terrain.SmoothSphereMeshCollisions,

        // terrains are meshes
        ShapeType = Quantum.Shape3DType.Mesh,

        // static data for terrain
        StaticData = new StaticColliderData {
          Name       = terrain.gameObject.name,
          Layer      = terrain.gameObject.layer,
          Tag        = terrain.gameObject.tag,
          Asset      = terrain.Asset.Settings,
          IsTrigger  = false,
          ColliderId = staticColliderIndex
        }
      });

      // add to 
      data.StaticCollider3DReferences.Add(terrain);

      // load all triangles
      terrain.Asset.Settings.Bake(staticColliderIndex);

      // add to static collider data
      data.Asset.Settings.StaticColliders3DTriangles.Add(staticColliderIndex, terrain.Asset.Settings.Triangles);
    }

    // this has to hold
    Assert.Check(staticCollider3DList.Count == data.StaticCollider3DReferences.Count);
    
    // assign collider 3d array
    data.Asset.Settings.StaticColliders3D = staticCollider3DList.ToArray();

    // clear this so it's not re-used by accident
    staticCollider3DList = null;
    
    {
      data.Asset.Prototypes.Clear();
      data.MapEntityReferences.Clear();

      var components = new List<EntityComponentBase>();
      var prototypes = FindLocalObjects<EntityPrototype>(scene).ToArray();
      SortBySiblingIndex(prototypes);

      var converter = new EntityPrototypeConverter(data, prototypes);
      var storeVisitor = new Quantum.Prototypes.FlatEntityPrototypeContainer.StoreVisitor();

      foreach (var prototype in prototypes) {
        prototype.GetComponents(components);

        var container = new Quantum.Prototypes.FlatEntityPrototypeContainer();
        storeVisitor.Storage = container;

        prototype.PreSerialize();
        prototype.SerializeImplicitComponents(storeVisitor, out var selfView);

        foreach (var component in components) {
          component.Refresh();
          var proto = component.CreatePrototype(converter);
          proto.Dispatch(storeVisitor);
        }


        data.Asset.Prototypes.Add(container);
        data.MapEntityReferences.Add(selfView);
        components.Clear();
      }
    }

    BakeMeshes(data, inEditor);

    // invoke callbacks
    InvokeCallbacks("OnBake", data);

    if (inEditor) {
      Debug.LogFormat("Baked {0} 2D static colliders",           data.Asset.Settings.StaticColliders2D.Length);
      Debug.LogFormat("Baked {0} 3D static primitive colliders", data.Asset.Settings.StaticColliders3D.Length);
      Debug.LogFormat("Baked {0} 3D static triangles",           data.Asset.Settings.StaticColliders3DTriangles.Select(x => x.Value.Length).Sum());
    }
  }

  private static void InvokeCallbacks(string callbackName, MapData data) {
    var searchAssemblies = QuantumEditorSettings.Instance.SearchAssemblies.Concat(QuantumEditorSettings.Instance.SearchEditorAssemblies).ToArray();
    var bakeCallbacks = TypeUtils.GetSubClasses(typeof(MapDataBakerCallback), searchAssemblies)
                                 .OrderBy(t => {
                                   var attr = TypeUtils.GetAttribute<MapDataBakerCallbackAttribute>(t);
                                   if (attr != null)
                                     return attr.InvokeOrder;
                                   return 0;
                                 });

    foreach (var callback in bakeCallbacks) {
      if (callback.IsAbstract == false) {
        try {
          switch (callbackName) {
            case "OnBeforeBake":
              (Activator.CreateInstance(callback) as MapDataBakerCallback).OnBeforeBake(data);
              break;
            case "OnBake":
              (Activator.CreateInstance(callback) as MapDataBakerCallback).OnBake(data);
              break;
            case "OnBeforeBakeNavMesh":
              (Activator.CreateInstance(callback) as MapDataBakerCallback).OnBeforeBakeNavMesh(data);
              break;
            case "OnBakeNavMesh":
              (Activator.CreateInstance(callback) as MapDataBakerCallback).OnBakeNavMesh(data);
              break;
          }
        }
        catch (Exception exn) {
          Debug.LogException(exn);
        }
      }
    }
  }

  static IEnumerable<NavMesh> BakeNavMeshesLoop(MapData data) {
    MapNavMesh.InvalidateGizmos();

    var scene = data.gameObject.scene;
    Debug.Assert(scene.IsValid());

    // Build unity navmeshes
    {
      var unityNavmeshes = data.GetComponentsInChildren<MapNavMeshUnity>().ToList();

      // The sorting is important to always generate the same order of regions name list.
      unityNavmeshes.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

      for (int i = 0; i < unityNavmeshes.Count; i++) {
        var navmesh = default(NavMesh);

        // If NavMeshSurface installed, this will deactivate non linked surfaces 
        // to make the CalculateTriangulation work only with the selected Unity navmesh.
        List<GameObject> deactivatedObjects = new List<GameObject>();

        try {
          if (unityNavmeshes[i].NavMeshSurfaces != null && unityNavmeshes[i].NavMeshSurfaces.Length > 0) {
            if (MapNavMesh.NavMeshSurfaceType != null) {
              var surfaces = FindLocalObjects(scene, MapNavMesh.NavMeshSurfaceType);
              foreach (MonoBehaviour surface in surfaces) {
                if (unityNavmeshes[i].NavMeshSurfaces.Contains(surface.gameObject) == false) {
                  surface.gameObject.SetActive(false);
                  deactivatedObjects.Add(surface.gameObject);
                }
              }
            }
          }

          var bakeData = MapNavMesh.ImportFromUnity(scene, unityNavmeshes[i].Settings, unityNavmeshes[i].name);
          if (bakeData == null) {
            Debug.LogErrorFormat("Could not import navmesh '{0}'", unityNavmeshes[i].name);
          } else {
            bakeData.Name             = unityNavmeshes[i].name;
            bakeData.AgentRadius      = MapNavMesh.FindSmallestAgentRadius(unityNavmeshes[i].NavMeshSurfaces);
            bakeData.EnableQuantum_XY = unityNavmeshes[i].Settings.EnableQuantum_XY;
            bakeData.ClosestTriangleCalculation      = unityNavmeshes[i].Settings.ClosestTriangleCalculation;
            bakeData.ClosestTriangleCalculationDepth = unityNavmeshes[i].Settings.ClosestTriangleCalculationDepth;
            navmesh                   = MapNavMeshBaker.BakeNavMesh(data, bakeData);
            Debug.LogFormat("Baking Quantum NavMesh '{0}' complete ({1}/{2})", unityNavmeshes[i].name, i + 1, unityNavmeshes.Count);
          }
        }
        catch (Exception exn) {
          Debug.LogException(exn);
        }

        foreach (var go in deactivatedObjects) {
          go.SetActive(true);
        }

        if (navmesh != null) {
          yield return navmesh;
        } else {
          Debug.LogErrorFormat("Baking Quantum NavMesh '{0}' failed", unityNavmeshes[i].name);
        }
      }
    }

    // Bake manual navmeshes
    {
      var navmeshDefinitions = data.GetComponentsInChildren<MapNavMeshDefinition>().ToList();

      // The sorting is important to always generate the same order of regions name list.
      navmeshDefinitions.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

      for (int i = 0; i < navmeshDefinitions.Count; i++) {
        var navmesh = default(NavMesh);

        try {
          navmesh = MapNavMeshBaker.BakeNavMesh(data, MapNavMeshDefinition.CreateBakeData(navmeshDefinitions[i]));
          Debug.LogFormat("Baking Quantum NavMesh '{0}' complete ({1}/{2})", navmeshDefinitions[i].name, i + 1, navmeshDefinitions.Count);
        }
        catch (Exception exn) {
          Debug.LogException(exn);
        }

        if (navmesh != null) {
          yield return navmesh;
        } else {
          Debug.LogErrorFormat("Baking Quantum NavMesh '{0}' failed", navmeshDefinitions[i].name);
        }
      }
    }
  }

  private static void SortBySiblingIndex<T>(T[] array) where T : Component {
    // sort by sibling indices; this should be uniform across machines
    List<int> list0 = new List<int>();
    List<int> list1 = new List<int>();
    Array.Sort(array, (a, b) => CompareLists(GetSiblingIndexPath(a.transform, list0), GetSiblingIndexPath(b.transform, list1)));
  }

  static List<int> GetSiblingIndexPath(Transform t, List<int> buffer) {
    buffer.Clear();
    while (t != null) {
      buffer.Add(t.GetSiblingIndex());
      t = t.parent;
    }

    buffer.Reverse();
    return buffer;
  }

  static int CompareLists(List<int> left, List<int> right) {
    while (left.Count > 0 && right.Count > 0) {
      if (left[0] < right[0]) {
        return -1;
      }

      if (left[0] > right[0]) {
        return 1;
      }

      left.RemoveAt(0);
      right.RemoveAt(0);
    }

    return 0;
  }

  static MapStaticCollider2D BakeStaticEdge2D(Transform t, FPVector2 vertexA, FPVector2 vertexB, FP height, QuantumStaticColliderSettings settings, int colliderId) {
    var scale   = t.localScale;
    var scaledA = Vector3.Scale(scale, vertexA.ToUnityVector3());
    var scaledB = Vector3.Scale(scale, vertexB.ToUnityVector3());

    var rot        = t.rotation;
    var start      = rot * scaledA;
    var end        = rot * scaledB;
    var startToEnd = end - start;

    var pos = t.position + start + (startToEnd / 2.0f);
    rot = Quaternion.FromToRotation(Vector3.right, startToEnd);
    var edgeExtent = startToEnd.magnitude / 2.0f;

    return new MapStaticCollider2D {
      Position = pos.ToFPVector2(),
      Rotation = rot.ToFPRotation2D(),
#if QUANTUM_XY
      VerticalOffset = -t.position.z.ToFP(),
      Height = height * scale.z.ToFP(),
#else
      VerticalOffset = t.position.y.ToFP(),
      Height         = height * scale.y.ToFP(),
#endif
      PhysicsMaterial = settings.PhysicsMaterial,
      StaticData      = GetStaticData(t.gameObject, settings, colliderId),
      Layer           = t.gameObject.layer,

      // edge
      ShapeType  = Quantum.Shape2DType.Edge,
      EdgeExtent = edgeExtent.ToFP(),
    };
  }

  public static List<T> FindLocalObjects<T>(Scene scene) where T : Component {
    List<T> partialResult = new List<T>();
    List<T> fullResult = new List<T>();
    foreach (var gameObject in scene.GetRootGameObjects()) {
      // GetComponentsInChildren seems to clear the list first, but we're not going to depend
      // on this implementation detail
      if (!gameObject.activeInHierarchy)
        continue;
      partialResult.Clear();
      gameObject.GetComponentsInChildren(partialResult);
      fullResult.AddRange(partialResult);
    }
    return fullResult;
  }

  public static List<Component> FindLocalObjects(Scene scene, Type type) {
    List<Component> result = new List<Component>();
    foreach (var gameObject in scene.GetRootGameObjects()) {
      if (!gameObject.activeInHierarchy)
        continue;
      foreach (var component in gameObject.GetComponentsInChildren(type)) {
        result.Add(component);
      }
    }
    return result;
  }
}