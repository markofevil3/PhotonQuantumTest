using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(QuantumStaticTerrainCollider3D), true)]
  public class TerrainDataEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() {
      base.DrawDefaultInspector();

      var data = target as QuantumStaticTerrainCollider3D;
      if (data) {

        if (data.Asset) {
          EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);

          if (GUILayout.Button("Bake Terrain Data", EditorStyles.miniButton)) {
            Debug.Log("Baking Terrain Data");
            data.Bake();
            EditorUtility.SetDirty(data.Asset);
            data.Asset.Loaded();
            AssetDatabase.Refresh();
          }

          

          EditorGUI.EndDisabledGroup();
        }

        OnInspectorGUI(data);

        CustomEditorsHelper.DrawHeadline("Experimental");
        data.SmoothSphereMeshCollisions = EditorGUI.Toggle(EditorGUILayout.GetControlRect(), "Smooth Sphere Mesh Collisions", data.SmoothSphereMeshCollisions);
      }
    }

    void OnInspectorGUI(QuantumStaticTerrainCollider3D data) {
      //data.transform.position = Vector3.zero;

      if (data.Asset) {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Asset Settings", EditorStyles.boldLabel);

        var asset = new SerializedObject(data.Asset);
        var property = asset.GetIterator();

        // enter first child
        property.Next(true);

        while (property.Next(false)) {
          if (property.name.StartsWith("m_")) {
            continue;
          }

          EditorGUILayout.PropertyField(property, true);
        }

        asset.ApplyModifiedProperties();
      }
    }
  }
}