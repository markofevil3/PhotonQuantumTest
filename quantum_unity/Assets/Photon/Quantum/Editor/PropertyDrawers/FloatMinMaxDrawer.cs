using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(Quantum.MinMaxSliderAttribute))]
class MinMaxSliderDrawer : PropertyDrawer {
  const Single MIN_MAX_WIDTH = 50f;
  const Single SPACING = 1f;

  static GUIStyle _overlay;

  static public GUIStyle OverlayStyle {
    get {
      if (_overlay == null) {
        _overlay = new GUIStyle(EditorStyles.miniLabel);
        _overlay.alignment = TextAnchor.MiddleRight;
        _overlay.contentOffset = new Vector2(-2, 0);

        Color c;
        c = EditorGUIUtility.isProSkin ? Color.yellow : Color.blue;
        c.a = 0.75f;

        _overlay.normal.textColor = c;
      }

      return _overlay;
    }
  }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    var spacing = SPACING * EditorGUIUtility.pixelsPerPoint;
    var min = property.FindPropertyRelative("Min");
    var minValue = min.floatValue;

    var max = property.FindPropertyRelative("Max");
    var maxValue = max.floatValue;

    var attr = (Quantum.MinMaxSliderAttribute)attribute;

    EditorGUI.PrefixLabel(position, label);

    //var p = position;
    //p.x += EditorGUIUtility.labelWidth + MIN_MAX_WIDTH + spacing;
    //p.width -= EditorGUIUtility.labelWidth + (MIN_MAX_WIDTH + spacing) * 2;

    //EditorGUI.BeginChangeCheck();
    //EditorGUI.MinMaxSlider(p, ref minValue, ref maxValue, attr.Min, attr.Max);
    //if (EditorGUI.EndChangeCheck()) {
    //  min.floatValue = minValue;
    //  max.floatValue = maxValue;
    //}

    var w = ((position.width - EditorGUIUtility.labelWidth) * 0.5f) - spacing;

    var p = position;
    p.x += EditorGUIUtility.labelWidth;
    p.width = w;
    min.floatValue = EditorGUI.FloatField(p, min.floatValue);

    GUI.Label(p, "(Start)", OverlayStyle);

    p = position;
    p.x += p.width - w;
    p.width = w;
    max.floatValue = EditorGUI.FloatField(p, max.floatValue);

    GUI.Label(p, "(End)", OverlayStyle);
  }
}