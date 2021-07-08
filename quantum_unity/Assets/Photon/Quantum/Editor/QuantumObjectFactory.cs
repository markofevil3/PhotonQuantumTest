using System.Linq;
using System.Runtime.CompilerServices;
using Photon.Deterministic;
using UnityEditor;
using UnityEngine;

namespace Quantum {
  static class QuantumObjectFactory {

    private static Mesh _circleMesh;
    private static Mesh CircleMesh => LoadMesh("Circle", ref _circleMesh);

    private static Mesh _quadMesh;
    private static Mesh QuadMesh => LoadMesh("Quad", ref _quadMesh);


    [MenuItem("GameObject/Quantum/Empty Entity", false, 10)]
    private static void CreateEntity(MenuCommand mc) => new GameObject()
      .ThenAdd<global::EntityPrototype>(x => x.TransformMode = EntityPrototypeTransformMode.None)
      .ThenAdd<global::EntityView>()
      .Finish(mc);

    [MenuItem("GameObject/Quantum/2D/Sprite Entity", false, 10)]
#if QUANTUM_XY
    private static void CreateSpriteEntity(MenuCommand mc) => new GameObject()
      .ThenAdd<SpriteRenderer>()
      .ThenAdd<global::EntityPrototype>(x => x.TransformMode = EntityPrototypeTransformMode.Transform2D)
      .ThenAdd<global::EntityView>()
      .Finish(mc);
#else
    private static void CreateSpriteEntity(MenuCommand mc) => new GameObject()
      .ThenAlter<Transform>(x => {
        var child = new GameObject("Sprite");
        child.AddComponent<SpriteRenderer>();
        child.transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.right);
        child.transform.SetParent(x, false);
      })
      .ThenAdd<global::EntityPrototype>(x => x.TransformMode = EntityPrototypeTransformMode.Transform2D)
      .ThenAdd<global::EntityView>()
      .Finish(mc, select: "Sprite");
#endif

    [MenuItem("GameObject/Quantum/2D/Quad Entity", false, 10)]
    private static void CreateQuadEntity(MenuCommand mc) => ObjectFactory.CreatePrimitive(PrimitiveType.Cube)
      .ThenRemove<Collider>()
      .ThenAlter<MeshFilter>(x => x.sharedMesh = QuadMesh)
      .ThenAdd<global::EntityPrototype>(x => {
        x.TransformMode = EntityPrototypeTransformMode.Transform2D;
        x.PhysicsCollider.IsEnabled = true;
        x.PhysicsCollider.Shape2D = new Shape2DConfig() {
          BoxExtents = FP._0_50 * FPVector2.One,
          ShapeType = Shape2DType.Box,
        };
      })
      .ThenAdd<global::EntityView>()
      .Finish(mc);


    [MenuItem("GameObject/Quantum/2D/Circle Entity", false, 10)]
    private static void CreateCircleEntity2D(MenuCommand mc) => ObjectFactory.CreatePrimitive(PrimitiveType.Sphere)
      .ThenRemove<Collider>()
      .ThenAlter<MeshFilter>(x => x.sharedMesh = CircleMesh)
      .ThenAdd<global::EntityPrototype>(x => {
        x.TransformMode = EntityPrototypeTransformMode.Transform2D;
        x.PhysicsCollider.IsEnabled = true;
        x.PhysicsCollider.Shape2D = new Shape2DConfig() {
          CircleRadius = FP._0_50,
          ShapeType = Shape2DType.Circle,
        };
      })
      .ThenAdd<global::EntityView>()
      .Finish(mc);

    [MenuItem("GameObject/Quantum/2D/Quad Static Collider", false, 10)]
    private static void CreateQuadStaticCollider(MenuCommand mc) => ObjectFactory.CreatePrimitive(PrimitiveType.Cube)
      .ThenRemove<Collider>()
      .ThenAlter<MeshFilter>(x => x.sharedMesh = QuadMesh)
      .ThenAdd<QuantumStaticBoxCollider2D>(x => x.Size = FPVector2.One)
      .Finish(mc);

    [MenuItem("GameObject/Quantum/2D/Circle Static Collider", false, 10)]
    private static void CreateCircleStaticCollider(MenuCommand mc) => ObjectFactory.CreatePrimitive(PrimitiveType.Sphere)
      .ThenRemove<Collider>()
      .ThenAlter<MeshFilter>(x => x.sharedMesh = CircleMesh)
      .ThenAdd<QuantumStaticCircleCollider2D>(x => x.Radius = FP._0_50)
      .Finish(mc);

    [MenuItem("GameObject/Quantum/3D/Box Entity", false, 10)]
    private static void CreateBoxEntity(MenuCommand mc) => ObjectFactory.CreatePrimitive(PrimitiveType.Cube)
      .ThenRemove<Collider>()
      .ThenAdd<global::EntityPrototype>(x => {
        x.TransformMode = EntityPrototypeTransformMode.Transform3D;
        x.PhysicsCollider.IsEnabled = true;
        x.PhysicsCollider.Shape3D = new Shape3DConfig() {
          BoxExtents = FP._0_50 * FPVector3.One,
          ShapeType = Shape3DType.Box,
        };
      })
      .ThenAdd<global::EntityView>()
      .Finish(mc);

    [MenuItem("GameObject/Quantum/3D/Sphere Entity", false, 10)]
    private static void CreateSphereEntity(MenuCommand mc) => ObjectFactory.CreatePrimitive(PrimitiveType.Sphere)
      .ThenRemove<Collider>()
      .ThenAdd<global::EntityPrototype>(x => {
        x.TransformMode = EntityPrototypeTransformMode.Transform3D;
        x.PhysicsCollider.IsEnabled = true;
        x.PhysicsCollider.Shape3D = new Shape3DConfig() {
          SphereRadius = FP._0_50,
          ShapeType = Shape3DType.Sphere,
        };
      })
      .ThenAdd<global::EntityView>()
      .Finish(mc);

    [MenuItem("GameObject/Quantum/3D/Box Static Collider", false, 10)]
    private static void CreateBoxStaticCollider(MenuCommand mc) => ObjectFactory.CreatePrimitive(PrimitiveType.Cube)
      .ThenRemove<Collider>()
      .ThenAdd<QuantumStaticBoxCollider3D>(x => x.Size = FPVector3.One)
      .Finish(mc);


    [MenuItem("GameObject/Quantum/3D/Sphere Static Collider", false, 10)]
    private static void CreateSphereStaticCollider(MenuCommand mc) => ObjectFactory.CreatePrimitive(PrimitiveType.Sphere)
      .ThenRemove<Collider>()
      .ThenAdd<QuantumStaticSphereCollider3D>(x => x.Radius = FP._0_50)
      .Finish(mc);

    private static GameObject ThenRemove<T>(this GameObject go) where T : Component {
      UnityEngine.Object.DestroyImmediate(go.GetComponent<T>());
      return go;
    }

    private static GameObject ThenAdd<T>(this GameObject go, System.Action<T> callback = null) where T : Component {
      var component = go.AddComponent<T>();
      callback?.Invoke(component);
      return go;
    }

    private static GameObject ThenAlter<T>(this GameObject go, System.Action<T> callback) where T : Component {
      var component = go.GetComponent<T>();
      callback(component);
      return go;
    }

    private static void Finish(this GameObject go, MenuCommand mc, string select = null, [CallerMemberName] string callerName = null) {
      Debug.Assert(callerName.StartsWith("Create"));
      go.name = callerName.Substring("Create".Length);
      GameObjectUtility.SetParentAndAlign(go, mc.context as GameObject);
      Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
      if (!string.IsNullOrEmpty(select)) {
        Selection.activeObject = go.transform.Find(select)?.gameObject;
      } else {
        Selection.activeObject = go;
      }
    }

    private static Mesh LoadMesh(string name, ref Mesh field) {
      if (field != null)
        return field;
      var fullName = name;
#if QUANTUM_XY
      fullName += "XY";
#else
      fullName += "XZ";
#endif
      const string resourcePath = "QuantumShapes2D";

      field = UnityEngine.Resources.LoadAll<Mesh>(resourcePath).FirstOrDefault(x => x.name.Equals(fullName, System.StringComparison.OrdinalIgnoreCase));
      if (field == null) {
        Debug.LogError($"Mesh not found: {fullName} in resource {resourcePath}.");
      }
      return field;
    }

  }
}