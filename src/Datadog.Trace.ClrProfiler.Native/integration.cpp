#include "integration.h"

#include <sstream>
#include <iomanip>

#ifdef _WIN32
#include <regex>
#else
#include <re2/re2.h>
#endif

#include "util.h"

namespace trace {

    bool PublicKey::operator==(const PublicKey& other) const {
        for (int i = 0; i < kPublicKeySize; i++) {
            if (data[i] != other.data[i]) {
                return false;
            }
        }
        return true;
    }
    WSTRING PublicKey::str() const {
        std::stringstream ss;
        for (int i = 0; i < kPublicKeySize; i++) {
            ss << std::setfill('0') << std::setw(2) << std::hex
               << static_cast<int>(data[i]);
        }
        return ToWSTRING(ss.str());
    }

    // ***

    bool Version::operator==(const Version& other) const {
        return major == other.major && minor == other.minor &&
               build == other.build && revision == other.revision;
    }
    bool Version::operator<(const Version& other) const {
        if (major < other.major) {
            return true;
        }
        if (major == other.major && minor < other.minor) {
            return true;
        }
        if (major == other.major && minor == other.minor && build < other.build) {
            return true;
        }
        return false;
    }
    bool Version::operator>(const Version& other) const {
        if (major > other.major) {
            return true;
        }
        if (major == other.major && minor > other.minor) {
            return true;
        }
        if (major == other.major && minor == other.minor && build > other.build) {
            return true;
        }
        return false;
    }
    WSTRING Version::str() const {
        std::stringstream  ss;
        ss << major << "." << minor << "." << build << "." << revision;
        return ToWSTRING(ss.str());
    }

    // ***

    AssemblyReference::AssemblyReference(const WSTRING& str)
            : name(GetNameFromAssemblyReferenceString(str)),
              version(GetVersionFromAssemblyReferenceString(str)),
              locale(GetLocaleFromAssemblyReferenceString(str)),
              public_key(GetPublicKeyFromAssemblyReferenceString(str)) {}

    bool AssemblyReference::operator==(const AssemblyReference& other) const {
        return name == other.name && version == other.version &&
               locale == other.locale && public_key == other.public_key;
    }
    WSTRING AssemblyReference::str() const {
        return name + ", Version="_W + version.str() + ", Culture="_W + locale
        + ", PublicKeyToken="_W + public_key.str();
    }

    // ***

    bool MethodSignature::operator==(const MethodSignature& other) const {
        return data == other.data;
    }
    CorCallingConvention MethodSignature::CallingConvention() const {
        return CorCallingConvention(data.empty() ? 0 : data[0]);
    }
    size_t MethodSignature::NumberOfTypeArguments() const {
        if (data.size() > 1 &&
            (CallingConvention() & IMAGE_CEE_CS_CALLCONV_GENERIC) != 0) {
            return data[1];
        }
        return 0;
    }
    size_t MethodSignature::NumberOfArguments() const {
        if (data.size() > 2 &&
            (CallingConvention() & IMAGE_CEE_CS_CALLCONV_GENERIC) != 0) {
            return data[2];
        }
        if (data.size() > 1) {
            return data[1];
        }
        return 0;
    }
    bool MethodSignature::ReturnTypeIsObject() const {
        if (data.size() > 2 &&
            (CallingConvention() & IMAGE_CEE_CS_CALLCONV_GENERIC) != 0) {
            return data[3] == ELEMENT_TYPE_OBJECT;
        }
        if (data.size() > 1) {
            return data[2] == ELEMENT_TYPE_OBJECT;
        }

        return false;
    }
    size_t MethodSignature::IndexOfReturnType() const {
        if (data.size() > 2 &&
            (CallingConvention() & IMAGE_CEE_CS_CALLCONV_GENERIC) != 0) {
            return 3;
        }
        if (data.size() > 1) {
            return 2;
        }
        return 0;
    }
    bool MethodSignature::IsInstanceMethod() const {
        return (CallingConvention() & IMAGE_CEE_CS_CALLCONV_HASTHIS) != 0;
    }
    WSTRING MethodSignature::str() const {
        std::stringstream ss;
        for (auto& b : data) {
            ss << std::hex << std::setfill('0') << std::setw(2) << static_cast<int>(b);
        }
        return ToWSTRING(ss.str());
    }

    // ***

    WSTRING MethodReference::get_type_cache_key() const {
        return "["_W + assembly.name + "]"_W + type_name + "_vMin_"_W +
               min_version.str() + "_vMax_"_W + max_version.str();
    }
    WSTRING MethodReference::get_method_cache_key() const {
        return "["_W + assembly.name + "]"_W + type_name + "."_W + method_name +
               "_vMin_"_W + min_version.str() + "_vMax_"_W + max_version.str();
    }
    bool MethodReference::operator==(const MethodReference& other) const {
        return assembly == other.assembly && type_name == other.type_name &&
               min_version == other.min_version &&
               max_version == other.max_version &&
               method_name == other.method_name &&
               method_signature == other.method_signature;
    }

    // ***

    bool MethodReplacement::operator==(const MethodReplacement& other) const {
        return caller_method == other.caller_method &&
               target_method == other.target_method &&
               wrapper_method == other.wrapper_method;
    }

    // ***

    bool Integration::operator==(const Integration& other) const {
        return integration_name == other.integration_name &&
               method_replacements == other.method_replacements;
    }

    // ***

    bool IntegrationMethod::operator==(const IntegrationMethod& other) const {
        return integration_name == other.integration_name &&
               replacement == other.replacement;
    }

    namespace {

        WSTRING GetNameFromAssemblyReferenceString(const WSTRING& wstr) {
            WSTRING name = wstr;

            auto pos = name.find(','_W);
            if (pos != WSTRING::npos) {
                name = name.substr(0, pos);
            }

            // strip spaces
            pos = name.rfind(' '_W);
            if (pos != WSTRING::npos) {
                name = name.substr(0, pos);
            }

            return name;
        }

        Version GetVersionFromAssemblyReferenceString(const WSTRING& str) {
            unsigned short major = 0;
            unsigned short minor = 0;
            unsigned short build = 0;
            unsigned short revision = 0;

#ifdef _WIN32

            static auto re =
                std::wregex("Version=([0-9]+)\\.([0-9]+)\\.([0-9]+)\\.([0-9]+)"_W);

            std::wsmatch match;
            if (std::regex_search(str, match, re) && match.size() == 5) {
              WSTRINGSTREAM(match.str(1)) >> major;
              WSTRINGSTREAM(match.str(2)) >> minor;
              WSTRINGSTREAM(match.str(3)) >> build;
              WSTRINGSTREAM(match.str(4)) >> revision;
            }

#else

            static re2::RE2 re("Version=([0-9]+)\\.([0-9]+)\\.([0-9]+)\\.([0-9]+)",
                               RE2::Quiet);
            re2::RE2::FullMatch(ToString(str), re, &major, &minor, &build, &revision);

#endif

            return {major, minor, build, revision};
        }

        WSTRING GetLocaleFromAssemblyReferenceString(const WSTRING& str) {
            WSTRING locale = "neutral"_W;

#ifdef _WIN32

            static auto re = std::wregex("Culture=([a-zA-Z0-9]+)"_W);
            std::wsmatch match;
            if (std::regex_search(str, match, re) && match.size() == 2) {
              locale = match.str(1);
            }

#else

            static re2::RE2 re("Culture=([a-zA-Z0-9]+)", RE2::Quiet);

            std::string match;
            if (re2::RE2::FullMatch(ToString(str), re, &match)) {
                locale = ToWSTRING(match);
            }

#endif

            return locale;
        }

        PublicKey GetPublicKeyFromAssemblyReferenceString(const WSTRING& str) {
            BYTE data[8] = {0};

#ifdef _WIN32

            static auto re = std::wregex("PublicKeyToken=([a-fA-F0-9]{16})"_W);
            std::wsmatch match;
            if (std::regex_search(str, match, re) && match.size() == 2) {
              for (int i = 0; i < 8; i++) {
                auto s = match.str(1).substr(i * 2, 2);
                unsigned long x;
                WSTRINGSTREAM(s) >> std::hex >> x;
                data[i] = BYTE(x);
              }
            }

#else

            static re2::RE2 re("PublicKeyToken=([a-fA-F0-9]{16})");
            std::string match;
            if (re2::RE2::FullMatch(ToString(str), re, &match)) {
                for (int i = 0; i < 8; i++) {
                    auto s = match.substr(i * 2, 2);
                    unsigned long x;
                    std::stringstream(s) >> std::hex >> x;
                    data[i] = BYTE(x);
                }
            }

#endif

            return PublicKey(data);
        }

    }  // namespace

}  // namespace trace
