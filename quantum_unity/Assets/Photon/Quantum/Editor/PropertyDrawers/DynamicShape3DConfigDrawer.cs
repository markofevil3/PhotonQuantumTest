using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {

  [CustomPropertyDrawer(typeof(Shape3DConfig))]
  [CustomPropertyDrawer(typeof(Shape3DConfig.CompoundShapeData3D))]
  public class DynamicShape3DConfigDrawer : PropertyDrawer {

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      switch ((Quantum.Shape3DType)property.FindPropertyRelative("ShapeType").intValue) {
        case Shape3DType.Box: return CustomEditorsHelper.GetLinesHeight(6) + EditorGUIUtility.standardVerticalSpacing;
        case Shape3DType.Sphere: return CustomEditorsHelper.GetLinesHeight(5) + EditorGUIUtility.standardVerticalSpacing;
        case Shape3DType.Compound: return CustomEditorsHelper.GetLinesHeight(property.type == nameof(Shape3DConfig) ? 3 : 2);
      }
      return CustomEditorsHelper.GetLinesHeight(2);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      var p = position.SetLineHeight();

      using (new CustomEditorsHelper.PropertyScope(position, label, property)) {
        EditorGUI.LabelField(p, label);
        var wideMode = EditorGUIUtility.wideMode;

        try {
          EditorGUIUtility.wideMode = true;
          EditorGUI.indentLevel += 1;
          EditorGUI.PropertyField(p = p.AddLine(), property.FindPropertyRelative("ShapeType"), new GUIContent("Type"));

          switch ((Quantum.Shape3DType)property.FindPropertyRelative("ShapeType").intValue) {
            case Quantum.Shape3DType.None:
              EditorGUILayout.HelpBox("Please select a shape type.", MessageType.Warning);
              break;

            case Quantum.Shape3DType.Box:
              FPVector3PropertyDrawer.DrawCompact(p = p.AddLine(), property.FindPropertyRelative("BoxExtents"), new GUIContent("Extents"));
              FPVector3PropertyDrawer.DrawCompact(p = p.AddLine(), property.FindPropertyRelative("PositionOffset"), new GUIContent("Center"));
              FPVector3PropertyDrawer.DrawCompact(p = p.AddLine(), property.FindPropertyRelative("RotationOffset"), new GUIContent("Rotation"));
              EditorGUI.PropertyField(p = p.AddLine(), property.FindPropertyRelative("UserTag"), new GUIContent("UserTag"));
              break;

            case Quantum.Shape3DType.Sphere:
              EditorGUI.PropertyField(p =  p.AddLine(), property.FindPropertyRelative("SphereRadius"), new GUIContent("Radius"));
              FPVector3PropertyDrawer.DrawCompact(p = p.AddLine(), property.FindPropertyRelative("PositionOffset"), new GUIContent("Center"));
              EditorGUI.PropertyField(p = p.AddLine(), property.FindPropertyRelative("UserTag"), new GUIContent("UserTag"));
              break;
            
            case Shape3DType.Compound:
              if (property.type == nameof(Shape3DConfig)) {
                // as for now, only persistent compound shapes are supported on the editor
                GUI.enabled = false;
                var isPersistentProperty = property.FindPropertyRelative("IsPersistent");
                isPersistentProperty.boolValue = true;
                EditorGUI.PropertyField(p = p.AddLine(), isPersistentProperty, new GUIContent("Is Persistent"));
                GUI.enabled = true;
                
                CustomEditorsHelper.DrawDefaultInspector(property.FindPropertyRelative("CompoundShapes"), skipRoot: false, label: new GUIContent("Shapes"));
              }
              else {
                // else: property is CompoundShapeData, but nested compound shapes are not supported on the editor yet
                Debug.Assert(property.type == nameof(Shape3DConfig.CompoundShapeData3D));
                EditorGUILayout.HelpBox("Nested compound shapes are not supported on the editor.", MessageType.Warning);
              }
              break;

            default:
              EditorGUILayout.HelpBox("Shape type not supported for dynamic bodies.", MessageType.Warning);
              break;
          }
        } finally {
          EditorGUI.indentLevel -= 1;
          EditorGUIUtility.wideMode = wideMode;
        }
      }
    }
  }
}