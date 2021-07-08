using System;
using Quantum;
using UnityEngine;
using Photon.Deterministic;

[ExecuteInEditMode]
public class QuantumStaticTerrainCollider3D : MonoBehaviour {
  public TerrainColliderAsset                     Asset;
  public Quantum.MapStaticCollider3D.MutableModes Mode;
  
  [HideInInspector]
  public Boolean SmoothSphereMeshCollisions = false;

  public void Bake() {
    FPMathUtils.LoadLookupTables();

    var t = GetComponent<Terrain>();

#if UNITY_2019_3_OR_NEWER
    Asset.Settings.Resolution = t.terrainData.heightmapResolution;
#else
    Asset.Settings.Resolution = t.terrainData.heightmapResolution;
#endif

    Asset.Settings.HeightMap = new FP[Asset.Settings.Resolution * Asset.Settings.Resolution];
    Asset.Settings.Position  = transform.position.ToFPVector3();
    Asset.Settings.Scale     = t.terrainData.heightmapScale.ToFPVector3();

    for (int i = 0; i < Asset.Settings.Resolution; i++) {
      for (int j = 0; j < Asset.Settings.Resolution; j++) {
        Asset.Settings.HeightMap[j + i * Asset.Settings.Resolution] = FP.FromFloat_UNSAFE(t.terrainData.GetHeight(i, j));
      }
    }

#if UNITY_EDITOR
    UnityEditor.EditorUtility.SetDirty(Asset);
    UnityEditor.EditorUtility.SetDirty(this);
#endif
  }
}