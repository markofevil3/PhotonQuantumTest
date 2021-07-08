using Quantum;
using TMPro;
using UnityEngine;

public class LobbyScreen : MonoBehaviour{

  [SerializeField] private Button _playButton;
  [SerializeField] private TMP_InputField _playerNameInput;

  public void OnClickPlayButton() {
    if (string.IsNullOrEmpty(_playerNameInput.text)) {
      Debug.LogError("Enter your name!!!");
    } else {
      Debug.Log("OnClickPlayButton====");
      MatchMakingManager.Instance.StartMatchMaking(_playerNameInput.text);
    }
  }
}