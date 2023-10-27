/*
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996 by Silicon Graphics.  All rights reserved.
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
/* An incomplete test for the garbage collector.                */
/* Some more obscure entry points are not tested at all.        */
/* This must be compiled with the same flags used to build the  */
/* GC.  It uses GC internals to allow more precise results      */
/* checking for some of the tests.                              */

# ifdef HAVE_CONFIG_H
#   include "config.h"
# endif

# undef GC_BUILD

#if (defined(DBG_HDRS_ALL) || defined(MAKE_BACK_GRAPH)) \
    && !defined(GC_DEBUG) && !defined(CPPCHECK)
#  define GC_DEBUG
#endif

#ifdef DEFAULT_VDB /* specified manually (e.g. passed to CFLAGS) */
# define TEST_DEFAULT_VDB
#endif

#if defined(CPPCHECK) && defined(GC_PTHREADS) && !defined(_GNU_SOURCE)
# define _GNU_SOURCE 1
#endif
#undef GC_NO_THREAD_REDIRECTS
#include "gc.h"

#ifndef NTHREADS /* Number of additional threads to fork. */
#  define NTHREADS 5 /* excludes main thread, which also runs a test. */
        /* Not respected by PCR test. */
#endif

# if defined(mips) && defined(SYSTYPE_BSD43)
    /* MIPS RISCOS 4 */
# else
#   include <stdlib.h>
# endif
# include <stdio.h>
# if defined(_WIN32_WCE) && !defined(__GNUC__)
#   include <winbase.h>
/* #   define assert ASSERT */
# else
#   include <assert.h>  /* Not normally used, but handy for debugging.  */
# endif

# include "gc_typed.h"
# include "private/gc_priv.h"   /* For output, locking, MIN_WORDS,      */
                                /* some statistics and gcconfig.h.      */

# if defined(MSWIN32) || defined(MSWINCE)
#   include <windows.h>
# endif

#ifdef GC_PRINT_VERBOSE_STATS
# define print_stats VERBOSE
# define INIT_PRINT_STATS /* empty */
#else
  /* Use own variable as GC_print_stats might not be exported.  */
  static int print_stats = 0;
# ifdef GC_READ_ENV_FILE
    /* GETENV uses GC internal function in this case.   */
#   define INIT_PRINT_STATS /* empty */
# else
#   define INIT_PRINT_STATS \
        { \
          if (0 != GETENV("GC_PRINT_VERBOSE_STATS")) \
            print_stats = VERBOSE; \
          else if (0 != GETENV("GC_PRINT_STATS")) \
            print_stats = 1; \
        }
# endif
#endif /* !GC_PRINT_VERBOSE_STATS */

# ifdef PCR
#   include "th/PCR_ThCrSec.h"
#   include "th/PCR_Th.h"
#   define GC_printf printf
# endif

# if defined(GC_PTHREADS) && !defined(GC_WIN32_PTHREADS)
#   include <pthread.h>
# else
#   define NO_TEST_HANDLE_FORK
# endif

# if (!defined(THREADS) || !defined(HANDLE_FORK) \
      || (defined(DARWIN) && defined(MPROTECT_VDB) \
          && !defined(NO_INCREMENTAL) && !defined(MAKE_BACK_GRAPH))) \
     && !defined(NO_TEST_HANDLE_FORK) && !defined(TEST_HANDLE_FORK) \
     && !defined(TEST_FORK_WITHOUT_ATFORK)
#   define NO_TEST_HANDLE_FORK
# endif

# ifndef NO_TEST_HANDLE_FORK
#   include <unistd.h>
#   include <sys/types.h>
#   include <sys/wait.h>
#   if defined(HANDLE_FORK) && defined(CAN_CALL_ATFORK)
#     define INIT_FORK_SUPPORT GC_set_handle_fork(1)
                /* Causes abort in GC_init on pthread_atfork failure.   */
#   elif !defined(TEST_FORK_WITHOUT_ATFORK)
#     define INIT_FORK_SUPPORT GC_set_handle_fork(-1)
                /* Passing -1 implies fork() should be as well manually */
                /* surrounded with GC_atfork_prepare/parent/child.      */
#   endif
# endif

# ifndef INIT_FORK_SUPPORT
#   define INIT_FORK_SUPPORT /* empty */
# endif

#ifdef PCR
# define FINALIZER_LOCK() PCR_ThCrSec_EnterSys()
# define FINALIZER_UNLOCK() PCR_ThCrSec_ExitSys()
#elif defined(GC_PTHREADS)
  static pthread_mutex_t incr_lock = PTHREAD_MUTEX_INITIALIZER;
# define FINALIZER_LOCK() pthread_mutex_lock(&incr_lock)
# define FINALIZER_UNLOCK() pthread_mutex_unlock(&incr_lock)
#elif defined(GC_WIN32_THREADS)
  static CRITICAL_SECTION incr_cs;
# define FINALIZER_LOCK() EnterCriticalSection(&incr_cs)
# define FINALIZER_UNLOCK() LeaveCriticalSection(&incr_cs)
#else
# define FINALIZER_LOCK() (void)0
# define FINALIZER_UNLOCK() (void)0
#endif /* !THREADS */

#include <stdarg.h>

#define CHECK_GCLIB_VERSION \
            if (GC_get_version() != ((GC_VERSION_MAJOR<<16) \
                                    | (GC_VERSION_MINOR<<8) \
                                    | GC_VERSION_MICRO)) { \
              GC_printf("libgc version mismatch\n"); \
              exit(1); \
            }

/* Call GC_INIT only on platforms on which we think we really need it,  */
/* so that we can test automatic initialization on the rest.            */
#if defined(TEST_EXPLICIT_GC_INIT) || defined(AIX) || defined(CYGWIN32) \
        || defined(DARWIN) || defined(HOST_ANDROID) \
        || (defined(MSWINCE) && !defined(GC_WINMAIN_REDIRECT))
#  define GC_OPT_INIT GC_INIT()
#else
#  define GC_OPT_INIT /* empty */
#endif

#ifdef NO_CLOCK
# define INIT_PERF_MEASUREMENT (void)0
#else
# define INIT_PERF_MEASUREMENT GC_start_performance_measurement()
#endif

#define GC_COND_INIT() \
    INIT_FORK_SUPPORT; GC_OPT_INIT; CHECK_GCLIB_VERSION; \
    INIT_PRINT_STATS; INIT_PERF_MEASUREMENT

#define CHECK_OUT_OF_MEMORY(p) \
            if ((p) == NULL) { \
              GC_printf("Out of memory\n"); \
              exit(1); \
            }

/* Define AO primitives for a single-threaded mode. */
#ifndef AO_CLEAR
  /* AO_t not defined. */
# define AO_t GC_word
#endif
#ifndef AO_HAVE_load_acquire
  static AO_t AO_load_acquire(const volatile AO_t *addr)
  {
    AO_t result;

    FINALIZER_LOCK();
    result = *addr;
    FINALIZER_UNLOCK();
    return result;
  }
#endif
#ifndef AO_HAVE_store_release
  /* Not a macro as new_val argument should be evaluated before the lock. */
  static void AO_store_release(volatile AO_t *addr, AO_t new_val)
  {
    FINALIZER_LOCK();
    *addr = new_val;
    FINALIZER_UNLOCK();
  }
#endif
#ifndef AO_HAVE_fetch_and_add1
# define AO_fetch_and_add1(p) ((*(p))++)
                /* This is used only to update counters.        */
#endif

/* Allocation Statistics.  Synchronization is not strictly necessary.   */
volatile AO_t uncollectable_count = 0;
volatile AO_t collectable_count = 0;
volatile AO_t atomic_count = 0;
volatile AO_t realloc_count = 0;

volatile AO_t extra_count = 0;  /* Amount of space wasted in cons node; */
                                /* also used in gcj_cons, mktree and    */
                                /* chktree (for other purposes).        */

#if defined(GC_AMIGA_FASTALLOC) && defined(AMIGA)
  EXTERN_C_BEGIN
  void GC_amiga_free_all_mem(void);
  EXTERN_C_END

  void Amiga_Fail(void){GC_amiga_free_all_mem();abort();}
# define FAIL Amiga_Fail()
  void *GC_amiga_gctest_malloc_explicitly_typed(size_t lb, GC_descr d){
    void *ret=GC_malloc_explicitly_typed(lb,d);
    if(ret==NULL){
              GC_gcollect();
              ret=GC_malloc_explicitly_typed(lb,d);
      if(ret==NULL){
        GC_printf("Out of memory, (typed allocations are not directly "
                      "supported with the GC_AMIGA_FASTALLOC option.)\n");
        FAIL;
      }
    }
    return ret;
  }
  void *GC_amiga_gctest_calloc_explicitly_typed(size_t a,size_t lb, GC_descr d){
    void *ret=GC_calloc_explicitly_typed(a,lb,d);
    if(ret==NULL){
              GC_gcollect();
              ret=GC_calloc_explicitly_typed(a,lb,d);
      if(ret==NULL){
        GC_printf("Out of memory, (typed allocations are not directly "
                      "supported with the GC_AMIGA_FASTALLOC option.)\n");
        FAIL;
      }
    }
    return ret;
  }
# define GC_malloc_explicitly_typed(a,b) GC_amiga_gctest_malloc_explicitly_typed(a,b)
# define GC_calloc_explicitly_typed(a,b,c) GC_amiga_gctest_calloc_explicitly_typed(a,b,c)

#else /* !AMIGA_FASTALLOC */

# if defined(PCR) || defined(LINT2)
#   define FAIL abort()
# else
#   define FAIL ABORT("Test failed")
# endif

#endif /* !AMIGA_FASTALLOC */

/* AT_END may be defined to exercise the interior pointer test  */
/* if the collector is configured with ALL_INTERIOR_POINTERS.   */
/* As it stands, this test should succeed with either           */
/* configuration.  In the FIND_LEAK configuration, it should    */
/* find lots of leaks, since we free almost nothing.            */

struct SEXPR {
    struct SEXPR * sexpr_car;
    struct SEXPR * sexpr_cdr;
};


typedef struct SEXPR * sexpr;

# define INT_TO_SEXPR(x) ((sexpr)(GC_word)(x))
# define SEXPR_TO_INT(x) ((int)(GC_word)(x))

# undef nil
# define nil (INT_TO_SEXPR(0))
# define car(x) ((x) -> sexpr_car)
# define cdr(x) ((x) -> sexpr_cdr)
# define is_nil(x) ((x) == nil)

/* Silly implementation of Lisp cons. Intentionally wastes lots of space */
/* to test collector.                                                    */
# ifdef VERY_SMALL_CONFIG
#   define cons small_cons
# else
sexpr cons (sexpr x, sexpr y)
{
    sexpr r;
    int *p;
    unsigned my_extra = (unsigned)AO_fetch_and_add1(&extra_count) % 5000;

    r = (sexpr)GC_MALLOC(sizeof(struct SEXPR) + my_extra);
    CHECK_OUT_OF_MEMORY(r);
    AO_fetch_and_add1(&collectable_count);
    for (p = (int *)r;
         (word)p < (word)r + my_extra + sizeof(struct SEXPR); p++) {
        if (*p) {
            GC_printf("Found nonzero at %p - allocator is broken\n",
                      (void *)p);
            FAIL;
        }
        *p = (int)((13 << 12) + ((p - (int *)r) & 0xfff));
    }
#   ifdef AT_END
        r = (sexpr)((char *)r + (my_extra & ~7));
#   endif
    r -> sexpr_car = x;
    r -> sexpr_cdr = y;
    GC_END_STUBBORN_CHANGE(r);
    return(r);
}
# endif

#include "gc_mark.h"

#ifdef GC_GCJ_SUPPORT

#include "gc_gcj.h"

/* The following struct emulates the vtable in gcj.     */
/* This assumes the default value of MARK_DESCR_OFFSET. */
struct fake_vtable {
  void * dummy;         /* class pointer in real gcj.   */
  GC_word descr;
};

struct fake_vtable gcj_class_struct1 = { 0, sizeof(struct SEXPR)
                                            + sizeof(struct fake_vtable *) };
                        /* length based descriptor.     */
struct fake_vtable gcj_class_struct2 =
                        { 0, ((GC_word)3 << (CPP_WORDSZ - 3)) | GC_DS_BITMAP};
                        /* Bitmap based descriptor.     */

struct GC_ms_entry * fake_gcj_mark_proc(word * addr,
                                        struct GC_ms_entry *mark_stack_ptr,
                                        struct GC_ms_entry *mark_stack_limit,
                                        word env   )
{
    sexpr x;
    if (1 == env) {
        /* Object allocated with debug allocator.       */
        addr = (word *)GC_USR_PTR_FROM_BASE(addr);
    }
    x = (sexpr)(addr + 1); /* Skip the vtable pointer. */
    mark_stack_ptr = GC_MARK_AND_PUSH(
                              (void *)(x -> sexpr_cdr), mark_stack_ptr,
                              mark_stack_limit, (void * *)&(x -> sexpr_cdr));
    mark_stack_ptr = GC_MARK_AND_PUSH(
                              (void *)(x -> sexpr_car), mark_stack_ptr,
                              mark_stack_limit, (void * *)&(x -> sexpr_car));
    return(mark_stack_ptr);
}

#endif /* GC_GCJ_SUPPORT */


sexpr small_cons (sexpr x, sexpr y)
{
    sexpr r = GC_NEW(struct SEXPR);

    CHECK_OUT_OF_MEMORY(r);
    AO_fetch_and_add1(&collectable_count);
    r -> sexpr_car = x;
    r -> sexpr_cdr = y;
    GC_END_STUBBORN_CHANGE(r);
    return(r);
}

sexpr small_cons_uncollectable (sexpr x, sexpr y)
{
    sexpr r = (sexpr)GC_MALLOC_UNCOLLECTABLE(sizeof(struct SEXPR));

    CHECK_OUT_OF_MEMORY(r);
    AO_fetch_and_add1(&uncollectable_count);
    r -> sexpr_car = x;
    r -> sexpr_cdr = (sexpr)(~(GC_word)y);
    GC_END_STUBBORN_CHANGE(r);
    return(r);
}

#ifdef GC_GCJ_SUPPORT
  sexpr gcj_cons(sexpr x, sexpr y)
  {
    sexpr result;
    GC_word * r = (GC_word *)GC_GCJ_MALLOC(
                        sizeof(struct SEXPR) + sizeof(struct fake_vtable*),
                        (AO_fetch_and_add1(&extra_count) & 1) != 0
                                                ? &gcj_class_struct1
                                                : &gcj_class_struct2);

    CHECK_OUT_OF_MEMORY(r);
    result = (sexpr)(r + 1);
    result -> sexpr_car = x;
    result -> sexpr_cdr = y;
    GC_END_STUBBORN_CHANGE(r);
    return(result);
  }
#endif /* GC_GCJ_SUPPORT */

/* Return reverse(x) concatenated with y */
sexpr reverse1(sexpr x, sexpr y)
{
    if (is_nil(x)) {
        return(y);
    } else {
        return( reverse1(cdr(x), cons(car(x), y)) );
    }
}

sexpr reverse(sexpr x)
{
#   ifdef TEST_WITH_SYSTEM_MALLOC
      GC_noop1(GC_HIDE_POINTER(malloc(100000)));
#   endif
    return( reverse1(x, nil) );
}

sexpr ints(int low, int up)
{
    if (low > up) {
        return(nil);
    } else {
        return(small_cons(small_cons(INT_TO_SEXPR(low), nil), ints(low+1, up)));
    }
}

#ifdef GC_GCJ_SUPPORT
/* Return reverse(x) concatenated with y */
sexpr gcj_reverse1(sexpr x, sexpr y)
{
    if (is_nil(x)) {
        return(y);
    } else {
        return( gcj_reverse1(cdr(x), gcj_cons(car(x), y)) );
    }
}

sexpr gcj_reverse(sexpr x)
{
    return( gcj_reverse1(x, nil) );
}

sexpr gcj_ints(int low, int up)
{
    if (low > up) {
        return(nil);
    } else {
        return(gcj_cons(gcj_cons(INT_TO_SEXPR(low), nil), gcj_ints(low+1, up)));
    }
}
#endif /* GC_GCJ_SUPPORT */

/* To check uncollectible allocation we build lists with disguised cdr  */
/* pointers, and make sure they don't go away.                          */
sexpr uncollectable_ints(int low, int up)
{
    if (low > up) {
        return(nil);
    } else {
        return(small_cons_uncollectable(small_cons(INT_TO_SEXPR(low), nil),
               uncollectable_ints(low+1, up)));
    }
}

void check_ints(sexpr list, int low, int up)
{
    if (is_nil(list)) {
        GC_printf("list is nil\n");
        FAIL;
    }
    if (SEXPR_TO_INT(car(car(list))) != low) {
        GC_printf(
           "List reversal produced incorrect list - collector is broken\n");
        FAIL;
    }
    if (low == up) {
        if (cdr(list) != nil) {
           GC_printf("List too long - collector is broken\n");
           FAIL;
        }
    } else {
        check_ints(cdr(list), low+1, up);
    }
}

# define UNCOLLECTABLE_CDR(x) (sexpr)(~(GC_word)(cdr(x)))

void check_uncollectable_ints(sexpr list, int low, int up)
{
    if (SEXPR_TO_INT(car(car(list))) != low) {
        GC_printf("Uncollectable list corrupted - collector is broken\n");
        FAIL;
    }
    if (low == up) {
      if (UNCOLLECTABLE_CDR(list) != nil) {
        GC_printf("Uncollectable list too long - collector is broken\n");
        FAIL;
      }
    } else {
        check_uncollectable_ints(UNCOLLECTABLE_CDR(list), low+1, up);
    }
}

/* Not used, but useful for debugging: */
void print_int_list(sexpr x)
{
    if (is_nil(x)) {
        GC_printf("NIL\n");
    } else {
        GC_printf("(%d)", SEXPR_TO_INT(car(car(x))));
        if (!is_nil(cdr(x))) {
            GC_printf(", ");
            print_int_list(cdr(x));
        } else {
            GC_printf("\n");
        }
    }
}

/* ditto: */
void check_marks_int_list(sexpr x)
{
    if (!GC_is_marked(x))
        GC_printf("[unm:%p]", (void *)x);
    else
        GC_printf("[mkd:%p]", (void *)x);
    if (is_nil(x)) {
        GC_printf("NIL\n");
    } else {
        if (!GC_is_marked(car(x)))
          GC_printf("[unm car:%p]", (void *)car(x));
        GC_printf("(%d)", SEXPR_TO_INT(car(car(x))));
        if (!is_nil(cdr(x))) {
            GC_printf(", ");
            check_marks_int_list(cdr(x));
        } else {
            GC_printf("\n");
        }
    }
}

/*
 * A tiny list reversal test to check thread creation.
 */
#ifdef THREADS

# ifdef VERY_SMALL_CONFIG
#   define TINY_REVERSE_UPPER_VALUE 4
# else
#   define TINY_REVERSE_UPPER_VALUE 10
# endif

# if defined(GC_WIN32_THREADS) && !defined(GC_PTHREADS)
    DWORD  __stdcall tiny_reverse_test(void * arg GC_ATTR_UNUSED)
# else
    void * tiny_reverse_test(void * arg GC_ATTR_UNUSED)
# endif
{
    int i;
    for (i = 0; i < 5; ++i) {
      check_ints(reverse(reverse(ints(1, TINY_REVERSE_UPPER_VALUE))),
                 1, TINY_REVERSE_UPPER_VALUE);
    }
#   if defined(GC_ENABLE_SUSPEND_THREAD)
      /* Force collection from a thread. */
      GC_gcollect();
#   endif
    return 0;
}

# if defined(GC_PTHREADS)
#   if defined(GC_ENABLE_SUSPEND_THREAD)
#     include "javaxfc.h"
#   endif

    void fork_a_thread(void)
    {
      pthread_t t;
      int code;

      code = pthread_create(&t, NULL, tiny_reverse_test, 0);
      if (code != 0) {
        GC_printf("Small thread creation failed %d\n", code);
        FAIL;
      }
#     if defined(GC_ENABLE_SUSPEND_THREAD) && !defined(GC_DARWIN_THREADS) \
         && !defined(GC_OPENBSD_UTHREADS) && !defined(GC_WIN32_THREADS) \
         && !defined(NACL) && !defined(GC_OSF1_THREADS)
        if (GC_is_thread_suspended(t)) {
          GC_printf("Running thread should be not suspended\n");
          FAIL;
        }
        /* Thread could be running or already terminated (but not joined). */
        GC_suspend_thread(t);
        if (!GC_is_thread_suspended(t)) {
          GC_printf("Thread expected to be suspended\n");
          FAIL;
        }
        GC_suspend_thread(t); /* should be no-op */
        GC_resume_thread(t);
        if (GC_is_thread_suspended(t)) {
          GC_printf("Resumed thread should be not suspended\n");
          FAIL;
        }
        GC_resume_thread(t); /* should be no-op */
#     endif
      if ((code = pthread_join(t, 0)) != 0) {
        GC_printf("Small thread join failed %d\n", code);
        FAIL;
      }
    }

# elif defined(GC_WIN32_THREADS)
    void fork_a_thread(void)
    {
        DWORD thread_id;
        HANDLE h;
        h = GC_CreateThread((SECURITY_ATTRIBUTES *)NULL, (word)0,
                            tiny_reverse_test, NULL, (DWORD)0, &thread_id);
                                /* Explicitly specify types of the      */
                                /* arguments to test the prototype.     */
        if (h == (HANDLE)NULL) {
            GC_printf("Small thread creation failed %d\n",
                          (int)GetLastError());
            FAIL;
        }
        if (WaitForSingleObject(h, INFINITE) != WAIT_OBJECT_0) {
            GC_printf("Small thread wait failed %d\n",
                          (int)GetLastError());
            FAIL;
        }
    }

# endif

#endif

void test_generic_malloc_or_special(void *p) {
  size_t size;
  int kind = GC_get_kind_and_size(p, &size);
  void *p2;

  if (size != GC_size(p)) {
    GC_printf("GC_get_kind_and_size returned size not matching GC_size\n");
    FAIL;
  }
  p2 = GC_GENERIC_OR_SPECIAL_MALLOC(10, kind);
  CHECK_OUT_OF_MEMORY(p2);
  if (GC_get_kind_and_size(p2, NULL) != kind) {
    GC_printf("GC_generic_or_special_malloc:"
              " unexpected kind of returned object\n");
    FAIL;
  }
  GC_FREE(p2);
}

/* Try to force a to be strangely aligned */
volatile struct A_s {
  char dummy;
  AO_t aa;
} A;
#define a_set(p) AO_store_release(&A.aa, (AO_t)(p))
#define a_get() (sexpr)AO_load_acquire(&A.aa)

/*
 * Repeatedly reverse lists built out of very different sized cons cells.
 * Check that we didn't lose anything.
 */
void *GC_CALLBACK reverse_test_inner(void *data)
{
    int i;
    sexpr b;
    sexpr c;
    sexpr d;
    sexpr e;
    sexpr *f, *g, *h;

    if (data == 0) {
      /* This stack frame is not guaranteed to be scanned. */
      return GC_call_with_gc_active(reverse_test_inner, (void*)(word)1);
    }

#   if defined(MACOS) \
       || (defined(UNIX_LIKE) && defined(NO_GETCONTEXT)) /* e.g. musl */
      /* Assume 128K stacks at least. */
#     define BIG 1000
#   elif defined(PCR)
      /* PCR default stack is 100K.  Stack frames are up to 120 bytes. */
#     define BIG 700
#   elif defined(MSWINCE) || defined(RTEMS)
      /* WinCE only allows 64K stacks */
#     define BIG 500
#   elif defined(OSF1)
      /* OSF has limited stack space by default, and large frames. */
#     define BIG 200
#   elif defined(__MACH__) && defined(__ppc64__)
#     define BIG 2500
#   else
#     define BIG 4500
#   endif

    a_set(ints(1, 49));
    b = ints(1, 50);
    c = ints(1, BIG);
    d = uncollectable_ints(1, 100);
    test_generic_malloc_or_special(d);
    e = uncollectable_ints(1, 1);
    /* Check that realloc updates object descriptors correctly */
    AO_fetch_and_add1(&collectable_count);
    f = (sexpr *)GC_MALLOC(4 * sizeof(sexpr));
    f = (sexpr *)GC_REALLOC((void *)f, 6 * sizeof(sexpr));
    CHECK_OUT_OF_MEMORY(f);
    AO_fetch_and_add1(&realloc_count);
    f[5] = ints(1,17);
    AO_fetch_and_add1(&collectable_count);
    g = (sexpr *)GC_MALLOC(513 * sizeof(sexpr));
    test_generic_malloc_or_special(g);
    g = (sexpr *)GC_REALLOC((void *)g, 800 * sizeof(sexpr));
    CHECK_OUT_OF_MEMORY(g);
    AO_fetch_and_add1(&realloc_count);
    g[799] = ints(1,18);
    AO_fetch_and_add1(&collectable_count);
    h = (sexpr *)GC_MALLOC(1025 * sizeof(sexpr));
    h = (sexpr *)GC_REALLOC((void *)h, 2000 * sizeof(sexpr));
    CHECK_OUT_OF_MEMORY(h);
    AO_fetch_and_add1(&realloc_count);
#   ifdef GC_GCJ_SUPPORT
      h[1999] = gcj_ints(1,200);
      for (i = 0; i < 51; ++i)
        h[1999] = gcj_reverse(h[1999]);
      /* Leave it as the reversed list for now. */
#   else
      h[1999] = ints(1,200);
#   endif
    /* Try to force some collections and reuse of small list elements */
    for (i = 0; i < 10; i++) {
      (void)ints(1, BIG);
    }
    /* Superficially test interior pointer recognition on stack */
    c = (sexpr)((char *)c + sizeof(char *));
    d = (sexpr)((char *)d + sizeof(char *));

    GC_FREE((void *)e);

    check_ints(b,1,50);
    check_ints(a_get(),1,49);
    for (i = 0; i < 50; i++) {
        check_ints(b,1,50);
        b = reverse(reverse(b));
    }
    check_ints(b,1,50);
    check_ints(a_get(),1,49);
    for (i = 0; i < 60; i++) {
#       if (defined(GC_PTHREADS) || defined(GC_WIN32_THREADS)) \
           && (NTHREADS > 0)
            if (i % 10 == 0) fork_a_thread();
#       endif
        /* This maintains the invariant that a always points to a list  */
        /* of 49 integers.  Thus, this is thread safe without locks,    */
        /* assuming acquire/release barriers in a_get/set() and atomic  */
        /* pointer assignments (otherwise, e.g., check_ints() may see   */
        /* an uninitialized object returned by GC_MALLOC).              */
        a_set(reverse(reverse(a_get())));
#       if !defined(AT_END) && !defined(THREADS)
          /* This is not thread safe, since realloc explicitly deallocates */
          a_set(GC_REALLOC(a_get(), (i & 1) != 0 ? 500 : 8200));
          AO_fetch_and_add1(&realloc_count);
#       endif
    }
    check_ints(a_get(),1,49);
    check_ints(b,1,50);

    /* Restore c and d values. */
    c = (sexpr)((char *)c - sizeof(char *));
    d = (sexpr)((char *)d - sizeof(char *));

    check_ints(c,1,BIG);
    check_uncollectable_ints(d, 1, 100);
    check_ints(f[5], 1,17);
    check_ints(g[799], 1,18);
#   ifdef GC_GCJ_SUPPORT
      h[1999] = gcj_reverse(h[1999]);
#   endif
    check_ints(h[1999], 1,200);
#   ifndef THREADS
      a_set(NULL);
#   endif
    *(sexpr volatile *)&b = 0;
    *(sexpr volatile *)&c = 0;
    return 0;
}

void reverse_test(void)
{
    /* Test GC_do_blocking/GC_call_with_gc_active. */
    (void)GC_do_blocking(reverse_test_inner, 0);
}

/*
 * The rest of this builds balanced binary trees, checks that they don't
 * disappear, and tests finalization.
 */
typedef struct treenode {
    int level;
    struct treenode * lchild;
    struct treenode * rchild;
} tn;

int finalizable_count = 0;
int finalized_count = 0;
int dropped_something = 0;

void GC_CALLBACK finalizer(void * obj, void * client_data)
{
  tn * t = (tn *)obj;

  FINALIZER_LOCK();
  if ((int)(GC_word)client_data != t -> level) {
     GC_printf("Wrong finalization data - collector is broken\n");
     FAIL;
  }
  finalized_count++;
  t -> level = -1;      /* detect duplicate finalization immediately */
  FINALIZER_UNLOCK();
}

# define MAX_FINALIZED ((NTHREADS+1)*4000)

# if !defined(MACOS)
  GC_FAR GC_word live_indicators[MAX_FINALIZED] = {0};
# ifndef GC_LONG_REFS_NOT_NEEDED
    GC_FAR void *live_long_refs[MAX_FINALIZED] = { NULL };
# endif
#else
  /* Too big for THINK_C. have to allocate it dynamically. */
  GC_word *live_indicators = 0;
# ifndef GC_LONG_REFS_NOT_NEEDED
#   define GC_LONG_REFS_NOT_NEEDED
# endif
#endif

int live_indicators_count = 0;

tn * mktree(int n)
{
    tn * result = GC_NEW(tn);

    AO_fetch_and_add1(&collectable_count);
#   if defined(MACOS)
        /* get around static data limitations. */
        if (!live_indicators) {
          live_indicators =
                    (GC_word*)NewPtrClear(MAX_FINALIZED * sizeof(GC_word));
          CHECK_OUT_OF_MEMORY(live_indicators);
        }
#   endif
    if (n == 0) return(0);
    CHECK_OUT_OF_MEMORY(result);
    result -> level = n;
    result -> lchild = mktree(n-1);
    result -> rchild = mktree(n-1);
    if (AO_fetch_and_add1(&extra_count) % 17 == 0 && n >= 2) {
        tn * tmp;
        tn * left = result -> lchild;
        tn * right = result -> rchild;

        CHECK_OUT_OF_MEMORY(left);
        tmp = left -> rchild;
        CHECK_OUT_OF_MEMORY(right);
        left -> rchild = right -> lchild;
        right -> lchild = tmp;
        GC_END_STUBBORN_CHANGE(left);
        GC_END_STUBBORN_CHANGE(right);
    }
    if (AO_fetch_and_add1(&extra_count) % 119 == 0) {
#       ifndef GC_NO_FINALIZATION
          int my_index;
          void *new_link;
#       endif

        {
          FINALIZER_LOCK();
                /* Losing a count here causes erroneous report of failure. */
          finalizable_count++;
#         ifndef GC_NO_FINALIZATION
            my_index = live_indicators_count++;
#         endif
          FINALIZER_UNLOCK();
        }

#     ifndef GC_NO_FINALIZATION
        GC_REGISTER_FINALIZER((void *)result, finalizer, (void *)(GC_word)n,
                              (GC_finalization_proc *)0, (void * *)0);
        if (my_index >= MAX_FINALIZED) {
                GC_printf("live_indicators overflowed\n");
                FAIL;
        }
        live_indicators[my_index] = 13;
        if (GC_GENERAL_REGISTER_DISAPPEARING_LINK(
            (void * *)(&(live_indicators[my_index])), result) != 0) {
                GC_printf("GC_general_register_disappearing_link failed\n");
                FAIL;
        }
        if (GC_move_disappearing_link((void **)(&(live_indicators[my_index])),
                   (void **)(&(live_indicators[my_index]))) != GC_SUCCESS) {
                GC_printf("GC_move_disappearing_link(link,link) failed\n");
                FAIL;
        }
        new_link = (void *)live_indicators[my_index];
        if (GC_move_disappearing_link((void **)(&(live_indicators[my_index])),
                                      &new_link) != GC_SUCCESS) {
                GC_printf("GC_move_disappearing_link(new_link) failed\n");
                FAIL;
        }
        if (GC_unregister_disappearing_link(&new_link) == 0) {
                GC_printf("GC_unregister_disappearing_link failed\n");
                FAIL;
        }
        if (GC_move_disappearing_link((void **)(&(live_indicators[my_index])),
                                      &new_link) != GC_NOT_FOUND) {
                GC_printf("GC_move_disappearing_link(new_link) failed 2\n");
                FAIL;
        }
        if (GC_GENERAL_REGISTER_DISAPPEARING_LINK(
            (void * *)(&(live_indicators[my_index])), result) != 0) {
                GC_printf("GC_general_register_disappearing_link failed 2\n");
                FAIL;
        }
#       ifndef GC_LONG_REFS_NOT_NEEDED
          if (GC_REGISTER_LONG_LINK(&live_long_refs[my_index], result) != 0) {
            GC_printf("GC_register_long_link failed\n");
            FAIL;
          }
          if (GC_move_long_link(&live_long_refs[my_index],
                                &live_long_refs[my_index]) != GC_SUCCESS) {
            GC_printf("GC_move_long_link(link,link) failed\n");
            FAIL;
          }
          new_link = live_long_refs[my_index];
          if (GC_move_long_link(&live_long_refs[my_index],
                                &new_link) != GC_SUCCESS) {
            GC_printf("GC_move_long_link(new_link) failed\n");
            FAIL;
          }
          if (GC_unregister_long_link(&new_link) == 0) {
            GC_printf("GC_unregister_long_link failed\n");
            FAIL;
          }
          if (GC_move_long_link(&live_long_refs[my_index],
                                &new_link) != GC_NOT_FOUND) {
            GC_printf("GC_move_long_link(new_link) failed 2\n");
            FAIL;
          }
          if (GC_REGISTER_LONG_LINK(&live_long_refs[my_index], result) != 0) {
            GC_printf("GC_register_long_link failed 2\n");
            FAIL;
          }
#       endif
#     endif
        GC_reachable_here(result);
    }
    GC_END_STUBBORN_CHANGE(result);
    return(result);
}

void chktree(tn *t, int n)
{
    if (0 == n) {
        if (NULL == t) /* is a leaf? */
            return;
        GC_printf("Clobbered a leaf - collector is broken\n");
        FAIL;
    }
    if (t -> level != n) {
        GC_printf("Lost a node at level %d - collector is broken\n", n);
        FAIL;
    }
    if (AO_fetch_and_add1(&extra_count) % 373 == 0) {
        (void)GC_MALLOC((unsigned)AO_fetch_and_add1(&extra_count) % 5001);
        AO_fetch_and_add1(&collectable_count);
    }
    chktree(t -> lchild, n-1);
    if (AO_fetch_and_add1(&extra_count) % 73 == 0) {
        (void)GC_MALLOC((unsigned)AO_fetch_and_add1(&extra_count) % 373);
        AO_fetch_and_add1(&collectable_count);
    }
    chktree(t -> rchild, n-1);
}


#if defined(GC_PTHREADS)
pthread_key_t fl_key;

void * alloc8bytes(void)
{
# if defined(SMALL_CONFIG) || defined(GC_DEBUG)
    AO_fetch_and_add1(&collectable_count);
    return(GC_MALLOC(8));
# else
    void ** my_free_list_ptr;
    void * my_free_list;

    my_free_list_ptr = (void **)pthread_getspecific(fl_key);
    if (my_free_list_ptr == 0) {
        my_free_list_ptr = GC_NEW_UNCOLLECTABLE(void *);
        CHECK_OUT_OF_MEMORY(my_free_list_ptr);
        AO_fetch_and_add1(&uncollectable_count);
        if (pthread_setspecific(fl_key, my_free_list_ptr) != 0) {
            GC_printf("pthread_setspecific failed\n");
            FAIL;
        }
    }
    my_free_list = *my_free_list_ptr;
    if (my_free_list == 0) {
        my_free_list = GC_malloc_many(8);
        CHECK_OUT_OF_MEMORY(my_free_list);
    }
    *my_free_list_ptr = GC_NEXT(my_free_list);
    GC_NEXT(my_free_list) = 0;
    GC_END_STUBBORN_CHANGE(my_free_list_ptr);
    AO_fetch_and_add1(&collectable_count);
    return(my_free_list);
# endif
}

#else
#   define alloc8bytes() GC_MALLOC_ATOMIC(8)
#endif

#include "gc_inline.h"

void test_tinyfl(void)
{
  void *results[3];
  void *tfls[3][GC_TINY_FREELISTS];

# ifndef DONT_ADD_BYTE_AT_END
    if (GC_get_all_interior_pointers()) return; /* skip */
# endif
  BZERO(tfls, sizeof(tfls));
  /* TODO: Improve testing of FAST_MALLOC functionality. */
  GC_MALLOC_WORDS(results[0], 11, tfls[0]);
  GC_MALLOC_ATOMIC_WORDS(results[1], 20, tfls[1]);
  GC_CONS(results[2], results[0], results[1], tfls[2]);
}

void alloc_small(int n)
{
    int i;

    for (i = 0; i < n; i += 8) {
        if (alloc8bytes() == 0) {
            GC_printf("Out of memory\n");
            FAIL;
        }
        AO_fetch_and_add1(&atomic_count);
    }
}

# if defined(THREADS) && defined(GC_DEBUG)
#   ifdef VERY_SMALL_CONFIG
#     define TREE_HEIGHT 12
#   else
#     define TREE_HEIGHT 15
#   endif
# else
#   ifdef VERY_SMALL_CONFIG
#     define TREE_HEIGHT 13
#   else
#     define TREE_HEIGHT 16
#   endif
# endif
void tree_test(void)
{
    tn * root;
    int i;

    root = mktree(TREE_HEIGHT);
#   ifndef VERY_SMALL_CONFIG
      alloc_small(5000000);
#   endif
    chktree(root, TREE_HEIGHT);
    FINALIZER_LOCK();
    if (finalized_count && !dropped_something) {
        GC_printf("Premature finalization - collector is broken\n");
        FAIL;
    }
    dropped_something = 1;
    FINALIZER_UNLOCK();
    GC_noop1((word)root);       /* Root needs to remain live until      */
                                /* dropped_something is set.            */
    root = mktree(TREE_HEIGHT);
    chktree(root, TREE_HEIGHT);
    for (i = TREE_HEIGHT; i >= 0; i--) {
        root = mktree(i);
        chktree(root, i);
    }
#   ifndef VERY_SMALL_CONFIG
      alloc_small(5000000);
#   endif
}

unsigned n_tests = 0;

const GC_word bm_huge[320 / CPP_WORDSZ] = {
# if CPP_WORDSZ == 32
    0xffffffff,
    0xffffffff,
    0xffffffff,
    0xffffffff,
    0xffffffff,
# endif
    (GC_word)((GC_signed_word)-1),
    (GC_word)((GC_signed_word)-1),
    (GC_word)((GC_signed_word)-1),
    (GC_word)((GC_signed_word)-1),
    ((GC_word)((GC_signed_word)-1)) >> 8 /* highest byte is zero */
};

/* A very simple test of explicitly typed allocation    */
void typed_test(void)
{
    GC_word * old, * newP;
    GC_word bm3[1] = {0};
    GC_word bm2[1] = {0};
    GC_word bm_large[1] = { 0xf7ff7fff };
    GC_descr d1;
    GC_descr d2;
    GC_descr d3 = GC_make_descriptor(bm_large, 32);
    GC_descr d4 = GC_make_descriptor(bm_huge, 320);
    GC_word * x = (GC_word *)GC_malloc_explicitly_typed(
                                320 * sizeof(GC_word) + 123, d4);
    int i;

    AO_fetch_and_add1(&collectable_count);
    (void)GC_make_descriptor(bm_large, 32);
    if (GC_get_bit(bm_huge, 32) == 0 || GC_get_bit(bm_huge, 311) == 0
        || GC_get_bit(bm_huge, 319) != 0) {
      GC_printf("Bad GC_get_bit() or bm_huge initialization\n");
      FAIL;
    }
    GC_set_bit(bm3, 0);
    GC_set_bit(bm3, 1);
    d1 = GC_make_descriptor(bm3, 2);
    GC_set_bit(bm2, 1);
    d2 = GC_make_descriptor(bm2, 2);
    old = 0;
    for (i = 0; i < 4000; i++) {
        newP = (GC_word *)GC_malloc_explicitly_typed(4 * sizeof(GC_word), d1);
        CHECK_OUT_OF_MEMORY(newP);
        AO_fetch_and_add1(&collectable_count);
        if (newP[0] != 0 || newP[1] != 0) {
            GC_printf("Bad initialization by GC_malloc_explicitly_typed\n");
            FAIL;
        }
        newP[0] = 17;
        newP[1] = (GC_word)old;
        old = newP;
        AO_fetch_and_add1(&collectable_count);
        newP = (GC_word *)GC_malloc_explicitly_typed(4 * sizeof(GC_word), d2);
        CHECK_OUT_OF_MEMORY(newP);
        newP[0] = 17;
        newP[1] = (GC_word)old;
        GC_END_STUBBORN_CHANGE(newP);
        old = newP;
        AO_fetch_and_add1(&collectable_count);
        newP = (GC_word*)GC_malloc_explicitly_typed(33 * sizeof(GC_word), d3);
        CHECK_OUT_OF_MEMORY(newP);
        newP[0] = 17;
        newP[1] = (GC_word)old;
        GC_END_STUBBORN_CHANGE(newP);
        old = newP;
        AO_fetch_and_add1(&collectable_count);
        newP = (GC_word *)GC_calloc_explicitly_typed(4, 2 * sizeof(GC_word),
                                                     d1);
        CHECK_OUT_OF_MEMORY(newP);
        newP[0] = 17;
        newP[1] = (GC_word)old;
        GC_END_STUBBORN_CHANGE(newP);
        old = newP;
        AO_fetch_and_add1(&collectable_count);
        if (i & 0xff) {
          newP = (GC_word *)GC_calloc_explicitly_typed(7, 3 * sizeof(GC_word),
                                                       d2);
        } else {
          newP = (GC_word *)GC_calloc_explicitly_typed(1001,
                                                       3 * sizeof(GC_word),
                                                       d2);
          if (newP != NULL && (newP[0] != 0 || newP[1] != 0)) {
            GC_printf("Bad initialization by GC_malloc_explicitly_typed\n");
            FAIL;
          }
        }
        CHECK_OUT_OF_MEMORY(newP);
        newP[0] = 17;
        newP[1] = (GC_word)old;
        GC_END_STUBBORN_CHANGE(newP);
        old = newP;
    }
    for (i = 0; i < 20000; i++) {
        if (newP[0] != 17) {
            GC_printf("Typed alloc failed at %d\n", i);
            FAIL;
        }
        newP[0] = 0;
        old = newP;
        newP = (GC_word *)old[1];
    }
    GC_gcollect();
    GC_noop1((word)x);
}

#ifdef DBG_HDRS_ALL
# define set_print_procs() (void)(A.dummy = 17)
#else
  int fail_count = 0;

  void GC_CALLBACK fail_proc1(void *x GC_ATTR_UNUSED)
  {
    fail_count++;
  }

  void set_print_procs(void)
  {
    /* Set these global variables just once to avoid TSan false positives. */
    A.dummy = 17;
    GC_is_valid_displacement_print_proc = fail_proc1;
    GC_is_visible_print_proc = fail_proc1;
  }

# ifdef THREADS
#   define TEST_FAIL_COUNT(n) 1
# else
#   define TEST_FAIL_COUNT(n) (fail_count >= (n))
# endif
#endif /* !DBG_HDRS_ALL */

static void uniq(void *p, ...) {
  va_list a;
  void *q[100];
  int n = 0, i, j;
  q[n++] = p;
  va_start(a,p);
  for (;(q[n] = va_arg(a,void *)) != NULL;n++) ;
  va_end(a);
  for (i=0; i<n; i++)
    for (j=0; j<i; j++)
      if (q[i] == q[j]) {
        GC_printf(
              "Apparently failed to mark from some function arguments.\n"
              "Perhaps GC_push_regs was configured incorrectly?\n"
        );
        FAIL;
      }
}

void * GC_CALLBACK inc_int_counter(void *pcounter)
{
 ++(*(int *)pcounter);
 return NULL;
}

void run_one_test(void)
{
#   ifndef DBG_HDRS_ALL
        char *x;
        char **z;
        char *y = (char *)(GC_word)fail_proc1;
#   endif
#   ifndef NO_CLOCK
      CLOCK_TYPE start_time;
      CLOCK_TYPE reverse_time;
      unsigned long time_diff;
#   endif
#   ifndef NO_TEST_HANDLE_FORK
      pid_t pid;
      int wstatus;
#   endif

#   ifdef FIND_LEAK
        GC_printf(
              "This test program is not designed for leak detection mode\n");
        GC_printf("Expect lots of problems\n");
#   endif
    GC_FREE(0);
#   ifdef THREADS
      if (!GC_thread_is_registered()) {
        GC_printf("Current thread is not registered with GC\n");
        FAIL;
      }
#   endif
    test_tinyfl();
#   ifndef DBG_HDRS_ALL
      AO_fetch_and_add1(&collectable_count); /* 1 */
      AO_fetch_and_add1(&collectable_count); /* 2 */
      AO_fetch_and_add1(&collectable_count); /* 3 */
      if ((GC_size(GC_malloc(7)) != 8 &&
           GC_size(GC_malloc(7)) != MIN_WORDS * sizeof(GC_word))
           || GC_size(GC_malloc(15)) != 16) {
        GC_printf("GC_size produced unexpected results\n");
        FAIL;
      }
      AO_fetch_and_add1(&collectable_count);
      if (GC_size(GC_malloc(0)) != MIN_WORDS * sizeof(GC_word)) {
        GC_printf("GC_malloc(0) failed: GC_size returns %lu\n",
                      (unsigned long)GC_size(GC_malloc(0)));
        FAIL;
      }
      AO_fetch_and_add1(&uncollectable_count);
      if (GC_size(GC_malloc_uncollectable(0)) != MIN_WORDS * sizeof(GC_word)) {
        GC_printf("GC_malloc_uncollectable(0) failed\n");
        FAIL;
      }
      AO_fetch_and_add1(&collectable_count);
      x = (char*)GC_malloc(16);
      if (GC_base(GC_PTR_ADD(x, 13)) != x) {
        GC_printf("GC_base(heap ptr) produced incorrect result\n");
        FAIL;
      }
      if (!GC_is_heap_ptr(x)) {
        GC_printf("GC_is_heap_ptr(heap_ptr) produced incorrect result\n");
        FAIL;
      }
      if (GC_is_heap_ptr(&x)) {
        GC_printf("GC_is_heap_ptr(&local_var) produced incorrect result\n");
        FAIL;
      }
      if (GC_is_heap_ptr(&fail_count) || GC_is_heap_ptr(NULL)) {
        GC_printf("GC_is_heap_ptr(&global_var) produced incorrect result\n");
        FAIL;
      }
      (void)GC_PRE_INCR(x, 0);
      (void)GC_POST_INCR(x);
      (void)GC_POST_DECR(x);
      if (GC_base(x) != x) {
        GC_printf("Bad INCR/DECR result\n");
        FAIL;
      }
#     ifndef PCR
        if (GC_base(y) != 0) {
          GC_printf("GC_base(fn_ptr) produced incorrect result\n");
          FAIL;
        }
#     endif
      if (GC_same_obj(x+5, x) != x + 5) {
        GC_printf("GC_same_obj produced incorrect result\n");
        FAIL;
      }
      if (GC_is_visible(y) != y || GC_is_visible(x) != x) {
        GC_printf("GC_is_visible produced incorrect result\n");
        FAIL;
      }
      z = (char**)GC_malloc(8);
      CHECK_OUT_OF_MEMORY(z);
      AO_fetch_and_add1(&collectable_count);
      GC_PTR_STORE(z, x);
      GC_end_stubborn_change(z);
      if (*z != x) {
        GC_printf("GC_PTR_STORE failed: %p != %p\n", (void *)(*z), (void *)x);
        FAIL;
      }
      if (!TEST_FAIL_COUNT(1)) {
#       if!(defined(POWERPC) || defined(IA64)) || defined(M68K)
          /* On POWERPCs function pointers point to a descriptor in the */
          /* data segment, so there should have been no failures.       */
          /* The same applies to IA64.  Something similar seems to      */
          /* be going on with NetBSD/M68K.                              */
          GC_printf("GC_is_visible produced wrong failure indication\n");
          FAIL;
#       endif
      }
      if (GC_is_valid_displacement(y) != y
        || GC_is_valid_displacement(x) != x
        || GC_is_valid_displacement(x + 3) != x + 3) {
        GC_printf("GC_is_valid_displacement produced incorrect result\n");
        FAIL;
      }
        {
          size_t i;

          (void)GC_malloc(17);
          AO_fetch_and_add1(&collectable_count);
          for (i = sizeof(GC_word); i < 512; i *= 2) {
            GC_word result = (GC_word) GC_memalign(i, 17);
            if (result % i != 0 || result == 0 || *(int *)result != 0) FAIL;
          }
        }
#     ifndef ALL_INTERIOR_POINTERS
#      if defined(RS6000) || defined(POWERPC)
        if (!TEST_FAIL_COUNT(1))
#      else
        if (!TEST_FAIL_COUNT(GC_get_all_interior_pointers() ? 1 : 2))
#      endif
        {
          GC_printf(
              "GC_is_valid_displacement produced wrong failure indication\n");
          FAIL;
        }
#     endif
#   endif /* DBG_HDRS_ALL */
    /* Test floating point alignment */
        {
          double *dp = GC_NEW(double);
          CHECK_OUT_OF_MEMORY(dp);
          AO_fetch_and_add1(&collectable_count);
          *dp = 1.0;
          dp = GC_NEW(double);
          CHECK_OUT_OF_MEMORY(dp);
          AO_fetch_and_add1(&collectable_count);
          *dp = 1.0;
        }
    /* Test size 0 allocation a bit more */
        {
           size_t i;
           for (i = 0; i < 10000; ++i) {
             (void)GC_MALLOC(0);
             AO_fetch_and_add1(&collectable_count);
             GC_FREE(GC_MALLOC(0));
             (void)GC_MALLOC_ATOMIC(0);
             AO_fetch_and_add1(&atomic_count);
             GC_FREE(GC_MALLOC_ATOMIC(0));
             test_generic_malloc_or_special(GC_malloc_atomic(1));
             AO_fetch_and_add1(&atomic_count);
           }
         }
#   ifdef GC_GCJ_SUPPORT
      GC_REGISTER_DISPLACEMENT(sizeof(struct fake_vtable *));
      GC_init_gcj_malloc(0, (void *)(GC_word)fake_gcj_mark_proc);
#   endif
    /* Make sure that fn arguments are visible to the collector.        */
      uniq(
        GC_malloc(12), GC_malloc(12), GC_malloc(12),
        (GC_gcollect(),GC_malloc(12)),
        GC_malloc(12), GC_malloc(12), GC_malloc(12),
        (GC_gcollect(),GC_malloc(12)),
        GC_malloc(12), GC_malloc(12), GC_malloc(12),
        (GC_gcollect(),GC_malloc(12)),
        GC_malloc(12), GC_malloc(12), GC_malloc(12),
        (GC_gcollect(),GC_malloc(12)),
        GC_malloc(12), GC_malloc(12), GC_malloc(12),
        (GC_gcollect(),GC_malloc(12)),
        (void *)0);
    /* GC_malloc(0) must return NULL or something we can deallocate. */
        GC_free(GC_malloc(0));
        GC_free(GC_malloc_atomic(0));
        GC_free(GC_malloc(0));
        GC_free(GC_malloc_atomic(0));
#   ifndef NO_TEST_HANDLE_FORK
        GC_atfork_prepare();
        pid = fork();
        if (pid != 0) {
          GC_atfork_parent();
          if (pid == -1) {
            GC_printf("Process fork failed\n");
            FAIL;
          }
          if (print_stats)
            GC_log_printf("Forked child process\n");
          if (waitpid(pid, &wstatus, 0) == -1) {
            GC_printf("Wait for child process failed\n");
            FAIL;
          }
          if (!WIFEXITED(wstatus) || WEXITSTATUS(wstatus) != 0) {
            GC_printf("Child process failed, status= 0x%x\n", wstatus);
            FAIL;
          }
        } else {
          GC_atfork_child();
          if (print_stats)
            GC_log_printf("Started a child process\n");
#         ifdef THREADS
#           ifdef PARALLEL_MARK
              GC_gcollect(); /* no parallel markers */
#           endif
            GC_start_mark_threads();
#         endif
          GC_gcollect();
#         ifdef THREADS
            tiny_reverse_test(0);
            GC_gcollect();
#         endif
          if (print_stats)
            GC_log_printf("Finished a child process\n");
          exit(0);
        }
#   endif
    /* Repeated list reversal test. */
#   ifndef NO_CLOCK
        GET_TIME(start_time);
#   endif
        reverse_test();
#   ifndef NO_CLOCK
        if (print_stats) {
          GET_TIME(reverse_time);
          time_diff = MS_TIME_DIFF(reverse_time, start_time);
          GC_log_printf("-------------Finished reverse_test at time %u (%p)\n",
                        (unsigned) time_diff, (void *)&start_time);
        }
#   endif
#   ifndef DBG_HDRS_ALL
      typed_test();
#     ifndef NO_CLOCK
        if (print_stats) {
          CLOCK_TYPE typed_time;

          GET_TIME(typed_time);
          time_diff = MS_TIME_DIFF(typed_time, start_time);
          GC_log_printf("-------------Finished typed_test at time %u (%p)\n",
                        (unsigned) time_diff, (void *)&start_time);
        }
#     endif
#   endif /* DBG_HDRS_ALL */
    tree_test();
#   ifndef NO_CLOCK
      if (print_stats) {
        CLOCK_TYPE tree_time;

        GET_TIME(tree_time);
        time_diff = MS_TIME_DIFF(tree_time, start_time);
        GC_log_printf("-------------Finished tree_test at time %u (%p)\n",
                      (unsigned) time_diff, (void *)&start_time);
      }
#   endif
    /* Run reverse_test a second time, so we hopefully notice corruption. */
    reverse_test();
#   ifndef NO_CLOCK
      if (print_stats) {
        GET_TIME(reverse_time);
        time_diff = MS_TIME_DIFF(reverse_time, start_time);
        GC_log_printf(
                "-------------Finished second reverse_test at time %u (%p)\n",
                (unsigned)time_diff, (void *)&start_time);
      }
#   endif
    /* GC_allocate_ml and GC_need_to_lock are no longer exported, and   */
    /* AO_fetch_and_add1() may be unavailable to update a counter.      */
    (void)GC_call_with_alloc_lock(inc_int_counter, &n_tests);
#   ifndef NO_CLOCK
      if (print_stats)
        GC_log_printf("Finished %p\n", (void *)&start_time);
#   endif
}

void GC_CALLBACK reachable_objs_counter(void *obj, size_t size,
                                        void *pcounter)
{
  if (0 == size) {
    GC_printf("Reachable object has zero size\n");
    FAIL;
  }
  if (GC_base(obj) != obj) {
    GC_printf("Invalid reachable object base passed by enumerator: %p\n",
              obj);
    FAIL;
  }
  if (GC_size(obj) != size) {
    GC_printf("Invalid reachable object size passed by enumerator: %lu\n",
              (unsigned long)size);
    FAIL;
  }
  (*(unsigned *)pcounter)++;
}

void * GC_CALLBACK reachable_objs_count_enumerator(void *pcounter)
{
  GC_enumerate_reachable_objects_inner(reachable_objs_counter, pcounter);
  return NULL;
}

#define NUMBER_ROUND_UP(v, bound) ((((v) + (bound) - 1) / (bound)) * (bound))

void check_heap_stats(void)
{
    size_t max_heap_sz;
    int i;
#   ifndef GC_NO_FINALIZATION
      int still_live;
#     ifndef GC_LONG_REFS_NOT_NEEDED
        int still_long_live = 0;
#     endif
#     ifdef FINALIZE_ON_DEMAND
        int late_finalize_count = 0;
#     endif
#   endif
    unsigned obj_count = 0;

    if (!GC_is_init_called()) {
      GC_printf("GC should be initialized!\n");
      FAIL;
    }
#   ifdef VERY_SMALL_CONFIG
    /* The upper bounds are a guess, which has been empirically */
    /* adjusted.  On low end uniprocessors with incremental GC  */
    /* these may be particularly dubious, since empirically the */
    /* heap tends to grow largely as a result of the GC not     */
    /* getting enough cycles.                                   */
#     if CPP_WORDSZ == 64
        max_heap_sz = 4500000;
#     else
        max_heap_sz = 2800000;
#     endif
#   else
#     if CPP_WORDSZ == 64
        max_heap_sz = 25000000;
#     else
        max_heap_sz = 16000000;
#     endif
#   endif
#   ifdef GC_DEBUG
        max_heap_sz *= 2;
#       ifdef SAVE_CALL_CHAIN
            max_heap_sz *= 3;
#           ifdef SAVE_CALL_COUNT
                max_heap_sz += max_heap_sz * NFRAMES / 4;
#           endif
#       endif
#   endif
#   if defined(ADDRESS_SANITIZER) && !defined(__clang__)
        max_heap_sz = max_heap_sz * 2 - max_heap_sz / 3;
#   endif
#   ifdef MEMORY_SANITIZER
        max_heap_sz += max_heap_sz / 4;
#   endif
    max_heap_sz *= n_tests;
#   if defined(USE_MMAP) || defined(MSWIN32)
      max_heap_sz = NUMBER_ROUND_UP(max_heap_sz, 4 * 1024 * 1024);
#   endif
    /* Garbage collect repeatedly so that all inaccessible objects      */
    /* can be finalized.                                                */
      while (GC_collect_a_little()) { }
      for (i = 0; i < 16; i++) {
        GC_gcollect();
#       ifndef GC_NO_FINALIZATION
#         ifdef FINALIZE_ON_DEMAND
            late_finalize_count +=
#         endif
                GC_invoke_finalizers();
#       endif
      }
      if (print_stats) {
        struct GC_stack_base sb;
        int res = GC_get_stack_base(&sb);

        if (res == GC_SUCCESS) {
          GC_log_printf("Primordial thread stack bottom: %p\n", sb.mem_base);
        } else if (res == GC_UNIMPLEMENTED) {
          GC_log_printf("GC_get_stack_base() unimplemented\n");
        } else {
          GC_printf("GC_get_stack_base() failed: %d\n", res);
          FAIL;
        }
      }
    (void)GC_call_with_alloc_lock(reachable_objs_count_enumerator,
                                  &obj_count);
    GC_printf("Completed %u tests\n", n_tests);
    GC_printf("Allocated %d collectable objects\n", (int)collectable_count);
    GC_printf("Allocated %d uncollectable objects\n",
                  (int)uncollectable_count);
    GC_printf("Allocated %d atomic objects\n", (int)atomic_count);
    GC_printf("Reallocated %d objects\n", (int)realloc_count);
    GC_printf("Finalized %d/%d objects - ",
                  finalized_count, finalizable_count);
#   ifndef GC_NO_FINALIZATION
#     ifdef FINALIZE_ON_DEMAND
        if (finalized_count != late_finalize_count) {
            GC_printf("Demand finalization error\n");
            FAIL;
        }
#     endif
      if (finalized_count > finalizable_count
          || finalized_count < finalizable_count/2) {
        GC_printf("finalization is probably broken\n");
        FAIL;
      } else {
        GC_printf("finalization is probably ok\n");
      }
      still_live = 0;
      for (i = 0; i < MAX_FINALIZED; i++) {
        if (live_indicators[i] != 0) {
            still_live++;
        }
#       ifndef GC_LONG_REFS_NOT_NEEDED
          if (live_long_refs[i] != NULL) {
              still_long_live++;
          }
#       endif
      }
      i = finalizable_count - finalized_count - still_live;
      if (0 != i) {
        GC_printf("%d disappearing links remain and %d more objects "
                      "were not finalized\n", still_live, i);
        if (i > 10) {
            GC_printf("\tVery suspicious!\n");
        } else {
            GC_printf("\tSlightly suspicious, but probably OK\n");
        }
      }
#     ifndef GC_LONG_REFS_NOT_NEEDED
        if (0 != still_long_live) {
          GC_printf("%d 'long' links remain\n", still_long_live);
        }
#     endif
#   endif
    GC_printf("Total number of bytes allocated is %lu\n",
                  (unsigned long)GC_get_total_bytes());
    GC_printf("Total memory use by allocated blocks is %lu bytes\n",
              (unsigned long)GC_get_memory_use());
    GC_printf("Final heap size is %lu bytes\n",
                  (unsigned long)GC_get_heap_size());
    if (GC_get_total_bytes() < (size_t)n_tests *
#   ifdef VERY_SMALL_CONFIG
        2700000
#   else
        33500000
#   endif
        ) {
      GC_printf("Incorrect execution - missed some allocations\n");
      FAIL;
    }
    if (GC_get_heap_size() + GC_get_unmapped_bytes() > max_heap_sz) {
        GC_printf("Unexpected heap growth - collector may be broken"
                  " (heapsize: %lu, expected: %lu)\n",
            (unsigned long)(GC_get_heap_size() + GC_get_unmapped_bytes()),
            (unsigned long)max_heap_sz);
        FAIL;
    }
    GC_printf("Final number of reachable objects is %u\n", obj_count);

#   ifndef GC_GET_HEAP_USAGE_NOT_NEEDED
      /* Get global counters (just to check the functions work).  */
      GC_get_heap_usage_safe(NULL, NULL, NULL, NULL, NULL);
      {
        struct GC_prof_stats_s stats;
        (void)GC_get_prof_stats(&stats, sizeof(stats));
#       ifdef THREADS
          (void)GC_get_prof_stats_unsafe(&stats, sizeof(stats));
#       endif
      }
      (void)GC_get_size_map_at(-1);
      (void)GC_get_size_map_at(1);
#   endif

#   ifdef THREADS
      GC_unregister_my_thread(); /* just to check it works (for main) */
#   endif
    GC_printf("Completed %u collections", (unsigned)GC_get_gc_no());
#   ifndef NO_CLOCK
      GC_printf(" in %lu msecs", GC_get_full_gc_total_time());
#   endif
#   ifdef PARALLEL_MARK
      GC_printf(" (using %d marker threads)", GC_get_parallel() + 1);
#   endif
    GC_printf("\n" "Collector appears to work\n");
}

#if defined(MACOS)
void SetMinimumStack(long minSize)
{
        if (minSize > LMGetDefltStack())
        {
                long newApplLimit = (long) GetApplLimit()
                                        - (minSize - LMGetDefltStack());
                SetApplLimit((Ptr) newApplLimit);
                MaxApplZone();
        }
}

#define cMinStackSpace (512L * 1024L)

#endif

void GC_CALLBACK warn_proc(char *msg, GC_word p)
{
    GC_printf(msg, (unsigned long)p);
    /*FAIL;*/
}

#if defined(CPPCHECK)
# include "javaxfc.h" /* for GC_finalize_all */
# define UNTESTED(sym) GC_noop1((word)&sym)
#endif

#if defined(MSWINCE) && defined(UNDER_CE)
# define WINMAIN_LPTSTR LPWSTR
#else
# define WINMAIN_LPTSTR LPSTR
#endif

#if !defined(PCR) && !defined(GC_WIN32_THREADS) && !defined(GC_PTHREADS)

#if ((defined(MSWIN32) && !defined(__MINGW32__)) || defined(MSWINCE)) \
    && !defined(NO_WINMAIN_ENTRY)
  int APIENTRY WinMain(HINSTANCE instance GC_ATTR_UNUSED,
                       HINSTANCE prev GC_ATTR_UNUSED,
                       WINMAIN_LPTSTR cmd GC_ATTR_UNUSED,
                       int n GC_ATTR_UNUSED)
#elif defined(RTEMS)
# include <bsp.h>
# define CONFIGURE_APPLICATION_NEEDS_CLOCK_DRIVER
# define CONFIGURE_APPLICATION_NEEDS_CONSOLE_DRIVER
# define CONFIGURE_RTEMS_INIT_TASKS_TABLE
# define CONFIGURE_MAXIMUM_TASKS 1
# define CONFIGURE_INIT
# define CONFIGURE_INIT_TASK_STACK_SIZE (64*1024)
# include <rtems/confdefs.h>
  rtems_task Init(rtems_task_argument ignord)
#else
  int main(void)
#endif
{
#   if defined(CPPCHECK) && !defined(NO_WINMAIN_ENTRY) \
       && ((defined(MSWIN32) && !defined(__MINGW32__)) || defined(MSWINCE))
      GC_noop1((GC_word)&WinMain);
#   elif defined(CPPCHECK) && defined(RTEMS)
      GC_noop1((GC_word)&Init);
#   endif
    n_tests = 0;
#   if defined(MACOS)
        /* Make sure we have lots and lots of stack space.      */
        SetMinimumStack(cMinStackSpace);
        /* Cheat and let stdio initialize toolbox for us.       */
        printf("Testing GC Macintosh port\n");
#   endif
    GC_COND_INIT();
    GC_set_warn_proc(warn_proc);
#   if !defined(GC_DISABLE_INCREMENTAL) \
       && (defined(TEST_DEFAULT_VDB) || !defined(DEFAULT_VDB))
#     if !defined(MAKE_BACK_GRAPH) && !defined(NO_INCREMENTAL) \
         && !(defined(MPROTECT_VDB) && defined(USE_MUNMAP))
        GC_enable_incremental();
#     endif
      if (GC_is_incremental_mode()) {
        GC_printf("Switched to incremental mode\n");
#       ifdef PROC_VDB
          GC_printf("Reading dirty bits from /proc\n");
#       elif defined(GWW_VDB)
          GC_printf("Using GetWriteWatch-based implementation\n");
#         ifdef MPROTECT_VDB
            GC_printf("Or emulating dirty bits with mprotect/signals\n");
#         endif
#       elif defined(MPROTECT_VDB)
          GC_printf("Emulating dirty bits with mprotect/signals\n");
#       endif /* MPROTECT_VDB && !GWW_VDB */
      }
#   endif
    set_print_procs();
    run_one_test();
    check_heap_stats();
#   ifndef MSWINCE
      fflush(stdout);
#   endif
#   if defined(CPPCHECK)
       /* Entry points we should be testing, but aren't.        */
#      ifndef GC_DEBUG
         UNTESTED(GC_debug_generic_or_special_malloc);
         UNTESTED(GC_debug_register_displacement);
         UNTESTED(GC_post_incr);
         UNTESTED(GC_pre_incr);
#        ifdef GC_GCJ_SUPPORT
           UNTESTED(GC_debug_gcj_malloc);
#        endif
#      endif
#      ifdef AMIGA
#        ifdef GC_AMIGA_FASTALLOC
           UNTESTED(GC_amiga_get_mem);
#        endif
#        ifndef GC_AMIGA_ONLYFAST
           UNTESTED(GC_amiga_set_toany);
#        endif
#      endif
#      if defined(MACOS) && defined(USE_TEMPORARY_MEMORY)
         UNTESTED(GC_MacTemporaryNewPtr);
#      endif
#      if !defined(_M_AMD64) && defined(_MSC_VER)
         UNTESTED(GetFileLineFromStack);
         UNTESTED(GetModuleNameFromStack);
         UNTESTED(GetSymbolNameFromStack);
#      endif
       UNTESTED(GC_get_bytes_since_gc);
       UNTESTED(GC_get_dont_expand);
       UNTESTED(GC_get_dont_precollect);
       UNTESTED(GC_get_finalize_on_demand);
       UNTESTED(GC_get_finalizer_notifier);
       UNTESTED(GC_get_find_leak);
       UNTESTED(GC_get_force_unmap_on_gcollect);
       UNTESTED(GC_get_free_bytes);
       UNTESTED(GC_get_free_space_divisor);
       UNTESTED(GC_get_full_freq);
       UNTESTED(GC_get_java_finalization);
       UNTESTED(GC_get_max_retries);
       UNTESTED(GC_get_no_dls);
       UNTESTED(GC_get_non_gc_bytes);
       UNTESTED(GC_get_on_collection_event);
       UNTESTED(GC_get_on_heap_resize);
       UNTESTED(GC_get_pages_executable);
       UNTESTED(GC_get_push_other_roots);
       UNTESTED(GC_get_start_callback);
       UNTESTED(GC_get_stop_func);
       UNTESTED(GC_get_time_limit);
       UNTESTED(GC_get_warn_proc);
       UNTESTED(GC_is_disabled);
       UNTESTED(GC_set_dont_precollect);
       UNTESTED(GC_set_finalize_on_demand);
       UNTESTED(GC_set_finalizer_notifier);
       UNTESTED(GC_set_free_space_divisor);
       UNTESTED(GC_set_full_freq);
       UNTESTED(GC_set_java_finalization);
       UNTESTED(GC_set_max_retries);
       UNTESTED(GC_set_no_dls);
       UNTESTED(GC_set_non_gc_bytes);
       UNTESTED(GC_set_on_collection_event);
       UNTESTED(GC_set_on_heap_resize);
       UNTESTED(GC_set_oom_fn);
       UNTESTED(GC_set_pages_executable);
       UNTESTED(GC_set_push_other_roots);
       UNTESTED(GC_set_start_callback);
       UNTESTED(GC_set_stop_func);
       UNTESTED(GC_set_time_limit);
       UNTESTED(GC_malloc_explicitly_typed_ignore_off_page);
       UNTESTED(GC_debug_change_stubborn);
       UNTESTED(GC_debug_strndup);
       UNTESTED(GC_deinit);
       UNTESTED(GC_strndup);
       UNTESTED(GC_posix_memalign);
       UNTESTED(GC_new_free_list);
       UNTESTED(GC_new_kind);
       UNTESTED(GC_new_proc);
       UNTESTED(GC_clear_roots);
       UNTESTED(GC_exclude_static_roots);
       UNTESTED(GC_expand_hp);
       UNTESTED(GC_register_describe_type_fn);
       UNTESTED(GC_register_has_static_roots_callback);
#      if !defined(PCR) && !defined(SMALL_CONFIG)
         UNTESTED(GC_get_abort_func);
         UNTESTED(GC_set_abort_func);
#      endif
#      ifdef GC_GCJ_SUPPORT
         UNTESTED(GC_gcj_malloc_ignore_off_page);
#      endif
#      ifndef NO_DEBUGGING
         UNTESTED(GC_dump_regions);
         UNTESTED(GC_is_tmp_root);
         UNTESTED(GC_print_free_list);
#      endif
#      ifdef TRACE_BUF
         UNTESTED(GC_print_trace);
#      endif
#      ifndef GC_NO_FINALIZATION
         UNTESTED(GC_debug_register_finalizer_unreachable);
         UNTESTED(GC_get_await_finalize_proc);
         UNTESTED(GC_register_disappearing_link);
         UNTESTED(GC_set_await_finalize_proc);
         UNTESTED(GC_should_invoke_finalizers);
#        ifndef JAVA_FINALIZATION_NOT_NEEDED
           UNTESTED(GC_finalize_all);
#        endif
#        ifndef NO_DEBUGGING
           UNTESTED(GC_dump_finalization);
#        endif
#        ifndef GC_TOGGLE_REFS_NOT_NEEDED
           UNTESTED(GC_get_toggleref_func);
           UNTESTED(GC_set_toggleref_func);
           UNTESTED(GC_toggleref_add);
#        endif
#      endif
#      if !defined(OS2) && !defined(MACOS) && !defined(GC_ANDROID_LOG) \
          && !defined(MSWIN32) && !defined(MSWINCE)
         UNTESTED(GC_set_log_fd);
#      endif
#      ifdef THREADS
         UNTESTED(GC_allow_register_threads);
         UNTESTED(GC_get_on_thread_event);
         UNTESTED(GC_register_altstack);
         UNTESTED(GC_set_on_thread_event);
#      endif
#      ifndef REDIRECT_MALLOC_IN_HEADER
#        ifdef REDIRECT_MALLOC
#          ifndef strndup
             UNTESTED(strndup);
#          endif
#          ifndef strdup
             UNTESTED(strdup);
#          endif
#        endif
#        ifdef REDIRECT_REALLOC
           UNTESTED(realloc);
#        endif
#      endif /* !REDIRECT_MALLOC_IN_HEADER */
#      ifdef GC_REQUIRE_WCSDUP
         UNTESTED(GC_wcsdup);
         UNTESTED(GC_debug_wcsdup);
#      endif
#   endif
#   ifdef MSWIN32
      GC_win32_free_heap();
#   endif
#   ifdef RTEMS
      exit(0);
#   else
      return(0);
#   endif
}
# endif /* !GC_WIN32_THREADS && !GC_PTHREADS */

#if defined(GC_WIN32_THREADS) && !defined(GC_PTHREADS)

DWORD __stdcall thr_run_one_test(void * arg GC_ATTR_UNUSED)
{
  run_one_test();
  return 0;
}

#ifdef MSWINCE
HANDLE win_created_h;
HWND win_handle;

LRESULT CALLBACK window_proc(HWND hwnd, UINT uMsg, WPARAM wParam,
                             LPARAM lParam)
{
  LRESULT ret = 0;
  switch (uMsg) {
    case WM_HIBERNATE:
      GC_printf("Received WM_HIBERNATE, calling GC_gcollect\n");
      /* Force "unmap as much memory as possible" mode. */
      GC_gcollect_and_unmap();
      break;
    case WM_CLOSE:
      GC_printf("Received WM_CLOSE, closing window\n");
      DestroyWindow(hwnd);
      break;
    case WM_DESTROY:
      PostQuitMessage(0);
      break;
    default:
      ret = DefWindowProc(hwnd, uMsg, wParam, lParam);
      break;
  }
  return ret;
}

DWORD __stdcall thr_window(void * arg GC_ATTR_UNUSED)
{
  WNDCLASS win_class = {
    CS_NOCLOSE,
    window_proc,
    0,
    0,
    GetModuleHandle(NULL),
    NULL,
    NULL,
    (HBRUSH)(COLOR_APPWORKSPACE+1),
    NULL,
    TEXT("GCtestWindow")
  };
  MSG msg;

  if (!RegisterClass(&win_class))
    FAIL;

  win_handle = CreateWindowEx(
    0,
    TEXT("GCtestWindow"),
    TEXT("GCtest"),
    0,
    CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT,
    NULL,
    NULL,
    GetModuleHandle(NULL),
    NULL);

  if (win_handle == NULL)
    FAIL;

  SetEvent(win_created_h);

  ShowWindow(win_handle, SW_SHOW);
  UpdateWindow(win_handle);

  while (GetMessage(&msg, NULL, 0, 0)) {
    TranslateMessage(&msg);
    DispatchMessage(&msg);
  }

  return 0;
}
#endif

#if !defined(NO_WINMAIN_ENTRY)
  int APIENTRY WinMain(HINSTANCE instance GC_ATTR_UNUSED,
                       HINSTANCE prev GC_ATTR_UNUSED,
                       WINMAIN_LPTSTR cmd GC_ATTR_UNUSED,
                       int n GC_ATTR_UNUSED)
#else
  int main(void)
#endif
{
# if NTHREADS > 0
   HANDLE h[NTHREADS];
   int i;
# endif
# ifdef MSWINCE
    HANDLE win_thr_h;
# endif
  DWORD thread_id;

# if defined(CPPCHECK) && !defined(NO_WINMAIN_ENTRY)
    GC_noop1((GC_word)&WinMain);
# endif
# if defined(GC_DLL) && !defined(GC_NO_THREADS_DISCOVERY) \
        && !defined(MSWINCE) && !defined(THREAD_LOCAL_ALLOC) \
        && !defined(PARALLEL_MARK)
    GC_use_threads_discovery();
                /* Test with implicit thread registration if possible. */
    GC_printf("Using DllMain to track threads\n");
# endif
  GC_COND_INIT();
# if !defined(MAKE_BACK_GRAPH) && !defined(NO_INCREMENTAL)
    GC_enable_incremental();
# endif
  InitializeCriticalSection(&incr_cs);
  GC_set_warn_proc(warn_proc);
# ifdef MSWINCE
    win_created_h = CreateEvent(NULL, FALSE, FALSE, NULL);
    if (win_created_h == (HANDLE)NULL) {
      GC_printf("Event creation failed %d\n", (int)GetLastError());
      FAIL;
    }
    win_thr_h = GC_CreateThread(NULL, 0, thr_window, 0, 0, &thread_id);
    if (win_thr_h == (HANDLE)NULL) {
      GC_printf("Thread creation failed %d\n", (int)GetLastError());
      FAIL;
    }
    if (WaitForSingleObject(win_created_h, INFINITE) != WAIT_OBJECT_0)
      FAIL;
    CloseHandle(win_created_h);
# endif
  set_print_procs();
# if NTHREADS > 0
   for (i = 0; i < NTHREADS; i++) {
    h[i] = GC_CreateThread(NULL, 0, thr_run_one_test, 0, 0, &thread_id);
    if (h[i] == (HANDLE)NULL) {
      GC_printf("Thread creation failed %d\n", (int)GetLastError());
      FAIL;
    }
   }
# endif /* NTHREADS > 0 */
  run_one_test();
# if NTHREADS > 0
   for (i = 0; i < NTHREADS; i++) {
    if (WaitForSingleObject(h[i], INFINITE) != WAIT_OBJECT_0) {
      GC_printf("Thread wait failed %d\n", (int)GetLastError());
      FAIL;
    }
   }
# endif /* NTHREADS > 0 */
# ifdef MSWINCE
    PostMessage(win_handle, WM_CLOSE, 0, 0);
    if (WaitForSingleObject(win_thr_h, INFINITE) != WAIT_OBJECT_0)
      FAIL;
# endif
  check_heap_stats();
# if defined(CPPCHECK) && defined(GC_WIN32_THREADS)
    UNTESTED(GC_ExitThread);
#   if !defined(MSWINCE) && !defined(CYGWIN32)
      UNTESTED(GC_beginthreadex);
      UNTESTED(GC_endthreadex);
#   endif
# endif
  return(0);
}

#endif /* GC_WIN32_THREADS */


#ifdef PCR
int test(void)
{
    PCR_Th_T * th1;
    PCR_Th_T * th2;
    int code;

#   if defined(CPPCHECK)
      GC_noop1((word)&PCR_GC_Run);
      GC_noop1((word)&PCR_GC_Setup);
      GC_noop1((word)&test);
#   endif
    n_tests = 0;
    /* GC_enable_incremental(); */
    GC_set_warn_proc(warn_proc);
    set_print_procs();
    th1 = PCR_Th_Fork(run_one_test, 0);
    th2 = PCR_Th_Fork(run_one_test, 0);
    run_one_test();
    if (PCR_Th_T_Join(th1, &code, NIL, PCR_allSigsBlocked, PCR_waitForever)
        != PCR_ERes_okay || code != 0) {
        GC_printf("Thread 1 failed\n");
    }
    if (PCR_Th_T_Join(th2, &code, NIL, PCR_allSigsBlocked, PCR_waitForever)
        != PCR_ERes_okay || code != 0) {
        GC_printf("Thread 2 failed\n");
    }
    check_heap_stats();
    return(0);
}
#endif

#if defined(GC_PTHREADS)
void * thr_run_one_test(void * arg GC_ATTR_UNUSED)
{
    run_one_test();
    return(0);
}

#ifdef GC_DEBUG
#  define GC_free GC_debug_free
#endif

int main(void)
{
#   if NTHREADS > 0
      pthread_t th[NTHREADS];
      int i;
#   endif
    pthread_attr_t attr;
    int code;
#   ifdef GC_IRIX_THREADS
        /* Force a larger stack to be preallocated      */
        /* Since the initial can't always grow later.   */
        *((volatile char *)&code - 1024*1024) = 0;      /* Require 1 MB */
#   endif /* GC_IRIX_THREADS */
#   if defined(GC_HPUX_THREADS)
        /* Default stack size is too small, especially with the 64 bit ABI */
        /* Increase it.                                                    */
        if (pthread_default_stacksize_np(1024*1024, 0) != 0) {
          GC_printf("pthread_default_stacksize_np failed\n");
        }
#   endif       /* GC_HPUX_THREADS */
#   ifdef PTW32_STATIC_LIB
        pthread_win32_process_attach_np ();
        pthread_win32_thread_attach_np ();
#   endif
#   if defined(GC_DARWIN_THREADS) && !defined(GC_NO_THREADS_DISCOVERY) \
        && !defined(DARWIN_DONT_PARSE_STACK) && !defined(THREAD_LOCAL_ALLOC)
      /* Test with the Darwin implicit thread registration. */
      GC_use_threads_discovery();
      GC_printf("Using Darwin task-threads-based world stop and push\n");
#   endif
    GC_COND_INIT();

    if ((code = pthread_attr_init(&attr)) != 0) {
      GC_printf("pthread_attr_init failed, error=%d\n", code);
      FAIL;
    }
#   if defined(GC_IRIX_THREADS) || defined(GC_FREEBSD_THREADS) \
        || defined(GC_DARWIN_THREADS) || defined(GC_AIX_THREADS) \
        || defined(GC_OPENBSD_THREADS)
        if ((code = pthread_attr_setstacksize(&attr, 1000 * 1024)) != 0) {
          GC_printf("pthread_attr_setstacksize failed, error=%d\n", code);
          FAIL;
        }
#   endif
    n_tests = 0;
#   if !defined(GC_DISABLE_INCREMENTAL) \
       && (defined(TEST_DEFAULT_VDB) || !defined(DEFAULT_VDB))
#     if !defined(REDIRECT_MALLOC) && !defined(MAKE_BACK_GRAPH) \
         && !defined(USE_PROC_FOR_LIBRARIES) && !defined(NO_INCREMENTAL) \
         && !defined(USE_MUNMAP)
        GC_enable_incremental();
#     endif
      if (GC_is_incremental_mode()) {
        GC_printf("Switched to incremental mode\n");
#       ifdef MPROTECT_VDB
          GC_printf("Emulating dirty bits with mprotect/signals\n");
#       endif
      }
#   endif
    GC_set_min_bytes_allocd(1);
    if (GC_get_min_bytes_allocd() != 1)
        FAIL;
    GC_set_rate(10);
    GC_set_max_prior_attempts(1);
    if (GC_get_rate() != 10 || GC_get_max_prior_attempts() != 1)
        FAIL;
    GC_set_warn_proc(warn_proc);
    if ((code = pthread_key_create(&fl_key, 0)) != 0) {
        GC_printf("Key creation failed %d\n", code);
        FAIL;
    }
    set_print_procs();
#   if NTHREADS > 0
      for (i = 0; i < NTHREADS; ++i) {
        if ((code = pthread_create(th+i, &attr, thr_run_one_test, 0)) != 0) {
          GC_printf("Thread %d creation failed %d\n", i, code);
          FAIL;
        }
      }
#   endif
    run_one_test();
#   if NTHREADS > 0
      for (i = 0; i < NTHREADS; ++i) {
        if ((code = pthread_join(th[i], 0)) != 0) {
          GC_printf("Thread %d failed %d\n", i, code);
          FAIL;
        }
      }
#   endif
    check_heap_stats();
    (void)fflush(stdout);
    (void)pthread_attr_destroy(&attr);
#   if defined(CPPCHECK)
      UNTESTED(GC_set_suspend_signal);
      UNTESTED(GC_set_thr_restart_signal);
#     ifndef GC_NO_DLOPEN
        UNTESTED(GC_dlopen);
#     endif
#     ifndef GC_NO_PTHREAD_CANCEL
        UNTESTED(GC_pthread_cancel);
#     endif
#     ifdef GC_HAVE_PTHREAD_EXIT
        UNTESTED(GC_pthread_exit);
#     endif
#     ifndef GC_NO_PTHREAD_SIGMASK
        UNTESTED(GC_pthread_sigmask);
#     endif
#     ifdef NO_TEST_HANDLE_FORK
        UNTESTED(GC_atfork_child);
        UNTESTED(GC_atfork_parent);
        UNTESTED(GC_atfork_prepare);
        UNTESTED(GC_set_handle_fork);
        UNTESTED(GC_start_mark_threads);
#     endif
#   endif /* CPPCHECK */
#   ifdef PTW32_STATIC_LIB
        pthread_win32_thread_detach_np ();
        pthread_win32_process_detach_np ();
#   endif
    return(0);
}
#endif /* GC_PTHREADS */
