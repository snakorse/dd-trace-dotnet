#ifndef DD_CLR_PROFILER_PAL_H_
#define DD_CLR_PROFILER_PAL_H_

#include "string.h"  // NOLINT

namespace trace {

    WSTRING DatadogLogFilePath();
    WSTRING GetCurrentProcessName();
    int GetPID();

} // namespace trace

#endif  // DD_CLR_PROFILER_PAL_H_
