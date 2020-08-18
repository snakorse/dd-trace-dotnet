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
        public static IDuckTypeFactory GetFactoryFor(Type duckType, Type instanceType)
        {
            return new DuckTypeFactory(GetOrCreateProxyType(duckType, instanceType));
        }

        /// <summary>
        /// Gets a ducktype factory for a base type and instance type
        /// </summary>
        /// <param name="instanceType">Type of instance</param>
        /// <typeparam name="T">Type of Duck</typeparam>
        /// <returns>Duck Type factory</returns>
        public static IDuckTypeFactory<T> GetFactoryFor<T>(Type instanceType)
            where T : class
        {
            return new DuckTypeFactory<T>(GetOrCreateProxyType(typeof(T), instanceType));
        }

        private class DuckTypeFactory : IDuckTypeFactory
        {
            private readonly Type _proxyType;

            internal DuckTypeFactory(Type proxyType)
            {
                _proxyType = proxyType;
            }

            public IDuckType Create(object instance)
            {
                var inst = (IDuckType)Activator.CreateInstance(_proxyType);
                inst.SetInstance(instance);
                return inst;
            }
        }

        private class DuckTypeFactory<T> : IDuckTypeFactory<T>, IDuckTypeFactory
            where T : class
        {
            private readonly Type _proxyType;

            internal DuckTypeFactory(Type proxyType)
            {
                _proxyType = proxyType;
            }

            public T Create(object instance)
            {
                var inst = (IDuckType)Activator.CreateInstance(_proxyType);
                inst.SetInstance(instance);
                return inst as T;
            }

            IDuckType IDuckTypeFactory.Create(object instance)
            {
                return (IDuckType)Create(instance);
            }
        }
    }
}
