using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(ComponentTypeSelector))]
  public unsafe class ComponentTypeSelectorDrawer : PropertyDrawer {
    private string[]     _qualifiedNames;
    private GUIContent[] _prettyNames;

    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      if (_qualifiedNames == null) {
        Initialize(ref _qualifiedNames, ref _prettyNames);
      }

      var qualifiedName = prop.FindPropertyRelative("QualifiedName");

      var index = Array.FindIndex(_qualifiedNames, t => t == qualifiedName.stringValue);
      index = Mathf.Max(EditorGUI.Popup(p, label, index, _prettyNames), 0);
      if (index >= 0) {
        qualifiedName.stringValue = _qualifiedNames[index];
      }
    }

    public static void Initialize(ref string[] qualifiedNames, ref GUIContent[] prettyNames) {
      var types = typeof(IComponent).GetSubClasses(typeof(IComponent).Assembly, typeof(Frame).Assembly).Where(t => t != typeof(IComponent) && !t.IsInterface).ToArray();
      Array.Sort(types, (a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
      qualifiedNames = types.Select(t => t.AssemblyQualifiedName).ToArray();
      prettyNames    = types.Select(t => new GUIContent(t.Name)).ToArray();
    }
  }
}
