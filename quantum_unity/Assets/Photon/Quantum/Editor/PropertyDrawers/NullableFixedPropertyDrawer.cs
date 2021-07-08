using System;
using UnityEditor;
using UnityEngine;
using Photon.Deterministic;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(NullableFP))]
  [CustomPropertyDrawer(typeof(NullableFPVector2))]
  [CustomPropertyDrawer(typeof(NullableFPVector3))]
  public class NullableFPPropertyDrawer : PropertyDrawer {

    private const string HasValueName = nameof(NullableFP._hasValue);
    private const string ValueName = nameof(NullableFP._value);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

      EditorGUI.BeginProperty(position, label, property);

      var hasValueProperty = property.FindPropertyRelativeOrThrow(HasValueName);
      var valueProperty = property.FindPropertyRelativeOrThrow(ValueName);

      var hasValue = hasValueProperty.intValue != 0;

      var toggleRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
      toggleRect.width = Mathf.Min(toggleRect.width, CustomEditorsHelper.CheckboxWidth);
      toggleRect.height = EditorGUIUtility.singleLineHeight;

      using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
        if (EditorGUI.Toggle(toggleRect, GUIContent.none, hasValue) != hasValue) {
          hasValueProperty.intValue = hasValue ? 0 : 1;
          hasValueProperty.serializedObject.ApplyModifiedProperties();
        }
      }

      if (hasValue) {
        EditorGUIUtility.labelWidth += CustomEditorsHelper.CheckboxWidth;
        EditorGUI.PropertyField(position, valueProperty, CustomEditorsHelper.WhitespaceContent);
        EditorGUIUtility.labelWidth -= CustomEditorsHelper.CheckboxWidth;
      }

      EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

      var hasValueProperty = property.FindPropertyRelativeOrThrow(HasValueName);
      var valueProperty = property.FindPropertyRelativeOrThrow(ValueName);

      if (hasValueProperty.intValue != 0) {
        return EditorGUI.GetPropertyHeight(valueProperty);
      } else {
        return EditorGUI.GetPropertyHeight(hasValueProperty);
      }

    }
  }
}