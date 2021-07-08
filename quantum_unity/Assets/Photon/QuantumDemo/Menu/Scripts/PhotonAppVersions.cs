using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quantum.Demo {
  [CreateAssetMenu(menuName = "Quantum/Demo/PhotonAppVersions", order = Quantum.EditorDefines.AssetMenuPriorityDemo)]
  public class PhotonAppVersions : ScriptableObject {
    [Serializable]
    public enum Type {
      UsePrivateAppVersion,
      UsePhotonAppVersion,
      Custom
    }

    public List<string> CustomVersions = new List<string>();

    public string Private {
      get {
        var resources = UnityEngine.Resources.LoadAll<PhotonPrivateAppVersion>("");
        if (resources.Length > 0) {
          return resources[0].Value;
        }

        return null;
      }
    }
  }
}
