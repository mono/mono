/*
 * Copyright (c) 2000-2005 by Hewlett-Packard Company.  All rights reserved.
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

/* Included indirectly from a thread-library-specific file.     */
/* This is the interface for thread-local allocation, whose     */
/* implementation is mostly thread-library-independent.         */
/* Here we describe only the interface that needs to be known   */
/* and invoked from the thread support layer;  the actual       */
/* implementation also exports GC_malloc and friends, which     */
/* are declared in gc.h.                                        */

#ifndef GC_THREAD_LOCAL_ALLOC_H
#define GC_THREAD_LOCAL_ALLOC_H

#include "private/gc_priv.h"

#ifdef THREAD_LOCAL_ALLOC

#include "gc_inline.h"

#if defined(USE_HPUX_TLS)
# error USE_HPUX_TLS macro was replaced by USE_COMPILER_TLS
#endif

#include <stdlib.h>

EXTERN_C_BEGIN

#if !defined(USE_PTHREAD_SPECIFIC) && !defined(USE_WIN32_SPECIFIC) \
    && !defined(USE_WIN32_COMPILER_TLS) && !defined(USE_COMPILER_TLS) \
    && !defined(USE_CUSTOM_SPECIFIC)
# if defined(MSWIN32) || defined(MSWINCE) || defined(CYGWIN32)
#   if defined(CYGWIN32) && GC_GNUC_PREREQ(4, 0)
#     if defined(__clang__)
        /* As of Cygwin clang3.5.2, thread-local storage is unsupported.    */
#       define USE_PTHREAD_SPECIFIC
#     else
#       define USE_COMPILER_TLS
#     endif
#   elif defined(__GNUC__) || defined(MSWINCE)
#     define USE_WIN32_SPECIFIC
#   else
#     define USE_WIN32_COMPILER_TLS
#   endif /* !GNU */
# elif (defined(LINUX) && !defined(ARM32) && !defined(AVR32) \
         && GC_GNUC_PREREQ(3, 3) \
         && !(defined(__clang__) && defined(HOST_ANDROID))) \
       || (defined(FREEBSD) && defined(__GLIBC__) /* kFreeBSD */ \
            && GC_GNUC_PREREQ(4, 4)) \
       || (defined(HOST_ANDROID) && defined(ARM32) \
            && (GC_GNUC_PREREQ(4, 6) || GC_CLANG_PREREQ_FULL(3, 8, 256229)))
#   define USE_COMPILER_TLS
# elif defined(GC_DGUX386_THREADS) || defined(GC_OSF1_THREADS) \
       || defined(GC_AIX_THREADS) || defined(GC_DARWIN_THREADS) \
       || defined(GC_FREEBSD_THREADS) || defined(GC_NETBSD_THREADS) \
       || defined(GC_LINUX_THREADS) || defined(GC_HAIKU_THREADS) \
       || defined(GC_RTEMS_PTHREADS)
#   define USE_PTHREAD_SPECIFIC
# elif defined(GC_HPUX_THREADS)
#   ifdef __GNUC__
#     define USE_PTHREAD_SPECIFIC
        /* Empirically, as of gcc 3.3, USE_COMPILER_TLS doesn't work.   */
#   else
#     define USE_COMPILER_TLS
#   endif
# else
#    define USE_CUSTOM_SPECIFIC  /* Use our own. */
# endif
#endif

#ifndef THREAD_FREELISTS_KINDS
# ifdef ENABLE_DISCLAIM
#   define THREAD_FREELISTS_KINDS (NORMAL+2)
# else
#   define THREAD_FREELISTS_KINDS (NORMAL+1)
# endif
#endif /* !THREAD_FREELISTS_KINDS */

/* One of these should be declared as the tlfs field in the     */
/* structure pointed to by a GC_thread.                         */
typedef struct thread_local_freelists {
  void * _freelists[THREAD_FREELISTS_KINDS][TINY_FREELISTS];
# define ptrfree_freelists _freelists[PTRFREE]
# define normal_freelists _freelists[NORMAL]
        /* Note: Preserve *_freelists names for some clients.   */
# ifdef GC_GCJ_SUPPORT
    void * gcj_freelists[TINY_FREELISTS];
#   define ERROR_FL ((void *)(word)-1)
        /* Value used for gcj_freelists[-1]; allocation is      */
        /* erroneous.                                           */
# endif
  /* Free lists contain either a pointer or a small count       */
  /* reflecting the number of granules allocated at that        */
  /* size.                                                      */
  /* 0 ==> thread-local allocation in use, free list            */
  /*       empty.                                               */
  /* > 0, <= DIRECT_GRANULES ==> Using global allocation,       */
  /*       too few objects of this size have been               */
  /*       allocated by this thread.                            */
  /* >= HBLKSIZE  => pointer to nonempty free list.             */
  /* > DIRECT_GRANULES, < HBLKSIZE ==> transition to            */
  /*    local alloc, equivalent to 0.                           */
# define DIRECT_GRANULES (HBLKSIZE/GRANULE_BYTES)
        /* Don't use local free lists for up to this much       */
        /* allocation.                                          */
} *GC_tlfs;

#if defined(USE_PTHREAD_SPECIFIC)
# define GC_getspecific pthread_getspecific
# define GC_setspecific pthread_setspecific
# define GC_key_create pthread_key_create
# define GC_remove_specific(key) pthread_setspecific(key, NULL)
                        /* Explicitly delete the value to stop the TLS  */
                        /* destructor from being called repeatedly.     */
# define GC_remove_specific_after_fork(key, t) (void)0
                                        /* Should not need any action.  */
  typedef pthread_key_t GC_key_t;
#elif defined(USE_COMPILER_TLS) || defined(USE_WIN32_COMPILER_TLS)
# define GC_getspecific(x) (x)
# define GC_setspecific(key, v) ((key) = (v), 0)
# define GC_key_create(key, d) 0
# define GC_remove_specific(key)  /* No need for cleanup on exit. */
# define GC_remove_specific_after_fork(key, t) (void)0
  typedef void * GC_key_t;
#elif defined(USE_WIN32_SPECIFIC)
# ifndef WIN32_LEAN_AND_MEAN
#   define WIN32_LEAN_AND_MEAN 1
# endif
# define NOSERVICE
  EXTERN_C_END
# include <windows.h>
  EXTERN_C_BEGIN
# define GC_getspecific TlsGetValue
# define GC_setspecific(key, v) !TlsSetValue(key, v)
        /* We assume 0 == success, msft does the opposite.      */
# ifndef TLS_OUT_OF_INDEXES
        /* this is currently missing in WinCE   */
#   define TLS_OUT_OF_INDEXES (DWORD)0xFFFFFFFF
# endif
# define GC_key_create(key, d) \
        ((d) != 0 || (*(key) = TlsAlloc()) == TLS_OUT_OF_INDEXES ? -1 : 0)
# define GC_remove_specific(key)  /* No need for cleanup on exit. */
        /* Need TlsFree on process exit/detach?   */
# define GC_remove_specific_after_fork(key, t) (void)0
  typedef DWORD GC_key_t;
#elif defined(USE_CUSTOM_SPECIFIC)
  EXTERN_C_END
# include "private/specific.h"
  EXTERN_C_BEGIN
#else
# error implement me
#endif

/* Each thread structure must be initialized.   */
/* This call must be made from the new thread.  */
/* Caller holds allocation lock.                */
GC_INNER void GC_init_thread_local(GC_tlfs p);

/* Called when a thread is unregistered, or exits.      */
/* We hold the allocator lock.                          */
GC_INNER void GC_destroy_thread_local(GC_tlfs p);

/* The thread support layer must arrange to mark thread-local   */
/* free lists explicitly, since the link field is often         */
/* invisible to the marker.  It knows how to find all threads;  */
/* we take care of an individual thread freelist structure.     */
GC_INNER void GC_mark_thread_local_fls_for(GC_tlfs p);

#ifdef GC_ASSERTIONS
  GC_bool GC_is_thread_tsd_valid(void *tsd);
  void GC_check_tls_for(GC_tlfs p);
# if defined(USE_CUSTOM_SPECIFIC)
    void GC_check_tsd_marks(tsd *key);
# endif
#endif /* GC_ASSERTIONS */

#ifndef GC_ATTR_TLS_FAST
# define GC_ATTR_TLS_FAST /* empty */
#endif

extern
#if defined(USE_COMPILER_TLS)
  __thread GC_ATTR_TLS_FAST
#elif defined(USE_WIN32_COMPILER_TLS)
  __declspec(thread) GC_ATTR_TLS_FAST
#endif
  GC_key_t GC_thread_key;
/* This is set up by the thread_local_alloc implementation.  No need    */
/* for cleanup on thread exit.  But the thread support layer makes sure */
/* that GC_thread_key is traced, if necessary.                          */

EXTERN_C_END

#endif /* THREAD_LOCAL_ALLOC */

#endif /* GC_THREAD_LOCAL_ALLOC_H */
