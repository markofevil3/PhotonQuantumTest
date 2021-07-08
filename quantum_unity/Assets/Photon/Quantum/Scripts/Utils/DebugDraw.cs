using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quantum {
  public static class DebugDraw {
    static Queue<Draw.DebugRay> _rays = new Queue<Draw.DebugRay>();
    static Queue<Draw.DebugRay3D> _rays3D = new Queue<Draw.DebugRay3D>();
    static Queue<Draw.DebugLine> _lines = new Queue<Draw.DebugLine>();
    static Queue<Draw.DebugLine3D> _lines3D = new Queue<Draw.DebugLine3D>();
    static Queue<Draw.DebugCircle> _circles = new Queue<Draw.DebugCircle>();
    static Queue<Draw.DebugSphere> _spheres = new Queue<Draw.DebugSphere>();
    static Queue<Draw.DebugRectangle> _rectangles = new Queue<Draw.DebugRectangle>();

    static Dictionary<ColorRGBA, Material> _materials = new Dictionary<ColorRGBA, Material>(ColorRGBA.EqualityComparer.Instance);

    public static void Ray(Draw.DebugRay ray) {
      lock (_rays) {
        _rays.Enqueue(ray);
      }
    }

    public static void Ray3D(Draw.DebugRay3D ray) {
      lock (_rays3D) {
        _rays3D.Enqueue(ray);
      }
    }

    public static void Line(Draw.DebugLine line) {
      lock (_lines) {
        _lines.Enqueue(line);
      }
    }

    public static void Line3D(Draw.DebugLine3D line) {
      lock (_lines3D) {
        _lines3D.Enqueue(line);
      }
    }

    public static void Circle(Draw.DebugCircle circle) {
      lock (_circles) {
        _circles.Enqueue(circle);
      }
    }

    public static void Sphere(Draw.DebugSphere sphere) {
      lock (_spheres) {
        _spheres.Enqueue(sphere);
      }
    }

    public static void Rectangle(Draw.DebugRectangle rectangle) {
      lock (_rectangles) {
        _rectangles.Enqueue(rectangle);
      }
    }

    public static Material GetMaterial(ColorRGBA color) {
      if (_materials.TryGetValue(color, out var mat) == false) {
        mat = new Material(DebugMesh.DebugMaterial);
        mat.SetColor("_Color", color.ToColor());

        _materials.Add(color, mat);
      }

      return mat;
    }

    static Draw.DebugRay[] _raysArray = new Draw.DebugRay[64];
    static Draw.DebugRay3D[] _raysArray3D = new Draw.DebugRay3D[64];
    static Draw.DebugLine[] _linesArray = new Draw.DebugLine[64];
    static Draw.DebugLine3D[] _linesArray3D = new Draw.DebugLine3D[64];
    static Draw.DebugCircle[] _circlesArray = new Draw.DebugCircle[64];
    static Draw.DebugSphere[] _spheresArray = new Draw.DebugSphere[64];
    static Draw.DebugRectangle[] _rectanglesArray = new Draw.DebugRectangle[64];

    static int _raysCount;
    static int _rays3DCount;
    static int _linesCount;
    static int _lines3DCount;
    static int _circlesCount;
    static int _spheresCount;
    static int _rectanglesCount;

    public static void Clear() {
      TakeAllFromQueueAndClearLocked(_rays,       ref _raysArray);
      TakeAllFromQueueAndClearLocked(_rays3D,     ref _raysArray3D);
      TakeAllFromQueueAndClearLocked(_lines,      ref _linesArray);
      TakeAllFromQueueAndClearLocked(_lines3D,    ref _linesArray3D);
      TakeAllFromQueueAndClearLocked(_circles,    ref _circlesArray);
      TakeAllFromQueueAndClearLocked(_spheres,    ref _spheresArray);
      TakeAllFromQueueAndClearLocked(_rectangles, ref _rectanglesArray);

      _raysCount       = 0;
      _rays3DCount     = 0;
      _linesCount      = 0;
      _lines3DCount    = 0;
      _circlesCount    = 0;
      _spheresCount    = 0;
      _rectanglesCount = 0;
    }

    public static void TakeAll() {
      _raysCount       = TakeAllFromQueueAndClearLocked(_rays,       ref _raysArray);
      _rays3DCount     = TakeAllFromQueueAndClearLocked(_rays3D,     ref _raysArray3D);
      _linesCount      = TakeAllFromQueueAndClearLocked(_lines,      ref _linesArray);
      _lines3DCount    = TakeAllFromQueueAndClearLocked(_lines3D,    ref _linesArray3D);
      _circlesCount    = TakeAllFromQueueAndClearLocked(_circles,    ref _circlesArray);
      _spheresCount    = TakeAllFromQueueAndClearLocked(_spheres,    ref _spheresArray);
      _rectanglesCount = TakeAllFromQueueAndClearLocked(_rectangles, ref _rectanglesArray);
    }

    public static void DrawAll() {
      for (Int32 i = 0; i < _raysCount; ++i) {
        DrawRay(_raysArray[i]);
      }

      for (Int32 i = 0; i < _rays3DCount; ++i) {
        DrawRay(_raysArray3D[i]);
      }

      for (Int32 i = 0; i < _linesCount; ++i) {
        DrawLine(_linesArray[i]);
      }

      for (Int32 i = 0; i < _lines3DCount; ++i) {
        DrawLine(_linesArray3D[i]);
      }

      for (Int32 i = 0; i < _circlesCount; ++i) {
        DrawCircle(_circlesArray[i]);
      }
      //Debug.Log(spheresCount);
      for (Int32 i = 0; i < _spheresCount; ++i) {
        DrawSphere(_spheresArray[i]);
      }

      for (Int32 i = 0; i < _rectanglesCount; ++i) {
        DrawRectangle(_rectanglesArray[i]);
      }
    }

    static void DrawRay(Draw.DebugRay ray) {
      Debug.DrawRay(ray.Origin.ToUnityVector3(), ray.Direction.ToUnityVector3(), ray.Color.ToColor());
    }

    static void DrawRay(Draw.DebugRay3D ray) {
      Debug.DrawRay(ray.Origin.ToUnityVector3(), ray.Direction.ToUnityVector3(), ray.Color.ToColor());
    }

    static void DrawLine(Draw.DebugLine line) {
      Debug.DrawLine(line.Start.ToUnityVector3(), line.End.ToUnityVector3(), line.Color.ToColor());
    }

    static void DrawLine(Draw.DebugLine3D line) {
      Debug.DrawLine(line.Start.ToUnityVector3(), line.End.ToUnityVector3(), line.Color.ToColor());
    }

    static Mesh solidSphere;
    static Material material = UnityEngine.Resources.Load<Material>("PREFABS/Materials/White");

    static Mesh GetSphere() {
      if (solidSphere != null)
        return solidSphere;
      var s = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Sphere);
      solidSphere = s.GetComponent<UnityEngine.MeshFilter>().mesh;
      UnityEngine.GameObject.Destroy(s);
      return solidSphere;
    }

    public static void DrawSphere(Vector3 position, float radius, Color color) {
      Matrix4x4 mat = Matrix4x4.TRS(position, Quaternion.identity, (radius + radius) * Vector3.one);
      MaterialPropertyBlock block = new MaterialPropertyBlock();
      block.SetColor("_Color", color);
      Graphics.DrawMesh(GetSphere(), mat, material, 0, null, 0, block);
    }

    static void DrawSphere(Draw.DebugSphere sphere) {
      //Gizmos.DrawSphere(sphere.Center.ToUnityVector3(), 0.1f);
      DrawSphere(sphere.Center.ToUnityVector3(), sphere.Radius.AsFloat, sphere.Color.ToColor());
    }

    static void DrawCircle(Draw.DebugCircle circle) {
      Quaternion rot;

#if QUANTUM_XY
      rot = Quaternion.Euler(180, 0, 0);
#else
      rot = Quaternion.Euler(-90, 0, 0);
#endif

      // matrix for mesh
      var m = Matrix4x4.TRS(circle.Center.ToUnityVector3(), rot, Vector3.one * (circle.Radius.AsFloat + circle.Radius.AsFloat));

      // draw
      Graphics.DrawMesh(DebugMesh.CircleMesh, m, GetMaterial(circle.Color), 0, null);
    }

    static void DrawRectangle(Draw.DebugRectangle rectangle) {
      Vector3 size = Vector3.one;
      size.x = rectangle.Size.X.AsFloat;
      size.y = rectangle.Size.Y.AsFloat;
      size.z = rectangle.Size.Y.AsFloat;

      Quaternion rot;

#if QUANTUM_XY
      rot = Quaternion.Euler(0, 0, rectangle.Rotation.AsFloat * Mathf.Rad2Deg) * Quaternion.Euler(90, 0, 0);
#else
      rot = Quaternion.Euler(0, -rectangle.Rotation.AsFloat * Mathf.Rad2Deg, 0);
#endif

      var m = Matrix4x4.TRS(rectangle.Center.ToUnityVector3(), rot, size);

      Graphics.DrawMesh(DebugMesh.QuadMesh, m, GetMaterial(rectangle.Color), 0, null);
    }

    static Int32 TakeAllFromQueueAndClearLocked<T>(Queue<T> queue, ref T[] result) {
      lock (queue) {
        var count = 0;

        if (queue.Count > 0) {
          // if result array size is less than queue count
          if (result.Length < queue.Count) {

            // find the next new size that is a multiple of the current result size
            var newSize = result.Length;

            while (newSize < queue.Count) {
              newSize = newSize * 2;
            }

            // and re-size array
            Array.Resize(ref result, newSize);
          }

          // grab all
          while (queue.Count > 0) {
            result[count++] = queue.Dequeue();
          }

          // clear queue
          queue.Clear();
        }

        return count;
      }
    }
  }
}