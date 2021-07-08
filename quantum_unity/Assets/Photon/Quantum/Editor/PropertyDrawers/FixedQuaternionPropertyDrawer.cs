using System;
using UnityEditor;
using UnityEngine;
using Photon.Deterministic;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(FPQuaternion))]
  public class FPQuaternionPropertyDrawer : PropertyDrawer {

    private static GUIContent[] _labels = new[] {
      new GUIContent("X"),
      new GUIContent("Y"),
      new GUIContent("Z"),
      new GUIContent("W"),
    };

    private static string[] _paths = new[] {
      "X.RawValue",
      "Y.RawValue",
      "Z.RawValue",
      "W.RawValue",
    };

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return CustomEditorsHelper.GetLinesHeightWithNarrowModeSupport(1);
    }

    public override void OnGUI(Rect p, SerializedProperty prop, GUIContent label) {
      DrawCompact(p, prop, label);
    }

    public static void DrawCompact(Rect p, SerializedProperty prop, GUIContent label) {

      EditorGUI.BeginChangeCheck();
      FPPropertyDrawer.DoMultiFPProperty(p, prop, label, _labels, _paths);
      if ( EditorGUI.EndChangeCheck()) {
        var rawX = prop.FindPropertyRelativeOrThrow("X.RawValue");
        var rawY = prop.FindPropertyRelativeOrThrow("Y.RawValue");
        var rawZ = prop.FindPropertyRelativeOrThrow("Z.RawValue");
        var rawW = prop.FindPropertyRelativeOrThrow("W.RawValue");
        Normalize(rawX, rawY, rawZ, rawW);
      }
    }

    private static void Normalize(SerializedProperty rawX, SerializedProperty rawY, SerializedProperty rawZ, SerializedProperty rawW) {
      var x = FP.FromRaw(rawX.longValue).AsDouble;
      var y = FP.FromRaw(rawY.longValue).AsDouble;
      var z = FP.FromRaw(rawZ.longValue).AsDouble;
      var w = FP.FromRaw(rawW.longValue).AsDouble;

      var magnitueSqr = x * x + y * y + z * z + w * w;
      if (magnitueSqr < 0.00001) {
        x = y = z = 0;
        w = 1;
      } else {
        var m = System.Math.Sqrt(magnitueSqr);
        x /= m;
        y /= m;
        z /= m;
        w /= m;
      }

      rawX.longValue = FP.FromFloat_UNSAFE((float)x).RawValue;
      rawY.longValue = FP.FromFloat_UNSAFE((float)y).RawValue;
      rawZ.longValue = FP.FromFloat_UNSAFE((float)z).RawValue;
      rawW.longValue = FP.FromFloat_UNSAFE((float)w).RawValue;
    }
  }
}