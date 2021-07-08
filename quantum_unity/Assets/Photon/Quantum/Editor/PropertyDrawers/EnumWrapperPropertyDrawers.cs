using Quantum;
using UnityEngine;
using UnityEditor;

namespace Quantum.Editor {
    [CustomPropertyDrawer(typeof(RotationFreezeFlags_Wrapper))]
    public class RotationFreezeFlags_WrapperPropertyDrawer : EnumWrapperPropertyDrawerBase<Quantum.RotationFreezeFlags> { }
    
    [CustomPropertyDrawer(typeof(QueryOptions_Wrapper))]
    public class QueryOptions_WrapperPropertyDrawer : EnumWrapperPropertyDrawerBase<Quantum.QueryOptions> { }
    
    [CustomPropertyDrawer(typeof(CallbackFlags_Wrapper))]
    public class CallbackFlags_WrapperPropertyDrawer : EnumWrapperPropertyDrawerBase<Quantum.CallbackFlags> { }
    
    [CustomPropertyDrawer(typeof(PhysicsBody2D.ConfigFlagsWrapper))]
    public class PhysicsBody2DConfigFlags_WrapperPropertyDrawer : EnumWrapperPropertyDrawerBase<Quantum.PhysicsBody2D.ConfigFlags> { }
    
    [CustomPropertyDrawer(typeof(PhysicsBody3D.ConfigFlagsWrapper))]
    public class PhysicsBody3DConfigFlags_WrapperPropertyDrawer : EnumWrapperPropertyDrawerBase<Quantum.PhysicsBody3D.ConfigFlags> { }
}