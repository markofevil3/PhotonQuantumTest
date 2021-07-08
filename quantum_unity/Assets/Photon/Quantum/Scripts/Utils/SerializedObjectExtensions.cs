using System;

#if UNITY_EDITOR

using UnityEditor;

namespace Quantum {
  public static class SerializedObjectExtensions {
    public static SerializedProperty FindPropertyOrThrow(this SerializedObject so, string propertyPath) {
      var result = so.FindProperty(propertyPath);
      if (result == null)
        throw new ArgumentOutOfRangeException($"Property not found: {propertyPath}");
      return result;
    }

    public static SerializedProperty FindPropertyRelativeOrThrow(this SerializedProperty sp, string relativePropertyPath) {
      var result = sp.FindPropertyRelative(relativePropertyPath);
      if (result == null)
        throw new ArgumentOutOfRangeException($"Property not found: {relativePropertyPath}");
      return result;
    }

    public static Int64 GetIntegerValue(this SerializedProperty sp) {
      switch (sp.type) {
        case "int":
        case "bool": return sp.intValue;
        case "long": return sp.longValue;
        case "FP": return sp.FindPropertyRelative("RawValue").longValue;
        case "Enum": return sp.enumValueIndex;
        default:
          switch (sp.propertyType) {
            case SerializedPropertyType.ObjectReference:
              return sp.objectReferenceInstanceIDValue;
          }
          return 0;

      }
    }

    public static void SetIntegerValue(this SerializedProperty sp, long value) {
      switch (sp.type) {
        case "int":
          sp.intValue = (int)value;
          break;
        case "bool":
          sp.boolValue = value != 0;
          break;
        case "long":
          sp.longValue = value;
          break;
        case "FP":
          sp.FindPropertyRelative("RawValue").longValue = value;
          break;
        case "Enum":
          sp.enumValueIndex = (int)value;
          break;
        default:
          throw new NotSupportedException($"Type {sp.type} is not supported");
      }
    }
  }
}

#endif