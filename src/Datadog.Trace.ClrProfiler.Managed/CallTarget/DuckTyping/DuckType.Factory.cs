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
            return new DuckTypeFactory(GetOrCreateProxyType(duckType, instanceType));
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
            return new DuckTypeFactory<T>(GetOrCreateProxyType(typeof(T), instanceType));
        }
    }
}
