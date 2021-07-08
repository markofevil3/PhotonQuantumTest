using System;
using UnityEditor;

// Disable this class completely by setting this define in the Unity player settings
#if !DISABLE_QUANTUM_ASSET_INSPECTOR

namespace Quantum.Editor {
  [CustomEditor(typeof(AssetBase), true)]
  public class AssetBaseEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {
      if (!QuantumEditorSettings.Instance.UseQuantumAssetInspector) {
        // Soft deactivate the Quantum asset editor
        base.OnInspectorGUI();
        return;
      }
      
      try {
        // call on-gui before
        ((AssetBase)target).OnInspectorGUIBefore(serializedObject);
      }
      catch (System.Exception exn) {
        UnityEngine.Debug.LogException(exn);
      }

      // This draws all fields except the "Settings" property.
      CustomEditorsHelper.DrawDefaultInspector(serializedObject, new string[] { "Settings" });

      // Retrieve name of the nested Quantum asset class.
      var headline = "Quantum Asset Inspector";
      try {
        headline = ObjectNames.NicifyVariableName(((AssetBase)target).AssetObject.GetType().Name);
      }
      catch (Exception) { }

      using (new CustomEditorsHelper.BoxScope(headline)) {
        // We know the data is called "Settings" so we can go ahead and only render the content of "Settings".
        CustomEditorsHelper.DrawDefaultInspector(serializedObject, "Settings", new string[] { }, false);
      }
      
      try {
        // call on-gui after
        ((AssetBase)target).OnInspectorGUIAfter(serializedObject);
      }
      catch (System.Exception exn) {
        UnityEngine.Debug.LogException(exn);
      }
    }
  }
}

#endif