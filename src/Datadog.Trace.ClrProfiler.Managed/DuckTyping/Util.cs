using System;
using System.Globalization;
using System.Reflection;

namespace Datadog.Trace.ClrProfiler.DuckTyping
{
    /// <summary>
    /// Utilities class
    /// </summary>
    public static class Util
    {
        internal static readonly MethodInfo GetTypeFromHandleMethodInfo = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));
        internal static readonly MethodInfo CheckExpectedTypeMethodInfo = typeof(Util).GetMethod(nameof(Util.CheckExpectedType));
        internal static readonly MethodInfo EnumToObjectMethodInfo = typeof(Enum).GetMethod(nameof(Enum.ToObject), new[] { typeof(Type), typeof(object) });

        /// <summary>
        /// Convert a value to an expected type
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="conversionType">Expected type</param>
        /// <returns>Value with the new type</returns>
        public static object CheckExpectedType(object value, Type conversionType)
        {
            if (value is null && conversionType.IsValueType)
            {
                throw new InvalidCastException();
            }

            return value;
        }
    }
}
