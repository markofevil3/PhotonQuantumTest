using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {

  [CustomPropertyDrawer(typeof(ComponentPrototypeRefWrapper))]
  public class ComponentPrototypeRefWrapperDrawer : PropertyDrawer {

    //private static Dictionary<string, Type> _quantumTypeNameToUnityType = Unit

    public static void DrawMultiField(Rect position, SerializedProperty property, GUIContent label, string componentTypeName, bool hasComponentTypeName = false) {
      var sceneReferenceProperty = property.FindPropertyRelativeOrThrow("_scenePrototype");
      var assetRefProperty = property.FindPropertyRelativeOrThrow(nameof(ComponentPrototypeRefWrapper.AssetPrototype));
      var assetRefValueProperty = assetRefProperty.FindPropertyRelative("Id.Value");
      var assetRefValue = new AssetGuid(assetRefValueProperty.longValue);

      using (new CustomEditorsHelper.PropertyScope(position, label, property)) {
        var rect = EditorGUI.PrefixLabel(position, label);

        bool showAssetRef = assetRefValue.IsValid || sceneReferenceProperty.objectReferenceValue == null;
        bool showReference = sceneReferenceProperty.objectReferenceValue != null || !assetRefValue.IsValid;
        bool showTypeCombo = hasComponentTypeName & !showReference;

        Debug.Assert(showAssetRef || showReference);

        if (showAssetRef && showReference) {
          rect.width /= 2;
        } else if (showTypeCombo) {
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

        string[] typePickerOptions = Array.Empty<string>();

        if (showAssetRef) {

          Rect assetRefRect = rect;

          if (assetRefValue.IsValid) {
            if (UnityDB.FindAssetForInspector(new AssetGuid(assetRefValueProperty.longValue)) is EntityPrototypeAsset asset) {

              var components = asset.Parent.GetComponents<EntityComponentBase>();

              if (components.Length == 0) {
                assetRefRect = CustomEditorsHelper.DrawIconPrefix(rect, $"Prototype has no components", MessageType.Error);
              } else if (string.IsNullOrEmpty(componentTypeName)) {
                assetRefRect = CustomEditorsHelper.DrawIconPrefix(rect, $"Component type not selected", MessageType.Error);
              } else {
                var component = Array.Find(components, c => c.ComponentType.Name == componentTypeName);
                if (!component) {
                  assetRefRect = CustomEditorsHelper.DrawIconPrefix(rect, $"Component {componentTypeName} not found", MessageType.Error);
                }
              }

              if (hasComponentTypeName) {
                typePickerOptions = components.Select(x => x.ComponentType.Name).ToArray();
              }
            }
          }

          EditorGUI.BeginChangeCheck();
          using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
            EditorGUI.PropertyField(assetRefRect, assetRefProperty, GUIContent.none);
          }

          rect.x += rect.width;
          if (EditorGUI.EndChangeCheck()) {
            sceneReferenceProperty.objectReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
          }
        }

        if (showTypeCombo) {
          var componentTypeProperty = property.FindPropertyRelativeOrThrow("_componentTypeName");
          using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
            using (new CustomEditorsHelper.PropertyScope(rect, GUIContent.none, componentTypeProperty)) {
              var index = Array.FindIndex(typePickerOptions, x => string.Equals(x, componentTypeName, StringComparison.OrdinalIgnoreCase));
              EditorGUI.BeginChangeCheck();
              index = EditorGUI.Popup(rect, index, typePickerOptions);
              if (EditorGUI.EndChangeCheck()) {
                componentTypeProperty.stringValue = typePickerOptions[index];
                property.serializedObject.ApplyModifiedProperties();
              }
            }
          }
        }
      }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      var componentTypeName = property.FindPropertyRelative("_componentTypeName");
      DrawMultiField(position, property, label, componentTypeName.stringValue, hasComponentTypeName: true);
    }
  }

  [CustomPropertyDrawer(typeof(ComponentPrototypeRefWrapperBase), true)]
  public class ComponentPrototypeRefWrapperBaseDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      var elementType = fieldInfo.FieldType;
      if (elementType.HasElementType) {
        elementType = elementType.GetElementType();
      }

      var baseType = elementType.BaseType;
      Debug.Assert(baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(ComponentPrototypeRefWrapperBase<,>));

      var componentType = baseType.GetGenericArguments()[1];
      Debug.Assert(componentType.GetInterface("Quantum.IComponent") != null);

      ComponentPrototypeRefWrapperDrawer.DrawMultiField(position, property, label, componentType.Name, hasComponentTypeName: false);
    }
  }
}