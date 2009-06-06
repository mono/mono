# based on configure.in/Makefile.am, which is:

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

# This is in sync with the configure.in/Makefile.am at r135583

#
# This is a cmake build file for libgc
# Only the functionality needed by mono is supported
#

# FIXME:
add_definitions("-g -O2")

set(THREADS ${libgc_threads})

set(THREADDLLIBS "")

if (THREADS STREQUAL "no" OR THREADS STREQUAL "none" OR THREADS STREQUAL "single")
  set(THREADS none)
endif()

if (THREADS STREQUAL "posix" OR THREADS STREQUAL "pthreads")
  set(THREADS posix)
  set(THREADDLLIBS "-lpthread")
  if (${host} MATCHES "x86-.*-linux.*|ia64-.*-linux.*|i386-.*-linux.*|i486-.*-linux.*|i586-.*-linux.*|i686-.*-linux.*|x86_64-.*-linux.*|alpha.*-.*-linux.*|s390.*-.*-linux.*|sparc.*-.*-linux.*|powerpc-.*-linux.*")
	add_definitions(-DGC_LINUX_THREADS -D_REENTRANT)
	set(parallel_mark_supported yes)
	add_definitions(-DTHREAD_LOCAL_ALLOC)
  elseif(${host} MATCHES ".*-linux.*")
	add_definitions(-DGC_LINUX_THREADS -D_REENTRANT)
  elseif(${host} MATCHES ".*-aix.*")
	add_definitions(-DGC_AIX_THREADS -D_REENTRANT)
  elseif(${host} MATCHES ".*-hpux.*")
	message("Only HP/UX 11 threads are supported.")
	add_definitions(-DGC_HPUX_THREADS -D_REENTRANT -D_POSIX_C_SOURCE=199506L)
	add_definitions(-DTHREAD_LOCAL_ALLOC)
	set(parallel_mark_supported yes)
	set(THREADDLLIBS "-lpthread -lrt")
  elseif(${host} ".*-.*-freebsd.*")
	add_definitions(-DGC_FREEBSD_THREADS)
	message(FATAL_ERROR "todo")
###	if test "x$PTHREAD_CFLAGS" != "x"; then
###		INCLUDES="$INCLUDES $PTHREAD_CFLAGS"
###	fi
###	if test "x$PTHREAD_LIBS" = "x"; then
###		THREADDLLIBS=-pthread
###	else
###		THREADDLLIBS="$PTHREAD_LIBS"
###	fi
  elseif(${host} ".*-.*-freebsd.*")
	add_definitions(-DGC_SOLARIS_THREADS -DGC_SOLARIS_PTHREADS)
  elseif(${host} ".*-.*-irix.*")
	add_definitions(-DGC_IRIX_THREADS)
  elseif(${host} ".*-.*-cygwin.*")
	add_definitons(-DGC_WIN32_THREADS)
  elseif(${host} ".*-.*-darwin.*")
	add_definitons(-DGC_DARWIN_THREADS -DTHREAD_LOCAL_ALLOC)
	set(parallel_mark_supported yes)
  elseif(${host} ".*-.*-netbsd.*")
	add_definitions(-DGC_NETBSD_THREADS -DTHREAD_LOCAL_ALLOC)
	set(parallel_mark_supported yes)
  elseif(${host} ".*-.*-osf.*")
	add_definitions(GC_OSF1_THREADS -DTHREAD_LOCAL_ALLOC)
	set(parallel_mark_supported yes)
	add_definitions(-pthread)
	set(THREADDLLIBS "-lpthread -lrt")
  else()
	message(FATAL_ERROR "Pthreads not supported by the GC on this platform.")
  endif()
endif()

if(THREADS STREQUAL "win32")
  add_definitions(GC_WIN32_THREADS)
  # Wine getenv may not return NULL for missing entry
  add_definitions(NO_GETENV)
  if (enable_win32_dllmain)
	add_definitions(gc_inside_dll)
  endif()
endif()

if(${host} MATCHES "powerpc-.*-darwin.*")
  set(powerpc_darwin yes)
endif()

if(parallel_mark_supported AND enable_parallel_mark)
  add_definitions("-DPARALLEL_MARK")
endif()

# Configuration of machine-dependent code

ac_msg_checking("which machine-dependent code should be used")

###AC_MSG_CHECKING(
###machdep=
###case "$host" in
### alpha-*-openbsd*)
###    machdep="alpha_mach_dep.lo"
###    if test x"${ac_cv_lib_dl_dlopen}" != xyes ; then
###       AC_MSG_WARN(OpenBSD/Alpha without dlopen(). Shared library support is disabled)
###    fi
###    ;;
### alpha*-*-linux*)
###    machdep="alpha_mach_dep.lo"
###    ;;
### i?86-*-solaris2.[[89]]|i?86-*-solaris2.1?)
###    AC_DEFINE(SOLARIS25_PROC_VDB_BUG_FIXED)
###    ;;
### mipstx39-*-elf*)
###    machdep="mips_ultrix_mach_dep.lo"
###    AC_DEFINE(STACKBASE, __stackbase)
###    AC_DEFINE(DATASTART_IS_ETEXT)
###    ;;
### mips-dec-ultrix*)
###    machdep="mips_ultrix_mach-dep.lo"
###    ;;
### mips-nec-sysv*|mips-unknown-sysv*)
###    ;;
### mips*-*-linux*) 
###    ;; 
### mips-*-*)
###    machdep="mips_sgi_mach_dep.lo"
###    AC_DEFINE(NO_EXECUTE_PERMISSION)
###    ;;
### sparc-*-netbsd*)
###    machdep="sparc_netbsd_mach_dep.lo"
###    ;;
### sparc-sun-solaris2.3)
###    machdep="sparc_mach_dep.lo"
###    AC_DEFINE(SUNOS53_SHARED_LIB)
###    ;;
### sparc*-sun-solaris2.*)
###    machdep="sparc_mach_dep.lo"
###    ;;
### ia64-*-*)
###    machdep="mach_dep.lo ia64_save_regs_in_stack.lo"
###    ;;
###esac
if(NOT machdep)
  set(machdep "mach_dep.c")
endif()
ac_msg_result("${machdep}")

ac_msg_checking("for threads package to use")
ac_msg_result("${THREADS}")

# Include defines that have become de facto standard.
# ALL_INTERIOR_POINTERS can be overridden in startup code.
add_definitions(-DSILENT -DNO_SIGNALS -DNO_EXECUTE_PERMISSION)
# AC_DEFINE(ALL_INTERIOR_POINTERS)

# By default, make the library as general as possible.
add_definitions(-DJAVA_FINALIZATION -DGC_GCJ_SUPPORT -DATOMIC_UNCOLLECTABLE)

add_definitions(-D_IN_LIBGC)

set(SUBDIRS include doc)

foreach(dir ${SUBDIRS})
  add_subdirectory(${dir})
endforeach()

set(top_srcdir ../)

# .. is needed for mono/utils/mono-compiler.h
INCLUDE_DIRECTORIES(. .. include)
add_definitions(${CPPFLAGS})

if(powerpc_darwin)
  set(asm_libgc_sources powerpc_darwin_mach_dep.s)
endif()

set(libmonogc_la_SOURCES 
  allchblk.c alloc.c blacklst.c checksums.c dbg_mlc.c
  dyn_load.c finalize.c gc_dlopen.c gcj_mlc.c headers.c
  malloc.c mallocx.c mark.c mark_rts.c misc.c new_hblk.c
  obj_map.c os_dep.c pcr_interface.c ptr_chck.c real_malloc.c reclaim.c
  solaris_pthreads.c solaris_threads.c specific.c stubborn.c typd_mlc.c
  backgraph.c win32_threads.c
  pthread_support.c pthread_stop_world.c darwin_stop_world.c
  ${asm_libgc_sources} ${machdep})

add_library(monogc-static STATIC ${libmonogc_la_SOURCES})
target_link_libraries (${THREADDLLIBS} ${UNWINDLIBS})
