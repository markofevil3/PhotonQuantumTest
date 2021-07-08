using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(EntityPrototypeAsset), false)]
  public class EntityPrototypeAssetEditor : NestedAssetBaseEditor {
    [MenuItem("Assets/Create/Quantum/EntityPrototype", priority = EditorDefines.AssetMenuPriorityStart + 5 * 26)]
    public static void CreateMenuItem() => NestedAssetBaseEditor.CreateNewAssetMenuItem<EntityPrototypeAsset>();
  }

  [CustomPropertyDrawer(typeof(AssetRefEntityPrototype))]
  public class EntityPrototypeLinkPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) => NestedAssetBaseEditor.AssetLinkOnGUI<EntityPrototypeAsset>(position, property, label);
  }
}
