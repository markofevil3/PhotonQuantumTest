using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Photon.Deterministic;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {

  [CustomEditor(typeof(global::EntityPrototype), false)]
  public class EntityPrototypeEditor : UnityEditor.Editor {

    private static readonly HashSet<Type> excludedComponents = new HashSet<Type>(new[] {
      typeof(EntityComponentTransform2D),
      typeof(EntityComponentTransform2DVertical),
      typeof(EntityComponentTransform3D),
      typeof(EntityComponentPhysicsCollider2D),
      typeof(EntityComponentPhysicsBody2D),
      typeof(EntityComponentPhysicsCollider3D),
      typeof(EntityComponentPhysicsBody3D),
      typeof(EntityComponentNavMeshPathfinder),
      typeof(EntityComponentNavMeshSteeringAgent),
      typeof(EntityComponentNavMeshAvoidanceAgent),
      typeof(EntityComponentView),
    });

    private static readonly Type[] componentPrototypeTypes = typeof(EntityComponentBase).Assembly.GetTypes()
      .Where(x => !x.IsAbstract)
      .Where(x => x.BaseType?.IsSubclassOf(typeof(EntityComponentBase)) == true)
      .Where(x => x.GetCustomAttribute<ObsoleteAttribute>() == null)
      .Where(x => !excludedComponents.Contains(x))
      .ToArray();

    private static readonly GUIContent[] transformPopupOptions = new[] {
      new GUIContent("2D"),
      new GUIContent("3D"),
      new GUIContent("None"),
    };

    

    private static Lazy<Skin> _skin = new Lazy<Skin>(() => new Skin());

    private static Skin skin => _skin.Value;

    private static readonly int[] transformPopupValues = new[] {
      (int)EntityPrototypeTransformMode.Transform2D,
      (int)EntityPrototypeTransformMode.Transform3D,
      (int)EntityPrototypeTransformMode.None
    };

    private class Skin {
      public readonly int titlebarHeight = 20;
      public readonly int imageIconSize = 16;
      public readonly int imagePadding = EditorStyles.foldout.padding.left + 1;
      public readonly GUIStyle boldFoldout = new GUIStyle(EditorStyles.foldout) {
        fontStyle = FontStyle.Bold,
        padding = new RectOffset(EditorStyles.foldout.padding.left + 16 + 2, 0, 2, 0),
      };
      public readonly GUIStyle inspectorTitlebar = new GUIStyle("IN Title") {
        alignment = TextAnchor.MiddleLeft
      };
      public readonly string cross = "\u2715";
      public readonly float buttonHeight = EditorGUIUtility.singleLineHeight;
      public readonly float buttonWidth = 19.0f;

      public Color inspectorTitlebarBackground =>
        EditorGUIUtility.isProSkin ? new Color32(64, 64, 64, 255) : new Color32(222, 222, 222, 255);
    }

    private static readonly Lazy<GUIStyle> watermarkStyle = new Lazy<GUIStyle>(() => {
      var result = new GUIStyle(EditorStyles.miniLabel);
      result.alignment = TextAnchor.MiddleRight;
      result.contentOffset = new Vector2(-2, 0);
      Color c = result.normal.textColor;
      c.a = 0.6f;
      result.normal.textColor = c;
      return result;
    });

    private static readonly GUIContent physicsCollider2D = new GUIContent(nameof(Quantum.PhysicsCollider2D));
    private static readonly GUIContent physicsCollider3D = new GUIContent(nameof(Quantum.PhysicsCollider3D));
    private static readonly GUIContent physicsBody2D = new GUIContent(nameof(Quantum.PhysicsBody2D));
    private static readonly GUIContent physicsBody3D = new GUIContent(nameof(Quantum.PhysicsBody3D));
    private static readonly GUIContent navMeshPathfinder = new GUIContent(nameof(Quantum.NavMeshPathfinder));
    private static readonly GUIContent navMeshSteeringAgent = new GUIContent(nameof(Quantum.NavMeshSteeringAgent));
    private static readonly GUIContent navMeshAvoidanceAgent = new GUIContent(nameof(Quantum.NavMeshAvoidanceAgent));
    private static readonly GUIContent navMeshAvoidanceObstacle = new GUIContent(nameof(Quantum.NavMeshAvoidanceObstacle));

    public override void OnInspectorGUI() {
      if (!QuantumEditorSettings.Instance.UseQuantumAssetInspector) {
        base.OnInspectorGUI();
        return;
      }

      var target = (global::EntityPrototype)this.target;
      CustomEditorsHelper.DrawScript(target);

      if (AssetDatabase.IsMainAsset(target.gameObject)) {
        if (NestedAssetBaseEditor.GetNested(target, out EntityPrototypeAsset asset)) {
          using (new CustomEditorsHelper.BoxScope(nameof(EntityPrototypeAsset))) {
            CustomEditorsHelper.DrawDefaultInspector(new SerializedObject(asset), "Settings", new[] { "Settings.Container" }, false);
          }
        }
      }


      if (Application.isPlaying) {
        EditorGUILayout.HelpBox("Prototypes are only used for entity instantiation. To inspect an actual entity check its EntityView.", MessageType.Info);
      }

      using (new EditorGUI.DisabledScope(Application.isPlaying)) {

        // draw enum popup manually, because this way we can reorder and not follow naming rules
        EntityPrototypeTransformMode transformMode;
        {
          var prop = serializedObject.FindPropertyOrThrow(nameof(target.TransformMode));
          var rect = EditorGUILayout.GetControlRect();
          var label = new GUIContent("Transform");

          using (new CustomEditorsHelper.PropertyScope(rect, label, prop)) {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUI.IntPopup(rect, label, prop.intValue, transformPopupOptions, transformPopupValues);
            if (EditorGUI.EndChangeCheck()) {
              prop.intValue = value;
            }
            transformMode = (EntityPrototypeTransformMode)value;
          }
        }

        bool is2D = transformMode == EntityPrototypeTransformMode.Transform2D;
        bool is3D = transformMode == EntityPrototypeTransformMode.Transform3D;

        EditorGUI.BeginChangeCheck();

        try {

          var verticalProperty = serializedObject.FindPropertyOrThrow(nameof(target.Transform2DVertical));
          CustomEditorsHelper.DrawDefaultInspector(verticalProperty, skipRoot: false, label: new GUIContent("Transform2DVertical"));

          // EntityPrototypes is space optimised and shares some 2D/3D settings; because of that some
          // extra miles need to be taken to draw things in the right context
          var physicsProperty = serializedObject.FindPropertyOrThrow(nameof(target.PhysicsCollider));
          CustomEditorsHelper.DrawDefaultInspector(physicsProperty, skipRoot: false, label: is3D ? physicsCollider3D : physicsCollider2D, callback: (p, field, type) => {
            if (type == typeof(Component)) {
              if (is3D)
                CustomEditorsHelper.HandleMultiTypeField(p, typeof(Collider));
              else
                CustomEditorsHelper.HandleMultiTypeField(p, typeof(Collider), typeof(Collider2D));
              return true;
            }

            if (!is2D && type == typeof(Shape2DConfig) || !is3D && type == typeof(Shape3DConfig)) {
              p.isExpanded = false;
              return true;
            }
            return false;
          });

          var physicsBodyProperty = serializedObject.FindPropertyOrThrow(nameof(target.PhysicsBody));
          CustomEditorsHelper.DrawDefaultInspector(physicsBodyProperty, skipRoot: false, label: is3D ? physicsBody3D : physicsBody2D, callback: (p, field, type) => {
            if (is2D) {
              if (type == typeof(FPVector3) || type == typeof(PhysicsBody3D.ConfigFlags) || type == typeof(RotationFreezeFlags)) {
                p.isExpanded = false;
                return true;
              }

              if (type == typeof(FPVector2) && p.name == "CenterOfMass2D") {
                FPVector2PropertyDrawer.DrawCompact(EditorGUILayout.GetControlRect(), p, new GUIContent("Center Of Mass"));
                return true;
              }
            }

            if (is3D) {
              if (type == typeof(FPVector2) || type == typeof(PhysicsBody2D.ConfigFlags)) {
                p.isExpanded = false;
                return true;
              }

              if (type == typeof(FPVector3) && p.name == "CenterOfMass3D") {
                FPVector3PropertyDrawer.DrawCompact(EditorGUILayout.GetControlRect(), p, new GUIContent("Center Of Mass"));
                return true;
              }
            }

            return false;
          });

          // NavMeshes can be pointed to in 3 ways: scene reference, asset ref and scene name
          var navmeshConfigGuid = 0L;
          var navMeshPathfinderProperty = serializedObject.FindPropertyOrThrow(nameof(target.NavMeshPathfinder));
          CustomEditorsHelper.DrawDefaultInspector(navMeshPathfinderProperty, skipRoot: false, label: navMeshPathfinder, callback: (p, field, type) => {
            if (type == typeof(AssetRefNavMeshAgentConfig)) {
              var guidProperty = p.FindPropertyRelativeOrThrow("Id");
              var valueProperty = guidProperty.FindPropertyRelativeOrThrow("Value");
              navmeshConfigGuid = valueProperty.longValue;
            }
            if (type == typeof(global::EntityPrototype.NavMeshSpec)) {
              HandleNavMeshSpec(EditorGUILayout.GetControlRect(), p, new GUIContent(p.displayName));
              p.isExpanded = false;
              return true;
            }
            return false;
          });

          var navMeshSteeringAgentProperty = serializedObject.FindPropertyOrThrow(nameof(target.NavMeshSteeringAgent));
          CustomEditorsHelper.DrawDefaultInspector(navMeshSteeringAgentProperty, skipRoot: false, label: navMeshSteeringAgent, callback: (p, field, type) => {
            if (type == typeof(AssetRefNavMeshAgentConfig)) {
              var guidProperty = p.FindPropertyRelativeOrThrow("Id");
              var valueProperty = guidProperty.FindPropertyRelativeOrThrow("Value");
              if (valueProperty.longValue == 0) {
                valueProperty.longValue = navmeshConfigGuid;
              }
            }
            return false;
          });

          var navMeshAvoidaceAgent = serializedObject.FindPropertyOrThrow(nameof(target.NavMeshAvoidanceAgent));
          CustomEditorsHelper.DrawDefaultInspector(navMeshAvoidaceAgent, skipRoot: false, label: navMeshAvoidanceAgent, callback: (p, field, type) => {
            if (type == typeof(AssetRefNavMeshAgentConfig)) {
              var guidProperty = p.FindPropertyRelativeOrThrow("Id");
              var valueProperty = guidProperty.FindPropertyRelativeOrThrow("Value");
              if (valueProperty.longValue == 0) {
                valueProperty.longValue = navmeshConfigGuid;
              }
            }
            return false;
          });

          // View can be either taken from same GameObject or fallback to asset ref
          {
            var viewProperty = serializedObject.FindPropertyOrThrow(nameof(target.View));
            var hasView = target.GetComponent<global::EntityView>() != null;
            var rect = EditorGUILayout.GetControlRect(true);
            var label = new GUIContent(viewProperty.displayName);

            using (new CustomEditorsHelper.PropertyScope(rect, label, viewProperty)) {
              rect = EditorGUI.PrefixLabel(rect, label);
              using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
                if (hasView) {
                  EditorGUI.LabelField(rect, "Self");
                } else {
                  EditorGUI.PropertyField(rect, viewProperty, GUIContent.none);
                }
              }
            }
          }

          // add new component dropdown
          if (QuantumEditorSettings.Instance.EntityComponentInspectorMode == QuantumEntityComponentInspectorMode.ShowMonoBehaviours) {
            using (new EditorGUILayout.HorizontalScope()) {
              var existingComponentPrototypes = target.GetComponents<EntityComponentBase>()
                .Select(x => x.GetType())
                .ToList();

              var availableComponents = componentPrototypeTypes
                .Where(x => !existingComponentPrototypes.Contains(x))
                .ToList();

              using (new EditorGUI.DisabledScope(availableComponents.Count == 0)) {
                GUIStyle style = EditorStyles.miniPullDown;
                var content = new GUIContent("Add Entity Component");
                var rect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(content, style));
                if (EditorGUI.DropdownButton(rect, content, FocusType.Keyboard, style)) {
                  EditorUtility.DisplayCustomMenu(rect, availableComponents.Select(x => new GUIContent(x.Name)).ToArray(), -1,
                    (userData, opts, selected) => {
                      Undo.AddComponent(target.gameObject, availableComponents[selected]);
                      Repaint();
                    }, null);
                }
              }
            }
          }
        } finally {
          if (EditorGUI.EndChangeCheck()) {
            serializedObject.ApplyModifiedProperties();
          }
        }
      }

      try {
        target.PreSerialize();
      } catch (System.Exception ex) {
        EditorGUILayout.HelpBox(ex.Message, MessageType.Error);
      }

      target.CheckComponentDuplicates(msg => {
        EditorGUILayout.HelpBox(msg, MessageType.Warning);
      });

      if (QuantumEditorSettings.Instance.EntityComponentInspectorMode != QuantumEntityComponentInspectorMode.ShowMonoBehaviours) {
        
        using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
          var components = target.GetComponents<EntityComponentBase>()
            .Where(x => x != null);

          { 
            var labelRect = EditorGUILayout.GetControlRect(true);
            EditorGUI.LabelField(labelRect, "Entity Components", EditorStyles.boldLabel);

            var buttonRect = labelRect.AddX(labelRect.width).AddX(-skin.buttonWidth).SetWidth(skin.buttonWidth);
            if (GUI.Button(buttonRect, "+", EditorStyles.miniButton)) {

              var existingComponentPrototypes = components
                .Select(x => x.GetType())
                .ToList();

              var availableComponents = componentPrototypeTypes
                .Where(x => !existingComponentPrototypes.Contains(x))
                .ToList();

              EditorUtility.DisplayCustomMenu(buttonRect, availableComponents.Select(x => new GUIContent(EntityComponentBase.UnityComponentTypeToQuantumComponentType(x).Name)).ToArray(), -1,
                  (userData, opts, selected) => {
                    Undo.AddComponent(target.gameObject, availableComponents[selected]);
                    Repaint();
                  }, null);
            }
          }

          using (new EditorGUI.IndentLevelScope()) {
            foreach (var c in components) {

              var so = new SerializedObject(c);
              var sp = so.GetIterator();

              var rect = GUILayoutUtility.GetRect(GUIContent.none, skin.inspectorTitlebar);
              sp.isExpanded = EditorGUI.InspectorTitlebar(rect, sp.isExpanded, c, true);

              // draw over the default label, as it contains useless noise
              Rect textRect = new Rect(rect.x + 35, rect.y, rect.width - 100, rect.height);
              if (Event.current.type == EventType.Repaint) {

                using (new CustomEditorsHelper.ColorScope(skin.inspectorTitlebarBackground)) {
                  var texRect = textRect;
                  texRect.y += 2;
                  texRect.height -= 2;
                  GUI.DrawTextureWithTexCoords(texRect, Texture2D.whiteTexture, new Rect(0.5f, 0.5f, 0.0f, 0.0f), false);
                }

                skin.inspectorTitlebar.Draw(textRect, c.ComponentType.Name, false, false, false, false);
              }

              if (sp.isExpanded) {
                c.OnInspectorGUI(so, CustomEditorsHelper.EditorGUIProxy);
              }
            }
          }
        }
      }
    }

    private static void HandleNavMeshSpec(Rect position, SerializedProperty property, GUIContent label) {
      var referenceProp = property.FindPropertyRelativeOrThrow("Reference");
      var assetProp = property.FindPropertyRelativeOrThrow("Asset");
      var nameProp = property.FindPropertyRelativeOrThrow("Name");

      using (new CustomEditorsHelper.PropertyScope(position, label, property)) {
        var rect = EditorGUI.PrefixLabel(position, label);
        using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
          if (referenceProp.objectReferenceValue != null) {
            EditorGUI.PropertyField(rect, referenceProp, GUIContent.none);
          } else if (assetProp.FindPropertyRelativeOrThrow("Id.Value").longValue > 0) {
            EditorGUI.PropertyField(rect, assetProp, GUIContent.none);
          } else if (!string.IsNullOrEmpty(nameProp.stringValue)) {
            EditorGUI.PropertyField(rect, nameProp, GUIContent.none);
            GUI.Label(rect, "(NavMesh name)", watermarkStyle.Value);
          } else {
            rect.width /= 3;
            EditorGUI.PropertyField(rect, referenceProp, GUIContent.none);
            EditorGUI.PropertyField(rect.AddX(rect.width), assetProp, GUIContent.none);
            EditorGUI.PropertyField(rect.AddX(2 * rect.width), nameProp, GUIContent.none);
            GUI.Label(rect.AddX(2 * rect.width), "(NavMesh name)", watermarkStyle.Value);
          }
        }
      }
    }

    private static bool DoesTypeRequireComponent(Type obj, Type requirement) {
      return Attribute.GetCustomAttributes(obj, typeof(RequireComponent)).OfType<RequireComponent>()
             .Any(rc => rc.m_Type0.IsAssignableFrom(requirement));
    }

    internal static IEnumerable<Component> GetDependentComponents(GameObject go, Type t) {
      return go.GetComponents<Component>().Where(c => DoesTypeRequireComponent(c.GetType(), t));
    }
  }
}