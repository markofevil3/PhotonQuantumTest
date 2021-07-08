using System;
using UnityEngine;

public class EntityViewAsset : AssetBase, IQuantumPrefabNestedAsset<EntityView> {
  public Quantum.EntityView Settings;

  public EntityView Parent;

  [Obsolete("Use View instead")]
  public EntityView Prefab => View;
  public EntityView View => Parent;

  public override Quantum.AssetObject AssetObject => Settings;

  Component IQuantumPrefabNestedAsset.Parent => Parent;

  public override void Reset() {
    if (Settings == null) {
      Settings = new Quantum.EntityView();
    }

    base.Reset();
  }
}
