using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datadog.Trace.Logging;

namespace Datadog.Trace.ClrProfiler
{
    /// <summary>
    /// End method delegate
    /// </summary>
    /// <param name="returnValue">Original return value</param>
    /// <param name="ex">Captured exception</param>
    /// <returns>Final return value</returns>
    public delegate object EndMethodDelegate(object returnValue, Exception ex);

    /// <summary>
    /// Call target begin method return type
    /// </summary>
    public class CallTargetBeginReturn
    {
        /// <summary>
        /// Noop default instance
        /// </summary>
        public static readonly CallTargetBeginReturn NoopInstance = new CallTargetBeginReturn();

        private EndMethodDelegate _endMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallTargetBeginReturn"/> class.
        /// </summary>
        public CallTargetBeginReturn()
        {
        }

        /// <summary>
        /// End Method delegate
        /// </summary>
        /// <param name="returnValue">Method return value</param>
        /// <param name="exception">Method exception</param>
        /// <returns>Return value</returns>
        public object EndMethod(object returnValue, object exception)
        {
            if (_endMethod is null)
            {
                return returnValue;
            }

            return _endMethod(returnValue, (Exception)exception);
        }

        /// <summary>
        /// Sets the end method delegate to the instance
        /// </summary>
        /// <param name="delegate">EndMethodDelegate instance</param>
        public void SetDelegate(EndMethodDelegate @delegate)
        {
            _endMethod = @delegate;
        }
    }
}
