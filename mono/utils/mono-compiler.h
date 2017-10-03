/**
 * \file
 */

#ifndef __UTILS_MONO_COMPILER_H__
#define __UTILS_MONO_COMPILER_H__

/*
 * This file includes macros used in the runtime to encapsulate different
 * compiler behaviours.
 */
#include <config.h>
#if defined(HAVE_UNISTD_H)
#include <unistd.h>
#endif

#ifdef __GNUC__
#define MONO_ATTR_USED __attribute__ ((__used__))
#else
#define MONO_ATTR_USED
#endif

#ifdef __GNUC__
#define MONO_ATTR_FORMAT_PRINTF(fmt_pos,arg_pos) __attribute__ ((__format__(__printf__,fmt_pos,arg_pos)))
#else
#define MONO_ATTR_FORMAT_PRINTF(fmt_pos,arg_pos)
#endif

/* Deal with Microsoft C compiler differences */
#ifdef _MSC_VER

#include <math.h>

#if _MSC_VER < 1800 /* VS 2013 */
#define strtoull _strtoui64
#endif

#include <float.h>
#define trunc(x)	(((x) < 0) ? ceil((x)) : floor((x)))
#if _MSC_VER < 1800 /* VS 2013 */
#define isnan(x)	_isnan(x)
#define isinf(x)	(_isnan(x) ? 0 : (_fpclass(x) == _FPCLASS_NINF) ? -1 : (_fpclass(x) == _FPCLASS_PINF) ? 1 : 0)
#define isnormal(x)	_finite(x)
#endif

#define popen		_popen
#define pclose		_pclose

#include <direct.h>
#define mkdir(x)	_mkdir(x)

#define __func__ __FUNCTION__

#include <BaseTsd.h>
typedef SSIZE_T ssize_t;

/*
 * SSIZE_MAX is not defined in MSVC, so define it here.
 *
 * These values come from MinGW64, and are public domain.
 *
 */
#ifndef SSIZE_MAX
#ifdef _WIN64
#define SSIZE_MAX _I64_MAX
#else
#define SSIZE_MAX INT_MAX
#endif
#endif

#endif /* _MSC_VER */

#ifdef _MSC_VER
// Quiet Visual Studio linker warning, LNK4221, in cases when this source file intentional ends up empty.
#define MONO_EMPTY_SOURCE_FILE(x) void __mono_win32_ ## x ## _quiet_lnk4221 (void) {}
#else
#define MONO_EMPTY_SOURCE_FILE(x)
#endif

#if !defined(_MSC_VER) && !defined(HOST_SOLARIS) && !defined(_WIN32) && !defined(__CYGWIN__) && !defined(MONOTOUCH) && HAVE_VISIBILITY_HIDDEN
#if MONO_LLVM_LOADED
#define MONO_LLVM_INTERNAL MONO_API
#else
#define MONO_LLVM_INTERNAL
#endif
#else
#define MONO_LLVM_INTERNAL 
#endif

/* Used to mark internal functions used by the profiler modules */
#define MONO_PROFILER_API MONO_API

#ifdef __GNUC__
#define MONO_ALWAYS_INLINE __attribute__ ((__always_inline__))
#elif defined(_MSC_VER)
#define MONO_ALWAYS_INLINE __forceinline
#else
#define MONO_ALWAYS_INLINE
#endif

#ifdef __GNUC__
#define MONO_NEVER_INLINE __attribute__ ((__noinline__))
#elif defined(_MSC_VER)
#define MONO_NEVER_INLINE __declspec(noinline)
#else
#define MONO_NEVER_INLINE
#endif

#ifdef __GNUC__
#define MONO_COLD __attribute__ ((__cold__))
#else
#define MONO_COLD
#endif

#if defined (__GNUC__) && defined (__GNUC_MINOR__) && defined (__GNUC_PATCHLEVEL__)
#define MONO_GNUC_VERSION (__GNUC__ * 10000 + __GNUC_MINOR__ * 100 + __GNUC_PATCHLEVEL__)
#endif

#if defined(__has_feature)
#if __has_feature(thread_sanitizer)
#define MONO_HAS_CLANG_THREAD_SANITIZER 1
#else
#define MONO_HAS_CLANG_THREAD_SANITIZER 0
#endif
#else
#define MONO_HAS_CLANG_THREAD_SANITIZER 0
#endif

/* Used to tell Clang's ThreadSanitizer to not report data races that occur within a certain function */
#if MONO_HAS_CLANG_THREAD_SANITIZER
#define MONO_NO_SANITIZE_THREAD __attribute__ ((no_sanitize("thread")))
#else
#define MONO_NO_SANITIZE_THREAD
#endif

/* Used when building with Android NDK's unified headers */
#if defined(HOST_ANDROID)
#if __ANDROID_API__ < 21

typedef int32_t __mono_off32_t;

#ifdef HAVE_SYS_MMAN_H
#include <sys/mman.h>
#endif

#if !defined(mmap)
/* Unified headers before API 21 do not declare mmap when LARGE_FILES are used (via -D_FILE_OFFSET_BITS=64)
 * which is always the case when Mono build targets Android. The problem here is that the unified headers
 * map `mmap` to `mmap64` if large files are enabled but this api exists only in API21 onwards. Therefore
 * we must carefully declare the 32-bit mmap here without changing the ABI along the way. Carefully because
 * in this instance off_t is redeclared to be 64-bit and that's not what we want.
 */
void* mmap (void*, size_t, int, int, int, __mono_off32_t);
#endif /* !mmap */

#ifdef HAVE_SYS_SENDFILE_H
#include <sys/sendfile.h>
#endif

#if !defined(sendfile)
/* The same thing as with mmap happens with sendfile */
ssize_t sendfile (int out_fd, int in_fd, __mono_off32_t* offset, size_t count);
#endif /* !sendfile */

#endif /* __ANDROID_API__ < 21 */
#endif /* HOST_ANDROID */

#endif /* __UTILS_MONO_COMPILER_H__*/

