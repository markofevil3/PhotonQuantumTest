using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomPropertyDrawer(typeof(AssetRef))]
  public class AssetRefDrawer : PropertyDrawer {
    static GUIStyle _overlay;

    public static GUIStyle OverlayStyle {
      get {
        if (_overlay == null) {
          _overlay = new GUIStyle(EditorStyles.miniLabel);
          _overlay.alignment = TextAnchor.MiddleRight;
          _overlay.contentOffset = new Vector2(-24, 0);
          _overlay.normal.textColor = Color.red.Alpha(0.9f);
        }

        return _overlay;
      }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      DrawAssetRefSelector(position, property, label);
    }

    public static unsafe void DrawAssetRefSelector(Rect position, SerializedProperty property, GUIContent label, Type type = null, Func<AssetBase> createAssetCallback = null) {
      type = type ?? typeof(AssetBase);

      var guidProperty = property.FindPropertyRelativeOrThrow("Id");
      var valueProperty = guidProperty.FindPropertyRelativeOrThrow("Value");
      var guid = (AssetGuid)valueProperty.longValue;

      var currentObject = default(AssetBase);
      if (guid.IsValid) {
        currentObject = UnityDB.FindAssetForInspector(guid);
      }

      EditorGUI.BeginChangeCheck();
      EditorGUI.BeginProperty(position, label, valueProperty);

      AssetBase selected = null;

      if (currentObject == null && !guid.IsValid) {
        position.width -= 25;
        selected = EditorGUI.ObjectField(position, label, null, type, false) as AssetBase;
        var buttonPosition = position.AddX(position.width).SetWidth(20);
        using (new EditorGUI.DisabledScope(type.IsAbstract)) {
          if (GUI.Button(buttonPosition, "+", EditorStyles.miniButton)) {
            if (createAssetCallback != null) {
              selected = createAssetCallback();
            } else {
              selected = ScriptableObject.CreateInstance(type) as AssetBase;
              var assetPath = string.Format($"{QuantumEditorSettings.Instance.DatabasePath}/{type.ToString()}.asset");
              assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
              AssetDatabase.CreateAsset(selected, assetPath);
              AssetDatabase.SaveAssets();
            }
          }
        }
      } else {
        var rect = EditorGUI.PrefixLabel(position, label);
        using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
          selected = DrawAsset(rect, guid);
        }
      }

      EditorGUI.EndProperty();

      if (EditorGUI.EndChangeCheck()) {
        valueProperty.longValue = selected != null ? selected.AssetObject.Guid.Value : 0L;
      }
    }

    private static Func<AssetBase, Boolean> ObjectFilter(AssetGuid guid, Type type) {
      return
        obj => obj &&
        type.IsInstanceOfType(obj) &&
        obj.AssetObject != null &&
        obj.AssetObject.Guid == guid;
    }

    internal static AssetBase DrawAsset(Rect position, AssetGuid assetGuid) {
      if (!assetGuid.IsValid) {
        return (AssetBase)EditorGUI.ObjectField(position, null, typeof(AssetBase), false);
      }

      if (assetGuid.IsDynamic) {
        // try to get an asset from the main runner
        var frame = QuantumRunner.Default ? QuantumRunner.Default.Game.Frames.Verified : null;
        if (frame != null) {
          var asset = frame.FindAsset<AssetObject>(assetGuid);
          if (asset != null) {
            if (EditorGUI.DropdownButton(position, new GUIContent(asset.ToString()), FocusType.Keyboard)) {
              // serialize asset
              var serializer = new QuantumUnityJsonSerializer() { IsPrettyPrintEnabled = true };
              var content = serializer.SerializeAssetReadable(asset);
              PopupWindow.Show(position, new TextPopupContent() { Text = content });
            }
          } else {
            EditorGUI.ObjectField(position, null, typeof(AssetBase), false);
            GUI.Label(position, "(dynamic asset not found)", OverlayStyle);
          }
        } else {
          EditorGUI.ObjectField(position, null, typeof(AssetBase), false);
          GUI.Label(position, "(dynamic asset not found)", OverlayStyle);
        }
        return null;
      } else {
        var asset = UnityDB.FindAssetForInspector(assetGuid);
        var result = EditorGUI.ObjectField(position, asset, asset?.GetType() ?? typeof(AssetBase), false);
        if (asset == null) {
          GUI.Label(position, "(asset missing)", OverlayStyle);
        }
        return (AssetBase)result;
      }
    }

    private sealed class TextPopupContent : PopupWindowContent {

      public string Text;
      private Vector2? _size;
      private Vector2 _scroll;

      public override Vector2 GetWindowSize() {
        if (_size == null) {
          var size = EditorStyles.textArea.CalcSize(new GUIContent(Text));
          size.x += 25; // account for the scroll bar & margins
          size.y += 10; // account for margins
          size.x = Mathf.Min(500, size.x);
          size.y = Mathf.Min(400, size.y);
          _size = size;
        }
        return _size.Value;
      }

      public override void OnGUI(Rect rect) {

        using (new GUILayout.AreaScope(rect)) {
          using (var scroll = new EditorGUILayout.ScrollViewScope(_scroll)) {
            _scroll = scroll.scrollPosition;
            EditorGUILayout.TextArea(Text);
          }
        }
      }
    }
  }

  [CustomPropertyDrawer(typeof(AssetRefCharacterController2DConfig))]
  public class AssetRefCharacterController2DConfigPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(CharacterController2DConfigAsset));
    }
  }

  [CustomPropertyDrawer(typeof(AssetRefCharacterController3DConfig))]
  public class AssetRefCharacterController3DConfigPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(CharacterController3DConfigAsset));
    }
  }

  [CustomPropertyDrawer(typeof(AssetRefEntityView))]
  public class AssetRefEntityViewPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(EntityViewAsset));
    }
  }

  [CustomPropertyDrawer(typeof(AssetRefMap))]
  public class AssetRefMapPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(MapAsset));
    }
  }

  [CustomPropertyDrawer(typeof(AssetRefNavMesh))]
  public class AssetRefNavMeshPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(NavMeshAsset));
    }
  }

  [CustomPropertyDrawer(typeof(AssetRefNavMeshAgentConfig))]
  public class AssetRefNavMeshAgentConfigPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(NavMeshAgentConfigAsset));
    }
  }

  [CustomPropertyDrawer(typeof(AssetRefPhysicsMaterial))]
  public class AssetRefPhysicsMaterialPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(PhysicsMaterialAsset));
    }
  }

  [CustomPropertyDrawer(typeof(AssetRefPolygonCollider))]
  public class AssetRefPolygonColliderPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(PolygonColliderAsset));
    }
  }

  [CustomPropertyDrawer(typeof(AssetRefSimulationConfig))]
  public class AssetRefSimulationConfigPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(SimulationConfigAsset));
    }
  }

  [CustomPropertyDrawer(typeof(AssetRefTerrainCollider))]
  public class AssetRefTerrainColliderPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(TerrainColliderAsset));
    }
  }
}
