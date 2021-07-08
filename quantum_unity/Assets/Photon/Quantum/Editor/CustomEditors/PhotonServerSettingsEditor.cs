using System;
using System.Linq;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(PhotonServerSettings), false)]
  public class PhotonServerSettingsEditor : UnityEditor.Editor {
    private static bool HasVoice;
    private static bool HasChat;
    private static bool HasCheckedForPlugins;

    public override void OnInspectorGUI() {
      if (HasCheckedForPlugins) {
        HasCheckedForPlugins = false;
        HasVoice = Type.GetType("Photon.Voice.VoiceClient, PhotonVoice.API") != null;
        HasChat = Type.GetType("Photon.Voice.ChatClient, PhotonChat.API") != null;
        foreach (var assemblyName in QuantumEditorSettings.Instance.SearchAssemblies) {
          HasVoice |= Type.GetType($"Photon.Voice.VoiceClient, {assemblyName}") != null;
          HasChat |= Type.GetType($"Photon.Voice.ChatClient, {assemblyName}") != null;
        }
      }

      var settings = (PhotonServerSettings)target;

      CustomEditorsHelper.DrawScript(target);

      using (new CustomEditorsHelper.BoxScope("Photon Server Settings")) {
        //CustomEditorsHelper.DrawDefaultInspector(serializedObject, new string[] { "AppSettings", "m_Script" });

        using (new CustomEditorsHelper.SectionScope("App Settings")) {
          SerializedProperty serializedProperty = this.serializedObject.FindProperty("AppSettings");
          BuildAppIdField(serializedProperty.FindPropertyRelative("AppIdRealtime"));
          if (HasChat) BuildAppIdField(serializedProperty.FindPropertyRelative("AppIdChat"));
          if (HasVoice) BuildAppIdField(serializedProperty.FindPropertyRelative("AppIdVoice"));

          CustomEditorsHelper.DrawDefaultInspector(serializedObject, "AppSettings", new string[] {"AppSettings.AppIdChat", "AppSettings.AppIdVoice", "AppSettings.AppIdRealtime"}, false);
        }

        using (new CustomEditorsHelper.SectionScope("Custom Settings")) {
          settings.PlayerTtlInSeconds = EditorGUILayout.IntField("PlayerTtl In Seconds", settings.PlayerTtlInSeconds);
        }

        using (new CustomEditorsHelper.SectionScope("Development Utils")) {

          DisplayBestRegionPreference(settings.AppSettings);

          using (new EditorGUILayout.HorizontalScope()) {
            EditorGUILayout.PrefixLabel("Configure App Settings");
            if (GUILayout.Button("Cloud", EditorStyles.miniButton)) {
              SetSettingsToCloud(settings.AppSettings);
              EditorUtility.SetDirty(settings);
            }
            if (GUILayout.Button("Local Master Server", EditorStyles.miniButton)) {
              SetSettingsToLocalServer(settings.AppSettings);
              EditorUtility.SetDirty(settings);
            }
          }
        }
      }
    }

    private void DisplayBestRegionPreference(AppSettings appSettings) {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PrefixLabel(new GUIContent("Best Region Preference", "Clears the Best Region of the editor.\n.Best region is used if Fixed Region is empty."));

      var bestRegionSummaryInPreferences = PlayerPrefs.GetString(QuantumLoadBalancingClient.BestRegionSummaryKey);

      var prefLabel = "n/a";
      if (!string.IsNullOrEmpty(bestRegionSummaryInPreferences)) {
        var regionsPrefsList = bestRegionSummaryInPreferences.Split(';');
        if (regionsPrefsList.Length > 1 && !string.IsNullOrEmpty(regionsPrefsList[0])) {
          prefLabel = $"'{regionsPrefsList[0]}' ping:{regionsPrefsList[1]}ms ";
        }
      }

      GUILayout.Label(prefLabel, GUILayout.ExpandWidth(false));

      if (GUILayout.Button("Reset", EditorStyles.miniButton)) {
        PlayerPrefs.SetString(QuantumLoadBalancingClient.BestRegionSummaryKey, String.Empty);
      }

      if (GUILayout.Button("Edit WhiteList", EditorStyles.miniButton)) {
        Application.OpenURL("https://dashboard.photonengine.com/en-US/App/RegionsWhitelistEdit/" + appSettings.AppIdRealtime);
      }

      EditorGUILayout.EndHorizontal();
    }

    private void BuildAppIdField(SerializedProperty property) {
      EditorGUILayout.BeginHorizontal();
      using (var checkScope = new EditorGUI.ChangeCheckScope()) {
        EditorGUILayout.PropertyField(property);
        if (checkScope.changed) {
          property.serializedObject.ApplyModifiedProperties();
        }
      }
      var appId = property.stringValue;
      var url   = "https://dashboard.photonengine.com/en-US/PublicCloud";
      if (!string.IsNullOrEmpty(appId)) {
        url = $"https://dashboard.photonengine.com/en-US/App/Manage/{appId}";
      }
      if (GUILayout.Button("Dashboard", EditorStyles.miniButton, GUILayout.Width(70))) {
        Application.OpenURL(url);
      }
      EditorGUILayout.EndHorizontal();
    }

    public static void SetSettingsToCloud(AppSettings appSettings) {
      appSettings.Server = string.Empty;
      appSettings.UseNameServer = true;
      appSettings.Port = 0;
    }

    public static void SetSettingsToLocalServer(AppSettings appSettings) {
      try {
        var ip = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
                       .AddressList
                       .First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                       .ToString();

        appSettings.Server        = ip;
        appSettings.UseNameServer = false;
        if (appSettings.Port == 0) {
          appSettings.Port = 5055;
        }
      }
      catch (Exception e) {
        Debug.LogWarning("Cannot set local server address, sorry.");
        Debug.LogException(e);
      }
    }
  }
}

