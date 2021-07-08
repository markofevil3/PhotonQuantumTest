using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(AssetGuid))]
  public unsafe class AssetGuidDrawer : PropertyDrawer {
    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      var value = prop.FindPropertyRelative("Value");
      EditorGUI.PropertyField(p, value, new GUIContent("Guid"));
    }
  }
}
