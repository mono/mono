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
#include <float.h>

#define popen		_popen
#define pclose		_pclose
#include <direct.h>
#define mkdir(x)	_mkdir(x)

#define __func__ __FUNCTION__

#include <stddef.h>
#include <stdint.h>

// ssize_t and SSIZE_MAX are Posix, define for Windows.
typedef ptrdiff_t ssize_t;
#ifndef SSIZE_MAX
#define SSIZE_MAX INTPTR_MAX
#endif

#endif /* _MSC_VER */

#ifdef _MSC_VER
// Quiet Visual Studio linker warning, LNK4221, in cases when this source file intentional ends up empty.
#define MONO_EMPTY_SOURCE_FILE(x) void __mono_win32_ ## x ## _quiet_lnk4221 (void) {}
#else
#define MONO_EMPTY_SOURCE_FILE(x)
#endif

#ifdef _MSC_VER
#define MONO_PRAGMA_WARNING_PUSH() __pragma(warning (push))
#define MONO_PRAGMA_WARNING_DISABLE(x) __pragma(warning (disable:x))
#define MONO_PRAGMA_WARNING_POP() __pragma(warning (pop))
#else
#define MONO_PRAGMA_WARNING_PUSH()
#define MONO_PRAGMA_WARNING_DISABLE(x)
#define MONO_PRAGMA_WARNING_POP()
#endif

#if !defined(_MSC_VER) && !defined(HOST_SOLARIS) && !defined(_WIN32) && !defined(__CYGWIN__) && !defined(MONOTOUCH) && HAVE_VISIBILITY_HIDDEN
#if MONO_LLVM_LOADED
#define MONO_LLVM_INTERNAL MONO_API_NO_EXTERN_C
#else
#define MONO_LLVM_INTERNAL
#endif
#else
#define MONO_LLVM_INTERNAL 
#endif

/* Used to mark internal functions used by the profiler modules */
#define MONO_PROFILER_API MONO_API

/* Used to mark internal functions used by the CoreFX PAL library */
#define MONO_PAL_API MONO_API

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

#ifdef __GNUC__
#define MONO_NO_OPTIMIZATION __attribute__ ((optimize("O0")))
#else
#define MONO_NO_OPTIMIZATION
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

#if __has_feature(address_sanitizer)
#define MONO_HAS_CLANG_ADDRESS_SANITIZER 1
#else
#define MONO_HAS_CLANG_ADDRESS_SANITIZER 0
#endif

#else
#define MONO_HAS_CLANG_THREAD_SANITIZER 0
#define MONO_HAS_CLANG_ADDRESS_SANITIZER 0
#endif

/* Used to tell Clang's ThreadSanitizer to not report data races that occur within a certain function */
#if MONO_HAS_CLANG_THREAD_SANITIZER
#define MONO_NO_SANITIZE_THREAD __attribute__ ((no_sanitize("thread")))
#else
#define MONO_NO_SANITIZE_THREAD
#endif

/* Used to tell Clang's AddressSanitizer to turn off instrumentation for a certain function */
#if MONO_HAS_CLANG_ADDRESS_SANITIZER
#define MONO_NO_SANITIZE_ADDRESS __attribute__ ((no_sanitize("address")))
#else
#define MONO_NO_SANITIZE_ADDRESS
#endif

/* Used when building with Android NDK's unified headers */
#if defined(HOST_ANDROID) && defined (ANDROID_UNIFIED_HEADERS)
#if __ANDROID_API__ < 21

typedef int32_t __mono_off32_t;

#ifdef HAVE_SYS_MMAN_H
#include <sys/mman.h>
#endif

#if !defined(mmap)

#ifdef __cplusplus
extern "C" {
#endif

/* Unified headers before API 21 do not declare mmap when LARGE_FILES are used (via -D_FILE_OFFSET_BITS=64)
 * which is always the case when Mono build targets Android. The problem here is that the unified headers
 * map `mmap` to `mmap64` if large files are enabled but this api exists only in API21 onwards. Therefore
 * we must carefully declare the 32-bit mmap here without changing the ABI along the way. Carefully because
 * in this instance off_t is redeclared to be 64-bit and that's not what we want.
 */
void* mmap (void*, size_t, int, int, int, __mono_off32_t);

#ifdef __cplusplus
} // extern C
#endif

#endif /* !mmap */

#ifdef HAVE_SYS_SENDFILE_H
#include <sys/sendfile.h>
#endif

#if !defined(sendfile)

#ifdef __cplusplus
extern "C" {
#endif

/* The same thing as with mmap happens with sendfile */
ssize_t sendfile (int out_fd, int in_fd, __mono_off32_t* offset, size_t count);

#ifdef __cplusplus
} // extern C
#endif

#endif /* !sendfile */

#endif /* __ANDROID_API__ < 21 */
#endif /* HOST_ANDROID && ANDROID_UNIFIED_HEADERS */

#endif /* __UTILS_MONO_COMPILER_H__*/

