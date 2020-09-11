using System;
using System.ComponentModel;

namespace Datadog.Trace.ClrProfiler.CallTarget.DuckTyping
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public partial class DuckType
    {
        /// <summary>
        /// Checks and ensures the arguments for the Create methods
        /// </summary>
        /// <param name="proxyType">Duck type</param>
        /// <param name="instance">Instance value</param>
        /// <exception cref="ArgumentNullException">If the duck type or the instance value is null</exception>
        private static void EnsureArguments(Type proxyType, object instance)
        {
            if (proxyType is null)
            {
                throw new ArgumentNullException(nameof(proxyType), "The proxy type can't be null");
            }

            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance), "The object instance can't be null");
            }

            if (!proxyType.IsPublic && !proxyType.IsNestedPublic)
            {
                throw new DuckTypeTypeIsNotPublicException(proxyType, nameof(proxyType));
            }
        }

        /// <summary>
        /// Get inner IDuckType
        /// </summary>
        /// <param name="field">Field reference</param>
        /// <param name="proxyType">Proxy type</param>
        /// <param name="value">Property value</param>
        /// <returns>DuckType instance</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDuckType GetInnerDuckType(ref IDuckType field, Type proxyType, object value)
        {
            if (value is null)
            {
                field = null;
                return null;
            }

            var valueType = value.GetType();
            if (field is null || field.Type != valueType)
            {
                CreateTypeResult result = GetOrCreateProxyType(proxyType, valueType);
                result.ExceptionInfo?.Throw();
                field = (IDuckType)Activator.CreateInstance(result.ProxyType);
            }

            field.SetInstance(value);
            return field;
        }

        /// <summary>
        /// Set inner DuckType
        /// </summary>
        /// <param name="field">Field reference</param>
        /// <param name="value">Proxy type instance</param>
        /// <returns>Property value</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static object SetInnerDuckType(ref IDuckType field, IDuckType value)
        {
            field = value;
            return field?.Instance;
        }
    }
}
