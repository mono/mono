/*
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 2000-2004 Hewlett-Packard Development Company, L.P.
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 */

/*
 * This header is private to the gc.  It is almost always included from
 * gc_priv.h.  However it is possible to include it by itself if just the
 * configuration macros are needed.  In that
 * case, a few declarations relying on types declared in gc_priv.h will be
 * omitted.
 */

#ifndef GCCONFIG_H
#define GCCONFIG_H

#define MANUAL_VDB

# ifndef GC_PRIVATE_H
    /* Fake ptr_t declaration, just to avoid compilation errors.        */
    /* This avoids many instances if "ifndef GC_PRIVATE_H" below.       */
    typedef struct GC_undefined_struct * ptr_t;
#   include <stddef.h>  /* For size_t etc. */
# endif

/* Note: Only wrap our own declarations, and not the included headers.  */
/* In this case, wrap our entire file, but temporarily unwrap/rewrap    */
/* around #includes.  Types and macros do not need such wrapping, only  */
/* the declared global data and functions.                              */
#ifdef __cplusplus
# define EXTERN_C_BEGIN extern "C" {
# define EXTERN_C_END } /* extern "C" */
#else
# define EXTERN_C_BEGIN /* empty */
# define EXTERN_C_END /* empty */
#endif

EXTERN_C_BEGIN

/* Convenient internal macro to test version of Clang.  */
#if defined(__clang__) && defined(__clang_major__)
# define GC_CLANG_PREREQ(major, minor) \
    ((__clang_major__ << 16) + __clang_minor__ >= ((major) << 16) + (minor))
# define GC_CLANG_PREREQ_FULL(major, minor, patchlevel) \
            (GC_CLANG_PREREQ(major, (minor) + 1) \
                || (__clang_major__ == (major) && __clang_minor__ == (minor) \
                    && __clang_patchlevel__ >= (patchlevel)))
#else
# define GC_CLANG_PREREQ(major, minor) 0 /* FALSE */
# define GC_CLANG_PREREQ_FULL(major, minor, patchlevel) 0
#endif

#ifdef LINT2
  /* A macro (based on a tricky expression) to prevent false warnings   */
  /* like "Array compared to 0", "Comparison of identical expressions", */
  /* "Untrusted loop bound" output by some static code analysis tools.  */
  /* The argument should not be a literal value.  The result is         */
  /* converted to word type.  (Actually, GC_word is used instead of     */
  /* word type as the latter might be undefined at the place of use.)   */
# define COVERT_DATAFLOW(w) (~(GC_word)(w)^(~(GC_word)0))
#else
# define COVERT_DATAFLOW(w) ((GC_word)(w))
#endif

/* Machine dependent parameters.  Some tuning parameters can be found   */
/* near the top of gc_private.h.                                        */

/* Machine specific parts contributed by various people.  See README file. */

#if defined(__ANDROID__) && !defined(HOST_ANDROID)
  /* __ANDROID__ macro is defined by Android NDK gcc.   */
# define HOST_ANDROID 1
#endif

#if defined(HOST_ANDROID)
#define IGNORE_PROG_DATA_START
#endif

#if defined(TIZEN) && !defined(HOST_TIZEN)
# define HOST_TIZEN 1
#endif

#if defined(__SYMBIAN32__) && !defined(SYMBIAN)
# define SYMBIAN
# ifdef __WINS__
#   pragma data_seg(".data2")
# endif
#endif

/* First a unified test for Linux: */
# if (defined(linux) || defined(__linux__) || defined(HOST_ANDROID)) \
     && !defined(LINUX) && !defined(__native_client__)
#   define LINUX
# endif

/* And one for QNX: */
# if defined(__QNX__)
#    define I386
#    define OS_TYPE "QNX"
#    define SA_RESTART 0
#    define HEURISTIC1
     extern char etext[];
     extern int _end[];
#    define DATASTART ((ptr_t)(etext))
#    define DATAEND ((ptr_t)(_end))
#    define mach_type_known
# endif

/* And one for NetBSD: */
# if defined(__NetBSD__)
#    define NETBSD
# endif

/* And one for OpenBSD: */
# if defined(__OpenBSD__)
#    define OPENBSD
# endif

/* And one for FreeBSD: */
# if (defined(__FreeBSD__) || defined(__DragonFly__) \
      || defined(__FreeBSD_kernel__)) && !defined(FREEBSD) \
     && !defined(GC_NO_FREEBSD) 
#   define FREEBSD
# endif

/* And one for Darwin: */
# if defined(macosx) || (defined(__APPLE__) && defined(__MACH__))
#   define DARWIN
    EXTERN_C_END
#   include <TargetConditionals.h>
    EXTERN_C_BEGIN
# endif

/* Determine the machine type: */
# if defined(__native_client__)
#    define NACL
#    if !defined(__portable_native_client__) && !defined(__arm__)
#      define I386
#      define mach_type_known
#    else
       /* Here we will rely upon arch-specific defines. */
#    endif
# endif
# if defined(__aarch64__)
#    define AARCH64
#    if !defined(LINUX) && !defined(DARWIN) && !defined(FREEBSD) \
        && !defined(NN_BUILD_TARGET_PLATFORM_NX)
#      define NOSYS
#      define mach_type_known
#    endif
# endif
# if defined(__arm) || defined(__arm__) || defined(__thumb__)
#    define ARM32
#    if defined(NACL)
#      define mach_type_known
#    elif !defined(LINUX) && !defined(NETBSD) && !defined(FREEBSD) \
          && !defined(OPENBSD) && !defined(DARWIN) && !defined(_WIN32) \
          && !defined(__CEGCC__) && !defined(NN_PLATFORM_CTR) \
          && !defined(NN_BUILD_TARGET_PLATFORM_NX) \
          && !defined(GC_NO_NOSYS) && !defined(SN_TARGET_PSP2) \
          && !defined(SYMBIAN)
#      define NOSYS
#      define mach_type_known
#    endif
# endif
# if defined(sun) && defined(mc68000) && !defined(CPPCHECK)
#    error SUNOS4 no longer supported
# endif
# if defined(hp9000s300) && !defined(CPPCHECK)
#    error M68K based HP machines no longer supported.
# endif
# if defined(OPENBSD) && defined(m68k)
     /* FIXME: Should we remove this case? */
#    define M68K
#    define mach_type_known
# endif
# if defined(OPENBSD) && defined(__sparc__)
#    define SPARC
#    define mach_type_known
# endif
# if defined(OPENBSD) && defined(__arm__)
#    define ARM32
#    define mach_type_known
# endif
# if defined(OPENBSD) && defined(__sh__)
#    define SH
#    define mach_type_known
# endif
# if defined(NETBSD) && (defined(m68k) || defined(__m68k__))
#    define M68K
#    define mach_type_known
# endif
# if defined(NETBSD) && defined(__powerpc__)
#    define POWERPC
#    define mach_type_known
# endif
# if defined(NETBSD) && (defined(__arm32__) || defined(__arm__))
#    define ARM32
#    define mach_type_known
# endif
# if defined(NETBSD) && defined(__sh__)
#    define SH
#    define mach_type_known
# endif
# if defined(vax) || defined(__vax__)
#    define VAX
#    ifdef ultrix
#       define ULTRIX
#    else
#       define BSD
#    endif
#    define mach_type_known
# endif
# if defined(__NetBSD__) && defined(__vax__)
#    define VAX
#    define mach_type_known
# endif
# if defined(mips) || defined(__mips) || defined(_mips)
#    define MIPS
#    if defined(nec_ews) || defined(_nec_ews)
#      define EWS4800
#    endif
#    if !defined(LINUX) && !defined(EWS4800) && !defined(NETBSD) \
        && !defined(OPENBSD)
#      if defined(ultrix) || defined(__ultrix)
#        define ULTRIX
#      else
#        define IRIX5   /* or IRIX 6.X */
#      endif
#    endif /* !LINUX */
#    if defined(__NetBSD__) && defined(__MIPSEL__)
#      undef ULTRIX
#    endif
#    define mach_type_known
# endif
# if defined(__NIOS2__) || defined(__NIOS2) || defined(__nios2__)
#   define NIOS2 /* Altera NIOS2 */
#   define mach_type_known
# endif
# if defined(__or1k__)
#    define OR1K        /* OpenRISC/or1k */
#    define mach_type_known
# endif
# if defined(DGUX) && (defined(i386) || defined(__i386__))
#    define I386
#    ifndef _USING_DGUX
#    define _USING_DGUX
#    endif
#    define mach_type_known
# endif
# if defined(sequent) && (defined(i386) || defined(__i386__))
#    define I386
#    define SEQUENT
#    define mach_type_known
# endif
# if (defined(sun) || defined(__sun)) && (defined(i386) || defined(__i386__))
#    define I386
#    define SOLARIS
#    define mach_type_known
# endif
# if (defined(sun) || defined(__sun)) && defined(__amd64)
#    define X86_64
#    define SOLARIS
#    define mach_type_known
# endif
# if (defined(__OS2__) || defined(__EMX__)) && defined(__32BIT__)
#    define I386
#    define OS2
#    define mach_type_known
# endif
# if defined(ibm032) && !defined(CPPCHECK)
#   error IBM PC/RT no longer supported.
# endif
# if (defined(sun) || defined(__sun)) && (defined(sparc) || defined(__sparc))
            /* Test for SunOS 5.x */
    EXTERN_C_END
#   include <errno.h>
    EXTERN_C_BEGIN
#   define SPARC
#   define SOLARIS
#   define mach_type_known
# elif defined(sparc) && defined(unix) && !defined(sun) && !defined(linux) \
       && !defined(__OpenBSD__) && !defined(__NetBSD__) \
       && !defined(__FreeBSD__) && !defined(__DragonFly__)
#   define SPARC
#   define DRSNX
#   define mach_type_known
# endif
# if defined(_IBMR2)
#   define POWERPC
#   define AIX
#   define mach_type_known
# endif
# if defined(__NetBSD__) && defined(__sparc__)
#   define SPARC
#   define mach_type_known
# endif
# if defined(_M_XENIX) && defined(_M_SYSV) && defined(_M_I386)
        /* The above test may need refinement   */
#   define I386
#   if defined(_SCO_ELF)
#     define SCO_ELF
#   else
#     define SCO
#   endif
#   define mach_type_known
# endif
# if defined(_AUX_SOURCE) && !defined(CPPCHECK)
#   error A/UX no longer supported
# endif
# if defined(_PA_RISC1_0) || defined(_PA_RISC1_1) || defined(_PA_RISC2_0) \
     || defined(hppa) || defined(__hppa__)
#   define HP_PA
#   if !defined(LINUX) && !defined(HPUX) && !defined(OPENBSD)
#     define HPUX
#   endif
#   define mach_type_known
# endif
# if defined(__ia64) && (defined(_HPUX_SOURCE) || defined(__HP_aCC))
#   define IA64
#   ifndef HPUX
#     define HPUX
#   endif
#   define mach_type_known
# endif
# if (defined(__BEOS__) || defined(__HAIKU__)) && defined(_X86_)
#    define I386
#    define HAIKU
#    define mach_type_known
# endif
# if defined(__HAIKU__) && (defined(__amd64__) || defined(__x86_64__))
#    define X86_64
#    define HAIKU
#    define mach_type_known
# endif
# if defined(OPENBSD) && defined(__amd64__)
#    define X86_64
#    define mach_type_known
# endif
# if defined(LINUX) && (defined(i386) || defined(__i386__))
#    define I386
#    define mach_type_known
# endif
# if defined(LINUX) && defined(__x86_64__)
#    define X86_64
#    define mach_type_known
# endif
# if defined(LINUX) && (defined(__ia64__) || defined(__ia64))
#    define IA64
#    define mach_type_known
# endif
# if defined(LINUX) && defined(__aarch64__)
#    define AARCH64
#    define mach_type_known
# endif
# if defined(LINUX) && (defined(__arm) || defined(__arm__))
#    define ARM32
#    define mach_type_known
# endif
# if defined(LINUX) && defined(__cris__)
#    ifndef CRIS
#       define CRIS
#    endif
#    define mach_type_known
# endif
# if defined(LINUX) && (defined(powerpc) || defined(__powerpc__) \
                        || defined(powerpc64) || defined(__powerpc64__))
#    define POWERPC
#    define mach_type_known
# endif
# if defined(LINUX) && defined(__mc68000__)
#    define M68K
#    define mach_type_known
# endif
# if defined(LINUX) && (defined(sparc) || defined(__sparc__))
#    define SPARC
#    define mach_type_known
# endif
# if defined(LINUX) && defined(__sh__)
#    define SH
#    define mach_type_known
# endif
# if defined(LINUX) && defined(__avr32__)
#    define AVR32
#    define mach_type_known
# endif
# if defined(LINUX) && defined(__m32r__)
#    define M32R
#    define mach_type_known
# endif
# if defined(__alpha) || defined(__alpha__)
#   define ALPHA
#   if !defined(LINUX) && !defined(NETBSD) && !defined(OPENBSD) \
       && !defined(FREEBSD)
#     define OSF1       /* a.k.a Digital Unix */
#   endif
#   define mach_type_known
# endif
# if defined(_AMIGA) && !defined(AMIGA)
#   define AMIGA
# endif
# ifdef AMIGA
#   define M68K
#   define mach_type_known
# endif
# if defined(THINK_C) \
     || (defined(__MWERKS__) && !defined(__powerc) && !defined(SYMBIAN))
#   define M68K
#   define MACOS
#   define mach_type_known
# endif
# if defined(__MWERKS__) && defined(__powerc) && !defined(__MACH__) \
     && !defined(SYMBIAN)
#   define POWERPC
#   define MACOS
#   define mach_type_known
# endif
# if defined(__OpenBSD__) && defined(__powerpc__)
#   define POWERPC
#   define OPENBSD
#   define mach_type_known
# endif
# if defined(DARWIN)
#   if defined(__ppc__)  || defined(__ppc64__)
#    define POWERPC
#    define mach_type_known
#   elif defined(__x86_64__) || defined(__x86_64)
#    define X86_64
#    define mach_type_known
#   elif defined(__i386__)
#    define I386
#    define mach_type_known
#   elif defined(__arm__)
#    define ARM32
#    define mach_type_known
#   elif defined(__aarch64__)
#    define AARCH64
#    define mach_type_known
#   endif
# endif
# if defined(__rtems__) && (defined(i386) || defined(__i386__))
#   define I386
#   define RTEMS
#   define mach_type_known
# endif
# if defined(NeXT) && defined(mc68000)
#   define M68K
#   define NEXT
#   define mach_type_known
# endif
# if defined(NeXT) && (defined(i386) || defined(__i386__))
#   define I386
#   define NEXT
#   define mach_type_known
# endif
# if defined(__OpenBSD__) && (defined(i386) || defined(__i386__))
#   define I386
#   define OPENBSD
#   define mach_type_known
# endif
# if defined(__NetBSD__) && (defined(i386) || defined(__i386__))
#   define I386
#   define mach_type_known
# endif
# if defined(__NetBSD__) && defined(__x86_64__)
#    define X86_64
#    define mach_type_known
# endif
# if defined(FREEBSD) && (defined(i386) || defined(__i386__))
#   define I386
#   define mach_type_known
# endif
# if (defined(FREEBSD)) \
     && (defined(__amd64__) || defined(__x86_64__))
#   define X86_64
#   define mach_type_known
# endif
# if defined(FREEBSD) && defined(__sparc__)
#   define SPARC
#   define mach_type_known
# endif
# if defined(FREEBSD) && (defined(powerpc) || defined(__powerpc__))
#   define POWERPC
#   define mach_type_known
# endif
# if defined(FREEBSD) && defined(__arm__)
#   define ARM32
#   define mach_type_known
# endif
# if defined(FREEBSD) && defined(__aarch64__)
#   define AARCH64
#   define mach_type_known
# endif
# if defined(FREEBSD) && (defined(mips) || defined(__mips) || defined(_mips))
#   define MIPS
#   define mach_type_known
# endif
# if defined(bsdi) && (defined(i386) || defined(__i386__))
#    define I386
#    define BSDI
#    define mach_type_known
# endif
# if !defined(mach_type_known) && defined(__386BSD__)
#   define I386
#   define THREE86BSD
#   define mach_type_known
# endif
# if defined(_CX_UX) && defined(_M88K)
#   define M88K
#   define CX_UX
#   define mach_type_known
# endif
# if defined(DGUX) && defined(m88k)
#   define M88K
    /* DGUX defined */
#   define mach_type_known
# endif
# if defined(_WIN32_WCE) || defined(__CEGCC__) || defined(__MINGW32CE__)
    /* SH3, SH4, MIPS already defined for corresponding architectures */
#   if defined(SH3) || defined(SH4)
#     define SH
#   endif
#   if defined(x86) || defined(__i386__)
#     define I386
#   endif
#   if defined(_M_ARM) || defined(ARM) || defined(_ARM_)
#     define ARM32
#   endif
#   define MSWINCE
#   define mach_type_known
# else
#   if ((defined(_MSDOS) || defined(_MSC_VER)) && (_M_IX86 >= 300)) \
       || (defined(_WIN32) && !defined(__CYGWIN32__) && !defined(__CYGWIN__) \
           && !defined(SYMBIAN))
#     if defined(__LP64__) || defined(_M_X64)
#       define X86_64
#     elif defined(_M_ARM)
#       define ARM32
#     elif defined(_M_ARM64)
#       define AARCH64
#     else /* _M_IX86 */
#       define I386
#     endif
#     ifdef _XBOX_ONE
#       define MSWIN_XBOX1
#     else
#       define MSWIN32  /* or Win64 */
#       if defined(WINAPI_FAMILY) && (WINAPI_FAMILY == WINAPI_FAMILY_APP)
#         define MSWINRT_FLAVOR
#       endif
#     endif
#     define mach_type_known
#   endif
#   if defined(_MSC_VER) && defined(_M_IA64)
#     define IA64
#     define MSWIN32    /* Really win64, but we don't treat 64-bit      */
                        /* variants as a different platform.            */
#   endif
# endif
# if defined(__DJGPP__)
#   define I386
#   ifndef DJGPP
#     define DJGPP  /* MSDOS running the DJGPP port of GCC */
#   endif
#   define mach_type_known
# endif
# if defined(__CYGWIN32__) || defined(__CYGWIN__)
#   if defined(__LP64__)
#     define X86_64
#   else
#     define I386
#   endif
#   define CYGWIN32
#   define mach_type_known
# endif
# if defined(__MINGW32__) && !defined(mach_type_known)
#   define I386
#   define MSWIN32
#   define mach_type_known
# endif
# if defined(__BORLANDC__)
#   define I386
#   define MSWIN32
#   define mach_type_known
# endif
# if defined(_UTS) && !defined(mach_type_known)
#   define S370
#   define UTS4
#   define mach_type_known
# endif
# if defined(__pj__) && !defined(CPPCHECK)
#   error PicoJava no longer supported
    /* The implementation had problems, and I haven't heard of users    */
    /* in ages.  If you want it resurrected, let me know.               */
# endif
# if defined(__embedded__) && defined(PPC)
#   define POWERPC
#   define NOSYS
#   define mach_type_known
# endif

# if defined(__WATCOMC__) && defined(__386__)
#   define I386
#   if !defined(OS2) && !defined(MSWIN32) && !defined(DOS4GW)
#     if defined(__OS2__)
#       define OS2
#     else
#       if defined(__WINDOWS_386__) || defined(__NT__)
#         define MSWIN32
#       else
#         define DOS4GW
#       endif
#     endif
#   endif
#   define mach_type_known
# endif
# if defined(__s390__) && defined(LINUX)
#    define S390
#    define mach_type_known
# endif
# if defined(__GNU__)
#   if defined(__i386__)
/* The Debian Hurd running on generic PC */
#     define  HURD
#     define  I386
#     define  mach_type_known
#    endif
# endif
# if defined(__TANDEM)
    /* Nonstop S-series */
    /* FIXME: Should recognize Integrity series? */
#   define MIPS
#   define NONSTOP
#   define mach_type_known
# endif
# if defined(__hexagon__) && defined(LINUX)
#    define HEXAGON
#    define mach_type_known
# endif
# if defined(__tile__) && defined(LINUX)
#   ifdef __tilegx__
#     define TILEGX
#   else
#     define TILEPRO
#   endif
#   define mach_type_known
# endif
# if defined(__riscv) && defined(LINUX)
#   define RISCV
#   define mach_type_known
# endif

# if defined(SN_TARGET_PSP2)
#   define mach_type_known
# endif

# if defined(NN_PLATFORM_CTR)
#   define mach_type_known
# endif

# if defined(NN_BUILD_TARGET_PLATFORM_NX)
#   define NINTENDO_SWITCH
#   define mach_type_known
# endif

# if defined(SYMBIAN)
#   define mach_type_known
# endif

# if defined(__EMSCRIPTEN__)
#   define I386
#   define mach_type_known
# endif

/* Feel free to add more clauses here */

/* Or manually define the machine type here.  A machine type is         */
/* characterized by the architecture.  Some                             */
/* machine types are further subdivided by OS.                          */
/* Macros such as LINUX, FREEBSD, etc. distinguish them.                */
/* SYSV on an M68K actually means A/UX.                                 */
/* The distinction in these cases is usually the stack starting address */
# if !defined(mach_type_known) && !defined(CPPCHECK)
#   error "The collector has not been ported to this machine/OS combination."
# endif
                    /* Mapping is: M68K       ==> Motorola 680X0        */
                    /*             (NEXT, and SYSV (A/UX),              */
                    /*             MACOS and AMIGA variants)            */
                    /*             I386       ==> Intel 386             */
                    /*              (SEQUENT, OS2, SCO, LINUX, NETBSD,  */
                    /*               FREEBSD, THREE86BSD, MSWIN32,      */
                    /*               BSDI, SOLARIS, NEXT and others)    */
                    /*             NS32K      ==> Encore Multimax       */
                    /*             MIPS       ==> R2000 through R14K    */
                    /*                  (many variants)                 */
                    /*             VAX        ==> DEC VAX               */
                    /*                  (BSD, ULTRIX variants)          */
                    /*             HP_PA      ==> HP9000/700 & /800     */
                    /*                            HP/UX, LINUX          */
                    /*             SPARC      ==> SPARC v7/v8/v9        */
                    /*                  (SOLARIS, LINUX, DRSNX variants)        */
                    /*             ALPHA      ==> DEC Alpha             */
                    /*                  (OSF1 and LINUX variants)       */
                    /*             M88K       ==> Motorola 88XX0        */
                    /*                  (CX_UX and DGUX)                */
                    /*             S370       ==> 370-like machine      */
                    /*                  running Amdahl UTS4             */
                    /*             S390       ==> 390-like machine      */
                    /*                  running LINUX                   */
                    /*             AARCH64    ==> ARM AArch64           */
                    /*             ARM32      ==> Intel StrongARM       */
                    /*             IA64       ==> Intel IPF             */
                    /*                            (e.g. Itanium)        */
                    /*                  (LINUX and HPUX)                */
                    /*             SH         ==> Hitachi SuperH        */
                    /*                  (LINUX & MSWINCE)               */
                    /*             X86_64     ==> AMD x86-64            */
                    /*             POWERPC    ==> IBM/Apple PowerPC     */
                    /*                  (MACOS(<=9),DARWIN(incl.MACOSX),*/
                    /*                   LINUX, NETBSD, AIX, NOSYS      */
                    /*                   variants)                      */
                    /*                  Handles 32 and 64-bit variants. */
                    /*             CRIS       ==> Axis Etrax            */
                    /*             M32R       ==> Renesas M32R          */
                    /*             HEXAGON    ==> Qualcomm Hexagon      */
                    /*             TILEPRO    ==> Tilera TILEPro        */
                    /*             TILEGX     ==> Tilera TILE-Gx        */


/*
 * For each architecture and OS, the following need to be defined:
 *
 * CPP_WORDSZ is a simple integer constant representing the word size.
 * in bits.  We assume byte addressability, where a byte has 8 bits.
 * We also assume CPP_WORDSZ is either 32 or 64.
 * (We care about the length of pointers, not hardware
 * bus widths.  Thus a 64 bit processor with a C compiler that uses
 * 32 bit pointers should use CPP_WORDSZ of 32, not 64. Default is 32.)
 *
 * MACH_TYPE is a string representation of the machine type.
 * OS_TYPE is analogous for the OS.
 *
 * ALIGNMENT is the largest N, such that
 * all pointer are guaranteed to be aligned on N byte boundaries.
 * defining it to be 1 will always work, but perform poorly.
 *
 * DATASTART is the beginning of the data segment.
 * On some platforms SEARCH_FOR_DATA_START is defined.
 * SEARCH_FOR_DATASTART will cause GC_data_start to
 * be set to an address determined by accessing data backwards from _end
 * until an unmapped page is found.  DATASTART will be defined to be
 * GC_data_start.
 * On UNIX-like systems, the collector will scan the area between DATASTART
 * and DATAEND for root pointers.
 *
 * DATAEND, if not "end", where "end" is defined as "extern int end[]".
 * RTH suggests gaining access to linker script synth'd values with
 * this idiom instead of "&end", where "end" is defined as "extern int end".
 * Otherwise, "GCC will assume these are in .sdata/.sbss" and it will, e.g.,
 * cause failures on alpha*-*-* with -msmall-data or -fpic or mips-*-*
 * without any special options.
 *
 * STACKBOTTOM is the cool end of the stack, which is usually the
 * highest address in the stack.
 * Under PCR or OS/2, we have other ways of finding thread stacks.
 * For each machine, the following should:
 * 1) define STACK_GROWS_UP if the stack grows toward higher addresses, and
 * 2) define exactly one of
 *      STACKBOTTOM (should be defined to be an expression)
 *      LINUX_STACKBOTTOM
 *      HEURISTIC1
 *      HEURISTIC2
 * If STACKBOTTOM is defined, then its value will be used directly as the
 * stack base.  If LINUX_STACKBOTTOM is defined, then it will be determined
 * with a method appropriate for most Linux systems.  Currently we look
 * first for __libc_stack_end (currently only if USE_LIBC_PRIVATES is
 * defined), and if that fails read it from /proc.  (If USE_LIBC_PRIVATES
 * is not defined and NO_PROC_STAT is defined, we revert to HEURISTIC2.)
 * If either of the last two macros are defined, then STACKBOTTOM is computed
 * during collector startup using one of the following two heuristics:
 * HEURISTIC1:  Take an address inside GC_init's frame, and round it up to
 *              the next multiple of STACK_GRAN.
 * HEURISTIC2:  Take an address inside GC_init's frame, increment it repeatedly
 *              in small steps (decrement if STACK_GROWS_UP), and read the value
 *              at each location.  Remember the value when the first
 *              Segmentation violation or Bus error is signaled.  Round that
 *              to the nearest plausible page boundary, and use that instead
 *              of STACKBOTTOM.
 *
 * Gustavo Rodriguez-Rivera points out that on most (all?) Unix machines,
 * the value of environ is a pointer that can serve as STACKBOTTOM.
 * I expect that HEURISTIC2 can be replaced by this approach, which
 * interferes far less with debugging.  However it has the disadvantage
 * that it's confused by a putenv call before the collector is initialized.
 * This could be dealt with by intercepting putenv ...
 *
 * If no expression for STACKBOTTOM can be found, and neither of the above
 * heuristics are usable, the collector can still be used with all of the above
 * undefined, provided one of the following is done:
 * 1) GC_mark_roots can be changed to somehow mark from the correct stack(s)
 *    without reference to STACKBOTTOM.  This is appropriate for use in
 *    conjunction with thread packages, since there will be multiple stacks.
 *    (Allocating thread stacks in the heap, and treating them as ordinary
 *    heap data objects is also possible as a last resort.  However, this is
 *    likely to introduce significant amounts of excess storage retention
 *    unless the dead parts of the thread stacks are periodically cleared.)
 * 2) Client code may set GC_stackbottom before calling any GC_ routines.
 *    If the author of the client code controls the main program, this is
 *    easily accomplished by introducing a new main program, setting
 *    GC_stackbottom to the address of a local variable, and then calling
 *    the original main program.  The new main program would read something
 *    like (provided real_main() is not inlined by the compiler):
 *
 *              #include "gc.h"
 *
 *              main(argc, argv, envp)
 *              int argc;
 *              char **argv, **envp;
 *              {
 *                  volatile int dummy;
 *
 *                  GC_stackbottom = (ptr_t)(&dummy);
 *                  return(real_main(argc, argv, envp));
 *              }
 *
 *
 * Each architecture may also define the style of virtual dirty bit
 * implementation to be used:
 *   MPROTECT_VDB: Write protect the heap and catch faults.
 *   GWW_VDB: Use win32 GetWriteWatch primitive.
 *   PROC_VDB: Use the SVR4 /proc primitives to read dirty bits.
 *
 * The first and second one may be combined, in which case a runtime
 * selection will be made, based on GetWriteWatch availability.
 *
 * An architecture may define DYNAMIC_LOADING if dyn_load.c
 * defined GC_register_dynamic_libraries() for the architecture.
 *
 * An architecture may define PREFETCH(x) to preload the cache with *x.
 * This defaults to GCC built-in operation (or a no-op for other compilers).
 *
 * GC_PREFETCH_FOR_WRITE(x) is used if *x is about to be written.
 *
 * An architecture may also define CLEAR_DOUBLE(x) to be a fast way to
 * clear the two words at GC_malloc-aligned address x.  By default,
 * word stores of 0 are used instead.
 *
 * HEAP_START may be defined as the initial address hint for mmap-based
 * allocation.
 */

/* If available, we can use __builtin_unwind_init() to push the     */
/* relevant registers onto the stack.                               */
# if GC_GNUC_PREREQ(2, 8) \
     && !defined(__INTEL_COMPILER) && !defined(__PATHCC__) \
     && !defined(__FUJITSU) /* for FX10 system */ \
     && !(defined(POWERPC) && defined(DARWIN)) /* for MacOS X 10.3.9 */ \
     && !defined(RTEMS) \
     && !defined(__ARMCC_VERSION) /* does not exist in armcc gnu emu */ \
     && !defined(__clang__) /* since no-op in clang (3.0) */
#   define HAVE_BUILTIN_UNWIND_INIT
# endif

# ifdef SYMBIAN
#   define MACH_TYPE "SYMBIAN"
#   define OS_TYPE "SYMBIAN"
#   define CPP_WORDSZ 32
#   define ALIGNMENT 4
#   define DATASTART (ptr_t)ALIGNMENT /* cannot be null */
#   define DATAEND (ptr_t)ALIGNMENT
# endif

# ifdef __EMSCRIPTEN__
#   define OS_TYPE "EMSCRIPTEN"
#   define CPP_WORDSZ 32
#   define ALIGNMENT 4
#   define DATASTART (ptr_t)ALIGNMENT
#   define DATAEND (ptr_t)ALIGNMENT
    /* Since JavaScript/asm.js/WebAssembly is not able to access the    */
    /* function call stack or the local data stack, it is not possible  */
    /* for GC to perform its stack walking operation to find roots on   */
    /* the stack.  To work around that, the clients generally only do   */
    /* BDWGC steps when the stack is empty so it is known that there    */
    /* are no objects that would be found on the stack, and BDWGC is    */
    /* compiled with stack walking disabled.                            */
#   define STACK_NOT_SCANNED
# endif

# define STACK_GRAN 0x1000000
# ifdef M68K
#   define MACH_TYPE "M68K"
#   define ALIGNMENT 2
#   ifdef OPENBSD
        /* FIXME: Should we remove this case? */
#       define OS_TYPE "OPENBSD"
#       define HEURISTIC2
#       ifdef __ELF__
          extern ptr_t GC_data_start;
#         define DATASTART GC_data_start
#         define DYNAMIC_LOADING
#       else
          extern char etext[];
#         define DATASTART ((ptr_t)(etext))
#       endif
#   endif
#   ifdef NETBSD
#       define OS_TYPE "NETBSD"
#       define HEURISTIC2
#       ifdef __ELF__
          extern ptr_t GC_data_start;
#         define DATASTART GC_data_start
#         define DYNAMIC_LOADING
#       else
          extern char etext[];
#         define DATASTART ((ptr_t)(etext))
#       endif
#   endif
#   ifdef LINUX
#       define OS_TYPE "LINUX"
#       define LINUX_STACKBOTTOM
#       define MPROTECT_VDB
#       ifdef __ELF__
#         define DYNAMIC_LOADING
          EXTERN_C_END
#         include <features.h>
          EXTERN_C_BEGIN
#         if defined(__GLIBC__) && __GLIBC__ >= 2
#           define SEARCH_FOR_DATA_START
#         else /* !GLIBC2 */
            extern char **__environ;
#           define DATASTART ((ptr_t)(&__environ))
                             /* hideous kludge: __environ is the first */
                             /* word in crt0.o, and delimits the start */
                             /* of the data segment, no matter which   */
                             /* ld options were passed through.        */
                             /* We could use _etext instead, but that  */
                             /* would include .rodata, which may       */
                             /* contain large read-only data tables    */
                             /* that we'd rather not scan.             */
#         endif /* !GLIBC2 */
          extern int _end[];
#         define DATAEND ((ptr_t)(_end))
#       else
          extern int etext[];
#         define DATASTART ((ptr_t)((((word)(etext)) + 0xfff) & ~0xfff))
#       endif
#   endif
#   ifdef AMIGA
#       define OS_TYPE "AMIGA"
                /* STACKBOTTOM and DATASTART handled specially  */
                /* in os_dep.c                                  */
#       define DATAEND  /* not needed */
#       define GETPAGESIZE() 4096
#   endif
#   ifdef MACOS
#     ifndef __LOWMEM__
        EXTERN_C_END
#       include <LowMem.h>
        EXTERN_C_BEGIN
#     endif
#     define OS_TYPE "MACOS"
                /* see os_dep.c for details of global data segments. */
#     define STACKBOTTOM ((ptr_t)LMGetCurStackBase())
#     define DATAEND    /* not needed */
#     define GETPAGESIZE() 4096
#   endif
#   ifdef NEXT
#       define OS_TYPE "NEXT"
#       define DATASTART ((ptr_t)get_etext())
#       define DATASTART_IS_FUNC
#       define STACKBOTTOM ((ptr_t)0x4000000)
#       define DATAEND  /* not needed */
#   endif
# endif

# if defined(POWERPC)
#   define MACH_TYPE "POWERPC"
#   ifdef MACOS
#     define ALIGNMENT 2  /* Still necessary?  Could it be 4?   */
#     ifndef __LOWMEM__
        EXTERN_C_END
#       include <LowMem.h>
        EXTERN_C_BEGIN
#     endif
#     define OS_TYPE "MACOS"
                        /* see os_dep.c for details of global data segments. */
#     define STACKBOTTOM ((ptr_t)LMGetCurStackBase())
#     define DATAEND  /* not needed */
#   endif
#   ifdef LINUX
#     if defined(__powerpc64__)
#       define ALIGNMENT 8
#       define CPP_WORDSZ 64
#       ifndef HBLKSIZE
#         define HBLKSIZE 4096
#       endif
#     else
#       define ALIGNMENT 4
#     endif
#     define OS_TYPE "LINUX"
      /* HEURISTIC1 has been reliably reported to fail for a 32-bit     */
      /* executable on a 64 bit kernel.                                 */
#     if defined(__bg__)
        /* The Linux Compute Node Kernel (used on BlueGene systems)     */
        /* does not support LINUX_STACKBOTTOM way.                      */
#       define HEURISTIC2
#       define NO_PTHREAD_GETATTR_NP
#     else
#       define LINUX_STACKBOTTOM
#     endif
#     define DYNAMIC_LOADING
#     define SEARCH_FOR_DATA_START
      extern int _end[];
#     define DATAEND ((ptr_t)(_end))
#   endif
#   ifdef DARWIN
#     define OS_TYPE "DARWIN"
#     define DYNAMIC_LOADING
#     if defined(__ppc64__)
#       define ALIGNMENT 8
#       define CPP_WORDSZ 64
#       define STACKBOTTOM ((ptr_t)0x7fff5fc00000)
#       define CACHE_LINE_SIZE 64
#       ifndef HBLKSIZE
#         define HBLKSIZE 4096
#       endif
#     else
#       define ALIGNMENT 4
#       define STACKBOTTOM ((ptr_t)0xc0000000)
#     endif
      /* XXX: see get_end(3), get_etext() and get_end() should not be used. */
      /* These aren't used when dyld support is enabled (it is by default). */
#     define DATASTART ((ptr_t)get_etext())
#     define DATAEND   ((ptr_t)get_end())
#     define USE_MMAP_ANON
#     define MPROTECT_VDB
      EXTERN_C_END
#     include <unistd.h>
      EXTERN_C_BEGIN
#     define GETPAGESIZE() (unsigned)getpagesize()
#     if defined(USE_PPC_PREFETCH) && defined(__GNUC__)
        /* The performance impact of prefetches is untested */
#       define PREFETCH(x) \
          __asm__ __volatile__ ("dcbt 0,%0" : : "r" ((const void *) (x)))
#       define GC_PREFETCH_FOR_WRITE(x) \
          __asm__ __volatile__ ("dcbtst 0,%0" : : "r" ((const void *) (x)))
#     endif
      /* There seems to be some issues with trylock hanging on darwin.  */
      /* This should be looked into some more.                          */
#     define NO_PTHREAD_TRYLOCK
#   endif
#   ifdef OPENBSD
#     define OS_TYPE "OPENBSD"
#     define ALIGNMENT 4
#     ifndef GC_OPENBSD_THREADS
        EXTERN_C_END
#       include <sys/param.h>
#       include <uvm/uvm_extern.h>
        EXTERN_C_BEGIN
        /* USRSTACK is defined in <machine/vmparam.h> but that is       */
        /* protected by _KERNEL in <uvm/uvm_param.h> file.              */
#       ifdef USRSTACK
#         define STACKBOTTOM ((ptr_t)USRSTACK)
#       else
#         define HEURISTIC2
#       endif
#     endif
      extern int __data_start[];
#     define DATASTART ((ptr_t)__data_start)
      extern int _end[];
#     define DATAEND ((ptr_t)(&_end))
#     define DYNAMIC_LOADING
#   endif
#   ifdef FREEBSD
#       if defined(__powerpc64__)
#           define ALIGNMENT 8
#           define CPP_WORDSZ 64
#           ifndef HBLKSIZE
#               define HBLKSIZE 4096
#           endif
#       else
#           define ALIGNMENT 4
#       endif
#       define OS_TYPE "FREEBSD"
#       ifndef GC_FREEBSD_THREADS
#           define MPROTECT_VDB
#       endif
#       define SIG_SUSPEND SIGUSR1
#       define SIG_THR_RESTART SIGUSR2
#       define FREEBSD_STACKBOTTOM
#       ifdef __ELF__
#           define DYNAMIC_LOADING
#       endif
        extern char etext[];
#       define DATASTART GC_FreeBSDGetDataStart(0x1000, (ptr_t)etext)
#       define DATASTART_USES_BSDGETDATASTART
#   endif
#   ifdef NETBSD
#     define ALIGNMENT 4
#     define OS_TYPE "NETBSD"
#     define HEURISTIC2
      extern ptr_t GC_data_start;
#     define DATASTART GC_data_start
#     define DYNAMIC_LOADING
#   endif
#   ifdef SN_TARGET_PS3
#     define NO_GETENV
#     define CPP_WORDSZ 32
#     define ALIGNMENT 4
      extern int _end[];
      extern int __bss_start;
#     define DATASTART ((ptr_t)(__bss_start))
#     define DATAEND ((ptr_t)(_end))
#     define STACKBOTTOM ((ptr_t)ps3_get_stack_bottom())
#     define NO_PTHREAD_TRYLOCK
                /* Current GC LOCK() implementation for PS3 explicitly  */
                /* use pthread_mutex_lock for some reason.              */
#   endif
#   ifdef AIX
#     define OS_TYPE "AIX"
#     undef ALIGNMENT /* in case it's defined   */
#     undef IA64
      /* DOB: some AIX installs stupidly define IA64 in */
      /* /usr/include/sys/systemcfg.h                   */
#     ifdef __64BIT__
#       define ALIGNMENT 8
#       define CPP_WORDSZ 64
#       define STACKBOTTOM ((ptr_t)0x1000000000000000)
#     else
#       define ALIGNMENT 4
#       define CPP_WORDSZ 32
#       define STACKBOTTOM ((ptr_t)((ulong)&errno))
#     endif
#     define USE_MMAP_ANON
        /* From AIX linker man page:
        _text Specifies the first location of the program.
        _etext Specifies the first location after the program.
        _data Specifies the first location of the data.
        _edata Specifies the first location after the initialized data
        _end or end Specifies the first location after all data.
        */
      extern int _data[], _end[];
#     define DATASTART ((ptr_t)((ulong)_data))
#     define DATAEND ((ptr_t)((ulong)_end))
      extern int errno;
#     define DYNAMIC_LOADING
        /* For really old versions of AIX, this may have to be removed. */
#   endif

#   ifdef NOSYS
#     define ALIGNMENT 4
#     define OS_TYPE "NOSYS"
      extern void __end[], __dso_handle[];
#     define DATASTART ((ptr_t)__dso_handle) /* OK, that's ugly.    */
#     define DATAEND ((ptr_t)(__end))
        /* Stack starts at 0xE0000000 for the simulator.  */
#     undef STACK_GRAN
#     define STACK_GRAN 0x10000000
#     define HEURISTIC1
#   endif
# endif

# ifdef NACL
#   define OS_TYPE "NACL"
#   if defined(__GLIBC__)
#     define DYNAMIC_LOADING
#   endif
#   define DATASTART ((ptr_t)0x10020000)
    extern int _end[];
#   define DATAEND ((ptr_t)_end)
#   undef STACK_GRAN
#   define STACK_GRAN 0x10000
#   define HEURISTIC1
#   define NO_PTHREAD_GETATTR_NP
#   define USE_MUNMAP
#   define USE_MMAP_ANON
#   define GETPAGESIZE() 65536
#   define MAX_NACL_GC_THREADS 1024
# endif

# ifdef VAX
#   define MACH_TYPE "VAX"
#   define ALIGNMENT 4  /* Pointers are longword aligned by 4.2 C compiler */
    extern char etext[];
#   define DATASTART ((ptr_t)(etext))
#   ifdef BSD
#       define OS_TYPE "BSD"
#       define HEURISTIC1
                        /* HEURISTIC2 may be OK, but it's hard to test. */
#   endif
#   ifdef ULTRIX
#       define OS_TYPE "ULTRIX"
#       define STACKBOTTOM ((ptr_t)0x7fffc800)
#   endif
# endif

# ifdef SPARC
#   define MACH_TYPE "SPARC"
#   if defined(__arch64__) || defined(__sparcv9)
#     define ALIGNMENT 8
#     define CPP_WORDSZ 64
#     define ELF_CLASS ELFCLASS64
#   else
#     define ALIGNMENT 4        /* Required by hardware */
#     define CPP_WORDSZ 32
#   endif
    /* Don't define USE_ASM_PUSH_REGS.  We do use an asm helper, but    */
    /* not to push the registers on the mark stack.                     */
#   ifdef SOLARIS
#       define OS_TYPE "SOLARIS"
        extern int _etext[];
        extern int _end[];
        ptr_t GC_SysVGetDataStart(size_t, ptr_t);
#       define DATASTART GC_SysVGetDataStart(0x10000, (ptr_t)_etext)
#       define DATASTART_IS_FUNC
#       define DATAEND ((ptr_t)(_end))
#       if !defined(USE_MMAP) && defined(REDIRECT_MALLOC)
#         define USE_MMAP
            /* Otherwise we now use calloc.  Mmap may result in the     */
            /* heap interleaved with thread stacks, which can result in */
            /* excessive blacklisting.  Sbrk is unusable since it       */
            /* doesn't interact correctly with the system malloc.       */
#       endif
#       ifdef USE_MMAP
#         define HEAP_START (ptr_t)0x40000000
#       else
#         define HEAP_START DATAEND
#       endif
#       define PROC_VDB
        /* HEURISTIC1 reportedly no longer works under 2.7.             */
        /* HEURISTIC2 probably works, but this appears to be preferable.*/
        /* Apparently USRSTACK is defined to be USERLIMIT, but in some  */
        /* installations that's undefined.  We work around this with a  */
        /* gross hack:                                                  */
        EXTERN_C_END
#       include <sys/vmparam.h>
#       include <unistd.h>
        EXTERN_C_BEGIN
#       ifdef USERLIMIT
          /* This should work everywhere, but doesn't.  */
#         define STACKBOTTOM ((ptr_t)USRSTACK)
#       else
#         define HEURISTIC2
#       endif
#       define GETPAGESIZE() (unsigned)sysconf(_SC_PAGESIZE)
                /* getpagesize() appeared to be missing from at least one */
                /* Solaris 5.4 installation.  Weird.                      */
#       define DYNAMIC_LOADING
#   endif
#   ifdef DRSNX
#       define OS_TYPE "DRSNX"
        extern int etext[];
        ptr_t GC_SysVGetDataStart(size_t, ptr_t);
#       define DATASTART GC_SysVGetDataStart(0x10000, (ptr_t)etext)
#       define DATASTART_IS_FUNC
#       define MPROTECT_VDB
#       define STACKBOTTOM ((ptr_t)0xdfff0000)
#       define DYNAMIC_LOADING
#   endif
#   ifdef LINUX
#     define OS_TYPE "LINUX"
#     ifdef __ELF__
#       define DYNAMIC_LOADING
#     else
#       error --> Linux SPARC a.out not supported
#     endif
      extern int _end[];
      extern int _etext[];
#     define DATAEND ((ptr_t)(_end))
#     define SVR4
      ptr_t GC_SysVGetDataStart(size_t, ptr_t);
#     ifdef __arch64__
#       define DATASTART GC_SysVGetDataStart(0x100000, (ptr_t)_etext)
#     else
#       define DATASTART GC_SysVGetDataStart(0x10000, (ptr_t)_etext)
#     endif
#     define DATASTART_IS_FUNC
#     define LINUX_STACKBOTTOM
#   endif
#   ifdef OPENBSD
#     define OS_TYPE "OPENBSD"
#     ifndef GC_OPENBSD_THREADS
        EXTERN_C_END
#       include <sys/param.h>
#       include <uvm/uvm_extern.h>
        EXTERN_C_BEGIN
#       ifdef USRSTACK
#         define STACKBOTTOM ((ptr_t)USRSTACK)
#       else
#         define HEURISTIC2
#       endif
#     endif
      extern int __data_start[];
#     define DATASTART ((ptr_t)__data_start)
      extern int _end[];
#     define DATAEND ((ptr_t)(&_end))
#     define DYNAMIC_LOADING
#   endif
#   ifdef NETBSD
#     define OS_TYPE "NETBSD"
#     define HEURISTIC2
#     ifdef __ELF__
        extern ptr_t GC_data_start;
#       define DATASTART GC_data_start
#       define DYNAMIC_LOADING
#     else
        extern char etext[];
#       define DATASTART ((ptr_t)(etext))
#     endif
#   endif
#   ifdef FREEBSD
#       define OS_TYPE "FREEBSD"
#       define SIG_SUSPEND SIGUSR1
#       define SIG_THR_RESTART SIGUSR2
#       define FREEBSD_STACKBOTTOM
#       ifdef __ELF__
#           define DYNAMIC_LOADING
#       endif
        extern char etext[];
        extern char edata[];
        extern char end[];
#       define NEED_FIND_LIMIT
#       define DATASTART ((ptr_t)(&etext))
        ptr_t GC_find_limit(ptr_t, GC_bool);
#       define DATAEND GC_find_limit(DATASTART, TRUE)
#       define DATAEND_IS_FUNC
#       define GC_HAVE_DATAREGION2
#       define DATASTART2 ((ptr_t)(&edata))
#       define DATAEND2 ((ptr_t)(&end))
#   endif
# endif

# ifdef I386
#   define MACH_TYPE "I386"
#   if defined(__LP64__) || defined(_WIN64)
#     error This should be handled as X86_64
#   else
#     define CPP_WORDSZ 32
#     define ALIGNMENT 4
                        /* Appears to hold for all "32 bit" compilers   */
                        /* except Borland.  The -a4 option fixes        */
                        /* Borland.  For Watcom the option is -zp4.     */
#   endif
#   ifdef SEQUENT
#       define OS_TYPE "SEQUENT"
        extern int etext[];
#       define DATASTART ((ptr_t)((((word)(etext)) + 0xfff) & ~0xfff))
#       define STACKBOTTOM ((ptr_t)0x3ffff000)
#   endif
#   ifdef HAIKU
#     define OS_TYPE "HAIKU"
      EXTERN_C_END
#     include <OS.h>
      EXTERN_C_BEGIN
#     define GETPAGESIZE() (unsigned)B_PAGE_SIZE
      extern int etext[];
#     define DATASTART ((ptr_t)((((word)(etext)) + 0xfff) & ~0xfff))
#     define DYNAMIC_LOADING
#     define MPROTECT_VDB
#   endif
#   ifdef SOLARIS
#       define OS_TYPE "SOLARIS"
        extern int _etext[], _end[];
        ptr_t GC_SysVGetDataStart(size_t, ptr_t);
#       define DATASTART GC_SysVGetDataStart(0x1000, (ptr_t)_etext)
#       define DATASTART_IS_FUNC
#       define DATAEND ((ptr_t)(_end))
        /* # define STACKBOTTOM ((ptr_t)(_start)) worked through 2.7,   */
        /* but reportedly breaks under 2.8.  It appears that the stack  */
        /* base is a property of the executable, so this should not     */
        /* break old executables.                                       */
        /* HEURISTIC2 probably works, but this appears to be preferable.*/
        /* Apparently USRSTACK is defined to be USERLIMIT, but in some  */
        /* installations that's undefined.  We work around this with a  */
        /* gross hack:                                                  */
        EXTERN_C_END
#       include <sys/vmparam.h>
        EXTERN_C_BEGIN
#       ifdef USERLIMIT
          /* This should work everywhere, but doesn't.  */
#         define STACKBOTTOM ((ptr_t)USRSTACK)
#       else
#         define HEURISTIC2
#       endif
/* At least in Solaris 2.5, PROC_VDB gives wrong values for dirty bits. */
/* It appears to be fixed in 2.8 and 2.9.                               */
#       ifdef SOLARIS25_PROC_VDB_BUG_FIXED
#         define PROC_VDB
#       endif
#       ifndef GC_THREADS
#         define MPROTECT_VDB
#       endif
#       define DYNAMIC_LOADING
#       if !defined(USE_MMAP) && defined(REDIRECT_MALLOC)
#         define USE_MMAP
            /* Otherwise we now use calloc.  Mmap may result in the     */
            /* heap interleaved with thread stacks, which can result in */
            /* excessive blacklisting.  Sbrk is unusable since it       */
            /* doesn't interact correctly with the system malloc.       */
#       endif
#       ifdef USE_MMAP
#         define HEAP_START (ptr_t)0x40000000
#       else
#         define HEAP_START DATAEND
#       endif
#   endif
#   ifdef SCO
#       define OS_TYPE "SCO"
        extern int etext[];
#       define DATASTART ((ptr_t)((((word)(etext)) + 0x3fffff) & ~0x3fffff) \
                                 + ((word)(etext) & 0xfff))
#       define STACKBOTTOM ((ptr_t)0x7ffffffc)
#   endif
#   ifdef SCO_ELF
#       define OS_TYPE "SCO_ELF"
        extern int etext[];
#       define DATASTART ((ptr_t)(etext))
#       define STACKBOTTOM ((ptr_t)0x08048000)
#       define DYNAMIC_LOADING
#       define ELF_CLASS ELFCLASS32
#   endif
#   ifdef DGUX
#       define OS_TYPE "DGUX"
        extern int _etext, _end;
        ptr_t GC_SysVGetDataStart(size_t, ptr_t);
#       define DATASTART GC_SysVGetDataStart(0x1000, (ptr_t)(&_etext))
#       define DATASTART_IS_FUNC
#       define DATAEND ((ptr_t)(&_end))
#       define STACK_GROWS_DOWN
#       define HEURISTIC2
        EXTERN_C_END
#       include <unistd.h>
        EXTERN_C_BEGIN
#       define GETPAGESIZE() (unsigned)sysconf(_SC_PAGESIZE)
#       define DYNAMIC_LOADING
#       ifndef USE_MMAP
#         define USE_MMAP
#       endif
#       define MAP_FAILED (void *) ((word)-1)
#       define HEAP_START (ptr_t)0x40000000
#   endif /* DGUX */
#   ifdef LINUX
#       define OS_TYPE "LINUX"
#       define LINUX_STACKBOTTOM
#       if !defined(GC_LINUX_THREADS) || !defined(REDIRECT_MALLOC)
#           define MPROTECT_VDB
#       else
            /* We seem to get random errors in incremental mode,        */
            /* possibly because Linux threads is itself a malloc client */
            /* and can't deal with the signals.                         */
#       endif
#       define HEAP_START (ptr_t)0x1000
                /* This encourages mmap to give us low addresses,       */
                /* thus allowing the heap to grow to ~3GB               */
#       ifdef __ELF__
#           define DYNAMIC_LOADING
            EXTERN_C_END
#           include <features.h>
            EXTERN_C_BEGIN
#            if defined(__GLIBC__) && __GLIBC__ >= 2 \
                || defined(HOST_ANDROID) || defined(HOST_TIZEN)
#                define SEARCH_FOR_DATA_START
#            else
                 extern char **__environ;
#                define DATASTART ((ptr_t)(&__environ))
                              /* hideous kludge: __environ is the first */
                              /* word in crt0.o, and delimits the start */
                              /* of the data segment, no matter which   */
                              /* ld options were passed through.        */
                              /* We could use _etext instead, but that  */
                              /* would include .rodata, which may       */
                              /* contain large read-only data tables    */
                              /* that we'd rather not scan.             */
#            endif
             extern int _end[];
#            define DATAEND ((ptr_t)(_end))
#            if !defined(GC_NO_SIGSETJMP) && (defined(HOST_TIZEN) \
                    || (defined(HOST_ANDROID) \
                        && !(GC_GNUC_PREREQ(4, 8) || GC_CLANG_PREREQ(3, 2) \
                             || __ANDROID_API__ >= 18)))
               /* Older Android NDK releases lack sigsetjmp in x86 libc */
               /* (setjmp is used instead to find data_start).  The bug */
               /* is fixed in Android NDK r8e (so, ok to use sigsetjmp  */
               /* if gcc4.8+, clang3.2+ or Android API level 18+).      */
#              define GC_NO_SIGSETJMP
#            endif
#       else
             extern int etext[];
#            define DATASTART ((ptr_t)((((word)(etext)) + 0xfff) & ~0xfff))
#       endif
#       ifdef USE_I686_PREFETCH
#         define PREFETCH(x) \
            __asm__ __volatile__ ("prefetchnta %0" : : "m"(*(char *)(x)))
            /* Empirically prefetcht0 is much more effective at reducing     */
            /* cache miss stalls for the targeted load instructions.  But it */
            /* seems to interfere enough with other cache traffic that the   */
            /* net result is worse than prefetchnta.                         */
#         ifdef FORCE_WRITE_PREFETCH
            /* Using prefetches for write seems to have a slight negative    */
            /* impact on performance, at least for a PIII/500.               */
#           define GC_PREFETCH_FOR_WRITE(x) \
              __asm__ __volatile__ ("prefetcht0 %0" : : "m"(*(char *)(x)))
#         else
#           define GC_NO_PREFETCH_FOR_WRITE
#         endif
#       elif defined(USE_3DNOW_PREFETCH)
#         define PREFETCH(x) \
            __asm__ __volatile__ ("prefetch %0" : : "m"(*(char *)(x)))
#         define GC_PREFETCH_FOR_WRITE(x) \
            __asm__ __volatile__ ("prefetchw %0" : : "m"(*(char *)(x)))
#       endif
#       if defined(__GLIBC__) && !defined(__UCLIBC__)
          /* Workaround lock elision implementation for some glibc.     */
#         define GLIBC_2_19_TSX_BUG
          EXTERN_C_END
#         include <gnu/libc-version.h> /* for gnu_get_libc_version() */
          EXTERN_C_BEGIN
#       endif
#   endif
#   ifdef CYGWIN32
#       define OS_TYPE "CYGWIN32"
#       define DATASTART ((ptr_t)GC_DATASTART)  /* From gc.h */
#       define DATAEND   ((ptr_t)GC_DATAEND)
#       undef STACK_GRAN
#       define STACK_GRAN 0x10000
#       ifdef USE_MMAP
#         define NEED_FIND_LIMIT
#         define USE_MMAP_ANON
#       endif
#   endif
#   ifdef OS2
#       define OS_TYPE "OS2"
                /* STACKBOTTOM and DATASTART are handled specially in   */
                /* os_dep.c. OS2 actually has the right                 */
                /* system call!                                         */
#       define DATAEND  /* not needed */
#   endif
#   ifdef MSWIN32
#       define OS_TYPE "MSWIN32"
                /* STACKBOTTOM and DATASTART are handled specially in   */
                /* os_dep.c.                                            */
#       define MPROTECT_VDB
#       define GWW_VDB
#       define DATAEND  /* not needed */
#   endif
#   ifdef MSWINCE
#       define OS_TYPE "MSWINCE"
#       define DATAEND  /* not needed */
#   endif
#   ifdef DJGPP
#       define OS_TYPE "DJGPP"
        EXTERN_C_END
#       include "stubinfo.h"
        EXTERN_C_BEGIN
        extern int etext[];
        extern int _stklen;
        extern int __djgpp_stack_limit;
#       define DATASTART ((ptr_t)((((word)(etext)) + 0x1ff) & ~0x1ff))
/* #define STACKBOTTOM ((ptr_t)((word)_stubinfo+_stubinfo->size+_stklen)) */
#       define STACKBOTTOM ((ptr_t)((word)__djgpp_stack_limit + _stklen))
                /* This may not be right.  */
#   endif
#   ifdef OPENBSD
#       define OS_TYPE "OPENBSD"
#       ifndef GC_OPENBSD_THREADS
          EXTERN_C_END
#         include <sys/param.h>
#         include <uvm/uvm_extern.h>
          EXTERN_C_BEGIN
#         ifdef USRSTACK
#           define STACKBOTTOM ((ptr_t)USRSTACK)
#         else
#           define HEURISTIC2
#         endif
#       endif
        extern int __data_start[];
#       define DATASTART ((ptr_t)__data_start)
        extern int _end[];
#       define DATAEND ((ptr_t)(&_end))
#       define DYNAMIC_LOADING
#   endif
#   ifdef FREEBSD
#       define OS_TYPE "FREEBSD"
#       ifndef GC_FREEBSD_THREADS
#           define MPROTECT_VDB
#       endif
#       ifdef __GLIBC__
#           define SIG_SUSPEND          (32+6)
#           define SIG_THR_RESTART      (32+5)
            extern int _end[];
#           define DATAEND ((ptr_t)(_end))
#       else
#           define SIG_SUSPEND SIGUSR1
#           define SIG_THR_RESTART SIGUSR2
                /* SIGTSTP and SIGCONT could be used alternatively.     */
#       endif
#       define FREEBSD_STACKBOTTOM
#       ifdef __ELF__
#           define DYNAMIC_LOADING
#       endif
        extern char etext[];
#       define DATASTART GC_FreeBSDGetDataStart(0x1000, (ptr_t)etext)
#       define DATASTART_USES_BSDGETDATASTART
#   endif
#   ifdef NETBSD
#       define OS_TYPE "NETBSD"
#       ifdef __ELF__
#           define DYNAMIC_LOADING
#       endif
#   endif
#   ifdef THREE86BSD
#       define OS_TYPE "THREE86BSD"
#   endif
#   ifdef BSDI
#       define OS_TYPE "BSDI"
#   endif
#   if defined(NETBSD) || defined(THREE86BSD) || defined(BSDI)
#       define HEURISTIC2
        extern char etext[];
#       define DATASTART ((ptr_t)(etext))
#   endif
#   ifdef NEXT
#       define OS_TYPE "NEXT"
#       define DATASTART ((ptr_t)get_etext())
#       define DATASTART_IS_FUNC
#       define STACKBOTTOM ((ptr_t)0xc0000000)
#       define DATAEND  /* not needed */
#   endif
#   ifdef RTEMS
#       define OS_TYPE "RTEMS"
        EXTERN_C_END
#       include <sys/unistd.h>
        EXTERN_C_BEGIN
        extern int etext[];
        extern int end[];
        void *rtems_get_stack_bottom(void);
#       define InitStackBottom rtems_get_stack_bottom()
#       define DATASTART ((ptr_t)etext)
#       define DATAEND ((ptr_t)end)
#       define STACKBOTTOM ((ptr_t)InitStackBottom)
#       define SIG_SUSPEND SIGUSR1
#       define SIG_THR_RESTART SIGUSR2
#   endif
#   ifdef DOS4GW
#     define OS_TYPE "DOS4GW"
      extern long __nullarea;
      extern char _end;
      extern char *_STACKTOP;
      /* Depending on calling conventions Watcom C either precedes      */
      /* or does not precedes with underscore names of C-variables.     */
      /* Make sure startup code variables always have the same names.   */
      #pragma aux __nullarea "*";
      #pragma aux _end "*";
#     define STACKBOTTOM ((ptr_t)_STACKTOP)
                         /* confused? me too. */
#     define DATASTART ((ptr_t)(&__nullarea))
#     define DATAEND ((ptr_t)(&_end))
#   endif
#   ifdef HURD
#     define OS_TYPE "HURD"
#     define STACK_GROWS_DOWN
#     define HEURISTIC2
#     define SIG_SUSPEND SIGUSR1
#     define SIG_THR_RESTART SIGUSR2
#     define SEARCH_FOR_DATA_START
      extern int _end[];
#     define DATAEND ((ptr_t)(_end))
/* #     define MPROTECT_VDB  Not quite working yet? */
#     define DYNAMIC_LOADING
#     define USE_MMAP_ANON
#   endif
#   ifdef DARWIN
#     define OS_TYPE "DARWIN"
#     define DARWIN_DONT_PARSE_STACK
#     ifndef GC_DONT_REGISTER_MAIN_STATIC_DATA
#       define DYNAMIC_LOADING
#     endif
      /* XXX: see get_end(3), get_etext() and get_end() should not be used. */
      /* These aren't used when dyld support is enabled (it is by default). */
#     define DATASTART ((ptr_t)get_etext())
#     define DATAEND   ((ptr_t)get_end())
#     define STACKBOTTOM ((ptr_t)0xc0000000)
#     define USE_MMAP_ANON
#     define MPROTECT_VDB
      EXTERN_C_END
#     include <unistd.h>
      EXTERN_C_BEGIN
#     define GETPAGESIZE() (unsigned)getpagesize()
      /* There seems to be some issues with trylock hanging on darwin.  */
      /* This should be looked into some more.                          */
#     define NO_PTHREAD_TRYLOCK
#     if TARGET_OS_IPHONE && !defined(NO_DYLD_BIND_FULLY_IMAGE)
        /* iPhone/iPad simulator */
#       define NO_DYLD_BIND_FULLY_IMAGE
#     endif
#   endif /* DARWIN */
# endif

# ifdef NS32K
#   define MACH_TYPE "NS32K"
#   define ALIGNMENT 4
    extern char **environ;
#   define DATASTART ((ptr_t)(&environ))
                              /* hideous kludge: environ is the first   */
                              /* word in crt0.o, and delimits the start */
                              /* of the data segment, no matter which   */
                              /* ld options were passed through.        */
#   define STACKBOTTOM ((ptr_t)0xfffff000) /* for Encore */
# endif

# ifdef MIPS
#   define MACH_TYPE "MIPS"
#   ifdef LINUX
#     define OS_TYPE "LINUX"
#     define DYNAMIC_LOADING
      extern int _end[];
#     pragma weak __data_start
      extern int __data_start[];
#     define DATASTART ((ptr_t)(__data_start))
#     define DATAEND ((ptr_t)(_end))
#     ifdef _MIPS_SZPTR
#       define CPP_WORDSZ _MIPS_SZPTR
#       define ALIGNMENT (_MIPS_SZPTR/8)
#     else
#       define ALIGNMENT 4
#     endif
#     ifndef HBLKSIZE
#       define HBLKSIZE 4096
#     endif
#     if __GLIBC__ == 2 && __GLIBC_MINOR__ >= 2 || __GLIBC__ > 2
#       define LINUX_STACKBOTTOM
#     else
#       define STACKBOTTOM ((ptr_t)0x7fff8000)
#     endif
#   endif /* Linux */
#   ifdef EWS4800
#      define HEURISTIC2
#      if defined(_MIPS_SZPTR) && (_MIPS_SZPTR == 64)
         extern int _fdata[], _end[];
#        define DATASTART ((ptr_t)_fdata)
#        define DATAEND ((ptr_t)_end)
#        define CPP_WORDSZ _MIPS_SZPTR
#        define ALIGNMENT (_MIPS_SZPTR/8)
#      else
         extern int etext[], edata[], end[];
         extern int _DYNAMIC_LINKING[], _gp[];
#        define DATASTART ((ptr_t)((((word)(etext) + 0x3ffff) & ~0x3ffff) \
                                   + ((word)(etext) & 0xffff)))
#        define DATAEND ((ptr_t)(edata))
#        define GC_HAVE_DATAREGION2
#        define DATASTART2 (_DYNAMIC_LINKING \
               ? (ptr_t)(((word)_gp + 0x8000 + 0x3ffff) & ~0x3ffff) \
               : (ptr_t)edata)
#        define DATAEND2 ((ptr_t)(end))
#        define ALIGNMENT 4
#      endif
#      define OS_TYPE "EWS4800"
#   endif
#   ifdef ULTRIX
#       define HEURISTIC2
#       define DATASTART ((ptr_t)0x10000000)
                              /* Could probably be slightly higher since */
                              /* startup code allocates lots of stuff.   */
#       define OS_TYPE "ULTRIX"
#       define ALIGNMENT 4
#   endif
#   ifdef IRIX5
#       define HEURISTIC2
        extern int _fdata[];
#       define DATASTART ((ptr_t)(_fdata))
#       ifdef USE_MMAP
#         define HEAP_START (ptr_t)0x30000000
#       else
#         define HEAP_START DATASTART
#       endif
                              /* Lowest plausible heap address.         */
                              /* In the MMAP case, we map there.        */
                              /* In either case it is used to identify  */
                              /* heap sections so they're not           */
                              /* considered as roots.                   */
#       define OS_TYPE "IRIX5"
/*#       define MPROTECT_VDB DOB: this should work, but there is evidence */
/*              of recent breakage.                                        */
#       ifdef _MIPS_SZPTR
#         define CPP_WORDSZ _MIPS_SZPTR
#         define ALIGNMENT (_MIPS_SZPTR/8)
#       else
#         define ALIGNMENT 4
#       endif
#       define DYNAMIC_LOADING
#   endif
#   ifdef MSWINCE
#       define OS_TYPE "MSWINCE"
#       define ALIGNMENT 4
#       define DATAEND /* not needed */
#   endif
#   if defined(NETBSD)
#     define OS_TYPE "NETBSD"
#     define ALIGNMENT 4
#     define HEURISTIC2
#     ifdef __ELF__
        extern ptr_t GC_data_start;
#       define DATASTART GC_data_start
#       define NEED_FIND_LIMIT
#       define DYNAMIC_LOADING
#     else
#       define DATASTART ((ptr_t)0x10000000)
#       define STACKBOTTOM ((ptr_t)0x7ffff000)
#     endif /* _ELF_ */
#  endif
#  ifdef OPENBSD
#     define OS_TYPE "OPENBSD"
#     define ALIGNMENT 4
#     ifndef GC_OPENBSD_THREADS
        EXTERN_C_END
#       include <sys/param.h>
#       include <uvm/uvm_extern.h>
        EXTERN_C_BEGIN
#       ifdef USRSTACK
#         define STACKBOTTOM ((ptr_t)USRSTACK)
#       else
#         define HEURISTIC2
#       endif
#     endif
      extern int _fdata[];
#     define DATASTART ((ptr_t)_fdata)
      extern int _end[];
#     define DATAEND ((ptr_t)(&_end))
#     define DYNAMIC_LOADING
#  endif
#  ifdef FREEBSD
#    define OS_TYPE "FREEBSD"
#    define ALIGNMENT 4
#    ifndef GC_FREEBSD_THREADS
#      define MPROTECT_VDB
#    endif
#    define SIG_SUSPEND SIGUSR1
#    define SIG_THR_RESTART SIGUSR2
#    define FREEBSD_STACKBOTTOM
#    ifdef __ELF__
#      define DYNAMIC_LOADING
#    endif
     extern char etext[];
#    define DATASTART GC_FreeBSDGetDataStart(0x1000, (ptr_t)etext)
#    define DATASTART_USES_BSDGETDATASTART
#  endif /* FreeBSD */
#  if defined(NONSTOP)
#    define CPP_WORDSZ 32
#    define OS_TYPE "NONSTOP"
#    define ALIGNMENT 4
#    define DATASTART ((ptr_t)0x08000000)
     extern char **environ;
#    define DATAEND ((ptr_t)(environ - 0x10))
#    define STACKBOTTOM ((ptr_t)0x4fffffff)
#   endif
# endif

# ifdef NIOS2
#  define CPP_WORDSZ 32
#  define MACH_TYPE "NIOS2"
#  ifdef LINUX
#    define OS_TYPE "LINUX"
#    define DYNAMIC_LOADING
     extern int _end[];
     extern int __data_start[];
#    define DATASTART ((ptr_t)(__data_start))
#    define DATAEND ((ptr_t)(_end))
#    define ALIGNMENT 4
#    ifndef HBLKSIZE
#      define HBLKSIZE 4096
#    endif
#    define LINUX_STACKBOTTOM
#  endif /* Linux */
# endif

# ifdef OR1K
#   define CPP_WORDSZ 32
#   define MACH_TYPE "OR1K"
#   ifdef LINUX
#     define OS_TYPE "LINUX"
#     define DYNAMIC_LOADING
      extern int _end[];
      extern int __data_start[];
#     define DATASTART ((ptr_t)(__data_start))
#     define DATAEND ((ptr_t)(_end))
#     define ALIGNMENT 4
#     ifndef HBLKSIZE
#       define HBLKSIZE 4096
#     endif
#     define LINUX_STACKBOTTOM
#   endif /* Linux */
# endif

# ifdef HP_PA
#   define MACH_TYPE "HP_PA"
#   ifdef __LP64__
#     define CPP_WORDSZ 64
#     define ALIGNMENT 8
#   else
#     define CPP_WORDSZ 32
#     define ALIGNMENT 4
#   endif
#   if !defined(GC_HPUX_THREADS) && !defined(GC_LINUX_THREADS) \
       && !defined(OPENBSD) && !defined(LINUX) /* For now. */
#     define MPROTECT_VDB
#   endif
#   define STACK_GROWS_UP
#   ifdef HPUX
#     define OS_TYPE "HPUX"
      extern int __data_start[];
#     define DATASTART ((ptr_t)(__data_start))
#     ifdef USE_HPUX_FIXED_STACKBOTTOM
        /* The following appears to work for 7xx systems running HP/UX  */
        /* 9.xx.  Furthermore, it might result in much faster           */
        /* collections than HEURISTIC2, which may involve scanning      */
        /* segments that directly precede the stack.  It is not the     */
        /* default, since it may not work on older machine/OS           */
        /* combinations. (Thanks to Raymond X.T. Nijssen for uncovering */
        /* this.)                                                       */
#       define STACKBOTTOM ((ptr_t)0x7b033000) /* from /etc/conf/h/param.h */
#     else
        /* Gustavo Rodriguez-Rivera suggested changing HEURISTIC2       */
        /* to this.  Note that the GC must be initialized before the    */
        /* first putenv call.                                           */
        extern char ** environ;
#       define STACKBOTTOM ((ptr_t)environ)
#     endif
#     define DYNAMIC_LOADING
      EXTERN_C_END
#     include <unistd.h>
      EXTERN_C_BEGIN
#     define GETPAGESIZE() (unsigned)sysconf(_SC_PAGE_SIZE)
#     ifndef __GNUC__
#       define PREFETCH(x)  do { \
                              register long addr = (long)(x); \
                              (void) _asm ("LDW", 0, 0, addr, 0); \
                            } while (0)
#     endif
#   endif /* HPUX */
#   ifdef LINUX
#     define OS_TYPE "LINUX"
#     define LINUX_STACKBOTTOM
#     define DYNAMIC_LOADING
#     define SEARCH_FOR_DATA_START
      extern int _end[];
#     define DATAEND ((ptr_t)(&_end))
#   endif /* LINUX */
#  ifdef OPENBSD
#     define OS_TYPE "OPENBSD"
#     ifndef GC_OPENBSD_THREADS
        EXTERN_C_END
#       include <sys/param.h>
#       include <uvm/uvm_extern.h>
        EXTERN_C_BEGIN
#       ifdef USRSTACK
#         define STACKBOTTOM ((ptr_t)USRSTACK)
#       else
#         define HEURISTIC2
#       endif
#     endif
      extern int __data_start[];
#     define DATASTART ((ptr_t)__data_start)
      extern int _end[];
#     define DATAEND ((ptr_t)(&_end))
#     define DYNAMIC_LOADING
#  endif
# endif /* HP_PA */

# ifdef ALPHA
#   define MACH_TYPE "ALPHA"
#   define ALIGNMENT 8
#   define CPP_WORDSZ 64
#   ifdef NETBSD
#       define OS_TYPE "NETBSD"
#       define HEURISTIC2
        extern ptr_t GC_data_start;
#       define DATASTART GC_data_start
#       define ELFCLASS32 32
#       define ELFCLASS64 64
#       define ELF_CLASS ELFCLASS64
#       define DYNAMIC_LOADING
#   endif
#   ifdef OPENBSD
#       define OS_TYPE "OPENBSD"
#       define ELF_CLASS ELFCLASS64
#       ifndef GC_OPENBSD_THREADS
          EXTERN_C_END
#         include <sys/param.h>
#         include <uvm/uvm_extern.h>
          EXTERN_C_BEGIN
#         ifdef USRSTACK
#           define STACKBOTTOM ((ptr_t)USRSTACK)
#         else
#           define HEURISTIC2
#         endif
#       endif
        extern int __data_start[];
#       define DATASTART ((ptr_t)__data_start)
        extern int _end[];
#       define DATAEND ((ptr_t)(&_end))
#       define DYNAMIC_LOADING
#   endif
#   ifdef FREEBSD
#       define OS_TYPE "FREEBSD"
/* MPROTECT_VDB is not yet supported at all on FreeBSD/alpha. */
#       define SIG_SUSPEND SIGUSR1
#       define SIG_THR_RESTART SIGUSR2
                /* SIGTSTP and SIGCONT could be used alternatively.     */
#       define FREEBSD_STACKBOTTOM
#       ifdef __ELF__
#           define DYNAMIC_LOADING
#       endif
/* Handle unmapped hole alpha*-*-freebsd[45]* puts between etext and edata. */
        extern char etext[];
        extern char edata[];
        extern char end[];
#       define NEED_FIND_LIMIT
#       define DATASTART ((ptr_t)(&etext))
        ptr_t GC_find_limit(ptr_t, GC_bool);
#       define DATAEND GC_find_limit(DATASTART, TRUE)
#       define DATAEND_IS_FUNC
#       define GC_HAVE_DATAREGION2
#       define DATASTART2 ((ptr_t)(&edata))
#       define DATAEND2 ((ptr_t)(&end))
#   endif
#   ifdef OSF1
#       define OS_TYPE "OSF1"
#       define DATASTART ((ptr_t)0x140000000)
        extern int _end[];
#       define DATAEND ((ptr_t)(&_end))
        extern char ** environ;
        /* round up from the value of environ to the nearest page boundary */
        /* Probably breaks if putenv is called before collector            */
        /* initialization.                                                 */
#       define STACKBOTTOM ((ptr_t)(((word)(environ) | (getpagesize()-1))+1))
/* #    define HEURISTIC2 */
        /* Normally HEURISTIC2 is too conservative, since               */
        /* the text segment immediately follows the stack.              */
        /* Hence we give an upper pound.                                */
        /* This is currently unused, since we disabled HEURISTIC2       */
        extern int __start[];
#       define HEURISTIC2_LIMIT ((ptr_t)((word)(__start) & ~(getpagesize()-1)))
#       ifndef GC_OSF1_THREADS
          /* Unresolved signal issues with threads.     */
#         define MPROTECT_VDB
#       endif
#       define DYNAMIC_LOADING
#   endif
#   ifdef LINUX
#       define OS_TYPE "LINUX"
#       define LINUX_STACKBOTTOM
#       ifdef __ELF__
#         define SEARCH_FOR_DATA_START
#         define DYNAMIC_LOADING
#       else
#           define DATASTART ((ptr_t)0x140000000)
#       endif
        extern int _end[];
#       define DATAEND ((ptr_t)(_end))
#       define MPROTECT_VDB
                /* Has only been superficially tested.  May not */
                /* work on all versions.                        */
#   endif
# endif

# ifdef IA64
#   define MACH_TYPE "IA64"
#   ifdef HPUX
#       ifdef _ILP32
#         define CPP_WORDSZ 32
            /* Requires 8 byte alignment for malloc */
#         define ALIGNMENT 4
#       else
#         ifndef _LP64
#           error --> unknown ABI
#         endif
#         define CPP_WORDSZ 64
            /* Requires 16 byte alignment for malloc */
#         define ALIGNMENT 8
#       endif
#       define OS_TYPE "HPUX"
        extern int __data_start[];
#       define DATASTART ((ptr_t)(__data_start))
        /* Gustavo Rodriguez-Rivera suggested changing HEURISTIC2       */
        /* to this.  Note that the GC must be initialized before the    */
        /* first putenv call.                                           */
        extern char ** environ;
#       define STACKBOTTOM ((ptr_t)environ)
#       define HPUX_STACKBOTTOM
#       define DYNAMIC_LOADING
        EXTERN_C_END
#       include <unistd.h>
        EXTERN_C_BEGIN
#       define GETPAGESIZE() (unsigned)sysconf(_SC_PAGE_SIZE)
        /* The following was empirically determined, and is probably    */
        /* not very robust.                                             */
        /* Note that the backing store base seems to be at a nice       */
        /* address minus one page.                                      */
#       define BACKING_STORE_DISPLACEMENT 0x1000000
#       define BACKING_STORE_ALIGNMENT 0x1000
        extern ptr_t GC_register_stackbottom;
#       define BACKING_STORE_BASE GC_register_stackbottom
        /* Known to be wrong for recent HP/UX versions!!!       */
#   endif
#   ifdef LINUX
#       define CPP_WORDSZ 64
#       define ALIGNMENT 8
#       define OS_TYPE "LINUX"
        /* The following works on NUE and older kernels:        */
/* #       define STACKBOTTOM ((ptr_t) 0xa000000000000000l)     */
        /* This does not work on NUE:                           */
#       define LINUX_STACKBOTTOM
        /* We also need the base address of the register stack  */
        /* backing store.                                       */
        extern ptr_t GC_register_stackbottom;
#       define BACKING_STORE_BASE GC_register_stackbottom
#       define SEARCH_FOR_DATA_START
#       ifdef __GNUC__
#         define DYNAMIC_LOADING
#       else
          /* In the Intel compiler environment, we seem to end up with  */
          /* statically linked executables and an undefined reference   */
          /* to _DYNAMIC                                                */
#       endif
#       define MPROTECT_VDB
                /* Requires Linux 2.3.47 or later.      */
        extern int _end[];
#       define DATAEND ((ptr_t)(_end))
#       ifdef __GNUC__
#         ifndef __INTEL_COMPILER
#           define PREFETCH(x) \
              __asm__ ("        lfetch  [%0]": : "r"(x))
#           define GC_PREFETCH_FOR_WRITE(x) \
              __asm__ ("        lfetch.excl     [%0]": : "r"(x))
#           define CLEAR_DOUBLE(x) \
              __asm__ ("        stf.spill       [%0]=f0": : "r"((void *)(x)))
#         else
            EXTERN_C_END
#           include <ia64intrin.h>
            EXTERN_C_BEGIN
#           define PREFETCH(x) __lfetch(__lfhint_none, (x))
#           define GC_PREFETCH_FOR_WRITE(x) __lfetch(__lfhint_nta, (x))
#           define CLEAR_DOUBLE(x) __stf_spill((void *)(x), 0)
#         endif /* __INTEL_COMPILER */
#       endif
#   endif
#   ifdef CYGWIN32
#       define OS_TYPE "CYGWIN32"
#       define DATASTART ((ptr_t)GC_DATASTART)  /* From gc.h */
#       define DATAEND   ((ptr_t)GC_DATAEND)
#       undef STACK_GRAN
#       define STACK_GRAN 0x10000
#       ifdef USE_MMAP
#         define NEED_FIND_LIMIT
#         define USE_MMAP_ANON
#       endif
#   endif
#   ifdef MSWIN32
      /* FIXME: This is a very partial guess.  There is no port, yet.   */
#     define OS_TYPE "MSWIN32"
                /* STACKBOTTOM and DATASTART are handled specially in   */
                /* os_dep.c.                                            */
#     define DATAEND  /* not needed */
#     if defined(_WIN64)
#       define CPP_WORDSZ 64
#     else
#       define CPP_WORDSZ 32   /* Is this possible?     */
#     endif
#     define ALIGNMENT 8
#   endif
# endif

# ifdef M88K
#   define MACH_TYPE "M88K"
#   define ALIGNMENT 4
    extern int etext[];
#   ifdef CX_UX
#       define OS_TYPE "CX_UX"
#       define DATASTART ((ptr_t)((((word)(etext) + 0x3fffff) & ~0x3fffff) \
                                  + 0x10000))
#   endif
#   ifdef  DGUX
#       define OS_TYPE "DGUX"
        ptr_t GC_SysVGetDataStart(size_t, ptr_t);
#       define DATASTART GC_SysVGetDataStart(0x10000, (ptr_t)etext)
#       define DATASTART_IS_FUNC
#   endif
#   define STACKBOTTOM ((char*)0xf0000000) /* determined empirically */
# endif

# ifdef S370
    /* If this still works, and if anyone cares, this should probably   */
    /* be moved to the S390 category.                                   */
#   define MACH_TYPE "S370"
#   define ALIGNMENT 4  /* Required by hardware */
#   ifdef UTS4
#       define OS_TYPE "UTS4"
        extern int etext[];
        extern int _etext[];
        extern int _end[];
        ptr_t GC_SysVGetDataStart(size_t, ptr_t);
#       define DATASTART GC_SysVGetDataStart(0x10000, (ptr_t)_etext)
#       define DATASTART_IS_FUNC
#       define DATAEND ((ptr_t)(_end))
#       define HEURISTIC2
#   endif
# endif

# ifdef S390
#   define MACH_TYPE "S390"
#   ifndef __s390x__
#   define ALIGNMENT 4
#   define CPP_WORDSZ 32
#   else
#   define ALIGNMENT 8
#   define CPP_WORDSZ 64
#   ifndef HBLKSIZE
#     define HBLKSIZE 4096
#   endif
#   endif
#   ifdef LINUX
#       define OS_TYPE "LINUX"
#       define LINUX_STACKBOTTOM
#       define DYNAMIC_LOADING
        extern int __data_start[] __attribute__((__weak__));
#       define DATASTART ((ptr_t)(__data_start))
        extern int _end[] __attribute__((__weak__));
#       define DATAEND ((ptr_t)(_end))
#       define CACHE_LINE_SIZE 256
#       define GETPAGESIZE() 4096
#   endif
# endif

# ifdef AARCH64
#   define MACH_TYPE "AARCH64"
#   ifdef __ILP32__
#     define CPP_WORDSZ 32
#     define ALIGNMENT 4
#   else
#     define CPP_WORDSZ 64
#     define ALIGNMENT 8
#   endif
#   ifndef HBLKSIZE
#     define HBLKSIZE 4096
#   endif
#   ifdef LINUX
#     define OS_TYPE "LINUX"
#     define LINUX_STACKBOTTOM
#     define DYNAMIC_LOADING
      extern int __data_start[];
      extern int _end[];
#     define DATASTART ((ptr_t)__data_start)
#     define DATAEND ((ptr_t)(&_end))
#   endif
#   ifdef DARWIN
      /* iOS */
#     define OS_TYPE "DARWIN"
#     define DARWIN_DONT_PARSE_STACK
#     ifndef GC_DONT_REGISTER_MAIN_STATIC_DATA
#       define DYNAMIC_LOADING
#     endif
#     define DATASTART ((ptr_t)get_etext())
#     define DATAEND   ((ptr_t)get_end())
#     define STACKBOTTOM ((ptr_t)0x16fdfffff)
#     define USE_MMAP_ANON
#     define MPROTECT_VDB
      EXTERN_C_END
#     include <unistd.h>
      EXTERN_C_BEGIN
#     define GETPAGESIZE() (unsigned)getpagesize()
      /* FIXME: There seems to be some issues with trylock hanging on   */
      /* darwin. This should be looked into some more.                  */
#     define NO_PTHREAD_TRYLOCK
#     if TARGET_OS_IPHONE && !defined(NO_DYLD_BIND_FULLY_IMAGE)
#       define NO_DYLD_BIND_FULLY_IMAGE
#     endif
#   endif
#   ifdef FREEBSD
#     define OS_TYPE "FREEBSD"
#     ifndef GC_FREEBSD_THREADS
#       define MPROTECT_VDB
#     endif
#     define FREEBSD_STACKBOTTOM
#     ifdef __ELF__
#       define DYNAMIC_LOADING
#     endif
      extern char etext[];
#     define DATASTART GC_FreeBSDGetDataStart(0x1000, (ptr_t)etext)
#     define DATASTART_USES_BSDGETDATASTART
#   endif
#   ifdef NINTENDO_SWITCH
      static int zero_fd = -1;
#     define OPT_MAP_ANON 0
      extern int __bss_end[];
#     define NO_HANDLE_FORK
#     define GETPAGESIZE() 4096
#     define DATASTART (ptr_t)ALIGNMENT /* cannot be null */
#     define DATAEND (ptr_t)(&__bss_end)
      void *switch_get_stack_bottom(void);
#     define STACKBOTTOM ((ptr_t)switch_get_stack_bottom())
#     undef USE_MMAP
#     undef USE_MUNMAP
#   endif
#   ifdef NOSYS
      /* __data_start is usually defined in the target linker script.   */
      extern int __data_start[];
#     define DATASTART ((ptr_t)__data_start)
      extern void *__stack_base__;
#     define STACKBOTTOM ((ptr_t)__stack_base__)
#   endif
# endif

# ifdef ARM32
#   if defined(NACL)
#     define MACH_TYPE "NACL"
#   else
#     define MACH_TYPE "ARM32"
#   endif
#   define CPP_WORDSZ 32
#   define ALIGNMENT 4
#   ifdef NETBSD
#       define OS_TYPE "NETBSD"
#       define HEURISTIC2
#       ifdef __ELF__
           extern ptr_t GC_data_start;
#          define DATASTART GC_data_start
#          define DYNAMIC_LOADING
#       else
           extern char etext[];
#          define DATASTART ((ptr_t)(etext))
#       endif
#   endif
#   ifdef LINUX
#       define OS_TYPE "LINUX"
#       define LINUX_STACKBOTTOM
#       undef STACK_GRAN
#       define STACK_GRAN 0x10000000
#       ifdef __ELF__
#           define DYNAMIC_LOADING
            EXTERN_C_END
#           include <features.h>
            EXTERN_C_BEGIN
#           if defined(__GLIBC__) && __GLIBC__ >= 2 \
                || defined(HOST_ANDROID) || defined(HOST_TIZEN)
#                define SEARCH_FOR_DATA_START
#           else
                 extern char **__environ;
#                define DATASTART ((ptr_t)(&__environ))
                              /* hideous kludge: __environ is the first */
                              /* word in crt0.o, and delimits the start */
                              /* of the data segment, no matter which   */
                              /* ld options were passed through.        */
                              /* We could use _etext instead, but that  */
                              /* would include .rodata, which may       */
                              /* contain large read-only data tables    */
                              /* that we'd rather not scan.             */
#           endif
            extern int _end[];
#           define DATAEND ((ptr_t)(_end))
#       else
            extern int etext[];
#           define DATASTART ((ptr_t)((((word)(etext)) + 0xfff) & ~0xfff))
#       endif
#   endif
#   ifdef MSWINCE
#     define OS_TYPE "MSWINCE"
#     define DATAEND /* not needed */
#   endif
#   ifdef FREEBSD
      /* FreeBSD/arm */
#     define OS_TYPE "FREEBSD"
#     ifndef GC_FREEBSD_THREADS
#       define MPROTECT_VDB
#     endif
#     define SIG_SUSPEND SIGUSR1
#     define SIG_THR_RESTART SIGUSR2
#     define FREEBSD_STACKBOTTOM
#     ifdef __ELF__
#       define DYNAMIC_LOADING
#     endif
      extern char etext[];
#     define DATASTART GC_FreeBSDGetDataStart(0x1000, (ptr_t)etext)
#     define DATASTART_USES_BSDGETDATASTART
#   endif
#   ifdef DARWIN
      /* iOS */
#     define OS_TYPE "DARWIN"
#     define DARWIN_DONT_PARSE_STACK
#     ifndef GC_DONT_REGISTER_MAIN_STATIC_DATA
#       define DYNAMIC_LOADING
#     endif
#     define DATASTART ((ptr_t)get_etext())
#     define DATAEND   ((ptr_t)get_end())
#     define STACKBOTTOM ((ptr_t)0x30000000)
#     define USE_MMAP_ANON
#     define MPROTECT_VDB
      EXTERN_C_END
#     include <unistd.h>
      EXTERN_C_BEGIN
#     define GETPAGESIZE() (unsigned)getpagesize()
      /* FIXME: There seems to be some issues with trylock hanging on   */
      /* darwin. This should be looked into some more.                  */
#     define NO_PTHREAD_TRYLOCK
#     if TARGET_OS_IPHONE && !defined(NO_DYLD_BIND_FULLY_IMAGE)
#       define NO_DYLD_BIND_FULLY_IMAGE
#     endif
#   endif
#   ifdef OPENBSD
#     define OS_TYPE "OPENBSD"
#     ifndef GC_OPENBSD_THREADS
        EXTERN_C_END
#       include <sys/param.h>
#       include <uvm/uvm_extern.h>
        EXTERN_C_BEGIN
#       ifdef USRSTACK
#         define STACKBOTTOM ((ptr_t)USRSTACK)
#       else
#         define HEURISTIC2
#       endif
#     endif
      extern int __data_start[];
#     define DATASTART ((ptr_t)__data_start)
      extern int _end[];
#     define DATAEND ((ptr_t)(&_end))
#     define DYNAMIC_LOADING
#   endif
#   ifdef SN_TARGET_PSP2
#     define NO_HANDLE_FORK
#     define DATASTART (ptr_t)ALIGNMENT
#     define DATAEND (ptr_t)ALIGNMENT
      void *psp2_get_stack_bottom(void);
#     define STACKBOTTOM ((ptr_t)psp2_get_stack_bottom())
#   endif
#   ifdef NN_PLATFORM_CTR
      extern unsigned char Image$$ZI$$ZI$$Base[];
#     define DATASTART (ptr_t)(Image$$ZI$$ZI$$Base)
      extern unsigned char Image$$ZI$$ZI$$Limit[];
#     define DATAEND (ptr_t)(Image$$ZI$$ZI$$Limit)
      void *n3ds_get_stack_bottom(void);
#     define STACKBOTTOM ((ptr_t)n3ds_get_stack_bottom())
#   endif
#   ifdef NOSYS
      /* __data_start is usually defined in the target linker script.  */
      extern int __data_start[];
#     define DATASTART ((ptr_t)(__data_start))
      /* __stack_base__ is set in newlib/libc/sys/arm/crt0.S  */
      extern void *__stack_base__;
#     define STACKBOTTOM ((ptr_t)(__stack_base__))
#   endif
#endif

# ifdef CRIS
#   define MACH_TYPE "CRIS"
#   define CPP_WORDSZ 32
#   define ALIGNMENT 1
#   define OS_TYPE "LINUX"
#   define DYNAMIC_LOADING
#   define LINUX_STACKBOTTOM
#   define SEARCH_FOR_DATA_START
      extern int _end[];
#   define DATAEND ((ptr_t)(_end))
# endif

# if defined(SH) && !defined(SH4)
#   define MACH_TYPE "SH"
#   define ALIGNMENT 4
#   ifdef MSWINCE
#     define OS_TYPE "MSWINCE"
#     define DATAEND /* not needed */
#   endif
#   ifdef LINUX
#     define OS_TYPE "LINUX"
#     define LINUX_STACKBOTTOM
#     define DYNAMIC_LOADING
#     define SEARCH_FOR_DATA_START
      extern int _end[];
#     define DATAEND ((ptr_t)(_end))
#   endif
#   ifdef NETBSD
#      define OS_TYPE "NETBSD"
#      define HEURISTIC2
       extern ptr_t GC_data_start;
#      define DATASTART GC_data_start
#      define DYNAMIC_LOADING
#   endif
#   ifdef OPENBSD
#     define OS_TYPE "OPENBSD"
#     ifndef GC_OPENBSD_THREADS
        EXTERN_C_END
#       include <sys/param.h>
#       include <uvm/uvm_extern.h>
        EXTERN_C_BEGIN
#       ifdef USRSTACK
#         define STACKBOTTOM ((ptr_t)USRSTACK)
#       else
#         define HEURISTIC2
#       endif
#     endif
      extern int __data_start[];
#     define DATASTART ((ptr_t)__data_start)
      extern int _end[];
#     define DATAEND ((ptr_t)(&_end))
#     define DYNAMIC_LOADING
#   endif
# endif

# ifdef SH4
#   define MACH_TYPE "SH4"
#   define OS_TYPE "MSWINCE"
#   define ALIGNMENT 4
#   define DATAEND /* not needed */
# endif

# ifdef AVR32
#   define MACH_TYPE "AVR32"
#   define CPP_WORDSZ 32
#   define ALIGNMENT 4
#   define OS_TYPE "LINUX"
#   define DYNAMIC_LOADING
#   define LINUX_STACKBOTTOM
#   define SEARCH_FOR_DATA_START
    extern int _end[];
#   define DATAEND ((ptr_t)(_end))
# endif

# ifdef M32R
#   define CPP_WORDSZ 32
#   define MACH_TYPE "M32R"
#   define ALIGNMENT 4
#   ifdef LINUX
#     define OS_TYPE "LINUX"
#     define LINUX_STACKBOTTOM
#     undef STACK_GRAN
#     define STACK_GRAN 0x10000000
#     define DYNAMIC_LOADING
#     define SEARCH_FOR_DATA_START
      extern int _end[];
#     define DATAEND ((ptr_t)(_end))
#   endif
# endif

# ifdef X86_64
#   define MACH_TYPE "X86_64"
#   ifdef __ILP32__
#     define ALIGNMENT 4
#     define CPP_WORDSZ 32
#   else
#     define ALIGNMENT 8
#     define CPP_WORDSZ 64
#   endif
#   ifndef HBLKSIZE
#     define HBLKSIZE 4096
#   endif
#   ifndef CACHE_LINE_SIZE
#     define CACHE_LINE_SIZE 64
#   endif
#   ifdef PLATFORM_GETMEM
#     define DATASTART (ptr_t)ALIGNMENT
#     define DATAEND (ptr_t)ALIGNMENT
      EXTERN_C_END
#     include <pthread.h>
      EXTERN_C_BEGIN
      void *platform_get_stack_bottom(void);
#     define STACKBOTTOM ((ptr_t)platform_get_stack_bottom())
#   endif
#   ifdef OPENBSD
#       define OS_TYPE "OPENBSD"
#       define ELF_CLASS ELFCLASS64
#       ifndef GC_OPENBSD_THREADS
          EXTERN_C_END
#         include <sys/param.h>
#         include <uvm/uvm_extern.h>
          EXTERN_C_BEGIN
#         ifdef USRSTACK
#           define STACKBOTTOM ((ptr_t)USRSTACK)
#         else
#           define HEURISTIC2
#         endif
#       endif
        extern int __data_start[];
        extern int _end[];
#       define DATASTART ((ptr_t)__data_start)
#       define DATAEND ((ptr_t)(&_end))
#       define DYNAMIC_LOADING
#   endif
#   ifdef LINUX
#       define OS_TYPE "LINUX"
#       define LINUX_STACKBOTTOM
#       if !defined(GC_LINUX_THREADS) || !defined(REDIRECT_MALLOC)
#           define MPROTECT_VDB
#       else
            /* We seem to get random errors in incremental mode,        */
            /* possibly because Linux threads is itself a malloc client */
            /* and can't deal with the signals.                         */
#       endif
#       ifdef __ELF__
#           define DYNAMIC_LOADING
            EXTERN_C_END
#           include <features.h>
            EXTERN_C_BEGIN
#           define SEARCH_FOR_DATA_START
            extern int _end[];
#           define DATAEND ((ptr_t)(_end))
#       else
             extern int etext[];
#            define DATASTART ((ptr_t)((((word)(etext)) + 0xfff) & ~0xfff))
#       endif
#       if defined(__GLIBC__) && !defined(__UCLIBC__)
          /* At present, there's a bug in GLibc getcontext() on         */
          /* Linux/x64 (it clears FPU exception mask).  We define this  */
          /* macro to workaround it.                                    */
          /* FIXME: This seems to be fixed in GLibc v2.14.              */
#         define GETCONTEXT_FPU_EXCMASK_BUG
#       endif
#       if defined(__GLIBC__) && !defined(__UCLIBC__)
          /* Workaround lock elision implementation for some glibc.     */
#         define GLIBC_2_19_TSX_BUG
          EXTERN_C_END
#         include <gnu/libc-version.h> /* for gnu_get_libc_version() */
          EXTERN_C_BEGIN
#       endif
#   endif
#   ifdef DARWIN
#     define OS_TYPE "DARWIN"
#     define DARWIN_DONT_PARSE_STACK
#     ifndef GC_DONT_REGISTER_MAIN_STATIC_DATA
#       define DYNAMIC_LOADING
#     endif
      /* XXX: see get_end(3), get_etext() and get_end() should not be used. */
      /* These aren't used when dyld support is enabled (it is by default)  */
#     define DATASTART ((ptr_t)get_etext())
#     define DATAEND   ((ptr_t)get_end())
#     define STACKBOTTOM ((ptr_t)0x7fff5fc00000)
#     define USE_MMAP_ANON
#     define MPROTECT_VDB
      EXTERN_C_END
#     include <unistd.h>
      EXTERN_C_BEGIN
#     define GETPAGESIZE() (unsigned)getpagesize()
      /* There seems to be some issues with trylock hanging on darwin.  */
      /* This should be looked into some more.                          */
#     define NO_PTHREAD_TRYLOCK
#     if TARGET_OS_IPHONE && !defined(NO_DYLD_BIND_FULLY_IMAGE)
        /* iPhone/iPad simulator */
#       define NO_DYLD_BIND_FULLY_IMAGE
#     endif
#   endif
#   ifdef FREEBSD
#       define OS_TYPE "FREEBSD"
#       ifndef GC_FREEBSD_THREADS
#           define MPROTECT_VDB
#       endif
#       ifdef __GLIBC__
#           define SIG_SUSPEND          (32+6)
#           define SIG_THR_RESTART      (32+5)
            extern int _end[];
#           define DATAEND ((ptr_t)(_end))
#       else
#           define SIG_SUSPEND SIGUSR1
#           define SIG_THR_RESTART SIGUSR2
                /* SIGTSTP and SIGCONT could be used alternatively.     */
#       endif
#       define FREEBSD_STACKBOTTOM
#       ifdef __ELF__
#           define DYNAMIC_LOADING
#       endif
        extern char etext[];
#       define DATASTART GC_FreeBSDGetDataStart(0x1000, (ptr_t)etext)
#       define DATASTART_USES_BSDGETDATASTART
#   endif
#   ifdef NETBSD
#       define OS_TYPE "NETBSD"
#       define HEURISTIC2
#       ifdef __ELF__
            extern ptr_t GC_data_start;
#           define DATASTART GC_data_start
#           define DYNAMIC_LOADING
#       else
#           define SEARCH_FOR_DATA_START
#       endif
#   endif
#   ifdef HAIKU
#     define OS_TYPE "HAIKU"
      EXTERN_C_END
#     include <OS.h>
      EXTERN_C_BEGIN
#     define GETPAGESIZE() (unsigned)B_PAGE_SIZE
#     define HEURISTIC2
#     define SEARCH_FOR_DATA_START
#     define DYNAMIC_LOADING
#     define MPROTECT_VDB
#   endif
#   ifdef SOLARIS
#       define OS_TYPE "SOLARIS"
#       define ELF_CLASS ELFCLASS64
        extern int _etext[], _end[];
        ptr_t GC_SysVGetDataStart(size_t, ptr_t);
#       define DATASTART GC_SysVGetDataStart(0x1000, (ptr_t)_etext)
#       define DATASTART_IS_FUNC
#       define DATAEND ((ptr_t)(_end))
        /* # define STACKBOTTOM ((ptr_t)(_start)) worked through 2.7,   */
        /* but reportedly breaks under 2.8.  It appears that the stack  */
        /* base is a property of the executable, so this should not     */
        /* break old executables.                                       */
        /* HEURISTIC2 probably works, but this appears to be preferable.*/
        /* Apparently USRSTACK is defined to be USERLIMIT, but in some  */
        /* installations that's undefined.  We work around this with a  */
        /* gross hack:                                                  */
        EXTERN_C_END
#       include <sys/vmparam.h>
        EXTERN_C_BEGIN
#       ifdef USERLIMIT
          /* This should work everywhere, but doesn't.  */
#         define STACKBOTTOM ((ptr_t)USRSTACK)
#       else
#         define HEURISTIC2
#       endif
/* At least in Solaris 2.5, PROC_VDB gives wrong values for dirty bits. */
/* It appears to be fixed in 2.8 and 2.9.                               */
#       ifdef SOLARIS25_PROC_VDB_BUG_FIXED
#         define PROC_VDB
#       endif
#       ifndef GC_THREADS
#         define MPROTECT_VDB
#       endif
#       define DYNAMIC_LOADING
#       if !defined(USE_MMAP) && defined(REDIRECT_MALLOC)
#         define USE_MMAP
            /* Otherwise we now use calloc.  Mmap may result in the     */
            /* heap interleaved with thread stacks, which can result in */
            /* excessive blacklisting.  Sbrk is unusable since it       */
            /* doesn't interact correctly with the system malloc.       */
#       endif
#       ifdef USE_MMAP
#         define HEAP_START (ptr_t)0x40000000
#       else
#         define HEAP_START DATAEND
#       endif
#   endif
#   ifdef MSWIN_XBOX1
#     define NO_GETENV
#     define DATASTART (ptr_t)ALIGNMENT
#     define DATAEND (ptr_t)ALIGNMENT
      LONG64 durango_get_stack_bottom(void);
#     define STACKBOTTOM ((ptr_t)durango_get_stack_bottom())
#     define GETPAGESIZE() 4096
#     ifndef USE_MMAP
#       define USE_MMAP
#     endif
      /* The following is from sys/mman.h:  */
#     define PROT_NONE  0
#     define PROT_READ  1
#     define PROT_WRITE 2
#     define PROT_EXEC  4
#     define MAP_PRIVATE 2
#     define MAP_FIXED  0x10
#     define MAP_FAILED ((void *)-1)
#   endif
#   ifdef MSWIN32
#       define OS_TYPE "MSWIN32"
                /* STACKBOTTOM and DATASTART are handled specially in   */
                /* os_dep.c.                                            */
#       if !defined(__GNUC__) || defined(__INTEL_COMPILER) \
           || GC_GNUC_PREREQ(4, 7)
          /* Older GCC has not supported SetUnhandledExceptionFilter    */
          /* properly on x64 (e.g. SEH unwinding information missed).   */
#         define MPROTECT_VDB
#       endif
#       define GWW_VDB
#       ifndef DATAEND
#         define DATAEND    /* not needed */
#       endif
#   endif
# endif /* X86_64 */

# ifdef HEXAGON
#   define CPP_WORDSZ 32
#   define MACH_TYPE "HEXAGON"
#   define ALIGNMENT 4
#   ifdef LINUX
#       define OS_TYPE "LINUX"
#       define LINUX_STACKBOTTOM
#       define MPROTECT_VDB
#       ifdef __ELF__
#           define DYNAMIC_LOADING
            EXTERN_C_END
#           include <features.h>
            EXTERN_C_BEGIN
#           if defined(__GLIBC__) && __GLIBC__ >= 2
#               define SEARCH_FOR_DATA_START
#           else
#               error --> unknown Hexagon libc configuration
#           endif
            extern int _end[];
#           define DATAEND ((ptr_t)(_end))
#       elif !defined(CPPCHECK)
#           error --> bad Hexagon Linux configuration
#       endif
#   else
#       error --> unknown Hexagon OS configuration
#   endif
# endif

# ifdef TILEPRO
#   define CPP_WORDSZ 32
#   define MACH_TYPE "TILEPro"
#   define ALIGNMENT 4
#   define PREFETCH(x) __insn_prefetch(x)
#   define CACHE_LINE_SIZE 64
#   ifdef LINUX
#     define OS_TYPE "LINUX"
      extern int __data_start[];
#     define DATASTART ((ptr_t)__data_start)
#     define LINUX_STACKBOTTOM
#     define DYNAMIC_LOADING
#   endif
# endif

# ifdef TILEGX
#   define CPP_WORDSZ (__SIZEOF_POINTER__ * 8)
#   define MACH_TYPE "TILE-Gx"
#   define ALIGNMENT __SIZEOF_POINTER__
#   if CPP_WORDSZ < 64
#     define CLEAR_DOUBLE(x) (*(long long *)(x) = 0)
#   endif
#   define PREFETCH(x) __insn_prefetch_l1(x)
#   define CACHE_LINE_SIZE 64
#   ifdef LINUX
#     define OS_TYPE "LINUX"
      extern int __data_start[];
#     define DATASTART ((ptr_t)__data_start)
#     define LINUX_STACKBOTTOM
#     define DYNAMIC_LOADING
#   endif
# endif

# ifdef RISCV
#   define MACH_TYPE "RISC-V"
#   define CPP_WORDSZ __riscv_xlen /* 32 or 64 */
#   define ALIGNMENT (CPP_WORDSZ/8)
#   ifdef LINUX
#     define OS_TYPE "LINUX"
      extern int __data_start[];
#     define DATASTART ((ptr_t)__data_start)
#     define LINUX_STACKBOTTOM
#     define DYNAMIC_LOADING
#   endif
# endif /* RISCV */

#if defined(__GLIBC__) && !defined(DONT_USE_LIBC_PRIVATES)
  /* Use glibc's stack-end marker. */
# define USE_LIBC_PRIVATES
#endif

#if defined(LINUX_STACKBOTTOM) && defined(NO_PROC_STAT) \
    && !defined(USE_LIBC_PRIVATES)
    /* This combination will fail, since we have no way to get  */
    /* the stack base.  Use HEURISTIC2 instead.                 */
#   undef LINUX_STACKBOTTOM
#   define HEURISTIC2
    /* This may still fail on some architectures like IA64.     */
    /* We tried ...                                             */
#endif

#if defined(USE_MMAP_ANON) && !defined(USE_MMAP)
#   define USE_MMAP
#elif defined(LINUX) && defined(USE_MMAP)
    /* The kernel may do a somewhat better job merging mappings etc.    */
    /* with anonymous mappings.                                         */
#   define USE_MMAP_ANON
#endif

#if defined(GC_LINUX_THREADS) && defined(REDIRECT_MALLOC) \
    && !defined(USE_PROC_FOR_LIBRARIES)
    /* Nptl allocates thread stacks with mmap, which is fine.  But it   */
    /* keeps a cache of thread stacks.  Thread stacks contain the       */
    /* thread control blocks.  These in turn contain a pointer to       */
    /* (sizeof (void *) from the beginning of) the dtv for thread-local */
    /* storage, which is calloc allocated.  If we don't scan the cached */
    /* thread stacks, we appear to lose the dtv.  This tends to         */
    /* result in something that looks like a bogus dtv count, which     */
    /* tends to result in a memset call on a block that is way too      */
    /* large.  Sometimes we're lucky and the process just dies ...      */
    /* There seems to be a similar issue with some other memory         */
    /* allocated by the dynamic loader.                                 */
    /* This should be avoidable by either:                              */
    /* - Defining USE_PROC_FOR_LIBRARIES here.                          */
    /*   That performs very poorly, precisely because we end up         */
    /*   scanning cached stacks.                                        */
    /* - Have calloc look at its callers.                               */
    /*   In spite of the fact that it is gross and disgusting.          */
    /* In fact neither seems to suffice, probably in part because       */
    /* even with USE_PROC_FOR_LIBRARIES, we don't scan parts of stack   */
    /* segments that appear to be out of bounds.  Thus we actually      */
    /* do both, which seems to yield the best results.                  */
#   define USE_PROC_FOR_LIBRARIES
#endif

#ifndef STACK_GROWS_UP
# define STACK_GROWS_DOWN
#endif

#ifndef CPP_WORDSZ
# define CPP_WORDSZ 32
#endif

#ifndef OS_TYPE
# define OS_TYPE ""
#endif

#ifndef DATAEND
  extern int end[];
# define DATAEND ((ptr_t)(end))
#endif

/* Workaround for Android NDK clang 3.5+ (as of NDK r10e) which does    */
/* not provide correct _end symbol.  Unfortunately, alternate __end__   */
/* symbol is provided only by NDK "bfd" linker.                         */
#if defined(HOST_ANDROID) && defined(__clang__)
# undef DATAEND
# pragma weak __end__
  extern int __end__[];
# define DATAEND (__end__ != 0 ? (ptr_t)__end__ : (ptr_t)_end)
#endif

#if (defined(SVR4) || defined(HOST_ANDROID) || defined(HOST_TIZEN)) \
    && !defined(GETPAGESIZE)
  EXTERN_C_END
# include <unistd.h>
  EXTERN_C_BEGIN
# define GETPAGESIZE() (unsigned)sysconf(_SC_PAGESIZE)
#endif

#ifndef GETPAGESIZE
# if defined(SOLARIS) || defined(IRIX5) || defined(LINUX) \
     || defined(NETBSD) || defined(FREEBSD) || defined(HPUX)
    EXTERN_C_END
#   include <unistd.h>
    EXTERN_C_BEGIN
# endif
# define GETPAGESIZE() (unsigned)getpagesize()
#endif

#if defined(HOST_ANDROID) && !(__ANDROID_API__ >= 23) \
    && ((defined(MIPS) && (CPP_WORDSZ == 32)) \
        || defined(ARM32) || defined(I386) /* but not x32 */)
  /* tkill() exists only on arm32/mips(32)/x86. */
  /* NDK r11+ deprecates tkill() but keeps it for Mono clients. */
/* Unity: Android NDK r13 removed this for good. We should not need it. */
/*# define USE_TKILL_ON_ANDROID*/
#endif

#if defined(SOLARIS) || defined(DRSNX) || defined(UTS4)
        /* OS has SVR4 generic features.        */
        /* Probably others also qualify.        */
# define SVR4
#endif

#if defined(SOLARIS) || defined(DRSNX)
        /* OS has SOLARIS style semi-undocumented interface     */
        /* to dynamic loader.                                   */
# define SOLARISDL
        /* OS has SOLARIS style signal handlers.        */
# define SUNOS5SIGS
#endif

#if defined(HPUX)
# define SUNOS5SIGS
#endif

#if defined(FREEBSD) && (defined(__DragonFly__) || __FreeBSD__ >= 4 \
                         || (__FreeBSD_kernel__ >= 4))
# define SUNOS5SIGS
#endif

#if !defined(GC_EXPLICIT_SIGNALS_UNBLOCK) && defined(SUNOS5SIGS) \
    && !defined(GC_NO_PTHREAD_SIGMASK)
# define GC_EXPLICIT_SIGNALS_UNBLOCK
#endif

#if !defined(NO_SIGNALS_UNBLOCK_IN_MAIN) && defined(GC_NO_PTHREAD_SIGMASK)
# define NO_SIGNALS_UNBLOCK_IN_MAIN
#endif

#if !defined(NO_MARKER_SPECIAL_SIGMASK) \
    && (defined(NACL) || defined(GC_WIN32_PTHREADS))
  /* Either there is no pthread_sigmask(), or GC marker thread cannot   */
  /* steal and drop user signal calls.                                  */
# define NO_MARKER_SPECIAL_SIGMASK
#endif

#ifdef GC_NETBSD_THREADS
# define SIGRTMIN 33
# define SIGRTMAX 63
  /* It seems to be necessary to wait until threads have restarted.     */
  /* But it is unclear why that is the case.                            */
# define GC_NETBSD_THREADS_WORKAROUND
#endif

#ifdef GC_OPENBSD_THREADS
  EXTERN_C_END
# include <sys/param.h>
  EXTERN_C_BEGIN
  /* Prior to 5.2 release, OpenBSD had user threads and required        */
  /* special handling.                                                  */
# if OpenBSD < 201211
#   define GC_OPENBSD_UTHREADS 1
# endif
#endif /* GC_OPENBSD_THREADS */

#if defined(SVR4) || defined(LINUX) || defined(IRIX5) || defined(HPUX) \
    || defined(OPENBSD) || defined(NETBSD) || defined(FREEBSD) \
    || defined(DGUX) || defined(BSD) || defined(HAIKU) || defined(HURD) \
    || defined(AIX) || defined(DARWIN) || defined(OSF1)
# define UNIX_LIKE      /* Basic Unix-like system calls work.   */
#endif

#if defined(CPPCHECK)
# undef CPP_WORDSZ
# define CPP_WORDSZ (__SIZEOF_POINTER__ * 8)
#endif

#if CPP_WORDSZ != 32 && CPP_WORDSZ != 64
# error --> bad word size
#endif

#if !defined(ALIGNMENT) && !defined(CPPCHECK)
# error --> undefined ALIGNMENT
#endif

#ifdef PCR
# undef DYNAMIC_LOADING
# undef STACKBOTTOM
# undef HEURISTIC1
# undef HEURISTIC2
# undef PROC_VDB
# undef MPROTECT_VDB
# define PCR_VDB
#endif

#if !defined(STACKBOTTOM) && (defined(ECOS) || defined(NOSYS)) \
    && !defined(CPPCHECK)
# error --> undefined STACKBOTTOM
#endif

#ifdef IGNORE_DYNAMIC_LOADING
# undef DYNAMIC_LOADING
#endif

#if defined(SMALL_CONFIG) && !defined(GC_DISABLE_INCREMENTAL)
  /* Presumably not worth the space it takes.   */
# define GC_DISABLE_INCREMENTAL
#endif

#if (defined(MSWIN32) || defined(MSWINCE)) && !defined(USE_WINALLOC)
  /* USE_WINALLOC is only an option for Cygwin. */
# define USE_WINALLOC
#endif

#ifdef USE_WINALLOC
# undef USE_MMAP
#endif

#if defined(LINUX) || defined(FREEBSD) || defined(SOLARIS) || defined(IRIX5) \
    || ((defined(USE_MMAP) || defined(USE_MUNMAP)) && !defined(USE_WINALLOC))
# define MMAP_SUPPORTED
#endif

/* Xbox One (DURANGO) may not need to be this aggressive, but the       */
/* default is likely too lax under heavy allocation pressure.           */
/* The platform does not have a virtual paging system, so it does not   */
/* have a large virtual address space that a standard x64 platform has. */
#if defined(USE_MUNMAP) && !defined(MUNMAP_THRESHOLD) \
    && (defined(SN_TARGET_PS3) \
        || defined(SN_TARGET_PSP2) || defined(MSWIN_XBOX1))
# define MUNMAP_THRESHOLD 2
#endif

#if defined(GC_DISABLE_INCREMENTAL) || defined(DEFAULT_VDB) \
    || defined(MANUAL_VDB)
# undef GWW_VDB
# undef MPROTECT_VDB
# undef PCR_VDB
# undef PROC_VDB
#endif

#ifdef GC_DISABLE_INCREMENTAL
# undef CHECKSUMS
#endif

#ifdef USE_GLOBAL_ALLOC
  /* Cannot pass MEM_WRITE_WATCH to GlobalAlloc().      */
# undef GWW_VDB
#endif

#if defined(USE_MUNMAP) && defined(GWW_VDB)
# undef MPROTECT_VDB  /* TODO: Cannot deal with address space holes. */
  /* Else if MPROTECT_VDB is available but not GWW_VDB then decide      */
  /* whether to disable memory unmapping or mprotect-based virtual      */
  /* dirty bits at runtime when GC_enable_incremental is called.        */
#endif

/* PARALLEL_MARK does not cause undef MPROTECT_VDB any longer.  */

#if defined(MPROTECT_VDB) && defined(GC_PREFER_MPROTECT_VDB)
  /* Choose MPROTECT_VDB manually (if multiple strategies available).   */
# undef PCR_VDB
# undef PROC_VDB
  /* #undef GWW_VDB - handled in os_dep.c       */
#endif

#ifdef PROC_VDB
  /* Multi-VDB mode is not implemented. */
# undef MPROTECT_VDB
#endif

#if defined(SIGBUS) && !defined(HAVE_SIGBUS) && !defined(CPPCHECK)
# define HAVE_SIGBUS
#endif

#ifndef SA_SIGINFO
# define NO_SA_SIGACTION
#endif

#if defined(NO_SA_SIGACTION) && defined(MPROTECT_VDB) && !defined(DARWIN) \
    && !defined(MSWIN32) && !defined(MSWINCE)
# undef MPROTECT_VDB
#endif

#if !defined(PCR_VDB) && !defined(PROC_VDB) && !defined(MPROTECT_VDB) \
    && !defined(GWW_VDB) && !defined(DEFAULT_VDB) && !defined(MANUAL_VDB) \
    && !defined(GC_DISABLE_INCREMENTAL)
# define DEFAULT_VDB
#endif

#if ((defined(UNIX_LIKE) && (defined(DARWIN) || defined(HAIKU) \
                             || defined(HURD) || defined(OPENBSD) \
                             || defined(ARM32) \
                             || defined(AVR32) || defined(MIPS) \
                             || defined(NIOS2) || defined(OR1K))) \
     || (defined(LINUX) && !defined(__gnu_linux__)) \
     || (defined(RTEMS) && defined(I386)) || defined(HOST_ANDROID)) \
    && !defined(NO_GETCONTEXT)
# define NO_GETCONTEXT
#endif

#ifndef PREFETCH
# if GC_GNUC_PREREQ(3, 0) && !defined(NO_PREFETCH)
#   define PREFETCH(x) __builtin_prefetch((x), 0, 0)
# else
#   define PREFETCH(x) (void)0
# endif
#endif

#ifndef GC_PREFETCH_FOR_WRITE
# if GC_GNUC_PREREQ(3, 0) && !defined(GC_NO_PREFETCH_FOR_WRITE)
#   define GC_PREFETCH_FOR_WRITE(x) __builtin_prefetch((x), 1)
# else
#   define GC_PREFETCH_FOR_WRITE(x) (void)0
# endif
#endif

#ifndef CACHE_LINE_SIZE
# define CACHE_LINE_SIZE 32     /* Wild guess   */
#endif

#ifndef STATIC
# ifndef NO_DEBUGGING
#   define STATIC /* ignore to aid profiling and possibly debugging     */
# else
#   define STATIC static
# endif
#endif

#if defined(LINUX) && (defined(USE_PROC_FOR_LIBRARIES) || defined(IA64) \
                       || !defined(SMALL_CONFIG))
# define NEED_PROC_MAPS
#endif

#if defined(LINUX) || defined(HURD) || defined(__GLIBC__)
# define REGISTER_LIBRARIES_EARLY
  /* We sometimes use dl_iterate_phdr, which may acquire an internal    */
  /* lock.  This isn't safe after the world has stopped.  So we must    */
  /* call GC_register_dynamic_libraries before stopping the world.      */
  /* For performance reasons, this may be beneficial on other           */
  /* platforms as well, though it should be avoided in win32.           */
#endif /* LINUX */

#if defined(SEARCH_FOR_DATA_START)
  extern ptr_t GC_data_start;
# define DATASTART GC_data_start
#endif

#ifndef HEAP_START
# define HEAP_START ((ptr_t)0)
#endif

#ifndef CLEAR_DOUBLE
# define CLEAR_DOUBLE(x) (((word*)(x))[0] = 0, ((word*)(x))[1] = 0)
#endif

#if defined(GC_LINUX_THREADS) && defined(REDIRECT_MALLOC) \
    && !defined(INCLUDE_LINUX_THREAD_DESCR)
  /* Will not work, since libc and the dynamic loader use thread        */
  /* locals, sometimes as the only reference.                           */
# define INCLUDE_LINUX_THREAD_DESCR
#endif

#if !defined(CPPCHECK)
#if defined(GC_IRIX_THREADS) && !defined(IRIX5)
# error --> inconsistent configuration
#endif
#if defined(GC_LINUX_THREADS) && !defined(LINUX) && !defined(NACL)
# error --> inconsistent configuration
#endif
#if defined(GC_NETBSD_THREADS) && !defined(NETBSD)
# error --> inconsistent configuration
#endif
#if defined(GC_FREEBSD_THREADS) && !defined(FREEBSD)
# error --> inconsistent configuration
#endif
#if defined(GC_SOLARIS_THREADS) && !defined(SOLARIS)
# error --> inconsistent configuration
#endif
#if defined(GC_HPUX_THREADS) && !defined(HPUX)
# error --> inconsistent configuration
#endif
#if defined(GC_AIX_THREADS) && !defined(_AIX)
# error --> inconsistent configuration
#endif
#if defined(GC_WIN32_THREADS) && !defined(CYGWIN32) && !defined(MSWIN32) \
    && !defined(MSWINCE) && !defined(MSWIN_XBOX1)
# error --> inconsistent configuration
#endif
# if defined(GC_WIN32_PTHREADS) && defined(CYGWIN32)
#   error --> inconsistent configuration
# endif
#endif /* !CPPCHECK */

#if defined(PCR) || defined(GC_WIN32_THREADS) || defined(GC_PTHREADS) \
    || defined(NN_PLATFORM_CTR) || defined(NINTENDO_SWITCH) \
    || defined(SN_TARGET_PS3) \
    || defined(SN_TARGET_PSP2)
  #if !defined(THREADS)
    # define THREADS
  #endif
#endif

#if defined(PARALLEL_MARK) && !defined(THREADS) && !defined(CPPCHECK)
# error "invalid config - PARALLEL_MARK requires GC_THREADS"
#endif

#if (((defined(MSWIN32) || defined(MSWINCE)) && !defined(__GNUC__)) \
        || (defined(MSWIN32) && defined(I386)) /* for Win98 */ \
        || (defined(USE_PROC_FOR_LIBRARIES) && defined(THREADS))) \
     && !defined(NO_WRAP_MARK_SOME)
  /* Under rare conditions, we may end up marking from nonexistent      */
  /* memory.  Hence we need to be prepared to recover by running        */
  /* GC_mark_some with a suitable handler in place.                     */
  /* TODO: Probably replace __GNUC__ above with ndef GC_PTHREADS.       */
  /* FIXME: Should we really need it for WinCE?  If yes then            */
  /* WRAP_MARK_SOME should be also defined for CeGCC which requires     */
  /* CPU/OS-specific code in mark_ex_handler and GC_mark_some (for      */
  /* manual stack unwinding and exception handler installation).        */
# define WRAP_MARK_SOME
#endif

#if defined(PARALLEL_MARK) && !defined(DEFAULT_STACK_MAYBE_SMALL) \
    && (defined(HPUX) || defined(GC_DGUX386_THREADS) \
        || defined(NO_GETCONTEXT) /* e.g. musl */)
    /* TODO: Test default stack size in configure. */
# define DEFAULT_STACK_MAYBE_SMALL
#endif

#ifdef PARALLEL_MARK
  /* The minimum stack size for a marker thread. */
# define MIN_STACK_SIZE (8 * HBLKSIZE * sizeof(word))
#endif

#if defined(HOST_ANDROID) && !defined(THREADS) \
    && !defined(USE_GET_STACKBASE_FOR_MAIN)
  /* Always use pthread_attr_getstack on Android ("-lpthread" option is  */
  /* not needed to be specified manually) since GC_linux_main_stack_base */
  /* causes app crash if invoked inside Dalvik VM.                       */
# define USE_GET_STACKBASE_FOR_MAIN
#endif

/* Outline pthread primitives to use in GC_get_[main_]stack_base.       */
#if ((defined(FREEBSD) && defined(__GLIBC__)) /* kFreeBSD */ \
     || defined(LINUX) || defined(NETBSD) || defined(HOST_ANDROID)) \
    && !defined(NO_PTHREAD_GETATTR_NP)
# define HAVE_PTHREAD_GETATTR_NP 1
#elif defined(FREEBSD) && !defined(__GLIBC__) \
      && !defined(NO_PTHREAD_ATTR_GET_NP)
# define HAVE_PTHREAD_NP_H 1 /* requires include pthread_np.h */
# define HAVE_PTHREAD_ATTR_GET_NP 1
#endif

#if defined(UNIX_LIKE) && defined(THREADS) && !defined(NO_CANCEL_SAFE) \
    && !defined(HOST_ANDROID)
  /* Make the code cancellation-safe.  This basically means that we     */
  /* ensure that cancellation requests are ignored while we are in      */
  /* the collector.  This applies only to Posix deferred cancellation;  */
  /* we don't handle Posix asynchronous cancellation.                   */
  /* Note that this only works if pthread_setcancelstate is             */
  /* async-signal-safe, at least in the absence of asynchronous         */
  /* cancellation.  This appears to be true for the glibc version,      */
  /* though it is not documented.  Without that assumption, there       */
  /* seems to be no way to safely wait in a signal handler, which       */
  /* we need to do for thread suspension.                               */
  /* Also note that little other code appears to be cancellation-safe.  */
  /* Hence it may make sense to turn this off for performance.          */
# define CANCEL_SAFE
#endif

#ifdef CANCEL_SAFE
# define IF_CANCEL(x) x
#else
# define IF_CANCEL(x) /* empty */
#endif

#if !defined(CAN_HANDLE_FORK) && !defined(NO_HANDLE_FORK) \
    && !defined(HAVE_NO_FORK) \
    && ((defined(GC_PTHREADS) && !defined(NACL) \
         && !defined(GC_WIN32_PTHREADS) && !defined(USE_WINALLOC)) \
        || (defined(DARWIN) && defined(MPROTECT_VDB)) || defined(HANDLE_FORK))
  /* Attempts (where supported and requested) to make GC_malloc work in */
  /* a child process fork'ed from a multi-threaded parent.              */
# define CAN_HANDLE_FORK
#endif

#if defined(CAN_HANDLE_FORK) && !defined(CAN_CALL_ATFORK) \
    && !defined(HURD) && !defined(GC_NO_CAN_CALL_ATFORK) && !defined(HOST_TIZEN) \
    && (!defined(HOST_ANDROID) || __ANDROID_API__ >= 21)
  /* Have working pthread_atfork().     */
# define CAN_CALL_ATFORK
#endif

#if !defined(CAN_HANDLE_FORK) && !defined(HAVE_NO_FORK) \
    && (defined(MSWIN32) || defined(MSWINCE) || defined(DOS4GW) \
        || defined(OS2) || defined(SYMBIAN) /* and probably others ... */)
# define HAVE_NO_FORK
#endif

#if !defined(USE_MARK_BITS) && !defined(USE_MARK_BYTES) \
    && defined(PARALLEL_MARK)
   /* Minimize compare-and-swap usage.  */
#  define USE_MARK_BYTES
#endif

#if (defined(MSWINCE) && !defined(__CEGCC__) || defined(MSWINRT_FLAVOR)) \
    && !defined(NO_GETENV)
# define NO_GETENV
#endif

#if (defined(NO_GETENV) || defined(MSWINCE)) && !defined(NO_GETENV_WIN32)
# define NO_GETENV_WIN32
#endif

#ifndef STRTOULL
# if defined(_WIN64) && !defined(__GNUC__)
#   define STRTOULL _strtoui64
# elif defined(_LLP64) || defined(__LLP64__) || defined(_WIN64)
#   define STRTOULL strtoull
# else
    /* strtoul() fits since sizeof(long) >= sizeof(word).       */
#   define STRTOULL strtoul
# endif
#endif /* !STRTOULL */

#ifndef GC_WORD_C
# if defined(_WIN64) && !defined(__GNUC__)
#   define GC_WORD_C(val) val##ui64
# elif defined(_LLP64) || defined(__LLP64__) || defined(_WIN64)
#   define GC_WORD_C(val) val##ULL
# else
#   define GC_WORD_C(val) ((word)val##UL)
# endif
#endif /* !GC_WORD_C */

#if defined(__has_feature)
  /* __has_feature() is supported.      */
# if __has_feature(address_sanitizer) && !defined(ADDRESS_SANITIZER)
#   define ADDRESS_SANITIZER
# endif
# if __has_feature(memory_sanitizer) && !defined(MEMORY_SANITIZER)
#   define MEMORY_SANITIZER
# endif
# if __has_feature(thread_sanitizer) && !defined(THREAD_SANITIZER)
#   define THREAD_SANITIZER
# endif
#else
# ifdef __SANITIZE_ADDRESS__
    /* GCC v4.8+ */
#   define ADDRESS_SANITIZER
# endif
#endif /* !__has_feature */

#if defined(SPARC)
# define ASM_CLEAR_CODE /* Stack clearing is crucial, and we    */
                        /* include assembly code to do it well. */
#endif

/* Can we save call chain in objects for debugging?                     */
/* SET NFRAMES (# of saved frames) and NARGS (#of args for each         */
/* frame) to reasonable values for the platform.                        */
/* Set SAVE_CALL_CHAIN if we can.  SAVE_CALL_COUNT can be specified     */
/* at build time, though we feel free to adjust it slightly.            */
/* Define NEED_CALLINFO if we either save the call stack or             */
/* GC_ADD_CALLER is defined.                                            */
/* GC_CAN_SAVE_CALL_STACKS is set in gc.h.                              */
#if defined(SPARC)
# define CAN_SAVE_CALL_ARGS
#endif
#if (defined(I386) || defined(X86_64)) \
    && (defined(LINUX) || defined(__GLIBC__))
  /* SAVE_CALL_CHAIN is supported if the code is compiled to save       */
  /* frame pointers by default, i.e. no -fomit-frame-pointer flag.      */
# define CAN_SAVE_CALL_ARGS
#endif

#if defined(SAVE_CALL_COUNT) && !defined(GC_ADD_CALLER) \
    && defined(GC_CAN_SAVE_CALL_STACKS)
# define SAVE_CALL_CHAIN
#endif
#ifdef SAVE_CALL_CHAIN
# if defined(SAVE_CALL_NARGS) && defined(CAN_SAVE_CALL_ARGS)
#   define NARGS SAVE_CALL_NARGS
# else
#   define NARGS 0      /* Number of arguments to save for each call.   */
# endif
#endif
#ifdef SAVE_CALL_CHAIN
# if !defined(SAVE_CALL_COUNT) || defined(CPPCHECK)
#   define NFRAMES 6    /* Number of frames to save. Even for   */
                        /* alignment reasons.                   */
# else
#   define NFRAMES ((SAVE_CALL_COUNT + 1) & ~1)
# endif
# define NEED_CALLINFO
#endif /* SAVE_CALL_CHAIN */
#ifdef GC_ADD_CALLER
# define NFRAMES 1
# define NARGS 0
# define NEED_CALLINFO
#endif

#if (defined(FREEBSD) || (defined(DARWIN) && !defined(_POSIX_C_SOURCE)) \
        || (defined(SOLARIS) && (!defined(_XOPEN_SOURCE) \
                                 || defined(__EXTENSIONS__))) \
        || defined(LINUX)) && !defined(HAVE_DLADDR)
# define HAVE_DLADDR
#endif

#if defined(MAKE_BACK_GRAPH) && !defined(DBG_HDRS_ALL)
# define DBG_HDRS_ALL
#endif

#if defined(POINTER_MASK) && !defined(POINTER_SHIFT)
# define POINTER_SHIFT 0
#endif

#if defined(POINTER_SHIFT) && !defined(POINTER_MASK)
# define POINTER_MASK ((word)(-1))
#endif

#if !defined(FIXUP_POINTER) && defined(POINTER_MASK)
# if defined(CPPCHECK)
#   define FIXUP_POINTER(p) (p = (p) << 4) /* e.g. */
# else
#   define FIXUP_POINTER(p) (p = ((p) & POINTER_MASK) << POINTER_SHIFT)
# endif
#endif

#if defined(FIXUP_POINTER)
# define NEED_FIXUP_POINTER
#else
# define FIXUP_POINTER(p)
#endif

#if !defined(MARK_BIT_PER_GRANULE) && !defined(MARK_BIT_PER_OBJ)
# define MARK_BIT_PER_GRANULE   /* Usually faster       */
#endif

/* Some static sanity tests.    */
#if !defined(CPPCHECK)
#if defined(MARK_BIT_PER_GRANULE) && defined(MARK_BIT_PER_OBJ)
# error Define only one of MARK_BIT_PER_GRANULE and MARK_BIT_PER_OBJ.
#endif

#if defined(STACK_GROWS_UP) && defined(STACK_GROWS_DOWN)
# error "Only one of STACK_GROWS_UP and STACK_GROWS_DOWN should be defd."
#endif
#if !defined(STACK_GROWS_UP) && !defined(STACK_GROWS_DOWN)
# error "One of STACK_GROWS_UP and STACK_GROWS_DOWN should be defd."
#endif

#if defined(REDIRECT_MALLOC) && defined(THREADS) && !defined(LINUX) \
     && !defined(REDIRECT_MALLOC_IN_HEADER)
# error "REDIRECT_MALLOC with THREADS works at most on Linux."
#endif
#endif /* !CPPCHECK */

#ifdef GC_PRIVATE_H
        /* This relies on some type definitions from gc_priv.h, from    */
        /* where it's normally included.                                */
        /*                                                              */
        /* How to get heap memory from the OS:                          */
        /* Note that sbrk()-like allocation is preferred, since it      */
        /* usually makes it possible to merge consecutively allocated   */
        /* chunks.  It also avoids unintended recursion with            */
        /* REDIRECT_MALLOC macro defined.                               */
        /* GET_MEM() argument should be of size_t type and have         */
        /* no side-effect.  GET_MEM() returns HBLKSIZE-aligned chunk;   */
        /* 0 is taken to mean failure.                                  */
        /* In case of MMAP_SUPPORTED, the argument must also be         */
        /* a multiple of a physical page size.                          */
        /* GET_MEM is currently not assumed to retrieve 0 filled space, */
        /* though we should perhaps take advantage of the case in which */
        /* does.                                                        */
        struct hblk;    /* See gc_priv.h.       */
# if defined(PCR)
    char * real_malloc(size_t bytes);
#   define GET_MEM(bytes) HBLKPTR(real_malloc(SIZET_SAT_ADD(bytes, \
                                                            GC_page_size)) \
                                          + GC_page_size-1)
# elif defined(OS2)
    void * os2_alloc(size_t bytes);
#   define GET_MEM(bytes) HBLKPTR((ptr_t)os2_alloc( \
                                            SIZET_SAT_ADD(bytes, \
                                                          GC_page_size)) \
                                  + GC_page_size-1)
# elif defined(NEXT) || defined(DOS4GW) || defined(NONSTOP) \
        || (defined(AMIGA) && !defined(GC_AMIGA_FASTALLOC)) \
        || (defined(SOLARIS) && !defined(USE_MMAP)) || defined(RTEMS) \
        || defined(__CC_ARM)
#   define GET_MEM(bytes) HBLKPTR((size_t)calloc(1, \
                                            SIZET_SAT_ADD(bytes, \
                                                          GC_page_size)) \
                                  + GC_page_size - 1)
# elif defined(MSWIN_XBOX1)
    void *durango_get_mem(size_t bytes, size_t page_size);
#   define GET_MEM(bytes) (struct hblk *)durango_get_mem(bytes, 0)
# elif defined(MSWIN32) || defined(CYGWIN32)
    ptr_t GC_win32_get_mem(size_t bytes);
#   define GET_MEM(bytes) (struct hblk *)GC_win32_get_mem(bytes)
# elif defined(MACOS)
#   if defined(USE_TEMPORARY_MEMORY)
      Ptr GC_MacTemporaryNewPtr(size_t size, Boolean clearMemory);
#     define GET_MEM(bytes) HBLKPTR(GC_MacTemporaryNewPtr( \
                                        SIZET_SAT_ADD(bytes, \
                                                      GC_page_size), true) \
                        + GC_page_size-1)
#   else
#     define GET_MEM(bytes) HBLKPTR(NewPtrClear(SIZET_SAT_ADD(bytes, \
                                                              GC_page_size)) \
                                    + GC_page_size-1)
#   endif
# elif defined(MSWINCE)
    ptr_t GC_wince_get_mem(size_t bytes);
#   define GET_MEM(bytes) (struct hblk *)GC_wince_get_mem(bytes)
# elif defined(AMIGA) && defined(GC_AMIGA_FASTALLOC)
    void *GC_amiga_get_mem(size_t bytes);
#   define GET_MEM(bytes) HBLKPTR((size_t)GC_amiga_get_mem( \
                                            SIZET_SAT_ADD(bytes, \
                                                          GC_page_size)) \
                          + GC_page_size-1)
# elif defined(PLATFORM_GETMEM)
    void *platform_get_mem(size_t bytes);
#   define GET_MEM(bytes) (struct hblk*)platform_get_mem(bytes)
# elif defined(SN_TARGET_PS3)
    void *ps3_get_mem(size_t bytes);
#   define GET_MEM(bytes) (struct hblk*)ps3_get_mem(bytes)
# elif defined(SN_TARGET_PSP2)
    void *psp2_get_mem(size_t bytes);
#   define GET_MEM(bytes) (struct hblk*)psp2_get_mem(bytes)
# elif defined(NINTENDO_SWITCH)
    void *switch_get_mem(size_t bytes);
#   define GET_MEM(bytes) (struct hblk*)switch_get_mem(bytes)
# elif defined(HAIKU)
    ptr_t GC_haiku_get_mem(size_t bytes);
#   define GET_MEM(bytes) (struct hblk*)GC_haiku_get_mem(bytes)
# elif defined(EMSCRIPTEN_TINY)
    void *emmalloc_memalign(size_t alignment, size_t size);
#   define GET_MEM(bytes) (struct hblk*)emmalloc_memalign(GC_page_size, bytes)
# else
    ptr_t GC_unix_get_mem(size_t bytes);
#   define GET_MEM(bytes) (struct hblk *)GC_unix_get_mem(bytes)
# endif
#endif /* GC_PRIVATE_H */

EXTERN_C_END

#ifndef SMALL_CONFIG
/* We were running out of memory due to the fact that 
 * GC has a static sized array for heap segments, and without
 * LARGE_CONFIG you go OOM at around 1.8 GB on 64-bit.
 * Note, we define it everywhere on Mono. */
#define LARGE_CONFIG
#endif

#endif /* GCCONFIG_H */
