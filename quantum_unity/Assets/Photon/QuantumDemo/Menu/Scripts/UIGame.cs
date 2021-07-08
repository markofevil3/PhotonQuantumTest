using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace Quantum.Demo {
  public class UIGame : UIScreen<UIGame>, IConnectionCallbacks {
    public GameObject UICamera;
    public List<GameObject> MenuObjects;

    public override void OnShowScreen(bool first) {
      UICamera.Hide();

      foreach (var menuObject in MenuObjects) {
        menuObject.Hide();
      }

      UIMain.Client?.AddCallbackTarget(this);
    }

    public override void OnHideScreen(bool first) {
      UIMain.Client?.RemoveCallbackTarget(this);

      UICamera.Show();

      foreach (var menuObject in MenuObjects) {
        menuObject.Show();
      }
    }

    public void OnLeaveClicked() {
      UIMain.Client.Disconnect();
    }

    public void OnConnected() {
    }

    public void OnConnectedToMaster() {
    }

    public void OnDisconnected(DisconnectCause cause) {
      Debug.Log($"Disconnected: {cause}");

      QuantumRunner.ShutdownAll(true);

      HideScreen();
      UIConnect.ShowScreen();
    }

    public void OnRegionListReceived(RegionHandler regionHandler) {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) {
    }

    public void OnCustomAuthenticationFailed(string debugMessage) {
    }
  }
}