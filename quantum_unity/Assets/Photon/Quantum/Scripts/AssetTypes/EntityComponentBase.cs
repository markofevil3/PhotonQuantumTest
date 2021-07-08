using System;
using Quantum;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(EntityPrototype))]
public abstract class EntityComponentBase : MonoBehaviour {
  private const string ExpectedTypeNamePrefix = "EntityComponent";

  public abstract System.Type PrototypeType { get; }
  public System.Type ComponentType => ComponentPrototype.PrototypeTypeToComponentType(PrototypeType);

  public virtual void Refresh() {
  }

  public abstract ComponentPrototype CreatePrototype(EntityPrototypeConverter converter);

  public static Type UnityComponentTypeToQuantumPrototypeType(Type type) {
    if (type == null) {
      throw new ArgumentNullException(nameof(type));
    }

    var baseType = type.BaseType;
    if (baseType?.IsGenericType == true &&
        (baseType.GetGenericTypeDefinition() == typeof(EntityComponentBase<>) || baseType.GetGenericTypeDefinition() == typeof(EntityComponentBase<,>))) {
      return baseType.GetGenericArguments()[0];
    } else {
      throw new InvalidOperationException($"Type {type} is not a subclass of {typeof(EntityComponentBase<>)} or {typeof(EntityComponentBase<,>)}");
    }
  }

  public static Type UnityComponentTypeToQuantumComponentType(Type type) => ComponentPrototype.PrototypeTypeToComponentType(UnityComponentTypeToQuantumPrototypeType(type));

#if UNITY_EDITOR

  public virtual void OnInspectorGUI(UnityEditor.SerializedObject so, IQuantumEditorGUI editor) {
    DrawPrototype(so, editor);
    DrawNonPrototypeFields(so, editor);
  }

  protected void DrawPrototype(UnityEditor.SerializedObject so, IQuantumEditorGUI editor) {
    editor.DrawProperty(so.FindPropertyOrThrow("Prototype"), skipRoot: true);
  }

  protected void DrawNonPrototypeFields(UnityEditor.SerializedObject so, IQuantumEditorGUI editor) {
    editor.DrawProperty(so.GetIterator(), filter: new[] { "m_Script", "Prototype" });
  }

#endif
}

public abstract class EntityComponentBase<TPrototype> : EntityComponentBase
  where TPrototype : ComponentPrototype, new() {

  [FormerlySerializedAs("prototype")]
  public TPrototype Prototype = new TPrototype();

  public override System.Type PrototypeType => typeof(TPrototype);

  [Obsolete("Use Prototype field")]
  public TPrototype prototype => Prototype;

  public override ComponentPrototype CreatePrototype(EntityPrototypeConverter converter) {
    return Prototype;
  }
}

public abstract class EntityComponentBase<TPrototype, TAdapter> : EntityComponentBase
  where TPrototype : ComponentPrototype, new()
  where TAdapter : class, IPrototypeAdapter<TPrototype>, new() {

  [FormerlySerializedAs("prototype")]
  public TAdapter Prototype = new TAdapter();

  public override System.Type PrototypeType => typeof(TPrototype);

  [Obsolete("Use Prototype field")]
  public TAdapter prototype => Prototype;

  public override ComponentPrototype CreatePrototype(EntityPrototypeConverter converter) {
    return Prototype.Convert(converter);
  }
}