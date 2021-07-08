using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using Photon.Deterministic;

namespace Quantum.Editor {
  [CustomEditor(typeof(MapNavMeshDefinition))]
  public partial class MapNavMeshDefinitionEditor : UnityEditor.Editor {
    public static bool Editing;
    public List<string> SelectedVertices = new List<string>();

    static bool _navMeshSurfaceTypeSearched;
    static System.Type _navMeshSurfaceType;

    static System.Type NavMeshSurfaceType {
      get {
        // TypeUtils.FindType can be quite slow
        if (_navMeshSurfaceTypeSearched == false) {
          _navMeshSurfaceTypeSearched = true;
          _navMeshSurfaceType = TypeUtils.FindType("NavMeshSurface");
        }

        return _navMeshSurfaceType;
      }
    }

    static string NewId() {
      return System.Guid.NewGuid().ToString();
    }

    public override void OnInspectorGUI() {
      var data = target as MapNavMeshDefinition;

      CustomEditorsHelper.DrawScript(target);
      using (new CustomEditorsHelper.BoxScope("Map NavMesh Definition")) {

        EditorGUILayout.HelpBox("If you are only importing the Unity navmesh switch to MapNavMeshUnity.", MessageType.Info);

        if (data) {
#if UNITY_2018_2_OR_NEWER
#pragma warning disable 612, 618
          if (PrefabUtility.GetCorrespondingObjectFromSource(data) == null && PrefabUtility.GetPrefabObject(data) != null) {
#pragma warning restore 612, 618      
#else
        if (PrefabUtility.GetPrefabParent(data) == null && PrefabUtility.GetPrefabObject(data) != null) {
#endif
            EditorGUILayout.HelpBox("The NavMesh Editor does not work on prefabs.", MessageType.Info);
            EditorGUILayout.Space();
            return;
          }

          if (data.Triangles == null) {
            data.Triangles = new MapNavMeshTriangle[0];
            Save();
          }

          if (data.Vertices == null) {
            data.Vertices = new MapNavMeshVertex[0];
            Save();
          }

          var backgroundColorRect = EditorGUILayout.BeginVertical();

          using (new CustomEditorsHelper.SectionScope("Import Unity NavMesh")) {

            if (Editing) GUI.enabled = false;

            using (new CustomEditorsHelper.BackgroundColorScope(Color.green)) {
              if (GUILayout.Button("Bake Unity Navmesh", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2))) {
                MapNavMeshEditor.BakeUnityNavmesh(data.gameObject);
              }

              if (GUILayout.Button("Import from Unity", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2))) {
                Undo.RegisterCompleteObjectUndo(data, "MapNavMeshDefinitionEditor - ImportFromUnity");
                SelectedVertices.Clear();
                MapNavMeshEditor.UpdateDefaultMinAgentRadius();
                ImportFromUnity(data);

                Save();
              }
            }

            using (var changed = new EditorGUI.ChangeCheckScope()) {
              data.WeldIdenticalVertices = EditorGUILayout.Toggle(new GUIContent("Weld Identical Vertices", "The Unity NavMesh is a collection of non-connected triangles, this option is very important and combines shared vertices."), data.WeldIdenticalVertices);
              if (data.WeldIdenticalVertices) {
                data.WeldVertexEpsilon = EditorGUILayout.FloatField(new GUIContent("Weld Vertices Epsilon", "Don't make the epsilon too small, vertices to fuse are missed, also don't make the value too big as it will deform your navmesh."), data.WeldVertexEpsilon);
                data.WeldVertexEpsilon = Mathf.Max(data.WeldVertexEpsilon, Mathf.Epsilon);
              }

              data.DelaunayTriangulation = EditorGUILayout.Toggle(new GUIContent("Delaunay Triangulation", "Post processes imported Unity navmesh with a Delaunay triangulation to reduce long triangles."), data.DelaunayTriangulation);

              data.FixTrianglesOnEdges = EditorGUILayout.Toggle(new GUIContent("Fix Triangles On Edges", "Sometimes vertices will lie on other triangle edges, this will lead to unwanted borders being detected, this option splits those vertices."), data.FixTrianglesOnEdges);
              if (data.FixTrianglesOnEdges) {
                data.FixTrianglesOnEdgesEpsilon = EditorGUILayout.FloatField(new GUIContent("Fix Triangles On Edges Epsilon", "Larger scaled navmeshes may require to increase this value (e.g. 0.001) when false-positive borders are detected."), data.FixTrianglesOnEdgesEpsilon);
                data.FixTrianglesOnEdgesEpsilon = Mathf.Max(data.FixTrianglesOnEdgesEpsilon, float.Epsilon);
              }

              data.ImportRegions = EditorGUILayout.Toggle(new GUIContent("Import Regions", "Toggle the Quantum region import."), data.ImportRegions);
              if (data.ImportRegions) {
                data.RegionDetectionMargin = EditorGUILayout.FloatField(new GUIContent("Region Detection Margin", "The artificial margin is necessary because the Unity NavMesh does not fit the source size very well. The value is added to the navmesh area and checked against all Quantum Region scripts to select the correct region id."), data.RegionDetectionMargin);
                data.RegionDetectionMargin = Mathf.Max(data.RegionDetectionMargin, 0.0f);
                EditorGUILayout.LabelField(new GUIContent("Convert Unity Areas To Quantum Region:", "Select what Unity NavMesh areas are used to generated Quantum regions. At least one must be selected."));
                EditorGUI.indentLevel++;
                var names = new List<string>(GameObjectUtility.GetNavMeshAreaNames());
                if (data.RegionAreaIds == null) {
                  data.RegionAreaIds = new List<int>();
                }

                for (int i = 0; i < data.RegionAreaIds.Count; i++) {
                  var areaId = data.RegionAreaIds[i];
                  var areaName = GameObjectUtility.GetNavMeshAreaNames().FirstOrDefault(name => GameObjectUtility.GetNavMeshAreaFromName(name) == areaId);
                  if (string.IsNullOrEmpty(areaName)) {
                    areaName = "missing Unity NavMesh area";
                  }

                  if (!EditorGUILayout.Toggle(areaName, true)) {
                    data.RegionAreaIds.Remove(areaId);
                  }
                  else {
                    names.Remove(areaName);
                  }
                }

                if (names.Count > 0) {
                  var newName = EditorGUILayout.Popup("Add New Area", -1, names.ToArray());
                  if (newName >= 0) {
                    var areaId = GameObjectUtility.GetNavMeshAreaFromName(names[newName]);
                    data.RegionAreaIds.Add(areaId);
                  }
                }

                EditorGUI.indentLevel--;
              }

              if (changed.changed) {
                Save();
              }
            }

            if (NavMeshSurfaceType != null) {
              serializedObject.Update();
              EditorGUILayout.PropertyField(serializedObject.FindProperty("NavMeshSurfaces"), true);
              serializedObject.ApplyModifiedProperties();
            }

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ClosestTriangleCalculation"), new GUIContent("Closest Triangle Calculation", "SpiralOut will be faster but fallback triangles can be null."));
            serializedObject.ApplyModifiedProperties();

            if (data.ClosestTriangleCalculation == MapNavMesh.FindClosestTriangleCalculation.SpiralOut) {
              serializedObject.Update();
              EditorGUILayout.PropertyField(serializedObject.FindProperty("ClosestTriangleCalculationDepth"), new GUIContent("Closest Triangle Calculation Depth", "Number of cells to search triangles in neighbors."));
              serializedObject.ApplyModifiedProperties();
            }

#if QUANTUM_XY
          serializedObject.Update();
          EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableQuantum_XY"), new GUIContent("Enable Quantum_XY", "Activate this and the navmesh baking will flip Y and Z to support navmeshes generated in the XY plane."));
          serializedObject.ApplyModifiedProperties();
#endif

            GUI.enabled = true;
          }

          using (new EditorGUI.DisabledScope(true)) {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AgentRadius"), new GUIContent("Imported Agent Radius", "This radius is overwritten during import. See Unity Navigation Tab/Bake/AgentRadius."));
            serializedObject.ApplyModifiedProperties();

            if (data.ImportRegions && data.Regions != null && data.Regions.Count > 0) {
              EditorGUILayout.PropertyField(serializedObject.FindProperty("Regions"), new GUIContent("Imported Regions", "Imported regions, cannot be overwritten and is reset on during every import."), true);
            }
          }

          using (new CustomEditorsHelper.SectionScope("NavMesh Debug")) {

            EditorGUI.BeginChangeCheck();

            var editorSettings = QuantumEditorSettings.Instance;
            editorSettings.NavMeshDefaultColor            = EditorGUILayout.ColorField("NavMesh Default Color", editorSettings.NavMeshDefaultColor);
            editorSettings.NavMeshRegionColor             = EditorGUILayout.ColorField("NavMesh Region Color",  editorSettings.NavMeshRegionColor);
            editorSettings.DrawNavMeshDefinitionAlways    = EditorGUILayout.Toggle("Toggle Always Draw",      editorSettings.DrawNavMeshDefinitionAlways);
            editorSettings.DrawNavMeshDefinitionMesh      = EditorGUILayout.Toggle("Toggle Draw Mesh",        editorSettings.DrawNavMeshDefinitionMesh);
            editorSettings.DrawNavMeshDefinitionOptimized = EditorGUILayout.Toggle("Toggle Optimized Gizmos", editorSettings.DrawNavMeshDefinitionOptimized);

            if (EditorGUI.EndChangeCheck()) {
              SceneView.RepaintAll();
            }

            if (GUILayout.Button("Export Test Mesh", EditorStyles.miniButton)) {
              Mesh m = data.CreateMesh();
              m.name = "NavMesh";

              AssetDatabase.CreateAsset(m, "Assets/NavMesh.asset");
              AssetDatabase.SaveAssets();
              AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
          }

          using (new CustomEditorsHelper.SectionScope("Edit Quantum NavMesh")) {

            using (new CustomEditorsHelper.BackgroundColorScope(Editing ? Color.red : Color.green)) {
              if (GUILayout.Button(Editing ? "Stop Editing NavMesh" : "Start Editing NavMesh", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2))) {
                Editing = !Editing;
                Save();
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                if (Editing && QuantumEditorSettings.Instance.DrawNavMeshDefinitionOptimized)
                  Debug.LogWarning("Manual NavMesh editing and optimized gizmo drawing (QuantumEditorSettings.DrawNavMeshDefinitionOptimized) does not work together. Deactivate it.");
                if (!Editing)
                  data.InvalidateGizmoMesh();
              }
            }

            if (Editing) {
              if (GUILayout.Button("Add Vertex", EditorStyles.miniButton)) {
                AddVertex(data);
              }

              if (GUILayout.Button("Delete Vertices", EditorStyles.miniButton)) {
                DeleteVertices(data);
              }


              if (SelectedVertices.Count == 1) {
                if (GUILayout.Button("Duplicate Vertex", EditorStyles.miniButton)) {
                  DuplicateVertex(data);
                }
              }

              if (SelectedVertices.Count == 2) {
                if (GUILayout.Button("Insert Vertex + Create Triangle", EditorStyles.miniButton)) {
                  InsertVertexAndCreateTriangle(data);
                }
              }

              if (SelectedVertices.Count == 3) {
                if (GUILayout.Button("Create Triangle", EditorStyles.miniButton)) {
                  CreateTriangle(data);
                }
              }

              if (GUILayout.Button("Duplicate And Flip", EditorStyles.miniButton)) {

                Undo.RegisterCompleteObjectUndo(data, "MapNavMeshDefinitionEditor - Duplicate And Flip");

                var idMap = new Dictionary<string, string>();

                foreach (var k in data.Vertices.ToList()) {
                  var v = k;

                  v.Id         = System.Guid.NewGuid().ToString();
                  v.Position.x = -v.Position.x;
                  v.Position.y = -v.Position.y;
                  v.Position.z = -v.Position.z;

                  idMap.Add(k.Id, v.Id);

                  ArrayUtility.Add(ref data.Vertices, v);
                }

                foreach (var k in data.Triangles.ToList()) {
                  var t = k;

                  t.Id           = System.Guid.NewGuid().ToString();
                  t.VertexIds    = new string[3];
                  t.VertexIds[0] = idMap[k.VertexIds[0]];
                  t.VertexIds[1] = idMap[k.VertexIds[1]];
                  t.VertexIds[2] = idMap[k.VertexIds[2]];

                  ArrayUtility.Add(ref data.Triangles, t);
                }
              }

              if (SelectedVertices.Count == 1) {
                var v = data.GetVertex(SelectedVertices.First());

                EditorGUI.BeginChangeCheck();

                v.Position = EditorGUILayout.Vector3Field("", v.Position);

                if (EditorGUI.EndChangeCheck()) {
                  data.Vertices[data.GetVertexIndex(SelectedVertices.First())] = v;
                }
              }
            }
          }

          using (new CustomEditorsHelper.SectionScope("Quantum NavMesh Information")) {
            EditorGUILayout.HelpBox(string.Format("Vertices: {0}\r\nTriangles: {1}", data.Vertices.Length, data.Triangles.Length), MessageType.Info);
          }

          EditorGUILayout.EndVertical();

          if (Editing) {
            backgroundColorRect = new Rect(backgroundColorRect.x - 13, backgroundColorRect.y - 17, EditorGUIUtility.currentViewWidth, backgroundColorRect.height + 20);
            EditorGUI.DrawRect(backgroundColorRect, new Color(0.5f, 0.0f, 0.0f, 0.1f));
          }
        }
        else {
          Editing = false;
        }
      }
    }

    void Save() {
      EditorUtility.SetDirty(target);
    }

    void OnDisable() {
      try {
        if (Editing && target) {
          Selection.activeGameObject = (target as MapNavMeshDefinition).gameObject;
        }
      }
      catch (System.Exception) { }
    }

    void DuplicateVertex(MapNavMeshDefinition data) {
      if (SelectedVertices.Count == 1) {
        var id = NewId();

        ArrayUtility.Add(ref data.Vertices, new MapNavMeshVertex {
          Id = id,
          Position = data.GetVertex(SelectedVertices.First()).Position.RoundToInt()
        });

        Save();

        SelectedVertices.Clear();
        SelectedVertices.Add(id);
      }
    }

    void AddVertex(MapNavMeshDefinition data) {
      Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { data, this }, "MapNavMeshDefinitionEditor - AddVertex");
      ArrayUtility.Add(ref data.Vertices, new MapNavMeshVertex {
        Id = NewId(),
        Position = new Vector3()
      });

      Save();
    }

    void DeleteVertices(MapNavMeshDefinition data) {
      if (SelectedVertices.Count > 0) {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { data, this }, "MapNavMeshDefinitionEditor - DeleteVertices");
        data.Vertices = data.Vertices.Where(x => SelectedVertices.Contains(x.Id) == false).ToArray();
        data.Triangles = data.Triangles.Where(x => x.VertexIds.Any(y => SelectedVertices.Contains(y)) == false).ToArray();

        Save();

        SelectedVertices.Clear();
      }
    }

    void CreateTriangle(MapNavMeshDefinition data) {
      if (SelectedVertices.Count() == 3) {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { data, this }, "MapNavMeshDefinitionEditor - CreateTriangle");
        ArrayUtility.Add(ref data.Triangles, new MapNavMeshTriangle {
          Id = NewId(),
          VertexIds = SelectedVertices.ToArray(),
          Area = 0,
          RegionId = null
        });

        Save();
      }
    }

    void InsertVertexAndCreateTriangle(MapNavMeshDefinition data) {
      if (SelectedVertices.Count() == 2) {
        Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { data, this }, "MapNavMeshDefinitionEditor - InsertVertexAndCreateTriangle");
        var id = NewId();
        ArrayUtility.Add(ref data.Vertices, new MapNavMeshVertex {
          Id = id,
          Position =
            Vector3.Lerp(
              data.GetVertex(SelectedVertices.First()).Position,
              data.GetVertex(SelectedVertices.Last()).Position,
              0.5f
            ).RoundToInt()
        });

        SelectedVertices.Add(id);

        CreateTriangle(data);

        SelectedVertices.Clear();
        SelectedVertices.Add(id);

        Save();
      }
    }

    void OnSceneGUI() {
      Tools.current = Tool.None;

      var data = target as MapNavMeshDefinition;

      if (Editing && data) {
        Selection.activeGameObject = (target as MapNavMeshDefinition).gameObject;
      }
      else {
        return;
      }

      if (data) {
        if (Event.current.type == EventType.KeyDown) {
          switch (Event.current.keyCode) {
            case KeyCode.Escape:
              SelectedVertices.Clear();
              break;

            case KeyCode.T:
              switch (SelectedVertices.Count()) {
                case 2:
                  InsertVertexAndCreateTriangle(data);
                  break;

                case 3:
                  CreateTriangle(data);
                  break;

                default:
                  Debug.LogError("Must select 2 or 3 vertices to use 'T' command");
                  break;
              }
              break;

            case KeyCode.X:
              Undo.RegisterCompleteObjectUndo(this, "MapNavMeshDefinitionEditor - Changed selection");
              var select = new HashSet<string>();
              foreach (var tri in data.Triangles) {
                foreach (var v in SelectedVertices) {
                  if (System.Array.IndexOf(tri.VertexIds, v) >= 0) {
                    select.Add(tri.VertexIds[0]);
                    select.Add(tri.VertexIds[1]);
                    select.Add(tri.VertexIds[2]);
                    break;
                  }
                }
              }

              foreach (var v in select) {
                SelectedVertices.Add(v);
              }
              break;

            case KeyCode.Backspace:
              DeleteVertices(data);
              break;

            case KeyCode.F:
              var pos = Vector3.zero;
              foreach (var v in SelectedVertices) pos += data.GetVertex(v).Position;
              pos /= SelectedVertices.Count;
              SceneView.lastActiveSceneView.pivot = pos;
              break;
          }
        }

        // Eat the default focus event. See case KeyCode.F.
        if (Event.current.type == EventType.ExecuteCommand) {
          if (Event.current.commandName == "FrameSelected") {
            Event.current.commandName = "";
            Event.current.Use();
          }
        }

        foreach (var v in data.Vertices) {
          var p = data.transform.TransformPoint(v.Position);
          var r = Quaternion.LookRotation((p - SceneView.currentDrawingSceneView.camera.transform.position).normalized);
          var s = 0f;

          if (SelectedVertices.Contains(v.Id)) {
            s = 0.2f;
            Handles.color = Color.green;
          }
          else {
            s = 0.1f;
            Handles.color = Color.white;
          }

          var handleSize = 0.5f * HandleUtility.GetHandleSize(p);
          if (Handles.Button(p, r, s * handleSize, s * handleSize, Handles.DotHandleCap)) {

            if (Event.current.shift) {
              if (SelectedVertices.Contains(v.Id)) {
                Undo.RegisterCompleteObjectUndo(this, "MapNavMeshDefinitionEditor - Changed selection");
                SelectedVertices.Remove(v.Id);
              }
              else {
                Undo.RegisterCompleteObjectUndo(this, "MapNavMeshDefinitionEditor - Changed selection");
                SelectedVertices.Add(v.Id);
              }
            }
            else {
              Undo.RegisterCompleteObjectUndo(this, "MapNavMeshDefinitionEditor - Changed selection");
              SelectedVertices.Clear();
              SelectedVertices.Add(v.Id);
            }

            Repaint();
          }
        }

        if (SelectedVertices.Count > 0) {
          var center = Vector3.zero;
          var positions = data.Vertices.Where(x => SelectedVertices.Contains(x.Id)).Select(x => data.transform.TransformPoint(x.Position));

          foreach (var p in positions) {
            center += p;
          }

          center /= positions.Count();

          var movedCenter = Handles.DoPositionHandle(center, Quaternion.identity);
          if (movedCenter != center) {
            var m = movedCenter - center;

#if QUANTUM_XY
          m.z = 0;
#else
            m.y = 0;
#endif

            Undo.RegisterCompleteObjectUndo(data, "MapNavMeshDefinitionEditor - Moved vertex");

            foreach (var selected in SelectedVertices) {
              var index = data.GetVertexIndex(selected);
              if (index >= 0) {
                data.Vertices[index].Position += m;
              }
            }
          }
        }
      }
    }
  }
}
