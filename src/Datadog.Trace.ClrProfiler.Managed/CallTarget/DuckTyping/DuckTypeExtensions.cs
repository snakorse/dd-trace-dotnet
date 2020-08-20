using System;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck type extensions
    /// </summary>
    public static class DuckTypeExtensions
    {
        /// <summary>
        /// Gets the duck type factory for the type implementing a base class or interface T
        /// </summary>
        /// <param name="source">Source type</param>
        /// <typeparam name="T">Base type</typeparam>
        /// <returns>DuckTypeFactory instance</returns>
        public static DuckTypeFactory<T> GetDuckTypeFactory<T>(this Type source)
            where T : class
            => DuckType.GetFactoryFor<T>(source);

        /// <summary>
        /// Gets the duck type factory for the type implementing a base class or interface T
        /// </summary>
        /// <param name="source">Source type</param>
        /// <param name="baseType">Base type</param>
        /// <returns>DuckTypeFactory instance</returns>
        public static DuckTypeFactory GetDuckTypeFactory(this Type source, Type baseType)
            => DuckType.GetFactoryFor(baseType, source);

        /// <summary>
        /// Gets the duck type instance for the object implementing a base class or interface T
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <typeparam name="T">Base type</typeparam>
        /// <returns>DuckType instance</returns>
        public static T As<T>(this object instance)
            where T : class
            => DuckType.Create<T>(instance);

        /// <summary>
        /// Gets the duck type instance for the object implementing a base class or interface T
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="baseType">Base type</param>
        /// <returns>DuckType instance</returns>
        public static object As(this object instance, Type baseType)
            => DuckType.Create(baseType, instance);
    }
}
