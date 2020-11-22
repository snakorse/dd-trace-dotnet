#ifndef DD_CLR_PROFILER_INTEGRATION_H_
#define DD_CLR_PROFILER_INTEGRATION_H_

#include <corhlpr.h>
#include <vector>

#include "pal_mstypes.h"
#include "string.h"

#undef major
#undef minor

namespace trace {

    const size_t kPublicKeySize = 8;

    // PublicKey represents an Assembly Public Key token, which is an 8 byte binary
    // RSA key.
    struct PublicKey {
        const BYTE data[kPublicKeySize];

        PublicKey() : data{0} {}
        PublicKey(const BYTE (&arr)[kPublicKeySize])
                : data{arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6], arr[7]} {}

        bool operator==(const PublicKey& other) const;
        WSTRING str() const;
    };

    // Version is an Assembly version in the form Major.Minor.Build.Revision
    // (1.0.0.0)
    struct Version {
        const unsigned short major;
        const unsigned short minor;
        const unsigned short build;
        const unsigned short revision;

        Version() : major(0), minor(0), build(0), revision(0) {}
        Version(const unsigned short major, const unsigned short minor,
                const unsigned short build, const unsigned short revision)
                : major(major), minor(minor), build(build), revision(revision) {}

        bool operator==(const Version& other) const;
        bool operator<(const Version& other) const;
        bool operator>(const Version& other) const;
        WSTRING str() const;
    };

    // An AssemblyReference is a reference to a .Net assembly. In general it will
    // look like:
    //     Some.Assembly.Name, Version=1.0.0.0, Culture=neutral,
    //     PublicKeyToken=abcdef0123456789
    struct AssemblyReference {
        const WSTRING name;
        const Version version;
        const WSTRING locale;
        const PublicKey public_key;

        AssemblyReference(): name(""_W), locale(""_W) {}
        AssemblyReference(const WSTRING& str);

        bool operator==(const AssemblyReference& other) const;
        WSTRING str() const;
    };

    // A MethodSignature is a byte array. The format is:
    // [calling convention, number of parameters, return type, parameter type...]
    // For types see CorElementType
    struct MethodSignature {
    public:
        const std::vector<BYTE> data;

        MethodSignature() {}
        MethodSignature(const std::vector<BYTE>& data) : data(data) {}

        bool operator==(const MethodSignature& other) const;
        CorCallingConvention CallingConvention() const;
        size_t NumberOfTypeArguments() const;
        size_t NumberOfArguments() const;
        bool ReturnTypeIsObject() const;
        size_t IndexOfReturnType() const;
        bool IsInstanceMethod() const;
        WSTRING str() const;
    };

    struct MethodReference {
        const AssemblyReference assembly;
        const WSTRING type_name;
        const WSTRING method_name;
        const WSTRING action;
        const MethodSignature method_signature;
        const Version min_version;
        const Version max_version;
        const std::vector<WSTRING> signature_types;

        MethodReference()
                : type_name(""_W),
                  method_name(""_W),
                  action(""_W),
                  min_version(Version(0, 0, 0, 0)),
                  max_version(Version(USHRT_MAX, USHRT_MAX, USHRT_MAX, USHRT_MAX)) {}

        MethodReference(const WSTRING& assembly_name, WSTRING type_name, WSTRING method_name,
                        WSTRING action, Version min_version, Version max_version,
                        const std::vector<BYTE>& method_signature,
                        const std::vector<WSTRING>& signature_types)
                : assembly(assembly_name),
                  type_name(type_name),
                  method_name(method_name),
                  action(action),
                  method_signature(method_signature),
                  min_version(min_version),
                  max_version(max_version),
                  signature_types(signature_types) {}

        WSTRING get_type_cache_key() const;
        WSTRING get_method_cache_key() const;
        bool operator==(const MethodReference& other) const;
    };

    struct MethodReplacement {
        const MethodReference caller_method;
        const MethodReference target_method;
        const MethodReference wrapper_method;

        MethodReplacement() {}

        MethodReplacement(MethodReference caller_method,
                          MethodReference target_method,
                          MethodReference wrapper_method)
                : caller_method(caller_method),
                  target_method(target_method),
                  wrapper_method(wrapper_method) {}

        bool operator==(const MethodReplacement& other) const;
    };

    struct Integration {
        const WSTRING integration_name;
        std::vector<MethodReplacement> method_replacements;

        Integration() : integration_name(""_W), method_replacements({}) {}

        Integration(WSTRING integration_name,
                    std::vector<MethodReplacement> method_replacements)
                : integration_name(integration_name),
                  method_replacements(method_replacements) {}

        bool operator==(const Integration& other) const;
    };

    struct IntegrationMethod {
        const WSTRING integration_name;
        MethodReplacement replacement;

        IntegrationMethod() : integration_name(""_W), replacement({}) {}

        IntegrationMethod(WSTRING integration_name, MethodReplacement replacement)
                : integration_name(integration_name), replacement(replacement) {}

        bool operator==(const IntegrationMethod& other) const;
    };

    namespace {

        WSTRING GetNameFromAssemblyReferenceString(const WSTRING& wstr);
        Version GetVersionFromAssemblyReferenceString(const WSTRING& wstr);
        WSTRING GetLocaleFromAssemblyReferenceString(const WSTRING& wstr);
        PublicKey GetPublicKeyFromAssemblyReferenceString(const WSTRING& wstr);

    }  // namespace

}  // namespace trace

#endif  // DD_CLR_PROFILER_INTEGRATION_H_
