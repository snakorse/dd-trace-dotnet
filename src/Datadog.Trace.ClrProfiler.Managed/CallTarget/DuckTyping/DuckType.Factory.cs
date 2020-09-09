using System;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public partial class DuckType
    {
        /// <summary>
        /// Gets a ducktype factory for a base type and instance type
        /// </summary>
        /// <param name="duckType">Duck type</param>
        /// <param name="instanceType">Object type</param>
        /// <returns>Duck type factory</returns>
        public static DuckTypeFactory GetFactoryFor(Type duckType, Type instanceType)
        {
            CreateTypeResult result = GetOrCreateProxyType(duckType, instanceType);
            result.ExceptionInfo?.Throw();
            return new DuckTypeFactory(result.Type);
        }

        /// <summary>
        /// Gets a ducktype factory for a base type and instance type
        /// </summary>
        /// <param name="instanceType">Type of instance</param>
        /// <typeparam name="T">Type of Duck</typeparam>
        /// <returns>Duck Type factory</returns>
        public static DuckTypeFactory<T> GetFactoryFor<T>(Type instanceType)
            where T : class
        {
            CreateTypeResult result = GetOrCreateProxyType(typeof(T), instanceType);
            result.ExceptionInfo?.Throw();
            return new DuckTypeFactory<T>(result.Type);
        }
    }
}
