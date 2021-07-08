using System.Collections;
using System.Collections.Generic;
using Photon.Deterministic;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {

  [CustomPropertyDrawer(typeof(Shape2DConfig))]
  [CustomPropertyDrawer(typeof(Shape2DConfig.CompoundShapeData2D))]
  public class DynamicShapeConfigDrawer : PropertyDrawer {

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      switch ((Quantum.Shape2DType)property.FindPropertyRelative("ShapeType").intValue) {
        case Shape2DType.None: return CustomEditorsHelper.GetLinesHeight(2);
        case Shape2DType.Compound: return CustomEditorsHelper.GetLinesHeight(property.type == nameof(Shape2DConfig) ? 3 : 2);
        case Shape2DType.Circle: return CustomEditorsHelper.GetLinesHeight(5);
        case Shape2DType.Box:
        case Shape2DType.Polygon:
        case Shape2DType.Edge:
          return CustomEditorsHelper.GetLinesHeight(6);
      }

      return CustomEditorsHelper.GetLinesHeight(4);
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

          switch ((Quantum.Shape2DType)property.FindPropertyRelative("ShapeType").intValue) {
            case Quantum.Shape2DType.None:
              EditorGUILayout.HelpBox("Select a shape type.", MessageType.Warning);
              break;
            
            case Quantum.Shape2DType.Box:
              FPVector2PropertyDrawer.DrawCompact(p = p.AddLine(), property.FindPropertyRelative("BoxExtents"), new GUIContent("Extents"));
              FPVector2PropertyDrawer.DrawCompact(p = p.AddLine(), property.FindPropertyRelative("PositionOffset"), new GUIContent("Center"));
              FPPropertyDrawer.DrawRawAs2DRotation(p = p.AddLine(), property.FindPropertyRelative("RotationOffset.RawValue"), new GUIContent("Rotation"));
              EditorGUI.PropertyField(p = p.AddLine(), property.FindPropertyRelative("UserTag"), new GUIContent("UserTag"));
              break;

            case Quantum.Shape2DType.Circle:
              EditorGUI.PropertyField(p = p.AddLine(), property.FindPropertyRelative("CircleRadius"), new GUIContent("Radius"));
              FPVector2PropertyDrawer.DrawCompact(p = p.AddLine(), property.FindPropertyRelative("PositionOffset"), new GUIContent("Center"));
              EditorGUI.PropertyField(p = p.AddLine(), property.FindPropertyRelative("UserTag"), new GUIContent("UserTag"));
              break;

            case Quantum.Shape2DType.Polygon:
              EditorGUI.PropertyField(p = p.AddLine(), property.FindPropertyRelative("PolygonCollider"), new GUIContent("Asset"));
              FPVector2PropertyDrawer.DrawCompact(p = p.AddLine(), property.FindPropertyRelative("PositionOffset"), new GUIContent("Center"));
              FPPropertyDrawer.DrawRawAs2DRotation(p = p.AddLine(), property.FindPropertyRelative("RotationOffset.RawValue"), new GUIContent("Rotation"));
              EditorGUI.PropertyField(p = p.AddLine(), property.FindPropertyRelative("UserTag"), new GUIContent("UserTag"));
              break;

            case Quantum.Shape2DType.Edge:
              EditorGUI.PropertyField(p = p.AddLine(), property.FindPropertyRelative("EdgeExtent"), new GUIContent("Extent"));
              FPVector2PropertyDrawer.DrawCompact(p = p.AddLine(), property.FindPropertyRelative("PositionOffset"), new GUIContent("Center"));
              FPPropertyDrawer.DrawRawAs2DRotation(p = p.AddLine(), property.FindPropertyRelative("RotationOffset.RawValue"), new GUIContent("Rotation"));
              EditorGUI.PropertyField(p = p.AddLine(), property.FindPropertyRelative("UserTag"), new GUIContent("UserTag"));
              break;

            case Quantum.Shape2DType.Compound:
              if (property.type == nameof(Shape2DConfig)) {
                GUI.enabled = false;
                var isPersistentProperty = property.FindPropertyRelative("IsPersistent");
                isPersistentProperty.boolValue = true;
                EditorGUI.PropertyField(p = p.AddLine(), isPersistentProperty, new GUIContent("Is Persistent"));
                GUI.enabled = true;
                
                CustomEditorsHelper.DrawDefaultInspector(property.FindPropertyRelative("CompoundShapes"), skipRoot: false, label: new GUIContent("Shapes"));
              }
              else {
                // else: property is CompoundShapeData, but nested compound shapes are not supported on the editor yet
                Debug.Assert(property.type == nameof(Shape2DConfig.CompoundShapeData2D));
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