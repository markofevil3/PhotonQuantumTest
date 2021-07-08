using System;
using System.Linq;
using System.Reflection;
using Photon.Deterministic;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {

  [CustomEditor(typeof(SimulationConfigAsset))]
  public class SimulationConfigAssetEditor : UnityEditor.Editor {

    private const string RemoveStart = "Default";
    private const string RemoveEnd = "Capacity";

    public override void OnInspectorGUI() {

      var data = (SimulationConfigAsset)target;
      CustomEditorsHelper.DrawScript(target);
      using (new CustomEditorsHelper.BoxScope("SimulationConfig")) {
        CustomEditorsHelper.DrawDefaultInspector(serializedObject, "Settings", new string[] {"Settings.Physics", "Settings.Navigation", "Settings.Entities" }, false);

        EditorGUILayout.Space();
        CustomEditorsHelper.DrawDefaultInspector(serializedObject, "Settings.Entities", new string[] { });
        
        var foldout = false;
        EditorGUILayout.Space();
        
        CustomEditorsHelper.DrawDefaultInspector(serializedObject, "Settings.Physics", new string[] {"Settings.Physics.Layers", "Settings.Physics.LayerMatrix"}, true, ref foldout, (property, field, type) => {
          if (type == typeof(FPVector3)) {
            FPVector3PropertyDrawer.DrawCompact(EditorGUILayout.GetControlRect(), property, new GUIContent(property.displayName));
            return true;
          }
          
          if (property.name.StartsWith(RemoveStart) && property.name.EndsWith(RemoveEnd)) {
            var header = field.GetCustomAttribute<Quantum.Inspector.HeaderAttribute>();
            if (header != null) {
              CustomEditorsHelper.DrawHeadline(header.Header);
            }
            EditorGUILayout.PropertyField(property, new GUIContent(property.displayName.Substring(RemoveStart.Length, property.displayName.Length - RemoveStart.Length - RemoveStart.Length - 1)));
            return true;
          }
          
          return false;
        });

        if (foldout) {
          EditorGUILayout.Space();
          using (new EditorGUI.IndentLevelScope()) {
            EditorGUILayout.LabelField("Layers");
            using (new EditorGUI.IndentLevelScope()) {

              using (new GUILayout.HorizontalScope()) {
                GUILayout.Space(EditorGUI.indentLevel * 15);
                if (GUILayout.Button("Import Layers From Unity (3D)")) {
                  data.ImportLayersFromUnity(SimulationConfigAssetHelper.PhysicsType.Physics3D);
                  EditorUtility.SetDirty(data);
                }

                if (GUILayout.Button("Import Layers From Unity (2D)")) {
                  data.ImportLayersFromUnity(SimulationConfigAssetHelper.PhysicsType.Physics2D);
                  EditorUtility.SetDirty(data);
                }
              }

              if (data.Settings.Physics.LayerMatrix        == null || data.Settings.Physics.Layers        == null ||
                  data.Settings.Physics.LayerMatrix.Length == 0    || data.Settings.Physics.Layers.Length == 0) {
                EditorGUILayout.HelpBox("Physics layers seem broken. Import them from Unity.", MessageType.Warning);
              }
              else {
                try {
                  LayerMatrixGUI_DoGUI(serializedObject);
                }
                catch (Exception) {
                  CustomEditorsHelper.DrawDefaultInspector(serializedObject, "Settings.Physics.LayerMatrix", new string[] { }, true, ref foldout);
                }

                DrawLayerList(data, serializedObject);
              }
            }
          }
        }

        EditorGUILayout.Space();
        CustomEditorsHelper.DrawDefaultInspector(serializedObject, "Settings.Navigation", new string[] { });
      }
    }

    private void DrawLayerList(SimulationConfigAsset data, SerializedObject serializedObject) {
      var property = serializedObject.FindProperty("Settings.Physics.Layers");

      property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, "Layer List", true);
      if (property.isExpanded) {

        EditorGUILayout.HelpBox("This matrix configuration is saved on the Simulation Config, but the matrix currently only shows layers that exist as Unity layers.\nThat is why the layer list below is not editable.", MessageType.Info);

        bool oldEnabled = GUI.enabled;

        // LayerMatrixGUI won't work with custom names, changing the layer names must be done over Unity.
        GUI.enabled = false;
        for (int i = 0; i < data.Settings.Physics.Layers.Count(); i++) {
          bool isUserLayer = i >= 8;
          var label = isUserLayer ? " Builtin Layer " : " User Layer ";
          var newName = EditorGUILayout.TextField(label, data.Settings.Physics.Layers[i]);
          if (newName != data.Settings.Physics.Layers[i]) {
            data.Settings.Physics.Layers[i] = newName;
            EditorUtility.SetDirty(data);
          }
        }
        GUI.enabled = oldEnabled;
      }
    }

    private void LayerMatrixGUI_DoGUI(SerializedObject serializedObject) {
      var property = serializedObject.FindProperty("Settings.Physics.LayerMatrix");

      var type = TypeUtils.FindType("LayerMatrixGUI");
      MethodInfo method = type.GetMethod("DoGUI");

      var delegates = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
          .Where(t => t.BaseType == typeof(MulticastDelegate)).ToList();

      MethodInfo setValueHandler = typeof(SimulationConfigAssetEditor).GetMethod("SetValue", BindingFlags.NonPublic | BindingFlags.Instance);
      MethodInfo getValueHandler = typeof(SimulationConfigAssetEditor).GetMethod("GetValue", BindingFlags.NonPublic | BindingFlags.Instance);

      // Somewhere the resulting oder got switched
      var setValueDelegateType = delegates[0].FullName.EndsWith("SetValueFunc") ? delegates[0] : delegates[1];
      var getValueDelegateType = delegates[1].FullName.EndsWith("GetValueFunc") ? delegates[1] : delegates[0];

      Delegate setValueDelegate = Delegate.CreateDelegate(setValueDelegateType, this, setValueHandler);
      Delegate getValueDelegate = Delegate.CreateDelegate(getValueDelegateType, this, getValueHandler);

      object[] parametersArray = null;
      try {
        parametersArray = new object[] { "Layer Matrix", property.isExpanded, getValueDelegate, setValueDelegate };
        method.Invoke(this, parametersArray);
      }
      catch {
        // Scroll pos has been removed along the way.
        Vector2 scrollPos = Vector2.zero;
        parametersArray = new object[] { "Layer Matrix", property.isExpanded, scrollPos, getValueDelegate, setValueDelegate };
        method.Invoke(this, parametersArray);
      }
      property.isExpanded = (bool)parametersArray[1];
    }

    private bool GetValue(int layerA, int layerB) {
      var data = (SimulationConfigAsset)target;
      return (data.Settings.Physics.LayerMatrix[layerA] & (1 << layerB)) > 0;
    }

    private void SetValue(int layerA, int layerB, bool val) {
      var data = (SimulationConfigAsset)target;
      if (val) {
        data.Settings.Physics.LayerMatrix[layerA] |= (1 << layerB);
        data.Settings.Physics.LayerMatrix[layerB] |= (1 << layerA);
      }
      else {
        data.Settings.Physics.LayerMatrix[layerA] &= ~(1 << layerB);
        data.Settings.Physics.LayerMatrix[layerB] &= ~(1 << layerA);
      }
    }
  }
}