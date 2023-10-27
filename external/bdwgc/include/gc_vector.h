/*
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1995 by Xerox Corporation.  All rights reserved.
 * Copyright 1996-1999 by Silicon Graphics.  All rights reserved.
 * Copyright 1999 by Hewlett-Packard Company.  All rights reserved.
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

/* This file assumes the collector has been compiled with GC_GCJ_SUPPORT. */

/*
 * We allocate objects whose first word contains a pointer to a struct
 * describing the object type.  This struct contains a garbage collector mark
 * descriptor at offset MARK_DESCR_OFFSET.  Alternatively, the objects
 * may be marked by the mark procedure passed to GC_init_gcj_malloc.
 */

#ifndef GC_VECTOR_H
#define GC_VECTOR_H

        /* Gcj keeps GC descriptor as second word of vtable.    This    */
        /* probably needs to be adjusted for other clients.             */
        /* We currently assume that this offset is such that:           */
        /*      - all objects of this kind are large enough to have     */
        /*        a value at that offset, and                           */
        /*      - it is not zero.                                       */
        /* These assumptions allow objects on the free list to be       */
        /* marked normally.                                             */

#ifndef GC_H
# include "gc.h"
#endif

# include "gc_typed.h"

#ifdef __cplusplus
  extern "C" {
#endif

GC_API void GC_CALL GC_init_gcj_vector (int /* mp_index */,
  void * /* really mark_proc */ /* mp */);

GC_API GC_ATTR_MALLOC void * GC_CALL GC_gcj_vector_malloc(size_t /* lb */,
  void * /* ptr_to_struct_containing_descr */);

GC_API struct GC_ms_entry *GC_CALL
GC_gcj_vector_mark_proc (struct GC_ms_entry *mark_stack_ptr,
  struct GC_ms_entry* mark_stack_limit,
  GC_descr element_desc,
  GC_word*start,
  GC_word*end,
  int words_per_element);

#ifdef __cplusplus
  } /* extern "C" */
#endif

#endif /* GC_VECTOR_H */
