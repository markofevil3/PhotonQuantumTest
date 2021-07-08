using Photon.Deterministic;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {

  [CustomPropertyDrawer(typeof(FPAnimationCurve))]
  public class FixedCurveDrawer : PropertyDrawer {

    private Dictionary<string, AnimationCurve> _animationCurveCache = new Dictionary<string, AnimationCurve>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return CustomEditorsHelper.GetLinesHeight(3);
    }

    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label) {

      // Get properties accessors.
      var resolutionProperty = prop.FindPropertyRelative("Resolution");
      var samplesProperty = prop.FindPropertyRelative("Samples");
      var startTimeProperty = GetPropertyNext(prop, "StartTime");
      var endTimeProperty = GetPropertyNext(prop, "EndTime");
      var preWrapModeProperty = prop.FindPropertyRelative("PreWrapMode");
      var postWrapModeProperty = prop.FindPropertyRelative("PostWrapMode");
      var preWrapModeOriginalProperty = prop.FindPropertyRelative("OriginalPreWrapMode");
      var postWrapModeOriginalProperty = prop.FindPropertyRelative("OriginalPostWrapMode");
      var keysProperty = prop.FindPropertyRelative("Keys");

      // Default values (because we changed FPAnimationCurve to be a struct)
      if (resolutionProperty.intValue <= 1) {
        resolutionProperty.intValue = 32;
        startTimeProperty.longValue = 0;
        endTimeProperty.longValue = FP.RAW_ONE;
      }

      AnimationCurve animationCurve;

      var propertyKey = prop.propertyPath + "_" + prop.serializedObject.GetHashCode();
      if (!_animationCurveCache.TryGetValue(propertyKey, out animationCurve)) {
        // Load the Quantum data into a Unity animation curve.
        animationCurve = new AnimationCurve();
        _animationCurveCache[propertyKey] = animationCurve;
        animationCurve.preWrapMode = (WrapMode)preWrapModeOriginalProperty.intValue;
        animationCurve.postWrapMode = (WrapMode)postWrapModeOriginalProperty.intValue;
        for (int i = 0; i < keysProperty.arraySize; i++) {
          var keyProperty = keysProperty.GetArrayElementAtIndex(i);
          var key = new Keyframe();
          key.time = FP.FromRaw(GetPropertyNext(keyProperty, "Time").longValue).AsFloat;
          key.value = FP.FromRaw(GetPropertyNext(keyProperty, "Value").longValue).AsFloat;
          key.inTangent = FP.FromRaw(GetPropertyNext(keyProperty, "InTangent").longValue).AsFloat;
          key.outTangent= FP.FromRaw(GetPropertyNext(keyProperty, "OutTangent").longValue).AsFloat;

          animationCurve.AddKey(key);

          var leftTangentMode = (AnimationUtility.TangentMode)keyProperty.FindPropertyRelative("TangentModeLeft").intValue;
          var rightTangentMode = (AnimationUtility.TangentMode)keyProperty.FindPropertyRelative("TangentModeRight").intValue;

          // Since 2018.1 key.TangentMode is depricated. AnimationUtility was already working on 2017, so just do the conversion here. 
          var depricatedTangentMode = keyProperty.FindPropertyRelative("TangentMode").intValue;
          if (depricatedTangentMode > 0) { 
            leftTangentMode = ConvertTangetMode(depricatedTangentMode, true);
            rightTangentMode = ConvertTangetMode(depricatedTangentMode, false);
#pragma warning disable 0618
            keyProperty.FindPropertyRelative("TangentMode").intValue = key.tangentMode;
#pragma warning restore 0618
            Debug.LogFormat("FPAnimationCurve: Converted tangent for key {0} from depricated={1} to left={2}, right={3}", i, depricatedTangentMode, leftTangentMode, rightTangentMode);
          }
            
          AnimationUtility.SetKeyLeftTangentMode(animationCurve, animationCurve.length - 1, leftTangentMode);
          AnimationUtility.SetKeyRightTangentMode(animationCurve, animationCurve.length - 1, rightTangentMode);
        }
      }

      EditorGUI.BeginChangeCheck();

      var p = position.SetLineHeight();

      EditorGUI.LabelField(p, prop.name);
      p = p.AddLine();

      EditorGUI.indentLevel++;

      resolutionProperty.intValue = EditorGUI.IntField(p, "Resolution", resolutionProperty.intValue);
      resolutionProperty.intValue = Mathf.Clamp(resolutionProperty.intValue, 2, 1024);

      p = p.AddLine();
      animationCurve = EditorGUI.CurveField(p, "Samples", animationCurve);
      _animationCurveCache[propertyKey] = animationCurve;

      EditorGUI.indentLevel--;

      if (EditorGUI.EndChangeCheck()) {

        // Save information to restore the Unity AnimationCurve.
        keysProperty.ClearArray();
        keysProperty.arraySize = animationCurve.keys.Length;
        for (int i = 0; i < animationCurve.keys.Length; i++) {
          var key = animationCurve.keys[i];
          var keyProperty = keysProperty.GetArrayElementAtIndex(i);
          GetPropertyNext(keyProperty, "Time").longValue = FP.FromFloat_UNSAFE(key.time).RawValue;
          GetPropertyNext(keyProperty, "Value").longValue = FP.FromFloat_UNSAFE(key.value).RawValue;
          try {
            GetPropertyNext(keyProperty, "InTangent").longValue = FP.FromFloat_UNSAFE(key.inTangent).RawValue;
          }
          catch (System.OverflowException) {
            GetPropertyNext(keyProperty, "InTangent").longValue = Mathf.Sign(key.inTangent) < 0.0f ? FP.MinValue.RawValue : FP.MaxValue.RawValue;
          }
          try {
            GetPropertyNext(keyProperty, "OutTangent").longValue = FP.FromFloat_UNSAFE(key.outTangent).RawValue;
          }
          catch (System.OverflowException) {
            GetPropertyNext(keyProperty, "OutTangent").longValue = Mathf.Sign(key.outTangent) < 0.0f ? FP.MinValue.RawValue : FP.MaxValue.RawValue;
          }

          keyProperty.FindPropertyRelative("TangentModeLeft").intValue = (int)AnimationUtility.GetKeyLeftTangentMode(animationCurve, i);
          keyProperty.FindPropertyRelative("TangentModeRight").intValue = (int)AnimationUtility.GetKeyRightTangentMode(animationCurve, i);
          keyProperty.FindPropertyRelative("TangentMode").intValue = 0;
        }

        // Save the curve onto the Quantum FPAnimationCurve object via SerializedObject.
        preWrapModeProperty.intValue = (int)GetWrapMode(animationCurve.preWrapMode);
        postWrapModeProperty.intValue = (int)GetWrapMode(animationCurve.postWrapMode);
        preWrapModeOriginalProperty.intValue = (int)animationCurve.preWrapMode;
        postWrapModeOriginalProperty.intValue = (int)animationCurve.postWrapMode;

        // Get the used segment.
        float startTime = animationCurve.keys.Length == 0 ? 0.0f : float.MaxValue;
        float endTime = animationCurve.keys.Length == 0 ? 1.0f : float.MinValue; ;
        for (int i = 0; i < animationCurve.keys.Length; i++) {
          startTime = Mathf.Min(startTime, animationCurve[i].time);
          endTime = Mathf.Max(endTime, animationCurve[i].time);
        }

        startTimeProperty.longValue = FP.FromFloat_UNSAFE(startTime).RawValue;
        endTimeProperty.longValue = FP.FromFloat_UNSAFE(endTime).RawValue;

        // Save the curve inside an array with specific resolution.
        var resolution = resolutionProperty.intValue;
        if (resolution <= 0)
          return;
        samplesProperty.ClearArray();
        samplesProperty.arraySize = resolution + 1;
        var deltaTime = (endTime - startTime) / (float)resolution;
        for (int i = 0; i < resolution + 1; i++) {
          var time = startTime + deltaTime * i;
          var fp = FP.FromFloat_UNSAFE(animationCurve.Evaluate(time));
          GetArrayElementNext(samplesProperty, i).longValue = fp.RawValue;
        }

        prop.serializedObject.ApplyModifiedProperties();
      }
    }

    private static SerializedProperty GetPropertyNext(SerializedProperty prop, string name) {
      var result = prop.FindPropertyRelative(name);
      if (result != null)
        result.Next(true);
      
      return result;
    }

    private static SerializedProperty GetArrayElementNext(SerializedProperty prop, int index) {
      var result = prop.GetArrayElementAtIndex(index);
      result.Next(true);
      return result;
    }

    private static FPAnimationCurve.WrapMode GetWrapMode(WrapMode wrapMode) {
      switch (wrapMode) {
        case WrapMode.Loop:
          return FPAnimationCurve.WrapMode.Loop;
        case WrapMode.PingPong:
          return FPAnimationCurve.WrapMode.PingPong;
        default:
          return FPAnimationCurve.WrapMode.Clamp;
      }
    }

    private static AnimationUtility.TangentMode ConvertTangetMode(int depricatedTangentMode, bool isLeftOrRight) {
      // old to new conversion
      // Left
      // Free     0000001 -> 00000000 (TangentMode.Free)
      // Constant 0000111 -> 00000011 (TangentMode.Constant)
      // Linear   0000101 -> 00000010 (TangentMode.Linear)
      // Right
      // Free     0000001 -> 00000000 (TangentMode.Free)
      // Linear   1000001 -> 00000010 (TangentMode.Constant)
      // Constant 1100001 -> 00000011 (TangentMode.Linear)

      var shift = isLeftOrRight ? 1 : 5;

      if (((depricatedTangentMode >> shift) & 0x3) == (int)AnimationUtility.TangentMode.Linear) {
        return AnimationUtility.TangentMode.Linear;
      }
      else if (((depricatedTangentMode >> shift) & 0x3) == (int)AnimationUtility.TangentMode.Constant) {
        return AnimationUtility.TangentMode.Constant;
      }

      return AnimationUtility.TangentMode.Free;
    }
  }
}

