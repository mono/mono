# Copyright (c) 1999-2001 by Red Hat, Inc. All rights reserved.
# 
# THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
# OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
# 
# Permission is hereby granted to use or copy this program
# for any purpose,  provided the above notices are retained on all copies.
# Permission to modify the code and to distribute modified code is granted,
# provided the above notices are retained, and a notice that the code was
# modified is included with the above copyright notice.
#
# Original author: Tom Tromey
# Modified by: Grzegorz Jakacki <jakacki at acm dot org>

dnl Process this file with autoconf to produce configure.

AC_PREREQ([2.53])
AC_INIT([libgc-mono], [6.6], [Hans_Boehm@hp.com])

AM_INIT_AUTOMAKE([1.9 no-define foreign])
AC_CONFIG_SRCDIR(gcj_mlc.c)
AC_CONFIG_MACRO_DIR([m4])
AC_CANONICAL_HOST

AC_SUBST(PACKAGE)
AC_SUBST(GC_VERSION)

AC_PROG_CC
AC_PROG_CXX

m4_ifdef([AM_SILENT_RULES], [AM_SILENT_RULES([yes])])

dnl automake 1.6 and later need the AM_PROG_AS macro.
ifdef([AM_PROG_AS],[AM_PROG_AS],[])
AC_CHECK_TOOL(AR, ar)
AC_CHECK_TOOL(RANLIB, ranlib, :)  # :)

AC_PROG_INSTALL

AM_MAINTAINER_MODE

. [$]{srcdir}/configure.host

# We use a separate variable to pass down CPPFLAGS and CFLAGS from the main mono 
# configure, because of autoconf brokeness
if test "x$CPPFLAGS_FOR_LIBGC" != "x"; then
   CPPFLAGS=$CPPFLAGS_FOR_LIBGC
fi
if test "x$CFLAGS_FOR_LIBGC" != "x"; then
   CFLAGS=$CFLAGS_FOR_LIBGC
fi

GC_CFLAGS=${gc_cflags}
AC_SUBST(GC_CFLAGS)

case $enable_embed_check in
no) ;;
*) AC_MSG_ERROR([This module is now part of 'mono' and cannot be built as a stand-alone module any longer.]) ;;
esac

THREADS=$with_libgc_threads

AC_ARG_ENABLE(win32-dllmain,
[  --enable-win32-dllmain    Define the DllMain function in win32_threads.c even if the collector is not built as a dll],
)

AC_ARG_ENABLE(parallel-mark,
[  --enable-parallel-mark	parallelize marking and free list construction],
   [case "$THREADS" in
      no | none | single)
	AC_MSG_ERROR([Parallel mark requires --enable-threads=x spec])
	;;
    esac]
)

AC_ARG_ENABLE(cplusplus,
[  --enable-cplusplus		install C++ support],
)

AM_CPPFLAGS=-I${srcdir}/include
THREADDLLIBS=
## Libraries needed to support dynamic loading and/or threads.
case "$THREADS" in
 no | none | single)
    THREADS=none
    ;;
 posix | pthreads)
    THREADS=posix
    AC_CHECK_LIB(pthread, pthread_self, THREADDLLIBS="-lpthread",,)
    case "$host" in
     x86-*-linux* | ia64-*-linux* | i386-*-linux* | i486-*-linux* | i586-*-linux* | i686-*-linux* | x86_64-*-linux* | alpha*-*-linux* | s390*-*-linux* | sparc*-*-linux* | powerpc-*-linux*)
	AC_DEFINE(GC_LINUX_THREADS)
	AC_DEFINE(_REENTRANT)
        if test "${enable_parallel_mark}" = yes; then
	  AC_DEFINE(PARALLEL_MARK)
	fi
	AC_DEFINE(THREAD_LOCAL_ALLOC)
	;;
     *-*-linux* | *-*-nacl*)
	AC_DEFINE(GC_LINUX_THREADS)
	AC_DEFINE(_REENTRANT)
	;;
     *-*-aix*)
	AC_DEFINE(GC_AIX_THREADS)
	AC_DEFINE(_REENTRANT)
	;;
     *-*-hpux*)
	AC_MSG_WARN("Only HP/UX 11 threads are supported.")
	AC_DEFINE(GC_HPUX_THREADS)
	AC_DEFINE(_POSIX_C_SOURCE,199506L)
	if test "${enable_parallel_mark}" = yes; then
	  AC_DEFINE(PARALLEL_MARK)
	fi
	AC_DEFINE(THREAD_LOCAL_ALLOC)
	THREADDLLIBS="-lpthread -lrt"
	;;
      *-*-freebsd*)
	AC_DEFINE(GC_FREEBSD_THREADS)
	if test "x$PTHREAD_CFLAGS" != "x"; then
		AM_CPPFLAGS="$AM_CPPFLAGS $PTHREAD_CFLAGS"
	fi
	if test "x$PTHREAD_LIBS" = "x"; then
		THREADDLLIBS=-pthread
	else
		THREADDLLIBS="$PTHREAD_LIBS"
	fi
	AC_DEFINE(THREAD_LOCAL_ALLOC)
	;;
     *-*-solaris*)
	AC_DEFINE(GC_SOLARIS_THREADS)
	AC_DEFINE(GC_SOLARIS_PTHREADS)
	;;
     *-*-irix*)
	AC_DEFINE(GC_IRIX_THREADS)
	;;
     *-*-cygwin*)
	AC_DEFINE(GC_WIN32_THREADS)
	;;
     *-*-darwin*)
	AC_DEFINE(GC_DARWIN_THREADS)
	AC_DEFINE(THREAD_LOCAL_ALLOC)
	if test "${enable_parallel_mark}" = yes; then
	  AC_DEFINE(PARALLEL_MARK)
	fi
	;;
     *-*-netbsd*)
	AC_DEFINE(GC_NETBSD_THREADS)
	if test "${enable_parallel_mark}" = yes; then
	  AC_DEFINE(PARALLEL_MARK)
	fi
	AC_DEFINE(THREAD_LOCAL_ALLOC)
	;;
     *-*-openbsd*)
	AC_DEFINE(GC_OPENBSD_THREADS)
	if test "${enable_parallel_mark}" = yes; then
	  AC_DEFINE(PARALLEL_MARK)
	fi
	AC_DEFINE(THREAD_LOCAL_ALLOC)
	;;
     *-*-osf*)
	AC_DEFINE(GC_OSF1_THREADS)
        if test "${enable_parallel_mark}" = yes; then
	  AC_DEFINE(PARALLEL_MARK)
	  AC_DEFINE(THREAD_LOCAL_ALLOC)
	  # May want to enable it in other cases, too.
	  # Measurements havent yet been done.
	fi
	AM_CPPFLAGS="$AM_CPPFLAGS -pthread"
	THREADDLLIBS="-lpthread -lrt"
	;;
      *)
	AC_MSG_ERROR("Pthreads not supported by the GC on this platform.")
	;;
    esac
    ;;
 win32)
    AC_DEFINE(GC_WIN32_THREADS)
    dnl Wine getenv may not return NULL for missing entry
    AC_DEFINE(NO_GETENV)
	if test "${enable_win32_dllmain}" = yes; then
	   AC_DEFINE(GC_INSIDE_DLL)
	fi
    ;;
 dgux386)
    THREADS=dgux386
    AC_MSG_RESULT($THREADDLLIBS)
    # Use pthread GCC  switch
    THREADDLLIBS=-pthread
    if test "${enable_parallel_mark}" = yes; then
        AC_DEFINE(PARALLEL_MARK)
    fi
    AC_DEFINE(THREAD_LOCAL_ALLOC)
    AC_DEFINE(GC_DGUX386_THREADS)
    AC_DEFINE(DGUX_THREADS)
    # Enable _POSIX4A_DRAFT10_SOURCE with flag -pthread
    AM_CPPFLAGS="-pthread $AM_CPPFLAGS"
    ;;
 aix)
    THREADS=posix
    THREADDLLIBS=-lpthread
    AC_DEFINE(GC_AIX_THREADS)
    AC_DEFINE(_REENTRANT)
    ;;
 decosf1 | irix | mach | os2 | solaris | dce | vxworks)
    AC_MSG_ERROR(thread package $THREADS not yet supported)
    ;;
 *)
    AC_MSG_ERROR($THREADS is an unknown thread package)
    ;;
esac
AC_SUBST(THREADDLLIBS)

case "$host" in 
   powerpc-*-darwin*)
      powerpc_darwin=true
      ;;
esac
AM_CONDITIONAL(POWERPC_DARWIN,test x$powerpc_darwin = xtrue)

# Check if the GCC builtin __sync_bool_compare_and_swap is available.
# It is preferred in gc_locks.h for PPC as GCC 4.4 has a problem with the inline assembly there.
AC_MSG_CHECKING(for __sync_bool_compare_and_swap)
AC_TRY_COMPILE([],[
volatile unsigned int foo = 0;
int main(int argc, char** argv) {
    unsigned int r1 = __sync_bool_compare_and_swap(&foo, 0, 1);
    return 0;
}
], [
AC_MSG_RESULT(yes)
AC_DEFINE(HAS___SYNC_BOOL_COMPARE_AND_SWAP)
], [
AC_MSG_RESULT(no)
])

AC_MSG_CHECKING(for xlc)
AC_TRY_COMPILE([],[
 #ifndef __xlC__
 # error
 #endif
], [compiler_xlc=yes], [compiler_xlc=no])
AC_MSG_RESULT($compiler_xlc)
AM_CONDITIONAL(COMPILER_XLC,test $compiler_xlc = yes)
if test $compiler_xlc = yes -a "$powerpc_darwin" = true; then
  # the darwin stack-frame-walking code is completely broken on xlc
  AC_DEFINE(DARWIN_DONT_PARSE_STACK)
fi

# We never want libdl on darwin. It is a fake libdl that just ends up making
# dyld calls anyway
case "$host" in
  *-*-darwin*) ;;
  *) 
    AC_CHECK_LIB(dl, dlopen, THREADDLLIBS="$THREADDLLIBS -ldl")
    ;;
esac

AC_SUBST(EXTRA_TEST_LIBS)

target_all=libgc.la
AC_SUBST(target_all)

dnl If the target is an eCos system, use the appropriate eCos
dnl I/O routines.
dnl FIXME: this should not be a local option but a global target
dnl system; at present there is no eCos target.
TARGET_ECOS="no"
AC_ARG_WITH(ecos,
[  --with-ecos             enable runtime eCos target support],
TARGET_ECOS="$with_ecos"
)

addobjs=
addlibs=
addincludes=
addtests=
CXXAM_CPPFLAGS=
case "$TARGET_ECOS" in
   no)
      ;;
   *)
      AC_DEFINE(ECOS)
      CXXINCLUDES="-I${TARGET_ECOS}/include"
      addobjs="$addobjs ecos.lo"
      ;;
esac

if test "${enable_cplusplus}" = yes; then
      addincludes="$addincludes include/gc_cpp.h include/gc_allocator.h"
      addtests="$addtests test_cpp"
fi

AM_CONDITIONAL(CPLUSPLUS, test "${enable_cplusplus}" = yes)

AC_SUBST(CXX)

AC_SUBST(AM_CPPFLAGS)
AC_SUBST(CXXINCLUDES)

# Configuration of shared libraries
#
AC_MSG_CHECKING(whether to build shared libraries)
AC_ENABLE_SHARED

case "$host" in
 alpha-*-openbsd*)
     enable_shared=no
     AC_MSG_RESULT(no)
     ;;
 *)
     AC_MSG_RESULT(yes)
     ;;
esac

# Configuration of machine-dependent code
#
AC_MSG_CHECKING(which machine-dependent code should be used) 
machdep=
case "$host" in
 alpha-*-openbsd*)
    machdep="alpha_mach_dep.lo"
    if test x"${ac_cv_lib_dl_dlopen}" != xyes ; then
       AC_MSG_WARN(OpenBSD/Alpha without dlopen(). Shared library support is disabled)
    fi
    ;;
 alpha*-*-linux*)
    machdep="alpha_mach_dep.lo"
    ;;
 i?86-*-solaris2.[[89]] | i?86-*-solaris2.1?)
    AC_DEFINE(SOLARIS25_PROC_VDB_BUG_FIXED)
    ;;
 mipstx39-*-elf*)
    machdep="mips_ultrix_mach_dep.lo"
    AC_DEFINE(STACKBASE, __stackbase)
    AC_DEFINE(DATASTART_IS_ETEXT)
    ;;
 mips-dec-ultrix*)
    machdep="mips_ultrix_mach-dep.lo"
    ;;
 mips-nec-sysv*|mips-unknown-sysv*)
    ;;
 mips*-*-linux*) 
    ;; 
 mips-*-*)
    machdep="mips_sgi_mach_dep.lo"
    AC_DEFINE(NO_EXECUTE_PERMISSION)
    ;;
 sparc-*-netbsd*)
    machdep="sparc_netbsd_mach_dep.lo"
    ;;
 sparc-sun-solaris2.3)
    machdep="sparc_mach_dep.lo"
    AC_DEFINE(SUNOS53_SHARED_LIB)
    ;;
 sparc*-sun-solaris2.*)
    machdep="sparc_mach_dep.lo"
    ;;
 ia64-*-*)
     machdep="mach_dep.lo ia64_save_regs_in_stack.lo"
	target_ia64=true
    ;;
 *-*-nacl*)
    AC_DEFINE(NO_EXECUTE_PERMISSION)
    ;;
esac
if test x"$machdep" = x; then
AC_MSG_RESULT($machdep)
   machdep="mach_dep.lo"
fi
addobjs="$addobjs $machdep"
#
# Adding object files directly to _LIBADD breaks -j builds, so we need to add the sources
# instead, but _SOURCES can't contain autoconf substitutions, so add them using automake
# conditionals.
#
#AC_SUBST(addobjs)
AC_SUBST(addincludes)
AC_SUBST(addlibs)
AC_SUBST(addtests)

AM_CONDITIONAL(TARGET_IA64,test x$target_ia64 = xtrue)

AC_PROG_LIBTOOL
# Use dolt (http://dolt.freedesktop.org/) instead of libtool for building.
DOLT

#
# Check for AViiON Machines running DGUX
#
ac_is_dgux=no
AC_CHECK_HEADER(sys/dg_sys_info.h,
[ac_is_dgux=yes;])

    ## :GOTCHA: we do not check anything but sys/dg_sys_info.h
if test $ac_is_dgux = yes; then
    if test "$enable_full_debug" = "yes"; then
      CFLAGS="-g -mstandard -DDGUX -D_DGUX_SOURCE -Di386 -mno-legend -O2"
      CXXFLAGS="-g -mstandard -DDGUX -D_DGUX_SOURCE -Di386 -mno-legend -O2"
    else
      CFLAGS="-DDGUX -D_DGUX_SOURCE -Di386 -mno-legend -O2"
      CXXFLAGS="-DDGUX -D_DGUX_SOURCE -Di386 -mno-legend -O2"
    fi
    AC_SUBST(CFLAGS)
    AC_SUBST(CXXFLAGS)
fi

dnl We use these options to decide which functions to include.
AC_ARG_WITH(target-subdir,
[  --with-target-subdir=SUBDIR
                          configuring with a cross compiler])
AC_ARG_WITH(cross-host,
[  --with-cross-host=HOST  configuring with a cross compiler])

# automake wants to see AC_EXEEXT.  But we don't need it.  And having
# it is actually a problem, because the compiler we're passed can't
# necessarily do a full link.  So we fool automake here.
if false; then
  # autoconf 2.50 runs AC_EXEEXT by default, and the macro expands
  # to nothing, so nothing would remain between `then' and `fi' if it
  # were not for the `:' below.
  :
  AC_EXEEXT
fi

AC_MSG_CHECKING([for threads package to use])
AC_MSG_RESULT($THREADS)

dnl As of 4.13a2, the collector will not properly work on Solaris when
dnl built with gcc and -O.  So we remove -O in the appropriate case.
dnl Not needed anymore on Solaris.
AC_MSG_CHECKING(whether Solaris gcc optimization fix is necessary)
case "$host" in
 *aix*)
    if test "$GCC" = yes; then
       AC_MSG_RESULT(yes)
       new_CFLAGS=
       for i in $CFLAGS; do
	  case "$i" in
	   -O*)
	      ;;
	   *)
	      new_CFLAGS="$new_CFLAGS $i"
	      ;;
	  esac
       done
       CFLAGS="$new_CFLAGS"
    else
       AC_MSG_RESULT(no)
    fi
    ;;
 *) AC_MSG_RESULT(no) ;;
esac

dnl We need to override the top-level CFLAGS.  This is how we do it.
MY_CFLAGS="$CFLAGS"
AC_SUBST(MY_CFLAGS)

dnl Include defines that have become de facto standard.
dnl ALL_INTERIOR_POINTERS can be overridden in startup code.
AC_DEFINE(SILENT)
AC_DEFINE(NO_SIGNALS)
AC_DEFINE(NO_EXECUTE_PERMISSION)
dnl AC_DEFINE(ALL_INTERIOR_POINTERS)

dnl By default, make the library as general as possible.
AC_DEFINE(JAVA_FINALIZATION)
AC_DEFINE(GC_GCJ_SUPPORT)
AC_DEFINE(ATOMIC_UNCOLLECTABLE)

dnl This is something of a hack.  When cross-compiling we turn off
dnl some functionality.  We also enable the "small" configuration.
dnl These is only correct when targetting an embedded system.  FIXME.
if test -n "${with_cross_host}"; then
   AC_DEFINE(NO_SIGSET)
   AC_DEFINE(NO_CLOCK)
   AC_DEFINE(SMALL_CONFIG)
   AC_DEFINE(NO_DEBUGGING)
fi

UNWINDLIBS=
AC_ARG_ENABLE(full-debug,
[  --enable-full-debug	include full support for pointer backtracing etc.],
[ if test "$enable_full_debug" = "yes"; then
    AC_MSG_WARN("Should define GC_DEBUG and use debug alloc. in clients.")
    AC_DEFINE(KEEP_BACK_PTRS)
    AC_DEFINE(DBG_HDRS_ALL)
    case $host in
      ia64-*-linux* )
	AC_DEFINE(MAKE_BACK_GRAPH)
	AC_DEFINE(SAVE_CALL_COUNT, 8)
        AC_CHECK_LIB(unwind, backtrace, [
	  AC_DEFINE(GC_HAVE_BUILTIN_BACKTRACE)
	  UNWINDLIBS=-lunwind
	  AC_MSG_WARN("Client code may need to link against libunwind.")
	])
      ;;
      x86-*-linux* | i586-*-linux* | i686-*-linux* | x86_64-*-linux* )
	AC_DEFINE(MAKE_BACK_GRAPH)
	AC_MSG_WARN("Client must not use -fomit-frame-pointer.")
	AC_DEFINE(SAVE_CALL_COUNT, 8)
      ;;
      i[3456]86-*-dgux*)
	AC_DEFINE(MAKE_BACK_GRAPH)
      ;;
    esac ]
  fi)

AC_SUBST(UNWINDLIBS)

AC_ARG_ENABLE(redirect-malloc,
[  --enable-redirect-malloc  redirect malloc and friends to GC routines])

if test "${enable_redirect_malloc}" = yes; then
    if test "${enable_full_debug}" = yes; then
	AC_DEFINE(REDIRECT_MALLOC, GC_debug_malloc_replacement)
	AC_DEFINE(REDIRECT_REALLOC, GC_debug_realloc_replacement)
	AC_DEFINE(REDIRECT_FREE, GC_debug_free)
    else
	AC_DEFINE(REDIRECT_MALLOC, GC_malloc)
    fi
fi

AC_DEFINE(_IN_LIBGC)

AC_ARG_ENABLE(gc-assertions,
[  --enable-gc-assertions  collector-internal assertion checking])
if test "${enable_gc_assertions}" = yes; then
    AC_DEFINE(GC_ASSERTIONS)
fi

AC_ARG_ENABLE(quiet-build, [  --enable-quiet-build  Enable quiet libgc build (on by default)], enable_quiet_build=$enableval, enable_quiet_build=yes)
AM_CONDITIONAL(USE_LIBDIR, test -z "$with_cross_host")

if test "${multilib}" = "yes"; then
  multilib_arg="--enable-multilib"
else
  multilib_arg=
fi

AC_OUTPUT(Makefile
m4/Makefile
include/Makefile
include/private/Makefile
doc/Makefile,,
srcdir=${srcdir}
host=${host}
CONFIG_SHELL=${CONFIG_SHELL-/bin/sh}
CC="${CC}"
DEFS="$DEFS"
)
