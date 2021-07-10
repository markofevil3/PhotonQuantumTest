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

  void Start() {
    var f = QuantumRunner.Default.Game.Frames.Verified;
    var player = f.Get<PlayerLink>(_entityView.EntityRef).PlayerRef;

    _playerName.text = f.GetPlayerData(player).PlayerName;

    if (QuantumRunner.Default.Game.PlayerIsLocal(player) == false)
    {
      _playerName.color = _enemyColor;
    } else {
      _playerName.color = _myColor;
      VirtualCameraManager.Instance.SetFollowTarget(transform);
    }
    _characterAnimator.SetBool("IsGrounded", true);
  }

  void Update() {
    var f = QuantumRunner.Default.Game.Frames.Verified;
    _characterAnimator.SetFloat("VelocityX", 1f);
  }
}