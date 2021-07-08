using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class CharacterView : MonoBehaviour {
  [SerializeField] private TextMeshPro _playerName;

  private Player _player;
  
  public void Init(Player player) {
    _player = player;
    _playerName.text = _player.NickName;
  }
}