/*
 * Copyright (c) 1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 1998 by Fergus Henderson.  All rights reserved.
 * Copyright (c) 2000-2009 by Hewlett-Packard Development Company.
 * All rights reserved.
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

#ifndef GC_PTHREAD_SUPPORT_H
#define GC_PTHREAD_SUPPORT_H

#include "private/gc_priv.h"

#if defined(GC_PTHREADS) && !defined(GC_WIN32_THREADS)

#if defined(GC_DARWIN_THREADS)
# include "private/darwin_stop_world.h"
#else
# include "private/pthread_stop_world.h"
#endif

#ifdef THREAD_LOCAL_ALLOC
# include "thread_local_alloc.h"
#endif

#ifdef THREAD_SANITIZER
# include "dbg_mlc.h" /* for oh type */
#endif

EXTERN_C_BEGIN

/* We use the allocation lock to protect thread-related data structures. */

/* The set of all known threads.  We intercept thread creation and      */
/* joins.                                                               */
/* Protected by allocation/GC lock.                                     */
/* Some of this should be declared volatile, but that's inconsistent    */
/* with some library routine declarations.                              */
typedef struct GC_Thread_Rep {
#   ifdef THREAD_SANITIZER
      char dummy[sizeof(oh)];     /* A dummy field to avoid TSan false  */
                                  /* positive about the race between    */
                                  /* GC_has_other_debug_info and        */
                                  /* GC_suspend_handler_inner (which    */
                                  /* sets store_stop.stack_ptr).        */
#   endif

    struct GC_Thread_Rep * next;  /* More recently allocated threads    */
                                  /* with a given pthread id come       */
                                  /* first.  (All but the first are     */
                                  /* guaranteed to be dead, but we may  */
                                  /* not yet have registered the join.) */
    pthread_t id;
#   ifdef USE_TKILL_ON_ANDROID
      pid_t kernel_id;
#   endif
    /* Extra bookkeeping information the stopping code uses */
    struct thread_stop_info stop_info;

#   if defined(GC_ENABLE_SUSPEND_THREAD) && !defined(GC_DARWIN_THREADS) \
        && !defined(GC_OPENBSD_UTHREADS) && !defined(NACL)
      volatile AO_t suspended_ext;  /* Thread was suspended externally. */
#   endif

    unsigned char flags;
#       define FINISHED 1       /* Thread has exited.                   */
#       define DETACHED 2       /* Thread is treated as detached.       */
                                /* Thread may really be detached, or    */
                                /* it may have been explicitly          */
                                /* registered, in which case we can     */
                                /* deallocate its GC_Thread_Rep once    */
                                /* it unregisters itself, since it      */
                                /* may not return a GC pointer.         */
#       define MAIN_THREAD 4    /* True for the original thread only.   */
#       define DISABLED_GC 0x10 /* Collections are disabled while the   */
                                /* thread is exiting.                   */

    unsigned char thread_blocked;
                                /* Protected by GC lock.                */
                                /* Treated as a boolean value.  If set, */
                                /* thread will acquire GC lock before   */
                                /* doing any pointer manipulations, and */
                                /* has set its SP value.  Thus it does  */
                                /* not need to be sent a signal to stop */
                                /* it.                                  */

    unsigned short finalizer_skipped;
    unsigned char finalizer_nested;
                                /* Used by GC_check_finalizer_nested()  */
                                /* to minimize the level of recursion   */
                                /* when a client finalizer allocates    */
                                /* memory (initially both are 0).       */

    ptr_t stack_end;            /* Cold end of the stack (except for    */
                                /* main thread).                        */
    ptr_t altstack;             /* The start of the alt-stack if there  */
                                /* is one, NULL otherwise.              */
    word altstack_size;         /* The size of the alt-stack if exists. */
    ptr_t stack;                /* The start and size of the normal     */
                                /* stack (set by GC_register_altstack). */
    word stack_size;
#   if defined(GC_DARWIN_THREADS) && !defined(DARWIN_DONT_PARSE_STACK)
      ptr_t topOfStack;         /* Result of GC_FindTopOfStack(0);      */
                                /* valid only if the thread is blocked; */
                                /* non-NULL value means already set.    */
#   endif
#   ifdef IA64
        ptr_t backing_store_end;
        ptr_t backing_store_ptr;
#   endif

    struct GC_traced_stack_sect_s *traced_stack_sect;
                        /* Points to the "frame" data held in stack by  */
                        /* the innermost GC_call_with_gc_active() of    */
                        /* this thread.  May be NULL.                   */

    void * status;              /* The value returned from the thread.  */
                                /* Used only to avoid premature         */
                                /* reclamation of any data it might     */
                                /* reference.                           */
                                /* This is unfortunately also the       */
                                /* reason we need to intercept join     */
                                /* and detach.                          */

#   ifdef THREAD_LOCAL_ALLOC
        struct thread_local_freelists tlfs;
#   endif
} * GC_thread;

#ifndef THREAD_TABLE_SZ
# define THREAD_TABLE_SZ 256    /* Power of 2 (for speed). */
#endif

#if CPP_WORDSZ == 64
# define THREAD_TABLE_INDEX(id) \
    (int)(((((NUMERIC_THREAD_ID(id) >> 8) ^ NUMERIC_THREAD_ID(id)) >> 16) \
          ^ ((NUMERIC_THREAD_ID(id) >> 8) ^ NUMERIC_THREAD_ID(id))) \
         % THREAD_TABLE_SZ)
#else
# define THREAD_TABLE_INDEX(id) \
                (int)(((NUMERIC_THREAD_ID(id) >> 16) \
                       ^ (NUMERIC_THREAD_ID(id) >> 8) \
                       ^ NUMERIC_THREAD_ID(id)) % THREAD_TABLE_SZ)
#endif

GC_EXTERN volatile GC_thread GC_threads[THREAD_TABLE_SZ];

GC_EXTERN GC_bool GC_thr_initialized;

GC_INNER GC_thread GC_lookup_thread(pthread_t id);

GC_EXTERN GC_bool GC_in_thread_creation;
        /* We may currently be in thread creation or destruction.       */
        /* Only set to TRUE while allocation lock is held.              */
        /* When set, it is OK to run GC from unknown thread.            */

#ifdef NACL
  GC_EXTERN __thread GC_thread GC_nacl_gc_thread_self;
  GC_INNER void GC_nacl_initialize_gc_thread(void);
  GC_INNER void GC_nacl_shutdown_gc_thread(void);
#endif

#ifdef GC_EXPLICIT_SIGNALS_UNBLOCK
  GC_INNER void GC_unblock_gc_signals(void);
#endif

#ifdef GC_PTHREAD_START_STANDALONE
# define GC_INNER_PTHRSTART /* empty */
#else
# define GC_INNER_PTHRSTART GC_INNER
#endif

GC_INNER_PTHRSTART void * GC_CALLBACK GC_inner_start_routine(
                                        struct GC_stack_base *sb, void *arg);

GC_INNER_PTHRSTART GC_thread GC_start_rtn_prepare_thread(
                                        void *(**pstart)(void *),
                                        void **pstart_arg,
                                        struct GC_stack_base *sb, void *arg);
GC_INNER_PTHRSTART void GC_thread_exit_proc(void *);

EXTERN_C_END

#endif /* GC_PTHREADS && !GC_WIN32_THREADS */

#endif /* GC_PTHREAD_SUPPORT_H */
