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

#include "private/pthread_support.h"

#if defined(GC_PTHREADS) && !defined(GC_WIN32_THREADS) && \
    !defined(GC_DARWIN_THREADS) && !defined(SN_TARGET_ORBIS) \
    && !defined(SN_TARGET_PSP2)

#ifdef NACL

# include <unistd.h>
# include <sys/time.h>

  STATIC int GC_nacl_num_gc_threads = 0;
  STATIC __thread int GC_nacl_thread_idx = -1;
  STATIC volatile int GC_nacl_park_threads_now = 0;
  STATIC volatile pthread_t GC_nacl_thread_parker = -1;

  GC_INNER __thread GC_thread GC_nacl_gc_thread_self = NULL;

  volatile int GC_nacl_thread_parked[MAX_NACL_GC_THREADS];
  int GC_nacl_thread_used[MAX_NACL_GC_THREADS];

#elif defined(GC_OPENBSD_UTHREADS)

# include <pthread_np.h>

#else /* !GC_OPENBSD_UTHREADS && !NACL */

#include <signal.h>
#include <semaphore.h>
#include <errno.h>
#include <time.h> /* for nanosleep() */
#include <unistd.h>

#if (!defined(AO_HAVE_load_acquire) || !defined(AO_HAVE_store_release)) \
    && !defined(CPPCHECK)
# error AO_load_acquire and/or AO_store_release are missing;
# error please define AO_REQUIRE_CAS manually
#endif

/* It's safe to call original pthread_sigmask() here.   */
#undef pthread_sigmask

#ifdef GC_ENABLE_SUSPEND_THREAD
  static void *GC_CALLBACK suspend_self_inner(void *client_data);
#endif

#ifdef DEBUG_THREADS
# ifndef NSIG
#   if defined(MAXSIG)
#     define NSIG (MAXSIG+1)
#   elif defined(_NSIG)
#     define NSIG _NSIG
#   elif defined(__SIGRTMAX)
#     define NSIG (__SIGRTMAX+1)
#   else
#     error define NSIG
#   endif
# endif /* NSIG */

  void GC_print_sig_mask(void)
  {
    sigset_t blocked;
    int i;

    if (pthread_sigmask(SIG_BLOCK, NULL, &blocked) != 0)
      ABORT("pthread_sigmask failed");
    for (i = 1; i < NSIG; i++) {
      if (sigismember(&blocked, i))
        GC_printf("Signal blocked: %d\n", i);
    }
  }
#endif /* DEBUG_THREADS */

/* Remove the signals that we want to allow in thread stopping  */
/* handler from a set.                                          */
STATIC void GC_remove_allowed_signals(sigset_t *set)
{
    if (sigdelset(set, SIGINT) != 0
          || sigdelset(set, SIGQUIT) != 0
          || sigdelset(set, SIGABRT) != 0
          || sigdelset(set, SIGTERM) != 0) {
        ABORT("sigdelset failed");
    }

#   ifdef MPROTECT_VDB
      /* Handlers write to the thread structure, which is in the heap,  */
      /* and hence can trigger a protection fault.                      */
      if (sigdelset(set, SIGSEGV) != 0
#         ifdef HAVE_SIGBUS
            || sigdelset(set, SIGBUS) != 0
#         endif
          ) {
        ABORT("sigdelset failed");
      }
#   endif
}

static sigset_t suspend_handler_mask;

STATIC volatile AO_t GC_stop_count = 0;
                        /* Incremented by two at the beginning of       */
                        /* GC_stop_world (the lowest bit is always 0).  */

STATIC volatile AO_t GC_world_is_stopped = FALSE;
                        /* FALSE ==> it is safe for threads to restart, */
                        /* i.e. they will see another suspend signal    */
                        /* before they are expected to stop (unless     */
                        /* they have stopped voluntarily).              */

#if defined(GC_OSF1_THREADS) || defined(THREAD_SANITIZER) \
    || defined(ADDRESS_SANITIZER) || defined(MEMORY_SANITIZER)
  STATIC GC_bool GC_retry_signals = TRUE;
#else
  // Unity: Always enable retry signals, since any platform could lose signals
  STATIC GC_bool GC_retry_signals = TRUE;
#endif

#define UNITY_RETRY_SIGNALS

/*
 * We use signals to stop threads during GC.
 *
 * Suspended threads wait in signal handler for SIG_THR_RESTART.
 * That's more portable than semaphores or condition variables.
 * (We do use sem_post from a signal handler, but that should be portable.)
 *
 * The thread suspension signal SIG_SUSPEND is now defined in gc_priv.h.
 * Note that we can't just stop a thread; we need it to save its stack
 * pointer(s) and acknowledge.
 */
#ifndef SIG_THR_RESTART
# if defined(GC_HPUX_THREADS) || defined(GC_OSF1_THREADS) \
     || defined(GC_NETBSD_THREADS) || defined(GC_USESIGRT_SIGNALS)
#   if defined(_SIGRTMIN) && !defined(CPPCHECK)
#     define SIG_THR_RESTART _SIGRTMIN + 5
#   else
#     define SIG_THR_RESTART SIGRTMIN + 5
#   endif
# else
#   define SIG_THR_RESTART SIGXCPU
# endif
#endif

#define SIGNAL_UNSET (-1)
    /* Since SIG_SUSPEND and/or SIG_THR_RESTART could represent */
    /* a non-constant expression (e.g., in case of SIGRTMIN),   */
    /* actual signal numbers are determined by GC_stop_init()   */
    /* unless manually set (before GC initialization).          */
STATIC int GC_sig_suspend = SIGNAL_UNSET;
STATIC int GC_sig_thr_restart = SIGNAL_UNSET;

GC_API void GC_CALL GC_set_suspend_signal(int sig)
{
  if (GC_is_initialized) return;

  GC_sig_suspend = sig;
}

GC_API void GC_CALL GC_set_thr_restart_signal(int sig)
{
  if (GC_is_initialized) return;

  GC_sig_thr_restart = sig;
}

GC_API int GC_CALL GC_get_suspend_signal(void)
{
  return GC_sig_suspend != SIGNAL_UNSET ? GC_sig_suspend : SIG_SUSPEND;
}

GC_API int GC_CALL GC_get_thr_restart_signal(void)
{
  return GC_sig_thr_restart != SIGNAL_UNSET
            ? GC_sig_thr_restart : SIG_THR_RESTART;
}

#if defined(GC_EXPLICIT_SIGNALS_UNBLOCK) \
    || !defined(NO_SIGNALS_UNBLOCK_IN_MAIN)
  /* Some targets (e.g., Solaris) might require this to be called when  */
  /* doing thread registering from the thread destructor.               */
  GC_INNER void GC_unblock_gc_signals(void)
  {
    sigset_t set;
    sigemptyset(&set);
    GC_ASSERT(GC_sig_suspend != SIGNAL_UNSET);
    GC_ASSERT(GC_sig_thr_restart != SIGNAL_UNSET);
    sigaddset(&set, GC_sig_suspend);
    sigaddset(&set, GC_sig_thr_restart);
    if (pthread_sigmask(SIG_UNBLOCK, &set, NULL) != 0)
      ABORT("pthread_sigmask failed");
  }
#endif /* GC_EXPLICIT_SIGNALS_UNBLOCK */

STATIC sem_t GC_suspend_ack_sem; /* also used to acknowledge restart */

STATIC void GC_suspend_handler_inner(ptr_t dummy, void *context);

#ifndef NO_SA_SIGACTION
  STATIC void GC_suspend_handler(int sig, siginfo_t * info GC_ATTR_UNUSED,
                                 void * context GC_ATTR_UNUSED)
#else
  STATIC void GC_suspend_handler(int sig)
#endif
{
  int old_errno = errno;

  if (sig != GC_sig_suspend) {
#   if defined(GC_FREEBSD_THREADS)
      /* Workaround "deferred signal handling" bug in FreeBSD 9.2.      */
      if (0 == sig) return;
#   endif
    ABORT("Bad signal in suspend_handler");
  }

# if defined(IA64) || defined(HP_PA) || defined(M68K)
    GC_with_callee_saves_pushed(GC_suspend_handler_inner, NULL);
# else
    /* We believe that in all other cases the full context is already   */
    /* in the signal handler frame.                                     */
    {
#     ifdef NO_SA_SIGACTION
        void *context = 0;
#     endif
      GC_suspend_handler_inner(NULL, context);
    }
# endif
  errno = old_errno;
}

/* The lookup here is safe, since this is done on behalf        */
/* of a thread which holds the allocation lock in order         */
/* to stop the world.  Thus concurrent modification of the      */
/* data structure is impossible.  Unfortunately, we have to     */
/* instruct TSan that the lookup is safe.                       */
#ifdef THREAD_SANITIZER
  /* The implementation of the function is the same as that of  */
  /* GC_lookup_thread except for the attribute added here.      */
  GC_ATTR_NO_SANITIZE_THREAD
  static GC_thread GC_lookup_thread_async(pthread_t id)
  {
    GC_thread p = GC_threads[THREAD_TABLE_INDEX(id)];

    while (p != NULL && !THREAD_EQUAL(p->id, id))
      p = p->next;
    return p;
  }
#else
# define GC_lookup_thread_async GC_lookup_thread
#endif

GC_INLINE void GC_store_stack_ptr(GC_thread me)
{
  /* There is no data race between the suspend handler (storing         */
  /* stack_ptr) and GC_push_all_stacks (fetching stack_ptr) because     */
  /* GC_push_all_stacks is executed after GC_stop_world exits and the   */
  /* latter runs sem_wait repeatedly waiting for all the suspended      */
  /* threads to call sem_post.  Nonetheless, stack_ptr is stored (here) */
  /* and fetched (by GC_push_all_stacks) using the atomic primitives to */
  /* avoid the related TSan warning.                                    */
# ifdef SPARC
    AO_store((volatile AO_t *)&me->stop_info.stack_ptr,
             (AO_t)GC_save_regs_in_stack());
# else
#   ifdef IA64
      me -> backing_store_ptr = GC_save_regs_in_stack();
#   endif
    AO_store((volatile AO_t *)&me->stop_info.stack_ptr, (AO_t)GC_approx_sp());
# endif
}

STATIC void GC_suspend_handler_inner(ptr_t dummy GC_ATTR_UNUSED,
                                     void * context GC_ATTR_UNUSED)
{
  pthread_t self = pthread_self();
  GC_thread me;
  IF_CANCEL(int cancel_state;)
  AO_t my_stop_count = AO_load_acquire(&GC_stop_count);
                        /* After the barrier, this thread should see    */
                        /* the actual content of GC_threads.            */

  DISABLE_CANCEL(cancel_state);
      /* pthread_setcancelstate is not defined to be async-signal-safe. */
      /* But the glibc version appears to be in the absence of          */
      /* asynchronous cancellation.  And since this signal handler      */
      /* to block on sigsuspend, which is both async-signal-safe        */
      /* and a cancellation point, there seems to be no obvious way     */
      /* out of it.  In fact, it looks to me like an async-signal-safe  */
      /* cancellation point is inherently a problem, unless there is    */
      /* some way to disable cancellation in the handler.               */
# ifdef DEBUG_THREADS
    GC_log_printf("Suspending %p\n", (void *)self);
# endif
  GC_ASSERT(((word)my_stop_count & 1) == 0);

  me = GC_lookup_thread_async(self);

# ifdef GC_ENABLE_SUSPEND_THREAD
    if (AO_load(&me->suspended_ext)) {
      GC_store_stack_ptr(me);
      sem_post(&GC_suspend_ack_sem);
      suspend_self_inner(me);
#     ifdef DEBUG_THREADS
        GC_log_printf("Continuing %p on GC_resume_thread\n", (void *)self);
#     endif
      RESTORE_CANCEL(cancel_state);
      return;
    }
# endif

  if (((word)me->stop_info.last_stop_count & ~(word)0x1)
        == (word)my_stop_count) {
      /* Duplicate signal.  OK if we are retrying.      */
      if (!GC_retry_signals) {
          WARN("Duplicate suspend signal in thread %p\n", self);
      }
      RESTORE_CANCEL(cancel_state);
      return;
  }
  GC_store_stack_ptr(me);

# ifdef THREAD_SANITIZER
    /* TSan disables signals around signal handlers.  Without   */
    /* a pthread_sigmask call, sigsuspend may block forever.    */
    {
      sigset_t set;
      sigemptyset(&set);
      GC_ASSERT(GC_sig_suspend != SIGNAL_UNSET);
      GC_ASSERT(GC_sig_thr_restart != SIGNAL_UNSET);
      sigaddset(&set, GC_sig_suspend);
      sigaddset(&set, GC_sig_thr_restart);
      if (pthread_sigmask(SIG_UNBLOCK, &set, NULL) != 0)
        ABORT("pthread_sigmask failed in suspend handler");
    }
# endif
  /* Tell the thread that wants to stop the world that this     */
  /* thread has been stopped.  Note that sem_post() is          */
  /* the only async-signal-safe primitive in LinuxThreads.      */
  sem_post(&GC_suspend_ack_sem);
  AO_store_release(&me->stop_info.last_stop_count, my_stop_count);

  /* Wait until that thread tells us to restart by sending      */
  /* this thread a GC_sig_thr_restart signal (should be masked  */
  /* at this point thus there is no race).                      */
  /* We do not continue until we receive that signal,           */
  /* but we do not take that as authoritative.  (We may be      */
  /* accidentally restarted by one of the user signals we       */
  /* don't block.)  After we receive the signal, we use a       */
  /* primitive and expensive mechanism to wait until it's       */
  /* really safe to proceed.  Under normal circumstances,       */
  /* this code should not be executed.                          */
  do {
      sigsuspend (&suspend_handler_mask);
  } while (AO_load_acquire(&GC_world_is_stopped)
           && AO_load(&GC_stop_count) == my_stop_count);

# ifdef DEBUG_THREADS
    GC_log_printf("Continuing %p\n", (void *)self);
# endif
# ifndef GC_NETBSD_THREADS_WORKAROUND
    if (GC_retry_signals)
# endif
  {
    /* If the RESTART signal loss is possible (though it should be      */
    /* less likely than losing the SUSPEND signal as we do not do       */
    /* much between the first sem_post and sigsuspend calls), more      */
    /* handshaking is provided to work around it.                       */
    sem_post(&GC_suspend_ack_sem);
#   ifdef GC_NETBSD_THREADS_WORKAROUND
      if (GC_retry_signals)
#   endif
    {
      /* Set the flag (the lowest bit of last_stop_count) that the      */
      /* thread has been restarted.                                     */
      AO_store_release(&me->stop_info.last_stop_count,
                       (AO_t)((word)my_stop_count | 1));
    }
  }
  RESTORE_CANCEL(cancel_state);
}

static void suspend_restart_barrier(int n_live_threads)
{
    int i;

    for (i = 0; i < n_live_threads; i++) {
      while (0 != sem_wait(&GC_suspend_ack_sem)) {
        /* On Linux, sem_wait is documented to always return zero.      */
        /* But the documentation appears to be incorrect.               */
        /* EINTR seems to happen with some versions of gdb.             */
        if (errno != EINTR)
          ABORT("sem_wait failed");
      }
    }
#   ifdef GC_ASSERTIONS
      sem_getvalue(&GC_suspend_ack_sem, &i);
      GC_ASSERT(0 == i);
#   endif
}

static int resend_lost_signals(int n_live_threads,
                               int (*suspend_restart_all)(void))
{
#   define WAIT_UNIT 3000
#   define RETRY_INTERVAL 100000

    if (n_live_threads > 0) {
      unsigned long wait_usecs = 0;  /* Total wait since retry. */
      for (;;) {
        int ack_count;

        sem_getvalue(&GC_suspend_ack_sem, &ack_count);
        if (ack_count == n_live_threads)
          break;
        if (wait_usecs > RETRY_INTERVAL) {
          int newly_sent = suspend_restart_all();

          GC_COND_LOG_PRINTF("Resent %d signals after timeout\n", newly_sent);
          sem_getvalue(&GC_suspend_ack_sem, &ack_count);
          if (newly_sent < n_live_threads - ack_count) {
            WARN("Lost some threads while stopping or starting world?!\n", 0);
            n_live_threads = ack_count + newly_sent;
          }
          wait_usecs = 0;
        }

#       ifdef LINT2
          /* Workaround "waiting while holding a lock" warning. */
#         undef WAIT_UNIT
#         define WAIT_UNIT 1
          sched_yield();
#       elif defined(CPPCHECK) /* || _POSIX_C_SOURCE >= 199309L */
          {
            struct timespec ts;

            ts.tv_sec = 0;
            ts.tv_nsec = WAIT_UNIT * 1000;
            (void)nanosleep(&ts, NULL);
          }
#       else
          usleep(WAIT_UNIT);
#       endif
        wait_usecs += WAIT_UNIT;
      }
    }
    return n_live_threads;
}

#ifdef UNITY_RETRY_SIGNALS
static void suspend_restart_barrier_retry(int n_live_threads, 
                                          int (*suspend_restart_all)(void))
{
#   define TIMEOUT_UNIT 10000

    int i;
    int acked_threads = 0;
    struct timespec ts;

    if (clock_gettime(CLOCK_REALTIME, &ts) == -1) {
        n_live_threads = resend_lost_signals(n_live_threads, suspend_restart_all);
        suspend_restart_barrier(n_live_threads);
        return;
    }

    ts.tv_nsec += TIMEOUT_UNIT * 1000;

    for (i = 0; i < n_live_threads; i++) {
      while (0 != sem_timedwait(&GC_suspend_ack_sem, &ts)) {
        /* On Linux, sem_wait is documented to always return zero.      */
        /* But the documentation appears to be incorrect.               */
        /* EINTR seems to happen with some versions of gdb.             */

        if (errno == ETIMEDOUT || errno == EINVAL) {
            // Wait timed out or the timeout period has passed
            n_live_threads = resend_lost_signals(n_live_threads - acked_threads, suspend_restart_all);
            suspend_restart_barrier(n_live_threads);
            return;
        }
        else if (errno != EINTR) {
          ABORT("sem_wait failed");
        }
      }
      acked_threads++;
    }
#   ifdef GC_ASSERTIONS
      sem_getvalue(&GC_suspend_ack_sem, &i);
      GC_ASSERT(0 == i);
#   endif
}
#endif

STATIC void GC_restart_handler(int sig)
{
# if defined(DEBUG_THREADS)
    int old_errno = errno;      /* Preserve errno value.        */
# endif

  if (sig != GC_sig_thr_restart)
    ABORT("Bad signal in restart handler");

  /*
  ** Note: even if we don't do anything useful here,
  ** it would still be necessary to have a signal handler,
  ** rather than ignoring the signals, otherwise
  ** the signals will not be delivered at all, and
  ** will thus not interrupt the sigsuspend() above.
  */
# ifdef DEBUG_THREADS
    GC_log_printf("In GC_restart_handler for %p\n", (void *)pthread_self());
    errno = old_errno;
# endif
}

# ifdef USE_TKILL_ON_ANDROID
    EXTERN_C_BEGIN
    extern int tkill(pid_t tid, int sig); /* from sys/linux-unistd.h */
    EXTERN_C_END

    static int android_thread_kill(pid_t tid, int sig)
    {
      int ret;
      int old_errno = errno;

      ret = tkill(tid, sig);
      if (ret < 0) {
          ret = errno;
          errno = old_errno;
      }
      return ret;
    }

#   define THREAD_SYSTEM_ID(t) (t)->kernel_id
#   define RAISE_SIGNAL(t, sig) android_thread_kill(THREAD_SYSTEM_ID(t), sig)
# else
#   define THREAD_SYSTEM_ID(t) (t)->id
#   define RAISE_SIGNAL(t, sig) pthread_kill(THREAD_SYSTEM_ID(t), sig)
# endif /* !USE_TKILL_ON_ANDROID */

# ifdef GC_ENABLE_SUSPEND_THREAD
#   include <sys/time.h>
#   include "javaxfc.h" /* to get the prototypes as extern "C" */

    STATIC void GC_brief_async_signal_safe_sleep(void)
    {
      struct timeval tv;
      tv.tv_sec = 0;
#     if defined(GC_TIME_LIMIT) && !defined(CPPCHECK)
        tv.tv_usec = 1000 * GC_TIME_LIMIT / 2;
#     else
        tv.tv_usec = 1000 * 50 / 2;
#     endif
      (void)select(0, 0, 0, 0, &tv);
    }

    static void *GC_CALLBACK suspend_self_inner(void *client_data) {
      GC_thread me = (GC_thread)client_data;

      while (AO_load_acquire(&me->suspended_ext)) {
        /* TODO: Use sigsuspend() instead. */
        GC_brief_async_signal_safe_sleep();
      }
      return NULL;
    }

    GC_API void GC_CALL GC_suspend_thread(GC_SUSPEND_THREAD_ID thread) {
      GC_thread t;
      IF_CANCEL(int cancel_state;)
      DCL_LOCK_STATE;

      LOCK();
      t = GC_lookup_thread((pthread_t)thread);
      if (t == NULL || t -> suspended_ext) {
        UNLOCK();
        return;
      }

      /* Set the flag making the change visible to the signal handler.  */
      /* This also removes the protection for t object, preventing      */
      /* write faults in GC_store_stack_ptr (thus double-locking should */
      /* not occur in async_set_pht_entry_from_index).                  */
      AO_store_release(&t->suspended_ext, TRUE);

      if (THREAD_EQUAL((pthread_t)thread, pthread_self())) {
        UNLOCK();
        /* It is safe as "t" cannot become invalid here (no race with   */
        /* GC_unregister_my_thread).                                    */
        (void)GC_do_blocking(suspend_self_inner, t);
        return;
      }
      if ((t -> flags & FINISHED) != 0) {
        /* Terminated but not joined yet. */
        UNLOCK();
        return;
      }

      DISABLE_CANCEL(cancel_state);
                /* GC_suspend_thread is not a cancellation point.   */
#     ifdef PARALLEL_MARK
        /* Ensure we do not suspend a thread while it is rebuilding */
        /* a free list, otherwise such a dead-lock is possible:     */
        /* thread 1 is blocked in GC_wait_for_reclaim holding       */
        /* the allocation lock, thread 2 is suspended in            */
        /* GC_reclaim_generic invoked from GC_generic_malloc_many   */
        /* (with GC_fl_builder_count > 0), and thread 3 is blocked  */
        /* acquiring the allocation lock in GC_resume_thread.       */
        if (GC_parallel)
          GC_wait_for_reclaim();
#     endif

      /* TODO: Support GC_retry_signals (not needed for TSan) */
      GC_acquire_dirty_lock();
      switch (RAISE_SIGNAL(t, GC_sig_suspend)) {
      /* ESRCH cannot happen as terminated threads are handled above.   */
      case 0:
        break;
      default:
        ABORT("pthread_kill failed");
      }

      /* Wait for the thread to complete threads table lookup and   */
      /* stack_ptr assignment.                                      */
      GC_ASSERT(GC_thr_initialized);
      while (sem_wait(&GC_suspend_ack_sem) != 0) {
        if (errno != EINTR)
          ABORT("sem_wait for handler failed (suspend_self)");
      }
      GC_release_dirty_lock();
      RESTORE_CANCEL(cancel_state);
      UNLOCK();
    }

    GC_API void GC_CALL GC_resume_thread(GC_SUSPEND_THREAD_ID thread) {
      GC_thread t;
      DCL_LOCK_STATE;

      LOCK();
      t = GC_lookup_thread((pthread_t)thread);
      if (t != NULL)
        AO_store(&t->suspended_ext, FALSE);
      UNLOCK();
    }

    GC_API int GC_CALL GC_is_thread_suspended(GC_SUSPEND_THREAD_ID thread) {
      GC_thread t;
      int is_suspended = 0;
      DCL_LOCK_STATE;

      LOCK();
      t = GC_lookup_thread((pthread_t)thread);
      if (t != NULL && t -> suspended_ext)
        is_suspended = (int)TRUE;
      UNLOCK();
      return is_suspended;
    }
# endif /* GC_ENABLE_SUSPEND_THREAD */

#endif /* !GC_OPENBSD_UTHREADS && !NACL */

#ifdef IA64
# define IF_IA64(x) x
#else
# define IF_IA64(x)
#endif
/* We hold allocation lock.  Should do exactly the right thing if the   */
/* world is stopped.  Should not fail if it isn't.                      */
GC_INNER void GC_push_all_stacks(void)
{
    GC_bool found_me = FALSE;
    size_t nthreads = 0;
    int i;
    GC_thread p;
    ptr_t lo, hi;
    /* On IA64, we also need to scan the register backing store. */
    IF_IA64(ptr_t bs_lo; ptr_t bs_hi;)
    struct GC_traced_stack_sect_s *traced_stack_sect;
    pthread_t self = pthread_self();
    word total_size = 0;

    if (!EXPECT(GC_thr_initialized, TRUE))
      GC_thr_init();
#   ifdef DEBUG_THREADS
      GC_log_printf("Pushing stacks from thread %p\n", (void *)self);
#   endif
    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        if (p -> flags & FINISHED) continue;
        ++nthreads;
        traced_stack_sect = p -> traced_stack_sect;
        if (THREAD_EQUAL(p -> id, self)) {
            GC_ASSERT(!p->thread_blocked);
#           ifdef SPARC
                lo = (ptr_t)GC_save_regs_in_stack();
#           else
                lo = GC_approx_sp();
#           endif
            found_me = TRUE;
            IF_IA64(bs_hi = (ptr_t)GC_save_regs_in_stack();)
        } else {
            lo = (ptr_t)AO_load((volatile AO_t *)&p->stop_info.stack_ptr);
            IF_IA64(bs_hi = p -> backing_store_ptr;)
            if (traced_stack_sect != NULL
                    && traced_stack_sect->saved_stack_ptr == lo) {
              /* If the thread has never been stopped since the recent  */
              /* GC_call_with_gc_active invocation then skip the top    */
              /* "stack section" as stack_ptr already points to.        */
              traced_stack_sect = traced_stack_sect->prev;
            }
        }
        if ((p -> flags & MAIN_THREAD) == 0) {
            hi = p -> stack_end;
            IF_IA64(bs_lo = p -> backing_store_end);
        } else {
            /* The original stack. */
            hi = GC_stackbottom;
            IF_IA64(bs_lo = BACKING_STORE_BASE;)
        }
#       ifdef DEBUG_THREADS
          GC_log_printf("Stack for thread %p = [%p,%p)\n",
                        (void *)p->id, (void *)lo, (void *)hi);
#       endif
        if (0 == lo) ABORT("GC_push_all_stacks: sp not set!");
        if (p->altstack != NULL && (word)p->altstack <= (word)lo
            && (word)lo <= (word)p->altstack + p->altstack_size) {
          hi = p->altstack + p->altstack_size;
          /* FIXME: Need to scan the normal stack too, but how ? */
          /* FIXME: Assume stack grows down */
        }
        GC_push_all_stack_sections(lo, hi, traced_stack_sect);
#       ifdef STACK_GROWS_UP
          total_size += lo - hi;
#       else
          total_size += hi - lo; /* lo <= hi */
#       endif
#       ifdef NACL
          /* Push reg_storage as roots, this will cover the reg context. */
          GC_push_all_stack((ptr_t)p -> stop_info.reg_storage,
              (ptr_t)(p -> stop_info.reg_storage + NACL_GC_REG_STORAGE_SIZE));
          total_size += NACL_GC_REG_STORAGE_SIZE * sizeof(ptr_t);
#       endif
#       ifdef IA64
#         ifdef DEBUG_THREADS
            GC_log_printf("Reg stack for thread %p = [%p,%p)\n",
                          (void *)p->id, (void *)bs_lo, (void *)bs_hi);
#         endif
          /* FIXME: This (if p->id==self) may add an unbounded number of */
          /* entries, and hence overflow the mark stack, which is bad.   */
          GC_push_all_register_sections(bs_lo, bs_hi,
                                        THREAD_EQUAL(p -> id, self),
                                        traced_stack_sect);
          total_size += bs_hi - bs_lo; /* bs_lo <= bs_hi */
#       endif
      }
    }
    GC_VERBOSE_LOG_PRINTF("Pushed %d thread stacks\n", (int)nthreads);
    if (!found_me && !GC_in_thread_creation)
      ABORT("Collecting from unknown thread");
    GC_total_stacksize = total_size;
}

#ifdef DEBUG_THREADS
  /* There seems to be a very rare thread stopping problem.  To help us */
  /* debug that, we save the ids of the stopping thread.                */
  pthread_t GC_stopping_thread;
  int GC_stopping_pid = 0;
#endif

/* We hold the allocation lock.  Suspend all threads that might */
/* still be running.  Return the number of suspend signals that */
/* were sent.                                                   */
STATIC int GC_suspend_all(void)
{
  int n_live_threads = 0;
  int i;
# ifndef NACL
    GC_thread p;
#   ifndef GC_OPENBSD_UTHREADS
      int result;
#   endif
    pthread_t self = pthread_self();

    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != 0; p = p -> next) {
        if (!THREAD_EQUAL(p -> id, self)) {
            if ((p -> flags & FINISHED) != 0) continue;
            if (p -> thread_blocked) /* Will wait */ continue;
#           ifndef GC_OPENBSD_UTHREADS
#             ifdef GC_ENABLE_SUSPEND_THREAD
                if (p -> suspended_ext) continue;
#             endif
              if (AO_load(&p->stop_info.last_stop_count) == GC_stop_count)
                continue; /* matters only if GC_retry_signals */
              n_live_threads++;
#           endif
#           ifdef DEBUG_THREADS
              GC_log_printf("Sending suspend signal to %p\n", (void *)p->id);
#           endif

#           ifdef GC_OPENBSD_UTHREADS
              {
                stack_t stack;

                GC_acquire_dirty_lock();
                if (pthread_suspend_np(p -> id) != 0)
                  ABORT("pthread_suspend_np failed");
                GC_release_dirty_lock();
                if (pthread_stackseg_np(p->id, &stack))
                  ABORT("pthread_stackseg_np failed");
                p -> stop_info.stack_ptr = (ptr_t)stack.ss_sp - stack.ss_size;
                if (GC_on_thread_event)
                  GC_on_thread_event(GC_EVENT_THREAD_SUSPENDED,
                                     (void *)p->id);
              }
#           else
              /* The synchronization between GC_dirty (based on         */
              /* test-and-set) and the signal-based thread suspension   */
              /* is performed in GC_stop_world because                  */
              /* GC_release_dirty_lock cannot be called before          */
              /* acknowledging the thread is really suspended.          */
              result = RAISE_SIGNAL(p, GC_sig_suspend);
              switch(result) {
                case ESRCH:
                    /* Not really there anymore.  Possible? */
                    n_live_threads--;
                    break;
                case 0:
                    if (GC_on_thread_event)
                      GC_on_thread_event(GC_EVENT_THREAD_SUSPENDED,
                                         (void *)(word)THREAD_SYSTEM_ID(p));
                                /* Note: thread id might be truncated.  */
                    break;
                default:
                    ABORT_ARG1("pthread_kill failed at suspend",
                               ": errcode= %d", result);
              }
#           endif
        }
      }
    }

# else /* NACL */
#   ifndef NACL_PARK_WAIT_NANOSECONDS
#     define NACL_PARK_WAIT_NANOSECONDS (100 * 1000)
#   endif
#   define NANOS_PER_SECOND (1000UL * 1000 * 1000)
    unsigned long num_sleeps = 0;

#   ifdef DEBUG_THREADS
      GC_log_printf("pthread_stop_world: num_threads=%d\n",
                    GC_nacl_num_gc_threads - 1);
#   endif
    GC_nacl_thread_parker = pthread_self();
    GC_nacl_park_threads_now = 1;

    while (1) {
      int num_threads_parked = 0;
      struct timespec ts;
      int num_used = 0;

      /* Check the 'parked' flag for each thread the GC knows about.    */
      for (i = 0; i < MAX_NACL_GC_THREADS
                  && num_used < GC_nacl_num_gc_threads; i++) {
        if (GC_nacl_thread_used[i] == 1) {
          num_used++;
          if (GC_nacl_thread_parked[i] == 1) {
            num_threads_parked++;
            if (GC_on_thread_event)
              GC_on_thread_event(GC_EVENT_THREAD_SUSPENDED, (void *)(word)i);
          }
        }
      }
      /* -1 for the current thread.     */
      if (num_threads_parked >= GC_nacl_num_gc_threads - 1)
        break;
      ts.tv_sec = 0;
      ts.tv_nsec = NACL_PARK_WAIT_NANOSECONDS;
#     ifdef DEBUG_THREADS
        GC_log_printf("Sleep waiting for %d threads to park...\n",
                      GC_nacl_num_gc_threads - num_threads_parked - 1);
#     endif
      /* This requires _POSIX_TIMERS feature.   */
      nanosleep(&ts, 0);
      if (++num_sleeps > NANOS_PER_SECOND / NACL_PARK_WAIT_NANOSECONDS) {
        WARN("GC appears stalled waiting for %" WARN_PRIdPTR
             " threads to park...\n",
             GC_nacl_num_gc_threads - num_threads_parked - 1);
        num_sleeps = 0;
      }
    }
# endif /* NACL */
  return n_live_threads;
}

GC_INNER void GC_stop_world(void)
{
# if !defined(GC_OPENBSD_UTHREADS) && !defined(NACL)
    int n_live_threads;
# endif
  GC_ASSERT(I_HOLD_LOCK());
# ifdef DEBUG_THREADS
    GC_stopping_thread = pthread_self();
    GC_stopping_pid = getpid();
    GC_log_printf("Stopping the world from %p\n", (void *)GC_stopping_thread);
# endif

  /* Make sure all free list construction has stopped before we start.  */
  /* No new construction can start, since free list construction is     */
  /* required to acquire and release the GC lock before it starts,      */
  /* and we have the lock.                                              */
# ifdef PARALLEL_MARK
    if (GC_parallel) {
      GC_acquire_mark_lock();
      GC_ASSERT(GC_fl_builder_count == 0);
      /* We should have previously waited for it to become zero.        */
    }
# endif /* PARALLEL_MARK */

# if defined(GC_OPENBSD_UTHREADS) || defined(NACL)
    (void)GC_suspend_all();
# else
    AO_store(&GC_stop_count, (AO_t)((word)GC_stop_count + 2));
        /* Only concurrent reads are possible. */
# ifdef MANUAL_VDB
    GC_acquire_dirty_lock();
    /* The write fault handler cannot be called if GC_manual_vdb      */
    /* (thus double-locking should not occur in                       */
    /* async_set_pht_entry_from_index based on test-and-set).         */
# endif
    AO_store_release(&GC_world_is_stopped, TRUE);
    n_live_threads = GC_suspend_all();
#ifndef UNITY_RETRY_SIGNALS
    if (GC_retry_signals)
      n_live_threads = resend_lost_signals(n_live_threads, GC_suspend_all);
    suspend_restart_barrier(n_live_threads);
#else
    if (GC_retry_signals)
        suspend_restart_barrier_retry(n_live_threads, GC_suspend_all);
    else
        suspend_restart_barrier(n_live_threads);
#endif
# ifdef MANUAL_VDB
    GC_release_dirty_lock(); /* cannot be done in GC_suspend_all */
# endif    
# endif

# ifdef PARALLEL_MARK
    if (GC_parallel)
      GC_release_mark_lock();
# endif
# ifdef DEBUG_THREADS
    GC_log_printf("World stopped from %p\n", (void *)pthread_self());
    GC_stopping_thread = 0;
# endif
}

#ifdef NACL
# if defined(__x86_64__)
#   define NACL_STORE_REGS() \
        do { \
          __asm__ __volatile__ ("push %rbx"); \
          __asm__ __volatile__ ("push %rbp"); \
          __asm__ __volatile__ ("push %r12"); \
          __asm__ __volatile__ ("push %r13"); \
          __asm__ __volatile__ ("push %r14"); \
          __asm__ __volatile__ ("push %r15"); \
          __asm__ __volatile__ ("mov %%esp, %0" \
                    : "=m" (GC_nacl_gc_thread_self->stop_info.stack_ptr)); \
          BCOPY(GC_nacl_gc_thread_self->stop_info.stack_ptr, \
                GC_nacl_gc_thread_self->stop_info.reg_storage, \
                NACL_GC_REG_STORAGE_SIZE * sizeof(ptr_t)); \
          __asm__ __volatile__ ("naclasp $48, %r15"); \
        } while (0)
# elif defined(__i386__)
#   define NACL_STORE_REGS() \
        do { \
          __asm__ __volatile__ ("push %ebx"); \
          __asm__ __volatile__ ("push %ebp"); \
          __asm__ __volatile__ ("push %esi"); \
          __asm__ __volatile__ ("push %edi"); \
          __asm__ __volatile__ ("mov %%esp, %0" \
                    : "=m" (GC_nacl_gc_thread_self->stop_info.stack_ptr)); \
          BCOPY(GC_nacl_gc_thread_self->stop_info.stack_ptr, \
                GC_nacl_gc_thread_self->stop_info.reg_storage, \
                NACL_GC_REG_STORAGE_SIZE * sizeof(ptr_t));\
          __asm__ __volatile__ ("add $16, %esp"); \
        } while (0)
# elif defined(__arm__)
#   define NACL_STORE_REGS() \
        do { \
          __asm__ __volatile__ ("push {r4-r8,r10-r12,lr}"); \
          __asm__ __volatile__ ("mov r0, %0" \
                : : "r" (&GC_nacl_gc_thread_self->stop_info.stack_ptr)); \
          __asm__ __volatile__ ("bic r0, r0, #0xc0000000"); \
          __asm__ __volatile__ ("str sp, [r0]"); \
          BCOPY(GC_nacl_gc_thread_self->stop_info.stack_ptr, \
                GC_nacl_gc_thread_self->stop_info.reg_storage, \
                NACL_GC_REG_STORAGE_SIZE * sizeof(ptr_t)); \
          __asm__ __volatile__ ("add sp, sp, #40"); \
          __asm__ __volatile__ ("bic sp, sp, #0xc0000000"); \
        } while (0)
# else
#   error TODO Please port NACL_STORE_REGS
# endif

  GC_API_OSCALL void nacl_pre_syscall_hook(void)
  {
    if (GC_nacl_thread_idx != -1) {
      NACL_STORE_REGS();
      GC_nacl_gc_thread_self->stop_info.stack_ptr = GC_approx_sp();
      GC_nacl_thread_parked[GC_nacl_thread_idx] = 1;
    }
  }

  GC_API_OSCALL void __nacl_suspend_thread_if_needed(void)
  {
    if (GC_nacl_park_threads_now) {
      pthread_t self = pthread_self();

      /* Don't try to park the thread parker.   */
      if (GC_nacl_thread_parker == self)
        return;

      /* This can happen when a thread is created outside of the GC     */
      /* system (wthread mostly).                                       */
      if (GC_nacl_thread_idx < 0)
        return;

      /* If it was already 'parked', we're returning from a syscall,    */
      /* so don't bother storing registers again, the GC has a set.     */
      if (!GC_nacl_thread_parked[GC_nacl_thread_idx]) {
        NACL_STORE_REGS();
        GC_nacl_gc_thread_self->stop_info.stack_ptr = GC_approx_sp();
      }
      GC_nacl_thread_parked[GC_nacl_thread_idx] = 1;
      while (GC_nacl_park_threads_now) {
        /* Just spin.   */
      }
      GC_nacl_thread_parked[GC_nacl_thread_idx] = 0;

      /* Clear out the reg storage for next suspend.    */
      BZERO(GC_nacl_gc_thread_self->stop_info.reg_storage,
            NACL_GC_REG_STORAGE_SIZE * sizeof(ptr_t));
    }
  }

  GC_API_OSCALL void nacl_post_syscall_hook(void)
  {
    /* Calling __nacl_suspend_thread_if_needed right away should        */
    /* guarantee we don't mutate the GC set.                            */
    __nacl_suspend_thread_if_needed();
    if (GC_nacl_thread_idx != -1) {
      GC_nacl_thread_parked[GC_nacl_thread_idx] = 0;
    }
  }

  STATIC GC_bool GC_nacl_thread_parking_inited = FALSE;
  STATIC pthread_mutex_t GC_nacl_thread_alloc_lock = PTHREAD_MUTEX_INITIALIZER;

  struct nacl_irt_blockhook {
    int (*register_block_hooks)(void (*pre)(void), void (*post)(void));
  };

  EXTERN_C_BEGIN
  extern size_t nacl_interface_query(const char *interface_ident,
                                     void *table, size_t tablesize);
  EXTERN_C_END

  GC_INNER void GC_nacl_initialize_gc_thread(void)
  {
    int i;
    static struct nacl_irt_blockhook gc_hook;

    pthread_mutex_lock(&GC_nacl_thread_alloc_lock);
    if (!EXPECT(GC_nacl_thread_parking_inited, TRUE)) {
      BZERO(GC_nacl_thread_parked, sizeof(GC_nacl_thread_parked));
      BZERO(GC_nacl_thread_used, sizeof(GC_nacl_thread_used));
      /* TODO: replace with public 'register hook' function when        */
      /* available from glibc.                                          */
      nacl_interface_query("nacl-irt-blockhook-0.1",
                           &gc_hook, sizeof(gc_hook));
      gc_hook.register_block_hooks(nacl_pre_syscall_hook,
                                   nacl_post_syscall_hook);
      GC_nacl_thread_parking_inited = TRUE;
    }
    GC_ASSERT(GC_nacl_num_gc_threads <= MAX_NACL_GC_THREADS);
    for (i = 0; i < MAX_NACL_GC_THREADS; i++) {
      if (GC_nacl_thread_used[i] == 0) {
        GC_nacl_thread_used[i] = 1;
        GC_nacl_thread_idx = i;
        GC_nacl_num_gc_threads++;
        break;
      }
    }
    pthread_mutex_unlock(&GC_nacl_thread_alloc_lock);
  }

  GC_INNER void GC_nacl_shutdown_gc_thread(void)
  {
    pthread_mutex_lock(&GC_nacl_thread_alloc_lock);
    GC_ASSERT(GC_nacl_thread_idx >= 0);
    GC_ASSERT(GC_nacl_thread_idx < MAX_NACL_GC_THREADS);
    GC_ASSERT(GC_nacl_thread_used[GC_nacl_thread_idx] != 0);
    GC_nacl_thread_used[GC_nacl_thread_idx] = 0;
    GC_nacl_thread_idx = -1;
    GC_nacl_num_gc_threads--;
    pthread_mutex_unlock(&GC_nacl_thread_alloc_lock);
  }

#else /* !NACL */

  /* Restart all threads that were suspended by the collector.  */
  /* Return the number of restart signals that were sent.       */
  STATIC int GC_restart_all(void)
  {
    int n_live_threads = 0;
    int i;
    pthread_t self = pthread_self();
    GC_thread p;
#   ifndef GC_OPENBSD_UTHREADS
      int result;
#   endif

    for (i = 0; i < THREAD_TABLE_SZ; i++) {
      for (p = GC_threads[i]; p != NULL; p = p -> next) {
        if (!THREAD_EQUAL(p -> id, self)) {
          if ((p -> flags & FINISHED) != 0) continue;
          if (p -> thread_blocked) continue;
#         ifndef GC_OPENBSD_UTHREADS
#           ifdef GC_ENABLE_SUSPEND_THREAD
              if (p -> suspended_ext) continue;
#           endif
            if (GC_retry_signals && AO_load(&p->stop_info.last_stop_count)
                                    == (AO_t)((word)GC_stop_count | 1))
              continue; /* The thread has been restarted. */
            n_live_threads++;
#         endif
#         ifdef DEBUG_THREADS
            GC_log_printf("Sending restart signal to %p\n", (void *)p->id);
#         endif
#         ifdef GC_OPENBSD_UTHREADS
            if (pthread_resume_np(p -> id) != 0)
              ABORT("pthread_resume_np failed");
            if (GC_on_thread_event)
              GC_on_thread_event(GC_EVENT_THREAD_UNSUSPENDED, (void *)p->id);
#         else
            result = RAISE_SIGNAL(p, GC_sig_thr_restart);
            switch(result) {
            case ESRCH:
              /* Not really there anymore.  Possible?   */
              n_live_threads--;
              break;
            case 0:
              if (GC_on_thread_event)
                GC_on_thread_event(GC_EVENT_THREAD_UNSUSPENDED,
                                   (void *)(word)THREAD_SYSTEM_ID(p));
              break;
            default:
              ABORT_ARG1("pthread_kill failed at resume",
                         ": errcode= %d", result);
            }
#         endif
        }
      }
    }
    return n_live_threads;
  }
#endif /* !NACL */

/* Caller holds allocation lock, and has held it continuously since     */
/* the world stopped.                                                   */
GC_INNER void GC_start_world(void)
{
# ifndef NACL
    int n_live_threads;

    GC_ASSERT(I_HOLD_LOCK());
#   ifdef DEBUG_THREADS
      GC_log_printf("World starting\n");
#   endif
#   ifndef GC_OPENBSD_UTHREADS
      AO_store_release(&GC_world_is_stopped, FALSE);
                    /* The updated value should now be visible to the   */
                    /* signal handler (note that pthread_kill is not on */
                    /* the list of functions which synchronize memory). */
#   endif
    n_live_threads = GC_restart_all();
#   ifndef GC_OPENBSD_UTHREADS
#   ifndef UNITY_RETRY_SIGNALS
      if (GC_retry_signals)
        n_live_threads = resend_lost_signals(n_live_threads, GC_restart_all);
#   endif
#     ifdef GC_NETBSD_THREADS_WORKAROUND
        suspend_restart_barrier(n_live_threads);
#     else
        if (GC_retry_signals)
#       ifndef UNITY_RETRY_SIGNALS
          suspend_restart_barrier(n_live_threads);
#       else
          suspend_restart_barrier_retry(n_live_threads, GC_restart_all);
#       endif

#     endif
#   else
      (void)n_live_threads;
#   endif
#   ifdef DEBUG_THREADS
      GC_log_printf("World started\n");
#   endif
# else /* NACL */
#   ifdef DEBUG_THREADS
      GC_log_printf("World starting...\n");
#   endif
    GC_nacl_park_threads_now = 0;
    if (GC_on_thread_event)
      GC_on_thread_event(GC_EVENT_THREAD_UNSUSPENDED, NULL);
      /* TODO: Send event for every unsuspended thread. */
# endif
}

GC_INNER void GC_stop_init(void)
{
# if !defined(GC_OPENBSD_UTHREADS) && !defined(NACL)
    struct sigaction act;
    char *str;

    if (SIGNAL_UNSET == GC_sig_suspend)
        GC_sig_suspend = SIG_SUSPEND;
    if (SIGNAL_UNSET == GC_sig_thr_restart)
        GC_sig_thr_restart = SIG_THR_RESTART;
    if (GC_sig_suspend == GC_sig_thr_restart)
        ABORT("Cannot use same signal for thread suspend and resume");

    if (sem_init(&GC_suspend_ack_sem, GC_SEM_INIT_PSHARED, 0) != 0)
        ABORT("sem_init failed");

#   ifdef SA_RESTART
      act.sa_flags = SA_RESTART
#   else
      act.sa_flags = 0
#   endif
#   ifndef NO_SA_SIGACTION
                     | SA_SIGINFO
#   endif
        ;
    if (sigfillset(&act.sa_mask) != 0) {
        ABORT("sigfillset failed");
    }
#   ifdef GC_RTEMS_PTHREADS
      if(sigprocmask(SIG_UNBLOCK, &act.sa_mask, NULL) != 0) {
        ABORT("sigprocmask failed");
      }
#   endif
    GC_remove_allowed_signals(&act.sa_mask);
    /* GC_sig_thr_restart is set in the resulting mask. */
    /* It is unmasked by the handler when necessary.    */
#   ifndef NO_SA_SIGACTION
      act.sa_sigaction = GC_suspend_handler;
#   else
      act.sa_handler = GC_suspend_handler;
#   endif
    /* act.sa_restorer is deprecated and should not be initialized. */
    if (sigaction(GC_sig_suspend, &act, NULL) != 0) {
        ABORT("Cannot set SIG_SUSPEND handler");
    }

#   ifndef NO_SA_SIGACTION
      act.sa_flags &= ~SA_SIGINFO;
#   endif
    act.sa_handler = GC_restart_handler;
    if (sigaction(GC_sig_thr_restart, &act, NULL) != 0) {
        ABORT("Cannot set SIG_THR_RESTART handler");
    }

    /* Initialize suspend_handler_mask (excluding GC_sig_thr_restart).  */
    if (sigfillset(&suspend_handler_mask) != 0) ABORT("sigfillset failed");
    GC_remove_allowed_signals(&suspend_handler_mask);
    if (sigdelset(&suspend_handler_mask, GC_sig_thr_restart) != 0)
        ABORT("sigdelset failed");

    /* Override the default value of GC_retry_signals.  */
    str = GETENV("GC_RETRY_SIGNALS");
    if (str != NULL) {
        if (*str == '0' && *(str + 1) == '\0') {
            /* Do not retry if the environment variable is set to "0". */
            GC_retry_signals = FALSE;
        } else {
            GC_retry_signals = TRUE;
        }
    }
    if (GC_retry_signals) {
      GC_COND_LOG_PRINTF(
                "Will retry suspend and restart signals if necessary\n");
    }
#   ifndef NO_SIGNALS_UNBLOCK_IN_MAIN
      /* Explicitly unblock the signals once before new threads creation. */
      GC_unblock_gc_signals();
#   endif
# endif /* !GC_OPENBSD_UTHREADS && !NACL */
}

#endif /* GC_PTHREADS && !GC_DARWIN_THREADS && !GC_WIN32_THREADS */
