using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Quantum;
using Quantum.Demo;
using TMPro;
using UnityEngine;

public class LobbyScreen : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, IOnEventCallback {

  [SerializeField] private Button _playButton;
  [SerializeField] private TextMeshProUGUI _statusText;
  [SerializeField] private TextMeshProUGUI _playerListText;
  [SerializeField] private TMP_InputField _playerNameInput;

  public void OnClickPlayButton() {
    if (string.IsNullOrEmpty(_playerNameInput.text)) {
      Debug.LogError("Enter your name!!!");
    } else {
      Debug.Log("OnClickPlayButton====");
      MatchMakingManager.Instance.StartMatchMaking(_playerNameInput.text);
      MatchMakingManager.Instance.Client.AddCallbackTarget(this);
    }
  }

  public void OnConnected() {
    _statusText.text = "Connected to server...";
  }

  public void OnConnectedToMaster() {
    _statusText.text = "Connected to master server...";
  }

  public void OnDisconnected(DisconnectCause cause) {
    _statusText.text = $"OnDisconnected...cause: {cause}";
  }

  public void OnRegionListReceived(RegionHandler regionHandler) {
  }

  public void OnCustomAuthenticationResponse(Dictionary<string, object> data) {
  }

  public void OnCustomAuthenticationFailed(string debugMessage) {
  }

  public void OnFriendListUpdate(List<FriendInfo> friendList) {
  }

  public void OnCreatedRoom() {
    _statusText.text = $"OnCreatedRoom...";
  }

  public void OnCreateRoomFailed(short returnCode, string message) {
    _statusText.text = $"OnCreateRoomFailed...returnCode {returnCode} message {message}";
  }

  public void OnJoinedRoom() {
    _statusText.text = $"OnJoinedRoom...";
    UpdatePlayerNames();
  }

  public void OnJoinRoomFailed(short returnCode, string message) {
    _statusText.text = $"OnJoinRoomFailed...returnCode {returnCode} message {message}";
  }

  public void OnJoinRandomFailed(short returnCode, string message) {
    _statusText.text = $"OnJoinRandomFailed...returnCode {returnCode} message {message}";
  }

  public void OnLeftRoom() {
    _statusText.text = $"OnLeftRoom...";
  }

  public void OnPlayerEnteredRoom(Player newPlayer) {
    UpdatePlayerNames();
  }
  public void OnPlayerLeftRoom(Player otherPlayer) {
    UpdatePlayerNames();
  }
  
  private void UpdatePlayerNames() {
    string playerNames = "";
    foreach (var VARIABLE in MatchMakingManager.Instance.Client.CurrentRoom.Players) {
      playerNames += $"{VARIABLE.Value.NickName}, ";
    }

    _playerListText.text = playerNames;
  }

  public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
  }

  public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {
  }

  public void OnMasterClientSwitched(Player newMasterClient) {
  }

  public void OnEvent(EventData photonEvent) {
    switch (photonEvent.Code) {
      case (byte)UIMain.PhotonEventCode.GameStartCountDown:
        _statusText.text = $"Starting in {photonEvent.Parameters[(byte)ParameterCode.Data]}";
        break;
    }
  }
}