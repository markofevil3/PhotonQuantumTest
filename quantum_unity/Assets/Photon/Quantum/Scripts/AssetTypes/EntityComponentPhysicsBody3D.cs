using Quantum;
using Quantum.Prototypes;
using UnityEngine;

[RequireComponent(typeof(EntityComponentPhysicsCollider3D))]
public partial class EntityComponentPhysicsBody3D {

#if UNITY_EDITOR
  
  private void OnValidate() {
    while (Prototype.Version < PhysicsCommon.BODY_PROTOTYPE_VERSION) {
      switch (Prototype.Version) {
        case 0:
          Prototype.Config |= PhysicsBody3D.ConfigFlags.IsAwakenedByForces;
          break;
      }

      ++Prototype.Version;
    }
    Debug.Assert(Prototype.Version == PhysicsCommon.BODY_PROTOTYPE_VERSION);
  }

  [ContextMenu("Migrate To EntityPrototype")]
  public void Migrate() {
    var parent = GetComponent<EntityPrototype>();
    UnityEditor.Undo.RecordObject(parent, "Migrate");
    parent.PhysicsBody.IsEnabled = true;
    parent.PhysicsBody.Version = Prototype.Version;
    parent.PhysicsBody.AngularDrag = Prototype.AngularDrag;
    parent.PhysicsBody.Drag = Prototype.Drag;
    parent.PhysicsBody.Mass = Prototype.Mass;
    parent.PhysicsBody.RotationFreeze = Prototype.RotationFreeze;
    parent.PhysicsBody.CenterOfMass3D = Prototype.CenterOfMass;
    UnityEditor.Undo.DestroyObjectImmediate(this);
  }

#endif
}