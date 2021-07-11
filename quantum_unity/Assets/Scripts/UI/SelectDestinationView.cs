using Quantum;
using UnityEngine;
using UnityEngine.UI;

public class SelectDestinationView : MonoBehaviour {

  enum State {
    HIDDEN,
    WAITING_FOR_INPUT
  }

  [SerializeField] private GameObject _selectMapNodeButton;
  [SerializeField] private GameObject _wrapperGo;
  [SerializeField] private GridLayoutGroup _buttonGrid;

  private State _state;
  
  void Start() {
    ToggleUI(false, null, null);
  }

  void ToggleUI(bool enable, FixedArray<EntityRef>? mapNodes, Frame f) {
    if (enable) {
      _wrapperGo.SetActive(true);
      _state = State.WAITING_FOR_INPUT;
      SpawnButtons(mapNodes, f);
    } else {
      _wrapperGo.SetActive(false);
      _state = State.HIDDEN;
    }
  }

  private void SpawnButtons(FixedArray<EntityRef>? mapNodes, Frame f) {
    // Should reuse button instead of Destroy
    for (int i = _buttonGrid.transform.childCount - 1; i >= 0; i--) {
      DestroyImmediate(_buttonGrid.transform.GetChild(i).gameObject);
    }
    for (int i = 0; i < mapNodes.Value.Length; i++) {
      if (mapNodes.Value[i] != null) {
        GameObject go = Instantiate(_selectMapNodeButton, _buttonGrid.transform);
        // Send mapNode index in NextNodes[], also send MapNodeSpec for showing node name
        go.GetComponent<SelectDestinationButton>().Init(i, f.Assets.MapNodeSpec(f.Get<MapNode>(mapNodes.Value[i]).Spec));
      }
    }
  }
  
  unsafe void Update() {
    if (QuantumRunner.Default == null ||
        QuantumRunner.Default.Game.Frames.Verified == null) {
      return;
    }

    var f = QuantumRunner.Default.Game.Frames.Verified;

    for (int i = 0; i < f.Global -> Players.Length; i++) {
      BattlePlayer player = f.Global -> Players[i];
      if (QuantumRunner.Default.Game.PlayerIsLocal(player.PlayerRef)) {
        if (player.ReachedNode) {
          if (_state == State.HIDDEN) {
            MapNode mapNode = f.Get<MapNode>(player.TargetMapNode);
            ToggleUI(true, mapNode.NextNodes, f);
          }
        } else {
          if (_state == State.WAITING_FOR_INPUT) {
            ToggleUI(false, null, f);
          }
        }
      }
    }
  }
}