using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.EditorUtility;

namespace Quantum.Editor {

  public abstract class EnumWrapperPropertyDrawerBase<EnumType> : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      DoEnumGUI(position, property.FindPropertyRelativeOrThrow("Value"), label);
    }

    private delegate void DisplayCustomMenuDelegate(Rect position, string[] options, int[] selected, SelectMenuItemFunction callback, object userData);
    private static DisplayCustomMenuDelegate DisplayCustomMenu =
      ReflectionUtils.CreateMethodDelegate<DisplayCustomMenuDelegate>(typeof(EditorUtility), nameof(DisplayCustomMenu), BindingFlags.NonPublic | BindingFlags.Static);

    private static void DoEnumGUI(Rect position, SerializedProperty property, GUIContent label) {

      try {
        EditorGUI.BeginProperty(position, label, property);

        long[] values;
        {
          var rawValues = Enum.GetValues(typeof(EnumType));
          values = new long[rawValues.Length];
          for (int i = 0; i < rawValues.Length; ++i)
            values[i] = Convert.ToInt64(rawValues.GetValue(i));
        }

        List<int> selectedIndices = new List<int>();

        var names = Enum.GetNames(typeof(EnumType));
        var underlyingType = Enum.GetUnderlyingType(typeof(EnumType));
        var currentValue = property.longValue;
        var isFlags = typeof(EnumType).GetCustomAttribute<FlagsAttribute>() != null;

        // find out what to show

        for (int i = 0; i < values.Length; ++i) {
          if (!isFlags) {
            if (currentValue == values[i]) {
              selectedIndices.Add(i);
              break;
            }
          } else if ((currentValue & values[i]) == values[i]) {
            selectedIndices.Add(i);
          }
        }

        string labelValue;
        if (selectedIndices.Count == 0) {
          if (isFlags && currentValue == 0) {
            labelValue = "Nothing";
          } else {
            labelValue = "";
          }
        } else if (selectedIndices.Count == 1) {
          labelValue = names[selectedIndices[0]];
        } else {
          Debug.Assert(isFlags);
          if (selectedIndices.Count == values.Length) {
            labelValue = "Everything";
          } else {
            labelValue = string.Join("|", selectedIndices.Select(x => names[x]));
          }
        }

        if (DisplayCustomMenu != null) {
          var r = EditorGUI.PrefixLabel(position, label);
          if (EditorGUI.DropdownButton(r, new GUIContent(labelValue), FocusType.Keyboard)) {
            if (isFlags) {
              var allOptions = new[] { "Nothing", "Everything" }.Concat(names).ToArray();
              List<int> allIndices = new List<int>();
              if (selectedIndices.Count == 0)
                allIndices.Add(0); // nothing
              else if (selectedIndices.Count == values.Length)
                allIndices.Add(1); // everything
              allIndices.AddRange(selectedIndices.Select(x => x + 2));

              DisplayCustomMenu(r, allOptions, allIndices.ToArray(), (userData, options, selected) => {
                if (selected == 0) {
                  property.longValue = 0;
                } else if (selected == 1) {
                  property.longValue = 0;
                  foreach (var value in values) {
                    property.longValue |= value;
                  }
                } else {
                  selected -= 2;
                  if (selectedIndices.Contains(selected)) {
                    property.longValue &= (~values[selected]);
                  } else {
                    property.longValue |= values[selected];
                  }
                }
                property.serializedObject.ApplyModifiedProperties();
              }, null);
            } else {
              DisplayCustomMenu(r, names, selectedIndices.ToArray(), (userData, options, selected) => {
                if (!selectedIndices.Contains(selected)) {
                  property.longValue = values[selected];
                  property.serializedObject.ApplyModifiedProperties();
                }
              }, null);
            }

          }
        } else {
          // fallback
          
        }
      } finally {
        EditorGUI.EndProperty();
      }

    }
  }
}
