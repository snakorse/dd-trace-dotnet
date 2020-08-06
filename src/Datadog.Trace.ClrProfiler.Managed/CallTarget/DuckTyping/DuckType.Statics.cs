using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public partial class DuckType
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly MethodInfo GetTypeFromHandleMethodInfo = typeof(Type).GetMethod("GetTypeFromHandle");
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly MethodInfo DuckTypeCreate = typeof(DuckType).GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Type), typeof(object) }, null);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly ConcurrentDictionary<ValueTuple<Type, Type>, Type> DuckTypeCache = new ConcurrentDictionary<ValueTuple<Type, Type>, Type>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly ConcurrentBag<DynamicMethod> DynamicMethods = new ConcurrentBag<DynamicMethod>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly MethodInfo GetInnerDuckTypeMethodInfo = typeof(DuckType).GetMethod("GetInnerDuckType", BindingFlags.Static | BindingFlags.NonPublic);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly MethodInfo SetInnerDuckTypeMethodInfo = typeof(DuckType).GetMethod("SetInnerDuckType", BindingFlags.Static | BindingFlags.NonPublic);
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly ConcurrentDictionary<ValueTuple<string, TypeBuilder>, FieldInfo> DynamicFields = new ConcurrentDictionary<ValueTuple<string, TypeBuilder>, FieldInfo>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static Func<DynamicMethod, RuntimeMethodHandle> _dynamicGetMethodDescriptor;

        private static RuntimeMethodHandle GetRuntimeHandle(DynamicMethod dynamicMethod)
        {
            _dynamicGetMethodDescriptor ??= (Func<DynamicMethod, RuntimeMethodHandle>)typeof(DynamicMethod)
                .GetMethod("GetMethodDescriptor", BindingFlags.NonPublic | BindingFlags.Instance)
                .CreateDelegate(typeof(Func<DynamicMethod, RuntimeMethodHandle>));
            return _dynamicGetMethodDescriptor(dynamicMethod);
        }
    }
}
