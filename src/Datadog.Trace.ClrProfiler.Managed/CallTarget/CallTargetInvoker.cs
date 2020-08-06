using System;
using System.Collections.Concurrent;
using Datadog.Trace.ClrProfiler.Integrations.Testing;
using Datadog.Trace.Util;

namespace Datadog.Trace.ClrProfiler.CallTarget
{
    /// <summary>
    /// Call target integration helper
    /// </summary>
    public static class CallTargetInvoker
    {
        private static ConcurrentDictionary<RuntimeTypeHandle, IObjectPool> _wrapperPools = new ConcurrentDictionary<RuntimeTypeHandle, IObjectPool>();

        private static IObjectPool GetWrapperPool(RuntimeTypeHandle wrapperTypeHandle)
        {
            return _wrapperPools.GetOrAdd(wrapperTypeHandle, handle =>
            {
                Type wrapperType = Type.GetTypeFromHandle(handle);
                return (IObjectPool)typeof(DefaultObjectPool<>).MakeGenericType(wrapperType).GetField("Shared").GetValue(null);
            });
        }

        /// <summary>
        /// Call target static begin method helper
        /// </summary>
        /// <param name="instanceTypeHandle">Instance type handle</param>
        /// <param name="instance">Object instance</param>
        /// <param name="arguments">Arguments</param>
        /// <param name="wrapperTypeHandle">Wrapper type handle</param>
        /// <returns>CallTargetBeginReturn instance</returns>
        public static CallTargetState BeginMethod(RuntimeTypeHandle instanceTypeHandle, object instance, object[] arguments, RuntimeTypeHandle wrapperTypeHandle)
        {
            var pool = GetWrapperPool(wrapperTypeHandle);
            var state = (CallTargetState)pool.Get();
            state.Init(instanceTypeHandle, instance, pool);
            state.OnStartMethodCall(new ArraySegment<object>(arguments));
            return state;
        }

        /// <summary>
        /// Call target static end method helper
        /// </summary>
        /// <param name="returnValue">Original method return value</param>
        /// <param name="exception">Original method exception</param>
        /// <param name="state">State from the BeginMethod</param>
        /// <returns>Return value</returns>
        public static object EndMethod(object returnValue, Exception exception, CallTargetState state)
        {
            returnValue = state.OnBeforeEndMethodCall(returnValue, exception);
            return AsyncTool.AddContinuation(returnValue, exception, state, (rValue, ex, s) => EndMethodAsync(rValue, ex, s));
        }

        private static object EndMethodAsync(object returnValue, Exception exception, CallTargetState state)
        {
            returnValue = state.OnEndMethodCall(returnValue, exception);
            var pool = state.Pool;
            state.Init(default, null, null);
            pool.Return(state);
            return returnValue;
        }
    }
}
