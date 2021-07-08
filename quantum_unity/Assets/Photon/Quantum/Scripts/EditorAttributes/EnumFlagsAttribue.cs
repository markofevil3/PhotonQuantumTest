using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace Quantum {
  [AttributeUsage(AttributeTargets.Field)]
  public class EnumFlagsAttribute : PropertyAttribute {
    public string tooltip { get; set; } = "";
  }
}

#if UNITY_EDITOR

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
  public class EnumFlagsDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

      var attribute = (EnumFlagsAttribute)this.attribute;
      var enumType = fieldInfo.FieldType.HasElementType ? fieldInfo.FieldType.GetElementType() : fieldInfo.FieldType;

      Debug.Assert(enumType.IsEnum);
      var oldValue = (Enum)Enum.ToObject(enumType, property.intValue);

      EditorGUI.BeginProperty(position, label, property);
      EditorGUI.BeginChangeCheck();

      if ( string.IsNullOrEmpty(label.tooltip) ) {
        label.tooltip = attribute.tooltip;
      }
      
      var newValue = EditorGUI.EnumFlagsField(position, label, oldValue);
      if (EditorGUI.EndChangeCheck()) {
        property.intValue = Convert.ToInt32(Convert.ChangeType(newValue, enumType));
      }

      EditorGUI.EndProperty();
    }
  }
}

#endif