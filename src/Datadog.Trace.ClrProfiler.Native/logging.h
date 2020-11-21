#ifndef DD_CLR_PROFILER_LOGGING_H_
#define DD_CLR_PROFILER_LOGGING_H_
#include "util.h"

#include <spdlog/spdlog.h>

namespace trace {

extern bool debug_logging_enabled;
extern bool dump_il_rewrite_enabled;

class Logger : public Singleton<Logger> {
  friend class Singleton<Logger>;

 private:
  std::shared_ptr<spdlog::logger> m_fileout;
  static std::string GetLogPath();
  Logger();
  ~Logger();

 public:
  void Debug(const std::string& str);
  void Info(const std::string& str);
  void Warn(const std::string& str);
  void Error(const std::string& str);
  void Critical(const std::string& str);
  void Flush();
  static void Shutdown() { spdlog::shutdown(); }
};

template <typename... Args>
void Debug(const Args... args);

template <typename... Args>
void Info(const Args... args);

template <typename... Args>
void Warn(const Args... args);

}  // namespace trace

#endif  // DD_CLR_PROFILER_LOGGING_H_
