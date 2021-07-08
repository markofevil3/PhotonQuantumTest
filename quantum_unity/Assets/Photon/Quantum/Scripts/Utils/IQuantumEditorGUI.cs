using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Quantum {
  public interface IQuantumEditorGUI {
#if UNITY_EDITOR
    void DrawProperty(SerializedProperty property, string[] filter = null, bool skipRoot = true);
    void HandleMultiTypeField(SerializedProperty p, params Type[] types);
    IDisposable BoxScope(string headline = null);
#endif
  }
}
