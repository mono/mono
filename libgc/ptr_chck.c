/* 
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
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
 * These are checking routines calls to which could be inserted by a
 * preprocessor to validate C pointer arithmetic.
 */

#include "private/gc_pmark.h"

#ifdef __STDC__
void GC_default_same_obj_print_proc(GC_PTR p, GC_PTR q)
#else
void GC_default_same_obj_print_proc (p, q)
GC_PTR p, q;
#endif
{
    GC_err_printf2("0x%lx and 0x%lx are not in the same object\n",
    		   (unsigned long)p, (unsigned long)q);
    ABORT("GC_same_obj test failed");
}

void (*GC_same_obj_print_proc) GC_PROTO((GC_PTR, GC_PTR))
		= GC_default_same_obj_print_proc;

/* Check that p and q point to the same object.  Call		*/
/* *GC_same_obj_print_proc if they don't.			*/
/* Returns the first argument.  (Return value may be hard 	*/
/* to use,due to typing issues.  But if we had a suitable 	*/
/* preprocessor ...)						*/
/* Succeeds if neither p nor q points to the heap.		*/
/* We assume this is performance critical.  (It shouldn't	*/
/* be called by production code, but this can easily make	*/
/* debugging intolerably slow.)					*/
#ifdef __STDC__
  GC_PTR GC_same_obj(register void *p, register void *q)
#else
  GC_PTR GC_same_obj(p, q)
  register char *p, *q;
#endif
{
    register struct hblk *h;
    register hdr *hhdr;
    register ptr_t base, limit;
    register word sz;
    
    if (!GC_is_initialized) GC_init();
    hhdr = HDR((word)p);
    if (hhdr == 0) {
   	if (divHBLKSZ((word)p) != divHBLKSZ((word)q)
   	    && HDR((word)q) != 0) {
   	    goto fail;
   	}
   	return(p);
    }
    /* If it's a pointer to the middle of a large object, move it	*/
    /* to the beginning.						*/
    if (IS_FORWARDING_ADDR_OR_NIL(hhdr)) {
    	h = HBLKPTR(p) - (word)hhdr;
    	hhdr = HDR(h);
	while (IS_FORWARDING_ADDR_OR_NIL(hhdr)) {
	   h = FORWARDED_ADDR(h, hhdr);
	   hhdr = HDR(h);
	}
	limit = (ptr_t)((word *)h + hhdr -> hb_sz);
	if ((ptr_t)p >= limit || (ptr_t)q >= limit || (ptr_t)q < (ptr_t)h ) {
	    goto fail;
	}
	return(p);
    }
    sz = WORDS_TO_BYTES(hhdr -> hb_sz);
    if (sz > MAXOBJBYTES) {
      base = (ptr_t)HBLKPTR(p);
      limit = base + sz;
      if ((ptr_t)p >= limit) {
        goto fail;
      }
    } else {
      register int map_entry;
      register int pdispl = HBLKDISPL(p);
      
      map_entry = MAP_ENTRY((hhdr -> hb_map), pdispl);
      if (map_entry > CPP_MAX_OFFSET) {
         map_entry = BYTES_TO_WORDS(pdispl) % BYTES_TO_WORDS(sz);
	 if (HBLKPTR(p) != HBLKPTR(q)) goto fail;
	 	/* W/o this check, we might miss an error if 	*/
	 	/* q points to the first object on a page, and	*/
	 	/* points just before the page.			*/
      }
      base = (char *)((word)p & ~(WORDS_TO_BYTES(1) - 1));
      base -= WORDS_TO_BYTES(map_entry);
      limit = base + sz;
    }
    /* [base, limit) delimits the object containing p, if any.	*/
    /* If p is not inside a valid object, then either q is	*/
    /* also outside any valid object, or it is outside 		*/
    /* [base, limit).						*/
    if ((ptr_t)q >= limit || (ptr_t)q < base) {
    	goto fail;
    }
    return(p);
fail:
    (*GC_same_obj_print_proc)((ptr_t)p, (ptr_t)q);
    return(p);
}

#ifdef __STDC__
void GC_default_is_valid_displacement_print_proc (GC_PTR p)
#else
void GC_default_is_valid_displacement_print_proc (p)
GC_PTR p;
#endif
{
    GC_err_printf1("0x%lx does not point to valid object displacement\n",
    		   (unsigned long)p);
    ABORT("GC_is_valid_displacement test failed");
}

void (*GC_is_valid_displacement_print_proc) GC_PROTO((GC_PTR)) = 
	GC_default_is_valid_displacement_print_proc;

/* Check that if p is a pointer to a heap page, then it points to	*/
/* a valid displacement within a heap object.				*/
/* Uninteresting with GC_all_interior_pointers.				*/
/* Always returns its argument.						*/
/* Note that we don't lock, since nothing relevant about the header	*/
/* should change while we have a valid object pointer to the block.	*/
#ifdef __STDC__
  void * GC_is_valid_displacement(void *p)
#else
  char *GC_is_valid_displacement(p)
  char *p;
#endif
{
    register hdr *hhdr;
    register word pdispl;
    register struct hblk *h;
    register map_entry_type map_entry;
    register word sz;
    
    if (!GC_is_initialized) GC_init();
    hhdr = HDR((word)p);
    if (hhdr == 0) return(p);
    h = HBLKPTR(p);
    if (GC_all_interior_pointers) {
	while (IS_FORWARDING_ADDR_OR_NIL(hhdr)) {
	   h = FORWARDED_ADDR(h, hhdr);
	   hhdr = HDR(h);
	}
    }
    if (IS_FORWARDING_ADDR_OR_NIL(hhdr)) {
    	goto fail;
    }
    sz = WORDS_TO_BYTES(hhdr -> hb_sz);
    pdispl = HBLKDISPL(p);
    map_entry = MAP_ENTRY((hhdr -> hb_map), pdispl);
    if (map_entry == OBJ_INVALID
    	|| sz > MAXOBJBYTES && (ptr_t)p >= (ptr_t)h + sz) {
    	goto fail;
    }
    return(p);
fail:
    (*GC_is_valid_displacement_print_proc)((ptr_t)p);
    return(p);
}

#ifdef __STDC__
void GC_default_is_visible_print_proc(GC_PTR p)
#else
void GC_default_is_visible_print_proc(p)
GC_PTR p;
#endif
{
    GC_err_printf1("0x%lx is not a GC visible pointer location\n",
    		   (unsigned long)p);
    ABORT("GC_is_visible test failed");
}

void (*GC_is_visible_print_proc) GC_PROTO((GC_PTR p)) = 
	GC_default_is_visible_print_proc;

/* Could p be a stack address? */
GC_bool GC_on_stack(p)
ptr_t p;
{
#   ifdef THREADS
	return(TRUE);
#   else
	int dummy;
#   	ifdef STACK_GROWS_DOWN
	    if ((ptr_t)p >= (ptr_t)(&dummy) && (ptr_t)p < GC_stackbottom ) {
	    	return(TRUE);
	    }
#	else
	    if ((ptr_t)p <= (ptr_t)(&dummy) && (ptr_t)p > GC_stackbottom ) {
	    	return(TRUE);
	    }
#	endif
	return(FALSE);
#   endif
}

/* Check that p is visible						*/
/* to the collector as a possibly pointer containing location.		*/
/* If it isn't invoke *GC_is_visible_print_proc.			*/
/* Returns the argument in all cases.  May erroneously succeed		*/
/* in hard cases.  (This is intended for debugging use with		*/
/* untyped allocations.  The idea is that it should be possible, though	*/
/* slow, to add such a call to all indirect pointer stores.)		*/
/* Currently useless for multithreaded worlds.				*/
#ifdef __STDC__
  void * GC_is_visible(void *p)
#else
  char *GC_is_visible(p)
  char *p;
#endif
{
    register hdr *hhdr;
    
    if ((word)p & (ALIGNMENT - 1)) goto fail;
    if (!GC_is_initialized) GC_init();
#   ifdef THREADS
	hhdr = HDR((word)p);
        if (hhdr != 0 && GC_base(p) == 0) {
            goto fail;
        } else {
            /* May be inside thread stack.  We can't do much. */
            return(p);
        }
#   else
	/* Check stack first: */
	  if (GC_on_stack(p)) return(p);
	hhdr = HDR((word)p);
    	if (hhdr == 0) {
    	    GC_bool result;
    	    
    	    if (GC_is_static_root(p)) return(p);
    	    /* Else do it again correctly:	*/
#           if (defined(DYNAMIC_LOADING) || defined(MSWIN32) || \
		defined(MSWINCE) || defined(PCR)) \
                && !defined(SRC_M3)
    	        DISABLE_SIGNALS();
    	        GC_register_dynamic_libraries();
    	        result = GC_is_static_root(p);
    	        ENABLE_SIGNALS();
    	        if (result) return(p);
#	    endif
    	    goto fail;
    	} else {
    	    /* p points to the heap. */
    	    word descr;
    	    ptr_t base = GC_base(p);	/* Should be manually inlined? */
    	    
    	    if (base == 0) goto fail;
    	    if (HBLKPTR(base) != HBLKPTR(p)) hhdr = HDR((word)p);
    	    descr = hhdr -> hb_descr;
    retry:
    	    switch(descr & GC_DS_TAGS) {
    	        case GC_DS_LENGTH:
    	            if ((word)((ptr_t)p - (ptr_t)base) > (word)descr) goto fail;
    	            break;
    	        case GC_DS_BITMAP:
    	            if ((ptr_t)p - (ptr_t)base
    	                 >= WORDS_TO_BYTES(BITMAP_BITS)
    	                 || ((word)p & (sizeof(word) - 1))) goto fail;
    	            if (!((1 << (WORDSZ - ((ptr_t)p - (ptr_t)base) - 1))
    	            	  & descr)) goto fail;
    	            break;
    	        case GC_DS_PROC:
    	            /* We could try to decipher this partially. 	*/
    	            /* For now we just punt.				*/
    	            break;
    	        case GC_DS_PER_OBJECT:
		    if ((signed_word)descr >= 0) {
    	              descr = *(word *)((ptr_t)base + (descr & ~GC_DS_TAGS));
		    } else {
		      ptr_t type_descr = *(ptr_t *)base;
		      descr = *(word *)(type_descr
			      - (descr - (GC_DS_PER_OBJECT
					  - GC_INDIR_PER_OBJ_BIAS)));
		    }
    	            goto retry;
    	    }
    	    return(p);
    	}
#   endif
fail:
    (*GC_is_visible_print_proc)((ptr_t)p);
    return(p);
}


GC_PTR GC_pre_incr (p, how_much)
GC_PTR *p;
size_t how_much;
{
    GC_PTR initial = *p;
    GC_PTR result = GC_same_obj((GC_PTR)((word)initial + how_much), initial);
    
    if (!GC_all_interior_pointers) {
    	(void) GC_is_valid_displacement(result);
    }
    return (*p = result);
}

GC_PTR GC_post_incr (p, how_much)
GC_PTR *p;
size_t how_much;
{
    GC_PTR initial = *p;
    GC_PTR result = GC_same_obj((GC_PTR)((word)initial + how_much), initial);
 
    if (!GC_all_interior_pointers) {
    	(void) GC_is_valid_displacement(result);
    }
    *p = result;
    return(initial);
}
