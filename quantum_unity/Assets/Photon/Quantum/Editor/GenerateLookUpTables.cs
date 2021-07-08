using Photon.Deterministic;
using UnityEditor;
using UnityEngine;

namespace Quantum {
  public static class GenerateLookUpTables {
    [MenuItem("Quantum/Generate Math Lookup Tables", false, 22)]
    public static void Generate() {
      FPLut.GenerateTables(PathUtils.Combine(Application.dataPath, "Photon/Quantum/Resources/LUT"));

      // this makes sure the tables are loaded into unity
      AssetDatabase.Refresh();
    }
  }
}