using System;
using System.Linq;
using Quantum;
using UnityEngine;

public static class EntityPrototypeUtils {

  public static Shape2DConfig ColliderToShape2D(Transform reference, Component collider, out bool isTrigger) {
    if (collider == null)
      throw new ArgumentNullException(nameof(collider));

    switch (collider) {
      case BoxCollider box:
        ThrowIfDifferentWorldPosition(reference, box.bounds);
        ThrowIfDifferentWorldRotation(reference, box);
        isTrigger = box.isTrigger;
        return new Shape2DConfig() {
          ShapeType = Shape2DType.Box,
          BoxExtents = Vector3.Scale(box.size / 2, box.transform.lossyScale).ToFPVector2()
        };

      case SphereCollider sphere:
        ThrowIfDifferentWorldPosition(reference, sphere.bounds);
        isTrigger = sphere.isTrigger;
        return new Shape2DConfig() {
          ShapeType = Shape2DType.Circle,
          CircleRadius = sphere.radius.ToFP(),
        };

      case BoxCollider2D box:
        ThrowIfDifferentWorldPosition(reference, box.bounds);
        ThrowIfDifferentWorldRotation(reference, box);
        isTrigger = box.isTrigger;
        return new Shape2DConfig() {
          ShapeType = Shape2DType.Box,
          BoxExtents = Vector2.Scale(box.size / 2, box.transform.lossyScale).ToFPVector2()
        };

      case CircleCollider2D sphere:
        ThrowIfDifferentWorldPosition(reference, sphere.bounds);
        isTrigger = sphere.isTrigger;
        return new Shape2DConfig() {
          ShapeType = Shape2DType.Circle,
          CircleRadius = sphere.radius.ToFP(),
        };

      default:
        throw new NotSupportedException(CreateTypeNotSupportedMessage(collider.GetType(), typeof(BoxCollider), typeof(SphereCollider), typeof(BoxCollider2D), typeof(CircleCollider2D)));
    }
  }

  public static Shape3DConfig ColliderToShape3D(Transform reference, Component collider, out bool isTrigger) {
    if (collider == null)
      throw new ArgumentNullException(nameof(collider));

    switch (collider) {
      case BoxCollider box:
        ThrowIfDifferentWorldPosition(reference, box.bounds);
        ThrowIfDifferentWorldRotation(reference, box);
        isTrigger = box.isTrigger;
        return new Shape3DConfig {
          ShapeType = Shape3DType.Box,
          BoxExtents = Vector3.Scale(box.size / 2, box.transform.lossyScale).ToFPVector3(),
        };

      case SphereCollider sphere:
        ThrowIfDifferentWorldPosition(reference, sphere.bounds);
        isTrigger = sphere.isTrigger;
        return new Shape3DConfig {
          ShapeType = Shape3DType.Sphere,
          SphereRadius = sphere.radius.ToFP(),
        };

      default:
        throw new NotSupportedException(CreateTypeNotSupportedMessage(collider.GetType(), typeof(BoxCollider), typeof(SphereCollider)));
    }
  }

  private static string CreateTypeNotSupportedMessage(Type colliderType, params Type[] supportedTypes) {
    return $"Type {colliderType.FullName} not supported, needs to be one of {(string.Join(", ", supportedTypes.Select(x => x.Name)))}";
  }

  private static void ThrowIfDifferentWorldPosition(Transform reference, Bounds bounds) {
    if (bounds.center != reference.position) {
      throw new InvalidOperationException("This collider needs to have the same position (including the offset) as the prototype.");
    }
  }

  private static void ThrowIfDifferentWorldRotation(Transform reference, Component collider) {
    if (collider.transform.rotation != reference.rotation) {
      throw new InvalidOperationException("This collider needs to have the same rotation as the prototype.");
    }
  }
}