
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(EntityViewAsset), false)]
  public class EntityViewAssetEditor : NestedAssetBaseEditor {
    [MenuItem("Assets/Create/Quantum/EntityView", priority = EditorDefines.AssetMenuPriorityStart + 5 * 26)]
    public static void CreateMenuItem() => NestedAssetBaseEditor.CreateNewAssetMenuItem<EntityViewAsset>();
  }

  [CustomPropertyDrawer(typeof(AssetRefEntityView))]
  public class EntityViewLinkPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) => NestedAssetBaseEditor.AssetLinkOnGUI<EntityViewAsset>(position, property, label);
  }
}
