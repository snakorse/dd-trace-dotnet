using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datadog.Trace.ClrProfiler
{
    /// <summary>
    /// CallTarget state abstract base class
    /// </summary>
    public abstract class CallTargetState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the method should rethrow in case of an exception
        /// </summary>
        protected bool EnableRethrow { get; set; }

        /// <summary>
        /// Gets if the method should rethrow in case on an exception
        /// </summary>
        /// <returns>True if the method should rethrow; otherwise, false</returns>
        public bool ShouldRethrow() => EnableRethrow;
    }
}
