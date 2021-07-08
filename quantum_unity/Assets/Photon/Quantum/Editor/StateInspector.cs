using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Photon.Deterministic;
using Quantum.Core;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Quantum.Editor {

  public unsafe class StateInspector : EditorWindow {

    private static Lazy<StaticContext> _context = new Lazy<StaticContext>(() => new StaticContext());

    [SerializeField]
    private MultiColumnHeaderState _headerState = new MultiColumnHeaderState(new[] {
      new MultiColumnHeaderState.Column() {
        headerContent = new GUIContent("Name"),
        canSort = false,
        width = 300,
      },
      new MultiColumnHeaderState.Column() {
        headerContent = new GUIContent("Value"),
        canSort = false,
        width = 300,
      },
    });

    [SerializeField]
    private ComponentTypeSetSelector _prohibitedComponents = new ComponentTypeSetSelector() { QualifiedNames = Array.Empty<string>() };

    [SerializeField]
    private ComponentTypeSetSelector _requiredComponents = new ComponentTypeSetSelector() { QualifiedNames = Array.Empty<string>() };

    [SerializeField]
    private TreeViewState _treeViewState = new TreeViewState();

    private StateInspectorTreeView _treeView;

    private static StaticContext Context => _context.Value;


    [MenuItem("Window/Quantum/State Inspector")]
    [MenuItem("Quantum/Show State Inspector", false, 44)]
    public static void ShowWindow() {
      StateInspector window = (StateInspector)GetWindow(typeof(StateInspector), false, "Quantum State Inspector");
      window.Show();
    }

    public void OnEnable() {
      var header = new MultiColumnHeader(_headerState);
      _treeView = new StateInspectorTreeView(_treeViewState, header);

      RemoveInvalidNames(_requiredComponents);
      RemoveInvalidNames(_prohibitedComponents);
    }

    public void OnGUI() {
      var game = QuantumRunner.Default?.Game;

      var rect = new Rect(0, 0, position.width, position.height);
      const float toolbarHeight = 18.0f;

      var toolbarRect = new Rect(0, 0, position.width, toolbarHeight);
      using (new GUI.GroupScope(toolbarRect, EditorStyles.toolbar)) {
        using (new GUILayout.HorizontalScope()) {
          var frame = game?.Frames?.Predicted;
          using (new EditorGUI.DisabledScope(frame == null)) {
            if (GUILayout.Button("Save Frame Dump", EditorStyles.toolbarButton)) {
              var path = EditorUtility.SaveFilePanel("Save Frame Dump", string.Empty, $"frame_{frame.Number}.txt", "txt");
              if (!string.IsNullOrEmpty(path)) {
                File.WriteAllText(path, frame.DumpFrame());
              }
            }

            var buttonStyle = new GUIStyle(EditorStyles.toolbarDropDown);
            buttonStyle.clipping = TextClipping.Clip;
            buttonStyle.alignment = TextAnchor.MiddleLeft;

            {
              var guiContent = new GUIContent($"Entities With: {GetPrettyString(_requiredComponents)}");
              var buttonRect = GUILayoutUtility.GetRect(guiContent, buttonStyle, GUILayout.MinWidth(50));
              if (GUI.Button(buttonRect, guiContent, buttonStyle)) {
                PopupWindow.Show(buttonRect, new SelectComponentsContent() { Selector = _requiredComponents });
              }
            }

            {
              var guiContent = new GUIContent($"Entities Without: {GetPrettyString(_prohibitedComponents)}");
              var buttonRect = GUILayoutUtility.GetRect(guiContent, buttonStyle, GUILayout.MinWidth(50));
              if (GUI.Button(buttonRect, guiContent, buttonStyle)) {
                PopupWindow.Show(buttonRect, new SelectComponentsContent() { Selector = _prohibitedComponents });
              }
            }

            GUILayout.FlexibleSpace();
          }
        }
      }

      _treeView.Game = game;
      _treeView.WithComponents = _requiredComponents.Set;
      _treeView.WithoutComponents = _prohibitedComponents.Set;
      var contentsRect = rect.AddY(toolbarHeight).AddHeight(-toolbarHeight);

      if (game == null) {
        using (new GUI.GroupScope(contentsRect)) {
          EditorGUILayout.HelpBox("Simulation not running.", MessageType.Info);
        }
      } else {
        _treeView.Reload();
        _treeView.OnGUI(contentsRect);
      }
    }

    public void OnInspectorUpdate() {
      Repaint();
    }

    private static void TraversePtr(Frame frame, void* objectPtr, Type objectType, string rootPath, Func<string, string, int, bool> onNode, Action<string, string, int, string> onValue) {
      const char splitter = '/';
      StringBuilder pathBuilder = new StringBuilder(rootPath + splitter);
      List<int> pathSplitters = new List<int>() { rootPath.Length };

      using (var reader = new StringReader(Context.DumpPointer(frame, objectPtr, objectType))) {
        Debug.Assert(reader.ReadLine() == "#ROOT:");

        int previousDepth = 1;
        int ignoreDepth = int.MaxValue;

        for (string line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
          var valueSplitter = line.IndexOf(':');
          if (valueSplitter < 0) {
            Debug.LogWarning($"Invalid line format: {line}");
            continue;
          }

          int indent = 0;
          while (indent < line.Length && line[indent] == ' ')
            ++indent;

          Debug.Assert(indent >= 2);
          Debug.Assert(indent % 2 == 0);
          var depth = indent / 2;

          if (depth > ignoreDepth)
            continue;

          ignoreDepth = int.MaxValue;

          var name = line.Substring(indent, valueSplitter - indent);

          if (depth > previousDepth) {
            Debug.Assert(depth == previousDepth + 1);
            for (int i = previousDepth; i < depth - 1; ++i) {
              pathSplitters.Add(pathBuilder.Length);
              pathBuilder.Append(splitter).Append("???");
            }
          } else {
            Debug.Assert(depth > 0);
            var splitterIndex = pathSplitters[depth - 1];
            pathSplitters.RemoveRange(depth - 1, pathSplitters.Count - depth + 1);
            pathBuilder.Remove(splitterIndex, pathBuilder.Length - splitterIndex);
          }

          pathSplitters.Add(pathBuilder.Length);
          pathBuilder.Append(splitter).Append(name);

          bool hasValue = false;
          for (int i = valueSplitter + 1; i < line.Length && !hasValue; ++i) {
            hasValue = !char.IsWhiteSpace(line[i]);
          }

          if (hasValue) {
            var value = line.Substring(valueSplitter + 1);
            onValue(name, pathBuilder.ToString(), depth, value);
          } else {
            if (!onNode(name, pathBuilder.ToString(), depth)) {
              ignoreDepth = depth;
            }
          }
          previousDepth = depth;
        }
      }
    }

    private static string GetPrettyString(ComponentTypeSetSelector selector) {
      if (!(selector.QualifiedNames?.Length > 0)) {
        return "None";
      }

      return string.Join(", ", selector.QualifiedNames.Select(x => Type.GetType(x)?.Name));
    }

    private static void DrawValue(Rect cellRect, string value) {
      var match = Context.AssetRefRegex.Match(value);
      if (match.Success && long.TryParse(match.Groups[1].Value, out long rawGuid)) {
        var halfRect = cellRect.SetWidth(cellRect.width / 2);        
        EditorGUI.SelectableLabel(halfRect, value);
        AssetRefDrawer.DrawAsset(halfRect.AddX(halfRect.width), new AssetGuid(rawGuid));
      } else {
        EditorGUI.SelectableLabel(cellRect, value);
      }
    }

    private void RemoveInvalidNames(ComponentTypeSetSelector selector) {
      selector.QualifiedNames = selector.QualifiedNames.Where(x => Type.GetType(x) != null).ToArray();
    }

    [Serializable]
    public class GUILayoutDrawer {

      [SerializeField]
      [HideInInspector]
      private List<string> _expandedNodes = new List<string>();

      public void DrawEntity(Frame frame, EntityRef entityRef, string rootLabel) {
        if (!DoFoldout("/", rootLabel)) {
          return;
        }

        using (new EditorGUI.IndentLevelScope()) {
          for (int componentTypeIndex = 1; componentTypeIndex < ComponentTypeId.Type.Length; componentTypeIndex++) {
            if (frame.Has(entityRef, componentTypeIndex)) {
              var componentType = ComponentTypeId.Type[componentTypeIndex];
              var componentNodePath = $"/{componentType.Name}";

              if (!DoFoldout(componentNodePath, componentType.Name))
                continue;

              void* componentPtr = Context.GetComponentPointer(frame, entityRef, componentType);
              TraversePtr(frame, componentPtr, componentType, componentNodePath, (name, path, depth) => {
                using (new EditorGUI.IndentLevelScope(depth - EditorGUI.indentLevel + 1)) {
                  return DoFoldout(path, name);
                }
              }, (name, path, depth, value) => {
                using (new EditorGUI.IndentLevelScope(depth - EditorGUI.indentLevel + 1)) {
                  var rect = EditorGUILayout.GetControlRect();
                  rect = EditorGUI.PrefixLabel(rect, new GUIContent(name));
                  using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
                    DrawValue(rect, value);
                  }
                }
              });
            }
          }
        }
      }

      private bool DoFoldout(string path, string label) {
        var index = _expandedNodes.BinarySearch(path);
        if (EditorGUILayout.Foldout(index >= 0, label)) {
          if (index < 0) {
            _expandedNodes.Insert(~index, path);
          }
          return true;
        } else {
          if (index >= 0) {
            _expandedNodes.RemoveAt(index);
          }
          return false;
        }
      }
    }

    private class SelectComponentsContent : PopupWindowContent {
      public ComponentTypeSetSelector Selector;
      private GUIContent[] _prettyNames;
      private string[] _qualifiedNames;
      private Vector2 _scrollPos;
      public override Vector2 GetWindowSize() {
        if (ComponentTypeId.Type.Length <= 1)
          return base.GetWindowSize();

        var perfectWidth = _prettyNames.Max(x => EditorStyles.label.CalcSize(x).x);
        var perfectHeight = CustomEditorsHelper.GetLinesHeight(_prettyNames.Length);
        return new Vector2(Mathf.Clamp(perfectWidth + 25, 200, Screen.width), Mathf.Clamp(perfectHeight + 10, 200, Screen.height));
      }

      public override void OnGUI(Rect rect) {
        using (new GUI.GroupScope(rect)) {
          using (var scroll = new GUILayout.ScrollViewScope(_scrollPos)) {
            _scrollPos = scroll.scrollPosition;
            for (int i = 0; i < _prettyNames.Length; ++i) {
              var wasSelected = ArrayUtility.Contains(Selector.QualifiedNames, _qualifiedNames[i]);
              if (wasSelected != EditorGUILayout.ToggleLeft(_prettyNames[i], wasSelected)) {
                ArrayUtility.Remove(ref Selector.QualifiedNames, _qualifiedNames[i]);
                if (!wasSelected) {
                  ArrayUtility.Add(ref Selector.QualifiedNames, _qualifiedNames[i]);
                }
                Selector.Reset();
              }
            }
          }
        }
      }

      public override void OnOpen() {
        base.OnOpen();
        ComponentTypeSelectorDrawer.Initialize(ref _qualifiedNames, ref _prettyNames);
      }
    }

    private class StaticContext {
      public readonly Regex AssetRefRegex = new Regex(@"^\s*Id: \[(-?\d+)\]\s*$", RegexOptions.Compiled);
      public readonly FramePrinter Printer;

      private readonly Dictionary<Type, Delegate> _getComponentPointerDelegates;

      public StaticContext() {
        Printer = new FramePrinter();
        _getComponentPointerDelegates = ComponentTypeId.Type.Where(x => x != null).ToDictionary(x => x, x => CreateGetComponentPointerDelegate(x));
      }

      private delegate T* GetPointerDelegate<T>(ref FrameBase.FrameBaseUnsafe frameUnsafe, EntityRef entityRef) where T : unmanaged;
      public string DumpPointer(Frame frame, void* ptr, Type type) {
        try {
          Printer.Reset(frame);
          Printer.AddPointer("#ROOT", ptr, type);
          return Printer.ToString();
        } finally {
          Printer.Reset(null);
        }
      }

      public void* GetComponentPointer(Frame frame, EntityRef entityRef, Type componentType) {
        return Pointer.Unbox(_getComponentPointerDelegates[componentType].DynamicInvoke(new object[] { frame.Unsafe, entityRef }));
      }

      private static Delegate CreateGetComponentPointerDelegate(Type componentType) {
        var getMethod = typeof(FrameBase.FrameBaseUnsafe).GetMethod("GetPointer");
        var getGenericMethod = getMethod.MakeGenericMethod(componentType);
        var getGenericDelegateType = typeof(GetPointerDelegate<>).MakeGenericType(componentType);
        return Delegate.CreateDelegate(getGenericDelegateType, getGenericMethod, true);
      }
    }

    private unsafe sealed class StateInspectorTreeView : TreeView {
      private readonly List<EntityRef> _entityRefBuffer = new List<EntityRef>();
      private readonly Dictionary<int, string> _ids = new Dictionary<int, string>();
      private readonly int EntitiesId;
      private readonly int GlobalsId;
      private readonly int TicksId;
      private List<TreeViewItem> _rowsBuffer = new List<TreeViewItem>();
      private int _initialDepth = 0;

      public StateInspectorTreeView(TreeViewState state, MultiColumnHeader header) : base(state, header) {
        EntitiesId = GetId("Entities");
        GlobalsId = GetId("Globals");
        TicksId = GetId("Ticks");
        Reload();
      }

      public QuantumGame Game { get; set; }
      public ComponentSet WithComponents { get; set; }
      public ComponentSet WithoutComponents { get; set; }

      protected override TreeViewItem BuildRoot() {
        return new TreeViewItem() {
          id = 0,
          depth = -1,
          displayName = "Root"
        };
      }
      protected override IList<TreeViewItem> BuildRows(TreeViewItem root) {
        _rowsBuffer.Clear();

        var frame = Game?.Frames.Predicted;
        if (frame == null) {
          return _rowsBuffer;
        }

        _rowsBuffer.Add(new TreeViewItemWithValue(TicksId, 0, "Ticks", frame.Number.ToString()));

        var globals = new TreeViewItem(GlobalsId, 0, "Globals");
        _rowsBuffer.Add(globals);

        if (IsExpanded(GlobalsId)) {
          _initialDepth = 1;
          TraversePtr(frame, frame.Global, typeof(_globals_), "Globals", AddTreeViewItemNode, AddTreeViewItemValue);
        } else {
          globals.children = CreateChildListForCollapsedParent();
        }

        var entities = new TreeViewItem(EntitiesId, 0, "Entities");
        _rowsBuffer.Add(entities);

        if (IsExpanded(EntitiesId)) {
          _entityRefBuffer.Clear();
          frame.GetAllEntityRefs(_entityRefBuffer);

          foreach (var entityRef in _entityRefBuffer) {
            if (!WithComponents.IsEmpty) {
              var componentSet = frame.GetComponentSet(entityRef);
              if (!componentSet.IsSupersetOf(WithComponents)) {
                continue;
              }
            }

            if (!WithoutComponents.IsEmpty) {
              var componentSet = frame.GetComponentSet(entityRef);
              if (componentSet.Overlaps(WithoutComponents)) {
                continue;
              }
            }

            var name = $"Entity {entityRef.Index:0000}";
            var entityNode = new TreeViewItem(GetId(entityRef.ToString()), 1, name);
            _rowsBuffer.Add(entityNode);

            if (IsExpanded(entityNode.id)) {
              for (int componentTypeIndex = 1; componentTypeIndex < ComponentTypeId.Type.Length; componentTypeIndex++) {
                if (frame.Has(entityRef, componentTypeIndex)) {
                  var componentType = ComponentTypeId.Type[componentTypeIndex];
                  var componentNodeName = $"{name}/{componentType.Name}";
                  var componentNode = new TreeViewItem(GetId(componentNodeName), 2, componentType.Name);
                  _rowsBuffer.Add(componentNode);

                  if (IsExpanded(componentNode.id)) {
                    var componentPtr = Context.GetComponentPointer(frame, entityRef, componentType);
                    _initialDepth = 2;
                    TraversePtr(frame, componentPtr, componentType, componentNodeName, AddTreeViewItemNode, AddTreeViewItemValue);
                  } else {
                    componentNode.children = CreateChildListForCollapsedParent();
                  }
                }
              }
            } else {
              entityNode.children = CreateChildListForCollapsedParent();
            }
          }
        } else {
          entities.children = CreateChildListForCollapsedParent();
        }

        SetupParentsAndChildrenFromDepths(root, _rowsBuffer);
        return _rowsBuffer;
      }

      protected override void RowGUI(RowGUIArgs args) {
        for (int i = 0; i < args.GetNumVisibleColumns(); ++i) {
          CellGUI(args.GetCellRect(i), args.item, args.GetColumn(i), ref args);
        }
      }

      private static uint Hash(uint x) {
        x = ((x >> 16) ^ x) * 0x45d9f3b;
        x = ((x >> 16) ^ x) * 0x45d9f3b;
        x = (x >> 16) ^ x;
        return x;
      }

      private bool AddTreeViewItemNode(string name, string path, int depth) {
        var id = GetId(path);
        var item = new TreeViewItem(id, depth + _initialDepth, name);
        _rowsBuffer.Add(item);

        if (!IsExpanded(id)) {
          item.children = CreateChildListForCollapsedParent();
          return false;
        }
        return true;
      }

      private void AddTreeViewItemValue(string name, string path, int depth, string value) {
        var id = GetId(path);
        _rowsBuffer.Add(new TreeViewItemWithValue(id, depth + _initialDepth, name, value));
      }

      private void CellGUI(Rect rect, TreeViewItem item, int column, ref RowGUIArgs args) {
        if (column == 0) {
          base.RowGUI(args);
        } else if (column == 1 && item is TreeViewItemWithValue v) {
          DrawValue(rect, v.Value);
        }
      }

      private int GetId(string key) {
        var id = key.GetHashCode();

        for (; ; ) {
          if (_ids.TryGetValue(id, out var idString)) {
            if (key == idString) {
              return id;
            } else {
              id = (int)Hash((uint)id);
            }
          } else {
            _ids.Add(id, key);
            return id;
          }
        }
      }
    }

    private sealed class TreeViewItemWithValue : TreeViewItem {
      public string Value;

      public TreeViewItemWithValue(int id, int depth, string name, string value) : base(id, depth, name) {
        Value = value;
      }
    }
  }
}