using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datadog.Trace.Util;

namespace Datadog.Trace.ClrProfiler.CallTarget
{
    /// <summary>
    /// CallTarget state abstract base class
    /// </summary>
    public abstract class CallTargetState
    {
        private RuntimeTypeHandle _instanceTypeHandle;
        private Type _instanceType;
        private object _instance;
        private IObjectPool _pool;

        internal IObjectPool Pool => _pool;

        /// <summary>
        /// Gets or sets a value indicating whether the method should rethrow in case of an exception
        /// </summary>
        protected bool EnableRethrow { get; set; } = true;

        /// <summary>
        /// Gets the current instance type handle
        /// </summary>
        protected Type InstanceType
        {
            get
            {
                if (_instanceType is null)
                {
                    _instanceType = Type.GetTypeFromHandle(_instanceTypeHandle);
                }

                return _instanceType;
            }
        }

        /// <summary>
        /// Gets the current instance
        /// </summary>
        protected object Instance => _instance;

        /// <summary>
        /// Gets if the method should rethrow in case on an exception
        /// </summary>
        /// <returns>True if the method should rethrow; otherwise, false</returns>
        public bool ShouldRethrow() => EnableRethrow;

        internal void Init(RuntimeTypeHandle instanceTypeHandle, object instance, IObjectPool pool)
        {
            _instanceType = null;
            _instanceTypeHandle = instanceTypeHandle;
            _instance = instance;
            _pool = pool;
        }

        /// <summary>
        /// Start method call
        /// </summary>
        /// <param name="args">Original method arguments</param>
        public abstract void OnStartMethodCall(ArraySegment<object> args);

        /// <summary>
        /// Before end method call continuations
        /// </summary>
        /// <param name="returnValue">Original return value</param>
        /// <param name="exception">Original exception</param>
        /// <returns>Return value</returns>
        public virtual object OnBeforeEndMethodCall(object returnValue, Exception exception)
        {
            return returnValue;
        }

        /// <summary>
        /// End method call
        /// </summary>
        /// <param name="returnValue">Original return value</param>
        /// <param name="exception">Original exception</param>
        /// <returns>Return value</returns>
        public abstract object OnEndMethodCall(object returnValue, Exception exception);
    }
}
