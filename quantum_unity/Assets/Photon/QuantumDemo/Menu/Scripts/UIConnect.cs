using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UI = UnityEngine.UI;

namespace Quantum.Demo {
  public class UIConnect : UIScreen<UIConnect> {
    public PhotonRegions     SelectableRegions;
    public PhotonAppVersions SelectableAppVersion;

    public UI.Dropdown   RegionDropdown;
    public UI.Dropdown   AppVersionDropdown;
    public UI.InputField Username;
    public UI.Button     ReconnectButton;

    private static string LastSelectedRegion {
      get => PlayerPrefs.GetString("Quantum.Demo.UIConnect.LastSelectedRegion", PhotonServerSettings.Instance.AppSettings.FixedRegion);
      set => PlayerPrefs.SetString("Quantum.Demo.UIConnect.LastSelectedRegion", value);
    }

    private static string LastUsername {
      get => PlayerPrefs.GetString("Quantum.Demo.UIConnect.LastUsername", Guid.NewGuid().ToString());
      set => PlayerPrefs.SetString("Quantum.Demo.UIConnect.LastUsername", value);
    }

    private static int LastSelectedAppVersion {
      get => PlayerPrefs.GetInt("Quantum.Demo.UIConnect.LastSelectedAppVersion");
      set => PlayerPrefs.SetInt("Quantum.Demo.UIConnect.LastSelectedAppVersion", value);
    }

    protected new void Awake() {
      base.Awake();

      Username.text = LastUsername;

      // Find best initial value for the region select
      var selectedOption          = 0;
      var lastSelectedRegionToken = LastSelectedRegion;
      if (string.IsNullOrEmpty(lastSelectedRegionToken)) {
        lastSelectedRegionToken = PhotonServerSettings.Instance.AppSettings.FixedRegion;
      }

      // Create region options
      var options = new List<UI.Dropdown.OptionData>();
      options.Add(new UI.Dropdown.OptionData("Best Region"));
      if (SelectableRegions) {
        foreach (var photonRegion in SelectableRegions.Regions) {
          options.Add(new UI.Dropdown.OptionData(photonRegion.Name));
          if (photonRegion.Token == lastSelectedRegionToken) {
            selectedOption = options.Count - 1;
          }
        }
      }
      else {
        options.Add(new UI.Dropdown.OptionData(PhotonServerSettings.Instance.AppSettings.FixedRegion));
        if (lastSelectedRegionToken == PhotonServerSettings.Instance.AppSettings.FixedRegion) {
          selectedOption = options.Count - 1;
        }
      }

      RegionDropdown.AddOptions(options);
      RegionDropdown.value = selectedOption;
      RegionDropdown.transform.parent.gameObject.SetActive(string.IsNullOrEmpty(PhotonServerSettings.Instance.AppSettings.Server));

      // Create version dropdown options
      options = new List<UI.Dropdown.OptionData>();
      options.Add(new UI.Dropdown.OptionData("Use Private AppVersion (recommended)"));
      options.Add(new UI.Dropdown.OptionData($"Use Photon AppVersion: '{PhotonServerSettings.Instance.AppSettings.AppVersion}'"));
      if (SelectableAppVersion) {
        foreach (var customVersion in SelectableAppVersion.CustomVersions) {
          options.Add(new UI.Dropdown.OptionData($"'{customVersion}'"));
        }
      }

      AppVersionDropdown.AddOptions(options);
      AppVersionDropdown.value = LastSelectedAppVersion;
    }

    public override void OnShowScreen(bool first) {
      base.OnShowScreen(first);

      ReconnectButton.interactable = UIMain.Client != null || ReconnectInformation.Instance.IsValid;
    }

    public void OnAppVersionHelpButtonClicked() {
      UIDialog.Show("AppVersion", "The AppVersion (string) separates clients connected to the cloud into different groups. This is important to maintain simultaneous different live version and the matchmaking.\n\nChoosing 'Private' in the demo menu f.e. will only allow players to find each other when they are using the exact same build.");
    }

    public void OnConnectClicked() {
      if (String.IsNullOrEmpty(Username.text.Trim())) {
        UIDialog.Show("Error", "User name not set.");
        return;
      }

      var appSettings = PhotonServerSettings.CloneAppSettings(PhotonServerSettings.Instance.AppSettings);

      LastUsername = Username.text;
      Debug.Log($"Using user name '{Username.text}'");

      UIMain.Client = new QuantumLoadBalancingClient(PhotonServerSettings.Instance.AppSettings.Protocol);

      // Overwrite region
      if (string.IsNullOrEmpty(appSettings.Server) == false) {
        // direct connect will not set a region
        appSettings.FixedRegion = string.Empty;
      }
      else {
        if (string.IsNullOrEmpty(appSettings.AppIdRealtime.Trim())) {
          UIDialog.Show("Error", "AppId not set.\n\nSearch or create PhotonServerSettings and configure an AppId.");
          return;
        }

        if (RegionDropdown.value == 0) {
          appSettings.FixedRegion = string.Empty;
          LastSelectedRegion = "best";
        }
        else if (RegionDropdown.value > 0 && SelectableRegions != null && RegionDropdown.value < SelectableRegions.Regions.Count + 1) {
          appSettings.FixedRegion = SelectableRegions.Regions[RegionDropdown.value - 1].Token;
          LastSelectedRegion = appSettings.FixedRegion;
        }

        Debug.Log($"Using region '{LastSelectedRegion}'");
      }

      // Overwrite app version
      switch (AppVersionDropdown.value) {
        case (int)PhotonAppVersions.Type.UsePrivateAppVersion:
          // Use the guid created only for this build
          if (SelectableAppVersion) {
            var privateValue = SelectableAppVersion.Private;
            if (!string.IsNullOrEmpty(privateValue)) {
              appSettings.AppVersion += $" {privateValue}";
            }
          }
          break;

        case (int)PhotonAppVersions.Type.UsePhotonAppVersion:
          // Keep the original version
          break;

        default:
          // Set a pre-defined app version to find play groups.
          var appVersionIndex = AppVersionDropdown.value - (int)PhotonAppVersions.Type.Custom;
          if (SelectableAppVersion && appVersionIndex < SelectableAppVersion.CustomVersions.Count) {
            appSettings.AppVersion += SelectableAppVersion.CustomVersions[appVersionIndex];
          }
          else {
            appSettings.AppVersion += $" Custom {appVersionIndex:00}";
          }
          break;
      }

      LastSelectedAppVersion = AppVersionDropdown.value;
      Debug.Log($"Using app version '{appSettings.AppVersion}'");

      if (UIMain.Client.ConnectUsingSettings(appSettings, Username.text)) {
        HideScreen();
        UIConnecting.ShowScreen();
      }
      else {
        Debug.LogError($"Failed to connect with app settings: '{appSettings.ToStringFull()}'");
      }
    }

    public void OnReconnectClicked() {
      if (UIMain.Client != null && PhotonServerSettings.Instance.CanRejoin && UIMain.Client.ReconnectAndRejoin()) {
        // ReconnectAndRejoin can lead to OnJoinRoomFailed or OnJoinedRoom in
        Debug.Log($"Reconnecting and rejoining");
        HideScreen();
        UIReconnecting.ShowScreen();
        return;
      }

      if (UIMain.Client == null) {
        // Recreate necessary information to connect to the master server to rejoin manually
        UIMain.Client = new QuantumLoadBalancingClient(PhotonServerSettings.Instance.AppSettings.Protocol) {
          AppVersion = ReconnectInformation.Instance.AppVersion,
          MasterServerAddress = ReconnectInformation.Instance.MasterServerAddress,
          AuthValues = new AuthenticationValues {UserId = ReconnectInformation.Instance.UserId, Token = ReconnectInformation.Instance.AuthToken}
        };
      }

      if (UIMain.Client.ConnectToMasterServer()) {
        Debug.Log($"Reconnecting to master sever");
        HideScreen();
        UIReconnecting.ShowScreen();
        return;
      }

      Debug.LogError($"Cannot reconnect");
      ReconnectInformation.Reset();
      ReconnectButton.interactable = false;
    }
  }
}