cmake_minimum_required (VERSION 3.8)
cmake_policy(SET CMP0015 NEW)

option(OSX "Setting compilation for OSX" OFF)

project("Datadog.Trace.ClrProfiler.Native" VERSION 1.21.0)

SET(MANAGED_LOADER_DIRECTORY
    ${CMAKE_SOURCE_DIR}/../bin/ProfilerResources/netcoreapp2.0
)

if (OSX)
    message(STATUS "Preparing OSX build.")

    add_compile_options(-std=c++11 -stdlib=libc++ -fPIC -fms-extensions)
    add_compile_options(-DBIT64 -DPAL_STDCPP_COMPAT -DPLATFORM_UNIX=1 -DUNICODE -DOSX)
    add_compile_options(-Wno-invalid-noreturn -Wno-macro-redefined -Wno-pragma-pack)

    add_custom_command(
            OUTPUT Datadog.Trace.ClrProfiler.Managed.Loader.dll.o
            COMMAND touch stub.c && gcc -o stub.o -c stub.c && cp "${MANAGED_LOADER_DIRECTORY}/Datadog.Trace.ClrProfiler.Managed.Loader.dll" Datadog.Trace.ClrProfiler.Managed.Loader.dll && ld -r -o Datadog.Trace.ClrProfiler.Managed.Loader.dll.o -sectcreate binary dll Datadog.Trace.ClrProfiler.Managed.Loader.dll stub.o
            DEPENDS ${MANAGED_LOADER_DIRECTORY}/Datadog.Trace.ClrProfiler.Managed.Loader.dll
    )

    add_custom_command(
            OUTPUT Datadog.Trace.ClrProfiler.Managed.Loader.pdb.o
            COMMAND touch stub.c && gcc -o stub.o -c stub.c && cp "${MANAGED_LOADER_DIRECTORY}/Datadog.Trace.ClrProfiler.Managed.Loader.pdb" Datadog.Trace.ClrProfiler.Managed.Loader.pdb && ld -r -o Datadog.Trace.ClrProfiler.Managed.Loader.pdb.o -sectcreate binary pdb Datadog.Trace.ClrProfiler.Managed.Loader.pdb stub.o
            DEPENDS ${MANAGED_LOADER_DIRECTORY}/Datadog.Trace.ClrProfiler.Managed.Loader.pdb
    )
    SET(GENERATED_OBJ_FILES
        ${CMAKE_BINARY_DIR}/Datadog.Trace.ClrProfiler.Managed.Loader.dll.o
        ${CMAKE_BINARY_DIR}/Datadog.Trace.ClrProfiler.Managed.Loader.pdb.o
    )
    SET_SOURCE_FILES_PROPERTIES(
        ${GENERATED_OBJ_FILES}
        PROPERTIES
        EXTERNAL_OBJECT false
        GENERATED true
    )

    include_directories("Datadog.Trace.ClrProfiler.Native.static"
            ./lib/coreclr/src/pal/inc/rt
            ./lib/coreclr/src/pal/prebuilt/inc
            ./lib/coreclr/src/pal/inc
            ./lib/coreclr/src/inc
            ./lib/spdlog_x64-linux/include
            /opt/fmt/include
            $<BUILD_INTERFACE:/opt/re2>
            $<BUILD_INTERFACE:/opt/json/include>
    )

    add_library("Datadog.Trace.ClrProfiler.Native.static" STATIC
        class_factory.cpp
        clr_helpers.cpp
        cor_profiler_base.cpp
        cor_profiler.cpp
        il_rewriter_wrapper.cpp
        il_rewriter.cpp
        integration_loader.cpp
        integration.cpp
        logging.cpp
        metadata_builder.cpp
        miniutf.cpp
        sig_helpers.cpp
        string.cpp
        util.cpp
        ${GENERATED_OBJ_FILES}
    )

    set_target_properties("Datadog.Trace.ClrProfiler.Native.static" PROPERTIES PREFIX "")

    target_link_libraries("Datadog.Trace.ClrProfiler.Native.static"
            /opt/re2/obj/libre2.a
            /opt/fmt/libfmt.a
    )

    add_library("Datadog.Trace.ClrProfiler.Native" SHARED
            dllmain.cpp
            interop.cpp
            ${GENERATED_OBJ_FILES}
    )

else()
    message(STATUS "Preparing Linux build.")

    add_compile_options(-std=c++11 -fPIC -fms-extensions)
    add_compile_options(-DBIT64 -DPAL_STDCPP_COMPAT -DPLATFORM_UNIX -DUNICODE -DLINUX)
    add_compile_options(-Wno-invalid-noreturn -Wno-macro-redefined -Wno-pragmas)

    add_custom_command(
        OUTPUT Datadog.Trace.ClrProfiler.Managed.Loader.dll.o
        COMMAND cp "${MANAGED_LOADER_DIRECTORY}/Datadog.Trace.ClrProfiler.Managed.Loader.dll" Datadog.Trace.ClrProfiler.Managed.Loader.dll && ld -r -b binary -o Datadog.Trace.ClrProfiler.Managed.Loader.dll.o Datadog.Trace.ClrProfiler.Managed.Loader.dll
        DEPENDS ${MANAGED_LOADER_DIRECTORY}/Datadog.Trace.ClrProfiler.Managed.Loader.dll
    )
    add_custom_command(
        OUTPUT Datadog.Trace.ClrProfiler.Managed.Loader.pdb.o
        COMMAND cp "${MANAGED_LOADER_DIRECTORY}/Datadog.Trace.ClrProfiler.Managed.Loader.pdb" Datadog.Trace.ClrProfiler.Managed.Loader.pdb && ld -r -b binary -o Datadog.Trace.ClrProfiler.Managed.Loader.pdb.o Datadog.Trace.ClrProfiler.Managed.Loader.pdb
        DEPENDS ${MANAGED_LOADER_DIRECTORY}/Datadog.Trace.ClrProfiler.Managed.Loader.pdb
    )
    SET(GENERATED_OBJ_FILES
        ${CMAKE_BINARY_DIR}/Datadog.Trace.ClrProfiler.Managed.Loader.dll.o
        ${CMAKE_BINARY_DIR}/Datadog.Trace.ClrProfiler.Managed.Loader.pdb.o
    )
    SET_SOURCE_FILES_PROPERTIES(
        ${GENERATED_OBJ_FILES}
        PROPERTIES
        EXTERNAL_OBJECT false
        GENERATED true
    )

    add_library("Datadog.Trace.ClrProfiler.Native.static" STATIC
        class_factory.cpp
        clr_helpers.cpp
        cor_profiler_base.cpp
        cor_profiler.cpp
        il_rewriter_wrapper.cpp
        il_rewriter.cpp
        integration_loader.cpp
        integration.cpp
        logging.cpp
        metadata_builder.cpp
        miniutf.cpp
        sig_helpers.cpp
        string.cpp
        util.cpp
        ${GENERATED_OBJ_FILES}
    )
    set_target_properties("Datadog.Trace.ClrProfiler.Native.static" PROPERTIES PREFIX "")
    target_include_directories("Datadog.Trace.ClrProfiler.Native.static"
        PUBLIC lib/coreclr/src/pal/inc/rt
        PUBLIC lib/coreclr/src/pal/prebuilt/inc
        PUBLIC lib/coreclr/src/pal/inc
        PUBLIC lib/coreclr/src/inc
        PUBLIC lib/spdlog_x64-linux/include
        PUBLIC lib/fmt_x64-linux/include
        PUBLIC $<BUILD_INTERFACE:/opt/re2>
        PUBLIC $<BUILD_INTERFACE:/opt/json/include>
    )
    target_link_libraries("Datadog.Trace.ClrProfiler.Native.static"
        /opt/re2/obj/libre2.a
        /opt/fmt/libfmt.a
        -static-libgcc
        -static-libstdc++
    )

    add_library("Datadog.Trace.ClrProfiler.Native" SHARED
        dllmain.cpp
        interop.cpp
    )

endif(OSX)

set_target_properties("Datadog.Trace.ClrProfiler.Native" PROPERTIES PREFIX "")
target_link_libraries("Datadog.Trace.ClrProfiler.Native" "Datadog.Trace.ClrProfiler.Native.static")

unset(OSX CACHE)