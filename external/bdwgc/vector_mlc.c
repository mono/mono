/*
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1999-2004 Hewlett-Packard Development Company, L.P.
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 *
 */

#include "private/gc_pmark.h"  /* includes gc_priv.h */

#ifdef GC_GCJ_SUPPORT

/*
 * This is an allocator interface tuned for gcj (the GNU static
 * java compiler).
 *
 * Each allocated object has a pointer in its first word to a vtable,
 * which for our purposes is simply a structure describing the type of
 * the object.
 * This descriptor structure contains a GC marking descriptor at offset
 * MARK_DESCR_OFFSET.
 *
 * It is hoped that this interface may also be useful for other systems,
 * possibly with some tuning of the constants.  But the immediate goal
 * is to get better gcj performance.
 *
 * We assume:
 *  1) Counting on explicit initialization of this interface is OK;
 *  2) FASTLOCK is not a significant win.
 */

#include "gc_vector.h"
#include "private/dbg_mlc.h"
#include "gc_typed.h"

#ifdef GC_ASSERTIONS
  GC_INNER /* variable is also used in thread_local_alloc.c */
#else
  STATIC
#endif
GC_bool GC_gcj_vector_initialized = FALSE;

int GC_gcj_vector_kind = 0;    /* Object kind for objects with descriptors     */
            /* in "vtable".                                 */

int GC_gcj_vector_mp_index = 0;

GC_INNER ptr_t * GC_gcjvecfreelist = NULL;

/* Caller does not hold allocation lock. */
GC_API void GC_CALL GC_init_gcj_vector (int mp_index,
  void * /* really GC_mark_proc */mp)
{
  DCL_LOCK_STATE;

  if (mp == 0)        /* In case GC_DS_PROC is unused.        */
    ABORT ("GC_init_gcj_vector: bad index");

  GC_init ();  /* In case it's not already done.       */
  LOCK ();
  if (GC_gcj_vector_initialized) {
    UNLOCK ();
    return;
  }
  GC_gcj_vector_initialized = TRUE;
  GC_gcj_vector_mp_index = mp_index;
  GC_ASSERT (GC_mark_procs[mp_index] == (GC_mark_proc)0); /* unused */
  GC_mark_procs[mp_index ] = (GC_mark_proc)(word)mp;
  if ((unsigned)mp_index >= GC_n_mark_procs)
    ABORT ("GC_init_gcj_vector: bad index");
  GC_gcjvecfreelist = (ptr_t *)GC_new_free_list_inner ();
  GC_gcj_vector_kind = GC_new_kind_inner ((void **)GC_gcjvecfreelist,
    GC_MAKE_PROC (mp_index,
      0),
    FALSE, TRUE);

  UNLOCK ();
}

#define GENERAL_MALLOC_INNER(lb,k) \
    GC_clear_stack(GC_generic_malloc_inner(lb, k))

#define GENERAL_MALLOC_INNER_IOP(lb,k) \
    GC_clear_stack(GC_generic_malloc_inner_ignore_off_page(lb, k))

#if !IL2CPP_ENABLE_WRITE_BARRIER_VALIDATION
#ifdef THREAD_LOCAL_ALLOC
  GC_INNER void * GC_gcj_vector_malloc(size_t lb,
                                     void * ptr_to_struct_containing_descr)
#else
  GC_API GC_ATTR_MALLOC void * GC_CALL GC_gcj_vector_malloc (size_t lb,
    void * ptr_to_struct_containing_descr)
#endif
  {
    ptr_t op;
    DCL_LOCK_STATE;

    GC_DBG_COLLECT_AT_MALLOC (lb);
    if (SMALL_OBJ (lb)) {
      word lg;

      LOCK ();
      lg = GC_size_map[lb];
      op = GC_gcjvecfreelist[lg];
      if (EXPECT (0 == op, FALSE)) {
        maybe_finalize ();
        op = (ptr_t)GENERAL_MALLOC_INNER ((word)lb, GC_gcj_vector_kind);
        if (0 == op) {
          GC_oom_func oom_fn = GC_oom_fn;
          UNLOCK ();
          return((*oom_fn)(lb));
        }
      }
      else {
        GC_gcjvecfreelist[lg] = (ptr_t)obj_link (op);
        GC_bytes_allocd += GRANULES_TO_BYTES ((word)lg);
      }
      GC_ASSERT (((void **)op)[1] == 0);
    }
    else {
      LOCK ();
      maybe_finalize ();
      op = (ptr_t)GENERAL_MALLOC_INNER ((word)lb, GC_gcj_vector_kind);
      if (0 == op) {
        GC_oom_func oom_fn = GC_oom_fn;
        UNLOCK ();
        return((*oom_fn)(lb));
      }
    }
    *(void **)op = ptr_to_struct_containing_descr;
    UNLOCK ();
    GC_dirty (op);
    return((void *)op);
  }
#define ELEMENT_CHUNK_SIZE 256

GC_API mse *GC_CALL
GC_gcj_vector_mark_proc (mse *mark_stack_ptr, mse* mark_stack_limit, GC_descr element_desc, word *start, word *end, int words_per_element)
{
  /* create new descriptor that is shifted two bits to account 
  * for lack of object header. Descriptors for value types include
  * the object header for boxed values */

  /* remove tags */
  GC_descr element_desc_shifted = element_desc & ~(GC_DS_TAGS);
  /* shift actual bits */
  element_desc_shifted = element_desc_shifted << 2;
  /* shifted and unmasked desc to use for bulk processing */
  GC_descr element_desc_shifted_unmasked = element_desc_shifted;
  /* add back tag to indicate descriptor is a bitmap */
  element_desc_shifted |= GC_DS_BITMAP;

  size_t remaining_elements = (end - start) / words_per_element;

  /* attempt to bulk process multiple elements with single descriptor */
  size_t elements_per_desc = (CPP_WORDSZ - GC_DS_TAG_BITS) / words_per_element;

  if (mark_stack_ptr >= mark_stack_limit)
    return GC_signal_mark_stack_overflow (mark_stack_ptr);

  /* setup bulk processing */
  if (elements_per_desc > 1) {
    word *current = start;
    size_t bulk_count = remaining_elements / elements_per_desc;
    size_t remainder_count = remaining_elements % elements_per_desc;

    /* bulk processing */
    if (bulk_count) {
      size_t bulk_stride = elements_per_desc * words_per_element;
      GC_descr bulk_desc = 0;
      size_t i;
      for (i = 0; i < elements_per_desc; ++i) {
        bulk_desc |= element_desc_shifted_unmasked >> (i * words_per_element);
      }
      bulk_desc |= GC_DS_BITMAP;

      if (bulk_count > ELEMENT_CHUNK_SIZE) {
        bulk_count = ELEMENT_CHUNK_SIZE;

        /* only process chunk number of items */
        end = start + bulk_count * bulk_stride;

        remainder_count = 0;

        mark_stack_ptr++;
        if (mark_stack_ptr >= mark_stack_limit)
          mark_stack_ptr = GC_signal_mark_stack_overflow (mark_stack_ptr);
        mark_stack_ptr->mse_descr.w = GC_MAKE_PROC (GC_gcj_vector_mp_index, 1 /* continue processing */);
        mark_stack_ptr->mse_start = (ptr_t)end;
      }

      while (bulk_count > 0) {
        mark_stack_ptr++;
        if (mark_stack_ptr >= mark_stack_limit)
          mark_stack_ptr = GC_signal_mark_stack_overflow (mark_stack_ptr);

        mark_stack_ptr->mse_start = (ptr_t) (current);
        mark_stack_ptr->mse_descr.w = bulk_desc;

        current += bulk_stride;

        bulk_count--;
      }
    }

    while (remainder_count > 0) {
      mark_stack_ptr++;
      if (mark_stack_ptr >= mark_stack_limit)
        mark_stack_ptr = GC_signal_mark_stack_overflow (mark_stack_ptr);

      mark_stack_ptr->mse_start = (ptr_t) (current);
      mark_stack_ptr->mse_descr.w = element_desc_shifted;

      current += words_per_element;

      remainder_count--;
    }
  } else {
    size_t remainder_count = remaining_elements;

    if (remainder_count > ELEMENT_CHUNK_SIZE) {
      remainder_count = ELEMENT_CHUNK_SIZE;

      /* only process chunk number of items */
      end = start + remainder_count * words_per_element;

      mark_stack_ptr++;
      if (mark_stack_ptr >= mark_stack_limit)
        mark_stack_ptr = GC_signal_mark_stack_overflow (mark_stack_ptr);

      mark_stack_ptr->mse_descr.w = GC_MAKE_PROC (GC_gcj_vector_mp_index, 1 /* continue processing */);
      mark_stack_ptr->mse_start = (ptr_t)end;
    }

    word *current = start;
    while (remainder_count > 0) {
      mark_stack_ptr++;
      if (mark_stack_ptr >= mark_stack_limit)
        mark_stack_ptr = GC_signal_mark_stack_overflow (mark_stack_ptr);

      mark_stack_ptr->mse_start = (ptr_t) (current);
      mark_stack_ptr->mse_descr.w = element_desc_shifted;

      current += words_per_element;

      remainder_count--;
    }
  }

  return (mark_stack_ptr);
}
#endif

#endif  /* GC_GCJ_SUPPORT */
