/*
 * Copyright (c) 1996-1998 by Silicon Graphics.  All rights reserved.
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

/* This file should never be included by clients directly.      */

#ifndef GC_ALLOC_PTRS_H
#define GC_ALLOC_PTRS_H

#include "gc.h"

#ifdef __cplusplus
  extern "C" {
#endif

GC_API void ** const GC_objfreelist_ptr;
GC_API void ** const GC_aobjfreelist_ptr;
GC_API void ** const GC_uobjfreelist_ptr;

#ifdef GC_ATOMIC_UNCOLLECTABLE
  GC_API void ** const GC_auobjfreelist_ptr;
#endif

GC_API void GC_CALL GC_incr_bytes_allocd(size_t bytes);
GC_API void GC_CALL GC_incr_bytes_freed(size_t bytes);

#ifdef __cplusplus
  } /* extern "C" */
#endif

#endif /* GC_ALLOC_PTRS_H */
