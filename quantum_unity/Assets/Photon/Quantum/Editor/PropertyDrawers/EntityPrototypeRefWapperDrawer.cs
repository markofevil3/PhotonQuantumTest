using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {

  [CustomPropertyDrawer(typeof(EntityPrototypeRefWrapper))]
  public class EntityPrototypeRefWrapperEditor : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      var sceneReferenceProperty = property.FindPropertyRelativeOrThrow(nameof(EntityPrototypeRefWrapper.ScenePrototype));
      var assetRefProperty = property.FindPropertyRelativeOrThrow(nameof(EntityPrototypeRefWrapper.AssetPrototype));
      var assetRefValueProperty = assetRefProperty.FindPropertyRelative("Id.Value");

      using (new CustomEditorsHelper.PropertyScope(position, label, property)) {
        var rect = EditorGUI.PrefixLabel(position, label);

        bool showAssetRef = assetRefValueProperty.longValue != 0 || sceneReferenceProperty.objectReferenceValue == null;
        bool showReference = sceneReferenceProperty.objectReferenceValue != null || assetRefValueProperty.longValue == 0;

        Debug.Assert(showAssetRef || showReference);

        if (showAssetRef && showReference) {
          rect.width /= 2;
        }

        if (showReference) {
          EditorGUI.BeginChangeCheck();
          using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
            EditorGUI.PropertyField(rect, sceneReferenceProperty, GUIContent.none);
          }
          rect.x += rect.width;
          if (EditorGUI.EndChangeCheck()) {
            assetRefValueProperty.longValue = 0;
            property.serializedObject.ApplyModifiedProperties();
          }
        }

        if (showAssetRef) {
          EditorGUI.BeginChangeCheck();
          using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
            EditorGUI.PropertyField(rect, assetRefProperty, GUIContent.none);
          }
          if (EditorGUI.EndChangeCheck()) {
            sceneReferenceProperty.objectReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
          }
        }
      }
    }
  }
}