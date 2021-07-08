using System;
using UnityEditor;
using UnityEngine;
using Photon.Deterministic;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(QBoolean))]
  public class QBooleanDrawer : PropertyDrawer {
    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      prop.Next(true);
      Debug.Assert(prop.name == "Value");

      EditorGUI.BeginChangeCheck();
      bool value = EditorGUI.Toggle(p, label, prop.GetIntegerValue() != 0);
      if (EditorGUI.EndChangeCheck()) {
        prop.SetIntegerValue(value ? 1 : 0);
      }
    }
  }
}