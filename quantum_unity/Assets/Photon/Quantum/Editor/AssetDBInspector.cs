using System;
using System.Collections.Generic;
using System.IO;
using Quantum;
using Quantum.Editor;
using UnityEditor;
using UnityEngine;

public class AssetDBInspector : EditorWindow {
  private static readonly Color Grey = Color.gray * 1.2f;

  private Vector2 _scrollPos;

  private enum SortingOrder {
    Path,
    Filename,
    Guid,
    Type,
    Loaded
  };

  private enum ResourceType {
    Asset,
    View,
    Prototype
  }

  private static SortingOrder Sorting {
    get => (SortingOrder)EditorPrefs.GetInt("Quantum_AssetDBInspector_SortingOrder", 0);
    set => EditorPrefs.SetInt("Quantum_AssetDBInspector_SortingOrder", (int)value);
  }

  private static bool HideUnloaded {
    get => EditorPrefs.GetBool("Quantum_AssetDBInspector_HideUnloaded", false);
    set => EditorPrefs.SetBool("Quantum_AssetDBInspector_HideUnloaded", value);
  }

  private static ResourceType PathToResourceType(string path) {
    var separatorIndex = path.LastIndexOf(AssetBase.NestedPathSeparator);
    if (separatorIndex >= 0) {
      if (path.EndsWith(nameof(Quantum.EntityView)) || path.EndsWith("EntityPrefab")) {
        return ResourceType.View;
      }
      else if (path.EndsWith(nameof(Quantum.EntityPrototype))) {
        return ResourceType.Prototype;
      }
    }

    return ResourceType.Asset;
  }

  [MenuItem("Window/Quantum/AssetDB Inspector")]
  [MenuItem("Quantum/Show AssetDB Inspector", false, 41)]
  static void Init() {
    AssetDBInspector window = (AssetDBInspector)GetWindow(typeof(AssetDBInspector), false, "Quantum Asset DB");
    window.Show();
  }

  public void OnGUI() {
    using (new GUILayout.HorizontalScope()) {
      if (QuantumRunner.Default == null && GUILayout.Button("Generate Quantum Asset DB")) {
        AssetDBGeneration.Generate();
      }

      if (QuantumRunner.Default != null && GUILayout.Button("Dispose Resource Manager")) {
        UnityDB.Dispose();
      }

      EditorGUILayout.LabelField("Sort By", GUILayout.Width(50));
      Sorting = (SortingOrder)EditorGUILayout.EnumPopup(Sorting, new GUILayoutOption[] { GUILayout.Width(70) });

      EditorGUILayout.LabelField("Only Show Loaded", GUILayout.Width(110));
      HideUnloaded = EditorGUILayout.Toggle(HideUnloaded, GUILayout.Width(20));
    }

    using (new GUILayout.VerticalScope())
    using (var scrollView = new GUILayout.ScrollViewScope(_scrollPos)) {
      var resources = new List<AssetResource>(UnityDB.AssetResources);
      switch (Sorting) {
        case SortingOrder.Guid:
          resources.Sort((a, b) => a.Guid.CompareTo(b.Guid));
          break;
        case SortingOrder.Path:
          resources.Sort((a, b) => string.Compare(a.Path, b.Path, StringComparison.Ordinal));
          break;
        case SortingOrder.Filename:
          // the char | is inside the invalid character list
          resources.Sort((a, b) => string.Compare(Path.GetFileName(a.Path.Replace(AssetBase.NestedPathSeparator, '.')), Path.GetFileName(b.Path.Replace(AssetBase.NestedPathSeparator, '.')), StringComparison.Ordinal));
          break;
        case SortingOrder.Type:
          resources.Sort((a, b) => {
            var resourceTypeA = PathToResourceType(a.Path);
            var resourceTypeB = PathToResourceType(b.Path);
            if (resourceTypeA == resourceTypeB) {
              return string.Compare(a.Path, b.Path, StringComparison.Ordinal);
            }

            return resourceTypeA.CompareTo(resourceTypeB);
          });
          break;
        case SortingOrder.Loaded:
          resources.Sort((a, b) => b.StateValue.CompareTo(a.StateValue));
          break;
      }

      foreach (var resource in resources) {
        // first one is the null asset
        if (resource.Guid == 0)
          continue;

        var loaded = resource.IsLoaded;

        if (HideUnloaded && !loaded)
          continue;

        var rect = EditorGUILayout.GetControlRect();
        var rectIcon = new Rect(rect.position, new Vector2(20.0f, rect.size.y));
        var rectGuid = new Rect(new Vector2(rectIcon.xMax, rect.position.y), new Vector2(200.0f, rect.size.y));
        var rectButton = new Rect(new Vector2(rect.xMax - 60.0f, rect.position.y), new Vector2(60.0f, rect.size.y));
        var rectName = new Rect(new Vector2(rectGuid.xMax, rect.position.y), new Vector2(rect.size.x - rectGuid.xMax - rectButton.size.x, rect.size.y));

        GUI.color = loaded ? Color.green : Color.gray;
        EditorGUI.LabelField(rect, EditorGUIUtility.IconContent("blendSampler"));

        GUI.color = loaded ? Color.white : Grey;
        GUI.Label(rectGuid, resource.Guid.ToString());

        // TODO replace by NestedAsset Utils
        var resourceType = PathToResourceType(resource.Path);
        var resourcePath = resource.Path;
        var separatorIndex = resourcePath.LastIndexOf(AssetBase.NestedPathSeparator);
        if (separatorIndex >= 0) {
          resourcePath = resourcePath.Substring(0, separatorIndex);
        }

        var resourceName = resourcePath;
        if (Sorting == SortingOrder.Filename) {
          resourceName = Path.GetFileName(resourcePath);
        }

        var resourceLabel = loaded ? $"{resourceName} ({resource.AssetObject?.GetType().Name})" : $"{resourceName} ({resourceType})";
        var color = resourceType == ResourceType.View ? Color.cyan: resourceType == ResourceType.Prototype ? Color.yellow : Color.green;

        GUI.color = loaded ? Desaturate(color, 0.75f) : Desaturate(color, 0.25f);
        if (GUI.Button(rectName, new GUIContent(resourceLabel, resource.Path), GUI.skin.label)) {
          var selectCandidates = AssetDatabase.FindAssets(Path.GetFileName(resourcePath)); //, new string[] { $"Assets/Resources/{Path.GetDirectoryName(resource.URL)}"});

          var candidateGuid = string.Empty;
          switch (resourceType) {
            case ResourceType.Asset:
            case ResourceType.Prototype:
              candidateGuid = Array.Find(selectCandidates, c => AssetDatabase.GUIDToAssetPath(c).EndsWith($"{resourcePath}.asset"));
              break;
            case ResourceType.View:
              candidateGuid = Array.Find(selectCandidates, c => AssetDatabase.GUIDToAssetPath(c).EndsWith($"{resourcePath}.prefab"));
              break;
          }

          if (!string.IsNullOrEmpty(candidateGuid)) {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(candidateGuid));
          }
        }

        GUI.color = loaded ? Color.white : Grey;
        if (GUI.Button(rectButton, loaded ? "Dispose" : "Load")) {
          if (loaded) {
            UnityDB.ResourceManager.DisposeAsset(resource.Guid);
            UnityDB.ResourceManager.MainThreadSimulationEnded();
            return;
          }
          else {
            UnityDB.ResourceManager.GetAsset(resource.Guid, true);
            //UnityDB.ResourceManager.LoadAssetAsync(resource.Guid, true);
          }
        }
      }

      GUI.color = Color.white;

      _scrollPos = scrollView.scrollPosition;
    }

    if (!Application.isPlaying) {
      UnityDB.Update();
    }
  }

  public void OnInspectorUpdate() {
    this.Repaint();
  }

  private static Color Desaturate(Color c, float t) {
    return Color.Lerp(new Color(c.grayscale, c.grayscale, c.grayscale), c, t);
  }
}
