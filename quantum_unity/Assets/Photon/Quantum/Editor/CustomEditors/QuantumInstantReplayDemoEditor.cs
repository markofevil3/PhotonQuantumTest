using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quantum.Editor {
  [CustomEditor(typeof(QuantumInstantReplayDemo))]
  public class QuantumInstantReplayDemoEditor : UnityEditor.Editor {

    private new QuantumInstantReplayDemo target => (QuantumInstantReplayDemo)base.target;

    public override void OnInspectorGUI() {
      base.DrawDefaultInspector();
      EditorGUILayout.HelpBox($"Use QuantumRunner.StartParameters.InstantReplaySettings to define the maximum replay length and the snapshot sampling rate.", MessageType.Info);
    }

    public override bool RequiresConstantRepaint() {
      return true;
    }
  }
}