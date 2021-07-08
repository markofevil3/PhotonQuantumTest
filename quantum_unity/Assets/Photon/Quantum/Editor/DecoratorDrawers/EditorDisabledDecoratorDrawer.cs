using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EditorDisabledAttribute))]
public class EditorDisabledDecoratorDrawer : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    try {
      GUI.enabled = false;
      EditorGUI.PropertyField(position, property, label, true);
    }
    finally {
      GUI.enabled = true;
    }
  }

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
    return EditorGUI.GetPropertyHeight(property);
  }
}
