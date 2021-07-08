using System;
using Photon.Deterministic;

namespace Quantum {
  public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>, ISignalOnPlayerDataSet {
    public struct Filter {
      public EntityRef Entity;
      public CharacterController3D* KCC;
      public Transform3D* Transform;
      public PlayerLink* PlayerLink;
    }

    public override void Update(Frame f, ref Filter filter) {
      Console.WriteLine($"update===========");

      filter.KCC->Move(f, filter.Entity, FPVector3.Forward);
      filter.Transform -> Rotation = FPQuaternion.LookRotation(FPVector3.Forward);
    }

    public void OnPlayerDataSet(Frame f, PlayerRef player) {
      RuntimePlayer playerData = f.GetPlayerData(player);
      EntityPrototype characterPrototype = f.FindAsset<EntityPrototype>(playerData.CharacterPrototype.Id);
      EntityRef characterRef = f.Create(characterPrototype);

      if (f.TryGet<PlayerLink>(characterRef, out var pl)) {
        pl.PlayerRef = player;
      }
    }
  }
}