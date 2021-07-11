using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using Quantum;
using TMPro;
using UnityEngine;

public class CharacterView : MonoBehaviour {
  [SerializeField] private TextMeshPro _playerName;
  [SerializeField] private EntityView _entityView;
  [SerializeField] private Animator _characterAnimator;

  private Player _player;
  private Color _enemyColor = Color.red;
  private Color _myColor = Color.blue;
  private PlayerRef _thisPlayer;

  void Start() {
    var f = QuantumRunner.Default.Game.Frames.Verified;
    _thisPlayer = f.Get<PlayerLink>(_entityView.EntityRef).PlayerRef;

    _playerName.text = f.GetPlayerData(_thisPlayer).PlayerName;

    if (QuantumRunner.Default.Game.PlayerIsLocal(_thisPlayer) == false)
    {
      _playerName.color = _enemyColor;
    } else {
      _playerName.color = _myColor;
      VirtualCameraManager.Instance.SetFollowTarget(transform);
    }
    _characterAnimator.SetBool("IsGrounded", true);
  }

  unsafe void Update() {
    if (QuantumRunner.Default == null ||
        QuantumRunner.Default.Game.Frames.Verified == null) {
      return;
    }

    var f = QuantumRunner.Default.Game.Frames.Verified;

    for (int i = 0; i < f.Global -> Players.Length; i++) {
      BattlePlayer player = f.Global -> Players[i];
      if (player.PlayerRef == _thisPlayer) {
        if (player.ReachedNode) {
          _characterAnimator.SetFloat("VelocityX", 0);
        } else {
          _characterAnimator.SetFloat("VelocityX", 1f);
        }
      }
    }
  }
}