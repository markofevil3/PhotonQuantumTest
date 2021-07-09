using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using Quantum;
using Quantum.Demo;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MatchMakingManager : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, IOnEventCallback  {
  
  [SerializeField] private RuntimeConfigContainer _runtimeConfigContainer;

  public QuantumLoadBalancingClient Client;
  private EnterRoomParams _enterRoomParams;

  public static MatchMakingManager Instance;
  
  public enum PhotonEventCode : byte {
    StartGame = 110
  }
  
  private void Awake() {
    DontDestroyOnLoad(this);
    Instance = this;
  }

  public void StartMatchMaking(string playerName) {
    Client = new QuantumLoadBalancingClient(PhotonServerSettings.Instance.AppSettings.Protocol);
    Client.AddCallbackTarget(this);
    Client.ConnectUsingSettings(PhotonServerSettings.Instance.AppSettings, playerName);
  }
  
  void Update() {
    Client?.Service();
  }
  
  public void OnConnected() {
    Debug.Log("==OnConnected==");
  }

  public void OnConnectedToMaster() {
    Debug.Log($"==OnConnectedToMaster==UserId: {Client.UserId} region '{Client.CloudRegion}");
    // Try to join or create new room
    var joinRandomParams = new OpJoinRandomRoomParams();
    _enterRoomParams = new EnterRoomParams();
    _enterRoomParams.RoomOptions = new RoomOptions();
    _enterRoomParams.RoomOptions.IsVisible  = true;
    _enterRoomParams.RoomOptions.MaxPlayers = 20;
    _enterRoomParams.RoomOptions.Plugins    = new string[] { "QuantumPlugin" };
    _enterRoomParams.RoomOptions.PlayerTtl = PhotonServerSettings.Instance.PlayerTtlInSeconds * 1000;
    
    if (!Client.OpJoinRandomOrCreateRoom(joinRandomParams, _enterRoomParams)) {
      Client.Disconnect();
      Debug.LogError($"Failed to send join random operation");
    }
  }

  public void OnDisconnected(DisconnectCause cause) {
    Debug.Log($"==OnDisconnected==cause {cause}");
  }

  public void OnRegionListReceived(RegionHandler regionHandler) {
    Debug.Log($"==OnRegionListReceived==" + regionHandler.BestRegion);
  }

  public void OnCustomAuthenticationResponse(Dictionary<string, object> data) {
    Debug.Log($"==OnCustomAuthenticationResponse==");
  }

  public void OnCustomAuthenticationFailed(string debugMessage) {
    Debug.Log($"==OnCustomAuthenticationFailed=={debugMessage}");
  }

  public void OnFriendListUpdate(List<FriendInfo> friendList) {
    Debug.Log($"==OnFriendListUpdate==");
  }

  public void OnCreatedRoom() {
    Debug.Log($"==OnCreatedRoom==");
  }

  public void OnCreateRoomFailed(short returnCode, string message) {
    Debug.Log($"==OnCreateRoomFailed==returnCode {returnCode} message {message}");
  }

  public void OnJoinedRoom() {
    Debug.Log($"==OnJoinedRoom==");
  }

  public void OnJoinRoomFailed(short returnCode, string message) {
    Debug.Log($"==OnJoinRoomFailed==returnCode {returnCode} message {message}");
  }

  public void OnJoinRandomFailed(short returnCode, string message) {
    Debug.Log($"==OnJoinRandomFailed==returnCode {returnCode} message {message}");
    if (returnCode == ErrorCode.NoRandomMatchFound) {
      // Try to create a room
      if (!Client.OpCreateRoom(_enterRoomParams)) {
        Debug.LogError("Failed to send join or create room operation");
        Client?.Disconnect();
      }
    }
    else {
      Debug.LogError($"Join random failed returnCode {returnCode} message {message}");
      Client?.Disconnect();
    }
  }

  public void OnLeftRoom() {
    Debug.Log($"==OnLeftRoom==");
  }

  public void OnPlayerEnteredRoom(Player newPlayer) {
    Debug.Log($"==OnPlayerEnteredRoom==newPlayer {newPlayer.NickName} PlayerCount {Client.CurrentRoom.PlayerCount}");
    if (Client.CurrentRoom.PlayerCount >= 2) {
      // Room master will send start game message to everyone
      if (Client.LocalPlayer.IsMasterClient) {
        bool success = Client.OpRaiseEvent((byte)UIMain.PhotonEventCode.StartGame, null,
                                           new RaiseEventOptions {Receivers = ReceiverGroup.All}, SendOptions.SendReliable);
        if (!success) {
          Debug.LogError($"Failed to send start game event");
        }
      }
    }
  }

  private void StartGame() {
    // SceneManager.LoadScene("BattleScene");
    var config = _runtimeConfigContainer != null ? RuntimeConfig.FromByteArray(RuntimeConfig.ToByteArray(_runtimeConfigContainer.Config)) : new RuntimeConfig();
    
    var param = new QuantumRunner.StartParameters {
      RuntimeConfig       = config,
      DeterministicConfig = DeterministicSessionConfigAsset.Instance.Config,
      ReplayProvider      = null,
      GameMode            = Photon.Deterministic.DeterministicGameMode.Multiplayer,
      InitialFrame        = 0,
      PlayerCount         = Client.CurrentRoom.MaxPlayers,
      LocalPlayerCount    = 1,
      RecordingFlags      = RecordingFlags.None,
      NetworkClient       = Client
    };

    Debug.Log($"Starting QuantumRunner with map guid '{config.Map.Id}'");

    // Joining with the same client id will result in the same quantum player slot which is important for reconnecting.
    var clientId = ClientIdProvider.CreateClientId(ClientIdProvider.Type.NewGuid, Client);
    QuantumRunner.StartGame(clientId, param);
  }

  public void OnPlayerLeftRoom(Player otherPlayer) {
    Debug.Log($"==OnPlayerLeftRoom==newPlayer {otherPlayer.NickName}");
  }

  public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
    Debug.Log($"==OnRoomPropertiesUpdate==propertiesThatChanged {propertiesThatChanged}");
  }

  public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {
    Debug.Log($"==OnPlayerPropertiesUpdate==targetPlayer {targetPlayer.NickName} changedProps {changedProps}");
  }

  public void OnMasterClientSwitched(Player newMasterClient) {
    Debug.Log($"==OnMasterClientSwitched==newMasterClient {newMasterClient.NickName}");
  }

  public void OnEvent(EventData photonEvent) {
    switch (photonEvent.Code) {
      case (byte)UIMain.PhotonEventCode.StartGame:
        StartGame();
        break;
    }
  }
}