using System;
using System.Linq;
using Quantum.Editor;
using UnityEditor;
using UnityEngine;
using Quantum;

public abstract class NestedAssetBaseEditor : AssetBaseEditor {
  public override void OnInspectorGUI() {
    base.OnInspectorGUI();

    var assetObj = (AssetBase)target;
    var asset = (IQuantumPrefabNestedAsset)target;

    if (asset.Parent == null) {
      EditorGUILayout.HelpBox($"Needs to an asset nested in a prefab.", MessageType.Error);
    }

    using (new EditorGUI.DisabledScope(asset.Parent == null)) {
      if (GUILayout.Button("Select Prefab")) {
        Selection.activeObject = asset.Parent;
      }
    }

    bool canDelete = asset.Parent == null;
    if (asset.Parent != null) {
      string assetPath = AssetDatabase.GetAssetPath(assetObj);
      if (QuantumEditorSettings.Instance.AssetSearchPaths.Any(x => assetPath.StartsWith(x))) {
        // a part of quantum db
      } else {
        canDelete = true;
      }
    }

    using (new EditorGUI.DisabledScope(!canDelete)) {
      if (GUILayout.Button("Delete Nested Object")) {
        Selection.activeObject = null;
        UnityEngine.Object.DestroyImmediate(assetObj, true);
        AssetDatabase.SaveAssets();
      }
    }
  }

  private static string GetName(AssetBase asset, UnityEngine.Object parent) {
    return parent.name + asset.AssetObject.GetType().Name;
  }

  public static IQuantumPrefabNestedAsset GetNested(Component parent, System.Type assetType) {
    ThrowIfNotNestedAsset(assetType, parent);

    var parentPath = AssetDatabase.GetAssetPath(parent.gameObject);

    if (string.IsNullOrEmpty(parentPath) || !AssetDatabase.IsMainAsset(parent.gameObject)) {
      throw new System.InvalidOperationException($"{parent} is not a main asset");
    }

    var subAssets = AssetDatabase.LoadAllAssetsAtPath(parentPath).Where(x => x?.GetType() == assetType).ToList();
    Debug.Assert(subAssets.Count <= 1, $"More than 1 asset of type {assetType} on {parent}, clean it up manually");

    return subAssets.Count == 0 ? null : (IQuantumPrefabNestedAsset)subAssets[0];
  }

  private static void ThrowIfNotNestedAsset(Type type, Component parent) {
    if (type == null)
      throw new ArgumentNullException(nameof(type));
    if (parent == null)
      throw new ArgumentNullException(nameof(parent));

    if (!type.IsSubclassOf(typeof(AssetBase))) {
      throw new InvalidOperationException($"Type {type} is not a subclass of {nameof(AssetBase)}");
    }

    if (type.GetInterface(typeof(IQuantumPrefabNestedAsset).FullName) == null) {
      throw new InvalidOperationException($"Type {type} does not implement {nameof(IQuantumPrefabNestedAsset)}");
    }

    var genericInterface = type.GetInterfaces()
      .Where(x => x.IsConstructedGenericType)
      .Where(x => x.GetGenericTypeDefinition() == typeof(IQuantumPrefabNestedAsset<>))
      .SingleOrDefault();

    if (genericInterface == null) {
      throw new InvalidOperationException($"Type {type} does not implement {nameof(IQuantumPrefabNestedAsset)}<>");
    }

    var expectedParentComponent = genericInterface.GetGenericArguments()[0];
    if (expectedParentComponent != parent.GetType() && !parent.GetType().IsSubclassOf(expectedParentComponent)) {
      throw new InvalidOperationException($"Parent's type ({parent.GetType()}) is not equal nor is a subclass of {expectedParentComponent}");
    }
  }

  public static bool EnsureExists(Component parent, System.Type assetType, out IQuantumPrefabNestedAsset result, bool save = true) {
    ThrowIfNotNestedAsset(assetType, parent);

    var parentPath = AssetDatabase.GetAssetPath(parent.gameObject);

    if (string.IsNullOrEmpty(parentPath) || !AssetDatabase.IsMainAsset(parent.gameObject)) {
      throw new System.InvalidOperationException($"{parent} is not a main asset");
    }

    var subAsset = GetNested(parent, assetType);
    bool isDirty = false;

    AssetBase assetObj;

    if (subAsset != null) {
      assetObj = (AssetBase)subAsset;
      result = subAsset;
      Debug.Assert(result != null);
    } else {
      assetObj = (AssetBase)ScriptableObject.CreateInstance(assetType);
      AssetDatabase.AddObjectToAsset(assetObj, parentPath);
      result = (IQuantumPrefabNestedAsset)assetObj;
      isDirty = true;
    }

    if (assetObj == null) {
      throw new InvalidOperationException($"Failed to create an instance of {assetType}");
    }

    string targetName = GetName(assetObj, parent);
    if (assetObj.name != targetName) {
      assetObj.name = targetName;
      isDirty = true;
    }

    if (result.Parent != parent) {
      var so = new SerializedObject(assetObj);
      var parentProperty = so.FindProperty("Parent");
      if (parentProperty == null) {
        throw new InvalidOperationException("Nested assets are expected to have \"Parent\" field");
      }

      parentProperty.objectReferenceValue = parent;
      so.ApplyModifiedPropertiesWithoutUndo();
      if (parentProperty.objectReferenceValue != parent)
        throw new InvalidOperationException($"Unable to set property Parent. Is the type convertible from {parent.GetType()}?");
      isDirty = true;
    }

    if (isDirty) {
      EditorUtility.SetDirty(assetObj);
      EditorUtility.SetDirty(parent.gameObject);
    }

    if (isDirty && save) {
      AssetDatabase.SaveAssets();
    }

    return isDirty;
  }

  public static void ClearParent(AssetBase asset) {
    using (var so = new SerializedObject(asset)) {
      so.FindPropertyOrThrow("Parent").objectReferenceValue = null;
      so.ApplyModifiedPropertiesWithoutUndo();
    }
  }

  public static bool EnsureExists<T, SubType>(T parent, out SubType result, bool save = true)
    where T : Component
    where SubType : IQuantumPrefabNestedAsset<T> {
    var flag = EnsureExists(parent, typeof(SubType), out IQuantumPrefabNestedAsset temp, save);
    result = (SubType)temp;
    return flag;
  }

  public static bool GetNested<T, SubType>(T parent, out SubType result)
    where T : Component
    where SubType : IQuantumPrefabNestedAsset<T> {
    result = (SubType)GetNested(parent, typeof(SubType));
    return result != null;
  }

  public static Type GetHostType(Type nestedAssetType) {

    var interfaceTypes = nestedAssetType.GetInterfaces();
    foreach (var t in interfaceTypes) {
      if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IQuantumPrefabNestedAsset<>)) {
        return t.GetGenericArguments()[0];
      }
    }

    throw new InvalidOperationException();
  }

  public static NestedAssetType CreateWithParentPrefab<NestedAssetType>(string path)
    where NestedAssetType : AssetBase, IQuantumPrefabNestedAsset {
    var name = System.IO.Path.GetFileNameWithoutExtension(path);
    var componentType = GetHostType(typeof(NestedAssetType));

    var go = new GameObject(name, componentType);
    try {
      var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
      if (!prefab)
        return null;
      return AssetDatabase.LoadAssetAtPath<NestedAssetType>(AssetDatabase.GetAssetPath(prefab));
    } finally {
      UnityEngine.Object.DestroyImmediate(go);
      AssetDatabase.SaveAssets();
    }
  }

  public static void CreateNewAssetMenuItem<NestedAssetType>() where NestedAssetType : AssetBase, IQuantumPrefabNestedAsset {
    var activeDirectory = AssetDatabase.GetAssetPath(Selection.activeObject);

    if (string.IsNullOrEmpty(activeDirectory)) {
      return;
    }

    if (!System.IO.Directory.Exists(activeDirectory)) {
      activeDirectory = System.IO.Path.GetDirectoryName(activeDirectory);
    }

    var targetPath = AssetDatabase.GenerateUniqueAssetPath($"{activeDirectory}/{typeof(NestedAssetType).Name}.prefab");
    CreateWithParentPrefab<NestedAssetType>(targetPath);
  }

  public static void AssetLinkOnGUI<NestedAssetType>(Rect position, SerializedProperty property, GUIContent label) where NestedAssetType : AssetBase, IQuantumPrefabNestedAsset {
    AssetRefDrawer.DrawAssetRefSelector(position, property, label, typeof(NestedAssetType), createAssetCallback: () => {
      var assetPath = string.Format($"{QuantumEditorSettings.Instance.DatabasePath}/{nameof(NestedAssetType)}.prefab");
      return CreateWithParentPrefab<NestedAssetType>(assetPath);
    });
  }
}