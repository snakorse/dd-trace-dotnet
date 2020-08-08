using System;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public partial class DuckType
    {
        /// <summary>
        /// Gets a ducktype factory for an interface and instance type
        /// </summary>
        /// <param name="duckType">Duck type</param>
        /// <param name="instanceType">Object type</param>
        /// <returns>Duck type factory</returns>
        public static IDuckTypeFactory GetFactoryByTypes(Type duckType, Type instanceType)
        {
            var type = GetOrCreateProxyType(duckType, instanceType);
            return new DuckTypeFactory(type);
        }

        /// <summary>
        /// Gets a ducktype factory for an interface and instance type
        /// </summary>
        /// <param name="instanceType">Type of instance</param>
        /// <typeparam name="T">Type of Duck</typeparam>
        /// <returns>Duck Type factory</returns>
        public static IDuckTypeFactory<T> GetFactoryByTypes<T>(Type instanceType)
            where T : class
        {
            var duckType = typeof(T);
            var type = GetOrCreateProxyType(duckType, instanceType);
            return new DuckTypeFactory<T>(type);
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
                var inst = (IDuckType)Activator.CreateInstance(_proxyType);
                inst.SetInstance(instance);
                return inst;
            }
        }
    }
}
