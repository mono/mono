/* 
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

/*
 * Interface for thread local allocation.  Memory obtained
 * this way can be used by all threads, as though it were obtained
 * from an allocator like GC_malloc.  The difference is that GC_local_malloc
 * counts the number of allocations of a given size from the current thread,
 * and uses GC_malloc_many to perform the allocations once a threashold
 * is exceeded.  Thus far less synchronization may be needed.
 * Allocation of known large objects should not use this interface.
 * This interface is designed primarily for fast allocation of small
 * objects on multiprocessors, e.g. for a JVM running on an MP server.
 *
 * If this file is included with GC_GCJ_SUPPORT defined, GCJ-style
 * bitmap allocation primitives will also be included.
 *
 * If this file is included with GC_REDIRECT_TO_LOCAL defined, then
 * GC_MALLOC, GC_MALLOC_ATOMIC, and possibly GC_GCJ_MALLOC will
 * be redefined to use the thread local allocatoor.
 *
 * The interface is available only if the collector is built with
 * -DTHREAD_LOCAL_ALLOC, which is currently supported only on Linux.
 *
 * The debugging allocators use standard, not thread-local allocation.
 *
 * These routines normally require an explicit call to GC_init(), though
 * that may be done from a constructor function.
 */

#ifndef GC_LOCAL_ALLOC_H
#define GC_LOCAL_ALLOC_H

#ifndef _GC_H
#   include "gc.h"
#endif

#if defined(GC_GCJ_SUPPORT) && !defined(GC_GCJ_H)
#   include "gc_gcj.h"
#endif

/* We assume ANSI C for this interface.	*/

GC_PTR GC_local_malloc(size_t bytes);

GC_PTR GC_local_malloc_atomic(size_t bytes);

#if defined(GC_GCJ_SUPPORT)
  GC_PTR GC_local_gcj_malloc(size_t bytes,
			     void * ptr_to_struct_containing_descr);
#endif

# ifdef GC_DEBUG
    /* We don't really use local allocation in this case.	*/
#   define GC_LOCAL_MALLOC(s) GC_debug_malloc(s,GC_EXTRAS)
#   define GC_LOCAL_MALLOC_ATOMIC(s) GC_debug_malloc_atomic(s,GC_EXTRAS)
#   ifdef GC_GCJ_SUPPORT
#	define GC_LOCAL_GCJ_MALLOC(s,d) GC_debug_gcj_malloc(s,d,GC_EXTRAS)
#   endif
# else
#   define GC_LOCAL_MALLOC(s) GC_local_malloc(s)
#   define GC_LOCAL_MALLOC_ATOMIC(s) GC_local_malloc_atomic(s)
#   ifdef GC_GCJ_SUPPORT
#	define GC_LOCAL_GCJ_MALLOC(s,d) GC_local_gcj_malloc(s,d)
#   endif
# endif

# ifdef GC_REDIRECT_TO_LOCAL
#   undef GC_MALLOC
#   define GC_MALLOC(s) GC_LOCAL_MALLOC(s)
#   undef GC_MALLOC_ATOMIC
#   define GC_MALLOC_ATOMIC(s) GC_LOCAL_MALLOC_ATOMIC(s)
#   ifdef GC_GCJ_SUPPORT
#	undef GC_GCJ_MALLOC
# 	define GC_GCJ_MALLOC(s,d) GC_LOCAL_GCJ_MALLOC(s,d)
#   endif
# endif

#endif /* GC_LOCAL_ALLOC_H */
