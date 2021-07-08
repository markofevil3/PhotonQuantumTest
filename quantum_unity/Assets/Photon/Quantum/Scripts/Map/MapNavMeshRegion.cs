using UnityEngine;

public class MapNavMeshRegion : MonoBehaviour {
  public enum RegionCastType {
    CastRegion,
    NoRegion
  }

  [Tooltip("All regions with the same id are toggle-able as one region. Check Map.Regions to see the results.")]
  public string Id;

  [Tooltip("Set to CastRegion when the region should be casted onto the navmesh. For Links for example chose NoRegion.")]
  public RegionCastType CastRegion;
}
