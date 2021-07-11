using System.Collections;
using System.Collections.Generic;
using Quantum;
using TMPro;
using UnityEngine;

public class SelectDestinationButton : MonoBehaviour {
  [SerializeField] private TextMeshProUGUI _destinationText;

  private int _nodeIndex;
  
  public void Init(int index, MapNodeSpec mapNodeSpec) {
    _nodeIndex = index;
    _destinationText.text = mapNodeSpec.Name;
  }
  
  public void OnClick() {
    ChooseMapNodeCommand command = new ChooseMapNodeCommand();
    command.NextNodeIndex = _nodeIndex;
    Debug.Log("OnClick=== " + command.NextNodeIndex);
    QuantumRunner.Default.Game.SendCommand(command);
  }
}