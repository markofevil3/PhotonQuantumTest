using Quantum;
using UnityEngine;
using UnityEditor;
namespace Quantum.Editor {

[CustomPropertyDrawer(typeof(AssetRefMapNodeSpec))]
public class AssetRefMapNodeSpecPropertyDrawer : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(MapNodeSpecAsset));
  }
}

}
