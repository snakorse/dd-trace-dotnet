#ifndef DD_CLR_PROFILER_SIG_HELPERS_H_
#define DD_CLR_PROFILER_SIG_HELPERS_H_

#include <corhlpr.h>

namespace trace {

bool ParseType(PCCOR_SIGNATURE* p_sig);

}  // namespace trace

#endif  // DD_CLR_PROFILER_SIG_HELPERS_H_