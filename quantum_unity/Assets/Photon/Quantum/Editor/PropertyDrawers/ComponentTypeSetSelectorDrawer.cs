using System;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(ComponentTypeSetSelector))]
  public unsafe class ComponentTypeSetSelectorDrawer : PropertyDrawer {
    private string[]     _qualifiedNames;
    private GUIContent[] _prettyNames;

    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      if (_qualifiedNames == null) {
        ComponentTypeSelectorDrawer.Initialize(ref _qualifiedNames, ref _prettyNames);
      }

      p = p.SetLineHeight();

      var qualifiedNames = prop.FindPropertyRelative("QualifiedNames");
      prop.isExpanded = EditorGUI.Foldout(p, prop.isExpanded, label);

      using (new EditorGUI.IndentLevelScope(1)) {
        if (prop.isExpanded) {
          qualifiedNames.arraySize = EditorGUI.IntField(p = p.AddLine(), "Size", qualifiedNames.arraySize);

          for (int i = 0; i < qualifiedNames.arraySize; i++) {
            var qualifiedName = qualifiedNames.GetArrayElementAtIndex(i);
            var index         = Array.FindIndex(_qualifiedNames, t => t == qualifiedName.stringValue);
            index = Mathf.Max(EditorGUI.Popup(p = p.AddLine(), new GUIContent($"Element {i}"), index, _prettyNames), 0);
            if (index >= 0) {
              qualifiedName.stringValue = _qualifiedNames[index];
            }
          }
        }
      }
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
      if ( !prop.isExpanded ) {
        return base.GetPropertyHeight(prop, label);
      }

      return CustomEditorsHelper.GetLinesHeight(2 + prop.FindPropertyRelative("QualifiedNames").arraySize);
    }
  }
}
