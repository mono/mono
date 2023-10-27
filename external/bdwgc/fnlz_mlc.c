/*
 * Copyright (c) 2011 by Hewlett-Packard Company.  All rights reserved.
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

#include "private/gc_priv.h"

#ifdef ENABLE_DISCLAIM

#include "gc_disclaim.h"
#include "gc_inline.h" /* for GC_malloc_kind */

STATIC int GC_finalized_kind = 0;

#if defined(KEEP_BACK_PTRS) || defined(MAKE_BACK_GRAPH)
  /* The first bit is already used for a debug purpose. */
# define FINALIZER_CLOSURE_FLAG 0x2
#else
# define FINALIZER_CLOSURE_FLAG 0x1
#endif

STATIC int GC_CALLBACK GC_finalized_disclaim(void *obj)
{
    word fc_word = *(word *)obj;

    if ((fc_word & FINALIZER_CLOSURE_FLAG) != 0) {
       /* The disclaim function may be passed fragments from the        */
       /* free-list, on which it should not run finalization.           */
       /* To recognize this case, we use the fact that the first word   */
       /* on such fragments is always multiple of 4 (a link to the next */
       /* fragment, or NULL).  If it is desirable to have a finalizer   */
       /* which does not use the first word for storing finalization    */
       /* info, GC_reclaim_with_finalization must be extended to clear  */
       /* fragments so that the assumption holds for the selected word. */
        const struct GC_finalizer_closure *fc
                        = (struct GC_finalizer_closure *)(fc_word
                                        & ~(word)FINALIZER_CLOSURE_FLAG);
        (*fc->proc)((word *)obj + 1, fc->cd);
    }
    return 0;
}

GC_API void GC_CALL GC_init_finalized_malloc(void)
{
    DCL_LOCK_STATE;

    GC_init();  /* In case it's not already done.       */
    LOCK();
    if (GC_finalized_kind != 0) {
        UNLOCK();
        return;
    }

    /* The finalizer closure is placed in the first word in order to    */
    /* use the lower bits to distinguish live objects from objects on   */
    /* the free list.  The downside of this is that we need one-word    */
    /* offset interior pointers, and that GC_base does not return the   */
    /* start of the user region.                                        */
    GC_register_displacement_inner(sizeof(word));

    GC_finalized_kind = GC_new_kind_inner(GC_new_free_list_inner(),
                                          GC_DS_LENGTH, TRUE, TRUE);
    GC_ASSERT(GC_finalized_kind != 0);
    GC_register_disclaim_proc(GC_finalized_kind, GC_finalized_disclaim, TRUE);
    UNLOCK();
}

GC_API void GC_CALL GC_register_disclaim_proc(int kind, GC_disclaim_proc proc,
                                              int mark_unconditionally)
{
    GC_ASSERT((unsigned)kind < MAXOBJKINDS);
    GC_obj_kinds[kind].ok_disclaim_proc = proc;
    GC_obj_kinds[kind].ok_mark_unconditionally = (GC_bool)mark_unconditionally;
}

GC_API GC_ATTR_MALLOC void * GC_CALL GC_finalized_malloc(size_t lb,
                                const struct GC_finalizer_closure *fclos)
{
    word *op;

    GC_ASSERT(GC_finalized_kind != 0);
    op = (word *)GC_malloc_kind(SIZET_SAT_ADD(lb, sizeof(word)),
                                GC_finalized_kind);
    if (EXPECT(NULL == op, FALSE))
        return NULL;
    *op = (word)fclos | FINALIZER_CLOSURE_FLAG;
    return op + 1;
}

#endif /* ENABLE_DISCLAIM */
