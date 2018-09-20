cmake_minimum_required (VERSION 2.8.10)

project (mono-openssl)

if(POLICY CMP0026)
cmake_policy(SET CMP0026 NEW)
endif()
if(POLICY CMP0042)
cmake_policy(SET CMP0042 NEW)
endif()

enable_language(C)
enable_language(CXX)

find_program(CCACHE_PROGRAM ccache)
if(CCACHE_PROGRAM)
    set_property(GLOBAL PROPERTY RULE_LAUNCH_COMPILE "${CCACHE_PROGRAM}")
endif()

# FIXME: cmake's asm detection is broken when using xcrun.
set (CMAKE_ASM_COMPILER "${CMAKE_C_COMPILER}")
set (CMAKE_ASM_COMPILER_ARG1 "${CMAKE_C_COMPILER_ARG1}")
set (CMAKE_ASM_COMPILER_ID "${CMAKE_C_COMPILER_ID}")
enable_language(ASM)

if (NOT "${OPENSSL_ARCH}" STREQUAL "")
	message (WARNING "SET ARCH: ${OPENSSL_ARCH}")
	set (CMAKE_SYSTEM_PROCESSOR "${OPENSSL_ARCH}")
endif ()

if (NOT MSVC)
	if(${CMAKE_SYSTEM_NAME} MATCHES "AIX" OR ${CMAKE_SYSTEM_NAME} MATCHES "OS400")
		# GCC+XCOFF doesn't support -fvisibility=hidden, and we would prefer XCOFF debugging info.
		set (C_CXX_FLAGS "-Wall -Wsign-compare -Wmissing-field-initializers -fPIC -gxcoff")
	else()
		set (C_CXX_FLAGS "-Wall -Wsign-compare -Wmissing-field-initializers -fPIC -ggdb -fvisibility=hidden")
	endif()
endif ()

set (CMAKE_C_FLAGS "${CMAKE_C_FLAGS} ${C_CXX_FLAGS} ${OPENSSL_CFLAGS}")
set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} ${C_CXX_FLAGS} ${OPENSSL_CFLAGS}")
set (CMAKE_LD_FLAGS "${CMAKE_LD_FLAGS} ${C_LD_FLAGS} ${OPENSSL_LDFLAGS}")
set (CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} ${OPENSSL_CFLAGS}")
set (CMAKE_MACOSX_RPATH 1)
set (MONO_OPENSSL 1)

set(BUILD_SHARED_LIBS_SAVED "${BUILD_SHARED_LIBS}")
set(BUILD_SHARED_LIBS OFF)
set(BUILD_SHARED_LIBS "${BUILD_SHARED_LIBS_SAVED}")

include_directories (
	${SRC_DIR}
)

set (
	MONO_OPENSSL_SOURCES

	openssl-bio.c
	openssl-bio.h
	openssl-error.c
	openssl-error.h
	openssl-key.c
	openssl-key.h
	openssl-pkcs12.c
	openssl-pkcs12.h
	openssl-rsa.c
	openssl-rsa.h
	openssl-ssl-ctx.c
	openssl-ssl-ctx.h
	openssl-ssl.c
	openssl-ssl.h
	openssl-time64.c
	openssl-util.c
	openssl-util.h
	openssl-x509-chain.c
	openssl-x509-chain.h
	openssl-x509-crl.c
	openssl-x509-crl.h
	openssl-x509-lookup.c
	openssl-x509-lookup.h
	openssl-x509-lookup-mono.c
	openssl-x509-lookup-mono.h
	openssl-x509-name.c
	openssl-x509-name.h
	openssl-x509-revoked.c
	openssl-x509-revoked.h
	openssl-x509-store-ctx.c
	openssl-x509-store-ctx.h
	openssl-x509-store.c
	openssl-x509-store.h
	openssl-x509-verify-param.c
	openssl-x509-verify-param.h
	openssl-x509.c
	openssl-x509.h

	${OPENSSL_OBJECTS}
)

add_library (mono-openssl-shared SHARED ${MONO_OPENSSL_SOURCES})
