using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace Quantum {
  [AttributeUsage(AttributeTargets.Field)]
  public class LocalReferenceAttribute : PropertyAttribute {
  }
}

#if UNITY_EDITOR

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(LocalReferenceAttribute))]
  public class LocalReferenceAttributeDrawer : PropertyDrawer {

    private static GUIStyle ErrorOverlayStyle {
      get {
        if (errorOverlay == null) {
          errorOverlay = new GUIStyle(EditorStyles.miniLabel) {
            alignment = TextAnchor.MiddleRight,
            contentOffset = new Vector2(-24, 0),
          };
          errorOverlay.normal.textColor = Color.red.Alpha(0.9f);
        }
        return errorOverlay;
      }
    }

    private static GUIStyle errorOverlay;
    private string lastError;
    private string lastErrorPropertyPath;

    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label) {

      EditorGUI.BeginChangeCheck();
      EditorGUI.PropertyField(position, prop, label);
      if (EditorGUI.EndChangeCheck()) {
        lastError = null;
      }

      if (lastError != null && lastErrorPropertyPath == prop.propertyPath) {
        GUI.Label(position, lastError, ErrorOverlayStyle);
      }

      var reference = prop.objectReferenceValue;
      if (reference == null)
        return;

      var target = prop.serializedObject.targetObject;

      if (target is MonoBehaviour mb) {
        if (reference is Component comp) {
          if (!AreLocal(mb, comp)) {
            NonLocalReferenceDetected(prop);
          }
        } else {
          throw new NotImplementedException("MonoBehaviour to ScriptableObject not supported yet");
        }
      } else {
        throw new NotImplementedException("ScriptableObject not supported yet");
      }
    }

    public static bool AreLocal(Component a, Component b) {
      if (EditorUtility.IsPersistent(a)) {
        if (AssetDatabase.GetAssetPath(a) != AssetDatabase.GetAssetPath(b)) {
          return false;
        }
      } else {
        if (a.gameObject.scene != b.gameObject.scene) {
          return false;
        }
      }
      return true;
    }

    private void NonLocalReferenceDetected(SerializedProperty prop) {
      prop.objectReferenceValue = null;
      prop.serializedObject.ApplyModifiedProperties();
      lastError = "Use only local references";
      lastErrorPropertyPath = prop.propertyPath;
    }
  }
}

#endif