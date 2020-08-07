using System;
using System.Runtime.InteropServices;

namespace Datadog.Trace.ClrProfiler.CallTarget
{
    /// <summary>
    /// Call target execution state
    /// </summary>
    public readonly struct CallTargetState
    {
        private readonly object _state;
        private readonly bool _executeMethod;
        private readonly bool _rethrowOnException;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallTargetState"/> struct.
        /// </summary>
        /// <param name="state">Object state instance</param>
        public CallTargetState(object state)
        {
            _state = state;
            _executeMethod = true;
            _rethrowOnException = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallTargetState"/> struct.
        /// </summary>
        /// <param name="state">Object state instance</param>
        /// <param name="executeMethod">Indicates if the method should be executed</param>
        public CallTargetState(object state, bool executeMethod)
        {
            _state = state;
            _executeMethod = executeMethod;
            _rethrowOnException = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallTargetState"/> struct.
        /// </summary>
        /// <param name="state">Object state instance</param>
        /// <param name="executeMethod">Indicates if the method should be executed</param>
        /// <param name="rethrowOnException">Indicates if the method should rethrow in case of an exception</param>
        public CallTargetState(object state, bool executeMethod, bool rethrowOnException)
        {
            _state = state;
            _executeMethod = executeMethod;
            _rethrowOnException = rethrowOnException;
        }

        /// <summary>
        /// Gets the CallTarget BeginMethod state
        /// </summary>
        public object State => _state;

        /// <summary>
        /// Gets if the original method should be executed
        /// </summary>
        /// <returns>True if the original method should be executed; otherwise, false.</returns>
        public bool ShouldExecuteMethod() => _executeMethod;

        /// <summary>
        /// Gets if the method should rethrow the exceptions from the original method
        /// </summary>
        /// <returns>True if the method should rethrow the exceptions from the original method; otherwise, false.</returns>
        public bool ShouldRethrowOnException() => _rethrowOnException;
    }
}
