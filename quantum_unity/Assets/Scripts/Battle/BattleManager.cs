using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class BattleManager : MonoBehaviour, IConnectionCallbacks {
  [SerializeField] private CharacterView[] _characterViews;

  void Start() {
    MatchMakingManager.Instance.Client.AddCallbackTarget(this);
    
    // TESTCODE: assign player to character
    int count = 0;
    foreach (var player in MatchMakingManager.Instance.Client.CurrentRoom.Players) {
      _characterViews[count].Init(player.Value);
      count++;
    }
  }

  public void OnConnected() {
    throw new System.NotImplementedException();
  }

  public void OnConnectedToMaster() {
    throw new System.NotImplementedException();
  }

  public void OnDisconnected(DisconnectCause cause) {
    Debug.LogError($"Disconnected: {cause}");
  }

  public void OnRegionListReceived(RegionHandler regionHandler) {
    throw new System.NotImplementedException();
  }

  public void OnCustomAuthenticationResponse(Dictionary<string, object> data) {
    throw new System.NotImplementedException();
  }

  public void OnCustomAuthenticationFailed(string debugMessage) {
    throw new System.NotImplementedException();
  }
}