using System;
using System.Linq;
using System.Reflection;
using Quantum.Inspector;
using UnityEditor;
using UnityEngine;
using Quantum.Editor;
using Quantum;
using System.Text.RegularExpressions;

public static class CustomEditorsHelper {

  public const int CheckboxWidth = 16;
  public static readonly GUIContent WhitespaceContent = new GUIContent(" ");
  public static Func<SerializedProperty, GUIContent, FieldInfo, bool> OnDrawProperty;
  private static readonly Regex _arrayElementRegex = new Regex(@"\.Array\.data\[\d+\]$", RegexOptions.Compiled);

  public static readonly IQuantumEditorGUI EditorGUIProxy = new QuantumEditorGUI();

  public delegate bool DrawCallback(SerializedProperty property, FieldInfo field, Type type);

  public static float GetLinesHeight(int count) {
    return count * (EditorGUIUtility.singleLineHeight) + (count - 1) * EditorGUIUtility.standardVerticalSpacing;
  }

  public static float GetLinesHeightWithNarrowModeSupport(int count) {
    if (!EditorGUIUtility.wideMode) {
      count++;
    }
    return count * (EditorGUIUtility.singleLineHeight) + (count - 1) * EditorGUIUtility.standardVerticalSpacing;
  }

  public static void DrawHeadline(string header) {
    EditorGUILayout.Space();
    EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
  }

  public static void DrawScript(UnityEngine.Object obj) {
    MonoScript script = null;
    var asScriptableObject = obj as ScriptableObject;
    if (asScriptableObject) {
      script = MonoScript.FromScriptableObject(asScriptableObject);
    } else {
      var asMonoBehaviour = obj as MonoBehaviour;
      if (asMonoBehaviour) {
        script = MonoScript.FromMonoBehaviour(asMonoBehaviour);
      }
    }

    GUI.enabled = false;
    script = EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false) as MonoScript;
    GUI.enabled = true;
  }

  public static bool DrawDefaultInspector(SerializedObject obj, string path, string[] filter, bool showFoldout = true, DrawCallback callback = null) {
    bool foldout = false;
    return DrawDefaultInspector(obj, path, filter, showFoldout, ref foldout, callback);
  }

  internal static Rect DrawIconPrefix(Rect rect, string tooltip, MessageType messageType) {
    var content = EditorGUIUtility.TrTextContentWithIcon(string.Empty, tooltip, messageType);
    var iconRect = rect;
    iconRect.width = Mathf.Min(20, rect.width);

    GUI.Label(iconRect, content, new GUIStyle());

    rect.width = Mathf.Max(0, rect.width - iconRect.width);
    rect.x += iconRect.width;

    return rect;
  }

  public static bool DrawDefaultInspector(SerializedObject obj, string path, string[] filter, bool showFoldout, ref bool foldout, DrawCallback callback = null) {
    var property = obj.FindProperty(path);
    if (property == null)
      return false;

    if (showFoldout) {
      EditorStyles.foldout.fontStyle = FontStyle.Bold;
      var pathTokens = path.Split('.');
      property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, pathTokens[pathTokens.Length - 1], true);
      EditorStyles.foldout.fontStyle = FontStyle.Normal;
      foldout = property.isExpanded;
      if (!property.isExpanded)
        return false;

      EditorGUI.indentLevel++;
    }

    EditorGUI.BeginChangeCheck();
    obj.Update();

    bool expanded = true;
    while (property.NextVisible(expanded)) {
      if (!property.propertyPath.StartsWith(path))
        continue;
      PropertyFieldWithQuantumCoreAttributes(property, filter, false, callback);
      expanded = false;
    }

    if (showFoldout)
      EditorGUI.indentLevel--;

    obj.ApplyModifiedProperties();
    return EditorGUI.EndChangeCheck();
  }

  public static bool DrawDefaultInspector(SerializedObject obj, string[] filter = null, DrawCallback callback = null) {
    EditorGUI.BeginChangeCheck();
    obj.Update();

    PropertyFieldWithQuantumCoreAttributes(obj.GetIterator(), filter, true, callback);

    obj.ApplyModifiedProperties();
    return EditorGUI.EndChangeCheck();
  }

  public static bool DrawDefaultInspector(SerializedProperty root, string[] filter = null, bool skipRoot = true, DrawCallback callback = null, GUIContent label = null) {

    EditorGUI.BeginChangeCheck();
    PropertyFieldWithQuantumCoreAttributes(root, filter, skipRoot, callback, label);
    root.serializedObject.ApplyModifiedProperties();
    return EditorGUI.EndChangeCheck();
  }

  public static void BeginSection(string headline = null) {
    if (string.IsNullOrEmpty(headline)) {
      EditorGUILayout.Space();
    } else {
      DrawHeadline(headline);
    }
  }

  public static void EndSection() {
  }

  public static void BeginBox(string headline = null, int indentLevel = 1) {
    GUILayout.BeginVertical(EditorStyles.helpBox);
    if (!string.IsNullOrEmpty(headline))
      EditorGUILayout.LabelField(headline, EditorStyles.boldLabel);
    EditorGUI.indentLevel += indentLevel;
  }

  public static void EndBox(int indentLevel = 1) {
    EditorGUI.indentLevel -= indentLevel;
    GUILayout.EndVertical();
  }

  public sealed class BoxScope : IDisposable {
    private readonly SerializedObject _serializedObject;
    private readonly Color _backgroundColor;
    private readonly int _indentLevel;

    public BoxScope(string headline = null, SerializedObject serializedObject = null, int indentLevel = 1) {
      _indentLevel = indentLevel;
#if !UNITY_2019_3_OR_NEWER
      _backgroundColor = GUI.backgroundColor;
      if (EditorGUIUtility.isProSkin) {
        GUI.backgroundColor = Color.grey;
      }
#endif

      BeginBox(headline, _indentLevel);

      _serializedObject = serializedObject;
      if (_serializedObject != null) {
        EditorGUI.BeginChangeCheck();
      }
    }

    public void Dispose() {
      EndBox(_indentLevel);

#if !UNITY_2019_3_OR_NEWER
      GUI.backgroundColor = _backgroundColor;
#endif

      if (_serializedObject != null && EditorGUI.EndChangeCheck()) {
        _serializedObject.ApplyModifiedProperties();
      }
    }
  }

  public sealed class EditorFoldoutScope : IDisposable {
    public bool IsFoldout;
    public EditorFoldoutScope(string headline, string key) {
      BeginBox(null);
      EditorGUI.indentLevel--;

      IsFoldout = EditorPrefs.GetBool(key);
      var newIsFoldout = EditorGUILayout.Foldout(EditorPrefs.GetBool(key), headline);
      if (newIsFoldout != IsFoldout) {
        EditorPrefs.SetBool(key, newIsFoldout);
        IsFoldout = newIsFoldout;
      }

      EditorGUI.indentLevel++;
    }

    public void Dispose() {
      EndBox();
    }
  }

  public sealed class PropertyScope : IDisposable {

    public PropertyScope(Rect position, GUIContent label, SerializedProperty property) {
      EditorGUI.BeginProperty(position, label, property);
    }

    public void Dispose() {
      EditorGUI.EndProperty();
    }
  }
  public sealed class SectionScope : IDisposable {
    public SectionScope(string headline = null) {
      BeginSection(headline);
    }

    public void Dispose() {
      EndSection();
    }
  }

  public sealed class IndentLevelScope : IDisposable {
    private readonly int _currentIndentLevel;

    public IndentLevelScope(int indentLevel) {
      _currentIndentLevel = EditorGUI.indentLevel;
      EditorGUI.indentLevel = indentLevel;
    }

    public void Dispose() {
      EditorGUI.indentLevel = _currentIndentLevel;
    }
  }

  public sealed class LabelWidthScope : IDisposable {
    private readonly float _currentLabelWidth;

    public LabelWidthScope(float labelWidth) {
      _currentLabelWidth          = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = labelWidth;
    }

    public void Dispose() {
      EditorGUIUtility.labelWidth = _currentLabelWidth;
    }
  }

  public sealed class BackgroundColorScope : IDisposable {
    private readonly Color _color;

    public BackgroundColorScope(Color color) {
      _color = GUI.backgroundColor;
      GUI.backgroundColor = color;
    }

    public void Dispose() {
      GUI.backgroundColor = _color;
    }
  }

  public sealed class ColorScope : IDisposable {
    private readonly Color _color;

    public ColorScope(Color color) {
      _color = GUI.color;
      GUI.color = color;
    }

    public void Dispose() {
      GUI.color = _color;
    }
  }

  public static void HandleMultiTypeField(SerializedProperty p, params Type[] types) {

    var c = p.objectReferenceValue;

    var rect = EditorGUILayout.GetControlRect(true);
    var label = new GUIContent(p.displayName);

    using (new CustomEditorsHelper.PropertyScope(rect, label, p)) {
      rect = EditorGUI.PrefixLabel(rect, label);
      using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel)) {
        EditorGUI.BeginChangeCheck();
        if (c != null) {
          var matchingType = types.SingleOrDefault(x => x.IsInstanceOfType(c));
          if (matchingType != null) {
            c = EditorGUI.ObjectField(rect, c, matchingType, true);
          } else {
            c = EditorGUI.ObjectField(rect, c, typeof(UnityEngine.Object), true);
            EditorGUILayout.HelpBox($"Type not supported: {c.GetType()}", MessageType.Error);
          }
        } else {
          var r = rect.SetWidth(rect.width / types.Length);
          foreach (var t in types) {
            var value = EditorGUI.ObjectField(r, null, t, true);
            if (c == null) {
              c = value;
            }
            r.x += r.width;
          }
        }
        if (EditorGUI.EndChangeCheck()) {
          p.objectReferenceValue = c;
          p.serializedObject.ApplyModifiedProperties();
        }
      }
    }
  }

  private delegate FieldInfo GetFieldInfoFromPropertyDelegate(SerializedProperty property, out Type type);

  private static readonly GetFieldInfoFromPropertyDelegate GetFieldInfoFromProperty =
    Quantum.ReflectionUtils.CreateEditorMethodDelegate<GetFieldInfoFromPropertyDelegate>(
      "UnityEditor.ScriptAttributeUtility",
      "GetFieldInfoFromProperty",
      BindingFlags.Static | BindingFlags.NonPublic);

  private static SerializedProperty FindPropertyRelativeToParent(SerializedProperty property, string relativePath) {
    SerializedProperty otherProperty;
    var path = property.propertyPath;

    // array element?
    if ( path.EndsWith("]") ) {
      var match = _arrayElementRegex.Match(path);
      if ( match.Success ) {
        path = path.Substring(0, match.Index);
      }
    }

    var lastDotIndex = path.LastIndexOf('.');
    if (lastDotIndex < 0) {
      otherProperty = property.serializedObject.FindProperty(relativePath);
    } else {
      otherProperty = property.serializedObject.FindProperty(path.Substring(0, lastDotIndex) + "." + relativePath);
    }
    return otherProperty;
  }

  private static SerializedProperty FindPropertyRelativeToParentOrThrow(SerializedProperty property, string relativePath) {
    var result = FindPropertyRelativeToParent(property, relativePath);
    if (result == null) {
      throw new ArgumentOutOfRangeException($"Property relative to the parent of \"{property.propertyPath}\" not found: {relativePath}");
    }
    return result;
  }

  private static void PropertyFieldWithQuantumCoreAttributes(SerializedProperty root, string[] filters, bool skipRoot, DrawCallback callback, GUIContent label = null) {
    SerializedProperty property = root.Copy();

    int indentLevel = EditorGUI.indentLevel;
    int indentOffset = indentLevel - root.depth - (skipRoot ? 1 : 0);
    int minDepth = root.depth;
    int? disabledDepth = null;

    try {
      bool expanded = true;

      do {
        var guiContent = label ?? new GUIContent(property.displayName);
        label = null;

        var enabled = true;
        if ( disabledDepth.HasValue ) {
          if ( disabledDepth.Value < property.depth ) {
            enabled = false;
          } else {
            disabledDepth = null;
          }
        }



        if (skipRoot) {
          skipRoot = false;
          continue;
        }

        if (filters?.Any(f => property.propertyPath.StartsWith(f)) == true) {
          expanded = false;
          continue;
        }

        EditorGUI.indentLevel = property.depth + indentOffset;


        // check attributes
        Quantum.Core.FixedArrayAttribute fixedArrayAttribute = null;
        bool isLayer = false;
        bool isDegrees = false;

        Type unionType = null;

        var field = GetFieldInfoFromProperty(property, out Type type);
        
        string optionalPropertyPath = null;

        if (callback != null) {
          var propertyCopy = property.Copy();
          if (callback.Invoke(propertyCopy, field, type)) {
            expanded = property.isExpanded;
            continue;
          }
        }


        if (field != null) {

          var header = field.GetCustomAttribute<Quantum.Inspector.HeaderAttribute>();
          if (header != null) {
            DrawHeadline(header.Header);
          }

          if (field.GetCustomAttribute<Quantum.Inspector.SpaceAttribute>() != null) {
            EditorGUILayout.Space();
          }

          var tooltip = field.GetCustomAttribute<Quantum.Inspector.TooltipAttribute>();
          if (tooltip != null) {
            guiContent = new GUIContent(guiContent.text, tooltip.Tooltip);
          }

          bool skipProperty = false;

          if (field.GetCustomAttribute<Quantum.Inspector.HideInInspectorAttribute>() != null) {
            skipProperty = true;
          }

          foreach (var drawIf in field.GetCustomAttributes<Quantum.Inspector.DrawIfAttribute>(true)) {
            var otherProperty = FindPropertyRelativeToParentOrThrow(property, drawIf.FieldName);

              if (!DrawIfAttribute.CheckDraw(drawIf, otherProperty.GetIntegerValue())) {
                if (drawIf.Hide == DrawIfHideType.Hide) {
                  skipProperty = true;
                  break;
                } else if (drawIf.Hide == DrawIfHideType.ReadOnly) {
                  enabled = false;
                  if (disabledDepth == null) {
                    disabledDepth = property.depth;
                  }
                }
              }
          }

          {
            var elementType = field.FieldType.HasElementType ? field.FieldType.GetElementType() : field.FieldType;
            if (elementType.IsSubclassOf(typeof(Quantum.UnionPrototype)))
              unionType = elementType;
          }

          optionalPropertyPath = field.GetCustomAttribute<Quantum.Inspector.OptionalAttribute>()?.EnabledPropertyPath;

          if (skipProperty) {
            expanded = false;
            continue;
          }

          isLayer = field.GetCustomAttribute<Quantum.Inspector.LayerAttribute>() != null;
          isDegrees = field.GetCustomAttribute<Quantum.Core.DegreesAttribute>() != null;

          fixedArrayAttribute = field.GetCustomAttribute<Quantum.Core.FixedArrayAttribute>();
        }

        EditorGUI.BeginChangeCheck();

        using (new EditorGUI.DisabledScope(!enabled || "m_Script" == property.propertyPath && property.depth == 0)) {
          if (isLayer) {
            var rect = EditorGUILayout.GetControlRect(true);
            EditorGUI.BeginProperty(rect, guiContent, property);
            property.intValue = EditorGUI.LayerField(rect, guiContent, property.intValue);
            EditorGUI.EndProperty();
          } else if ( isDegrees && field.FieldType == typeof(Photon.Deterministic.FP)) {
            // 2D-rotation is special and needs to go through inversion, if using XZ plane
            var rect = EditorGUILayout.GetControlRect();
            using (new PropertyScope(rect, guiContent, property)) {
              FPPropertyDrawer.DrawAs2DRotation(rect, property, guiContent);
            }
            expanded = false;
          } else {

            if (OnDrawProperty != null) {
              expanded = OnDrawProperty(property, guiContent, field);
            } else {
              var propertyRect = EditorGUILayout.GetControlRect(true, EditorGUI.GetPropertyHeight(property, false));

              if (property.isArray && fixedArrayAttribute?.MinLength == 0 && fixedArrayAttribute?.MaxLength == 1) {
                // implicit optional array
                expanded = DoPropertyFieldWithToggle(propertyRect, property, guiContent, property.arraySize > 0, x => property.arraySize = x ? 1 : 0);
              } else if (!property.isArray && !string.IsNullOrWhiteSpace(optionalPropertyPath)) {
                // explicit optional
                var enabledProperty = FindPropertyRelativeToParent(property, optionalPropertyPath);
                expanded = DoPropertyFieldWithToggle(propertyRect, property, guiContent, enabledProperty.boolValue, x => enabledProperty.boolValue = x);
              } else {
                expanded = EditorGUI.PropertyField(propertyRect, property, guiContent, false);
              }
            }


            if (expanded) {
              if (property.isArray && fixedArrayAttribute != null) {
                var arraySize = Mathf.Clamp(property.arraySize, fixedArrayAttribute.MinLength, fixedArrayAttribute.MaxLength);
                if (arraySize != property.arraySize) {
                  property.arraySize = arraySize;
                }

                if (fixedArrayAttribute.MinLength == fixedArrayAttribute.MaxLength) {
                  // go through the array manually, as we dont want the size
                  expanded = false;
                  using (new EditorGUI.IndentLevelScope(1)) {
                    for (int i = 0; i < property.arraySize; ++i) {
                      var sp = property.GetArrayElementAtIndex(i);
                      PropertyFieldWithQuantumCoreAttributes(sp, filters, false, callback);
                    }
                  }
                } else if (fixedArrayAttribute.MinLength == 0 && fixedArrayAttribute.MaxLength == 1) {
                  expanded = false;
                  using (new EditorGUI.IndentLevelScope(1)) {
                    for (int i = 0; i < property.arraySize; ++i) {
                      var sp = property.GetArrayElementAtIndex(i);
                      PropertyFieldWithQuantumCoreAttributes(sp, filters, true, callback);
                    }
                  }
                }
              } else if (!property.isArray && unionType != null) {
                expanded = false;
                var fieldProperty = DoUnionFieldPopup(property, unionType);
                if (fieldProperty != null) {
                  using (new EditorGUI.IndentLevelScope(1)) {
                    PropertyFieldWithQuantumCoreAttributes(fieldProperty, filters, false, callback);
                  }
                }
              }
            }
          }
        }

        if (EditorGUI.EndChangeCheck()) {
          break;
        }
      }
      while (property.NextVisible(expanded) && property.depth > minDepth);
    } finally {
      EditorGUI.indentLevel = indentLevel;
    }
  }

  private static bool DoPropertyFieldWithToggle(Rect rect, SerializedProperty property, GUIContent label, bool value, Action<bool> assign) {
    var toggleRect = rect.SetLineHeight();

    EditorGUI.BeginChangeCheck();
    using (new PropertyScope(toggleRect, label, property)) {
      value = EditorGUI.Toggle(toggleRect, label, value);
    }
    if (EditorGUI.EndChangeCheck()) {
      assign(value);
    }

    if (value) {
      return EditorGUI.PropertyField(rect, property, WhitespaceContent, false);
    } else {
      return false;
    }
  }

  private static SerializedProperty DoUnionFieldPopup(SerializedProperty unionProperty, Type unionType) {
    var displayName = "Field Used";
    var fieldUsedProperty = unionProperty.FindPropertyRelativeOrThrow(nameof(Quantum.UnionPrototype._field_used_));

    var fields = unionType.GetFields().Where(x => x.DeclaringType != typeof(Quantum.UnionPrototype))
      .OrderBy(x => x.Name)
      .ToArray();

    var values = new[] { "(None)" }.Concat(fields.Select(x => x.Name.ToUpperInvariant())).ToArray();

    var selectedIndex = Array.IndexOf(values, fieldUsedProperty.stringValue);

    // fallback to "(None)" 
    if (selectedIndex < 0 && string.IsNullOrEmpty(fieldUsedProperty.stringValue)) {
      selectedIndex = 0;
    }

    var rect = EditorGUILayout.GetControlRect();
    EditorGUI.BeginProperty(rect, new GUIContent(displayName), fieldUsedProperty);
    EditorGUI.BeginChangeCheck();


    selectedIndex = EditorGUI.Popup(rect, displayName, selectedIndex, values);

    if (EditorGUI.EndChangeCheck()) {
      fieldUsedProperty.stringValue = selectedIndex == 0 ? "" : values[selectedIndex];
    }
    EditorGUI.EndProperty();

    return selectedIndex > 0 ? unionProperty.FindPropertyRelativeOrThrow(fields[selectedIndex - 1].Name) : null;
  }

  private sealed class QuantumEditorGUI : IQuantumEditorGUI {
    public IDisposable BoxScope(string headline = null) {
      return new CustomEditorsHelper.BoxScope(headline);
    }

    public void DrawProperty(SerializedProperty property, string[] filter = null, bool skipRoot = true) {
      CustomEditorsHelper.DrawDefaultInspector(property, filter: filter, skipRoot: skipRoot);
    }

    public void HandleMultiTypeField(SerializedProperty property, params Type[] types) {
      CustomEditorsHelper.HandleMultiTypeField(property, types);
    }
  }
}

