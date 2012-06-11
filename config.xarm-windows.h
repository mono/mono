/* config.h.  Generated from config.h.in by configure.  */
/* config.h.in.  Generated from configure.in by autoheader.  */

/* The architecture this is running on */
#define ARCHITECTURE "arm"

/* String of disabled features */
#define DISABLED_FEATURES "none"

/* Have Boehm GC */
#define HAVE_BOEHM_GC 1

/* Have GC_enable */
#define HAVE_GC_ENABLE 1

/* Have GC_gcj_malloc */
#define HAVE_GC_GCJ_MALLOC 1

/* Define to 1 if you have the <gc/gc.h> header file. */
/* #undef HAVE_GC_GC_H */

/* Have gc.h */
#define HAVE_GC_H 1

/* Define to 1 if you have the `trunc' function. */
#define HAVE_TRUNC 1

/* The runtime is compiled for cross-compiling mode */
#define MONO_CROSS_COMPILE 1
#define PLATFORM_IPHONE_XCOMP 1
#define CROSS_COMPILING 1

#define ARM_FPU_VFP 1

/* The Mono Debugger is supported on this platform */
/* #undef MONO_DEBUGGER_SUPPORTED */

/* Xen-specific behaviour */
//#define MONO_XEN_OPT 1

/* Length of zero length arrays */
#define MONO_ZERO_ARRAY_LENGTH 0

/* Define if Unix sockets cannot be created in an anonymous namespace */
/* #undef NEED_LINK_UNLINK */

/* Name of package */
#define PACKAGE "mono"

/* Define to the address where bug reports for this package should be sent. */
#define PACKAGE_BUGREPORT ""

/* Define to the full name of this package. */
#define PACKAGE_NAME ""

/* Define to the full name and version of this package. */
#define PACKAGE_STRING ""

/* Define to the one symbol short name of this package. */
#define PACKAGE_TARNAME ""

/* Define to the version of this package. */
#define PACKAGE_VERSION ""

/* Define to 1 if you have the ANSI C header files. */
#define STDC_HEADERS 1

/* ... */
/* #undef TARGET_AMD64 */

#define TARGET_ARM 1

/* byte order of target */
#define TARGET_BYTE_ORDER G_LITTLE_ENDIAN

/* ... */
/* #undef TARGET_X86 */

/* GC description */
#define USED_GC_NAME "Included Boehm (with typed GC and Parallel Mark)"

/* Use included libgc */
#define USE_INCLUDED_LIBGC 1

/* ... */
/* #undef USE_MACH_SEMA */

/* Use malloc for each single mempool allocation */
/* #undef USE_MALLOC_FOR_MEMPOOLS */

/* Use mono_mutex_t */
/* #undef USE_MONO_MUTEX */

/* Version number of package */
#define VERSION "2.5"

///* 64 bit mode with 4 byte longs and pointers */
//#define __mono_ilp32__ 1
///* #define __mono_ppc_ilp32__ 1 */

///* ... */
//#define __mono_ppc64__ 1

/* Define to 1 if you have the `GetProcessId' function. */
#if (_WIN32_WINNT >= 0x0502)
#define HAVE_GETPROCESSID 1
#endif

#define HAVE_INTTYPES_H 1
#define HAVE_ISINF 1
#define HAVE_MEMORY_H 1
#define HAVE_STRINGS_H 1
#define HAVE_STRING_H 1
#define HAVE_STRUCT_IP_MREQ 1
#define HAVE_SYS_STAT_H 1
#define HAVE_SYS_TYPES_H 1
#define HAVE_SYS_UTIME_H 1
#define HAVE_TRUNC 1
#define HAVE_WCHAR_H 1

#define DISABLE_PORTABILITY 1
#define PLATFORM_NO_SYMLINKS 1

#define SIZEOF_SIZE_T 4
#define SIZEOF_VOID_P 4
#define SIZEOF_REGISTER 4
#define _POSIX_PATH_MAX 1055 

#define HOST_WIN32 1
