using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace Quantum.Demo {
  public class UIReconnecting : UIScreen<UIReconnecting>, IConnectionCallbacks, IMatchmakingCallbacks {
    #region UIScreen

    public override void OnShowScreen(bool first) {
      UIMain.Client?.AddCallbackTarget(this);
    }

    public override void OnHideScreen(bool first) {
      UIMain.Client?.RemoveCallbackTarget(this);
    }

    #endregion

    #region Unity UI Callbacks

    public void OnDisconnectClicked() {
      UIMain.Client.Disconnect();
    }

    #endregion

    #region IConnectionCallbacks

    public void OnConnected() {
    }

    public void OnConnectedToMaster() {
      var roomName = ReconnectInformation.Instance.Room;

      if (PhotonServerSettings.Instance.CanRejoin) {
        Debug.Log($"Trying to rejoin room '{roomName}");
        if (!UIMain.Client.OpJoinRoom(new EnterRoomParams { RoomName = roomName, RejoinOnly = true })) {
          Debug.LogError("Failed to send rejoin room operation");
          UIMain.Client.Disconnect();
        }
      }
      else {
        Debug.Log($"Trying to join room '{roomName}'");
        if (!UIMain.Client.OpJoinRoom(new EnterRoomParams { RoomName = roomName })) {
          Debug.LogError("Failed to send join room operation");
          UIMain.Client.Disconnect();
        }
      }
    }

    public void OnDisconnected(DisconnectCause cause) {
      Debug.Log($"Disconnected: {cause}");

      // Reconnecting failed, reset everything
      UIMain.Client = null;

      switch (cause) {
        case DisconnectCause.AuthenticationTicketExpired:
        case DisconnectCause.InvalidAuthentication:
          // On any Authentication error we could try to log in over the name server again.
          Debug.Log("Trying new connection");
          HideScreen();
          var appSettings = PhotonServerSettings.CloneAppSettings(PhotonServerSettings.Instance.AppSettings);
          appSettings.AppVersion = ReconnectInformation.Instance.AppVersion;
          appSettings.FixedRegion = ReconnectInformation.Instance.Region;
          UIMain.Client = new QuantumLoadBalancingClient(PhotonServerSettings.Instance.AppSettings.Protocol);
          UIMain.Client.AuthValues = new AuthenticationValues { UserId = ReconnectInformation.Instance.UserId };
          if (!UIMain.Client.ConnectUsingSettings(appSettings)) {
            ReconnectInformation.Reset();
            UIDialog.Show("Reconnecting Failed", cause.ToString(), UIConnect.ShowScreen);
          }
          else {
            UIReconnecting.ShowScreen();
          }

          break;

        case DisconnectCause.DisconnectByClientLogic:
          ReconnectInformation.Reset();
          HideScreen();
          UIConnect.ShowScreen();
          break;

        default:
          ReconnectInformation.Reset();
          UIDialog.Show("Reconnecting Failed", cause.ToString(), () => {
            HideScreen();
            UIConnect.ShowScreen();
          });
          break;
      }
    }

    public void OnRegionListReceived(RegionHandler regionHandler) {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) {
    }

    public void OnCustomAuthenticationFailed(string debugMessage) {
    }

    #endregion

    #region IMatchmakingCallbacks

    public void OnFriendListUpdate(List<FriendInfo> friendList) {
    }

    public void OnCreatedRoom() {
    }

    public void OnCreateRoomFailed(short returnCode, string message) {
    }

    public void OnJoinedRoom() {
      Debug.Log($"Joined or rejoined room '{UIMain.Client.CurrentRoom.Name}' successfully");
      HideScreen();
      UIRoom.ShowScreen();
    }

    public void OnJoinRoomFailed(short returnCode, string message) {
      if (returnCode == ErrorCode.JoinFailedWithRejoinerNotFound) {
        var roomName = ReconnectInformation.Instance.Room;
        Debug.Log($"Trying to join room '{roomName}'");
        if (!UIMain.Client.OpJoinRoom(new EnterRoomParams { RoomName = roomName})) {
          Debug.LogError("Failed to send join room operation");
          UIMain.Client.Disconnect();
        }

        return;
      }

      Debug.LogError($"Joining or rejoining room failed with error '{returnCode}': {message}");
      UIDialog.Show("Joining Room Failed", message, () => UIMain.Client.Disconnect());
    }

    public void OnJoinRandomFailed(short returnCode, string message) {
    }

    public void OnLeftRoom() {
    }

    #endregion
  }
}