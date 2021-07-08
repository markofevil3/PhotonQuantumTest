using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(AssetObjectIdentifier))]
  public class AssetObjectIdentifierDrawer : PropertyDrawer {
    private bool _editPath;
    private bool _editGuid;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return CustomEditorsHelper.GetLinesHeight(2);
    }

    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      const float ButtonSize = 40;
      p = p.SetLineHeight();

      using (new EditorGUI.DisabledScope(!_editPath)) {
        var pathRect = p;
        pathRect.xMax -= ButtonSize + 5;
        var guid = prop.FindPropertyRelative("Path");
        EditorGUI.PropertyField(pathRect, guid, false);
      }

      var buttonRect   = p;
      buttonRect.xMin  = p.xMax - ButtonSize;
      buttonRect.width = ButtonSize;
      if (GUI.Button(buttonRect, new GUIContent("Edit", "Set the field edit-able to copy the path for example."))) {
        _editPath = !_editPath;
      }

      using (new EditorGUI.DisabledScope(!_editGuid)) {
        p = p.AddLine();
        var guidRect = p;
        guidRect.xMax -= ButtonSize + 5;
        var guid64 = prop.FindPropertyRelative("Guid");
        EditorGUI.PropertyField(guidRect, guid64, false);
      }

      buttonRect       = p;
      buttonRect.xMin  = p.xMax - ButtonSize;
      buttonRect.width = ButtonSize;
      if (GUI.Button(buttonRect, new GUIContent("Edit", "Set the field edit-able set a certain guid (not recommended)."))) {
        _editGuid = !_editGuid;
      }
    }
  }
}