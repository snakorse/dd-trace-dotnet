using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datadog.Trace.Util
{
    /// <summary>
    /// Generic object pool interface
    /// </summary>
    internal interface IObjectPool
    {
        object Get();

        void Return(object value);
    }
}
