#if UNITY_2018_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace Quantum.Editor {

  public class QuantumSettingsProvider {

    [SettingsProvider]
    public static SettingsProvider CreateEditorSettings() {
      return CreateAssetSettingsProvider<QuantumEditorSettings>("Quantum/Editor Settings");
    }

    [SettingsProvider]
    public static SettingsProvider CreateDeterministicConfig() {
      return CreateAssetSettingsProvider<DeterministicSessionConfigAsset>("Quantum/Deterministic Config");
    }

    private static readonly MultiAssetSettingsProvider.PopupData ServerSettingsPopupData        = new MultiAssetSettingsProvider.PopupData(typeof(PhotonServerSettings));
    private static readonly MultiAssetSettingsProvider.PopupData SimulationConfigPopupData      = new MultiAssetSettingsProvider.PopupData(typeof(SimulationConfigAsset));
    private static readonly MultiAssetSettingsProvider.PopupData PhysicsMaterialPopupData       = new MultiAssetSettingsProvider.PopupData(typeof(PhysicsMaterialAsset));
    private static readonly MultiAssetSettingsProvider.PopupData NavMeshAgentConfigPopupData    = new MultiAssetSettingsProvider.PopupData(typeof(NavMeshAgentConfigAsset));
    private static readonly MultiAssetSettingsProvider.PopupData CharacterController2DPopupData = new MultiAssetSettingsProvider.PopupData(typeof(CharacterController2DConfigAsset));
    private static readonly MultiAssetSettingsProvider.PopupData CharacterController3DPopupData = new MultiAssetSettingsProvider.PopupData(typeof(CharacterController3DConfigAsset));

    [SettingsProvider]
    public static SettingsProvider CreatePhotonServerSettings() {
      return CreateMultiAssetSettingsProvider<PhotonServerSettings>("Quantum/Photon Server Settings", ServerSettingsPopupData);
    }

    [SettingsProvider]
    public static SettingsProvider CreateSimulationConfig() {
      return CreateMultiAssetSettingsProvider<SimulationConfigAsset>("Quantum/Simulation Config", SimulationConfigPopupData);
    }

    [SettingsProvider]
    public static SettingsProvider CreatePhysicsMaterials() {
      return CreateMultiAssetSettingsProvider<PhysicsMaterialAsset>("Quantum/Physics Materials", PhysicsMaterialPopupData);
    }

    [SettingsProvider]
    public static SettingsProvider CreateNavMeshAgentConfigs() {
      return CreateMultiAssetSettingsProvider<NavMeshAgentConfigAsset>("Quantum/Nav Mesh Agents", NavMeshAgentConfigPopupData);
    }

    [SettingsProvider]
    public static SettingsProvider CreateCharacterController2D() {
      return CreateMultiAssetSettingsProvider<CharacterController2DConfigAsset>("Quantum/Character Controller 2D", CharacterController2DPopupData);
    }

    [SettingsProvider]
    public static SettingsProvider CreateCharacterController3D() {
      return CreateMultiAssetSettingsProvider<CharacterController3DConfigAsset>("Quantum/Character Controller 3D", CharacterController3DPopupData);
    }

    private static SettingsProvider CreateAssetSettingsProvider<T>(string settingsWindowPath) where T : ScriptableObject {
      var assets = SearchAndLoadAsset<T>();
      if (assets.Count > 0) {
        var asset    = SearchAndLoadAsset<T>()[0];
        var provider = AssetSettingsProvider.CreateProviderFromObject(settingsWindowPath, asset);
        provider.keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(new SerializedObject(asset));
        return provider;
      }

      return null;
    }

    private static SettingsProvider CreateMultiAssetSettingsProvider<T>(string settingsWindowPath, MultiAssetSettingsProvider.PopupData popupData) where T : ScriptableObject {
      var assets = SearchAndLoadAsset<T>();
      if (assets.Count > 0) {
        return new MultiAssetSettingsProvider(
          settingsWindowPath,
          () => MultiAssetSettingsProvider.CreateEditor(popupData),
          popupData,
          SettingsProvider.GetSearchKeywordsFromSerializedObject(new SerializedObject(assets[0])));
      }

      return null;
    }

    public static List<UnityEngine.Object> SearchAndLoadAsset<T>() where T : ScriptableObject {
      return SearchAndLoadAsset(typeof(T));
    }

    public static List<UnityEngine.Object> SearchAndLoadAsset(Type t) {
      string[] guids = AssetDatabase.FindAssets("t:" + t.Name, null);

      var selectedObjects = new List<UnityEngine.Object>();
      for (int i = 0; i < guids.Length; i++) {
        selectedObjects.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), t));
      }

      return selectedObjects;
    }
  }

  public class MultiAssetSettingsProvider : AssetSettingsProvider {
    public class PopupData {
      public string[] OptionsDisplay;
      public string[] OptionsPath;
      public int      CurrentIndex;
      public Type     AssetType;

      public PopupData(Type assetType) {
        AssetType = assetType;
      }
    }

    public static UnityEditor.Editor CreateEditor(PopupData popupData) {
      if (popupData.OptionsPath.Length == 0) {
        return null;
      }

      return CreateEditorFromAssetPath(popupData.OptionsPath[popupData.CurrentIndex]);
    }

    private static UnityEditor.Editor CreateEditorFromAssetPath(string assetPath) {
      UnityEngine.Object[] targetObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
      if (targetObjects != null)
        return UnityEditor.Editor.CreateEditor(targetObjects);
      return null;
    }

    private readonly PopupData _popupData;

    public MultiAssetSettingsProvider(string settingsWindowPath, Func<UnityEditor.Editor> editorCreator, PopupData popupData, IEnumerable<string> keywords) :
      base(settingsWindowPath, editorCreator, keywords) {
      _popupData = popupData;
    }

    public override void OnActivate(string searchContext, VisualElement rootElement) {
      var assets = QuantumSettingsProvider.SearchAndLoadAsset(_popupData.AssetType);
      _popupData.OptionsPath    = assets.Select(a => AssetDatabase.GetAssetPath(a)).ToArray();
      _popupData.OptionsDisplay = _popupData.OptionsPath.Select(o => o.Replace("/", " \u2215 ")).ToArray();
      _popupData.CurrentIndex   = Mathf.Min(_popupData.CurrentIndex, Mathf.Max(0, _popupData.OptionsPath.Length - 1));

      base.OnActivate(searchContext, rootElement);
    }

    public override void OnTitleBarGUI() {
      try {
        // Only because there are some nasty exceptions when an assets is deleted while being selected.
        base.OnTitleBarGUI();
      }
      catch (Exception e) {
        Debug.Log(e);
        Reload();
      }
    }

    public override void OnGUI(string searchContext) {
      if (string.IsNullOrEmpty(searchContext) && _popupData.OptionsDisplay.Length > 1) {
        using (new EditorGUI.IndentLevelScope()) {
          var newIndex = EditorGUILayout.Popup("Chose Asset:", _popupData.CurrentIndex, _popupData.OptionsDisplay);
          if (newIndex != _popupData.CurrentIndex) {
            _popupData.CurrentIndex = newIndex;
            Reload(searchContext);
          }
        }
      }

      base.OnGUI(searchContext);
    }

    private void Reload(string searchContext = null) {
      // Give me access to settingsEditor or creatorFunc and I won't have to do this:
      OnDeactivate();
      OnActivate(searchContext, null);
    }
  }
}
#endif