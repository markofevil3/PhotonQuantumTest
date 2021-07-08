using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Quantum.Editor {

  [CustomEditor(typeof(EntityComponentBase), true)]
  public class EntityComponentBaseEditor : UnityEditor.Editor {

    private delegate void BoolSetterDelegate(UnityEditor.Editor editor, bool value);
    private static readonly Lazy<BoolSetterDelegate> InternalSetHidden = new Lazy<BoolSetterDelegate>(() => ReflectionUtils.CreateEditorMethodDelegate<BoolSetterDelegate>("UnityEditor.Editor", "InternalSetHidden", BindingFlags.NonPublic | BindingFlags.Instance));

    void OnEnable() {
      if (!QuantumEditorSettings.Instance.UseQuantumAssetInspector) {
        return;
      }

      if (QuantumEditorSettings.Instance.EntityComponentInspectorMode == QuantumEntityComponentInspectorMode.InlineInEntityPrototypeAndHideMonoBehaviours) {
        InternalSetHidden.Value(this, true);
      }
    }


    public override void OnInspectorGUI() {
      if (!QuantumEditorSettings.Instance.UseQuantumAssetInspector) {
        base.OnInspectorGUI();
        return;
      }

      
      CustomEditorsHelper.DrawScript(target);


      if (QuantumEditorSettings.Instance.EntityComponentInspectorMode != QuantumEntityComponentInspectorMode.ShowMonoBehaviours) {
        bool comparisonPopup = false;
        var trace = new StackFrame(1);
        if (trace?.GetMethod()?.DeclaringType.Name.EndsWith("ComparisonViewPopup") == true) {
          comparisonPopup = true;
        }
        if (!comparisonPopup)
          return;
      }

      try {
        EditorGUI.BeginChangeCheck();
        using (new EditorGUI.DisabledScope(Application.isPlaying)) {
#pragma warning disable CS0618 // Type or member is obsolete
          DrawFields(serializedObject);
#pragma warning restore CS0618 // Type or member is obsolete
          ((EntityComponentBase)target).OnInspectorGUI(serializedObject, CustomEditorsHelper.EditorGUIProxy);
        }
      } finally {
        if (EditorGUI.EndChangeCheck()) {
          serializedObject.ApplyModifiedProperties();
        }
      }
    }

    [Obsolete("Use EntityComponentBase.OnInspectorGUI instead")]
    protected virtual void DrawFields(SerializedObject so) {
    }
  }
}

