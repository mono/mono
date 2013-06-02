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

# define GCCONFIG_H

# ifndef GC_PRIVATE_H
    /* Fake ptr_t declaration, just to avoid compilation errors.	*/
    /* This avoids many instances if "ifndef GC_PRIVATE_H" below.	*/
    typedef struct GC_undefined_struct * ptr_t;
# endif

/* Machine dependent parameters.  Some tuning parameters can be found	*/
/* near the top of gc_private.h.					*/

/* Machine specific parts contributed by various people.  See README file. */

/* First a unified test for Linux: */
# if defined(linux) || defined(__linux__)
#  ifndef LINUX
#    define LINUX
#  endif
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
# if defined(__FreeBSD__) && !defined(FREEBSD)
#    define FREEBSD
# endif

/* And one for Darwin: */
# if defined(macosx) || (defined(__APPLE__) && defined(__MACH__))
#   define DARWIN
# endif

/* Determine the machine type: */
# if defined(__native_client__)
#    define NACL
#    if !defined(__portable_native_client__) && !defined(__arm__)
#        define I386
#        define mach_type_known
#    else
         /* Here we will rely upon arch-specific defines. */
#    endif
# endif
# if defined(__arm__) || defined(__thumb__)
#    define ARM32
#    if defined(NACL)
#      define mach_type_known
#    elif !defined(LINUX) && !defined(NETBSD) && !defined(DARWIN)
#      define NOSYS
#      define mach_type_known
#    endif
# endif
# if defined(sun) && defined(mc68000)
#    define M68K
#    define SUNOS4
#    define mach_type_known
# endif
# if defined(hp9000s300)
#    define M68K
#    define HP
#    define mach_type_known
# endif
# if defined(OPENBSD) && defined(m68k)
#    define M68K
#    define mach_type_known
# endif
# if defined(OPENBSD) && defined(__sparc__)
#    define SPARC
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
# if defined(vax)
#    define VAX
#    ifdef ultrix
#	define ULTRIX
#    else
#	define BSD
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
#    if !defined(LINUX) && !defined(EWS4800) && !defined(NETBSD)
#      if defined(ultrix) || defined(__ultrix)
#	 define ULTRIX
#      else
#	 if defined(_SYSTYPE_SVR4) || defined(SYSTYPE_SVR4) \
	    || defined(__SYSTYPE_SVR4__)
#	   define IRIX5   /* or IRIX 6.X */
#	 else
#	   define RISCOS  /* or IRIX 4.X */
#	 endif
#      endif
#    endif /* !LINUX */
#    if defined(__NetBSD__) && defined(__MIPSEL__)
#      undef ULTRIX
#    endif
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
# if defined(sun) && (defined(i386) || defined(__i386__))
#    define I386
#    define SUNOS5
#    define mach_type_known
# endif
# if (defined(__OS2__) || defined(__EMX__)) && defined(__32BIT__)
#    define I386
#    define OS2
#    define mach_type_known
# endif
# if defined(ibm032)
#   define RT
#   define mach_type_known
# endif
# if defined(sun) && (defined(sparc) || defined(__sparc))
#   define SPARC
    /* Test for SunOS 5.x */
#     include <errno.h>
#     ifdef ECHRNG
#       define SUNOS5
#     else
#	define SUNOS4
#     endif
#   define mach_type_known
# endif
# if defined(sparc) && defined(unix) && !defined(sun) && !defined(linux) \
     && !defined(__OpenBSD__) && !defined(__NetBSD__) && !defined(__FreeBSD__)
#   define SPARC
#   define DRSNX
#   define mach_type_known
# endif
# if defined(_IBMR2)
#   define RS6000
#   define mach_type_known
# endif
# if defined(__NetBSD__) && defined(__sparc__)
#   define SPARC
#   define mach_type_known
# endif
# if defined(_M_XENIX) && defined(_M_SYSV) && defined(_M_I386)
	/* The above test may need refinement	*/
#   define I386
#   if defined(_SCO_ELF)
#     define SCO_ELF
#   else
#     define SCO
#   endif
#   define mach_type_known
# endif
# if defined(_AUX_SOURCE)
#   define M68K
#   define SYSV
#   define mach_type_known
# endif
# if defined(_PA_RISC1_0) || defined(_PA_RISC1_1) || defined(_PA_RISC2_0) \
     || defined(hppa) || defined(__hppa__)
#   define HP_PA
#   if !defined(LINUX) && !defined(HPUX)
#     define HPUX
#   endif
#   define mach_type_known
# endif
# if defined(__ia64) && defined(_HPUX_SOURCE)
#   define IA64
#   ifndef HPUX
#     define HPUX
#   endif
#   define mach_type_known
# endif
# if defined(__BEOS__) && defined(_X86_)
#    define I386
#    define BEOS
#    define mach_type_known
# endif
# if defined(LINUX) && (defined(i386) || defined(__i386__))
#    define I386
#    define mach_type_known
# endif
# if defined(OPENBSD) && defined(__amd64__)
#    define X86_64
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
# if defined(LINUX) && defined(__arm__)
#    define ARM32
#    define mach_type_known
# endif
# if defined(LINUX) && defined(__cris__)
#    ifndef CRIS
#	define CRIS
#    endif
#    define mach_type_known
# endif
# if defined(LINUX) && (defined(powerpc) || defined(__powerpc__) || \
		        defined(powerpc64) || defined(__powerpc64__))
#    define POWERPC
#    define mach_type_known
# endif
# if defined(FREEBSD) && (defined(powerpc) || defined(__powerpc__))
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
# if defined(LINUX) && defined(__arm__)
#    define ARM32
#    define mach_type_known
# endif
# if defined(LINUX) && defined(__sh__)
#    define SH
#    define mach_type_known
# endif
# if defined(LINUX) && defined(__m32r__)
#    define M32R
#    define mach_type_known
# endif
# if defined(__alpha) || defined(__alpha__)
#   define ALPHA
#   if !defined(LINUX) && !defined(NETBSD) && !defined(OPENBSD) && !defined(FREEBSD)
#     define OSF1	/* a.k.a Digital Unix */
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
# if defined(THINK_C) || defined(__MWERKS__) && !defined(__powerc)
#   define M68K
#   define MACOS
#   define mach_type_known
# endif
# if defined(__MWERKS__) && defined(__powerc) && !defined(__MACH__)
#   define POWERPC
#   define MACOS
#   define mach_type_known
# endif
# ifdef DARWIN
#    include "TargetConditionals.h"
#   if defined(__ppc__)  || defined(__ppc64__)
#    define POWERPC
#    define mach_type_known
#   elif defined(__i386__)
#    define I386
#    define mach_type_known
#    define DARWIN_DONT_PARSE_STACK
#    define OS_TYPE "DARWIN"
#    if TARGET_IPHONE_SIMULATOR == 0
#     define DYNAMIC_LOADING
#    endif
     /* XXX: see get_end(3), get_etext() and get_end() should not be used.
        These aren't used when dyld support is enabled (it is by default) */
#    define DATASTART ((ptr_t) get_etext())
#    define DATAEND	((ptr_t) get_end())
#    define STACKBOTTOM ((ptr_t) pthread_get_stackaddr_np(pthread_self()))
#ifndef USE_MMAP
#    define USE_MMAP
#endif
#    define USE_MMAP_ANON
#    define USE_ASM_PUSH_REGS
     /* This is potentially buggy. It needs more testing. See the comments in
        os_dep.c.  It relies on threads to track writes. */
#    ifdef GC_DARWIN_THREADS
/* #       define MPROTECT_VDB -- diabled for now.  May work for some apps. */
#    endif
#    include <unistd.h>
#    define GETPAGESIZE() getpagesize()
#   elif defined(__arm__)
#    define ARM
#    define mach_type_known
#    define DARWIN_DONT_PARSE_STACK
#    define GC_DONT_REGISTER_MAIN_STATIC_DATA
#   elif defined(__x86_64)
#    define X86_64
#    define mach_type_known
#   endif
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
# if defined(FREEBSD) && (defined(i386) || defined(__i386__))
#   define I386
#   define mach_type_known
# endif
# if defined(FREEBSD) && defined(__x86_64__)
#   define X86_64
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
# if defined(FREEBSD) && defined(__sparc__)
#    define SPARC
#    define mach_type_known
#endif
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
# if defined(_WIN32_WCE)
    /* SH3, SH4, MIPS already defined for corresponding architectures */
#   if defined(SH3) || defined(SH4)
#     define SH
#   endif
#   if defined(x86)
#     define I386
#   endif
#   if defined(ARM)
#     define ARM32
#   endif
#   define MSWINCE
#   define mach_type_known
# else
#   if (defined(_MSDOS) || defined(_MSC_VER)) && (_M_IX86 >= 300) \
        || defined(_WIN32) && !defined(__CYGWIN32__) && !defined(__CYGWIN__)
#     if defined(__LP64__) || defined(_WIN64)
#	define X86_64
#     else
#       define I386
#     endif
#     define MSWIN32	/* or Win64 */
#     define mach_type_known
#   endif
#   if defined(_MSC_VER) && defined(_M_IA64)
#     define IA64
#     define MSWIN32	/* Really win64, but we don't treat 64-bit 	*/
			/* variants as a differnt platform.		*/
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
#   define I386
#   define CYGWIN32
#   define mach_type_known
# endif
# if defined(__MINGW32__)
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
# if defined(__pj__)
#   define PJ
#   define mach_type_known
# endif
# if defined(__embedded__) && defined(PPC)
#   define POWERPC
#   define NOSYS
#   define mach_type_known
# endif
/* Ivan Demakov */
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

/* Feel free to add more clauses here */

/* Or manually define the machine type here.  A machine type is 	*/
/* characterized by the architecture.  Some				*/
/* machine types are further subdivided by OS.				*/
/* the macros ULTRIX, RISCOS, and BSD to distinguish.			*/
/* Note that SGI IRIX is treated identically to RISCOS.			*/
/* SYSV on an M68K actually means A/UX.					*/
/* The distinction in these cases is usually the stack starting address */
# ifndef mach_type_known
	--> unknown machine type
# endif
		    /* Mapping is: M68K       ==> Motorola 680X0	*/
		    /*		   (SUNOS4,HP,NEXT, and SYSV (A/UX),	*/
		    /*		   MACOS and AMIGA variants)		*/
		    /*             I386       ==> Intel 386	 	*/
		    /*		    (SEQUENT, OS2, SCO, LINUX, NETBSD,	*/
		    /*		     FREEBSD, THREE86BSD, MSWIN32,	*/
		    /* 		     BSDI,SUNOS5, NEXT, other variants)	*/
                    /*             NS32K      ==> Encore Multimax 	*/
                    /*             MIPS       ==> R2000 or R3000	*/
                    /*			(RISCOS, ULTRIX variants)	*/
                    /*		   VAX	      ==> DEC VAX		*/
                    /*			(BSD, ULTRIX variants)		*/
                    /*		   RS6000     ==> IBM RS/6000 AIX3.X	*/
                    /*		   RT	      ==> IBM PC/RT		*/
                    /*		   HP_PA      ==> HP9000/700 & /800	*/
                    /*				  HP/UX, LINUX		*/
		    /*		   SPARC      ==> SPARC	v7/v8/v9	*/
		    /*			(SUNOS4, SUNOS5, LINUX,		*/
		    /*			 DRSNX variants)		*/
		    /* 		   ALPHA      ==> DEC Alpha 		*/
		    /*			(OSF1 and LINUX variants)	*/
		    /* 		   M88K       ==> Motorola 88XX0        */
		    /* 		        (CX_UX and DGUX)		*/
		    /* 		   S370	      ==> 370-like machine	*/
		    /* 			running Amdahl UTS4		*/
                    /*             S390       ==> 390-like machine      */
		    /*                  running LINUX                   */
		    /* 		   ARM32      ==> Intel StrongARM	*/
		    /* 		   IA64	      ==> Intel IPF		*/
		    /*				  (e.g. Itanium)	*/
		    /*			(LINUX and HPUX)	        */
		    /*		   SH	      ==> Hitachi SuperH	*/
		    /* 			(LINUX & MSWINCE)		*/
		    /* 		   X86_64     ==> AMD x86-64		*/
		    /*		   POWERPC    ==> IBM/Apple PowerPC	*/
		    /*			(MACOS(<=9),DARWIN(incl.MACOSX),*/
		    /*			 LINUX, NETBSD, NOSYS variants)	*/
		    /*			Handles 32 and 64-bit variants.	*/
		    /* 			AIX should be handled here, but	*/
		    /*			that's called an RS6000.	*/
		    /*		   CRIS       ==> Axis Etrax		*/
		    /*		   M32R	      ==> Renesas M32R		*/


/*
 * For each architecture and OS, the following need to be defined:
 *
 * CPP_WORDSZ is a simple integer constant representing the word size.
 * in bits.  We assume byte addressibility, where a byte has 8 bits.
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
 * DATAEND, if not `end' where `end' is defined as ``extern int end[];''.
 * RTH suggests gaining access to linker script synth'd values with
 * this idiom instead of `&end' where `end' is defined as ``extern int end;'' .
 * Otherwise, ``GCC will assume these are in .sdata/.sbss'' and it will, e.g.,
 * cause failures on alpha*-*-* with ``-msmall-data or -fpic'' or mips-*-*
 * without any special options.
 *
 * ALIGN_DOUBLE of GC_malloc should return blocks aligned to twice
 * the pointer size.
 *
 * STACKBOTTOM is the cool end of the stack, which is usually the
 * highest address in the stack.
 * Under PCR or OS/2, we have other ways of finding thread stacks.
 * For each machine, the following should:
 * 1) define STACK_GROWS_UP if the stack grows toward higher addresses, and
 * 2) define exactly one of
 *	STACKBOTTOM (should be defined to be an expression)
 *	LINUX_STACKBOTTOM
 *	HEURISTIC1
 *	HEURISTIC2
 * If STACKBOTTOM is defined, then it's value will be used directly as the
 * stack base.  If LINUX_STACKBOTTOM is defined, then it will be determined
 * with a method appropriate for most Linux systems.  Currently we look
 * first for __libc_stack_end, and if that fails read it from /proc.
 * If either of the last two macros are defined, then STACKBOTTOM is computed
 * during collector startup using one of the following two heuristics:
 * HEURISTIC1:  Take an address inside GC_init's frame, and round it up to
 *		the next multiple of STACK_GRAN.
 * HEURISTIC2:  Take an address inside GC_init's frame, increment it repeatedly
 *		in small steps (decrement if STACK_GROWS_UP), and read the value
 *		at each location.  Remember the value when the first
 *		Segmentation violation or Bus error is signalled.  Round that
 *		to the nearest plausible page boundary, and use that instead
 *		of STACKBOTTOM.
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
 *    like:
 *
 *		# include "gc_private.h"
 *
 *		main(argc, argv, envp)
 *		int argc;
 *		char **argv, **envp;
 *		{
 *		    int dummy;
 *
 *		    GC_stackbottom = (ptr_t)(&dummy);
 *		    return(real_main(argc, argv, envp));
 *		}
 *
 *
 * Each architecture may also define the style of virtual dirty bit
 * implementation to be used:
 *   MPROTECT_VDB: Write protect the heap and catch faults.
 *   PROC_VDB: Use the SVR4 /proc primitives to read dirty bits.
 *
 * An architecture may define DYNAMIC_LOADING if dynamic_load.c
 * defined GC_register_dynamic_libraries() for the architecture.
 *
 * An architecture may define PREFETCH(x) to preload the cache with *x.
 * This defaults to a no-op.
 *
 * PREFETCH_FOR_WRITE(x) is used if *x is about to be written.
 *
 * An architecture may also define CLEAR_DOUBLE(x) to be a fast way to
 * clear the two words at GC_malloc-aligned address x.  By default,
 * word stores of 0 are used instead.
 *
 * HEAP_START may be defined as the initial address hint for mmap-based
 * allocation.
 */

/* If we are using a recent version of gcc, we can use __builtin_unwind_init()
 * to push the relevant registers onto the stack.  This generally makes
 * USE_GENERIC_PUSH_REGS the preferred approach for marking from registers.
 */
# if defined(__GNUC__) && ((__GNUC__ >= 3) || \
			   (__GNUC__ == 2 && __GNUC_MINOR__ >= 8)) \
		       && !defined(__INTEL_COMPILER) \
 		       && !defined(__PATHCC__)
#   define HAVE_BUILTIN_UNWIND_INIT
# endif

# define STACK_GRAN 0x1000000
# ifdef M68K
#   define MACH_TYPE "M68K"
#   define ALIGNMENT 2
#   ifdef OPENBSD
#	define OS_TYPE "OPENBSD"
#	define HEURISTIC2
#	ifdef __ELF__
#	  define DATASTART GC_data_start
#	  define DYNAMIC_LOADING
#	else
	  extern char etext[];
#	  define DATASTART ((ptr_t)(etext))
#       endif
#       define USE_GENERIC_PUSH_REGS
#   endif
#   ifdef NETBSD
#	define OS_TYPE "NETBSD"
#	define HEURISTIC2
#	ifdef __ELF__
#	  define DATASTART GC_data_start
#	  define DYNAMIC_LOADING
#	else
	  extern char etext[];
#	  define DATASTART ((ptr_t)(etext))
#       endif
#	define USE_GENERIC_PUSH_REGS
#   endif
#   ifdef LINUX
#       define OS_TYPE "LINUX"
#       define STACKBOTTOM ((ptr_t)0xf0000000)
#       define USE_GENERIC_PUSH_REGS
		/* We never got around to the assembly version. */
/* #       define MPROTECT_VDB - Reported to not work  9/17/01 */
#       ifdef __ELF__
#            define DYNAMIC_LOADING
#	     include <features.h>
#	     if defined(__GLIBC__)&& __GLIBC__>=2
#              define SEARCH_FOR_DATA_START
#	     else /* !GLIBC2 */
#              if defined(PLATFORM_ANDROID)
#                      define __environ environ
#              endif
               extern char **__environ;
#              define DATASTART ((ptr_t)(&__environ))
                             /* hideous kludge: __environ is the first */
                             /* word in crt0.o, and delimits the start */
                             /* of the data segment, no matter which   */
                             /* ld options were passed through.        */
                             /* We could use _etext instead, but that  */
                             /* would include .rodata, which may       */
                             /* contain large read-only data tables    */
                             /* that we'd rather not scan.             */
#	     endif /* !GLIBC2 */
             extern int _end[];
#            define DATAEND (_end)
#       else
             extern int etext[];
#            define DATASTART ((ptr_t)((((word) (etext)) + 0xfff) & ~0xfff))
#       endif
#   endif
#   ifdef SUNOS4
#	define OS_TYPE "SUNOS4"
	extern char etext[];
#	define DATASTART ((ptr_t)((((word) (etext)) + 0x1ffff) & ~0x1ffff))
#	define HEURISTIC1	/* differs	*/
#	define DYNAMIC_LOADING
#   endif
#   ifdef HP
#	define OS_TYPE "HP"
	extern char etext[];
#       define DATASTART ((ptr_t)((((word) (etext)) + 0xfff) & ~0xfff))
#       define STACKBOTTOM ((ptr_t) 0xffeffffc)
			      /* empirically determined.  seems to work. */
#  	include <unistd.h>
#	define GETPAGESIZE() sysconf(_SC_PAGE_SIZE)
#   endif
#   ifdef SYSV
#	define OS_TYPE "SYSV"
	extern etext[];
#   	define DATASTART ((ptr_t)((((word) (etext)) + 0x3fffff) \
				   & ~0x3fffff) \
				  +((word)etext & 0x1fff))
	/* This only works for shared-text binaries with magic number 0413.
	   The other sorts of SysV binaries put the data at the end of the text,
	   in which case the default of etext would work.  Unfortunately,
	   handling both would require having the magic-number available.
	   	   		-- Parag
	   */
#	define STACKBOTTOM ((ptr_t)0xFFFFFFFE)
			/* The stack starts at the top of memory, but   */
			/* 0x0 cannot be used as setjump_test complains */
			/* that the stack direction is incorrect.  Two  */
			/* bytes down from 0x0 should be safe enough.   */
			/* 		--Parag				*/
#   	include <sys/mmu.h>
#	define GETPAGESIZE() PAGESIZE	/* Is this still right? */
#   endif
#   ifdef AMIGA
#	define OS_TYPE "AMIGA"
 	    	/* STACKBOTTOM and DATASTART handled specially	*/
 	    	/* in os_dep.c					*/
# 	define DATAEND	/* not needed */
#	define GETPAGESIZE() 4096
#   endif
#   ifdef MACOS
#     ifndef __LOWMEM__
#     include <LowMem.h>
#     endif
#     define OS_TYPE "MACOS"
			/* see os_dep.c for details of global data segments. */
#     define STACKBOTTOM ((ptr_t) LMGetCurStackBase())
#     define DATAEND	/* not needed */
#     define GETPAGESIZE() 4096
#   endif
#   ifdef NEXT
#	define OS_TYPE "NEXT"
#	define DATASTART ((ptr_t) get_etext())
#	define STACKBOTTOM ((ptr_t) 0x4000000)
#	define DATAEND	/* not needed */
#   endif
# endif

# if defined(POWERPC)
#   define MACH_TYPE "POWERPC"
#   ifdef MACOS
#     define ALIGNMENT 2  /* Still necessary?  Could it be 4?	*/
#     ifndef __LOWMEM__
#     include <LowMem.h>
#     endif
#     define OS_TYPE "MACOS"
			/* see os_dep.c for details of global data segments. */
#     define STACKBOTTOM ((ptr_t) LMGetCurStackBase())
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
      /* HEURISTIC1 has been reliably reported to fail for a 32-bit	*/
      /* executable on a 64 bit kernel.					*/
#     define LINUX_STACKBOTTOM
#     define DYNAMIC_LOADING
#     define SEARCH_FOR_DATA_START
      extern int _end[];
#     define DATAEND (_end)
#   endif
#   ifdef DARWIN
#     ifdef __ppc64__
#       define ALIGNMENT 8
#       define CPP_WORDSZ 64
#     else
#       define ALIGNMENT 4
#     endif
#     define OS_TYPE "DARWIN"
#     define DYNAMIC_LOADING
      /* XXX: see get_end(3), get_etext() and get_end() should not be used.
         These aren't used when dyld support is enabled (it is by default) */
#     define DATASTART ((ptr_t) get_etext())
#     define DATAEND	((ptr_t) get_end())
#     define STACKBOTTOM ((ptr_t) 0xc0000000)
#     define USE_MMAP
#     define USE_MMAP_ANON
#     define USE_ASM_PUSH_REGS
      /* This is potentially buggy. It needs more testing. See the comments in
         os_dep.c.  It relies on threads to track writes. */
#     ifdef GC_DARWIN_THREADS
/* #       define MPROTECT_VDB -- diabled for now.  May work for some apps. */
#     endif
#     include <unistd.h>
#     define GETPAGESIZE() getpagesize()
#     if defined(USE_PPC_PREFETCH) && defined(__GNUC__)
	/* The performance impact of prefetches is untested */
#	define PREFETCH(x) \
	  __asm__ __volatile__ ("dcbt 0,%0" : : "r" ((const void *) (x)))
#	define PREFETCH_FOR_WRITE(x) \
	  __asm__ __volatile__ ("dcbtst 0,%0" : : "r" ((const void *) (x)))
#     endif
      /* There seems to be some issues with trylock hanging on darwin. This
         should be looked into some more */
#     define NO_PTHREAD_TRYLOCK
#   endif
#   ifdef FREEBSD
#       define ALIGNMENT 4
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
        extern char * GC_FreeBSDGetDataStart();
#       define DATASTART GC_FreeBSDGetDataStart(0x1000, &etext)
#   endif
#   ifdef NETBSD
#     define ALIGNMENT 4
#     define OS_TYPE "NETBSD"
#     define HEURISTIC2
      extern char etext[];
#     define DATASTART GC_data_start
#     define DYNAMIC_LOADING
#   endif
#   ifdef SN_TARGET_PS3
#       define NO_GETENV
#       define CPP_WORDSZ 32
#       define ALIGNMENT 4
        extern int _end [];
//       extern int _dso_handle[];
		extern int __bss_start;

#       define DATAEND (_end)
#       define DATASTART (__bss_start)
#       define STACKBOTTOM ((ptr_t) ps3_get_stack_bottom ())
#       define USE_GENERIC_PUSHREGS
#   endif

#   ifdef NOSYS
#     define ALIGNMENT 4
#     define OS_TYPE "NOSYS"
      extern void __end[], __dso_handle[];
#     define DATASTART (__dso_handle)  /* OK, that's ugly.  */
#     define DATAEND (__end)
	/* Stack starts at 0xE0000000 for the simulator.  */
#     undef STACK_GRAN
#     define STACK_GRAN 0x10000000
#     define HEURISTIC1
#   endif
# endif


# ifdef NACL
#   define OS_TYPE "NACL"
#   if defined(__GLIBC__)
#      define DYNAMIC_LOADING
#   endif
#   define DATASTART ((ptr_t)0x10020000)
    extern int _end[];
#   define DATAEND (_end)
#   ifdef STACK_GRAN
#      undef STACK_GRAN
#   endif /* STACK_GRAN */
#   define STACK_GRAN 0x10000
#   define HEURISTIC1
#   define USE_MMAP
#   define USE_MUNMAP
#   define USE_MMAP_ANON
#   ifdef USE_MMAP_FIXED
#	undef USE_MMAP_FIXED
#   endif
#   define GETPAGESIZE() 65536
#   define MAX_NACL_GC_THREADS 1024
# endif

# ifdef VAX
#   define MACH_TYPE "VAX"
#   define ALIGNMENT 4	/* Pointers are longword aligned by 4.2 C compiler */
    extern char etext[];
#   define DATASTART ((ptr_t)(etext))
#   ifdef BSD
#	define OS_TYPE "BSD"
#	define HEURISTIC1
			/* HEURISTIC2 may be OK, but it's hard to test. */
#   endif
#   ifdef ULTRIX
#	define OS_TYPE "ULTRIX"
#	define STACKBOTTOM ((ptr_t) 0x7fffc800)
#   endif
# endif

# ifdef RT
#   define MACH_TYPE "RT"
#   define ALIGNMENT 4
#   define DATASTART ((ptr_t) 0x10000000)
#   define STACKBOTTOM ((ptr_t) 0x1fffd800)
# endif

# ifdef SPARC
#   define MACH_TYPE "SPARC"
#   if defined(__arch64__) || defined(__sparcv9)
#     define ALIGNMENT 8
#     define CPP_WORDSZ 64
#     define ELF_CLASS ELFCLASS64
#   else
#     define ALIGNMENT 4	/* Required by hardware	*/
#     define CPP_WORDSZ 32
#   endif
#   define ALIGN_DOUBLE
#   ifdef _FILE_OFFSET_BITS
#     undef _FILE_OFFSET_BITS /* libelf.h & procfs.h doesn't compile with large file support */
#     define _FILE_OFFSET_BITS 32
#   endif
#   ifdef SUNOS5
#	define OS_TYPE "SUNOS5"
	extern int _etext[];
	extern int _end[];
	extern ptr_t GC_SysVGetDataStart();
#       define DATASTART GC_SysVGetDataStart(0x10000, _etext)
#	define DATAEND (_end)
#	if !defined(USE_MMAP) && defined(REDIRECT_MALLOC)
#	    define USE_MMAP
	    /* Otherwise we now use calloc.  Mmap may result in the	*/
	    /* heap interleaved with thread stacks, which can result in	*/
	    /* excessive blacklisting.  Sbrk is unusable since it	*/
	    /* doesn't interact correctly with the system malloc.	*/
#	endif
#       ifdef USE_MMAP
#         define HEAP_START (ptr_t)0x40000000
#       else
#	  define HEAP_START DATAEND
#       endif
#	define PROC_VDB
/*	HEURISTIC1 reportedly no longer works under 2.7.  		*/
/*  	HEURISTIC2 probably works, but this appears to be preferable.	*/
/*	Apparently USRSTACK is defined to be USERLIMIT, but in some	*/
/* 	installations that's undefined.  We work around this with a	*/
/*	gross hack:							*/
#       include <sys/vmparam.h>
#	ifdef USERLIMIT
	  /* This should work everywhere, but doesn't.	*/
#	  define STACKBOTTOM USRSTACK
#       else
#	  define HEURISTIC2
#       endif
#	include <unistd.h>
#       define GETPAGESIZE()  sysconf(_SC_PAGESIZE)
		/* getpagesize() appeared to be missing from at least one */
		/* Solaris 5.4 installation.  Weird.			  */
#	define DYNAMIC_LOADING
#   endif
#   ifdef SUNOS4
#	define OS_TYPE "SUNOS4"
	/* [If you have a weak stomach, don't read this.]		*/
	/* We would like to use:					*/
/* #       define DATASTART ((ptr_t)((((word) (etext)) + 0x1fff) & ~0x1fff)) */
	/* This fails occasionally, due to an ancient, but very 	*/
	/* persistent ld bug.  etext is set 32 bytes too high.		*/
	/* We instead read the text segment size from the a.out		*/
	/* header, which happens to be mapped into our address space	*/
	/* at the start of the text segment.  The detective work here	*/
	/* was done by Robert Ehrlich, Manuel Serrano, and Bernard	*/
	/* Serpette of INRIA.						*/
	/* This assumes ZMAGIC, i.e. demand-loadable executables.	*/
#	define TEXTSTART 0x2000
#       define DATASTART ((ptr_t)(*(int *)(TEXTSTART+0x4)+TEXTSTART))
#	define MPROTECT_VDB
#	define HEURISTIC1
# 	define DYNAMIC_LOADING
#   endif
#   ifdef DRSNX
#	define OS_TYPE "DRSNX"
	extern ptr_t GC_SysVGetDataStart();
	extern int etext[];
#       define DATASTART GC_SysVGetDataStart(0x10000, etext)
#	define MPROTECT_VDB
#       define STACKBOTTOM ((ptr_t) 0xdfff0000)
#	define DYNAMIC_LOADING
#   endif
#   ifdef LINUX
#     define OS_TYPE "LINUX"
#     ifdef __ELF__
#       define DYNAMIC_LOADING
#     else
          Linux Sparc/a.out not supported
#     endif
#     define SVR4
#     include <features.h>
#     if defined(__GLIBC__) && __GLIBC__ >= 2
#       define SEARCH_FOR_DATA_START
#     else
          extern char **__environ;
#         define DATASTART ((ptr_t)(&__environ))
                     /* hideous kludge: __environ is the first */
                     /* word in crt0.o, and delimits the start */
                     /* of the data segment, no matter which   */
                     /* ld options were passed through.        */
                     /* We could use _etext instead, but that  */
                     /* would include .rodata, which may       */
                     /* contain large read-only data tables    */
                     /* that we'd rather not scan.             */
#     endif
      extern int _end[];
#     define DATAEND (_end)
#     define LINUX_STACKBOTTOM
#   endif
#   ifdef OPENBSD
#     define OS_TYPE "OPENBSD"
#     define STACKBOTTOM ((ptr_t) 0xf8000000)
      extern int etext[];
#     define DATASTART ((ptr_t)(etext))
#   endif
#   ifdef NETBSD
#     define OS_TYPE "NETBSD"
#     define HEURISTIC2
#     ifdef __ELF__
#	define DATASTART GC_data_start
#	define DYNAMIC_LOADING
#     else
	extern char etext[];
#	define DATASTART ((ptr_t)(etext))
#     endif
#   endif
#   ifdef FREEBSD
#	define OS_TYPE "FREEBSD"
#	define SIG_SUSPEND SIGUSR1
#	define SIG_THR_RESTART SIGUSR2
#	define FREEBSD_STACKBOTTOM
#	ifdef __ELF__
#	    define DYNAMIC_LOADING
#	endif
	extern char etext[];
	extern char edata[];
	extern char end[];
#	define NEED_FIND_LIMIT
#	define DATASTART ((ptr_t)(&etext))
#	define DATAEND (GC_find_limit (DATASTART, TRUE))
#	define DATASTART2 ((ptr_t)(&edata))
#	define DATAEND2 ((ptr_t)(&end))
#   endif
# endif

# ifdef I386
#   if defined( NACL )
#     define MACH_TYPE "NACL"
#     define CPP_WORDSZ 32
#     define ALIGNMENT 4
#   else
#     define MACH_TYPE "I386"
#     if defined(__LP64__) || defined(_WIN64)
#       define CPP_WORDSZ 64
#       define ALIGNMENT 8
#     else
#       define CPP_WORDSZ 32
#       define ALIGNMENT 4
#     endif
			/* Appears to hold for all "32 bit" compilers	*/
			/* except Borland.  The -a4 option fixes 	*/
			/* Borland.					*/
                        /* Ivan Demakov: For Watcom the option is -zp4. */
#   endif
#   ifndef SMALL_CONFIG
#     define ALIGN_DOUBLE /* Not strictly necessary, but may give speed   */
			  /* improvement on Pentiums.			  */
#   endif
#   ifdef HAVE_BUILTIN_UNWIND_INIT
#	define USE_GENERIC_PUSH_REGS
#   endif
#   ifdef SEQUENT
#	define OS_TYPE "SEQUENT"
	extern int etext[];
#       define DATASTART ((ptr_t)((((word) (etext)) + 0xfff) & ~0xfff))
#       define STACKBOTTOM ((ptr_t) 0x3ffff000) 
#   endif
#   ifdef BEOS
#     define OS_TYPE "BEOS"
#     include <OS.h>
#     define GETPAGESIZE() B_PAGE_SIZE
      extern int etext[];
#     define DATASTART ((ptr_t)((((word) (etext)) + 0xfff) & ~0xfff))
#   endif
#   ifdef SUNOS5
#	define OS_TYPE "SUNOS5"
        extern int _etext[], _end[];
  	extern ptr_t GC_SysVGetDataStart();
#       define DATASTART GC_SysVGetDataStart(0x1000, _etext)
#	define DATAEND (_end)
/*	# define STACKBOTTOM ((ptr_t)(_start)) worked through 2.7,  	*/
/*      but reportedly breaks under 2.8.  It appears that the stack	*/
/* 	base is a property of the executable, so this should not break	*/
/* 	old executables.						*/
/*  	HEURISTIC2 probably works, but this appears to be preferable.	*/
#       include <sys/vm.h>
#	define STACKBOTTOM USRSTACK
/* At least in Solaris 2.5, PROC_VDB gives wrong values for dirty bits. */
/* It appears to be fixed in 2.8 and 2.9.				*/
#	ifdef SOLARIS25_PROC_VDB_BUG_FIXED
/* Reading of dirty bits doesn't seem to work even in solaris 10 */
/*#	  define PROC_VDB */
#	endif
#	define DYNAMIC_LOADING
#	if !defined(USE_MMAP) && defined(REDIRECT_MALLOC)
#	    define USE_MMAP
	    /* Otherwise we now use calloc.  Mmap may result in the	*/
	    /* heap interleaved with thread stacks, which can result in	*/
	    /* excessive blacklisting.  Sbrk is unusable since it	*/
	    /* doesn't interact correctly with the system malloc.	*/
#	endif
#       ifdef USE_MMAP
#         define HEAP_START (ptr_t)0x40000000
#       else
#	  define HEAP_START DATAEND
#       endif
#   endif
#   ifdef SCO
#	define OS_TYPE "SCO"
	extern int etext[];
#   	define DATASTART ((ptr_t)((((word) (etext)) + 0x3fffff) \
				  & ~0x3fffff) \
				 +((word)etext & 0xfff))
#	define STACKBOTTOM ((ptr_t) 0x7ffffffc)
#   endif
#   ifdef SCO_ELF
#       define OS_TYPE "SCO_ELF"
        extern int etext[];
#       define DATASTART ((ptr_t)(etext))
#       define STACKBOTTOM ((ptr_t) 0x08048000)
#       define DYNAMIC_LOADING
#	define ELF_CLASS ELFCLASS32
#   endif
#   ifdef DGUX
#	define OS_TYPE "DGUX"
	extern int _etext, _end;
	extern ptr_t GC_SysVGetDataStart();
#	define DATASTART GC_SysVGetDataStart(0x1000, &_etext)
#	define DATAEND (&_end)
#	define STACK_GROWS_DOWN
#	define HEURISTIC2
#	include <unistd.h>
#	define GETPAGESIZE()  sysconf(_SC_PAGESIZE)
#	define DYNAMIC_LOADING
#	ifndef USE_MMAP
#	  define USE_MMAP
#	endif /* USE_MMAP */
#	define MAP_FAILED (void *) -1
#	ifdef USE_MMAP
#	  define HEAP_START (ptr_t)0x40000000
#	else /* USE_MMAP */
#	  define HEAP_START DATAEND
#	endif /* USE_MMAP */
#   endif /* DGUX */
#   ifdef LINUX
#	ifndef __GNUC__
	  /* The Intel compiler doesn't like inline assembly */
#	  define USE_GENERIC_PUSH_REGS
# 	endif
#	define OS_TYPE "LINUX"
#       define LINUX_STACKBOTTOM
#	if 0
#	  define HEURISTIC1
#         undef STACK_GRAN
#         define STACK_GRAN 0x10000000
	  /* STACKBOTTOM is usually 0xc0000000, but this changes with	*/
	  /* different kernel configurations.  In particular, systems	*/
	  /* with 2GB physical memory will usually move the user	*/
	  /* address space limit, and hence initial SP to 0x80000000.	*/
#       endif
#       if !defined(GC_LINUX_THREADS) || !defined(REDIRECT_MALLOC)
#	    define MPROTECT_VDB
#	else
	    /* We seem to get random errors in incremental mode,	*/
	    /* possibly because Linux threads is itself a malloc client */
	    /* and can't deal with the signals.				*/
#	endif
#	define HEAP_START (ptr_t)0x1000
		/* This encourages mmap to give us low addresses,	*/
		/* thus allowing the heap to grow to ~3GB		*/
#       ifdef __ELF__
#            define DYNAMIC_LOADING
#	     ifdef UNDEFINED	/* includes ro data */
	       extern int _etext[];
#              define DATASTART ((ptr_t)((((word) (_etext)) + 0xfff) & ~0xfff))
#	     endif
#	     include <features.h>
#	     if defined(__GLIBC__) && __GLIBC__ >= 2
#		 define SEARCH_FOR_DATA_START
#	     else
     	         extern char **__environ;
#                define DATASTART ((ptr_t)(&__environ))
			      /* hideous kludge: __environ is the first */
			      /* word in crt0.o, and delimits the start */
			      /* of the data segment, no matter which   */
			      /* ld options were passed through.        */
			      /* We could use _etext instead, but that  */
			      /* would include .rodata, which may       */
			      /* contain large read-only data tables    */
			      /* that we'd rather not scan.		*/
#	     endif
	     extern int _end[];
#	     define DATAEND (_end)
#	else
	     extern int etext[];
#            define DATASTART ((ptr_t)((((word) (etext)) + 0xfff) & ~0xfff))
#       endif
#	ifdef USE_I686_PREFETCH
	  /* FIXME: Thus should use __builtin_prefetch, but we'll leave that	*/
	  /* for the next rtelease.						*/
#	  define PREFETCH(x) \
	    __asm__ __volatile__ ("	prefetchnta	%0": : "m"(*(char *)(x)))
	    /* Empirically prefetcht0 is much more effective at reducing	*/
	    /* cache miss stalls for the targetted load instructions.  But it	*/
	    /* seems to interfere enough with other cache traffic that the net	*/
	    /* result is worse than prefetchnta.				*/
#         if 0 
	    /* Using prefetches for write seems to have a slight negative	*/
	    /* impact on performance, at least for a PIII/500.			*/
#	    define PREFETCH_FOR_WRITE(x) \
	      __asm__ __volatile__ ("	prefetcht0	%0": : "m"(*(char *)(x)))
#	  endif
#	endif
#	ifdef USE_3DNOW_PREFETCH
#	  define PREFETCH(x) \
	    __asm__ __volatile__ ("	prefetch	%0": : "m"(*(char *)(x)))
#	  define PREFETCH_FOR_WRITE(x) \
	    __asm__ __volatile__ ("	prefetchw	%0": : "m"(*(char *)(x)))
#	endif
#   endif
#   ifdef CYGWIN32
#       define OS_TYPE "CYGWIN32"
#       define DATASTART ((ptr_t)GC_DATASTART)  /* From gc.h */
#       define DATAEND	 ((ptr_t)GC_DATAEND)
#	undef STACK_GRAN
#       define STACK_GRAN 0x10000
#       define HEURISTIC1
#   endif
#   ifdef OS2
#	define OS_TYPE "OS2"
 	    	/* STACKBOTTOM and DATASTART are handled specially in 	*/
		/* os_dep.c. OS2 actually has the right			*/
		/* system call!						*/
#	define DATAEND	/* not needed */
#	define USE_GENERIC_PUSH_REGS
#   endif
#   ifdef MSWIN32
#	define OS_TYPE "MSWIN32"
		/* STACKBOTTOM and DATASTART are handled specially in 	*/
		/* os_dep.c.						*/
#       ifndef __WATCOMC__
#	  define MPROTECT_VDB
#	endif
#       define DATAEND  /* not needed */
#   endif
#   ifdef MSWINCE
#	define OS_TYPE "MSWINCE"
#       define DATAEND  /* not needed */
#   endif
#   ifdef DJGPP
#       define OS_TYPE "DJGPP"
#       include "stubinfo.h"
        extern int etext[];
        extern int _stklen;
        extern int __djgpp_stack_limit;
#       define DATASTART ((ptr_t)((((word) (etext)) + 0x1ff) & ~0x1ff))
/* #       define STACKBOTTOM ((ptr_t)((word) _stubinfo + _stubinfo->size \
                                                     + _stklen)) */
#       define STACKBOTTOM ((ptr_t)((word) __djgpp_stack_limit + _stklen))
		/* This may not be right.  */
#   endif
#   ifdef OPENBSD
#	define OS_TYPE "OPENBSD"
#    ifdef GC_OPENBSD_THREADS
#       define UTHREAD_SP_OFFSET 192
#    else
#       include <sys/param.h>
#       include <uvm/uvm_extern.h>
#       define STACKBOTTOM USRSTACK
#    endif
        extern int __data_start[];
#       define DATASTART ((ptr_t)(__data_start))
        extern char _end[];
#       define DATAEND ((ptr_t)(&_end))
#       define DYNAMIC_LOADING
#   endif
#   ifdef FREEBSD
#	define OS_TYPE "FREEBSD"
#	ifndef GC_FREEBSD_THREADS
#	    define MPROTECT_VDB
#	endif
#      define SIG_SUSPEND SIGTSTP
#      define SIG_THR_RESTART SIGCONT
#	define FREEBSD_STACKBOTTOM
#	ifdef __ELF__
#	    define DYNAMIC_LOADING
#	endif
	extern char etext[];
	extern char * GC_FreeBSDGetDataStart();
#	define DATASTART GC_FreeBSDGetDataStart(0x1000, &etext)
#   endif
#   ifdef NETBSD
#	define OS_TYPE "NETBSD"
#	ifdef __ELF__
#	    define DYNAMIC_LOADING
#	endif
#   endif
#   ifdef THREE86BSD
#	define OS_TYPE "THREE86BSD"
#   endif
#   ifdef BSDI
#	define OS_TYPE "BSDI"
#   endif
#   if defined(OPENBSD) || defined(NETBSD) \
        || defined(THREE86BSD) || defined(BSDI)
#	define HEURISTIC2
	extern char etext[];
#	define DATASTART ((ptr_t)(etext))
#   endif
#   ifdef NEXT
#	define OS_TYPE "NEXT"
#	define DATASTART ((ptr_t) get_etext())
#	define STACKBOTTOM ((ptr_t)0xc0000000)
#	define DATAEND	/* not needed */
#   endif
#   ifdef DOS4GW
#     define OS_TYPE "DOS4GW"
      extern long __nullarea;
      extern char _end;
      extern char *_STACKTOP;
      /* Depending on calling conventions Watcom C either precedes
         or does not precedes with undescore names of C-variables.
         Make sure startup code variables always have the same names.  */
      #pragma aux __nullarea "*";
      #pragma aux _end "*";
#     define STACKBOTTOM ((ptr_t) _STACKTOP)
                         /* confused? me too. */
#     define DATASTART ((ptr_t) &__nullarea)
#     define DATAEND ((ptr_t) &_end)
#   endif
#   ifdef HURD
#     define OS_TYPE "HURD"
#     define STACK_GROWS_DOWN
#     define HEURISTIC2
      extern int  __data_start[];
#     define DATASTART ( (ptr_t) (__data_start))
      extern int   _end[];
#     define DATAEND ( (ptr_t) (_end))
/* #     define MPROTECT_VDB  Not quite working yet? */
#     define DYNAMIC_LOADING
#   endif
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
#   define STACKBOTTOM ((ptr_t) 0xfffff000) /* for Encore */
# endif

# ifdef MIPS
#   define MACH_TYPE "MIPS"
#   ifdef LINUX
      /* This was developed for a linuxce style platform.  Probably	*/
      /* needs to be tweaked for workstation class machines.		*/
#     define OS_TYPE "LINUX"
#     define DYNAMIC_LOADING
      extern int _end[];
#     define DATAEND (_end)
#pragma weak __data_start
      extern int __data_start[];
#     define DATASTART ((ptr_t)(__data_start))
#     if defined(_MIPS_SZPTR) && (_MIPS_SZPTR == 64)
#        define ALIGNMENT 8
#        define CPP_WORDSZ 64
#     else
#        define ALIGNMENT 4
#     endif
#     define USE_GENERIC_PUSH_REGS
#     if __GLIBC__ == 2 && __GLIBC_MINOR__ >= 2 || __GLIBC__ > 2
#        define LINUX_STACKBOTTOM
#     else
#        define STACKBOTTOM 0x80000000
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
#        define DATASTART ((ptr_t)((((word)etext + 0x3ffff) & ~0x3ffff) \
               + ((word)etext & 0xffff)))
#        define DATAEND (edata)
#        define DATASTART2 (_DYNAMIC_LINKING \
               ? (ptr_t)(((word)_gp + 0x8000 + 0x3ffff) & ~0x3ffff) \
               : (ptr_t)edata)
#        define DATAEND2 (end)
#        define ALIGNMENT 4
#      endif
#      define OS_TYPE "EWS4800"
#      define USE_GENERIC_PUSH_REGS 1
#   endif
#   ifdef ULTRIX
#	define HEURISTIC2
#       define DATASTART (ptr_t)0x10000000
			      /* Could probably be slightly higher since */
			      /* startup code allocates lots of stuff.   */
#	define OS_TYPE "ULTRIX"
#       define ALIGNMENT 4
#   endif
#   ifdef RISCOS
#	define HEURISTIC2
#       define DATASTART (ptr_t)0x10000000
#	define OS_TYPE "RISCOS"
#   	define ALIGNMENT 4  /* Required by hardware */
#   endif
#   ifdef IRIX5
#	define HEURISTIC2
        extern int _fdata[];
#       define DATASTART ((ptr_t)(_fdata))
#       ifdef USE_MMAP
#         define HEAP_START (ptr_t)0x30000000
#       else
#	  define HEAP_START DATASTART
#       endif
			      /* Lowest plausible heap address.		*/
			      /* In the MMAP case, we map there.	*/
			      /* In either case it is used to identify	*/
			      /* heap sections so they're not 		*/
			      /* considered as roots.			*/
#	define OS_TYPE "IRIX5"
/*#       define MPROTECT_VDB DOB: this should work, but there is evidence */
/* 	  	of recent breakage.					   */
#       ifdef _MIPS_SZPTR
#	  define CPP_WORDSZ _MIPS_SZPTR
#	  define ALIGNMENT (_MIPS_SZPTR/8)
#	  if CPP_WORDSZ != 64
#	    define ALIGN_DOUBLE
#	  endif
#	else
#         define ALIGNMENT 4
#	  define ALIGN_DOUBLE
#	endif
#	define DYNAMIC_LOADING
#   endif
#   ifdef MSWINCE
#       define OS_TYPE "MSWINCE"
#       define ALIGNMENT 4
#       define DATAEND /* not needed */
#   endif
#   if defined(NETBSD)
#     define ALIGNMENT 4
#     define OS_TYPE "NETBSD"
#     define HEURISTIC2
#     define USE_GENERIC_PUSH_REGS
#     ifdef __ELF__
        extern int etext[];
#       define DATASTART GC_data_start
#       define NEED_FIND_LIMIT
#       define DYNAMIC_LOADING
#     else
#       define DATASTART ((ptr_t) 0x10000000)
#       define STACKBOTTOM ((ptr_t) 0x7ffff000)
#     endif /* _ELF_ */
#  endif
# endif

# ifdef RS6000
#   define MACH_TYPE "RS6000"
#   ifdef ALIGNMENT
#     undef ALIGNMENT
#   endif
#   ifdef IA64
#     undef IA64 /* DOB: some AIX installs stupidly define IA64 in /usr/include/sys/systemcfg.h */
#   endif
#   ifdef __64BIT__
#     define ALIGNMENT 8
#     define CPP_WORDSZ 64
#     define STACKBOTTOM ((ptr_t)0x1000000000000000)
#   else
#     define ALIGNMENT 4
#     define CPP_WORDSZ 32
#     define STACKBOTTOM ((ptr_t)((ulong)&errno))
#   endif
#   define USE_MMAP
#   define USE_MMAP_ANON
 /* From AIX linker man page:
 _text Specifies the first location of the program.
 _etext Specifies the first location after the program.
 _data Specifies the first location of the data.
 _edata Specifies the first location after the initialized data
 _end or end Specifies the first location after all data.
 */
    extern int _data[], _end[];
#   define DATASTART ((ptr_t)((ulong)_data))
#   define DATAEND ((ptr_t)((ulong)_end))
    extern int errno;
#   define USE_GENERIC_PUSH_REGS
#   define DYNAMIC_LOADING
	/* For really old versions of AIX, this may have to be removed. */
# endif

# ifdef HP_PA
#   define MACH_TYPE "HP_PA"
#   ifdef __LP64__
#     define CPP_WORDSZ 64
#     define ALIGNMENT 8
#   else
#     define CPP_WORDSZ 32
#     define ALIGNMENT 4
#     define ALIGN_DOUBLE
#   endif
#   if !defined(GC_HPUX_THREADS) && !defined(GC_LINUX_THREADS)
#     ifndef LINUX /* For now. */
#       define MPROTECT_VDB
#     endif
#   else
#     define GENERIC_COMPARE_AND_SWAP
	/* No compare-and-swap instruction.  Use pthread mutexes 	*/
	/* when we absolutely have to.					*/
#     ifdef PARALLEL_MARK
#	define USE_MARK_BYTES
		/* Minimize compare-and-swap usage.		*/
#     endif
#   endif
#   define STACK_GROWS_UP
#   ifdef HPUX
#     define OS_TYPE "HPUX"
      extern int __data_start[];
#     define DATASTART ((ptr_t)(__data_start))
#     if 0
	/* The following appears to work for 7xx systems running HP/UX	*/
	/* 9.xx Furthermore, it might result in much faster		*/
	/* collections than HEURISTIC2, which may involve scanning	*/
	/* segments that directly precede the stack.  It is not the	*/
	/* default, since it may not work on older machine/OS		*/
	/* combinations. (Thanks to Raymond X.T. Nijssen for uncovering	*/
	/* this.)							*/
#       define STACKBOTTOM ((ptr_t) 0x7b033000)  /* from /etc/conf/h/param.h */
#     else
	/* Gustavo Rodriguez-Rivera suggested changing HEURISTIC2	*/
	/* to this.  Note that the GC must be initialized before the	*/
    	/* first putenv call.						*/
	extern char ** environ;
#       define STACKBOTTOM ((ptr_t)environ)
#     endif
#     define DYNAMIC_LOADING
#     include <unistd.h>
#     define GETPAGESIZE() sysconf(_SC_PAGE_SIZE)
#     ifndef __GNUC__
#       define PREFETCH(x)  { \
                              register long addr = (long)(x); \
                              (void) _asm ("LDW", 0, 0, addr, 0); \
                            }
#     endif
#   endif /* HPUX */
#   ifdef LINUX
#     define OS_TYPE "LINUX"
#     define LINUX_STACKBOTTOM
#     define DYNAMIC_LOADING
#     define SEARCH_FOR_DATA_START
      extern int _end[];
#     define DATAEND (&_end)
#   endif /* LINUX */
# endif /* HP_PA */

# ifdef ALPHA
#   define MACH_TYPE "ALPHA"
#   define ALIGNMENT 8
#   define CPP_WORDSZ 64
#   ifndef LINUX
#     define USE_GENERIC_PUSH_REGS
      /* Gcc and probably the DEC/Compaq compiler spill pointers to preserved */
      /* fp registers in some cases when the target is a 21264.  The assembly */
      /* code doesn't handle that yet, and version dependencies make that a   */
      /* bit tricky.  Do the easy thing for now.				    */
#   endif
#   ifdef NETBSD
#	define OS_TYPE "NETBSD"
#	define HEURISTIC2
#	define DATASTART GC_data_start
#	define ELFCLASS32 32
#	define ELFCLASS64 64
#	define ELF_CLASS ELFCLASS64
#       define DYNAMIC_LOADING
#   endif
#   ifdef OPENBSD
#	define OS_TYPE "OPENBSD"
#	define HEURISTIC2
#   	ifdef __ELF__	/* since OpenBSD/Alpha 2.9 */
#	   define DATASTART GC_data_start
#   	   define ELFCLASS32 32
#   	   define ELFCLASS64 64
#   	   define ELF_CLASS ELFCLASS64
#       else		/* ECOFF, until OpenBSD/Alpha 2.7 */
#   	   define DATASTART ((ptr_t) 0x140000000)
#   	endif
#   endif
#   ifdef FREEBSD
#	define OS_TYPE "FREEBSD"
/* MPROTECT_VDB is not yet supported at all on FreeBSD/alpha. */
#      define SIG_SUSPEND SIGTSTP
#      define SIG_THR_RESTART SIGCONT
#	define FREEBSD_STACKBOTTOM
#	ifdef __ELF__
#	    define DYNAMIC_LOADING
#	endif
/* Handle unmapped hole alpha*-*-freebsd[45]* puts between etext and edata. */
	extern char etext[];
	extern char edata[];
	extern char end[];
#	define NEED_FIND_LIMIT
#	define DATASTART ((ptr_t)(&etext))
#	define DATAEND (GC_find_limit (DATASTART, TRUE))
#	define DATASTART2 ((ptr_t)(&edata))
#	define DATAEND2 ((ptr_t)(&end))
#   endif
#   ifdef OSF1
#	define OS_TYPE "OSF1"
#   	define DATASTART ((ptr_t) 0x140000000)
	extern int _end[];
#   	define DATAEND ((ptr_t) &_end)
 	extern char ** environ;
	/* round up from the value of environ to the nearest page boundary */
	/* Probably breaks if putenv is called before collector 	   */
	/* initialization.						   */
#	define STACKBOTTOM ((ptr_t)(((word)(environ) | (getpagesize()-1))+1))
/* #   	define HEURISTIC2 */
	/* Normally HEURISTIC2 is too conervative, since		*/
	/* the text segment immediately follows the stack.		*/
	/* Hence we give an upper pound.				*/
	/* This is currently unused, since we disabled HEURISTIC2	*/
    	extern int __start[];
#   	define HEURISTIC2_LIMIT ((ptr_t)((word)(__start) & ~(getpagesize()-1)))
#	ifndef GC_OSF1_THREADS
	  /* Unresolved signal issues with threads.	*/
#   	  define MPROTECT_VDB
#       endif
#   	define DYNAMIC_LOADING
#   endif
#   ifdef LINUX
#       define OS_TYPE "LINUX"
#       define LINUX_STACKBOTTOM
#       ifdef __ELF__
#	  define SEARCH_FOR_DATA_START
#         define DYNAMIC_LOADING
#       else
#           define DATASTART ((ptr_t) 0x140000000)
#       endif
	extern int _end[];
#	define DATAEND (_end)
#	define MPROTECT_VDB
		/* Has only been superficially tested.  May not	*/
		/* work on all versions.			*/
#   endif
# endif

# ifdef IA64
#   define MACH_TYPE "IA64"
#   define USE_GENERIC_PUSH_REGS
	/* We need to get preserved registers in addition to register   */
	/* windows.   That's easiest to do with setjmp.			*/
#   ifdef PARALLEL_MARK
#	define USE_MARK_BYTES
	    /* Compare-and-exchange is too expensive to use for 	*/
	    /* setting mark bits.					*/
#   endif
#   ifdef HPUX
#	ifdef _ILP32
#	  define CPP_WORDSZ 32
#         define ALIGN_DOUBLE
	    /* Requires 8 byte alignment for malloc */
#   	  define ALIGNMENT 4
#       else
#	  ifndef _LP64
		---> unknown ABI
#         endif
#	  define CPP_WORDSZ 64
#   	  define ALIGN_DOUBLE
	    /* Requires 16 byte alignment for malloc */
#         define ALIGNMENT 8
#       endif
#       define OS_TYPE "HPUX"	
        extern int __data_start[];
#       define DATASTART ((ptr_t)(__data_start))
        /* Gustavo Rodriguez-Rivera suggested changing HEURISTIC2	*/
        /* to this.  Note that the GC must be initialized before the	*/
    	/* first putenv call.						*/
	extern char ** environ;
#       define STACKBOTTOM ((ptr_t)environ)
#       define HPUX_STACKBOTTOM
#       define DYNAMIC_LOADING
#       include <unistd.h>
#       define GETPAGESIZE() sysconf(_SC_PAGE_SIZE)
 	/* The following was empirically determined, and is probably	*/
	/* not very robust.						*/
	/* Note that the backing store base seems to be at a nice 	*/
	/* address minus one page.					*/
#	define BACKING_STORE_DISPLACEMENT 0x1000000
#	define BACKING_STORE_ALIGNMENT 0x1000
	extern ptr_t GC_register_stackbottom;
#	define BACKING_STORE_BASE GC_register_stackbottom
	/* Known to be wrong for recent HP/UX versions!!!	*/
#   endif
#   ifdef LINUX
#   	define CPP_WORDSZ 64
#   	define ALIGN_DOUBLE
	  /* Requires 16 byte alignment for malloc */
#   	define ALIGNMENT 8
#       define OS_TYPE "LINUX"
	/* The following works on NUE and older kernels:	*/
/* #       define STACKBOTTOM ((ptr_t) 0xa000000000000000l)	*/
	/* This does not work on NUE:				*/
#       define LINUX_STACKBOTTOM
	/* We also need the base address of the register stack	*/
	/* backing store.  This is computed in 			*/
	/* GC_linux_register_stack_base based on the following	*/
	/* constants:						*/
#       define BACKING_STORE_ALIGNMENT 0x100000
#       define BACKING_STORE_DISPLACEMENT 0x80000000
	extern ptr_t GC_register_stackbottom;
#	define BACKING_STORE_BASE GC_register_stackbottom
#	define SEARCH_FOR_DATA_START
#	ifdef __GNUC__
#         define DYNAMIC_LOADING
#	else
	  /* In the Intel compiler environment, we seem to end up with  */
	  /* statically linked executables and an undefined reference	*/
	  /* to _DYNAMIC						*/
#  	endif
#	define MPROTECT_VDB
		/* Requires Linux 2.3.47 or later.	*/
	extern int _end[];
#	define DATAEND (_end)
#       ifdef __GNUC__
#	  ifndef __INTEL_COMPILER
#	    define PREFETCH(x) \
	      __asm__ ("	lfetch	[%0]": : "r"(x))
#	    define PREFETCH_FOR_WRITE(x) \
	      __asm__ ("	lfetch.excl	[%0]": : "r"(x))
#	    define CLEAR_DOUBLE(x) \
	      __asm__ ("	stf.spill	[%0]=f0": : "r"((void *)(x)))
#	  else
#           include <ia64intrin.h>
#	    define PREFETCH(x) \
	      __lfetch(__lfhint_none, (x))
#	    define PREFETCH_FOR_WRITE(x) \
	      __lfetch(__lfhint_nta,  (x))
#	    define CLEAR_DOUBLE(x) \
	      __stf_spill((void *)(x), 0)
#	  endif // __INTEL_COMPILER
#       endif
#   endif
#   ifdef MSWIN32
      /* FIXME: This is a very partial guess.  There is no port, yet.	*/
#     define OS_TYPE "MSWIN32"
		/* STACKBOTTOM and DATASTART are handled specially in 	*/
		/* os_dep.c.						*/
#     define DATAEND  /* not needed */
#     if defined(_WIN64)
#       define CPP_WORDSZ 64
#     else
#       define CPP_WORDSZ 32   /* Is this possible?	*/
#     endif
#     define ALIGNMENT 8
#   endif
# endif

# ifdef M88K
#   define MACH_TYPE "M88K"
#   define ALIGNMENT 4
#   define ALIGN_DOUBLE
    extern int etext[];
#   ifdef CX_UX
#	define OS_TYPE "CX_UX"
#       define DATASTART ((((word)etext + 0x3fffff) & ~0x3fffff) + 0x10000)
#   endif
#   ifdef  DGUX
#	define OS_TYPE "DGUX"
	extern ptr_t GC_SysVGetDataStart();
#       define DATASTART GC_SysVGetDataStart(0x10000, etext)
#   endif
#   define STACKBOTTOM ((char*)0xf0000000) /* determined empirically */
# endif

# ifdef S370
    /* If this still works, and if anyone cares, this should probably	*/
    /* be moved to the S390 category.					*/
#   define MACH_TYPE "S370"
#   define ALIGNMENT 4	/* Required by hardware	*/
#   define USE_GENERIC_PUSH_REGS
#   ifdef UTS4
#       define OS_TYPE "UTS4"
	extern int etext[];
	extern int _etext[];
	extern int _end[];
	extern ptr_t GC_SysVGetDataStart();
#       define DATASTART GC_SysVGetDataStart(0x10000, _etext)
#	define DATAEND (_end)
#	define HEURISTIC2
#   endif
# endif

# ifdef S390
#   define MACH_TYPE "S390"
#   define USE_GENERIC_PUSH_REGS
#   ifndef __s390x__
#     define ALIGNMENT 4
#     define CPP_WORDSZ 32
#   else
#     define ALIGNMENT 8
#     define CPP_WORDSZ 64
#   endif
#   ifndef HBLKSIZE
#     define HBLKSIZE 4096
#   endif
#   ifdef LINUX
#       define OS_TYPE "LINUX"
#       define LINUX_STACKBOTTOM
#       define DYNAMIC_LOADING
	extern int __data_start[] __attribute__((weak));
#       define DATASTART ((ptr_t)(__data_start))
        extern int _end[] __attribute__((weak));
#       define DATAEND (_end)
#       define CACHE_LINE_SIZE 256
#       define GETPAGESIZE() 4096
#   endif
# endif

# if defined(PJ)
#   define ALIGNMENT 4
    extern int _etext[];
#   define DATASTART ((ptr_t)(_etext))
#   define HEURISTIC1
# endif

# ifdef ARM32
# if defined( NACL )
#   define MACH_TYPE "NACL"
# else
#   define MACH_TYPE "ARM32"
# endif
#   define CPP_WORDSZ 32
#   define ALIGNMENT 4
#   ifdef NETBSD
#       define OS_TYPE "NETBSD"
#       define HEURISTIC2
#	ifdef __ELF__
#          define DATASTART GC_data_start
#	   define DYNAMIC_LOADING
#	else
           extern char etext[];
#          define DATASTART ((ptr_t)(etext))
#	endif
#       define USE_GENERIC_PUSH_REGS
#   endif
#   ifdef LINUX
#       define OS_TYPE "LINUX"
#       define LINUX_STACKBOTTOM
#       undef STACK_GRAN
#       define STACK_GRAN 0x10000000
#       define USE_GENERIC_PUSH_REGS
#       ifdef __ELF__
#            define DYNAMIC_LOADING
#	     include <features.h>
#	     if defined(__GLIBC__) && __GLIBC__ >= 2
#		 define SEARCH_FOR_DATA_START
#	     elif defined(PLATFORM_ANDROID)
#		 define SEARCH_FOR_DATA_START
#	     else
     	         extern char **__environ;
#                define DATASTART ((ptr_t)(&__environ))
			      /* hideous kludge: __environ is the first */
			      /* word in crt0.o, and delimits the start */
			      /* of the data segment, no matter which   */
			      /* ld options were passed through.        */
			      /* We could use _etext instead, but that  */
			      /* would include .rodata, which may       */
			      /* contain large read-only data tables    */
			      /* that we'd rather not scan.		*/
#	     endif
	     extern int _end[];
#	     define DATAEND (_end)
#	else
	     extern int etext[];
#            define DATASTART ((ptr_t)((((word) (etext)) + 0xfff) & ~0xfff))
#       endif
#   endif
#   ifdef MSWINCE
#     define OS_TYPE "MSWINCE"
#     define DATAEND /* not needed */
#   endif
#   ifdef DARWIN
#     define OS_TYPE "DARWIN"
#     define DATASTART ((ptr_t) get_etext())
#     define DATAEND	((ptr_t) get_end())
#     define STACKBOTTOM ((ptr_t) 0x30000000)
#     define USE_MMAP
#     define USE_MMAP_ANON
#     define USE_MUNMAP
#   endif
#   ifdef NOSYS
      /* __data_start is usually defined in the target linker script.  */
      extern int __data_start[];
#     define DATASTART (ptr_t)(__data_start)
#     define USE_GENERIC_PUSH_REGS
      /* __stack_base__ is set in newlib/libc/sys/arm/crt0.S  */
      extern void *__stack_base__;
#     define STACKBOTTOM ((ptr_t) (__stack_base__))
#   endif
#endif

# ifdef CRIS
#   define MACH_TYPE "CRIS"
#   define CPP_WORDSZ 32
#   define ALIGNMENT 1
#   define OS_TYPE "LINUX"
#   define DYNAMIC_LOADING
#   define LINUX_STACKBOTTOM
#   define USE_GENERIC_PUSH_REGS
#   define SEARCH_FOR_DATA_START
      extern int _end[];
#   define DATAEND (_end)
# endif

# ifdef SH
#   define MACH_TYPE "SH"
#   define ALIGNMENT 4
#   ifdef MSWINCE
#     define OS_TYPE "MSWINCE"
#     define DATAEND /* not needed */
#   endif
#   ifdef LINUX
#     define OS_TYPE "LINUX"
#     define LINUX_STACKBOTTOM
#     define USE_GENERIC_PUSH_REGS
#     define DYNAMIC_LOADING
#     define SEARCH_FOR_DATA_START
      extern int _end[];
#     define DATAEND (_end)
#   endif
#   ifdef NETBSD
#      define OS_TYPE "NETBSD"
#      define HEURISTIC2
#      define DATASTART GC_data_start
#       define USE_GENERIC_PUSH_REGS
#      define DYNAMIC_LOADING
#   endif
# endif
 
# ifdef SH4
#   define MACH_TYPE "SH4"
#   define OS_TYPE "MSWINCE"
#   define ALIGNMENT 4
#   define DATAEND /* not needed */
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
#     define USE_GENERIC_PUSH_REGS
#     define DYNAMIC_LOADING
#     define SEARCH_FOR_DATA_START
      extern int _end[];
#     define DATAEND (_end)
#   endif
# endif

# ifdef X86_64
#   define MACH_TYPE "X86_64"
#   define ALIGNMENT 8
#   define CPP_WORDSZ 64
#   ifndef HBLKSIZE
#     define HBLKSIZE 4096
#   endif
#   define CACHE_LINE_SIZE 64
#   define USE_GENERIC_PUSH_REGS
#   ifdef LINUX
#	define OS_TYPE "LINUX"
#       define LINUX_STACKBOTTOM
#   ifndef USE_MMAP
#       define FALLBACK_TO_MMAP
#   endif
#       if !defined(GC_LINUX_THREADS) || !defined(REDIRECT_MALLOC)
#	    define MPROTECT_VDB
#	else
	    /* We seem to get random errors in incremental mode,	*/
	    /* possibly because Linux threads is itself a malloc client */
	    /* and can't deal with the signals.				*/
#	endif
#       ifdef __ELF__
#            define DYNAMIC_LOADING
#	     ifdef UNDEFINED	/* includes ro data */
	       extern int _etext[];
#              define DATASTART ((ptr_t)((((word) (_etext)) + 0xfff) & ~0xfff))
#	     endif
#	     include <features.h>
#	     define SEARCH_FOR_DATA_START
	     extern int _end[];
#	     define DATAEND (_end)
#	else
	     extern int etext[];
#            define DATASTART ((ptr_t)((((word) (etext)) + 0xfff) & ~0xfff))
#       endif
#       if defined(__GNUC__) && __GNUC >= 3
#	    define PREFETCH(x) __builtin_prefetch((x), 0, 0)
#	    define PREFETCH_FOR_WRITE(x) __builtin_prefetch((x), 1)
#	endif
#   endif
#   ifdef DARWIN
#     define OS_TYPE "DARWIN"
#     define DARWIN_DONT_PARSE_STACK
#     define DYNAMIC_LOADING
      /* XXX: see get_end(3), get_etext() and get_end() should not be used.
         These aren't used when dyld support is enabled (it is by default) */
#     define DATASTART ((ptr_t) get_etext())
#     define DATAEND    ((ptr_t) get_end())
#     define STACKBOTTOM ((ptr_t) 0x7fff5fc00000)
#     ifndef USE_MMAP
#       define USE_MMAP
#     endif
#     define USE_MMAP_ANON
#     ifdef GC_DARWIN_THREADS
#       define MPROTECT_VDB
#     endif
#     include <unistd.h>
#     define GETPAGESIZE() getpagesize()
      /* There seems to be some issues with trylock hanging on darwin. This
         should be looked into some more */
#   endif
#   ifdef FREEBSD
#	define OS_TYPE "FREEBSD"
#	ifndef GC_FREEBSD_THREADS
#	    define MPROTECT_VDB
#	endif
#      define SIG_SUSPEND SIGTSTP
#      define SIG_THR_RESTART SIGCONT
#	define NEED_FIND_LIMIT
#	define FREEBSD_STACKBOTTOM
#	ifdef __ELF__
#	    define DYNAMIC_LOADING
#	endif
	extern char etext[];
	extern char * GC_FreeBSDGetDataStart();
#	define DATASTART GC_FreeBSDGetDataStart(0x1000, &etext)
#   endif
#   ifdef NETBSD
#	define OS_TYPE "NETBSD"
#	ifdef __ELF__
#	    define DYNAMIC_LOADING
#	endif
#	define HEURISTIC2
	extern char etext[];
#	define SEARCH_FOR_DATA_START
#   endif
#   ifdef OPENBSD
#       define OS_TYPE "OPENBSD"
#       define ELF_CLASS ELFCLASS64
#    ifdef GC_OPENBSD_THREADS
#       define UTHREAD_SP_OFFSET 400
#    else
#       include <sys/param.h>
#       include <uvm/uvm_extern.h>
#       define STACKBOTTOM USRSTACK
#    endif
        extern int __data_start[];
#       define DATASTART ((ptr_t)(__data_start))
        extern char _end[];
#       define DATAEND ((ptr_t)(&_end))
#       define DYNAMIC_LOADING
#   endif
# endif

#if defined(LINUX) && defined(USE_MMAP)
    /* The kernel may do a somewhat better job merging mappings etc.	*/
    /* with anonymous mappings.						*/
#   define USE_MMAP_ANON
#endif

#if defined(LINUX) && defined(REDIRECT_MALLOC)
    /* Rld appears to allocate some memory with its own allocator, and	*/
    /* some through malloc, which might be redirected.  To make this	*/
    /* work with collectable memory, we have to scan memory allocated	*/
    /* by rld's internal malloc.					*/
#   define USE_PROC_FOR_LIBRARIES
#endif
    
# ifndef STACK_GROWS_UP
#   define STACK_GROWS_DOWN
# endif

# ifndef CPP_WORDSZ
#   define CPP_WORDSZ 32
# endif

# ifndef OS_TYPE
#   define OS_TYPE ""
# endif

# ifndef DATAEND
    extern int end[];
#   define DATAEND (end)
# endif

# if defined(SVR4) && !defined(GETPAGESIZE)
#    include <unistd.h>
#    define GETPAGESIZE()  sysconf(_SC_PAGESIZE)
# endif

# ifndef GETPAGESIZE
#   if defined(SUNOS5) || defined(IRIX5)
#	include <unistd.h>
#   endif
#   define GETPAGESIZE() getpagesize()
# endif

# if defined(SUNOS5) || defined(DRSNX) || defined(UTS4)
	    /* OS has SVR4 generic features.  Probably others also qualify.	*/
#   define SVR4
# endif

# if defined(SUNOS5) || defined(DRSNX)
	    /* OS has SUNOS5 style semi-undocumented interface to dynamic 	*/
	    /* loader.								*/
#   define SUNOS5DL
	    /* OS has SUNOS5 style signal handlers.				*/
#   define SUNOS5SIGS
# endif

# if defined(HPUX)
#   define SUNOS5SIGS
# endif

# if defined(FREEBSD) && (__FreeBSD__ >= 4)
#   define SUNOS5SIGS
# endif

# if defined(SVR4) || defined(LINUX) || defined(IRIX5) || defined(HPUX) \
	    || defined(OPENBSD) || defined(NETBSD) || defined(FREEBSD) \
	    || defined(DGUX) || defined(BSD) || defined(SUNOS4) \
	    || defined(_AIX) || defined(DARWIN) || defined(OSF1)
#   define UNIX_LIKE   /* Basic Unix-like system calls work.	*/
# endif

# if CPP_WORDSZ != 32 && CPP_WORDSZ != 64
	   -> bad word size
# endif

# ifdef PCR
#   undef DYNAMIC_LOADING
#   undef STACKBOTTOM
#   undef HEURISTIC1
#   undef HEURISTIC2
#   undef PROC_VDB
#   undef MPROTECT_VDB
#   define PCR_VDB
# endif

# ifdef SRC_M3
	/* Postponed for now. */
#   undef PROC_VDB
#   undef MPROTECT_VDB
# endif

# ifdef SMALL_CONFIG
	/* Presumably not worth the space it takes. */
#   undef PROC_VDB
#   undef MPROTECT_VDB
# endif

# ifdef USE_MUNMAP
#   undef MPROTECT_VDB  /* Can't deal with address space holes. */
# endif

# ifdef PARALLEL_MARK
#   undef MPROTECT_VDB  /* For now.	*/
# endif

# if !defined(PCR_VDB) && !defined(PROC_VDB) && !defined(MPROTECT_VDB)
#   define DEFAULT_VDB
# endif

# ifndef PREFETCH
#   define PREFETCH(x)
#   define NO_PREFETCH
# endif

# ifndef PREFETCH_FOR_WRITE
#   define PREFETCH_FOR_WRITE(x)
#   define NO_PREFETCH_FOR_WRITE
# endif

# ifndef CACHE_LINE_SIZE
#   define CACHE_LINE_SIZE 32	/* Wild guess	*/
# endif

# ifdef LINUX
#   define REGISTER_LIBRARIES_EARLY
    /* We sometimes use dl_iterate_phdr, which may acquire an internal	*/
    /* lock.  This isn't safe after the world has stopped.  So we must	*/
    /* call GC_register_dynamic_libraries before stopping the world.	*/
    /* For performance reasons, this may be beneficial on other		*/
    /* platforms as well, though it should be avoided in win32.		*/
# endif /* LINUX */

# if defined(SEARCH_FOR_DATA_START)
    extern ptr_t GC_data_start;
#   define DATASTART GC_data_start
# endif

# ifndef CLEAR_DOUBLE
#   define CLEAR_DOUBLE(x) \
		((word*)x)[0] = 0; \
		((word*)x)[1] = 0;
# endif /* CLEAR_DOUBLE */

	/* Internally we use GC_SOLARIS_THREADS to test for either old or pthreads. */
# if defined(GC_SOLARIS_PTHREADS) && !defined(GC_SOLARIS_THREADS)
#   define GC_SOLARIS_THREADS
# endif

# if defined(GC_IRIX_THREADS) && !defined(IRIX5)
	--> inconsistent configuration
# endif
# if defined(GC_LINUX_THREADS) && !(defined(LINUX) || defined(NACL))
	--> inconsistent configuration
# endif
# if defined(GC_SOLARIS_THREADS) && !defined(SUNOS5)
	--> inconsistent configuration
# endif
# if defined(GC_HPUX_THREADS) && !defined(HPUX)
	--> inconsistent configuration
# endif
# if defined(GC_AIX_THREADS) && !defined(_AIX)
	--> inconsistent configuration
# endif
# if defined(GC_WIN32_THREADS) && !defined(MSWIN32) && !defined(CYGWIN32)
	--> inconsistent configuration
# endif

# if defined(PCR) || defined(SRC_M3) || \
		defined(GC_SOLARIS_THREADS) || defined(GC_WIN32_THREADS) || \
		defined(GC_PTHREADS) || defined(SN_TARGET_PS3)
#   define THREADS
# endif

# if defined(HP_PA) || defined(M88K) \
             || defined(POWERPC) && !defined(DARWIN) \
	     || defined(LINT) || defined(MSWINCE) || defined(ARM32) || defined(CRIS) \
	     || (defined(I386) && defined(__LCC__))
	/* Use setjmp based hack to mark from callee-save registers.    */
	/* The define should move to the individual platform 		*/
	/* descriptions.						*/
#	define USE_GENERIC_PUSH_REGS
# endif

# if defined(MSWINCE)
#   define NO_GETENV
# endif

# if defined(SPARC)
#   define ASM_CLEAR_CODE	/* Stack clearing is crucial, and we 	*/
				/* include assembly code to do it well.	*/
# endif

  /* Can we save call chain in objects for debugging?   	        */
  /* SET NFRAMES (# of saved frames) and NARGS (#of args for each 	*/
  /* frame) to reasonable values for the platform.			*/
  /* Set SAVE_CALL_CHAIN if we can.  SAVE_CALL_COUNT can be specified 	*/
  /* at build time, though we feel free to adjust it slightly.		*/
  /* Define NEED_CALLINFO if we either save the call stack or 		*/
  /* GC_ADD_CALLER is defined.						*/
  /* GC_CAN_SAVE_CALL_STACKS is set in gc.h.				*/

#if defined(SPARC)
# define CAN_SAVE_CALL_ARGS
#endif
#if (defined(I386) || defined(X86_64)) && defined(LINUX)
	    /* SAVE_CALL_CHAIN is supported if the code is compiled to save	*/
	    /* frame pointers by default, i.e. no -fomit-frame-pointer flag.	*/
# define CAN_SAVE_CALL_ARGS
#endif

# if defined(SAVE_CALL_COUNT) && !defined(GC_ADD_CALLER) \
	     && defined(GC_CAN_SAVE_CALL_STACKS)
#   define SAVE_CALL_CHAIN 
# endif
# ifdef SAVE_CALL_CHAIN
#   if defined(SAVE_CALL_NARGS) && defined(CAN_SAVE_CALL_ARGS)
#     define NARGS SAVE_CALL_NARGS
#   else
#     define NARGS 0	/* Number of arguments to save for each call.	*/
#   endif
# endif
# ifdef SAVE_CALL_CHAIN
#   ifndef SAVE_CALL_COUNT
#     define NFRAMES 6	/* Number of frames to save. Even for		*/
				/* alignment reasons.				*/
#   else
#     define NFRAMES ((SAVE_CALL_COUNT + 1) & ~1)
#   endif
#   define NEED_CALLINFO
# endif /* SAVE_CALL_CHAIN */
# ifdef GC_ADD_CALLER
#   define NFRAMES 1
#   define NARGS 0
#   define NEED_CALLINFO
# endif

# if defined(MAKE_BACK_GRAPH) && !defined(DBG_HDRS_ALL)
#   define DBG_HDRS_ALL
# endif

# if defined(POINTER_MASK) && !defined(POINTER_SHIFT)
#   define POINTER_SHIFT 0
# endif

# if defined(POINTER_SHIFT) && !defined(POINTER_MASK)
#   define POINTER_MASK ((GC_word)(-1))
# endif

# if !defined(FIXUP_POINTER) && defined(POINTER_MASK)
#   define FIXUP_POINTER(p) (p) = ((p) & (POINTER_MASK) << POINTER_SHIFT)
# endif

# if defined(FIXUP_POINTER)
#   define NEED_FIXUP_POINTER 1
# else
#   define NEED_FIXUP_POINTER 0
#   define FIXUP_POINTER(p)
# endif

#ifdef GC_PRIVATE_H
	/* This relies on some type definitions from gc_priv.h, from	*/
        /* where it's normally included.				*/
	/*								*/
	/* How to get heap memory from the OS:				*/
	/* Note that sbrk()-like allocation is preferred, since it 	*/
	/* usually makes it possible to merge consecutively allocated	*/
	/* chunks.  It also avoids unintented recursion with		*/
	/* -DREDIRECT_MALLOC.						*/
	/* GET_MEM() returns a HLKSIZE aligned chunk.			*/
	/* 0 is taken to mean failure. 					*/
	/* In the case os USE_MMAP, the argument must also be a 	*/
	/* physical page size.						*/
	/* GET_MEM is currently not assumed to retrieve 0 filled space, */
	/* though we should perhaps take advantage of the case in which */
	/* does.							*/
	struct hblk;	/* See gc_priv.h.	*/
# ifdef PCR
	    char * real_malloc();
#   define GET_MEM(bytes) HBLKPTR(real_malloc((size_t)bytes + GC_page_size) \
					  + GC_page_size-1)
# else
#   ifdef OS2
	      void * os2_alloc(size_t bytes);
#     define GET_MEM(bytes) HBLKPTR((ptr_t)os2_alloc((size_t)bytes \
					    + GC_page_size) \
					    + GC_page_size-1)
#   else
#     if defined(NEXT) || defined(DOS4GW) || \
		 (defined(AMIGA) && !defined(GC_AMIGA_FASTALLOC)) || \
		 (defined(SUNOS5) && !defined(USE_MMAP))
#       define GET_MEM(bytes) HBLKPTR((size_t) \
					      calloc(1, (size_t)bytes + GC_page_size) \
					      + GC_page_size-1)
#     else
#	ifdef MSWIN32
	  extern ptr_t GC_win32_get_mem();
#         define GET_MEM(bytes) (struct hblk *)GC_win32_get_mem(bytes)
#	else
#	  ifdef MACOS
#	    if defined(USE_TEMPORARY_MEMORY)
			extern Ptr GC_MacTemporaryNewPtr(size_t size,
							 Boolean clearMemory);
#               define GET_MEM(bytes) HBLKPTR( \
			    GC_MacTemporaryNewPtr(bytes + GC_page_size, true) \
			    + GC_page_size-1)
#	    else
#         	    define GET_MEM(bytes) HBLKPTR( \
				NewPtrClear(bytes + GC_page_size) + GC_page_size-1)
#	    endif
#	  else
#	    ifdef MSWINCE
	      extern ptr_t GC_wince_get_mem();
#	      define GET_MEM(bytes) (struct hblk *)GC_wince_get_mem(bytes)
#	    else
#	      if defined(AMIGA) && defined(GC_AMIGA_FASTALLOC)
			extern void *GC_amiga_get_mem(size_t size);
#		define GET_MEM(bytes) HBLKPTR((size_t) \
			  GC_amiga_get_mem((size_t)bytes + GC_page_size) \
			  + GC_page_size-1)
#	      else
#           if defined(SN_TARGET_PS3)
	           extern void *ps3_get_mem (size_t size);
#              define GET_MEM(bytes) (struct hblk*) ps3_get_mem (bytes)
#           else
		extern ptr_t GC_unix_get_mem(word size);
#               define GET_MEM(bytes) (struct hblk *)GC_unix_get_mem(bytes)
#endif
#	      endif
#	    endif
#	  endif
#	endif
#     endif
#   endif
# endif

#endif /* GC_PRIVATE_H */

#if defined(_AIX) && !defined(__GNUC__) && !defined(__STDC__)
  /* IBMs xlc compiler doesn't appear to follow the convention of	*/
  /* defining  __STDC__ to be zero in extended mode.			*/
#   define __STDC__ 0
#endif

# endif /* GCCONFIG_H */
