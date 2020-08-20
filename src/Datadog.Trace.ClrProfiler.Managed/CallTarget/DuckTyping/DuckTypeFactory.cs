using System;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck type factory
    /// </summary>
    public readonly struct DuckTypeFactory
    {
        private readonly Type _proxyType;

        internal DuckTypeFactory(Type proxyType)
        {
            _proxyType = proxyType;
        }

        /// <summary>
        /// Create duck type proxy instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Duck type proxy instance</returns>
        public IDuckType Create(object instance)
        {
            var inst = (IDuckType)Activator.CreateInstance(_proxyType);
            inst.SetInstance(instance);
            return inst;
        }
    }

    /// <summary>
    /// Duck type factory
    /// </summary>
    /// <typeparam name="T">Base class or interface for duck type</typeparam>
    public readonly struct DuckTypeFactory<T>
        where T : class
    {
        private readonly Type _proxyType;

        internal DuckTypeFactory(Type proxyType)
        {
            _proxyType = proxyType;
        }

        /// <summary>
        /// Create duck type proxy instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Duck type proxy instance</returns>
        public T Create(object instance)
        {
            var inst = (IDuckType)Activator.CreateInstance(_proxyType);
            inst.SetInstance(instance);
            return inst as T;
        }
    }
}
