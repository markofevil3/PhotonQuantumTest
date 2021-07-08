using System;
using UnityEditor;
using UnityEngine;
using Photon.Deterministic;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(FP))]
  public class FPPropertyDrawer : PropertyDrawer {

    public const int DefaultPrecision = 5;

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

    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      // go into child property (raw)
      prop.Next(true);
      Debug.Assert(prop.name == nameof(FP.RawValue));

      // draw field
      DrawRaw(p, prop, label);
    }

    public static float GetRawAsFloat(SerializedProperty prop) {
      return GetRawAsFloat(prop.longValue);
    }

    public static float GetRawAsFloat(long rawValue) {
      var f = FP.FromRaw(rawValue);
      var precision = QuantumEditorSettings.InstanceFailSilently?.FPDisplayPrecision ?? DefaultPrecision;
      var v = (Single)Math.Round(f.AsFloat, precision);
      return v;
    }


    public static void DrawRaw(Rect p, SerializedProperty prop, GUIContent label, bool opposite = false) {
      using (new CustomEditorsHelper.PropertyScope(p, label, prop)) {
        if (DrawRawValueAsFloat(p, prop.longValue, label, out var raw)) {
          prop.longValue = raw;
        }
      }
    }

    public static void DrawAs2DRotation(Rect p, SerializedProperty prop, GUIContent label) {
      // go into child property (raw)
      prop = prop.Copy();
      prop.Next(true);
      Debug.Assert(prop.name == nameof(FP.RawValue));
      DrawRawAs2DRotation(p, prop, label);
    }

    public static void DrawRawAs2DRotation(Rect p, SerializedProperty prop, GUIContent label) {
      using (new CustomEditorsHelper.PropertyScope(p, label, prop)) {
        long initialValue = prop.longValue;
#if !QUANTUM_XY
        initialValue = -initialValue;
#endif
        if (DrawRawValueAsFloat(p, initialValue, label, out var raw)) {
#if !QUANTUM_XY
          prop.longValue = -raw;
#else
          prop.longValue = raw;
#endif
        }
      }
    }

    private static bool DrawRawValueAsFloat(Rect p, long rawValue, GUIContent label, out long rawResult) {
      // grab value
      var v = GetRawAsFloat(rawValue);

      // edit value
      try {
        var n = label == null ? EditorGUI.FloatField(p, v) : EditorGUI.FloatField(p, label, v);
        if (n != v) {
          rawResult = FP.FromFloat_UNSAFE(n).RawValue;
          return true;
        }

        GUI.Label(p, "(FP)", OverlayStyle);

      } catch (FormatException exn) {
        if (exn.Message != ".") {
          Debug.LogException(exn);
        }
      }

      rawResult = default;
      return false;
    }

    internal static Rect DoMultiFPProperty(Rect p, SerializedProperty prop, GUIContent label, GUIContent[] labels, string[] paths) {
      EditorGUI.BeginProperty(p, label, prop);
      try {
        int id = GUIUtility.GetControlID(_multiFieldPrefixId, FocusType.Keyboard, p);
        p = MultiFieldPrefixLabel(p, id, label, labels.Length);
        if (p.width > 1) {
          using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
            float w = (p.width - (labels.Length - 1) * SpacingSubLabel) / labels.Length;
            var ph = new Rect(p) { width = w };

            for (int i = 0; i < labels.Length; ++i) {
              using (new CustomEditorsHelper.LabelWidthScope(CalcPrefixLabelWidth(labels[i]))) {
                FPPropertyDrawer.DrawRaw(ph, prop.FindPropertyRelativeOrThrow(paths[i]), labels[i]);
              }
              ph.x += w + SpacingSubLabel;
            }
          }
        }
      } finally {
        EditorGUI.EndProperty();
      }

      return p;
    }

    private static float CalcPrefixLabelWidth(GUIContent label) {
#if UNITY_2019_3_OR_NEWER
      return EditorStyles.label.CalcSize(label).x;
#else
      return 13.0f;
#endif
    }

    private delegate Rect MultiFieldPrefixLabelDelegate(Rect totalPosition, int id, GUIContent label, int columns);
    private static readonly MultiFieldPrefixLabelDelegate MultiFieldPrefixLabel = ReflectionUtils.CreateEditorMethodDelegate<MultiFieldPrefixLabelDelegate>("UnityEditor.EditorGUI", "MultiFieldPrefixLabel");

    private const float SpacingSubLabel = 2;
    private static readonly int _multiFieldPrefixId = "MultiFieldPrefixId".GetHashCode();
  }

  [CustomPropertyDrawer(typeof(FPVector2))]
  public class FPVector2PropertyDrawer : PropertyDrawer {

    private static GUIContent[] _labels = new[] {
      new GUIContent("X"),
      new GUIContent("Y"),
    };

    private static string[] _paths = new[] {
      "X.RawValue",
      "Y.RawValue",
    };

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return CustomEditorsHelper.GetLinesHeightWithNarrowModeSupport(1);
    }

    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      DrawCompact(p, prop, label);
    }

    public static void DrawCompact(Rect p, SerializedProperty prop, GUIContent label) {
      FPPropertyDrawer.DoMultiFPProperty(p, prop, label, _labels, _paths);
    }
  }

  [CustomPropertyDrawer(typeof(FPVector3))]
  public class FPVector3PropertyDrawer : PropertyDrawer {

    private static GUIContent[] _labels = new[] {
      new GUIContent("X"),
      new GUIContent("Y"),
      new GUIContent("Z")
    };

    private static string[] _paths = new[] {
      "X.RawValue",
      "Y.RawValue",
      "Z.RawValue"
    };

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return CustomEditorsHelper.GetLinesHeightWithNarrowModeSupport(1);
    }

    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      DrawCompact(p, prop, label);
    }

    public static void DrawCompact(Rect p, SerializedProperty prop, GUIContent label) {
      FPPropertyDrawer.DoMultiFPProperty(p, prop, label, _labels, _paths);
    }
  }
}