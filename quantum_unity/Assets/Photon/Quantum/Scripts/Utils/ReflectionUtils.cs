using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Quantum {

  public static class ReflectionUtils {
    public const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

#if UNITY_EDITOR
    public static T CreateEditorMethodDelegate<T>(string editorAssemblyTypeName, string methodName, BindingFlags flags = DefaultBindingFlags) where T : Delegate {
      return CreateMethodDelegate<T>(typeof(UnityEditor.Editor).Assembly, editorAssemblyTypeName, methodName, flags);
    }
#endif

    public static T CreateMethodDelegate<T>(this Type type, string methodName, BindingFlags flags = DefaultBindingFlags) where T : Delegate {
      try {
        return CreateMethodDelegateInternal<T>(type, methodName, flags);
      } catch (System.Exception ex) {
        throw new InvalidOperationException(CreateExceptionMessage<T>(type.Assembly, type.FullName, methodName, flags), ex);
      }
    }

    public static Delegate CreateMethodDelegate(this Type type, string methodName, BindingFlags flags, Type delegateType) {
      try {
        return CreateMethodDelegateInternal(type, methodName, flags, delegateType);
      } catch (System.Exception ex) {
        throw new InvalidOperationException(CreateExceptionMessage(type.Assembly, type.FullName, methodName, flags, delegateType), ex);
      }
    }

    public static T CreateMethodDelegate<T>(Assembly assembly, string typeName, string methodName, BindingFlags flags = DefaultBindingFlags) where T : Delegate {
      try {
        var type = assembly.GetType(typeName, true);
        return CreateMethodDelegateInternal<T>(type, methodName, flags);
      } catch (System.Exception ex) {
        throw new InvalidOperationException(CreateExceptionMessage<T>(assembly, typeName, methodName, flags), ex);
      }
    }

    public static FieldInfo GetFieldOrThrow(this Type type, string fieldName, BindingFlags flags = DefaultBindingFlags) {
      var field = type.GetField(fieldName, flags);
      if (field == null) {
        throw new ArgumentOutOfRangeException(CreateExceptionMessage(type.Assembly, type.FullName, fieldName, flags));
      }
      return field;
    }
    private static string CreateExceptionMessage<T>(Assembly assembly, string typeName, string methodName, BindingFlags flags) {
      return CreateExceptionMessage(assembly, typeName, methodName, flags, typeof(T));
    }

    private static string CreateExceptionMessage(Assembly assembly, string typeName, string methodName, BindingFlags flags, Type delegateType) {
      return $"{assembly.FullName}.{typeName}.{methodName} with flags: {flags} and type: {delegateType}";
    }

    private static string CreateExceptionMessage(Assembly assembly, string typeName, string fieldName, BindingFlags flags) {
      return $"{assembly.FullName}.{typeName}.{fieldName} with flags: {flags}";
    }

    private static T CreateMethodDelegateInternal<T>(this Type type, string name, BindingFlags flags) where T : Delegate {
      return (T)CreateMethodDelegateInternal(type, name, flags, typeof(T));
    }

    private static Delegate CreateMethodDelegateInternal(this Type type, string name, BindingFlags flags, Type delegateType) {

      IEnumerable<ParameterInfo> delegateParamters = delegateType.GetMethod("Invoke").GetParameters();

      if (!flags.HasFlag(BindingFlags.Static)) {
        delegateParamters = delegateParamters.Skip(1);
      }
        

      var method = type.GetMethod(name, flags, null, delegateParamters.Select(x => x.ParameterType).ToArray(), null);

      if (method == null) {
        throw new ArgumentOutOfRangeException($"No method found matching name \"{name}\", signature \"{delegateType}\" and flags \"{flags}\"");
      }

      return System.Delegate.CreateDelegate(delegateType, null, method);
    }
  }
}