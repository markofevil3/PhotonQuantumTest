using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(Quantum.PlayerRef))]
  public class PlayerRefDrawer : PropertyDrawer {

    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      EditorGUI.BeginProperty(p, label, prop);
      EditorGUI.BeginChangeCheck();

      var valueProperty = prop.FindPropertyRelativeOrThrow(nameof(PlayerRef._index));
      int value = valueProperty.intValue;

      var toggleRect = EditorGUI.PrefixLabel(p, GUIUtility.GetControlID(FocusType.Passive), label);
      toggleRect.width = Mathf.Min(toggleRect.width, CustomEditorsHelper.CheckboxWidth);

      var hasValue = value > 0;

      using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
        if (EditorGUI.Toggle(toggleRect, GUIContent.none, hasValue) != hasValue) {
          value = hasValue ? 0 : 1;
        }
      }

      if (hasValue) {
        EditorGUIUtility.labelWidth += CustomEditorsHelper.CheckboxWidth;
        value = EditorGUI.IntSlider(p, CustomEditorsHelper.WhitespaceContent, value, 1, Quantum.Input.MAX_COUNT);
        EditorGUIUtility.labelWidth -= CustomEditorsHelper.CheckboxWidth;
      }

      if (EditorGUI.EndChangeCheck()) {
        valueProperty.intValue = value;
      }
      EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return CustomEditorsHelper.GetLinesHeight(1);
    }
  }
}
