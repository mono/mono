/* 
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
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
/* Boehm, February 7, 1996 4:32 pm PST */
 
#include <stdio.h>
#include "private/gc_priv.h"

extern ptr_t GC_clear_stack();	/* in misc.c, behaves like identity */
void GC_extend_size_map();	/* in misc.c. */

/* Allocate reclaim list for kind:	*/
/* Return TRUE on success		*/
GC_bool GC_alloc_reclaim_list(kind)
register struct obj_kind * kind;
{
    struct hblk ** result = (struct hblk **)
    		GC_scratch_alloc((MAXOBJSZ+1) * sizeof(struct hblk *));
    if (result == 0) return(FALSE);
    BZERO(result, (MAXOBJSZ+1)*sizeof(struct hblk *));
    kind -> ok_reclaim_list = result;
    return(TRUE);
}

/* Allocate a large block of size lw words.	*/
/* The block is not cleared.			*/
/* Flags is 0 or IGNORE_OFF_PAGE.		*/
/* We hold the allocation lock.			*/
ptr_t GC_alloc_large(lw, k, flags)
word lw;
int k;
unsigned flags;
{
    struct hblk * h;
    word n_blocks = OBJ_SZ_TO_BLOCKS(lw);
    ptr_t result;
	
    if (!GC_is_initialized) GC_init_inner();
    /* Do our share of marking work */
        if(GC_incremental && !GC_dont_gc)
	    GC_collect_a_little_inner((int)n_blocks);
    h = GC_allochblk(lw, k, flags);
#   ifdef USE_MUNMAP
	if (0 == h) {
	    GC_merge_unmapped();
	    h = GC_allochblk(lw, k, flags);
	}
#   endif
    while (0 == h && GC_collect_or_expand(n_blocks, (flags != 0))) {
	h = GC_allochblk(lw, k, flags);
    }
    if (h == 0) {
	result = 0;
    } else {
	int total_bytes = n_blocks * HBLKSIZE;
	if (n_blocks > 1) {
	    GC_large_allocd_bytes += total_bytes;
	    if (GC_large_allocd_bytes > GC_max_large_allocd_bytes)
	        GC_max_large_allocd_bytes = GC_large_allocd_bytes;
	}
	result = (ptr_t) (h -> hb_body);
	GC_words_wasted += BYTES_TO_WORDS(total_bytes) - lw;
    }
    return result;
}


/* Allocate a large block of size lb bytes.  Clear if appropriate.	*/
/* We hold the allocation lock.						*/
ptr_t GC_alloc_large_and_clear(lw, k, flags)
word lw;
int k;
unsigned flags;
{
    ptr_t result = GC_alloc_large(lw, k, flags);
    word n_blocks = OBJ_SZ_TO_BLOCKS(lw);

    if (0 == result) return 0;
    if (GC_debugging_started || GC_obj_kinds[k].ok_init) {
	/* Clear the whole block, in case of GC_realloc call. */
	BZERO(result, n_blocks * HBLKSIZE);
    }
    return result;
}

/* allocate lb bytes for an object of kind k.	*/
/* Should not be used to directly to allocate	*/
/* objects such as STUBBORN objects that	*/
/* require special handling on allocation.	*/
/* First a version that assumes we already	*/
/* hold lock:					*/
ptr_t GC_generic_malloc_inner(lb, k)
register word lb;
register int k;
{
register word lw;
register ptr_t op;
register ptr_t *opp;

    if( SMALL_OBJ(lb) ) {
        register struct obj_kind * kind = GC_obj_kinds + k;
#       ifdef MERGE_SIZES
	  lw = GC_size_map[lb];
#	else
	  lw = ALIGNED_WORDS(lb);
	  if (lw == 0) lw = MIN_WORDS;
#       endif
	opp = &(kind -> ok_freelist[lw]);
        if( (op = *opp) == 0 ) {
#	    ifdef MERGE_SIZES
	      if (GC_size_map[lb] == 0) {
	        if (!GC_is_initialized)  GC_init_inner();
	        if (GC_size_map[lb] == 0) GC_extend_size_map(lb);
	        return(GC_generic_malloc_inner(lb, k));
	      }
#	    else
	      if (!GC_is_initialized) {
	        GC_init_inner();
	        return(GC_generic_malloc_inner(lb, k));
	      }
#	    endif
	    if (kind -> ok_reclaim_list == 0) {
	    	if (!GC_alloc_reclaim_list(kind)) goto out;
	    }
	    op = GC_allocobj(lw, k);
	    if (op == 0) goto out;
        }
        /* Here everything is in a consistent state.	*/
        /* We assume the following assignment is	*/
        /* atomic.  If we get aborted			*/
        /* after the assignment, we lose an object,	*/
        /* but that's benign.				*/
        /* Volatile declarations may need to be added	*/
        /* to prevent the compiler from breaking things.*/
	/* If we only execute the second of the 	*/
	/* following assignments, we lose the free	*/
	/* list, but that should still be OK, at least	*/
	/* for garbage collected memory.		*/
        *opp = obj_link(op);
        obj_link(op) = 0;
    } else {
	lw = ROUNDED_UP_WORDS(lb);
	op = (ptr_t)GC_alloc_large_and_clear(lw, k, 0);
    }
    GC_words_allocd += lw;
    
out:
    return op;
}

/* Allocate a composite object of size n bytes.  The caller guarantees  */
/* that pointers past the first page are not relevant.  Caller holds    */
/* allocation lock.                                                     */
ptr_t GC_generic_malloc_inner_ignore_off_page(lb, k)
register size_t lb;
register int k;
{
    register word lw;
    ptr_t op;

    if (lb <= HBLKSIZE)
        return(GC_generic_malloc_inner((word)lb, k));
    lw = ROUNDED_UP_WORDS(lb);
    op = (ptr_t)GC_alloc_large_and_clear(lw, k, IGNORE_OFF_PAGE);
    GC_words_allocd += lw;
    return op;
}

ptr_t GC_generic_malloc(lb, k)
register word lb;
register int k;
{
    ptr_t result;
    DCL_LOCK_STATE;

    if (GC_have_errors) GC_print_all_errors();
    GC_INVOKE_FINALIZERS();
    if (SMALL_OBJ(lb)) {
    	DISABLE_SIGNALS();
	LOCK();
        result = GC_generic_malloc_inner((word)lb, k);
	UNLOCK();
	ENABLE_SIGNALS();
    } else {
	word lw;
	word n_blocks;
	GC_bool init;
	lw = ROUNDED_UP_WORDS(lb);
	n_blocks = OBJ_SZ_TO_BLOCKS(lw);
	init = GC_obj_kinds[k].ok_init;
	DISABLE_SIGNALS();
	LOCK();
	result = (ptr_t)GC_alloc_large(lw, k, 0);
	if (0 != result) {
	  if (GC_debugging_started) {
	    BZERO(result, n_blocks * HBLKSIZE);
	  } else {
#           ifdef THREADS
	      /* Clear any memory that might be used for GC descriptors */
	      /* before we release the lock.			      */
	        ((word *)result)[0] = 0;
	        ((word *)result)[1] = 0;
	        ((word *)result)[lw-1] = 0;
	        ((word *)result)[lw-2] = 0;
#	    endif
	  }
	}
	GC_words_allocd += lw;
	UNLOCK();
	ENABLE_SIGNALS();
    	if (init && !GC_debugging_started && 0 != result) {
	    BZERO(result, n_blocks * HBLKSIZE);
        }
    }
    if (0 == result) {
        return((*GC_oom_fn)(lb));
    } else {
        return(result);
    }
}   


#define GENERAL_MALLOC(lb,k) \
    (GC_PTR)GC_clear_stack(GC_generic_malloc((word)lb, k))
/* We make the GC_clear_stack_call a tail call, hoping to get more of	*/
/* the stack.								*/

/* Allocate lb bytes of atomic (pointerfree) data */
# ifdef __STDC__
    GC_PTR GC_malloc_atomic(size_t lb)
# else
    GC_PTR GC_malloc_atomic(lb)
    size_t lb;
# endif
{
register ptr_t op;
register ptr_t * opp;
register word lw;
DCL_LOCK_STATE;

    if( EXPECT(SMALL_OBJ(lb), 1) ) {
#       ifdef MERGE_SIZES
	  lw = GC_size_map[lb];
#	else
	  lw = ALIGNED_WORDS(lb);
#       endif
	opp = &(GC_aobjfreelist[lw]);
	FASTLOCK();
        if( EXPECT(!FASTLOCK_SUCCEEDED() || (op = *opp) == 0, 0) ) {
            FASTUNLOCK();
            return(GENERAL_MALLOC((word)lb, PTRFREE));
        }
        /* See above comment on signals.	*/
        *opp = obj_link(op);
        GC_words_allocd += lw;
        FASTUNLOCK();
        return((GC_PTR) op);
   } else {
       return(GENERAL_MALLOC((word)lb, PTRFREE));
   }
}

/* Allocate lb bytes of composite (pointerful) data */
# ifdef __STDC__
    GC_PTR GC_malloc(size_t lb)
# else
    GC_PTR GC_malloc(lb)
    size_t lb;
# endif
{
register ptr_t op;
register ptr_t *opp;
register word lw;
DCL_LOCK_STATE;

    if( EXPECT(SMALL_OBJ(lb), 1) ) {
#       ifdef MERGE_SIZES
	  lw = GC_size_map[lb];
#	else
	  lw = ALIGNED_WORDS(lb);
#       endif
	opp = &(GC_objfreelist[lw]);
	FASTLOCK();
        if( EXPECT(!FASTLOCK_SUCCEEDED() || (op = *opp) == 0, 0) ) {
            FASTUNLOCK();
            return(GENERAL_MALLOC((word)lb, NORMAL));
        }
        /* See above comment on signals.	*/
	GC_ASSERT(0 == obj_link(op)
		  || (word)obj_link(op)
		  	<= (word)GC_greatest_plausible_heap_addr
		     && (word)obj_link(op)
		     	>= (word)GC_least_plausible_heap_addr);
        *opp = obj_link(op);
        obj_link(op) = 0;
        GC_words_allocd += lw;
        FASTUNLOCK();
        return((GC_PTR) op);
   } else {
       return(GENERAL_MALLOC((word)lb, NORMAL));
   }
}

# ifdef REDIRECT_MALLOC

/* Avoid unnecessary nested procedure calls here, by #defining some	*/
/* malloc replacements.  Otherwise we end up saving a 			*/
/* meaningless return address in the object.  It also speeds things up,	*/
/* but it is admittedly quite ugly.					*/
# ifdef GC_ADD_CALLER
#   define RA GC_RETURN_ADDR,
# else
#   define RA
# endif
# define GC_debug_malloc_replacement(lb) \
	GC_debug_malloc(lb, RA "unknown", 0)

# ifdef __STDC__
    GC_PTR malloc(size_t lb)
# else
    GC_PTR malloc(lb)
    size_t lb;
# endif
  {
    /* It might help to manually inline the GC_malloc call here.	*/
    /* But any decent compiler should reduce the extra procedure call	*/
    /* to at most a jump instruction in this case.			*/
#   if defined(I386) && defined(GC_SOLARIS_THREADS)
      /*
       * Thread initialisation can call malloc before
       * we're ready for it.
       * It's not clear that this is enough to help matters.
       * The thread implementation may well call malloc at other
       * inopportune times.
       */
      if (!GC_is_initialized) return sbrk(lb);
#   endif /* I386 && GC_SOLARIS_THREADS */
    return((GC_PTR)REDIRECT_MALLOC(lb));
  }

# ifdef __STDC__
    GC_PTR calloc(size_t n, size_t lb)
# else
    GC_PTR calloc(n, lb)
    size_t n, lb;
# endif
  {
    return((GC_PTR)REDIRECT_MALLOC(n*lb));
  }

#ifndef strdup
# include <string.h>
# ifdef __STDC__
    char *strdup(const char *s)
# else
    char *strdup(s)
    char *s;
# endif
  {
    size_t len = strlen(s) + 1;
    char * result = ((char *)REDIRECT_MALLOC(len+1));
    BCOPY(s, result, len+1);
    return result;
  }
#endif /* !defined(strdup) */
 /* If strdup is macro defined, we assume that it actually calls malloc, */
 /* and thus the right thing will happen even without overriding it.	 */
 /* This seems to be true on most Linux systems.			 */

#undef GC_debug_malloc_replacement

# endif /* REDIRECT_MALLOC */

/* Explicitly deallocate an object p.				*/
# ifdef __STDC__
    void GC_free(GC_PTR p)
# else
    void GC_free(p)
    GC_PTR p;
# endif
{
    register struct hblk *h;
    register hdr *hhdr;
    register signed_word sz;
    register ptr_t * flh;
    register int knd;
    register struct obj_kind * ok;
    DCL_LOCK_STATE;

    if (p == 0) return;
    	/* Required by ANSI.  It's not my fault ...	*/
    h = HBLKPTR(p);
    hhdr = HDR(h);
    GC_ASSERT(GC_base(p) == p);
#   if defined(REDIRECT_MALLOC) && \
	(defined(GC_SOLARIS_THREADS) || defined(GC_LINUX_THREADS) \
	 || defined(__MINGW32__)) /* Should this be MSWIN32 in general? */
	/* For Solaris, we have to redirect malloc calls during		*/
	/* initialization.  For the others, this seems to happen 	*/
 	/* implicitly.							*/
	/* Don't try to deallocate that memory.				*/
	if (0 == hhdr) return;
#   endif
    knd = hhdr -> hb_obj_kind;
    sz = hhdr -> hb_sz;
    ok = &GC_obj_kinds[knd];
    if (EXPECT((sz <= MAXOBJSZ), 1)) {
#	ifdef THREADS
	    DISABLE_SIGNALS();
	    LOCK();
#	endif
	GC_mem_freed += sz;
	/* A signal here can make GC_mem_freed and GC_non_gc_bytes	*/
	/* inconsistent.  We claim this is benign.			*/
	if (IS_UNCOLLECTABLE(knd)) GC_non_gc_bytes -= WORDS_TO_BYTES(sz);
		/* Its unnecessary to clear the mark bit.  If the 	*/
		/* object is reallocated, it doesn't matter.  O.w. the	*/
		/* collector will do it, since it's on a free list.	*/
	if (ok -> ok_init) {
	    BZERO((word *)p + 1, WORDS_TO_BYTES(sz-1));
	}
	flh = &(ok -> ok_freelist[sz]);
	obj_link(p) = *flh;
	*flh = (ptr_t)p;
#	ifdef THREADS
	    UNLOCK();
	    ENABLE_SIGNALS();
#	endif
    } else {
    	DISABLE_SIGNALS();
        LOCK();
        GC_mem_freed += sz;
	if (IS_UNCOLLECTABLE(knd)) GC_non_gc_bytes -= WORDS_TO_BYTES(sz);
        GC_freehblk(h);
        UNLOCK();
        ENABLE_SIGNALS();
    }
}

/* Explicitly deallocate an object p when we already hold lock.		*/
/* Only used for internally allocated objects, so we can take some 	*/
/* shortcuts.								*/
#ifdef THREADS
void GC_free_inner(GC_PTR p)
{
    register struct hblk *h;
    register hdr *hhdr;
    register signed_word sz;
    register ptr_t * flh;
    register int knd;
    register struct obj_kind * ok;
    DCL_LOCK_STATE;

    h = HBLKPTR(p);
    hhdr = HDR(h);
    knd = hhdr -> hb_obj_kind;
    sz = hhdr -> hb_sz;
    ok = &GC_obj_kinds[knd];
    if (sz <= MAXOBJSZ) {
	GC_mem_freed += sz;
	if (IS_UNCOLLECTABLE(knd)) GC_non_gc_bytes -= WORDS_TO_BYTES(sz);
	if (ok -> ok_init) {
	    BZERO((word *)p + 1, WORDS_TO_BYTES(sz-1));
	}
	flh = &(ok -> ok_freelist[sz]);
	obj_link(p) = *flh;
	*flh = (ptr_t)p;
    } else {
        GC_mem_freed += sz;
	if (IS_UNCOLLECTABLE(knd)) GC_non_gc_bytes -= WORDS_TO_BYTES(sz);
        GC_freehblk(h);
    }
}
#endif /* THREADS */

# if defined(REDIRECT_MALLOC) && !defined(REDIRECT_FREE)
#   define REDIRECT_FREE GC_free
# endif
# ifdef REDIRECT_FREE
#   ifdef __STDC__
      void free(GC_PTR p)
#   else
      void free(p)
      GC_PTR p;
#   endif
  {
#   ifndef IGNORE_FREE
      REDIRECT_FREE(p);
#   endif
  }
# endif  /* REDIRECT_MALLOC */
