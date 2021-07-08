using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(Quantum.LayerMask))]
  public class LayerMaskDrawer : PropertyDrawer {
    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      // go into child property (raw)
      prop.Next(true);

      // draw field
      Draw(p, prop, label);
    }

    public static void Draw(Rect p, SerializedProperty prop, GUIContent label) {
      prop.intValue = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(EditorGUI.MaskField(p, label, InternalEditorUtility.LayerMaskToConcatenatedLayersMask(prop.intValue), InternalEditorUtility.layers));
    }
  }
}
