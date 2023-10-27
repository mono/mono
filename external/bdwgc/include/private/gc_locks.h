/*
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996-1999 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 1999 by Hewlett-Packard Company. All rights reserved.
 *
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

#ifndef GC_LOCKS_H
#define GC_LOCKS_H

/*
 * Mutual exclusion between allocator/collector routines.
 * Needed if there is more than one allocator thread.
 * DCL_LOCK_STATE declares any local variables needed by LOCK and UNLOCK.
 *
 * Note that I_HOLD_LOCK and I_DONT_HOLD_LOCK are used only positively
 * in assertions, and may return TRUE in the "don't know" case.
 */
# ifdef THREADS

#  ifdef PCR
#    include <base/PCR_Base.h>
#    include <th/PCR_Th.h>
#  endif

   EXTERN_C_BEGIN

#  ifdef PCR
     GC_EXTERN PCR_Th_ML GC_allocate_ml;
#    if defined(CPPCHECK)
#      define DCL_LOCK_STATE /* empty */
#    else
#      define DCL_LOCK_STATE \
         PCR_ERes GC_fastLockRes; PCR_sigset_t GC_old_sig_mask
#    endif
#    define UNCOND_LOCK() PCR_Th_ML_Acquire(&GC_allocate_ml)
#    define UNCOND_UNLOCK() PCR_Th_ML_Release(&GC_allocate_ml)
#  endif

#  if (!defined(AO_HAVE_test_and_set_acquire) || defined(GC_RTEMS_PTHREADS) \
       || defined(SN_TARGET_PS3) \
       || defined(GC_WIN32_THREADS) || defined(LINT2)) && defined(GC_PTHREADS)
#    define USE_PTHREAD_LOCKS
#    undef USE_SPIN_LOCK
#  endif

#  if defined(GC_WIN32_THREADS) && !defined(USE_PTHREAD_LOCKS)
#    ifndef WIN32_LEAN_AND_MEAN
#      define WIN32_LEAN_AND_MEAN 1
#    endif
#    define NOSERVICE
     EXTERN_C_END
#    include <windows.h>
     EXTERN_C_BEGIN
#    define NO_THREAD (DWORD)(-1)
     GC_EXTERN CRITICAL_SECTION GC_allocate_ml;
#    ifdef GC_ASSERTIONS
       GC_EXTERN DWORD GC_lock_holder;
#      define SET_LOCK_HOLDER() GC_lock_holder = GetCurrentThreadId()
#      define UNSET_LOCK_HOLDER() GC_lock_holder = NO_THREAD
#      define I_HOLD_LOCK() (!GC_need_to_lock \
                           || GC_lock_holder == GetCurrentThreadId())
#      ifdef THREAD_SANITIZER
#        define I_DONT_HOLD_LOCK() TRUE /* Conservatively say yes */
#      else
#        define I_DONT_HOLD_LOCK() (!GC_need_to_lock \
                           || GC_lock_holder != GetCurrentThreadId())
#      endif
#      define UNCOND_LOCK() \
                { GC_ASSERT(I_DONT_HOLD_LOCK()); \
                  EnterCriticalSection(&GC_allocate_ml); \
                  SET_LOCK_HOLDER(); }
#      define UNCOND_UNLOCK() \
                { GC_ASSERT(I_HOLD_LOCK()); UNSET_LOCK_HOLDER(); \
                  LeaveCriticalSection(&GC_allocate_ml); }
#    else
#      define UNCOND_LOCK() EnterCriticalSection(&GC_allocate_ml)
#      define UNCOND_UNLOCK() LeaveCriticalSection(&GC_allocate_ml)
#    endif /* !GC_ASSERTIONS */
#  elif defined(GC_PTHREADS)
     EXTERN_C_END
#    include <pthread.h>
     EXTERN_C_BEGIN
     /* Posix allows pthread_t to be a struct, though it rarely is.     */
     /* Unfortunately, we need to use a pthread_t to index a data       */
     /* structure.  It also helps if comparisons don't involve a        */
     /* function call.  Hence we introduce platform-dependent macros    */
     /* to compare pthread_t ids and to map them to integers.           */
     /* The mapping to integers does not need to result in different    */
     /* integers for each thread, though that should be true as much    */
     /* as possible.                                                    */
     /* Refine to exclude platforms on which pthread_t is struct.       */
#    if !defined(GC_WIN32_PTHREADS)
#      define NUMERIC_THREAD_ID(id) ((unsigned long)(id))
#      define THREAD_EQUAL(id1, id2) ((id1) == (id2))
#      define NUMERIC_THREAD_ID_UNIQUE
#    elif defined(__WINPTHREADS_VERSION_MAJOR) /* winpthreads */
#      define NUMERIC_THREAD_ID(id) ((unsigned long)(id))
#      define THREAD_EQUAL(id1, id2) ((id1) == (id2))
#      ifndef _WIN64
         /* NUMERIC_THREAD_ID is 32-bit and not unique on Win64. */
#        define NUMERIC_THREAD_ID_UNIQUE
#      endif
#    else /* pthreads-win32 */
#      define NUMERIC_THREAD_ID(id) ((unsigned long)(word)(id.p))
       /* Using documented internal details of pthreads-win32 library.  */
       /* Faster than pthread_equal(). Should not change with           */
       /* future versions of pthreads-win32 library.                    */
#      define THREAD_EQUAL(id1, id2) ((id1.p == id2.p) && (id1.x == id2.x))
#      undef NUMERIC_THREAD_ID_UNIQUE
       /* Generic definitions based on pthread_equal() always work but  */
       /* will result in poor performance (as NUMERIC_THREAD_ID is      */
       /* defined to just a constant) and weak assertion checking.      */
#    endif
#    define NO_THREAD ((unsigned long)(-1l))
                /* != NUMERIC_THREAD_ID(pthread_self()) for any thread */

#    ifdef SN_TARGET_PSP2
       EXTERN_C_END
#      include "psp2-support.h"
       EXTERN_C_BEGIN
       GC_EXTERN WapiMutex GC_allocate_ml_PSP2;
#      define UNCOND_LOCK() { int res; GC_ASSERT(I_DONT_HOLD_LOCK()); \
                              res = PSP2_MutexLock(&GC_allocate_ml_PSP2); \
                              GC_ASSERT(0 == res); (void)res; \
                              SET_LOCK_HOLDER(); }
#      define UNCOND_UNLOCK() { int res; GC_ASSERT(I_HOLD_LOCK()); \
                              UNSET_LOCK_HOLDER(); \
                              res = PSP2_MutexUnlock(&GC_allocate_ml_PSP2); \
                              GC_ASSERT(0 == res); (void)res; }

#    elif (!defined(THREAD_LOCAL_ALLOC) || defined(USE_SPIN_LOCK)) \
          && !defined(USE_PTHREAD_LOCKS)
      /* In the THREAD_LOCAL_ALLOC case, the allocation lock tends to   */
      /* be held for long periods, if it is held at all.  Thus spinning */
      /* and sleeping for fixed periods are likely to result in         */
      /* significant wasted time.  We thus rely mostly on queued locks. */
#     undef USE_SPIN_LOCK
#     define USE_SPIN_LOCK
      GC_EXTERN volatile AO_TS_t GC_allocate_lock;
      GC_INNER void GC_lock(void);
        /* Allocation lock holder.  Only set if acquired by client through */
        /* GC_call_with_alloc_lock.                                        */
#     ifdef GC_ASSERTIONS
#        define UNCOND_LOCK() \
              { GC_ASSERT(I_DONT_HOLD_LOCK()); \
                if (AO_test_and_set_acquire(&GC_allocate_lock) == AO_TS_SET) \
                  GC_lock(); \
                SET_LOCK_HOLDER(); }
#        define UNCOND_UNLOCK() \
              { GC_ASSERT(I_HOLD_LOCK()); UNSET_LOCK_HOLDER(); \
                AO_CLEAR(&GC_allocate_lock); }
#     else
#        define UNCOND_LOCK() \
              { if (AO_test_and_set_acquire(&GC_allocate_lock) == AO_TS_SET) \
                  GC_lock(); }
#        define UNCOND_UNLOCK() AO_CLEAR(&GC_allocate_lock)
#     endif /* !GC_ASSERTIONS */
#    else /* THREAD_LOCAL_ALLOC || USE_PTHREAD_LOCKS */
#      ifndef USE_PTHREAD_LOCKS
#        define USE_PTHREAD_LOCKS
#      endif
#    endif /* THREAD_LOCAL_ALLOC || USE_PTHREAD_LOCKS */
#    ifdef USE_PTHREAD_LOCKS
       EXTERN_C_END
#      include <pthread.h>
       EXTERN_C_BEGIN
       GC_EXTERN pthread_mutex_t GC_allocate_ml;
#      ifdef GC_ASSERTIONS
#        define UNCOND_LOCK() { GC_ASSERT(I_DONT_HOLD_LOCK()); \
                                GC_lock(); SET_LOCK_HOLDER(); }
#        define UNCOND_UNLOCK() \
                { GC_ASSERT(I_HOLD_LOCK()); UNSET_LOCK_HOLDER(); \
                  pthread_mutex_unlock(&GC_allocate_ml); }
#      else /* !GC_ASSERTIONS */
#        if defined(NO_PTHREAD_TRYLOCK)
#          define UNCOND_LOCK() pthread_mutex_lock(&GC_allocate_ml)
#        else
#          define UNCOND_LOCK() \
              { if (0 != pthread_mutex_trylock(&GC_allocate_ml)) \
                  GC_lock(); }
#        endif
#        define UNCOND_UNLOCK() pthread_mutex_unlock(&GC_allocate_ml)
#      endif /* !GC_ASSERTIONS */
#    endif /* USE_PTHREAD_LOCKS */
#    ifdef GC_ASSERTIONS
       GC_EXTERN unsigned long GC_lock_holder;
#      define SET_LOCK_HOLDER() \
                GC_lock_holder = NUMERIC_THREAD_ID(pthread_self())
#      define UNSET_LOCK_HOLDER() GC_lock_holder = NO_THREAD
#      define I_HOLD_LOCK() \
                (!GC_need_to_lock \
                 || GC_lock_holder == NUMERIC_THREAD_ID(pthread_self()))
#      if !defined(NUMERIC_THREAD_ID_UNIQUE) || defined(THREAD_SANITIZER)
#        define I_DONT_HOLD_LOCK() TRUE /* Conservatively say yes */
#      else
#        define I_DONT_HOLD_LOCK() \
                (!GC_need_to_lock \
                 || GC_lock_holder != NUMERIC_THREAD_ID(pthread_self()))
#      endif
#    endif /* GC_ASSERTIONS */
#    ifndef GC_WIN32_THREADS
       GC_EXTERN volatile GC_bool GC_collecting;
#      ifdef AO_HAVE_char_store
#        define ENTER_GC() AO_char_store((unsigned char*)&GC_collecting, TRUE)
#        define EXIT_GC() AO_char_store((unsigned char*)&GC_collecting, FALSE)
#      else
#        define ENTER_GC() (void)(GC_collecting = TRUE)
#        define EXIT_GC() (void)(GC_collecting = FALSE)
#      endif
#    endif
     GC_INNER void GC_lock(void);
#  endif /* GC_PTHREADS */
#  if defined(GC_ALWAYS_MULTITHREADED) \
      && (defined(USE_PTHREAD_LOCKS) || defined(USE_SPIN_LOCK))
#    define GC_need_to_lock TRUE
#    define set_need_to_lock() (void)0
#  else
#    if defined(GC_ALWAYS_MULTITHREADED) && !defined(CPPCHECK)
#      error Runtime initialization of GC lock is needed!
#    endif
#    undef GC_ALWAYS_MULTITHREADED
     GC_EXTERN GC_bool GC_need_to_lock;
#    ifdef THREAD_SANITIZER
        /* To workaround TSan false positive (e.g., when                */
        /* GC_pthread_create is called from multiple threads in         */
        /* parallel), do not set GC_need_to_lock if it is already set.  */
#       define set_need_to_lock() \
                (void)(*(GC_bool volatile *)&GC_need_to_lock \
                        ? FALSE \
                        : (GC_need_to_lock = TRUE))
#    else
#       define set_need_to_lock() (void)(GC_need_to_lock = TRUE)
                                        /* We are multi-threaded now.   */
#    endif
#  endif

#if !defined(UNCOND_LOCK)
       extern void GC_lock(void);
       extern void GC_unlock(void);
#      define UNCOND_LOCK() GC_lock()
#      define UNCOND_UNLOCK() GC_unlock()
#endif

   EXTERN_C_END

# else /* !THREADS */
#   define LOCK() (void)0
#   define UNLOCK() (void)0
#   ifdef GC_ASSERTIONS
#     define I_HOLD_LOCK() TRUE
#     define I_DONT_HOLD_LOCK() TRUE
                /* Used only in positive assertions or to test whether  */
                /* we still need to acquire the lock.  TRUE works in    */
                /* either case.                                         */
#   endif
# endif /* !THREADS */

#if defined(UNCOND_LOCK) && !defined(LOCK)
# if (defined(LINT2) && defined(USE_PTHREAD_LOCKS)) \
     || defined(GC_ALWAYS_MULTITHREADED)
    /* Instruct code analysis tools not to care about GC_need_to_lock   */
    /* influence to LOCK/UNLOCK semantic.                               */
#   define LOCK() UNCOND_LOCK()
#   define UNLOCK() UNCOND_UNLOCK()
# else
                /* At least two thread running; need to lock.   */
#   define LOCK() do { if (GC_need_to_lock) UNCOND_LOCK(); } while (0)
#   define UNLOCK() do { if (GC_need_to_lock) UNCOND_UNLOCK(); } while (0)
# endif
#endif

# ifndef ENTER_GC
#   define ENTER_GC()
#   define EXIT_GC()
# endif

# ifndef DCL_LOCK_STATE
#   define DCL_LOCK_STATE
# endif

#endif /* GC_LOCKS_H */
