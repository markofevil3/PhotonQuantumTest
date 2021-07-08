using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Quantum.Editor {
  public class QuantumShortcuts : EditorWindow {
    public static float ButtonWidth = 200.0f;

    [MenuItem("Window/Quantum/Shortcuts")]
    [MenuItem("Quantum/Show Shortcuts", false, 43)]
    public static void ShowWindow() {
      GetWindow(typeof(QuantumShortcuts), false, "Quantum Shortcuts");
    }

    public class GridScope : IDisposable {
      private bool _endHorizontal;

      public GridScope(int columnCount, ref int currentColumn) {
        if (currentColumn % columnCount == 0) {
          GUILayout.BeginHorizontal();
        }

        _endHorizontal = ++currentColumn % columnCount == 0;
      }

      public void Dispose() {
        if (_endHorizontal) { 
          GUILayout.EndHorizontal();
        }
      }
    }

    public virtual void OnGUI() {
      var buttonWidth = ButtonWidth;
      var columnCount = (int)Mathf.Max(EditorGUIUtility.currentViewWidth / buttonWidth, 1);
      buttonWidth = EditorGUIUtility.currentViewWidth / columnCount;
      var currentColumn = 0;

      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("Settings", buttonWidth), "Simulation Configs", EditorStyles.miniButton)) SearchAndSelect<SimulationConfigAsset>();
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("Grid Icon", buttonWidth), "Deterministic Configs", EditorStyles.miniButton)) SearchAndSelect<DeterministicSessionConfigAsset>();
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("NetworkView Icon", buttonWidth), "Photon Server Settings", EditorStyles.miniButton)) SearchAndSelect<PhotonServerSettings>();
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("BuildSettings.Editor.Small", buttonWidth), "Editor Settings", EditorStyles.miniButton)) SearchAndSelect<QuantumEditorSettings>();
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("PhysicMaterial Icon", buttonWidth), "Physics Materials", EditorStyles.miniButton)) SearchAndSelect<PhysicsMaterialAsset>();
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("NavMeshData Icon", buttonWidth), "NavMesh Agent Configs", EditorStyles.miniButton)) SearchAndSelect<NavMeshAgentConfigAsset>();
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("CapsuleCollider2D Icon", buttonWidth), "Character Controller 2D", EditorStyles.miniButton)) SearchAndSelect<CharacterController2DConfigAsset>();
      }
      using (new GridScope(columnCount, ref currentColumn)) {
        if (GUI.Button(DrawIcon("CapsuleCollider Icon", buttonWidth), "Character Controller 3D", EditorStyles.miniButton)) SearchAndSelect<CharacterController3DConfigAsset>();
      }

    }

    public static Rect DrawIcon(string iconName, float width) {
      var rect = EditorGUILayout.GetControlRect();
      rect.width = 20;
      EditorGUI.LabelField(rect, EditorGUIUtility.IconContent(iconName));
      rect.xMin  += 20;
      rect.width = width - 25;
      return rect;
    }

    public static T SearchAndSelect<T>() where T : UnityEngine.Object {
      var t     = typeof(T);
      var guids = AssetDatabase.FindAssets("t:" + t.Name, null);
      if (guids.Length == 0) {
        Debug.LogFormat("No UnityEngine.Objects of type '{0}' found.", t.Name);
        return null;
      }

      var selectedObjects = new List<UnityEngine.Object>();
      for (int i = 0; i < guids.Length; i++) {
        selectedObjects.Add(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), t));
      }

      Selection.objects = selectedObjects.ToArray();
      return (T)selectedObjects[0];
    }
  }
}
