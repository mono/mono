/*
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1997 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 2000 by Hewlett-Packard Company.  All rights reserved.
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

#include "private/gc_priv.h"

/* This used to be in dyn_load.c.  It was extracted into a separate     */
/* file to avoid having to link against libdl.{a,so} if the client      */
/* doesn't call dlopen.  Of course this fails if the collector is in    */
/* a dynamic library. -HB                                               */
#if defined(GC_PTHREADS) && !defined(GC_NO_DLOPEN)

#undef GC_MUST_RESTORE_REDEFINED_DLOPEN
#if defined(dlopen) && !defined(GC_USE_LD_WRAP)
  /* To support various threads pkgs, gc.h interposes on dlopen by      */
  /* defining "dlopen" to be "GC_dlopen", which is implemented below.   */
  /* However, both GC_FirstDLOpenedLinkMap() and GC_dlopen() use the    */
  /* real system dlopen() in their implementation. We first remove      */
  /* gc.h's dlopen definition and restore it later, after GC_dlopen().  */
# undef dlopen
# define GC_MUST_RESTORE_REDEFINED_DLOPEN
#endif

/* Make sure we're not in the middle of a collection, and make sure we  */
/* don't start any.  This is invoked prior to a dlopen call to avoid    */
/* synchronization issues.  We can't just acquire the allocation lock,  */
/* since startup code in dlopen may try to allocate.  This solution     */
/* risks heap growth (or, even, heap overflow) in the presence of many  */
/* dlopen calls in either a multi-threaded environment, or if the       */
/* library initialization code allocates substantial amounts of GC'ed   */
/* memory.                                                              */
#ifndef USE_PROC_FOR_LIBRARIES
  static void disable_gc_for_dlopen(void)
  {
    DCL_LOCK_STATE;
    LOCK();
    while (GC_incremental && GC_collection_in_progress()) {
      GC_collect_a_little_inner(1000);
    }
    ++GC_dont_gc;
    UNLOCK();
  }
#endif

/* Redefine dlopen to guarantee mutual exclusion with           */
/* GC_register_dynamic_libraries.  Should probably happen for   */
/* other operating systems, too.                                */

/* This is similar to WRAP/REAL_FUNC() in pthread_support.c.    */
#ifdef GC_USE_LD_WRAP
# define WRAP_DLFUNC(f) __wrap_##f
# define REAL_DLFUNC(f) __real_##f
  void * REAL_DLFUNC(dlopen)(const char *, int);
#else
# define WRAP_DLFUNC(f) GC_##f
# define REAL_DLFUNC(f) f
#endif

GC_API void * WRAP_DLFUNC(dlopen)(const char *path, int mode)
{
  void * result;

# ifndef USE_PROC_FOR_LIBRARIES
    /* Disable collections.  This solution risks heap growth (or,       */
    /* even, heap overflow) but there seems no better solutions.        */
    disable_gc_for_dlopen();
# endif
  result = REAL_DLFUNC(dlopen)(path, mode);
# ifndef USE_PROC_FOR_LIBRARIES
    GC_enable(); /* undoes disable_gc_for_dlopen */
# endif
  return(result);
}

#ifdef GC_USE_LD_WRAP
  /* Define GC_ function as an alias for the plain one, which will be   */
  /* intercepted.  This allows files which include gc.h, and hence      */
  /* generate references to the GC_ symbol, to see the right symbol.    */
  GC_API void *GC_dlopen(const char *path, int mode)
  {
    return dlopen(path, mode);
  }
#endif /* GC_USE_LD_WRAP */

#ifdef GC_MUST_RESTORE_REDEFINED_DLOPEN
# define dlopen GC_dlopen
#endif

#endif  /* GC_PTHREADS && !GC_NO_DLOPEN */
