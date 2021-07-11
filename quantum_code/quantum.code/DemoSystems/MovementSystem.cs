using System;
using System.Collections.Generic;
using Photon.Deterministic;

namespace Quantum {
  public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>, ISignalOnPlayerDataSet {
    public struct Filter {
      public EntityRef Entity;
      public CharacterController3D* KCC;
      public Transform3D* Transform;
      public PlayerLink* PlayerLink;
    }

    Int32 speed = 10;
    
    public override void Update(Frame f, ref Filter filter) {
      BattlePlayer* battlePlayer = f.Global -> Players.GetPointer(filter.PlayerLink->PlayerRef);
      var spawnTransform = f.Get<Transform3D>(battlePlayer->TargetMapNode);
      // If reach destination -> continue to next one
      if (FPVector3.Distance(filter.Transform -> Position, spawnTransform.Position) < 1) {
        // MapNode mapNode = f.Get<MapNode>(battlePlayer->TargetMapNode);
        // MapNodeSpec mapNodeSpec = f.FindAsset<MapNodeSpec>(mapNode.Spec.Id);
        // if (mapNodeSpec != null && mapNodeSpec.NextNodes.Length > 0 && mapNodeSpec.NextNodes[0] != null) {
        //   battlePlayer -> TargetMapNode = mapNode.NextNodes[0];
        // }
        // When player reach node -> wait for input to select new node
        battlePlayer -> ReachedNode = true;
      } else {
        var dir=(spawnTransform.Position - filter.Transform->Position).Normalized;
        // filter.KCC->Move(f, filter.Entity, (dir * speed * f.DeltaTime));
        filter.Transform -> Position += (dir * speed * f.DeltaTime);
        // Keep player look straight
        // dir.Y = 1;
        var q = FPQuaternion.LookRotation(dir);

        filter.Transform -> Rotation = FPQuaternion.RotateTowards(filter.Transform->Rotation, q, 300 * f.DeltaTime);
      }
    }

    public void OnPlayerDataSet(Frame f, PlayerRef player) {
      RuntimePlayer playerData = f.GetPlayerData(player);
      EntityPrototype characterPrototype = f.FindAsset<EntityPrototype>(playerData.CharacterPrototype.Id);
      EntityRef characterRef = f.Create(characterPrototype);

      if (f.Unsafe.TryGetPointer<PlayerLink>(characterRef, out var pl)) {
        pl->PlayerRef = player;
      }
      List<EntityComponentPair<MapNode>> spawnableMapNode = new List<EntityComponentPair<MapNode>>();
      // Pick a random position on map??
      foreach (var mapNode in f.GetComponentIterator<MapNode>()) {
        MapNodeSpec mapNodeSpec = f.FindAsset<MapNodeSpec>(mapNode.Component.Spec.Id);
        if (mapNodeSpec.IsFirstNode && !mapNode.Component.Occupied) {
          spawnableMapNode.Add(mapNode);
        }
      }
      var index = f.RNG->Next(0, spawnableMapNode.Count);

      var spawnTransform = f.Get<Transform3D>(spawnableMapNode[index].Entity);
      f.Unsafe.GetPointer<Transform3D>(characterRef)->Position = spawnTransform.Position;
      f.Unsafe.GetPointer<MapNode>(spawnableMapNode[index].Entity) -> Occupied = true;
      // Set target node to next node
      BattlePlayer* battlePlayer = f.Global -> Players.GetPointer(player);
      battlePlayer->PlayerRef = player;
      battlePlayer -> TargetMapNode = spawnableMapNode[index].Component.NextNodes[0];
    }
  }
}