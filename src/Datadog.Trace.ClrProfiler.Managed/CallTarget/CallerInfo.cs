using System;

namespace Datadog.Trace.ClrProfiler.CallTarget
{
    /// <summary>
    /// Call target caller info
    /// </summary>
    public readonly struct CallerInfo
    {
        private readonly RuntimeTypeHandle _typeHandle;
        private readonly object _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallerInfo"/> struct.
        /// </summary>
        /// <param name="typeHandle">Type handle</param>
        /// <param name="instance">Object instance</param>
        public CallerInfo(RuntimeTypeHandle typeHandle, object instance)
        {
            _typeHandle = typeHandle;
            _instance = instance;
        }

        /// <summary>
        /// Gets the caller instance
        /// </summary>
        public object Instance => _instance;

        /// <summary>
        /// Gets the caller type
        /// </summary>
        public Type Type => Type.GetTypeFromHandle(_typeHandle);
    }
}
