#
# Copyright (c) 1994 by Xerox Corporation.  All rights reserved.
# Copyright (c) 1996 by Silicon Graphics.  All rights reserved.
# Copyright (c) 1998 by Fergus Henderson.  All rights reserved.
# Copyright (c) 2000-2010 by Hewlett-Packard Company.  All rights reserved.
##
# THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
# OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
##
# Permission is hereby granted to use or copy this program
# for any purpose,  provided the above notices are retained on all copies.
# Permission to modify the code and to distribute modified code is granted,
# provided the above notices are retained, and a notice that the code was
# modified is included with the above copyright notice.
##

#
#  get cmake and run:
#    cmake -G "Visual Studio 8 2005"
#  in the same dir as this file
#  this will generate gc.sln
#

SET(CMAKE_LEGACY_CYGWIN_WIN32 0) # Remove when CMake >= 2.8.4 is required

PROJECT(gc)

INCLUDE(CTest)

CMAKE_MINIMUM_REQUIRED(VERSION 2.6)

ADD_DEFINITIONS("-D_CRT_SECURE_NO_DEPRECATE
                 -DALL_INTERIOR_POINTERS -DNO_EXECUTE_PERMISSION")

IF(APPLE)
    IF("${CMAKE_OSX_ARCHITECTURES}" STREQUAL "")
        SET(CMAKE_OSX_ARCHITECTURES "ppc;i386;x86_64" CACHE STRING "Build architectures for Mac OS X" FORCE)
    ENDIF()
ENDIF(APPLE)

#LIBATOMIC #TODO
#ADD_LIBRARY(atomic_ops STATIC )
#SET_TARGET_PROPERTIES(atomic_ops PROPERTIES COMPILE_FLAGS -DNO_DEBUGGING)


#LIBGC

INCLUDE_DIRECTORIES(include)
INCLUDE_DIRECTORIES(libatomic_ops/src)

SET(SRC alloc.c reclaim.c allchblk.c misc.c mach_dep.c os_dep.c
        mark_rts.c headers.c mark.c obj_map.c blacklst.c finalize.c
        new_hblk.c dbg_mlc.c malloc.c dyn_load.c typd_mlc.c ptr_chck.c
        mallocx.c)
SET(LIBS)
OPTION(enable_threads "TODO" NO)
IF(enable_threads)
        FIND_PACKAGE(Threads REQUIRED)
        MESSAGE("Thread Model: ${CMAKE_THREAD_LIBS_INIT}" )
        INCLUDE_DIRECTORIES(${Threads_INCLUDE_DIR})
        SET(LIBS ${LIBS} ${Threads_LIBRARIES})
ENDIF(enable_threads)

OPTION(enable_handle_fork "Attempt to ensure a usable collector after fork()" ON)

OPTION(enable_thread_local_alloc "Turn on thread-local allocation optimization" ON)

OPTION(enable_parallel_mark "Parallelize marking and free list construction" ON)

#IF(Threads_FOUND)
#       ADD_DEFINITIONS("")
#ELSE
#       MESSAGE("Parallel mark requires enable_threads ON" )
#ENDIF(Threads_FOUND)

#OPTION(enable_cplusplus "install C++ support" ON)
SET(SRC ${SRC} gc_cpp.cc)

SET(_HOST ${CMAKE_HOST_SYSTEM_PROCESSOR}--${CMAKE_SYSTEM}) #FIXME missing the vendor field.
STRING(TOLOWER ${_HOST} HOST)
MESSAGE("HOST = ${HOST}")

#Thread Detection. Relying on cmake for lib an includes.
#TODO check cmake detection
IF(CMAKE_USE_PTHREADS_INIT)
        SET(SRC ${SRC} pthread_start.c pthread_support.c pthread_stop_world.c)
        # Common defines for most POSIX platforms.
        IF( HOST MATCHES .*-.*-aix.*|.*-.*-android.*|.*-.*-cygwin.*|.*-.*-darwin.*|.*-.*-.*freebsd.*|.*-.*-haiku.*|.*-.*-gnu.*|.*-.*-hpux11.*|.*-.*-irix.*|.*-.*-.*linux.*|.*-.*-nacl.*|.*-.*-netbsd.*|.*-.*-openbsd.*|.*-.*-osf.*|.*-.*-solaris.*)
                ADD_DEFINITIONS("-DGC_THREADS -D_REENTRANT")
                IF(enable_parallel_mark)
                    ADD_DEFINITIONS("-DPARALLEL_MARK")
                ENDIF(enable_parallel_mark)
                IF(enable_thread_local_alloc)
                    ADD_DEFINITIONS("-DTHREAD_LOCAL_ALLOC")
                    SET(SRC ${SRC} thread_local_alloc.c)
                ENDIF(enable_thread_local_alloc)
                MESSAGE("Explicit GC_INIT() calls may be required.")
        ENDIF()
        IF ( HOST MATCHES .*-.*-hpux11.*)
                MESSAGE("Only HP/UX 11 POSIX threads are supported.")
                ADD_DEFINITIONS("-D_POSIX_C_SOURCE=199506L") #TODO test -DVAR=value. Alternative is COMPILE_DEFINITIONS property
        ENDIF()
        IF ( HOST MATCHES .*-.*-hpux10.*)
                MESSAGE("HP/UX 10 POSIX threads are not supported.")
        ENDIF()
        IF ( HOST MATCHES .*-.*-netbsd.*)
                MESSAGE("Only on NetBSD 2.0 or later.")
                ADD_DEFINITIONS("-D_PTHREADS")
        ENDIF()
        IF( HOST MATCHES .*-.*-aix.*|.*-.*-cygwin.*|.*-.*-freebsd.*|.*-.*-haiku.*|.*-.*-hpux11.*|.*-.*-irix.*|.*-.*-kfreebsd.*-gnu|.*-.*-.*linux.*|.*-.*-netbsd.*|.*-.*-openbsd.*|.*-.*-osf.*|.*-.*-solaris.*)
                IF(enable_handle_fork)
                    ADD_DEFINITIONS("-DHANDLE_FORK")
                ENDIF(enable_handle_fork)
        ENDIF()
        IF ( HOST MATCHES .*-.*-cygwin.*)
                SET(SRC ${SRC} win32_threads.c)
        ENDIF()
        IF ( HOST MATCHES .*-.*-darwin.*)
                IF(enable_handle_fork)
                    # The incremental mode conflicts with fork handling.
                    IF(enable_parallel_mark)
                        ADD_DEFINITIONS("-DHANDLE_FORK")
                    ENDIF(enable_parallel_mark)
                ENDIF(enable_handle_fork)
                SET(SRC ${SRC} darwin_stop_world.c)
                #TODO
                #darwin_threads=true
        ENDIF()
ENDIF(CMAKE_USE_PTHREADS_INIT)

IF(CMAKE_USE_WIN32_THREADS_INIT)
        ADD_DEFINITIONS("-DGC_THREADS")
        IF(enable_parallel_mark)
                ADD_DEFINITIONS("-DPARALLEL_MARK")
                IF(enable_thread_local_alloc)
                    ADD_DEFINITIONS("-DTHREAD_LOCAL_ALLOC")
                    SET(SRC ${SRC} thread_local_alloc.c)
                ENDIF(enable_thread_local_alloc)
        ENDIF()
        ADD_DEFINITIONS("-DEMPTY_GETENV_RESULTS") #TODO test
        SET(SRC ${SRC} win32_threads.c)
ENDIF(CMAKE_USE_WIN32_THREADS_INIT)

OPTION(enable_gcj_support "Support for gcj" ON)
IF(enable_gcj_support)
        ADD_DEFINITIONS("-DGC_GCJ_SUPPORT")
        IF(enable_threads)
                ADD_DEFINITIONS("-DGC_ENABLE_SUSPEND_THREAD")
        ENDIF(enable_threads)
        SET(SRC ${SRC} gcj_mlc.c)
ENDIF(enable_gcj_support)

OPTION(enable_disclaim "Support alternative finalization interface" ON)
IF(enable_disclaim)
    ADD_DEFINITIONS("-DENABLE_DISCLAIM")
    SET(SRC ${SRC} fnlz_mlc.c)
ENDIF(enable_disclaim)

OPTION(enable_java_finalization "Support for java finalization" ON)
IF(enable_java_finalization)
    ADD_DEFINITIONS("-DJAVA_FINALIZATION")
ENDIF(enable_java_finalization)

OPTION(enable_atomic_uncollectable "Support for atomic uncollectible allocation" ON)
IF(enable_atomic_uncollectable)
    ADD_DEFINITIONS("-DGC_ATOMIC_UNCOLLECTABLE")
ENDIF(enable_atomic_uncollectable)

OPTION(enable_gc_debug "Support for pointer back-tracing" NO)
IF(enable_gc_debug)
    ADD_DEFINITIONS("-DDBG_HDRS_ALL -DKEEP_BACK_PTRS")
    IF (HOST MATCHES ia64-.*-linux.*|i586-.*-linux.*|i686-.*-linux.*|x86-.*-linux.*|x86_64-.*-linux.*)
        ADD_DEFINITIONS("-DMAKE_BACK_GRAPH")
        ADD_DEFINITIONS("-DSAVE_CALL_COUNT=8")
        SET(SRC ${SRC} backgraph.c)
    ENDIF()
    IF (HOST MATCHES i.86-.*-dgux.*)
        ADD_DEFINITIONS("-DMAKE_BACK_GRAPH")
        SET(SRC ${SRC} backgraph.c)
    ENDIF()
ENDIF(enable_gc_debug)

OPTION(enable_redirect_malloc "Redirect malloc and friends to GC routines" NO)
IF(enable_redirect_malloc)
    IF(enable_gc_debug)
        ADD_DEFINITIONS("-DREDIRECT_MALLOC=GC_debug_malloc_replacement")
        ADD_DEFINITIONS("-DREDIRECT_REALLOC=GC_debug_realloc_replacement")
        ADD_DEFINITIONS("-DREDIRECT_FREE=GC_debug_free")
    ELSE(enable_gc_debug)
        ADD_DEFINITIONS("-DREDIRECT_MALLOC=GC_malloc")
    ENDIF(enable_gc_debug)
    ADD_DEFINITIONS("-DGC_USE_DLOPEN_WRAP")
ENDIF(enable_redirect_malloc)

OPTION(enable_munmap "Return page to the OS if empty for N collections" ON)
IF(enable_munmap)
    ADD_DEFINITIONS("-DUSE_MMAP -DUSE_MUNMAP")
ENDIF(enable_munmap)

OPTION(enable_large_config "Optimize for large heap or root set" NO)
IF(enable_large_config)
    ADD_DEFINITIONS("-DLARGE_CONFIG")
ENDIF(enable_large_config)

OPTION(enable_gc_assertions "Enable collector-internal assertion checking" NO)
IF(enable_gc_assertions)
    ADD_DEFINITIONS("-DGC_ASSERTIONS")
ENDIF(enable_gc_assertions)

OPTION(enable_checksums "Report erroneously cleared dirty bits" NO)
IF(enable_checksums)
    IF(enable_munmap OR enable_threads)
        MESSAGE("CHECKSUMS not compatible with USE_MUNMAP or threads")
    ENDIF(enable_threads)
    ADD_DEFINITIONS("-DCHECKSUMS")
    SET(SRC ${SRC} checksums.c)
ENDIF(enable_checksums)

ADD_LIBRARY(          gc-lib   STATIC      ${SRC})
SET_TARGET_PROPERTIES(gc-lib   PROPERTIES
                      COMPILE_DEFINITIONS GC_NOT_DLL)
#TODO TARGET_LINK_LIBRARIES(...  ...  ${LIBS})

ADD_LIBRARY(          gcmt-lib STATIC      ${SRC})
SET_TARGET_PROPERTIES(gcmt-lib PROPERTIES
                      COMPILE_DEFINITIONS GC_NOT_DLL)

ADD_LIBRARY(          gcmt-dll  SHARED     ${SRC})

IF(WIN32)
  ADD_EXECUTABLE(cord cord/cordbscs.c cord/cordxtra.c
                 cord/tests/de.c cord/tests/de_win.c)
  SET_TARGET_PROPERTIES(cord PROPERTIES WIN32_EXECUTABLE TRUE)
  SET_TARGET_PROPERTIES(cord    PROPERTIES
                      COMPILE_DEFINITIONS GC_NOT_DLL)
  TARGET_LINK_LIBRARIES(cord gc-lib)
  TARGET_LINK_LIBRARIES(cord gdi32)
ENDIF(WIN32)

ADD_SUBDIRECTORY(tests)
