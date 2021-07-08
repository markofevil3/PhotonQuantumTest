using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Quantum;

public class QuantumFrameDifferWindow : EditorWindow {
  private class StaticFrameStateStorage : ScriptableObject {
    public QuantumFrameDifferGUI.FrameDifferState State = new QuantumFrameDifferGUI.FrameDifferState();
  }

  private static StaticFrameStateStorage _stateStorage;

  private static StaticFrameStateStorage Storage {
    get {
      if ( !_stateStorage ) {
        _stateStorage = FindObjectOfType<StaticFrameStateStorage>();
        if ( !_stateStorage ) {
          _stateStorage = ScriptableObject.CreateInstance<StaticFrameStateStorage>();
        }
      }
      return _stateStorage;
    }
  }

  [InitializeOnLoadMethod]
  static void Initialize() {
    QuantumCallback.SubscribeManual((CallbackChecksumErrorFrameDump callback) => {
      Storage.State.AddEntry(QuantumRunner.FindRunner(callback.Game).Id, callback.ActorId, callback.FrameNumber, callback.FrameDump);
      ShowWindow();
    });
  }

  class QuantumFrameDifferGUIEditor : QuantumFrameDifferGUI {
    QuantumFrameDifferWindow _window;

    public override Rect Position {
      get { return _window.position; }
    }

    public override GUIStyle MiniButton {
      get { return EditorStyles.miniButton; }
    }

    public override GUIStyle MiniButtonLeft {
      get { return EditorStyles.miniButtonLeft; }
    }

    public override GUIStyle MiniButtonRight {
      get { return EditorStyles.miniButtonRight; }
    }

    public override GUIStyle BoldLabel {
      get { return EditorStyles.boldLabel; }
    }

    public override GUIStyle DiffHeaderError {
      get { return (GUIStyle)"flow node 6"; }
    }

    public override GUIStyle DiffHeader {
      get { return (GUIStyle)"flow node 1"; }
    }

    public override GUIStyle DiffBackground {
      get { return (GUIStyle)"CurveEditorBackground"; }
    }

    public override GUIStyle DiffLineOverlay {
      get { return (GUIStyle)"ProfilerTimelineBar"; }
    }

    public override bool IsEditor {
      get { return true; }
    }

    public override GUIStyle TextLabel {
      get { return EditorStyles.label; }
    }

    public QuantumFrameDifferGUIEditor(QuantumFrameDifferWindow window, FrameDifferState state) : base(state) {
      _window = window;
    }

    public override void Repaint() {
      _window.Repaint();
    }

    public override void DrawHeader() {
      bool wasEnabled = GUI.enabled;
      GUI.enabled = State.RunnerIds.Any();
      if (GUILayout.Button("Save", MiniButton, GUILayout.Height(16))) {
        var savePath = UnityEditor.EditorUtility.SaveFilePanel("Save", "", "frameDiff", "json");
        if (!string.IsNullOrEmpty(savePath)) {
          File.WriteAllText(savePath, JsonUtility.ToJson(State));
        }
      }
      GUI.enabled = wasEnabled;

      if (GUILayout.Button("Load", MiniButton, GUILayout.Height(16))) {
        var loadPath = UnityEditor.EditorUtility.OpenFilePanel("Load", "", "json");
        if (!string.IsNullOrEmpty(loadPath)) {
          JsonUtility.FromJsonOverwrite(File.ReadAllText(loadPath), State);
        }
      }
    }
  }

  [MenuItem("Window/Quantum/Frame Differ")]
  [MenuItem("Quantum/Show Frame Differ", false, 42)]
  public static void ShowWindow() {
    GetWindow(typeof(QuantumFrameDifferWindow));
  }

  QuantumFrameDifferGUIEditor _gui;

  void OnGUI() {
    titleContent = new GUIContent("Frame Differ");

    if(_gui == null) {
      _gui = new QuantumFrameDifferGUIEditor(this, Storage.State);
    }

    _gui.OnGUI();
  }
}
