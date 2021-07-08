using Photon.Deterministic;
using Quantum;
using System;
using Quantum.Core;
using UnityEngine;
using Assert = Quantum.Assert;

public static class QuantumGameGizmos {

  public static unsafe void OnDrawGizmos(QuantumGame game) {
#if UNITY_EDITOR
    var frame = game.Frames.Predicted;
    
    if (frame != null) {

      #region Components 

      // ################## Components: PhysicsCollider2D ##################

      foreach(var (handle, c) in frame.GetComponentIterator<PhysicsCollider2D>()) {
        var t      = frame.Unsafe.GetPointer<Transform2D>(handle);
        var s      = c.Shape;
        var hasBody = frame.Unsafe.TryGetPointer<PhysicsBody2D>(handle, out var body);
        
        var hasTransformVertical = frame.Unsafe.TryGetPointer<Transform2DVertical>(handle, out var tVertical);

        Color color;
        if (hasBody) {
          if (body->IsKinematic) {
            color = QuantumEditorSettings.Instance.KinematicColliderColor;
          }
          else if (body->IsSleeping) {
            color = QuantumEditorSettings.Instance.AsleepColliderColor;
          } 
          else if (!body->Enabled) {
            color = QuantumEditorSettings.Instance.DisabledColliderColor;
          }
          else {
            color = QuantumEditorSettings.Instance.DynamicColliderColor;
          }
        }
        else {
          color = QuantumEditorSettings.Instance.KinematicColliderColor;
        }

        // Set 3d position of 2d object to simulate the vertical offset.
        var height = 0.0f;
        
#if QUANTUM_XY
        if (hasTransformVertical) {
          height = -tVertical->Height.AsFloat;
        }
#else
        if (hasTransformVertical) {
          height = tVertical->Height.AsFloat;
        }
#endif

        if (c.Shape.Type == Shape2DType.Compound) {
          DrawCompoundShape2D(frame, &c.Shape, t, tVertical, color, height);
        }
        else {
            var pos = t->Position.ToUnityVector3();
            var rot = t->Rotation.ToUnityQuaternion();
            
  #if QUANTUM_XY
            if (hasTransformVertical) {
              pos.z = -tVertical->Position.AsFloat;
            }
  #else
            if (hasTransformVertical) {
              pos.y = tVertical->Position.AsFloat;
            }
  #endif
            
            DrawShape2DGizmo(c.Shape, pos, rot, color, height, frame);
        }
      }

      // ################## Components: PhysicsCollider3D ##################

      foreach(var (handle, collider) in frame.GetComponentIterator<PhysicsCollider3D>()) {
        var transform = frame.Unsafe.GetPointer<Transform3D>(handle);
        frame.Unsafe.TryGetPointer(handle, out PhysicsBody3D* body);

        Color color;
        if (body != null) {
          if (body->IsKinematic) {
            color = QuantumEditorSettings.Instance.KinematicColliderColor;
          }
          else if (body->IsSleeping) {
            color = QuantumEditorSettings.Instance.AsleepColliderColor;
          } 
          else if (!body->Enabled) {
            color = QuantumEditorSettings.Instance.DisabledColliderColor;
          }
          else {
            color = QuantumEditorSettings.Instance.DynamicColliderColor;
          }
        }
        else {
          color = QuantumEditorSettings.Instance.KinematicColliderColor;
        }
        
        if (collider.Shape.Type == Shape3DType.Compound) {
          DrawCompoundShape3D(frame, &collider.Shape, transform, color);
        }
        else {
          DrawShape3DGizmo(collider.Shape, transform->Position.ToUnityVector3(), transform->Rotation.ToUnityQuaternion(), color);
        }
      }

      // ################## Components: CharacterController3D ##################

      foreach(var (entity, kcc3D) in frame.GetComponentIterator<CharacterController3D>()) {
        var t      = frame.Unsafe.GetPointer<Transform3D>(entity);
        var config = frame.FindAsset(kcc3D.Config);
        var color  = QuantumEditorSettings.Instance.CharacterControllerColor;
        var color2 = QuantumEditorSettings.Instance.AsleepColliderColor;
        GizmoUtils.DrawGizmosSphere(t->Position.ToUnityVector3() + config.Offset.ToUnityVector3(), config.Radius.AsFloat,                         false, QuantumEditorSettings.Instance.CharacterControllerColor);
        GizmoUtils.DrawGizmosSphere(t->Position.ToUnityVector3() + config.Offset.ToUnityVector3(), config.Radius.AsFloat + config.Extent.AsFloat, false, QuantumEditorSettings.Instance.AsleepColliderColor);
      }

      // ################## Components: CharacterController2D ##################

      foreach(var (entity, kcc2D) in frame.GetComponentIterator<CharacterController2D>()) {
        var t      = frame.Unsafe.GetPointer<Transform2D>(entity);
        var config = frame.FindAsset(kcc2D.Config);
        var color  = QuantumEditorSettings.Instance.CharacterControllerColor;
        var color2 = QuantumEditorSettings.Instance.AsleepColliderColor;
        GizmoUtils.DrawGizmosCircle(t->Position.ToUnityVector3() + config.Offset.ToUnityVector3(), config.Radius.AsFloat,                         false, QuantumEditorSettings.Instance.CharacterControllerColor);
        GizmoUtils.DrawGizmosCircle(t->Position.ToUnityVector3() + config.Offset.ToUnityVector3(), config.Radius.AsFloat + config.Extent.AsFloat, false, QuantumEditorSettings.Instance.AsleepColliderColor);
      }

      // ################## Components: NavMeshSteeringAgent ##################

      if (QuantumEditorSettings.Instance.DrawNavMeshAgents) {
        foreach(var (entity, navmeshPathfinderAgent) in frame.GetComponentIterator<NavMeshPathfinder>()) {
          var position = Vector3.zero;

          if (frame.Has<Transform2D>(entity)) {
            position = frame.Unsafe.GetPointer<Transform2D>(entity)->Position.ToUnityVector3();
          }
          else if (frame.Has<Transform3D>(entity)) {
            position = frame.Unsafe.GetPointer<Transform3D>(entity)->Position.ToUnityVector3();
          }

          var config = frame.FindAsset<NavMeshAgentConfig>(navmeshPathfinderAgent.ConfigId);

          if (frame.Has<NavMeshSteeringAgent>(entity)) {
            var steeringAgent = frame.Get<NavMeshSteeringAgent>(entity);
            Gizmos.color = QuantumEditorSettings.Instance.NavMeshAgentColor;
            GizmoUtils.DrawGizmoVector(position, position + steeringAgent.Velocity.XOY.ToUnityVector3());
          }

          if (frame.Has<NavMeshAvoidanceAgent>(entity)) {
            var avoidanceRadius = config.OverrideRadiusForAvoidance ? config.AvoidanceRadius : config.Radius;
            GizmoUtils.DrawGizmosCircle(position, avoidanceRadius.AsFloat, false, QuantumEditorSettings.Instance.NavMeshAvoidanceColor);
          }

          UnityEditor.Handles.color = QuantumEditorSettings.Instance.NavMeshAgentColor;
          GizmoUtils.DrawGizmosCircle(position, config.Radius.AsFloat, false, QuantumEditorSettings.Instance.NavMeshAgentColor);
        }

        foreach(var (entity, navmeshObstacles) in frame.GetComponentIterator<NavMeshAvoidanceObstacle>()) {
          var position = Vector3.zero;

          if (frame.Has<Transform2D>(entity)) {
            position = frame.Unsafe.GetPointer<Transform2D>(entity)->Position.ToUnityVector3();
          }
          else if (frame.Has<Transform3D>(entity)) {
            position = frame.Unsafe.GetPointer<Transform3D>(entity)->Position.ToUnityVector3();
          }

          GizmoUtils.DrawGizmosCircle(position, navmeshObstacles.Radius.AsFloat, false, QuantumEditorSettings.Instance.NavMeshAvoidanceColor);

          if (navmeshObstacles.Velocity != FPVector2.Zero) {
            GizmoUtils.DrawGizmoVector(position, position + navmeshObstacles.Velocity.XOY.ToUnityVector3());
          }
        }
      }

      #endregion

      #region Navmesh And Pathfinder

      // ################## NavMeshes ##################

      if (QuantumEditorSettings.Instance.DrawNavMesh) {
        var navmeshes = frame.Map.NavMeshes.Values;
        foreach (var navmesh in navmeshes) {
          MapNavMesh.CreateAndDrawGizmoMesh(navmesh, *frame.NavMeshRegionMask);

          for (Int32 i = 0; i < navmesh.Triangles.Length; i++) {
            var t = navmesh.Triangles[i];

            // Links
            for (Int32 l = 0; l < t.Links.Length; l++) {
              var color = Color.blue;
              if (t.Links[l].Region.IsSubset(*frame.NavMeshRegionMask) == false) {
                color = Color.gray;
              }

              Gizmos.color = color;
              GizmoUtils.DrawGizmoVector(t.Links[l].Start.ToUnityVector3(), t.Links[l].End.ToUnityVector3());
              GizmoUtils.DrawGizmosCircle(t.Links[l].Start.ToUnityVector3(), 0.1f, color);
              GizmoUtils.DrawGizmosCircle(t.Links[l].End.ToUnityVector3(), 0.1f, color);
            }

            if (QuantumEditorSettings.Instance.DrawNavMeshRegionIds) {
              if (t.Regions.HasValidRegions) {
                var s = string.Empty;
                for (int r = 0; r < frame.Map.Regions.Length; r++) {
                  if (t.Regions.IsRegionEnabled(r)) {
                    s += $"{frame.Map.Regions[r]} ({r})";
                  }
                }

                var vertex0 = ToUnityVector3_QUANTUM_XY(navmesh.Vertices[t.Vertex0].Point);
                var vertex1 = ToUnityVector3_QUANTUM_XY(navmesh.Vertices[t.Vertex1].Point);
                var vertex2 = ToUnityVector3_QUANTUM_XY(navmesh.Vertices[t.Vertex2].Point);
                UnityEditor.Handles.Label((vertex0 + vertex1 + vertex2) / 3.0f, s);
              }
            }
          }

          if (QuantumEditorSettings.Instance.DrawNavMeshVertexNormals) {
            Gizmos.color = Color.blue;
            for (Int32 v = 0; v < navmesh.Vertices.Length; ++v) {
              if (navmesh.Vertices[v].Borders.Length >= 2) {
                var normal = NavMeshVertex.CalculateNormal(v, navmesh, *frame.NavMeshRegionMask);
                if (normal != FPVector3.Zero) {
                  GizmoUtils.DrawGizmoVector(
                                             ToUnityVector3_QUANTUM_XY(navmesh.Vertices[v].Point),
                                             ToUnityVector3_QUANTUM_XY(navmesh.Vertices[v].Point) +
                                             ToUnityVector3_QUANTUM_XY(normal) / 3.0f);
                }
              }
            }
          }
        }
      }

      // ################## NavMesh Borders ##################

      if (QuantumEditorSettings.Instance.DrawNavMeshBorders) {
        Gizmos.color = Color.blue;
        var navmeshes = frame.Map.NavMeshes.Values;
        foreach (var navmesh in navmeshes) {
          for (Int32 i = 0; i < navmesh.BorderGrid.Length; i++) {
            if (navmesh.BorderGrid[i].Borders != null) {
              for (int j = 0; j < navmesh.BorderGrid[i].Borders.Length; j++) {
                var b = navmesh.Borders[navmesh.BorderGrid[i].Borders[j]];
                if (b.Regions.HasValidRegions && b.Regions.IsSubset(*frame.NavMeshRegionMask)) {
                  // grayed out?
                  continue;
                }

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(ToUnityVector3_QUANTUM_XY(b.V0), ToUnityVector3_QUANTUM_XY(b.V1));

                //// How to do a thick line? Multiple GizmoDrawLine also possible.
                //var color = QuantumEditorSettings.Instance.GetNavMeshColor(b.Regions);
                //UnityEditor.Handles.color = color;
                //UnityEditor.Handles.lighting = true;
                //UnityEditor.Handles.DrawAAConvexPolygon(
                //  ToUnityVector3_QUANTUM_XY(b.V0), 
                //  ToUnityVector3_QUANTUM_XY(b.V1), 
                //  ToUnityVector3_QUANTUM_XY(b.V1) + Vector3.up * 0.05f,
                //  ToUnityVector3_QUANTUM_XY(b.V0) + Vector3.up * 0.05f);
              }
            }
          }
        }
      }

      // ################## NavMesh Triangle Ids ##################

      if (QuantumEditorSettings.Instance.DrawNavMeshTriangleIds) {
        UnityEditor.Handles.color = Color.white;
        var navmeshes = frame.Map.NavMeshes.Values;
        foreach (var navmesh in navmeshes) {
          for (Int32 i = 0; i < navmesh.Triangles.Length; i++) {
            UnityEditor.Handles.Label(ToUnityVector3_QUANTUM_XY(navmesh.Triangles[i].Center), i.ToString());
          }
        }
      }

      // ################## Pathfinder ##################

      if (frame.Navigation != null) {

        // Iterate though task contexts:
        var threadCount = frame.Context.TaskContext.ThreadCount;
        for (int t = 0; t < threadCount; t++) {

          // Iterate through path finders:
          var pf = frame.Navigation.GetDebugInformation(t).Item0;
          if (pf.RawPathSize >= 2) {
            if (QuantumEditorSettings.Instance.DrawPathfinderFunnel) {
              for (Int32 i = 0; i < pf.PathSize; i++) {
                var point = pf.Path[i].Point.XZ;
                GizmoUtils.DrawGizmosCircle(point.ToUnityVector3(), 0.05f, pf.Path[i].IsLink ? Color.black : Color.green);
                if (i > 0) {
                  Gizmos.color = pf.Path[i].IsLink && pf.Path[i - 1].IsLink ? Color.black : Color.green;
                  Gizmos.DrawLine(point.ToUnityVector3(), pf.Path[i - 1].Point.ToUnityVector3());
                }
              }
            }

            if (QuantumEditorSettings.Instance.DrawPathfinderRawPath) {
              for (Int32 i = 0; i < pf.RawPathSize; i++) {
                var point = pf.RawPath[i].Point.XZ;
                GizmoUtils.DrawGizmosCircle(point.ToUnityVector3(), 0.1f, pf.RawPath[i].IsLink ? Color.black : Color.magenta);
                if (i > 0) {
                  Gizmos.color = pf.RawPath[i].IsLink && pf.RawPath[i - 1].IsLink ? Color.black : Color.magenta;
                  Gizmos.DrawLine(point.ToUnityVector3(), pf.RawPath[i - 1].Point.ToUnityVector3());
                }
              }
            }

            if (QuantumEditorSettings.Instance.DrawPathfinderRawTrianglePath) {
              var nmGuid = frame.Navigation.GetDebugInformation(t).Item1;
              if (!string.IsNullOrEmpty(nmGuid)) {
                var nm = UnityDB.FindAsset<NavMeshAsset>(nmGuid).Settings;
                for (Int32 i = 0; i < pf.RawPathSize; i++) {
                  var triangleIndex = pf.RawPath[i].Index;
                  if (triangleIndex >= 0) {
                    var vertex0 = ToUnityVector3_QUANTUM_XY(nm.Vertices[nm.Triangles[triangleIndex].Vertex0].Point);
                    var vertex1 = ToUnityVector3_QUANTUM_XY(nm.Vertices[nm.Triangles[triangleIndex].Vertex1].Point);
                    var vertex2 = ToUnityVector3_QUANTUM_XY(nm.Vertices[nm.Triangles[triangleIndex].Vertex2].Point);
                    GizmoUtils.DrawGizmosTriangle(vertex0, vertex1, vertex2, true, Color.magenta);
                    UnityEditor.Handles.color = Color.magenta.Alpha(0.25f);
                    UnityEditor.Handles.lighting = true;
                    UnityEditor.Handles.DrawAAConvexPolygon(vertex0, vertex1, vertex2);
                  }
                }
              }
            }
          }
        }
      }

      #endregion

      #region Various

      // ################## Prediction Area ##################

      if (QuantumEditorSettings.Instance.DrawPredictionArea && frame.Context.Culling != null) {
        var context = frame.Context;
        if (context.PredictionAreaRadius != FP.UseableMax) {
#if QUANTUM_XY
          // The Quantum simulation does not know about QUANTUM_XY and always keeps the vector2 Y component in the vector3 Z component.
          var predictionAreaCenter = new UnityEngine.Vector3(context.PredictionAreaCenter.X.AsFloat, context.PredictionAreaCenter.Z.AsFloat, 0);
#else
          var predictionAreaCenter = context.PredictionAreaCenter.ToUnityVector3();
#endif
          GizmoUtils.DrawGizmosSphere(predictionAreaCenter, context.PredictionAreaRadius.AsFloat, QuantumEditorSettings.Instance.PredictionAreaColor);
        }
      }

      #endregion
    }
#endif
  }

  private static Vector3 ToUnityVector3_QUANTUM_XY(Photon.Deterministic.FPVector3 v) {
#if QUANTUM_XY
    // Quantum NavMesh, although saving 3D vectors, is always in XZ layout. Adjust the gizmo rendering here for QUANTUM_XY.
    return new Vector3(v.X.AsFloat, v.Z.AsFloat, v.Y.AsFloat);
#else
    return v.ToUnityVector3();
#endif
  }

  public static unsafe void DrawShape3DGizmo(Shape3D s, Vector3 position, Quaternion rotation, Color color) {

    var localOffset = s.LocalTransform.Position.ToUnityVector3();
    var localRotation = s.LocalTransform.Rotation.ToUnityQuaternion();

    position += rotation * localOffset;
    rotation *= localRotation;

    switch (s.Type) {
      case Shape3DType.Sphere:
        GizmoUtils.DrawGizmosSphere(position, s.Sphere.Radius.AsFloat, false, color);
        break;
      case Shape3DType.Box:
        GizmoUtils.DrawGizmosBox(position, rotation, s.Box.Extents.ToUnityVector3() * 2, false, color);
        break;
    }
  }

  public static unsafe void DrawShape2DGizmo(Shape2D s, Vector3 pos, Quaternion rot, Color color, float height, Frame currentFrame ) {

    var localOffset = s.LocalTransform.Position.ToUnityVector3();
    var localRotation = s.LocalTransform.Rotation.ToUnityQuaternion();

    pos += rot * localOffset;
    rot = rot * localRotation;

    switch (s.Type) {
      case Shape2DType.Circle:
        GizmoUtils.DrawGizmosCircle(pos, s.Circle.Radius.AsFloat, height, color);
        break;

      case Shape2DType.Box:
        var size = s.Box.Extents.ToUnityVector3() * 2.0f;
#if QUANTUM_XY
        size.z = height;
        pos.z += height * 0.5f;
#else
        size.y = height;
        pos.y += height * 0.5f;
#endif
        GizmoUtils.DrawGizmosBox(pos, rot, size, false, color);

        break;

      case Shape2DType.Polygon:
        PolygonCollider p;
        if (currentFrame != null) {
          p = currentFrame.FindAsset(s.Polygon.AssetRef);
        } else {
          p = UnityDB.FindAsset<PolygonColliderAsset>(s.Polygon.AssetRef.Id)?.Settings;
        }

        if (p != null) {
          GizmoUtils.DrawGizmoPolygon2D(pos, rot, p.Vertices, height, color);
        }
        break;


      case Shape2DType.Edge:
        var extent = rot * Vector3.right * s.Edge.Extent.AsFloat;
        GizmoUtils.DrawGizmosEdge(pos - extent, pos + extent, height, false, color);
        break;
    }
  }
  
  private static unsafe void DrawCompoundShape2D(Frame f, Shape2D* compoundShape, Transform2D* transform, Transform2DVertical* transformVertical, Color color, float height) {
    Debug.Assert(compoundShape->Type == Shape2DType.Compound);

    if (compoundShape->Compound.GetShapes(f, out var shapesBuffer, out var count)) {
      for (var i = 0; i < count; i++) {
        var shape = shapesBuffer + i;

        if (shape->Type == Shape2DType.Compound) {
          DrawCompoundShape2D(f, shape, transform, transformVertical, color, height);
        }
        else {
          var pos = transform->Position.ToUnityVector3();
          var rot = transform->Rotation.ToUnityQuaternion();
                  
      #if QUANTUM_XY
          if (transformVertical != null) {
            pos.z = -transformVertical->Position.AsFloat;
          }
      #else
          if (transformVertical != null) {
            pos.y = transformVertical->Position.AsFloat;
          }
      #endif
                  
          DrawShape2DGizmo(*shape, pos, rot, color, height, f);
        }
      }
    }
  }
  
  private static unsafe void DrawCompoundShape3D(Frame f, Shape3D* compoundShape, Transform3D* transform, Color color) {
    Debug.Assert(compoundShape->Type == Shape3DType.Compound);

    if (compoundShape->Compound.GetShapes(f, out var shapesBuffer, out var count)) {
      for (var i = 0; i < count; i++) {
        var shape = shapesBuffer + i;

        if (shape->Type == Shape3DType.Compound) {
          DrawCompoundShape3D(f, shape, transform, color);
        } else {
          DrawShape3DGizmo(*shape, transform->Position.ToUnityVector3(), transform->Rotation.ToUnityQuaternion(), color);
        }
      }
    }
  }
}