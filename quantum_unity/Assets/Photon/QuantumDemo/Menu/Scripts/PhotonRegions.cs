using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quantum.Demo {
  [CreateAssetMenu(menuName = "Quantum/Demo/PhotonRegions", order = EditorDefines.AssetMenuPriorityDemo)]
  public class PhotonRegions : ScriptableObject {
    [Serializable]
    public struct RegionInfo {
      public string Name;
      public string Token;
    }

    public List<RegionInfo> Regions = new List<RegionInfo>();
  }
}
