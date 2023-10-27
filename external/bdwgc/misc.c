/*
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1999-2001 by Hewlett-Packard Company. All rights reserved.
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

#include "private/gc_pmark.h"
#include "private/gc_priv.h"

#include <stdio.h>
#include <limits.h>
#include <stdarg.h>

#ifndef MSWINCE
# include <signal.h>
#endif

#ifdef GC_SOLARIS_THREADS
# include <sys/syscall.h>
#endif

#if defined(MSWIN32) || defined(MSWINCE) \
    || (defined(CYGWIN32) && defined(GC_READ_ENV_FILE))
# ifndef WIN32_LEAN_AND_MEAN
#   define WIN32_LEAN_AND_MEAN 1
# endif
# define NOSERVICE
# include <windows.h>
#ifdef MSWINRT
# include <windows.storage.h>
  // This API is defined in roapi.h, but we cannot include it here since it does not compile in C -_-
  DECLSPEC_IMPORT HRESULT WINAPI RoGetActivationFactory(HSTRING activatableClassId, REFIID iid, void** factory);
#endif
#endif

#if defined(UNIX_LIKE) || defined(CYGWIN32) || defined(SYMBIAN)
# include <fcntl.h>
# include <sys/types.h>
# include <sys/stat.h>
#endif

#ifdef NONSTOP
# include <floss.h>
#endif

#ifdef THREADS
# ifdef PCR
#   include "il/PCR_IL.h"
    GC_INNER PCR_Th_ML GC_allocate_ml;
# elif defined(SN_TARGET_PSP2)
    GC_INNER WapiMutex GC_allocate_ml_PSP2 = { 0, NULL };
# elif defined(GC_PTHREAD_MUTEX) || defined(SN_TARGET_PS3)
#   include <pthread.h>
    GC_INNER pthread_mutex_t GC_allocate_ml;
# endif
  /* For other platforms with threads, the lock and possibly            */
  /* GC_lock_holder variables are defined in the thread support code.   */
#endif /* THREADS */

#ifdef DYNAMIC_LOADING
  /* We need to register the main data segment.  Returns  TRUE unless   */
  /* this is done implicitly as part of dynamic library registration.   */
# define GC_REGISTER_MAIN_STATIC_DATA() GC_register_main_static_data()
#elif defined(GC_DONT_REGISTER_MAIN_STATIC_DATA)
# define GC_REGISTER_MAIN_STATIC_DATA() FALSE
#else
  /* Don't unnecessarily call GC_register_main_static_data() in case    */
  /* dyn_load.c isn't linked in.                                        */
# define GC_REGISTER_MAIN_STATIC_DATA() TRUE
#endif

#ifdef NEED_CANCEL_DISABLE_COUNT
  __thread unsigned char GC_cancel_disable_count = 0;
#endif

GC_FAR struct _GC_arrays GC_arrays /* = { 0 } */;

GC_INNER GC_bool GC_debugging_started = FALSE;
                /* defined here so we don't have to load dbg_mlc.o */

ptr_t GC_stackbottom = 0;

#ifdef IA64
  ptr_t GC_register_stackbottom = 0;
#endif

int GC_dont_gc = FALSE;

int GC_dont_precollect = FALSE;

GC_bool GC_quiet = 0; /* used also in pcr_interface.c */

#if !defined(NO_CLOCK) || !defined(SMALL_CONFIG)
  int GC_print_stats = 0;
#endif

#ifdef GC_PRINT_BACK_HEIGHT
  GC_INNER GC_bool GC_print_back_height = TRUE;
#else
  GC_INNER GC_bool GC_print_back_height = FALSE;
#endif

#ifndef NO_DEBUGGING
# ifdef GC_DUMP_REGULARLY
    GC_INNER GC_bool GC_dump_regularly = TRUE;
                                /* Generate regular debugging dumps. */
# else
    GC_INNER GC_bool GC_dump_regularly = FALSE;
# endif
# ifndef NO_CLOCK
    STATIC CLOCK_TYPE GC_init_time;
                /* The time that the GC was initialized at.     */
# endif
#endif /* !NO_DEBUGGING */

#ifdef KEEP_BACK_PTRS
  GC_INNER long GC_backtraces = 0;
                /* Number of random backtraces to generate for each GC. */
#endif

#ifdef FIND_LEAK
  int GC_find_leak = 1;
#else
  int GC_find_leak = 0;
#endif

#ifndef SHORT_DBG_HDRS
# ifdef GC_FINDLEAK_DELAY_FREE
    GC_INNER GC_bool GC_findleak_delay_free = TRUE;
# else
    GC_INNER GC_bool GC_findleak_delay_free = FALSE;
# endif
#endif /* !SHORT_DBG_HDRS */

#ifdef ALL_INTERIOR_POINTERS
  int GC_all_interior_pointers = 1;
#else
  int GC_all_interior_pointers = 0;
#endif

#ifdef FINALIZE_ON_DEMAND
  int GC_finalize_on_demand = 1;
#else
  int GC_finalize_on_demand = 0;
#endif

#ifdef JAVA_FINALIZATION
  int GC_java_finalization = 1;
#else
  int GC_java_finalization = 0;
#endif

/* All accesses to it should be synchronized to avoid data races.       */
GC_finalizer_notifier_proc GC_finalizer_notifier =
                                        (GC_finalizer_notifier_proc)0;

#ifdef GC_FORCE_UNMAP_ON_GCOLLECT
  /* Has no effect unless USE_MUNMAP.                           */
  /* Has no effect on implicitly-initiated garbage collections. */
  GC_INNER GC_bool GC_force_unmap_on_gcollect = TRUE;
#else
  GC_INNER GC_bool GC_force_unmap_on_gcollect = FALSE;
#endif

#ifndef GC_LARGE_ALLOC_WARN_INTERVAL
# define GC_LARGE_ALLOC_WARN_INTERVAL 5
#endif
GC_INNER long GC_large_alloc_warn_interval = GC_LARGE_ALLOC_WARN_INTERVAL;
                        /* Interval between unsuppressed warnings.      */

STATIC void * GC_CALLBACK GC_default_oom_fn(
                                        size_t bytes_requested GC_ATTR_UNUSED)
{
    return(0);
}

/* All accesses to it should be synchronized to avoid data races.       */
GC_oom_func GC_oom_fn = GC_default_oom_fn;

#ifdef CAN_HANDLE_FORK
# ifdef HANDLE_FORK
    GC_INNER int GC_handle_fork = 1;
                        /* The value is examined by GC_thr_init.        */
# else
    GC_INNER int GC_handle_fork = FALSE;
# endif

#elif !defined(HAVE_NO_FORK)

  /* Same as above but with GC_CALL calling conventions.  */
  GC_API void GC_CALL GC_atfork_prepare(void)
  {
#   ifdef THREADS
      ABORT("fork() handling unsupported");
#   endif
  }

  GC_API void GC_CALL GC_atfork_parent(void)
  {
    /* empty */
  }

  GC_API void GC_CALL GC_atfork_child(void)
  {
    /* empty */
  }
#endif /* !CAN_HANDLE_FORK && !HAVE_NO_FORK */

/* Overrides the default automatic handle-fork mode.  Has effect only   */
/* if called before GC_INIT.                                            */
GC_API void GC_CALL GC_set_handle_fork(int value GC_ATTR_UNUSED)
{
# ifdef CAN_HANDLE_FORK
    if (!GC_is_initialized)
      GC_handle_fork = value >= -1 ? value : 1;
                /* Map all negative values except for -1 to a positive one. */
# elif defined(THREADS) || (defined(DARWIN) && defined(MPROTECT_VDB))
    if (!GC_is_initialized && value) {
#     ifndef SMALL_CONFIG
        GC_init(); /* just to initialize GC_stderr */
#     endif
      ABORT("fork() handling unsupported");
    }
# else
    /* No at-fork handler is needed in the single-threaded mode.        */
# endif
}

/* Set things up so that GC_size_map[i] >= granules(i),                 */
/* but not too much bigger                                              */
/* and so that size_map contains relatively few distinct entries        */
/* This was originally stolen from Russ Atkinson's Cedar                */
/* quantization algorithm (but we precompute it).                       */
STATIC void GC_init_size_map(void)
{
    size_t i;

    /* Map size 0 to something bigger.                  */
    /* This avoids problems at lower levels.            */
      GC_size_map[0] = 1;
    for (i = 1; i <= GRANULES_TO_BYTES(TINY_FREELISTS-1) - EXTRA_BYTES; i++) {
        GC_size_map[i] = ROUNDED_UP_GRANULES(i);
#       ifndef _MSC_VER
          GC_ASSERT(GC_size_map[i] < TINY_FREELISTS);
          /* Seems to tickle bug in VC++ 2008 for AMD64 */
#       endif
    }
    /* We leave the rest of the array to be filled in on demand. */
}

/*
 * The following is a gross hack to deal with a problem that can occur
 * on machines that are sloppy about stack frame sizes, notably SPARC.
 * Bogus pointers may be written to the stack and not cleared for
 * a LONG time, because they always fall into holes in stack frames
 * that are not written.  We partially address this by clearing
 * sections of the stack whenever we get control.
 */

#ifndef SMALL_CLEAR_SIZE
# define SMALL_CLEAR_SIZE 256   /* Clear this much every time.  */
#endif

#if defined(ALWAYS_SMALL_CLEAR_STACK) || defined(STACK_NOT_SCANNED)
  GC_API void * GC_CALL GC_clear_stack(void *arg)
  {
#   ifndef STACK_NOT_SCANNED
      word volatile dummy[SMALL_CLEAR_SIZE];
      BZERO((/* no volatile */ void *)dummy, sizeof(dummy));
#   endif
    return arg;
  }
#else

# ifdef THREADS
#   define BIG_CLEAR_SIZE 2048  /* Clear this much now and then.        */
# else
    STATIC word GC_stack_last_cleared = 0; /* GC_no when we last did this */
    STATIC ptr_t GC_min_sp = NULL;
                        /* Coolest stack pointer value from which       */
                        /* we've already cleared the stack.             */
    STATIC ptr_t GC_high_water = NULL;
                        /* "hottest" stack pointer value we have seen   */
                        /* recently.  Degrades over time.               */
    STATIC word GC_bytes_allocd_at_reset = 0;
#   define DEGRADE_RATE 50
# endif

# if defined(ASM_CLEAR_CODE)
    void *GC_clear_stack_inner(void *, ptr_t);
# else
    /* Clear the stack up to about limit.  Return arg.  This function   */
    /* is not static because it could also be erroneously defined in .S */
    /* file, so this error would be caught by the linker.               */
    void *GC_clear_stack_inner(void *arg,
#                           if defined(__APPLE_CC__) && !GC_CLANG_PREREQ(6, 0)
                               volatile /* to workaround some bug */
#                           endif
                               ptr_t limit)
    {
#     define CLEAR_SIZE 213 /* granularity */
      volatile word dummy[CLEAR_SIZE];

      BZERO((/* no volatile */ void *)dummy, sizeof(dummy));
      if ((word)GC_approx_sp() COOLER_THAN (word)limit) {
        (void)GC_clear_stack_inner(arg, limit);
      }
      /* Make sure the recursive call is not a tail call, and the bzero */
      /* call is not recognized as dead code.                           */
      GC_noop1((word)dummy);
      return(arg);
    }
# endif /* !ASM_CLEAR_CODE */

# ifdef THREADS
    /* Used to occasionally clear a bigger chunk.       */
    /* TODO: Should be more random than it is ...       */
    GC_ATTR_NO_SANITIZE_THREAD
    static unsigned next_random_no(void)
    {
      static unsigned random_no = 0;
      return ++random_no % 13;
    }
# endif /* THREADS */

/* Clear some of the inaccessible part of the stack.  Returns its       */
/* argument, so it can be used in a tail call position, hence clearing  */
/* another frame.                                                       */
  GC_API void * GC_CALL GC_clear_stack(void *arg)
  {
    ptr_t sp = GC_approx_sp();  /* Hotter than actual sp */
#   ifdef THREADS
        word volatile dummy[SMALL_CLEAR_SIZE];
#   endif

#   define SLOP 400
        /* Extra bytes we clear every time.  This clears our own        */
        /* activation record, and should cause more frequent            */
        /* clearing near the cold end of the stack, a good thing.       */
#   define GC_SLOP 4000
        /* We make GC_high_water this much hotter than we really saw    */
        /* it, to cover for GC noise etc. above our current frame.      */
#   define CLEAR_THRESHOLD 100000
        /* We restart the clearing process after this many bytes of     */
        /* allocation.  Otherwise very heavily recursive programs       */
        /* with sparse stacks may result in heaps that grow almost      */
        /* without bounds.  As the heap gets larger, collection         */
        /* frequency decreases, thus clearing frequency would decrease, */
        /* thus more junk remains accessible, thus the heap gets        */
        /* larger ...                                                   */
#   ifdef THREADS
      if (next_random_no() == 0) {
        ptr_t limit = sp;

        MAKE_HOTTER(limit, BIG_CLEAR_SIZE*sizeof(word));
        limit = (ptr_t)((word)limit & ~0xf);
                        /* Make it sufficiently aligned for assembly    */
                        /* implementations of GC_clear_stack_inner.     */
        return GC_clear_stack_inner(arg, limit);
      }
      BZERO((void *)dummy, SMALL_CLEAR_SIZE*sizeof(word));
#   else
      if (GC_gc_no > GC_stack_last_cleared) {
        /* Start things over, so we clear the entire stack again */
        if (GC_stack_last_cleared == 0)
          GC_high_water = (ptr_t)GC_stackbottom;
        GC_min_sp = GC_high_water;
        GC_stack_last_cleared = GC_gc_no;
        GC_bytes_allocd_at_reset = GC_bytes_allocd;
      }
      /* Adjust GC_high_water */
      MAKE_COOLER(GC_high_water, WORDS_TO_BYTES(DEGRADE_RATE) + GC_SLOP);
      if ((word)sp HOTTER_THAN (word)GC_high_water) {
          GC_high_water = sp;
      }
      MAKE_HOTTER(GC_high_water, GC_SLOP);
      {
        ptr_t limit = GC_min_sp;

        MAKE_HOTTER(limit, SLOP);
        if ((word)sp COOLER_THAN (word)limit) {
          limit = (ptr_t)((word)limit & ~0xf);
                          /* Make it sufficiently aligned for assembly  */
                          /* implementations of GC_clear_stack_inner.   */
          GC_min_sp = sp;
          return GC_clear_stack_inner(arg, limit);
        }
      }
      if (GC_bytes_allocd - GC_bytes_allocd_at_reset > CLEAR_THRESHOLD) {
        /* Restart clearing process, but limit how much clearing we do. */
        GC_min_sp = sp;
        MAKE_HOTTER(GC_min_sp, CLEAR_THRESHOLD/4);
        if ((word)GC_min_sp HOTTER_THAN (word)GC_high_water)
          GC_min_sp = GC_high_water;
        GC_bytes_allocd_at_reset = GC_bytes_allocd;
      }
#   endif
    return arg;
  }

#endif /* !ALWAYS_SMALL_CLEAR_STACK && !STACK_NOT_SCANNED */

/* Return a pointer to the base address of p, given a pointer to a      */
/* an address within an object.  Return 0 o.w.                          */
GC_API void * GC_CALL GC_base(void * p)
{
    ptr_t r;
    struct hblk *h;
    bottom_index *bi;
    hdr *candidate_hdr;

    r = (ptr_t)p;
    if (!EXPECT(GC_is_initialized, TRUE)) return 0;
    h = HBLKPTR(r);
    GET_BI(r, bi);
    candidate_hdr = HDR_FROM_BI(bi, r);
    if (candidate_hdr == 0) return(0);
    /* If it's a pointer to the middle of a large object, move it       */
    /* to the beginning.                                                */
        while (IS_FORWARDING_ADDR_OR_NIL(candidate_hdr)) {
           h = FORWARDED_ADDR(h,candidate_hdr);
           r = (ptr_t)h;
           candidate_hdr = HDR(h);
        }
    if (HBLK_IS_FREE(candidate_hdr)) return(0);
    /* Make sure r points to the beginning of the object */
        r = (ptr_t)((word)r & ~(WORDS_TO_BYTES(1) - 1));
        {
            size_t offset = HBLKDISPL(r);
            word sz = candidate_hdr -> hb_sz;
            size_t obj_displ = offset % sz;
            ptr_t limit;

            r -= obj_displ;
            limit = r + sz;
            if ((word)limit > (word)(h + 1) && sz <= HBLKSIZE) {
                return(0);
            }
            if ((word)p >= (word)limit) return(0);
        }
    return((void *)r);
}

/* Return TRUE if and only if p points to somewhere in GC heap. */
GC_API int GC_CALL GC_is_heap_ptr(const void *p)
{
    bottom_index *bi;

    GC_ASSERT(GC_is_initialized);
    GET_BI(p, bi);
    return HDR_FROM_BI(bi, p) != 0;
}

/* Return the size of an object, given a pointer to its base.           */
/* (For small objects this also happens to work from interior pointers, */
/* but that shouldn't be relied upon.)                                  */
GC_API size_t GC_CALL GC_size(const void * p)
{
    hdr * hhdr = HDR(p);

    return (size_t)hhdr->hb_sz;
}


/* These getters remain unsynchronized for compatibility (since some    */
/* clients could call some of them from a GC callback holding the       */
/* allocator lock).                                                     */
GC_API size_t GC_CALL GC_get_heap_size(void)
{
    /* ignore the memory space returned to OS (i.e. count only the      */
    /* space owned by the garbage collector)                            */
    return (size_t)(GC_heapsize - GC_unmapped_bytes);
}

GC_API size_t GC_CALL GC_get_free_bytes(void)
{
    /* ignore the memory space returned to OS */
    return (size_t)(GC_large_free_bytes - GC_unmapped_bytes);
}

GC_API size_t GC_CALL GC_get_unmapped_bytes(void)
{
    return (size_t)GC_unmapped_bytes;
}

GC_API size_t GC_CALL GC_get_bytes_since_gc(void)
{
    return (size_t)GC_bytes_allocd;
}

GC_API size_t GC_CALL GC_get_total_bytes(void)
{
    return (size_t)(GC_bytes_allocd + GC_bytes_allocd_before_gc);
}

#ifndef GC_GET_HEAP_USAGE_NOT_NEEDED

GC_API size_t GC_CALL GC_get_size_map_at(int i)
{
  if ((unsigned)i > MAXOBJBYTES)
    return GC_SIZE_MAX;
  return GRANULES_TO_BYTES(GC_size_map[i]);
}

/* Return the heap usage information.  This is a thread-safe (atomic)   */
/* alternative for the five above getters.  NULL pointer is allowed for */
/* any argument.  Returned (filled in) values are of word type.         */
GC_API void GC_CALL GC_get_heap_usage_safe(GC_word *pheap_size,
                        GC_word *pfree_bytes, GC_word *punmapped_bytes,
                        GC_word *pbytes_since_gc, GC_word *ptotal_bytes)
{
  DCL_LOCK_STATE;

  LOCK();
  if (pheap_size != NULL)
    *pheap_size = GC_heapsize - GC_unmapped_bytes;
  if (pfree_bytes != NULL)
    *pfree_bytes = GC_large_free_bytes - GC_unmapped_bytes;
  if (punmapped_bytes != NULL)
    *punmapped_bytes = GC_unmapped_bytes;
  if (pbytes_since_gc != NULL)
    *pbytes_since_gc = GC_bytes_allocd;
  if (ptotal_bytes != NULL)
    *ptotal_bytes = GC_bytes_allocd + GC_bytes_allocd_before_gc;
  UNLOCK();
}

  GC_INNER word GC_reclaimed_bytes_before_gc = 0;

  /* Fill in GC statistics provided the destination is of enough size.  */
  static void fill_prof_stats(struct GC_prof_stats_s *pstats)
  {
    pstats->heapsize_full = GC_heapsize;
    pstats->free_bytes_full = GC_large_free_bytes;
    pstats->unmapped_bytes = GC_unmapped_bytes;
    pstats->bytes_allocd_since_gc = GC_bytes_allocd;
    pstats->allocd_bytes_before_gc = GC_bytes_allocd_before_gc;
    pstats->non_gc_bytes = GC_non_gc_bytes;
    pstats->gc_no = GC_gc_no; /* could be -1 */
#   ifdef PARALLEL_MARK
      pstats->markers_m1 = (word)GC_markers_m1;
#   else
      pstats->markers_m1 = 0; /* one marker */
#   endif
    pstats->bytes_reclaimed_since_gc = GC_bytes_found > 0 ?
                                        (word)GC_bytes_found : 0;
    pstats->reclaimed_bytes_before_gc = GC_reclaimed_bytes_before_gc;
    pstats->expl_freed_bytes_since_gc = GC_bytes_freed; /* since gc-7.7 */
  }

# include <string.h> /* for memset() */

  GC_API size_t GC_CALL GC_get_prof_stats(struct GC_prof_stats_s *pstats,
                                          size_t stats_sz)
  {
    struct GC_prof_stats_s stats;
    DCL_LOCK_STATE;

    LOCK();
    fill_prof_stats(stats_sz >= sizeof(stats) ? pstats : &stats);
    UNLOCK();

    if (stats_sz == sizeof(stats)) {
      return sizeof(stats);
    } else if (stats_sz > sizeof(stats)) {
      /* Fill in the remaining part with -1.    */
      memset((char *)pstats + sizeof(stats), 0xff, stats_sz - sizeof(stats));
      return sizeof(stats);
    } else {
      if (EXPECT(stats_sz > 0, TRUE))
        BCOPY(&stats, pstats, stats_sz);
      return stats_sz;
    }
  }

# ifdef THREADS
    /* The _unsafe version assumes the caller holds the allocation lock. */
    GC_API size_t GC_CALL GC_get_prof_stats_unsafe(
                                            struct GC_prof_stats_s *pstats,
                                            size_t stats_sz)
    {
      struct GC_prof_stats_s stats;

      if (stats_sz >= sizeof(stats)) {
        fill_prof_stats(pstats);
        if (stats_sz > sizeof(stats))
          memset((char *)pstats + sizeof(stats), 0xff,
                 stats_sz - sizeof(stats));
        return sizeof(stats);
      } else {
        if (EXPECT(stats_sz > 0, TRUE)) {
          fill_prof_stats(&stats);
          BCOPY(&stats, pstats, stats_sz);
        }
        return stats_sz;
      }
    }
# endif /* THREADS */

#endif /* !GC_GET_HEAP_USAGE_NOT_NEEDED */

#if defined(GC_DARWIN_THREADS) || defined(GC_OPENBSD_UTHREADS) \
    || defined(GC_WIN32_THREADS) || (defined(NACL) && defined(THREADS))
  /* GC does not use signals to suspend and restart threads.    */
  GC_API void GC_CALL GC_set_suspend_signal(int sig GC_ATTR_UNUSED)
  {
    /* empty */
  }

  GC_API void GC_CALL GC_set_thr_restart_signal(int sig GC_ATTR_UNUSED)
  {
    /* empty */
  }

  GC_API int GC_CALL GC_get_suspend_signal(void)
  {
    return -1;
  }

  GC_API int GC_CALL GC_get_thr_restart_signal(void)
  {
    return -1;
  }
#endif /* GC_DARWIN_THREADS || GC_WIN32_THREADS || ... */

#if !defined(_MAX_PATH) && (defined(MSWIN32) || defined(MSWINCE) \
                            || defined(CYGWIN32))
# define _MAX_PATH MAX_PATH
#endif

#ifdef GC_READ_ENV_FILE
  /* This works for Win32/WinCE for now.  Really useful only for WinCE. */
  STATIC char *GC_envfile_content = NULL;
                        /* The content of the GC "env" file with CR and */
                        /* LF replaced to '\0'.  NULL if the file is    */
                        /* missing or empty.  Otherwise, always ends    */
                        /* with '\0'.                                   */
  STATIC unsigned GC_envfile_length = 0;
                        /* Length of GC_envfile_content (if non-NULL).  */

# ifndef GC_ENVFILE_MAXLEN
#   define GC_ENVFILE_MAXLEN 0x4000
# endif

# define GC_ENV_FILE_EXT ".gc.env"

  /* The routine initializes GC_envfile_content from the GC "env" file. */
  STATIC void GC_envfile_init(void)
  {
#   if defined(MSWIN32) || defined(MSWINCE) || defined(CYGWIN32)
      HANDLE hFile;
      char *content;
      unsigned ofs;
      unsigned len;
      DWORD nBytesRead;
      TCHAR path[_MAX_PATH + 0x10]; /* buffer for path + ext */
      len = (unsigned)GetModuleFileName(NULL /* hModule */, path,
                                        _MAX_PATH + 1);
      /* If GetModuleFileName() has failed then len is 0. */
      if (len > 4 && path[len - 4] == (TCHAR)'.') {
        len -= 4; /* strip executable file extension */
      }
      BCOPY(TEXT(GC_ENV_FILE_EXT), &path[len], sizeof(TEXT(GC_ENV_FILE_EXT)));
      hFile = CreateFile(path, GENERIC_READ,
                         FILE_SHARE_READ | FILE_SHARE_WRITE,
                         NULL /* lpSecurityAttributes */, OPEN_EXISTING,
                         FILE_ATTRIBUTE_NORMAL, NULL /* hTemplateFile */);
      if (hFile == INVALID_HANDLE_VALUE)
        return; /* the file is absent or the operation is failed */
      len = (unsigned)GetFileSize(hFile, NULL);
      if (len <= 1 || len >= GC_ENVFILE_MAXLEN) {
        CloseHandle(hFile);
        return; /* invalid file length - ignoring the file content */
      }
      /* At this execution point, GC_setpagesize() and GC_init_win32()  */
      /* must already be called (for GET_MEM() to work correctly).      */
      content = (char *)GET_MEM(ROUNDUP_PAGESIZE_IF_MMAP((size_t)len + 1));
      if (content == NULL) {
        CloseHandle(hFile);
        return; /* allocation failure */
      }
      ofs = 0;
      nBytesRead = (DWORD)-1L;
          /* Last ReadFile() call should clear nBytesRead on success. */
      while (ReadFile(hFile, content + ofs, len - ofs + 1, &nBytesRead,
                      NULL /* lpOverlapped */) && nBytesRead != 0) {
        if ((ofs += nBytesRead) > len)
          break;
      }
      CloseHandle(hFile);
      if (ofs != len || nBytesRead != 0)
        return; /* read operation is failed - ignoring the file content */
      content[ofs] = '\0';
      while (ofs-- > 0) {
       if (content[ofs] == '\r' || content[ofs] == '\n')
         content[ofs] = '\0';
      }
      GC_ASSERT(NULL == GC_envfile_content);
      GC_envfile_length = len + 1;
      GC_envfile_content = content;
#   endif
  }

  /* This routine scans GC_envfile_content for the specified            */
  /* environment variable (and returns its value if found).             */
  GC_INNER char * GC_envfile_getenv(const char *name)
  {
    char *p;
    char *end_of_content;
    unsigned namelen;
#   ifndef NO_GETENV
      p = getenv(name); /* try the standard getenv() first */
      if (p != NULL)
        return *p != '\0' ? p : NULL;
#   endif
    p = GC_envfile_content;
    if (p == NULL)
      return NULL; /* "env" file is absent (or empty) */
    namelen = strlen(name);
    if (namelen == 0) /* a sanity check */
      return NULL;
    for (end_of_content = p + GC_envfile_length;
         p != end_of_content; p += strlen(p) + 1) {
      if (strncmp(p, name, namelen) == 0 && *(p += namelen) == '=') {
        p++; /* the match is found; skip '=' */
        return *p != '\0' ? p : NULL;
      }
      /* If not matching then skip to the next line. */
    }
    return NULL; /* no match found */
  }
#endif /* GC_READ_ENV_FILE */

GC_INNER GC_bool GC_is_initialized = FALSE;

GC_API int GC_CALL GC_is_init_called(void)
{
  return GC_is_initialized;
}

#if (defined(MSWIN32) || defined(MSWINCE) || defined(MSWIN_XBOX1)) \
    && defined(THREADS)
  GC_INNER CRITICAL_SECTION GC_write_cs;
#endif

#ifndef DONT_USE_ATEXIT
# if !defined(PCR) && !defined(SMALL_CONFIG)
    /* A dedicated variable to avoid a garbage collection on abort.     */
    /* GC_find_leak cannot be used for this purpose as otherwise        */
    /* TSan finds a data race (between GC_default_on_abort and, e.g.,   */
    /* GC_finish_collection).                                           */
    static GC_bool skip_gc_atexit = FALSE;
# else
#   define skip_gc_atexit FALSE
# endif

  STATIC void GC_exit_check(void)
  {
    if (GC_find_leak && !skip_gc_atexit) {
      GC_gcollect();
    }
  }
#endif

#if defined(UNIX_LIKE) && !defined(NO_DEBUGGING)
  static void looping_handler(int sig)
  {
    GC_err_printf("Caught signal %d: looping in handler\n", sig);
    for (;;) {
       /* empty */
    }
  }

  static GC_bool installed_looping_handler = FALSE;

  static void maybe_install_looping_handler(void)
  {
    /* Install looping handler before the write fault handler, so we    */
    /* handle write faults correctly.                                   */
    if (!installed_looping_handler && 0 != GETENV("GC_LOOP_ON_ABORT")) {
      GC_set_and_save_fault_handler(looping_handler);
      installed_looping_handler = TRUE;
    }
  }

#else /* !UNIX_LIKE */
# define maybe_install_looping_handler()
#endif

#define GC_DEFAULT_STDOUT_FD 1
#define GC_DEFAULT_STDERR_FD 2

#if !defined(OS2) && !defined(MACOS) && !defined(GC_ANDROID_LOG) \
    && !defined(NN_PLATFORM_CTR) && !defined(NINTENDO_SWITCH) \
    && !defined(MSWIN32) && !defined(MSWINCE)
  STATIC int GC_stdout = GC_DEFAULT_STDOUT_FD;
  STATIC int GC_stderr = GC_DEFAULT_STDERR_FD;
  STATIC int GC_log = GC_DEFAULT_STDERR_FD;

  GC_API void GC_CALL GC_set_log_fd(int fd)
  {
    GC_log = fd;
  }
#endif

#if defined(MSWIN32) && !defined(MSWINRT_FLAVOR) && (!defined(SMALL_CONFIG) \
                         || (!defined(_WIN64) && defined(GC_WIN32_THREADS) \
                             && defined(CHECK_NOT_WOW64))) && !defined(_XBOX_ONE)
  STATIC void GC_win32_MessageBoxA(const char *msg, const char *caption,
                                   unsigned flags)
  {
#   ifndef DONT_USE_USER32_DLL
      /* Use static binding to "user32.dll".    */
      (void)MessageBoxA(NULL, msg, caption, flags);
#   else
      /* This simplifies linking - resolve "MessageBoxA" at run-time. */
      HINSTANCE hU32 = LoadLibrary(TEXT("user32.dll"));
      if (hU32) {
        FARPROC pfn = GetProcAddress(hU32, "MessageBoxA");
        if (pfn)
          (void)(*(int (WINAPI *)(HWND, LPCSTR, LPCSTR, UINT))pfn)(
                              NULL /* hWnd */, msg, caption, flags);
        (void)FreeLibrary(hU32);
      }
#   endif
  }
#endif /* MSWIN32 */

#if defined(THREADS) && defined(UNIX_LIKE) && !defined(NO_GETCONTEXT)
  static void callee_saves_pushed_dummy_fn(ptr_t data GC_ATTR_UNUSED,
                                           void * context GC_ATTR_UNUSED) {}
#endif

STATIC word GC_parse_mem_size_arg(const char *str)
{
  word result = 0; /* bad value */

  if (*str != '\0') {
    char *endptr;
    char ch;

    result = (word)STRTOULL(str, &endptr, 10);
    ch = *endptr;
    if (ch != '\0') {
      if (*(endptr + 1) != '\0')
        return 0;
      /* Allow k, M or G suffix. */
      switch (ch) {
      case 'K':
      case 'k':
        result <<= 10;
        break;
      case 'M':
      case 'm':
        result <<= 20;
        break;
      case 'G':
      case 'g':
        result <<= 30;
        break;
      default:
        result = 0;
      }
    }
  }
  return result;
}

#define GC_LOG_STD_NAME "gc.log"

GC_API void GC_CALL GC_init(void)
{
    /* LOCK(); -- no longer does anything this early. */
    word initial_heap_sz;
    IF_CANCEL(int cancel_state;)
#   if defined(GC_ASSERTIONS) && defined(GC_ALWAYS_MULTITHREADED)
      DCL_LOCK_STATE;
#   endif

    if (EXPECT(GC_is_initialized, TRUE)) return;
#   ifdef REDIRECT_MALLOC
      {
        static GC_bool init_started = FALSE;
        if (init_started)
          ABORT("Redirected malloc() called during GC init");
        init_started = TRUE;
      }
#   endif

#   if defined(GC_INITIAL_HEAP_SIZE) && !defined(CPPCHECK)
      initial_heap_sz = GC_INITIAL_HEAP_SIZE;
#   else
      initial_heap_sz = MINHINCR * HBLKSIZE;
#   endif

#   if defined(MSWIN32) && !defined(_WIN64) && defined(GC_WIN32_THREADS) \
       && defined(CHECK_NOT_WOW64) && !defined(_XBOX_ONE)
      {
        /* Windows: running 32-bit GC on 64-bit system is broken!       */
        /* WoW64 bug affects SuspendThread, no workaround exists.       */
        HMODULE hK32 = GetModuleHandle(TEXT("kernel32.dll"));
        if (hK32) {
          FARPROC pfn = GetProcAddress(hK32, "IsWow64Process");
          BOOL bIsWow64 = FALSE;
          if (pfn
              && (*(BOOL (WINAPI*)(HANDLE, BOOL*))pfn)(GetCurrentProcess(),
                                                       &bIsWow64)
              && bIsWow64) {
            GC_win32_MessageBoxA("This program uses BDWGC garbage collector"
                " compiled for 32-bit but running on 64-bit Windows.\n"
                "This is known to be broken due to a design flaw"
                " in Windows itself! Expect erratic behavior...",
                "32-bit program running on 64-bit system",
                MB_ICONWARNING | MB_OK);
          }
        }
      }
#   endif

    DISABLE_CANCEL(cancel_state);
    /* Note that although we are nominally called with the */
    /* allocation lock held, the allocation lock is now    */
    /* only really acquired once a second thread is forked.*/
    /* And the initialization code needs to run before     */
    /* then.  Thus we really don't hold any locks, and can */
    /* in fact safely initialize them here.                */
#   ifdef THREADS
#     ifndef GC_ALWAYS_MULTITHREADED
        GC_ASSERT(!GC_need_to_lock);
#     endif
#     ifdef SN_TARGET_PS3
        {
          pthread_mutexattr_t mattr;

          if (0 != pthread_mutexattr_init(&mattr)) {
            ABORT("pthread_mutexattr_init failed");
          }
          if (0 != pthread_mutex_init(&GC_allocate_ml, &mattr)) {
            ABORT("pthread_mutex_init failed");
          }
          (void)pthread_mutexattr_destroy(&mattr);
        }
#     endif
#   endif /* THREADS */
#   if defined(GC_WIN32_THREADS) && !defined(GC_PTHREADS)
#     ifndef SPIN_COUNT
#       define SPIN_COUNT 4000
#     endif
#     ifdef MSWINRT_FLAVOR
        InitializeCriticalSectionAndSpinCount(&GC_allocate_ml, SPIN_COUNT);
#     else
        {
#         ifndef MSWINCE
#         ifndef MSWINRT
            BOOL (WINAPI *pfn)(LPCRITICAL_SECTION, DWORD) = 0;
            HMODULE hK32 = GetModuleHandle(TEXT("kernel32.dll"));
            if (hK32)
              pfn = (BOOL (WINAPI *)(LPCRITICAL_SECTION, DWORD))
                      GetProcAddress(hK32,
                                     "InitializeCriticalSectionAndSpinCount");
            if (pfn) {
              pfn(&GC_allocate_ml, SPIN_COUNT);
            } else
#          else
		        InitializeCriticalSectionAndSpinCount(&GC_allocate_ml, 4000);
#          endif            
#         endif /* !MSWINCE */
#         ifndef MSWINRT
          /* else */ InitializeCriticalSection(&GC_allocate_ml);
#         endif          
        }
#     endif
#   endif /* GC_WIN32_THREADS */
#   if (defined(MSWIN32) || defined(MSWINCE)) && defined(THREADS)
      InitializeCriticalSection(&GC_write_cs);
#   endif
    GC_setpagesize();
#   ifdef MSWIN32
      GC_init_win32();
#   endif
#   ifdef GC_READ_ENV_FILE
      GC_envfile_init();
#   endif
#   if !defined(NO_CLOCK) || !defined(SMALL_CONFIG)
#     ifdef GC_PRINT_VERBOSE_STATS
        /* This is useful for debugging and profiling on platforms with */
        /* missing getenv() (like WinCE).                               */
        GC_print_stats = VERBOSE;
#     else
        if (0 != GETENV("GC_PRINT_VERBOSE_STATS")) {
          GC_print_stats = VERBOSE;
        } else if (0 != GETENV("GC_PRINT_STATS")) {
          GC_print_stats = 1;
        }
#     endif
#   endif
#   if ((defined(UNIX_LIKE) && !defined(GC_ANDROID_LOG)) \
        || defined(CYGWIN32) || defined(SYMBIAN)) && !defined(SMALL_CONFIG)
        {
          char * file_name = TRUSTED_STRING(GETENV("GC_LOG_FILE"));
#         ifdef GC_LOG_TO_FILE_ALWAYS
            if (NULL == file_name)
              file_name = GC_LOG_STD_NAME;
#         else
            if (0 != file_name)
#         endif
          {
            int log_d = open(file_name, O_CREAT|O_WRONLY|O_APPEND, 0666);
            if (log_d < 0) {
              GC_err_printf("Failed to open %s as log file\n", file_name);
            } else {
              char *str;
              GC_log = log_d;
              str = GETENV("GC_ONLY_LOG_TO_FILE");
#             ifdef GC_ONLY_LOG_TO_FILE
                /* The similar environment variable set to "0"  */
                /* overrides the effect of the macro defined.   */
                if (str != NULL && *str == '0' && *(str + 1) == '\0')
#             else
                /* Otherwise setting the environment variable   */
                /* to anything other than "0" will prevent from */
                /* redirecting stdout/err to the log file.      */
                if (str == NULL || (*str == '0' && *(str + 1) == '\0'))
#             endif
              {
                GC_stdout = log_d;
                GC_stderr = log_d;
              }
            }
          }
        }
#   endif
#   if !defined(NO_DEBUGGING) && !defined(GC_DUMP_REGULARLY)
      if (0 != GETENV("GC_DUMP_REGULARLY")) {
        GC_dump_regularly = TRUE;
      }
#   endif
#   ifdef KEEP_BACK_PTRS
      {
        char * backtraces_string = GETENV("GC_BACKTRACES");
        if (0 != backtraces_string) {
          GC_backtraces = atol(backtraces_string);
          if (backtraces_string[0] == '\0') GC_backtraces = 1;
        }
      }
#   endif
    if (0 != GETENV("GC_FIND_LEAK")) {
      GC_find_leak = 1;
    }
#   ifndef SHORT_DBG_HDRS
      if (0 != GETENV("GC_FINDLEAK_DELAY_FREE")) {
        GC_findleak_delay_free = TRUE;
      }
#   endif
    if (0 != GETENV("GC_ALL_INTERIOR_POINTERS")) {
      GC_all_interior_pointers = 1;
    }
    if (0 != GETENV("GC_DONT_GC")) {
      GC_dont_gc = 1;
    }
    if (0 != GETENV("GC_PRINT_BACK_HEIGHT")) {
      GC_print_back_height = TRUE;
    }
    if (0 != GETENV("GC_NO_BLACKLIST_WARNING")) {
      GC_large_alloc_warn_interval = LONG_MAX;
    }
    {
      char * addr_string = GETENV("GC_TRACE");
      if (0 != addr_string) {
#       ifndef ENABLE_TRACE
          WARN("Tracing not enabled: Ignoring GC_TRACE value\n", 0);
#       else
          word addr = (word)STRTOULL(addr_string, NULL, 16);
          if (addr < 0x1000)
              WARN("Unlikely trace address: %p\n", (void *)addr);
          GC_trace_addr = (ptr_t)addr;
#       endif
      }
    }
#   ifdef GC_COLLECT_AT_MALLOC
      {
        char * string = GETENV("GC_COLLECT_AT_MALLOC");
        if (0 != string) {
          size_t min_lb = (size_t)STRTOULL(string, NULL, 10);
          if (min_lb > 0)
            GC_dbg_collect_at_malloc_min_lb = min_lb;
        }
      }
#   endif
#   ifndef GC_DISABLE_INCREMENTAL
      {
        char * time_limit_string = GETENV("GC_PAUSE_TIME_TARGET");
        if (0 != time_limit_string) {
          long time_limit = atol(time_limit_string);
          if (time_limit < 5) {
            WARN("GC_PAUSE_TIME_TARGET environment variable value too small "
                 "or bad syntax: Ignoring\n", 0);
          } else {
            GC_time_limit = time_limit * 1000000;
          }
        }
      }
#   endif
#   ifndef SMALL_CONFIG
      {
        char * full_freq_string = GETENV("GC_FULL_FREQUENCY");
        if (full_freq_string != NULL) {
          int full_freq = atoi(full_freq_string);
          if (full_freq > 0)
            GC_full_freq = full_freq;
        }
      }
#   endif
    {
      char * interval_string = GETENV("GC_LARGE_ALLOC_WARN_INTERVAL");
      if (0 != interval_string) {
        long interval = atol(interval_string);
        if (interval <= 0) {
          WARN("GC_LARGE_ALLOC_WARN_INTERVAL environment variable has "
               "bad value: Ignoring\n", 0);
        } else {
          GC_large_alloc_warn_interval = interval;
        }
      }
    }
    {
        char * space_divisor_string = GETENV("GC_FREE_SPACE_DIVISOR");
        if (space_divisor_string != NULL) {
          int space_divisor = atoi(space_divisor_string);
          if (space_divisor > 0)
            GC_free_space_divisor = (word)space_divisor;
        }
    }
#   ifdef USE_MUNMAP
      {
        char * string = GETENV("GC_UNMAP_THRESHOLD");
        if (string != NULL) {
          if (*string == '0' && *(string + 1) == '\0') {
            /* "0" is used to disable unmapping. */
            GC_unmap_threshold = 0;
          } else {
            int unmap_threshold = atoi(string);
            if (unmap_threshold > 0)
              GC_unmap_threshold = unmap_threshold;
          }
        }
      }
      {
        char * string = GETENV("GC_FORCE_UNMAP_ON_GCOLLECT");
        if (string != NULL) {
          if (*string == '0' && *(string + 1) == '\0') {
            /* "0" is used to turn off the mode. */
            GC_force_unmap_on_gcollect = FALSE;
          } else {
            GC_force_unmap_on_gcollect = TRUE;
          }
        }
      }
      {
        char * string = GETENV("GC_USE_ENTIRE_HEAP");
        if (string != NULL) {
          if (*string == '0' && *(string + 1) == '\0') {
            /* "0" is used to turn off the mode. */
            GC_use_entire_heap = FALSE;
          } else {
            GC_use_entire_heap = TRUE;
          }
        }
      }
#   endif
#   if !defined(NO_DEBUGGING) && !defined(NO_CLOCK)
      GET_TIME(GC_init_time);
#   endif
    maybe_install_looping_handler();
#   if ALIGNMENT > GC_DS_TAGS
      /* Adjust normal object descriptor for extra allocation.  */
      if (EXTRA_BYTES != 0)
        GC_obj_kinds[NORMAL].ok_descriptor = (word)(-ALIGNMENT) | GC_DS_LENGTH;
#   endif
    GC_exclude_static_roots_inner(beginGC_arrays, endGC_arrays);
    GC_exclude_static_roots_inner(beginGC_obj_kinds, endGC_obj_kinds);
#   ifdef SEPARATE_GLOBALS
      GC_exclude_static_roots_inner(beginGC_objfreelist, endGC_objfreelist);
      GC_exclude_static_roots_inner(beginGC_aobjfreelist, endGC_aobjfreelist);
#   endif
#   if defined(USE_PROC_FOR_LIBRARIES) && defined(GC_LINUX_THREADS)
        WARN("USE_PROC_FOR_LIBRARIES + GC_LINUX_THREADS performs poorly.\n", 0);
        /* If thread stacks are cached, they tend to be scanned in      */
        /* entirety as part of the root set.  This wil grow them to     */
        /* maximum size, and is generally not desirable.                */
#   endif
#   if defined(SEARCH_FOR_DATA_START)
        GC_init_linux_data_start();
#   endif
#   if defined(NETBSD) && defined(__ELF__)
        GC_init_netbsd_elf();
#   endif
#   if !defined(THREADS) || defined(GC_PTHREADS) \
        || defined(NN_PLATFORM_CTR) || defined(NINTENDO_SWITCH) \
        || defined(GC_WIN32_THREADS) || defined(GC_SOLARIS_THREADS)
      if (GC_stackbottom == 0) {
        GC_stackbottom = GC_get_main_stack_base();
#       if (defined(LINUX) || defined(HPUX)) && defined(IA64)
          GC_register_stackbottom = GC_get_register_stack_base();
#       endif
      } else {
#       if (defined(LINUX) || defined(HPUX)) && defined(IA64)
          if (GC_register_stackbottom == 0) {
            WARN("GC_register_stackbottom should be set with GC_stackbottom\n", 0);
            /* The following may fail, since we may rely on             */
            /* alignment properties that may not hold with a user set   */
            /* GC_stackbottom.                                          */
            GC_register_stackbottom = GC_get_register_stack_base();
          }
#       endif
      }
#   endif
#   if !defined(CPPCHECK)
      GC_STATIC_ASSERT(sizeof(ptr_t) == sizeof(word));
      GC_STATIC_ASSERT(sizeof(signed_word) == sizeof(word));
#     if !defined(_AUX_SOURCE) || defined(__GNUC__)
        GC_STATIC_ASSERT((word)(-1) > (word)0);
        /* word should be unsigned */
#     endif
      /* We no longer check for ((void*)(-1) > NULL) since all pointers */
      /* are explicitly cast to word in every less/greater comparison.  */
      GC_STATIC_ASSERT((signed_word)(-1) < (signed_word)0);
#   endif
    GC_STATIC_ASSERT(sizeof (struct hblk) == HBLKSIZE);
#   ifndef THREADS
      GC_ASSERT(!((word)GC_stackbottom HOTTER_THAN (word)GC_approx_sp()));
#   endif
#   ifndef GC_DISABLE_INCREMENTAL
      if (GC_incremental || 0 != GETENV("GC_ENABLE_INCREMENTAL")) {
        /* For GWW_VDB on Win32, this needs to happen before any        */
        /* heap memory is allocated.                                    */
        GC_incremental = GC_dirty_init();
        GC_ASSERT(GC_bytes_allocd == 0);
      }
#   endif

    /* Add initial guess of root sets.  Do this first, since sbrk(0)    */
    /* might be used.                                                   */
      if (GC_REGISTER_MAIN_STATIC_DATA()) GC_register_data_segments();
    GC_init_headers();
    GC_bl_init();
    GC_mark_init();
    {
        char * sz_str = GETENV("GC_INITIAL_HEAP_SIZE");
        if (sz_str != NULL) {
          initial_heap_sz = GC_parse_mem_size_arg(sz_str);
          if (initial_heap_sz <= MINHINCR * HBLKSIZE) {
            WARN("Bad initial heap size %s - ignoring it.\n", sz_str);
          }
        }
    }
    {
        char * sz_str = GETENV("GC_MAXIMUM_HEAP_SIZE");
        if (sz_str != NULL) {
          word max_heap_sz = GC_parse_mem_size_arg(sz_str);
          if (max_heap_sz < initial_heap_sz) {
            WARN("Bad maximum heap size %s - ignoring it.\n", sz_str);
          }
          if (0 == GC_max_retries) GC_max_retries = 2;
          GC_set_max_heap_size(max_heap_sz);
        }
    }
    if (!GC_expand_hp_inner(divHBLKSZ(initial_heap_sz))) {
        GC_err_printf("Can't start up: not enough memory\n");
        EXIT();
    } else {
        GC_requested_heapsize += initial_heap_sz;
    }
    if (GC_all_interior_pointers)
      GC_initialize_offsets();
    GC_register_displacement_inner(0L);
#   if defined(GC_LINUX_THREADS) && defined(REDIRECT_MALLOC)
      if (!GC_all_interior_pointers) {
        /* TLS ABI uses pointer-sized offsets for dtv. */
        GC_register_displacement_inner(sizeof(void *));
      }
#   endif
    GC_init_size_map();
#   ifdef PCR
      if (PCR_IL_Lock(PCR_Bool_false, PCR_allSigsBlocked, PCR_waitForever)
          != PCR_ERes_okay) {
          ABORT("Can't lock load state");
      } else if (PCR_IL_Unlock() != PCR_ERes_okay) {
          ABORT("Can't unlock load state");
      }
      PCR_IL_Unlock();
      GC_pcr_install();
#   endif
    GC_is_initialized = TRUE;
#   if defined(GC_ASSERTIONS) && defined(GC_ALWAYS_MULTITHREADED)
        LOCK(); /* just to set GC_lock_holder */
#   endif
#   if defined(GC_PTHREADS) || defined(GC_WIN32_THREADS)
        GC_thr_init();
#       ifdef PARALLEL_MARK
          /* Actually start helper threads.     */
#         if defined(GC_ASSERTIONS) && defined(GC_ALWAYS_MULTITHREADED)
            UNLOCK();
#         endif
          GC_start_mark_threads_inner();
#         if defined(GC_ASSERTIONS) && defined(GC_ALWAYS_MULTITHREADED)
            LOCK();
#         endif
#       endif
#   endif
    COND_DUMP;
    /* Get black list set up and/or incremental GC started */
    if (!GC_dont_precollect || GC_incremental) {
        GC_gcollect_inner();
    }
#   if defined(GC_ASSERTIONS) && defined(GC_ALWAYS_MULTITHREADED)
        UNLOCK();
#   endif
#   if defined(THREADS) && defined(UNIX_LIKE) && !defined(NO_GETCONTEXT)
      /* Ensure getcontext_works is set to avoid potential data race.   */
      if (GC_dont_gc || GC_dont_precollect)
        GC_with_callee_saves_pushed(callee_saves_pushed_dummy_fn, NULL);
#   endif
#   ifndef DONT_USE_ATEXIT
      if (GC_find_leak) {
        /* This is to give us at least one chance to detect leaks.        */
        /* This may report some very benign leaks, but ...                */
        atexit(GC_exit_check);
      }
#   endif

    /* The rest of this again assumes we don't really hold      */
    /* the allocation lock.                                     */
#   if defined(PARALLEL_MARK) || defined(THREAD_LOCAL_ALLOC) \
       || (defined(GC_ALWAYS_MULTITHREADED) && defined(GC_WIN32_THREADS) \
           && !defined(GC_NO_THREADS_DISCOVERY))
        /* Make sure marker threads are started and thread local */
        /* allocation is initialized, in case we didn't get      */
        /* called from GC_init_parallel.                         */
        GC_init_parallel();
#   endif /* PARALLEL_MARK || THREAD_LOCAL_ALLOC */

#   if defined(DYNAMIC_LOADING) && defined(DARWIN)
        /* This must be called WITHOUT the allocation lock held */
        /* and before any threads are created.                  */
        GC_init_dyld();
#   endif
    RESTORE_CANCEL(cancel_state);
}

GC_API void GC_CALL GC_enable_incremental(void)
{
# if !defined(GC_DISABLE_INCREMENTAL) && !defined(KEEP_BACK_PTRS)
    DCL_LOCK_STATE;
    /* If we are keeping back pointers, the GC itself dirties all */
    /* pages on which objects have been marked, making            */
    /* incremental GC pointless.                                  */
    if (!GC_find_leak && 0 == GETENV("GC_DISABLE_INCREMENTAL")) {
      LOCK();
      if (!GC_incremental) {
        GC_setpagesize();
        /* if (GC_no_win32_dlls) goto out; Should be win32S test? */
        maybe_install_looping_handler(); /* Before write fault handler! */
        if (!GC_is_initialized) {
          UNLOCK();
          GC_incremental = TRUE; /* indicate intention to turn it on */
          GC_init();
          LOCK();
        } else {
          GC_incremental = GC_dirty_init();
        }
        if (GC_incremental && !GC_dont_gc) {
                                /* Can't easily do it if GC_dont_gc.    */
          IF_CANCEL(int cancel_state;)

          DISABLE_CANCEL(cancel_state);
          if (GC_bytes_allocd > 0) {
            /* There may be unmarked reachable objects. */
            GC_gcollect_inner();
          }
            /* else we're OK in assuming everything's   */
            /* clean since nothing can point to an      */
            /* unmarked object.                         */
          GC_read_dirty(FALSE);
          RESTORE_CANCEL(cancel_state);
        }
      }
      UNLOCK();
      return;
    }
# endif
  GC_init();
}

#if defined(THREADS)
  GC_API void GC_CALL GC_start_mark_threads(void)
  {
#   if defined(PARALLEL_MARK) && defined(CAN_HANDLE_FORK) \
       && !defined(THREAD_SANITIZER)
      /* TSan does not support threads creation in the child process.   */
      IF_CANCEL(int cancel_state;)

      DISABLE_CANCEL(cancel_state);
      GC_start_mark_threads_inner();
      RESTORE_CANCEL(cancel_state);
#   else
      /* No action since parallel markers are disabled (or no POSIX fork). */
      GC_ASSERT(I_DONT_HOLD_LOCK());
#   endif
  }
#endif

  extern void GC_reset_default_push_other_roots(void);

  GC_API void GC_CALL GC_deinit(void)
  {
    if (GC_is_initialized) {
      /* Prevent duplicate resource close.  */
      GC_is_initialized = FALSE;
#     if defined(THREADS) && (defined(MSWIN32) || defined(MSWINCE))
        DeleteCriticalSection(&GC_write_cs);
        DeleteCriticalSection(&GC_allocate_ml);
#     endif
        GC_clear_exclusion_table();
        memset(&GC_arrays, 0, sizeof(GC_arrays));
        GC_clear_freelist();
        GC_clear_bottom_indices();
        GC_clear_finalizable_object_table();
        GC_reset_mark_statics();
        GC_reset_default_push_other_roots();
    }
  }

#if defined(MSWIN32) || defined(MSWINCE)

# if defined(_MSC_VER) && defined(_DEBUG) && !defined(MSWINCE)
#   include <crtdbg.h>
# endif

  STATIC HANDLE GC_log = 0;

# ifdef THREADS
#   if defined(PARALLEL_MARK) && !defined(GC_ALWAYS_MULTITHREADED)
#     define IF_NEED_TO_LOCK(x) if (GC_parallel || GC_need_to_lock) x
#   else
#     define IF_NEED_TO_LOCK(x) if (GC_need_to_lock) x
#   endif
# else
#   define IF_NEED_TO_LOCK(x)
# endif /* !THREADS */

# ifdef MSWINRT_FLAVOR
#   include <windows.storage.h>

    /* This API is defined in roapi.h, but we cannot include it here    */
    /* since it does not compile in C.                                  */
    DECLSPEC_IMPORT HRESULT WINAPI RoGetActivationFactory(
                                        HSTRING activatableClassId,
                                        REFIID iid, void** factory);

    static GC_bool getWinRTLogPath(wchar_t* buf, size_t bufLen)
    {
      static const GUID kIID_IApplicationDataStatics = {
        0x5612147B, 0xE843, 0x45E3,
        0x94, 0xD8, 0x06, 0x16, 0x9E, 0x3C, 0x8E, 0x17
      };
      static const GUID kIID_IStorageItem = {
        0x4207A996, 0xCA2F, 0x42F7,
        0xBD, 0xE8, 0x8B, 0x10, 0x45, 0x7A, 0x7F, 0x30
      };
      GC_bool result = FALSE;
      HSTRING_HEADER appDataClassNameHeader;
      HSTRING appDataClassName;
      __x_ABI_CWindows_CStorage_CIApplicationDataStatics* appDataStatics = 0;

      GC_ASSERT(bufLen > 0);
      if (SUCCEEDED(WindowsCreateStringReference(
                      RuntimeClass_Windows_Storage_ApplicationData,
                      (sizeof(RuntimeClass_Windows_Storage_ApplicationData)-1)
                        / sizeof(wchar_t),
                      &appDataClassNameHeader, &appDataClassName))
          && SUCCEEDED(RoGetActivationFactory(appDataClassName,
                                              &kIID_IApplicationDataStatics,
                                              &appDataStatics))) {
        __x_ABI_CWindows_CStorage_CIApplicationData* appData = NULL;
        __x_ABI_CWindows_CStorage_CIStorageFolder* tempFolder = NULL;
        __x_ABI_CWindows_CStorage_CIStorageItem* tempFolderItem = NULL;
        HSTRING tempPath = NULL;

        if (SUCCEEDED(appDataStatics->lpVtbl->get_Current(appDataStatics,
                                                          &appData))
            && SUCCEEDED(appData->lpVtbl->get_TemporaryFolder(appData,
                                                              &tempFolder))
            && SUCCEEDED(tempFolder->lpVtbl->QueryInterface(tempFolder,
                                                        &kIID_IStorageItem,
                                                        &tempFolderItem))
            && SUCCEEDED(tempFolderItem->lpVtbl->get_Path(tempFolderItem,
                                                          &tempPath))) {
          UINT32 tempPathLen;
          const wchar_t* tempPathBuf =
                          WindowsGetStringRawBuffer(tempPath, &tempPathLen);

          buf[0] = '\0';
          if (wcsncat_s(buf, bufLen, tempPathBuf, tempPathLen) == 0
              && wcscat_s(buf, bufLen, L"\\") == 0
              && wcscat_s(buf, bufLen, TEXT(GC_LOG_STD_NAME)) == 0)
            result = TRUE;
          WindowsDeleteString(tempPath);
        }

        if (tempFolderItem != NULL)
          tempFolderItem->lpVtbl->Release(tempFolderItem);
        if (tempFolder != NULL)
          tempFolder->lpVtbl->Release(tempFolder);
        if (appData != NULL)
          appData->lpVtbl->Release(appData);
        appDataStatics->lpVtbl->Release(appDataStatics);
      }
      return result;
    }
# endif /* MSWINRT_FLAVOR */

  STATIC HANDLE GC_CreateLogFile(void)
  {
    HANDLE hFile;
# ifdef MSWINRT_FLAVOR
      TCHAR pathBuf[_MAX_PATH + 0x10]; /* buffer for path + ext */

      hFile = INVALID_HANDLE_VALUE;
      if (getWinRTLogPath(pathBuf, _MAX_PATH + 1)) {
        CREATEFILE2_EXTENDED_PARAMETERS extParams;

        BZERO(&extParams, sizeof(extParams));
        extParams.dwSize = sizeof(extParams);
        extParams.dwFileAttributes = FILE_ATTRIBUTE_NORMAL;
        extParams.dwFileFlags = GC_print_stats == VERBOSE ? 0
                                    : FILE_FLAG_WRITE_THROUGH;
        hFile = CreateFile2(pathBuf, GENERIC_WRITE, FILE_SHARE_READ,
                            CREATE_ALWAYS, &extParams);
      }

# else
    TCHAR *logPath;
    BOOL appendToFile = FALSE;
#   if !defined(NO_GETENV_WIN32) || !defined(OLD_WIN32_LOG_FILE)
      TCHAR pathBuf[_MAX_PATH + 0x10]; /* buffer for path + ext */

      logPath = pathBuf;
#   endif

#   ifndef MSWINRT
    /* Use GetEnvironmentVariable instead of GETENV() for unicode support. */
#   ifndef NO_GETENV_WIN32
      if (GetEnvironmentVariable(TEXT("GC_LOG_FILE"), pathBuf,
                                 _MAX_PATH + 1) - 1U < (DWORD)_MAX_PATH) {
        appendToFile = TRUE;
      } else
#   endif
    /* else */ {
      /* Env var not found or its value too long.       */
#     ifdef OLD_WIN32_LOG_FILE
        logPath = TEXT(GC_LOG_STD_NAME);
#     else
        int len = (int)GetModuleFileName(NULL /* hModule */, pathBuf,
                                         _MAX_PATH + 1);
        /* If GetModuleFileName() has failed then len is 0. */
        if (len > 4 && pathBuf[len - 4] == (TCHAR)'.') {
          len -= 4; /* strip executable file extension */
        }
        BCOPY(TEXT(".") TEXT(GC_LOG_STD_NAME), &pathBuf[len],
              sizeof(TEXT(".") TEXT(GC_LOG_STD_NAME)));
#     endif
    }
#   endif

#   ifndef MSWINRT
    hFile = CreateFile(logPath, GENERIC_WRITE, FILE_SHARE_READ,
                       NULL /* lpSecurityAttributes */,
                       appendToFile ? OPEN_ALWAYS : CREATE_ALWAYS,
                       GC_print_stats == VERBOSE ? FILE_ATTRIBUTE_NORMAL :
                            /* immediately flush writes unless very verbose */
                            FILE_ATTRIBUTE_NORMAL | FILE_FLAG_WRITE_THROUGH,
                       NULL /* hTemplateFile */);
#   else
      {
        if (GetWinRTLogPath(pathBuf, MAX_PATH + 1))
        {
          CREATEFILE2_EXTENDED_PARAMETERS extendedParameters;
          ZeroMemory(&extendedParameters, sizeof(extendedParameters));

          extendedParameters.dwSize = sizeof(CREATEFILE2_EXTENDED_PARAMETERS);
          extendedParameters.dwFileFlags = GC_print_stats == VERBOSE
            ? 0
            : /* immediately flush writes unless very verbose */ FILE_FLAG_WRITE_THROUGH;
          extendedParameters.dwFileAttributes = FILE_ATTRIBUTE_NORMAL;

          hFile = CreateFile2(logPath, GENERIC_WRITE, FILE_SHARE_READ,
            appendToFile ? OPEN_ALWAYS : CREATE_ALWAYS,
            &extendedParameters);
        }
        else
        {
          hFile = INVALID_HANDLE_VALUE;
        }
      }
#   endif

#   ifndef NO_GETENV_WIN32
      if (appendToFile && hFile != INVALID_HANDLE_VALUE) {
        LONG posHigh = 0;
        (void)SetFilePointer(hFile, 0, &posHigh, FILE_END);
                                  /* Seek to file end (ignoring any error) */
      }
#   endif
# endif
    return hFile;
  }

  STATIC int GC_write(const char *buf, size_t len)
  {
      BOOL res;
      DWORD written;
#     if defined(THREADS) && defined(GC_ASSERTIONS)
        static GC_bool inside_write = FALSE;
                        /* to prevent infinite recursion at abort.      */
        if (inside_write)
          return -1;
#     endif

      if (len == 0)
          return 0;
      IF_NEED_TO_LOCK(EnterCriticalSection(&GC_write_cs));
#     if defined(THREADS) && defined(GC_ASSERTIONS)
        if (GC_write_disabled) {
          inside_write = TRUE;
          ABORT("Assertion failure: GC_write called with write_disabled");
        }
#     endif
      if (GC_log == 0) {
        GC_log = GC_CreateLogFile();
      }
      if (GC_log == INVALID_HANDLE_VALUE) {
        IF_NEED_TO_LOCK(LeaveCriticalSection(&GC_write_cs));
#       ifdef NO_DEBUGGING
          /* Ignore open log failure (e.g., it might be caused by       */
          /* read-only folder of the client application).               */
          return 0;
#       else
          return -1;
#       endif
      }
      res = WriteFile(GC_log, buf, (DWORD)len, &written, NULL);
#     if defined(_MSC_VER) && defined(_DEBUG) && !defined(NO_CRT)
#         ifdef MSWINCE
              /* There is no CrtDbgReport() in WinCE */
              {
                  WCHAR wbuf[1024];
                  /* Always use Unicode variant of OutputDebugString() */
                  wbuf[MultiByteToWideChar(CP_ACP, 0 /* dwFlags */,
                                buf, len, wbuf,
                                sizeof(wbuf) / sizeof(wbuf[0]) - 1)] = 0;
                  OutputDebugStringW(wbuf);
              }
#         else
              _CrtDbgReport(_CRT_WARN, NULL, 0, NULL, "%.*s", len, buf);
#         endif
#     endif
      IF_NEED_TO_LOCK(LeaveCriticalSection(&GC_write_cs));
      return res ? (int)written : -1;
  }

  /* FIXME: This is pretty ugly ... */
# define WRITE(f, buf, len) GC_write(buf, len)

#elif defined(OS2) || defined(MACOS)
  STATIC FILE * GC_stdout = NULL;
  STATIC FILE * GC_stderr = NULL;
  STATIC FILE * GC_log = NULL;

  /* Initialize GC_log (and the friends) passed to GC_write().  */
  STATIC void GC_set_files(void)
  {
    if (GC_stdout == NULL) {
      GC_stdout = stdout;
    }
    if (GC_stderr == NULL) {
      GC_stderr = stderr;
    }
    if (GC_log == NULL) {
      GC_log = stderr;
    }
  }

  GC_INLINE int GC_write(FILE *f, const char *buf, size_t len)
  {
    int res = fwrite(buf, 1, len, f);
    fflush(f);
    return res;
  }

# define WRITE(f, buf, len) (GC_set_files(), GC_write(f, buf, len))

#elif defined(GC_ANDROID_LOG)

# include <android/log.h>

# ifndef GC_ANDROID_LOG_TAG
#   define GC_ANDROID_LOG_TAG "BDWGC"
# endif

# define GC_stdout ANDROID_LOG_DEBUG
# define GC_stderr ANDROID_LOG_ERROR
# define GC_log GC_stdout

# define WRITE(level, buf, unused_len) \
                __android_log_write(level, GC_ANDROID_LOG_TAG, buf)

# elif defined(NN_PLATFORM_CTR)
    int n3ds_log_write(const char* text, int length);
#   define WRITE(level, buf, len) n3ds_log_write(buf, len)
# elif defined(NINTENDO_SWITCH)
    int switch_log_write(const char* text, int length);
#   define WRITE(level, buf, len) switch_log_write(buf, len)

#else
# if !defined(AMIGA) && !defined(MSWIN_XBOX1) && !defined(GC_NO_TYPES) \
     && !defined(SN_TARGET_PSP2) && !defined(__CC_ARM)
#   include <unistd.h>
# endif

  STATIC int GC_write(int fd, const char *buf, size_t len)
  {
#   if defined(ECOS) || defined(PLATFORM_WRITE) || defined(SN_TARGET_PSP2) \
       || defined(NOSYS)
#     ifdef ECOS
        /* FIXME: This seems to be defined nowhere at present.  */
        /* _Jv_diag_write(buf, len); */
#     else
        /* No writing.  */
#     endif
      return len;
#   else
      int bytes_written = 0;
      IF_CANCEL(int cancel_state;)

      DISABLE_CANCEL(cancel_state);
      while ((size_t)bytes_written < len) {
#        ifdef GC_SOLARIS_THREADS
             int result = syscall(SYS_write, fd, buf + bytes_written,
                                             len - bytes_written);
#        else
             int result = write(fd, buf + bytes_written, len - bytes_written);
#        endif

         if (-1 == result) {
             RESTORE_CANCEL(cancel_state);
             return(result);
         }
         bytes_written += result;
      }
      RESTORE_CANCEL(cancel_state);
      return(bytes_written);
#   endif
  }

# define WRITE(f, buf, len) GC_write(f, buf, len)
#endif /* !MSWIN32 && !OS2 && !MACOS && !GC_ANDROID_LOG */

#define BUFSZ 1024

#if !defined(NO_CRT)
#if defined(DJGPP) || defined(__STRICT_ANSI__)
  /* vsnprintf is missing in DJGPP (v2.0.3) */
# define GC_VSNPRINTF(buf, bufsz, format, args) vsprintf(buf, format, args)
#elif defined(_MSC_VER)
# ifdef MSWINCE
    /* _vsnprintf is deprecated in WinCE */
#   define GC_VSNPRINTF StringCchVPrintfA
# else
#   define GC_VSNPRINTF _vsnprintf
# endif
#else
# define GC_VSNPRINTF vsnprintf
#endif
#endif
/* A version of printf that is unlikely to call malloc, and is thus safer */
/* to call from the collector in case malloc has been bound to GC_malloc. */
/* Floating point arguments and formats should be avoided, since FP       */
/* conversion is more likely to allocate memory.                          */
/* Assumes that no more than BUFSZ-1 characters are written at once.      */
#if defined(GC_VSNPRINTF)
#define GC_PRINTF_FILLBUF(buf, format) \
        do { \
          va_list args; \
          va_start(args, format); \
          (buf)[sizeof(buf) - 1] = 0x15; /* guard */ \
          (void)GC_VSNPRINTF(buf, sizeof(buf) - 1, format, args); \
          va_end(args); \
          if ((buf)[sizeof(buf) - 1] != 0x15) \
            ABORT("GC_printf clobbered stack"); \
        } while (0)
#else
#define GC_PRINTF_FILLBUF(buf, format) \
      do { \
      } while (0)
#endif
void GC_printf(const char *format, ...)
{
    if (!GC_quiet) {
      char buf[BUFSZ + 1];

      GC_PRINTF_FILLBUF(buf, format);
#     ifdef NACL
        (void)WRITE(GC_stdout, buf, strlen(buf));
        /* Ignore errors silently.      */
#     else
        if (WRITE(GC_stdout, buf, strlen(buf)) < 0)
          ABORT("write to stdout failed");
#     endif
    }
}

void GC_err_printf(const char *format, ...)
{
    char buf[BUFSZ + 1];

    GC_PRINTF_FILLBUF(buf, format);
    GC_err_puts(buf);
}

void GC_log_printf(const char *format, ...)
{
    char buf[BUFSZ + 1];

    GC_PRINTF_FILLBUF(buf, format);
#   ifdef NACL
      (void)WRITE(GC_log, buf, strlen(buf));
#   else
      if (WRITE(GC_log, buf, strlen(buf)) < 0)
        ABORT("write to GC log failed");
#   endif
}

#ifndef GC_ANDROID_LOG

# define GC_warn_printf GC_err_printf

#else

  GC_INNER void GC_info_log_printf(const char *format, ...)
  {
    char buf[BUFSZ + 1];

    GC_PRINTF_FILLBUF(buf, format);
    (void)WRITE(ANDROID_LOG_INFO, buf, 0 /* unused */);
  }

  GC_INNER void GC_verbose_log_printf(const char *format, ...)
  {
    char buf[BUFSZ + 1];

    GC_PRINTF_FILLBUF(buf, format);
    (void)WRITE(ANDROID_LOG_VERBOSE, buf, 0); /* ignore write errors */
  }

  STATIC void GC_warn_printf(const char *format, ...)
  {
    char buf[BUFSZ + 1];

    GC_PRINTF_FILLBUF(buf, format);
    (void)WRITE(ANDROID_LOG_WARN, buf, 0);
  }

#endif /* GC_ANDROID_LOG */

void GC_err_puts(const char *s)
{
    (void)WRITE(GC_stderr, s, strlen(s)); /* ignore errors */
}

STATIC void GC_CALLBACK GC_default_warn_proc(char *msg, GC_word arg)
{
    /* TODO: Add assertion on arg comply with msg (format).     */
    GC_warn_printf(msg, arg);
}

GC_INNER GC_warn_proc GC_current_warn_proc = GC_default_warn_proc;

/* This is recommended for production code (release). */
GC_API void GC_CALLBACK GC_ignore_warn_proc(char *msg, GC_word arg)
{
    if (GC_print_stats) {
      /* Don't ignore warnings if stats printing is on. */
      GC_default_warn_proc(msg, arg);
    }
}

GC_API void GC_CALL GC_set_warn_proc(GC_warn_proc p)
{
    DCL_LOCK_STATE;
    GC_ASSERT(NONNULL_ARG_NOT_NULL(p));
#   ifdef GC_WIN32_THREADS
#     ifdef CYGWIN32
        /* Need explicit GC_INIT call */
        GC_ASSERT(GC_is_initialized);
#     else
        if (!GC_is_initialized) GC_init();
#     endif
#   endif
    LOCK();
    GC_current_warn_proc = p;
    UNLOCK();
}

GC_API GC_warn_proc GC_CALL GC_get_warn_proc(void)
{
    GC_warn_proc result;
    DCL_LOCK_STATE;
    LOCK();
    result = GC_current_warn_proc;
    UNLOCK();
    return(result);
}

#if !defined(PCR) && !defined(SMALL_CONFIG)
  /* Print (or display) a message before abnormal exit (including       */
  /* abort).  Invoked from ABORT(msg) macro (there msg is non-NULL)     */
  /* and from EXIT() macro (msg is NULL in that case).                  */
  STATIC void GC_CALLBACK GC_default_on_abort(const char *msg)
  {
#   ifndef DONT_USE_ATEXIT
      skip_gc_atexit = TRUE; /* disable at-exit GC_gcollect() */
#   endif

    if (msg != NULL) {
#     if defined(MSWIN32) && !defined(MSWINRT_FLAVOR) && !defined(_XBOX_ONE)
        GC_win32_MessageBoxA(msg, "Fatal error in GC", MB_ICONERROR | MB_OK);
        /* Also duplicate msg to GC log file.   */
#     endif

#   ifndef GC_ANDROID_LOG
      /* Avoid calling GC_err_printf() here, as GC_on_abort() could be  */
      /* called from it.  Note 1: this is not an atomic output.         */
      /* Note 2: possible write errors are ignored.                     */
#     if defined(THREADS) && defined(GC_ASSERTIONS) \
         && (defined(MSWIN32) || defined(MSWINCE))
        if (!GC_write_disabled)
#     endif
      {
        if (WRITE(GC_stderr, msg, strlen(msg)) >= 0)
          (void)WRITE(GC_stderr, "\n", 1);
      }
#   else
      __android_log_assert("*" /* cond */, GC_ANDROID_LOG_TAG, "%s\n", msg);
#   endif
    }

#   if !defined(NO_DEBUGGING) && !defined(GC_ANDROID_LOG)
      if (GETENV("GC_LOOP_ON_ABORT") != NULL) {
            /* In many cases it's easier to debug a running process.    */
            /* It's arguably nicer to sleep, but that makes it harder   */
            /* to look at the thread if the debugger doesn't know much  */
            /* about threads.                                           */
            for(;;) {
              /* Empty */
            }
      }
#   endif
  }

  GC_abort_func GC_on_abort = GC_default_on_abort;

  GC_API void GC_CALL GC_set_abort_func(GC_abort_func fn)
  {
      DCL_LOCK_STATE;
      GC_ASSERT(NONNULL_ARG_NOT_NULL(fn));
      LOCK();
      GC_on_abort = fn;
      UNLOCK();
  }

  GC_API GC_abort_func GC_CALL GC_get_abort_func(void)
  {
      GC_abort_func fn;
      DCL_LOCK_STATE;
      LOCK();
      fn = GC_on_abort;
      UNLOCK();
      return fn;
  }
#endif /* !SMALL_CONFIG */

GC_API void GC_CALL GC_enable(void)
{
    DCL_LOCK_STATE;

    LOCK();
    GC_ASSERT(GC_dont_gc != 0); /* ensure no counter underflow */
    GC_dont_gc--;
    UNLOCK();
}

GC_API void GC_CALL GC_disable(void)
{
    DCL_LOCK_STATE;
    LOCK();
    GC_dont_gc++;
    UNLOCK();
}

GC_API int GC_CALL GC_is_disabled(void)
{
    return GC_dont_gc != 0;
}

/* Helper procedures for new kind creation.     */
GC_API void ** GC_CALL GC_new_free_list_inner(void)
{
    void *result;

    GC_ASSERT(I_HOLD_LOCK());
    result = GC_INTERNAL_MALLOC((MAXOBJGRANULES+1) * sizeof(ptr_t), PTRFREE);
    if (NULL == result) ABORT("Failed to allocate freelist for new kind");
    BZERO(result, (MAXOBJGRANULES+1)*sizeof(ptr_t));
    return (void **)result;
}

GC_API void ** GC_CALL GC_new_free_list(void)
{
    void ** result;
    DCL_LOCK_STATE;
    LOCK();
    result = GC_new_free_list_inner();
    UNLOCK();
    return result;
}

GC_API unsigned GC_CALL GC_new_kind_inner(void **fl, GC_word descr,
                                          int adjust, int clear)
{
    unsigned result = GC_n_kinds;

    GC_ASSERT(adjust == FALSE || adjust == TRUE);
    /* If an object is not needed to be cleared (when moved to the      */
    /* free list) then its descriptor should be zero to denote          */
    /* a pointer-free object (and, as a consequence, the size of the    */
    /* object should not be added to the descriptor template).          */
    GC_ASSERT(clear == TRUE
              || (descr == 0 && adjust == FALSE && clear == FALSE));
    if (result < MAXOBJKINDS) {
      GC_n_kinds++;
      GC_obj_kinds[result].ok_freelist = fl;
      GC_obj_kinds[result].ok_reclaim_list = 0;
      GC_obj_kinds[result].ok_descriptor = descr;
      GC_obj_kinds[result].ok_relocate_descr = adjust;
      GC_obj_kinds[result].ok_init = (GC_bool)clear;
#     ifdef ENABLE_DISCLAIM
        GC_obj_kinds[result].ok_mark_unconditionally = FALSE;
        GC_obj_kinds[result].ok_disclaim_proc = 0;
#     endif
    } else {
      ABORT("Too many kinds");
    }
    return result;
}

GC_API unsigned GC_CALL GC_new_kind(void **fl, GC_word descr, int adjust,
                                    int clear)
{
    unsigned result;
    DCL_LOCK_STATE;
    LOCK();
    result = GC_new_kind_inner(fl, descr, adjust, clear);
    UNLOCK();
    return result;
}

GC_API unsigned GC_CALL GC_new_proc_inner(GC_mark_proc proc)
{
    unsigned result = GC_n_mark_procs;

    if (result < MAX_MARK_PROCS) {
      GC_n_mark_procs++;
      GC_mark_procs[result] = proc;
    } else {
      ABORT("Too many mark procedures");
    }
    return result;
}

GC_API unsigned GC_CALL GC_new_proc(GC_mark_proc proc)
{
    unsigned result;
    DCL_LOCK_STATE;
    LOCK();
    result = GC_new_proc_inner(proc);
    UNLOCK();
    return result;
}

GC_API void * GC_CALL GC_call_with_alloc_lock(GC_fn_type fn, void *client_data)
{
    void * result;
    DCL_LOCK_STATE;

#   ifdef THREADS
      LOCK();
#   endif
    result = (*fn)(client_data);
#   ifdef THREADS
      UNLOCK();
#   endif
    return(result);
}

GC_API void * GC_CALL GC_call_with_stack_base(GC_stack_base_func fn, void *arg)
{
    struct GC_stack_base base;
    void *result;

    base.mem_base = (void *)&base;
#   ifdef IA64
      base.reg_base = (void *)GC_save_regs_in_stack();
      /* Unnecessarily flushes register stack,          */
      /* but that probably doesn't hurt.                */
#   endif
    result = fn(&base, arg);
    /* Strongly discourage the compiler from treating the above */
    /* as a tail call.                                          */
    GC_noop1((word)(&base));
    return result;
}

#ifndef THREADS

GC_INNER ptr_t GC_blocked_sp = NULL;
        /* NULL value means we are not inside GC_do_blocking() call. */
# ifdef IA64
    STATIC ptr_t GC_blocked_register_sp = NULL;
# endif

GC_INNER struct GC_traced_stack_sect_s *GC_traced_stack_sect = NULL;

/* This is nearly the same as in win32_threads.c        */
GC_API void * GC_CALL GC_call_with_gc_active(GC_fn_type fn,
                                             void * client_data)
{
    struct GC_traced_stack_sect_s stacksect;
    GC_ASSERT(GC_is_initialized);

    /* Adjust our stack base value (this could happen if        */
    /* GC_get_main_stack_base() is unimplemented or broken for  */
    /* the platform).                                           */
    if ((word)GC_stackbottom HOTTER_THAN (word)(&stacksect))
      GC_stackbottom = (ptr_t)(&stacksect);

    if (GC_blocked_sp == NULL) {
      /* We are not inside GC_do_blocking() - do nothing more.  */
      client_data = fn(client_data);
      /* Prevent treating the above as a tail call.     */
      GC_noop1((word)(&stacksect));
      return client_data; /* result */
    }

    /* Setup new "stack section".       */
    stacksect.saved_stack_ptr = GC_blocked_sp;
#   ifdef IA64
      /* This is the same as in GC_call_with_stack_base().      */
      stacksect.backing_store_end = GC_save_regs_in_stack();
      /* Unnecessarily flushes register stack,          */
      /* but that probably doesn't hurt.                */
      stacksect.saved_backing_store_ptr = GC_blocked_register_sp;
#   endif
    stacksect.prev = GC_traced_stack_sect;
    GC_blocked_sp = NULL;
    GC_traced_stack_sect = &stacksect;

    client_data = fn(client_data);
    GC_ASSERT(GC_blocked_sp == NULL);
    GC_ASSERT(GC_traced_stack_sect == &stacksect);

#   if defined(CPPCHECK)
      GC_noop1((word)GC_traced_stack_sect - (word)GC_blocked_sp);
#   endif
    /* Restore original "stack section".        */
    GC_traced_stack_sect = stacksect.prev;
#   ifdef IA64
      GC_blocked_register_sp = stacksect.saved_backing_store_ptr;
#   endif
    GC_blocked_sp = stacksect.saved_stack_ptr;

    return client_data; /* result */
}

/* This is nearly the same as in win32_threads.c        */
STATIC void GC_do_blocking_inner(ptr_t data, void * context GC_ATTR_UNUSED)
{
    struct blocking_data * d = (struct blocking_data *) data;
    GC_ASSERT(GC_is_initialized);
    GC_ASSERT(GC_blocked_sp == NULL);
#   ifdef SPARC
        GC_blocked_sp = GC_save_regs_in_stack();
#   else
        GC_blocked_sp = (ptr_t) &d; /* save approx. sp */
#   endif
#   ifdef IA64
        GC_blocked_register_sp = GC_save_regs_in_stack();
#   endif

    d -> client_data = (d -> fn)(d -> client_data);

#   ifdef SPARC
        GC_ASSERT(GC_blocked_sp != NULL);
#   else
        GC_ASSERT(GC_blocked_sp == (ptr_t)(&d));
#   endif
#   if defined(CPPCHECK)
      GC_noop1((word)GC_blocked_sp);
#   endif
    GC_blocked_sp = NULL;
}

#endif /* !THREADS */

/* Wrapper for functions that are likely to block (or, at least, do not */
/* allocate garbage collected memory and/or manipulate pointers to the  */
/* garbage collected heap) for an appreciable length of time.           */
/* In the single threaded case, GC_do_blocking() (together              */
/* with GC_call_with_gc_active()) might be used to make stack scanning  */
/* more precise (i.e. scan only stack frames of functions that allocate */
/* garbage collected memory and/or manipulate pointers to the garbage   */
/* collected heap).                                                     */
GC_API void * GC_CALL GC_do_blocking(GC_fn_type fn, void * client_data)
{
    struct blocking_data my_data;

    my_data.fn = fn;
    my_data.client_data = client_data;
    GC_with_callee_saves_pushed(GC_do_blocking_inner, (ptr_t)(&my_data));
    return my_data.client_data; /* result */
}

#if !defined(NO_DEBUGGING)
  GC_API void GC_CALL GC_dump(void)
  {
    DCL_LOCK_STATE;

    LOCK();
    GC_dump_named(NULL);
    UNLOCK();
  }

  GC_API void GC_CALL GC_dump_named(const char *name)
  {
#   ifndef NO_CLOCK
      CLOCK_TYPE current_time;

      GET_TIME(current_time);
#   endif
    if (name != NULL) {
      GC_printf("***GC Dump %s\n", name);
    } else {
      GC_printf("***GC Dump collection #%lu\n", (unsigned long)GC_gc_no);
    }
#   ifndef NO_CLOCK
      /* Note that the time is wrapped in ~49 days if sizeof(long)==4.  */
      GC_printf("Time since GC init: %lu msecs\n",
                MS_TIME_DIFF(current_time, GC_init_time));
#   endif

    GC_printf("\n***Static roots:\n");
    GC_print_static_roots();
    GC_printf("\n***Heap sections:\n");
    GC_print_heap_sects();
    GC_printf("\n***Free blocks:\n");
    GC_print_hblkfreelist();
    GC_printf("\n***Blocks in use:\n");
    GC_print_block_list();
  }
#endif /* !NO_DEBUGGING */

static void block_add_size(struct hblk *h, word pbytes)
{
  hdr *hhdr = HDR(h);
  *(word *)pbytes += (WORDS_TO_BYTES(hhdr->hb_sz) + (HBLKSIZE - 1))
                        & ~(word)(HBLKSIZE - 1);
}

GC_API size_t GC_CALL GC_get_memory_use(void)
{
  word bytes = 0;
  DCL_LOCK_STATE;

  LOCK();
  GC_apply_to_all_blocks(block_add_size, (word)(&bytes));
  UNLOCK();
  return (size_t)bytes;
}

/* Getter functions for the public Read-only variables.                 */

/* GC_get_gc_no() is unsynchronized and should be typically called      */
/* inside the context of GC_call_with_alloc_lock() to prevent data      */
/* races (on multiprocessors).                                          */
GC_API GC_word GC_CALL GC_get_gc_no(void)
{
    return GC_gc_no;
}

#ifdef THREADS
  GC_API int GC_CALL GC_get_parallel(void)
  {
    /* GC_parallel is initialized at start-up.  */
    return GC_parallel;
  }

  GC_INNER GC_on_thread_event_proc GC_on_thread_event = 0;

  GC_API void GC_CALL GC_set_on_thread_event(GC_on_thread_event_proc fn)
  {
    /* fn may be 0 (means no event notifier). */
    DCL_LOCK_STATE;
    LOCK();
    GC_on_thread_event = fn;
    UNLOCK();
  }

  GC_API GC_on_thread_event_proc GC_CALL GC_get_on_thread_event(void)
  {
    GC_on_thread_event_proc fn;
    DCL_LOCK_STATE;
    LOCK();
    fn = GC_on_thread_event;
    UNLOCK();
    return fn;
  }
#endif /* THREADS */

/* Setter and getter functions for the public R/W function variables.   */
/* These functions are synchronized (like GC_set_warn_proc() and        */
/* GC_get_warn_proc()).                                                 */

GC_API void GC_CALL GC_set_oom_fn(GC_oom_func fn)
{
    GC_ASSERT(NONNULL_ARG_NOT_NULL(fn));
    DCL_LOCK_STATE;
    LOCK();
    GC_oom_fn = fn;
    UNLOCK();
}

GC_API GC_oom_func GC_CALL GC_get_oom_fn(void)
{
    GC_oom_func fn;
    DCL_LOCK_STATE;
    LOCK();
    fn = GC_oom_fn;
    UNLOCK();
    return fn;
}

GC_API void GC_CALL GC_set_on_heap_resize(GC_on_heap_resize_proc fn)
{
    /* fn may be 0 (means no event notifier). */
    DCL_LOCK_STATE;
    LOCK();
    GC_on_heap_resize = fn;
    UNLOCK();
}

GC_API GC_on_heap_resize_proc GC_CALL GC_get_on_heap_resize(void)
{
    GC_on_heap_resize_proc fn;
    DCL_LOCK_STATE;
    LOCK();
    fn = GC_on_heap_resize;
    UNLOCK();
    return fn;
}

GC_API void GC_CALL GC_set_finalizer_notifier(GC_finalizer_notifier_proc fn)
{
    /* fn may be 0 (means no finalizer notifier). */
    DCL_LOCK_STATE;
    LOCK();
    GC_finalizer_notifier = fn;
    UNLOCK();
}

GC_API GC_finalizer_notifier_proc GC_CALL GC_get_finalizer_notifier(void)
{
    GC_finalizer_notifier_proc fn;
    DCL_LOCK_STATE;
    LOCK();
    fn = GC_finalizer_notifier;
    UNLOCK();
    return fn;
}

/* Setter and getter functions for the public numeric R/W variables.    */
/* It is safe to call these functions even before GC_INIT().            */
/* These functions are unsynchronized and should be typically called    */
/* inside the context of GC_call_with_alloc_lock() (if called after     */
/* GC_INIT()) to prevent data races (unless it is guaranteed the        */
/* collector is not multi-threaded at that execution point).            */

GC_API void GC_CALL GC_set_find_leak(int value)
{
    /* value is of boolean type. */
    GC_find_leak = value;
}

GC_API int GC_CALL GC_get_find_leak(void)
{
    return GC_find_leak;
}

GC_API void GC_CALL GC_set_all_interior_pointers(int value)
{
    DCL_LOCK_STATE;

    GC_all_interior_pointers = value ? 1 : 0;
    if (GC_is_initialized) {
      /* It is not recommended to change GC_all_interior_pointers value */
      /* after GC is initialized but it seems GC could work correctly   */
      /* even after switching the mode.                                 */
      LOCK();
      GC_initialize_offsets(); /* NOTE: this resets manual offsets as well */
      if (!GC_all_interior_pointers)
        GC_bl_init_no_interiors();
      UNLOCK();
    }
}

GC_API int GC_CALL GC_get_all_interior_pointers(void)
{
    return GC_all_interior_pointers;
}

GC_API void GC_CALL GC_set_finalize_on_demand(int value)
{
    GC_ASSERT(value != -1);
    /* value is of boolean type. */
    GC_finalize_on_demand = value;
}

GC_API int GC_CALL GC_get_finalize_on_demand(void)
{
    return GC_finalize_on_demand;
}

GC_API void GC_CALL GC_set_java_finalization(int value)
{
    GC_ASSERT(value != -1);
    /* value is of boolean type. */
    GC_java_finalization = value;
}

GC_API int GC_CALL GC_get_java_finalization(void)
{
    return GC_java_finalization;
}

GC_API void GC_CALL GC_set_dont_expand(int value)
{
    GC_ASSERT(value != -1);
    /* value is of boolean type. */
    GC_dont_expand = value;
}

GC_API int GC_CALL GC_get_dont_expand(void)
{
    return GC_dont_expand;
}

GC_API void GC_CALL GC_set_no_dls(int value)
{
    GC_ASSERT(value != -1);
    /* value is of boolean type. */
    GC_no_dls = value;
}

GC_API int GC_CALL GC_get_no_dls(void)
{
    return GC_no_dls;
}

GC_API void GC_CALL GC_set_non_gc_bytes(GC_word value)
{
    GC_non_gc_bytes = value;
}

GC_API GC_word GC_CALL GC_get_non_gc_bytes(void)
{
    return GC_non_gc_bytes;
}

GC_API void GC_CALL GC_set_free_space_divisor(GC_word value)
{
    GC_ASSERT(value > 0);
    GC_free_space_divisor = value;
}

GC_API GC_word GC_CALL GC_get_free_space_divisor(void)
{
    return GC_free_space_divisor;
}

GC_API void GC_CALL GC_set_max_retries(GC_word value)
{
    GC_ASSERT(value != ~(word)0);
    GC_max_retries = value;
}

GC_API GC_word GC_CALL GC_get_max_retries(void)
{
    return GC_max_retries;
}

GC_API void GC_CALL GC_set_dont_precollect(int value)
{
    GC_ASSERT(value != -1);
    /* value is of boolean type. */
    GC_dont_precollect = value;
}

GC_API int GC_CALL GC_get_dont_precollect(void)
{
    return GC_dont_precollect;
}

GC_API void GC_CALL GC_set_full_freq(int value)
{
    GC_ASSERT(value >= 0);
    GC_full_freq = value;
}

GC_API int GC_CALL GC_get_full_freq(void)
{
    return GC_full_freq;
}

GC_API void GC_CALL GC_set_time_limit(unsigned long value)
{
    GC_ASSERT(value != (unsigned long)-1L);
    GC_time_limit = value * 1000000;
}

GC_API unsigned long GC_CALL GC_get_time_limit(void)
{
    return (unsigned long)(GC_time_limit / 1000000);
}

GC_API void GC_CALL GC_set_time_limit_ns(unsigned long long value)
{
    GC_ASSERT(value != (unsigned long long)-1L);
    GC_time_limit = value;
}

GC_API unsigned long long GC_CALL GC_get_time_limit_ns(void)
{
    return GC_time_limit;
}

GC_API void GC_CALL GC_set_force_unmap_on_gcollect(int value)
{
    GC_force_unmap_on_gcollect = (GC_bool)value;
}

GC_API int GC_CALL GC_get_force_unmap_on_gcollect(void)
{
    return (int)GC_force_unmap_on_gcollect;
}

/* Unity specific APIs */
GC_API void GC_CALL GC_stop_world_external()
{
    LOCK();
    STOP_WORLD();
}

GC_API void GC_CALL GC_start_world_external()
{
    START_WORLD();
    UNLOCK();
}

/* Disable incremental GC. Only tested with MANUAL_VDB mode. Might */
/* require extra teardown work when using other VDB configs.*/
GC_API void GC_CALL GC_disable_incremental(void)
{
  LOCK();
  GC_gcollect_inner();
#ifndef GC_DISABLE_INCREMENTAL
  GC_incremental = FALSE;
#endif
  UNLOCK();
}
