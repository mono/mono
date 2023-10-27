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

#include "gc_gcj.h"
#include "private/dbg_mlc.h"

#ifdef GC_ASSERTIONS
  GC_INNER /* variable is also used in thread_local_alloc.c */
#else
  STATIC
#endif
GC_bool GC_gcj_malloc_initialized = FALSE;

int GC_gcj_kind = 0;    /* Object kind for objects with descriptors     */
                        /* in "vtable".                                 */
int GC_gcj_debug_kind = 0;
                        /* The kind of objects that is always marked    */
                        /* with a mark proc call.                       */

GC_INNER ptr_t * GC_gcjobjfreelist = NULL;

STATIC struct GC_ms_entry * GC_gcj_fake_mark_proc(word * addr GC_ATTR_UNUSED,
                        struct GC_ms_entry *mark_stack_ptr,
                        struct GC_ms_entry * mark_stack_limit GC_ATTR_UNUSED,
                        word env GC_ATTR_UNUSED)
{
    ABORT_RET("No client gcj mark proc is specified");
    return mark_stack_ptr;
}

/* Caller does not hold allocation lock. */
GC_API void GC_CALL GC_init_gcj_malloc(int mp_index,
                                       void * /* really GC_mark_proc */mp)
{
    GC_bool ignore_gcj_info;
    DCL_LOCK_STATE;

    if (mp == 0)        /* In case GC_DS_PROC is unused.        */
      mp = (void *)(word)GC_gcj_fake_mark_proc;

    GC_init();  /* In case it's not already done.       */
    LOCK();
    if (GC_gcj_malloc_initialized) {
      UNLOCK();
      return;
    }
    GC_gcj_malloc_initialized = TRUE;
#   ifdef GC_IGNORE_GCJ_INFO
      /* This is useful for debugging on platforms with missing getenv(). */
      ignore_gcj_info = 1;
#   else
      ignore_gcj_info = (0 != GETENV("GC_IGNORE_GCJ_INFO"));
#   endif
    if (ignore_gcj_info) {
      GC_COND_LOG_PRINTF("Gcj-style type information is disabled!\n");
    }
    GC_ASSERT(GC_mark_procs[mp_index] == (GC_mark_proc)0); /* unused */
    GC_mark_procs[mp_index] = (GC_mark_proc)(word)mp;
    if ((unsigned)mp_index >= GC_n_mark_procs)
        ABORT("GC_init_gcj_malloc: bad index");
    /* Set up object kind gcj-style indirect descriptor. */
      GC_gcjobjfreelist = (ptr_t *)GC_new_free_list_inner();
      if (ignore_gcj_info) {
        /* Use a simple length-based descriptor, thus forcing a fully   */
        /* conservative scan.                                           */
        GC_gcj_kind = GC_new_kind_inner((void **)GC_gcjobjfreelist,
                                        /* 0 | */ GC_DS_LENGTH,
                                        TRUE, TRUE);
      } else {
        GC_gcj_kind = GC_new_kind_inner(
                        (void **)GC_gcjobjfreelist,
                        (((word)(-(signed_word)MARK_DESCR_OFFSET
                                 - GC_INDIR_PER_OBJ_BIAS))
                         | GC_DS_PER_OBJECT),
                        FALSE, TRUE);
      }
    /* Set up object kind for objects that require mark proc call.      */
      if (ignore_gcj_info) {
        GC_gcj_debug_kind = GC_gcj_kind;
      } else {
        GC_gcj_debug_kind = GC_new_kind_inner(GC_new_free_list_inner(),
                                GC_MAKE_PROC(mp_index,
                                             1 /* allocated with debug info */),
                                FALSE, TRUE);
      }
    UNLOCK();
}

#define GENERAL_MALLOC_INNER(lb,k) \
    GC_clear_stack(GC_generic_malloc_inner(lb, k))

#define GENERAL_MALLOC_INNER_IOP(lb,k) \
    GC_clear_stack(GC_generic_malloc_inner_ignore_off_page(lb, k))

/* We need a mechanism to release the lock and invoke finalizers.       */
/* We don't really have an opportunity to do this on a rarely executed  */
/* path on which the lock is not held.  Thus we check at a              */
/* rarely executed point at which it is safe to release the lock.       */
/* We do this even where we could just call GC_INVOKE_FINALIZERS,       */
/* since it's probably cheaper and certainly more uniform.              */
/* FIXME - Consider doing the same elsewhere?                           */
static void maybe_finalize(void)
{
   static word last_finalized_no = 0;
   DCL_LOCK_STATE;

   if (GC_gc_no == last_finalized_no ||
       !EXPECT(GC_is_initialized, TRUE)) return;
   UNLOCK();
   GC_INVOKE_FINALIZERS();
   LOCK();
   last_finalized_no = GC_gc_no;
}

/* Allocate an object, clear it, and store the pointer to the   */
/* type structure (vtable in gcj).                              */
/* This adds a byte at the end of the object if GC_malloc would.*/
#if !IL2CPP_ENABLE_WRITE_BARRIER_VALIDATION
#ifdef THREAD_LOCAL_ALLOC
  GC_INNER void * GC_core_gcj_malloc(size_t lb,
                                     void * ptr_to_struct_containing_descr)
#else
  GC_API GC_ATTR_MALLOC void * GC_CALL GC_gcj_malloc(size_t lb,
                                      void * ptr_to_struct_containing_descr)
#endif
{
    ptr_t op;
    DCL_LOCK_STATE;

    GC_DBG_COLLECT_AT_MALLOC(lb);
    if(SMALL_OBJ(lb)) {
        word lg;

        LOCK();
        lg = GC_size_map[lb];
        op = GC_gcjobjfreelist[lg];
        if(EXPECT(0 == op, FALSE)) {
            maybe_finalize();
            op = (ptr_t)GENERAL_MALLOC_INNER((word)lb, GC_gcj_kind);
            if (0 == op) {
                GC_oom_func oom_fn = GC_oom_fn;
                UNLOCK();
                return((*oom_fn)(lb));
            }
        } else {
            GC_gcjobjfreelist[lg] = (ptr_t)obj_link(op);
            GC_bytes_allocd += GRANULES_TO_BYTES((word)lg);
        }
        GC_ASSERT(((void **)op)[1] == 0);
    } else {
        LOCK();
        maybe_finalize();
        op = (ptr_t)GENERAL_MALLOC_INNER((word)lb, GC_gcj_kind);
        if (0 == op) {
            GC_oom_func oom_fn = GC_oom_fn;
            UNLOCK();
            return((*oom_fn)(lb));
        }
    }
    *(void **)op = ptr_to_struct_containing_descr;
    UNLOCK();
    GC_dirty(op);
    return((void *) op);
}

#endif

/* Similar to GC_gcj_malloc, but add debug info.  This is allocated     */
/* with GC_gcj_debug_kind.                                              */
GC_API GC_ATTR_MALLOC void * GC_CALL GC_debug_gcj_malloc(size_t lb,
                void * ptr_to_struct_containing_descr, GC_EXTRA_PARAMS)
{
    void * result;
    DCL_LOCK_STATE;

    /* We're careful to avoid extra calls, which could          */
    /* confuse the backtrace.                                   */
    LOCK();
    maybe_finalize();
    result = GC_generic_malloc_inner(SIZET_SAT_ADD(lb, DEBUG_BYTES),
                                     GC_gcj_debug_kind);
    if (result == 0) {
        GC_oom_func oom_fn = GC_oom_fn;
        UNLOCK();
        GC_err_printf("GC_debug_gcj_malloc(%lu, %p) returning NULL (%s:%d)\n",
                (unsigned long)lb, ptr_to_struct_containing_descr, s, i);
        return((*oom_fn)(lb));
    }
    *((void **)((ptr_t)result + sizeof(oh))) = ptr_to_struct_containing_descr;
    if (!GC_debugging_started) {
        GC_start_debugging_inner();
    }
    ADD_CALL_CHAIN(result, ra);
    result = GC_store_debug_info_inner(result, (word)lb, s, i);
    UNLOCK();
    GC_dirty(result);
    return result;
}

/* There is no THREAD_LOCAL_ALLOC for GC_gcj_malloc_ignore_off_page().  */
GC_API GC_ATTR_MALLOC void * GC_CALL GC_gcj_malloc_ignore_off_page(size_t lb,
                                     void * ptr_to_struct_containing_descr)
{
    ptr_t op;
    DCL_LOCK_STATE;

    GC_DBG_COLLECT_AT_MALLOC(lb);
    if(SMALL_OBJ(lb)) {
        word lg;

        LOCK();
        lg = GC_size_map[lb];
        op = GC_gcjobjfreelist[lg];
        if (EXPECT(0 == op, FALSE)) {
            maybe_finalize();
            op = (ptr_t)GENERAL_MALLOC_INNER_IOP(lb, GC_gcj_kind);
            if (0 == op) {
                GC_oom_func oom_fn = GC_oom_fn;
                UNLOCK();
                return((*oom_fn)(lb));
            }
        } else {
            GC_gcjobjfreelist[lg] = (ptr_t)obj_link(op);
            GC_bytes_allocd += GRANULES_TO_BYTES((word)lg);
        }
    } else {
        LOCK();
        maybe_finalize();
        op = (ptr_t)GENERAL_MALLOC_INNER_IOP(lb, GC_gcj_kind);
        if (0 == op) {
            GC_oom_func oom_fn = GC_oom_fn;
            UNLOCK();
            return((*oom_fn)(lb));
        }
    }
    *(void **)op = ptr_to_struct_containing_descr;
    UNLOCK();
    GC_dirty(op);
    return((void *) op);
}

#endif  /* GC_GCJ_SUPPORT */
