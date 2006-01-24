
/*
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1995 by Xerox Corporation.  All rights reserved.
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
 *
 */


# include <stdio.h>
# include "private/gc_pmark.h"

#if defined(MSWIN32) && defined(__GNUC__)
# include <excpt.h>
#endif

/* We put this here to minimize the risk of inlining. */
/*VARARGS*/
#ifdef __WATCOMC__
  void GC_noop(void *p, ...) {}
#else
  void GC_noop() {}
#endif

/* Single argument version, robust against whole program analysis. */
void GC_noop1(x)
word x;
{
    static VOLATILE word sink;

    sink = x;
}

/* mark_proc GC_mark_procs[MAX_MARK_PROCS] = {0} -- declared in gc_priv.h */

word GC_n_mark_procs = GC_RESERVED_MARK_PROCS;

/* Initialize GC_obj_kinds properly and standard free lists properly.  	*/
/* This must be done statically since they may be accessed before 	*/
/* GC_init is called.							*/
/* It's done here, since we need to deal with mark descriptors.		*/
struct obj_kind GC_obj_kinds[MAXOBJKINDS] = {
/* PTRFREE */ { &GC_aobjfreelist[0], 0 /* filled in dynamically */,
		0 | GC_DS_LENGTH, FALSE, FALSE },
/* NORMAL  */ { &GC_objfreelist[0], 0,
		0 | GC_DS_LENGTH,  /* Adjusted in GC_init_inner for EXTRA_BYTES */
		TRUE /* add length to descr */, TRUE },
/* UNCOLLECTABLE */
	      { &GC_uobjfreelist[0], 0,
		0 | GC_DS_LENGTH, TRUE /* add length to descr */, TRUE },
# ifdef ATOMIC_UNCOLLECTABLE
   /* AUNCOLLECTABLE */
	      { &GC_auobjfreelist[0], 0,
		0 | GC_DS_LENGTH, FALSE /* add length to descr */, FALSE },
# endif
# ifdef STUBBORN_ALLOC
/*STUBBORN*/ { &GC_sobjfreelist[0], 0,
		0 | GC_DS_LENGTH, TRUE /* add length to descr */, TRUE },
# endif
};

# ifdef ATOMIC_UNCOLLECTABLE
#   ifdef STUBBORN_ALLOC
      int GC_n_kinds = 5;
#   else
      int GC_n_kinds = 4;
#   endif
# else
#   ifdef STUBBORN_ALLOC
      int GC_n_kinds = 4;
#   else
      int GC_n_kinds = 3;
#   endif
# endif


# ifndef INITIAL_MARK_STACK_SIZE
#   define INITIAL_MARK_STACK_SIZE (1*HBLKSIZE)
		/* INITIAL_MARK_STACK_SIZE * sizeof(mse) should be a 	*/
		/* multiple of HBLKSIZE.				*/
		/* The incremental collector actually likes a larger	*/
		/* size, since it want to push all marked dirty objs	*/
		/* before marking anything new.  Currently we let it	*/
		/* grow dynamically.					*/
# endif

/*
 * Limits of stack for GC_mark routine.
 * All ranges between GC_mark_stack(incl.) and GC_mark_stack_top(incl.) still
 * need to be marked from.
 */

word GC_n_rescuing_pages;	/* Number of dirty pages we marked from */
				/* excludes ptrfree pages, etc.		*/

mse * GC_mark_stack;

mse * GC_mark_stack_limit;

word GC_mark_stack_size = 0;
 
#ifdef PARALLEL_MARK
  mse * VOLATILE GC_mark_stack_top;
#else
  mse * GC_mark_stack_top;
#endif

static struct hblk * scan_ptr;

mark_state_t GC_mark_state = MS_NONE;

GC_bool GC_mark_stack_too_small = FALSE;

GC_bool GC_objects_are_marked = FALSE;	/* Are there collectable marked	*/
					/* objects in the heap?		*/

/* Is a collection in progress?  Note that this can return true in the	*/
/* nonincremental case, if a collection has been abandoned and the	*/
/* mark state is now MS_INVALID.					*/
GC_bool GC_collection_in_progress()
{
    return(GC_mark_state != MS_NONE);
}

/* clear all mark bits in the header */
void GC_clear_hdr_marks(hhdr)
register hdr * hhdr;
{
#   ifdef USE_MARK_BYTES
      BZERO(hhdr -> hb_marks, MARK_BITS_SZ);
#   else
      BZERO(hhdr -> hb_marks, MARK_BITS_SZ*sizeof(word));
#   endif
}

/* Set all mark bits in the header.  Used for uncollectable blocks. */
void GC_set_hdr_marks(hhdr)
register hdr * hhdr;
{
    register int i;

    for (i = 0; i < MARK_BITS_SZ; ++i) {
#     ifdef USE_MARK_BYTES
    	hhdr -> hb_marks[i] = 1;
#     else
    	hhdr -> hb_marks[i] = ONES;
#     endif
    }
}

/*
 * Clear all mark bits associated with block h.
 */
/*ARGSUSED*/
# if defined(__STDC__) || defined(__cplusplus)
    static void clear_marks_for_block(struct hblk *h, word dummy)
# else
    static void clear_marks_for_block(h, dummy)
    struct hblk *h;
    word dummy;
# endif
{
    register hdr * hhdr = HDR(h);
    
    if (IS_UNCOLLECTABLE(hhdr -> hb_obj_kind)) return;
        /* Mark bit for these is cleared only once the object is 	*/
        /* explicitly deallocated.  This either frees the block, or	*/
        /* the bit is cleared once the object is on the free list.	*/
    GC_clear_hdr_marks(hhdr);
}

/* Slow but general routines for setting/clearing/asking about mark bits */
void GC_set_mark_bit(p)
ptr_t p;
{
    register struct hblk *h = HBLKPTR(p);
    register hdr * hhdr = HDR(h);
    register int word_no = (word *)p - (word *)h;
    
    set_mark_bit_from_hdr(hhdr, word_no);
}

void GC_clear_mark_bit(p)
ptr_t p;
{
    register struct hblk *h = HBLKPTR(p);
    register hdr * hhdr = HDR(h);
    register int word_no = (word *)p - (word *)h;
    
    clear_mark_bit_from_hdr(hhdr, word_no);
}

GC_bool GC_is_marked(p)
ptr_t p;
{
    register struct hblk *h = HBLKPTR(p);
    register hdr * hhdr = HDR(h);
    register int word_no = (word *)p - (word *)h;
    
    return(mark_bit_from_hdr(hhdr, word_no));
}


/*
 * Clear mark bits in all allocated heap blocks.  This invalidates
 * the marker invariant, and sets GC_mark_state to reflect this.
 * (This implicitly starts marking to reestablish the invariant.)
 */
void GC_clear_marks()
{
    GC_apply_to_all_blocks(clear_marks_for_block, (word)0);
    GC_objects_are_marked = FALSE;
    GC_mark_state = MS_INVALID;
    scan_ptr = 0;
#   ifdef GATHERSTATS
	/* Counters reflect currently marked objects: reset here */
        GC_composite_in_use = 0;
        GC_atomic_in_use = 0;
#   endif

}

/* Initiate a garbage collection.  Initiates a full collection if the	*/
/* mark	state is invalid.						*/
/*ARGSUSED*/
void GC_initiate_gc()
{
    if (GC_dirty_maintained) GC_read_dirty();
#   ifdef STUBBORN_ALLOC
    	GC_read_changed();
#   endif
#   ifdef CHECKSUMS
	{
	    extern void GC_check_dirty();
	    
	    if (GC_dirty_maintained) GC_check_dirty();
	}
#   endif
    GC_n_rescuing_pages = 0;
    if (GC_mark_state == MS_NONE) {
        GC_mark_state = MS_PUSH_RESCUERS;
    } else if (GC_mark_state != MS_INVALID) {
    	ABORT("unexpected state");
    } /* else this is really a full collection, and mark	*/
      /* bits are invalid.					*/
    scan_ptr = 0;
}


static void alloc_mark_stack();

/* Perform a small amount of marking.			*/
/* We try to touch roughly a page of memory.		*/
/* Return TRUE if we just finished a mark phase.	*/
/* Cold_gc_frame is an address inside a GC frame that	*/
/* remains valid until all marking is complete.		*/
/* A zero value indicates that it's OK to miss some	*/
/* register values.					*/
/* We hold the allocation lock.  In the case of 	*/
/* incremental collection, the world may not be stopped.*/
#ifdef MSWIN32
  /* For win32, this is called after we establish a structured	*/
  /* exception handler, in case Windows unmaps one of our root	*/
  /* segments.  See below.  In either case, we acquire the 	*/
  /* allocator lock long before we get here.			*/
  GC_bool GC_mark_some_inner(cold_gc_frame)
  ptr_t cold_gc_frame;
#else
  GC_bool GC_mark_some(cold_gc_frame)
  ptr_t cold_gc_frame;
#endif
{
    switch(GC_mark_state) {
    	case MS_NONE:
    	    return(FALSE);
    	    
    	case MS_PUSH_RESCUERS:
    	    if (GC_mark_stack_top
    	        >= GC_mark_stack_limit - INITIAL_MARK_STACK_SIZE/2) {
		/* Go ahead and mark, even though that might cause us to */
		/* see more marked dirty objects later on.  Avoid this	 */
		/* in the future.					 */
		GC_mark_stack_too_small = TRUE;
    	        MARK_FROM_MARK_STACK();
    	        return(FALSE);
    	    } else {
    	        scan_ptr = GC_push_next_marked_dirty(scan_ptr);
    	        if (scan_ptr == 0) {
#		    ifdef CONDPRINT
		      if (GC_print_stats) {
			GC_printf1("Marked from %lu dirty pages\n",
				   (unsigned long)GC_n_rescuing_pages);
		      }
#		    endif
    	    	    GC_push_roots(FALSE, cold_gc_frame);
    	    	    GC_objects_are_marked = TRUE;
    	    	    if (GC_mark_state != MS_INVALID) {
    	    	        GC_mark_state = MS_ROOTS_PUSHED;
    	    	    }
    	    	}
    	    }
    	    return(FALSE);
    	
    	case MS_PUSH_UNCOLLECTABLE:
    	    if (GC_mark_stack_top
    	        >= GC_mark_stack + GC_mark_stack_size/4) {
#		ifdef PARALLEL_MARK
		  /* Avoid this, since we don't parallelize the marker	*/
		  /* here.						*/
		  if (GC_parallel) GC_mark_stack_too_small = TRUE;
#		endif
    	        MARK_FROM_MARK_STACK();
    	        return(FALSE);
    	    } else {
    	        scan_ptr = GC_push_next_marked_uncollectable(scan_ptr);
    	        if (scan_ptr == 0) {
    	    	    GC_push_roots(TRUE, cold_gc_frame);
    	    	    GC_objects_are_marked = TRUE;
    	    	    if (GC_mark_state != MS_INVALID) {
    	    	        GC_mark_state = MS_ROOTS_PUSHED;
    	    	    }
    	    	}
    	    }
    	    return(FALSE);
    	
    	case MS_ROOTS_PUSHED:
#	    ifdef PARALLEL_MARK
	      /* In the incremental GC case, this currently doesn't	*/
	      /* quite do the right thing, since it runs to		*/
	      /* completion.  On the other hand, starting a		*/
	      /* parallel marker is expensive, so perhaps it is		*/
	      /* the right thing?					*/
	      /* Eventually, incremental marking should run		*/
	      /* asynchronously in multiple threads, without grabbing	*/
	      /* the allocation lock.					*/
	        if (GC_parallel) {
		  GC_do_parallel_mark();
		  GC_ASSERT(GC_mark_stack_top < GC_first_nonempty);
		  GC_mark_stack_top = GC_mark_stack - 1;
    	          if (GC_mark_stack_too_small) {
    	            alloc_mark_stack(2*GC_mark_stack_size);
    	          }
		  if (GC_mark_state == MS_ROOTS_PUSHED) {
    	            GC_mark_state = MS_NONE;
    	            return(TRUE);
		  } else {
		    return(FALSE);
	          }
		}
#	    endif
    	    if (GC_mark_stack_top >= GC_mark_stack) {
    	        MARK_FROM_MARK_STACK();
    	        return(FALSE);
    	    } else {
    	        GC_mark_state = MS_NONE;
    	        if (GC_mark_stack_too_small) {
    	            alloc_mark_stack(2*GC_mark_stack_size);
    	        }
    	        return(TRUE);
    	    }
    	    
    	case MS_INVALID:
    	case MS_PARTIALLY_INVALID:
	    if (!GC_objects_are_marked) {
		GC_mark_state = MS_PUSH_UNCOLLECTABLE;
		return(FALSE);
	    }
    	    if (GC_mark_stack_top >= GC_mark_stack) {
    	        MARK_FROM_MARK_STACK();
    	        return(FALSE);
    	    }
    	    if (scan_ptr == 0 && GC_mark_state == MS_INVALID) {
		/* About to start a heap scan for marked objects. */
		/* Mark stack is empty.  OK to reallocate.	  */
		if (GC_mark_stack_too_small) {
    	            alloc_mark_stack(2*GC_mark_stack_size);
		}
		GC_mark_state = MS_PARTIALLY_INVALID;
    	    }
    	    scan_ptr = GC_push_next_marked(scan_ptr);
    	    if (scan_ptr == 0 && GC_mark_state == MS_PARTIALLY_INVALID) {
    	    	GC_push_roots(TRUE, cold_gc_frame);
    	    	GC_objects_are_marked = TRUE;
    	    	if (GC_mark_state != MS_INVALID) {
    	    	    GC_mark_state = MS_ROOTS_PUSHED;
    	    	}
    	    }
    	    return(FALSE);
    	default:
    	    ABORT("GC_mark_some: bad state");
    	    return(FALSE);
    }
}


#ifdef MSWIN32

# ifdef __GNUC__

    typedef struct {
      EXCEPTION_REGISTRATION ex_reg;
      void *alt_path;
    } ext_ex_regn;


    static EXCEPTION_DISPOSITION mark_ex_handler(
        struct _EXCEPTION_RECORD *ex_rec, 
        void *est_frame,
        struct _CONTEXT *context,
        void *disp_ctxt)
    {
        if (ex_rec->ExceptionCode == STATUS_ACCESS_VIOLATION) {
          ext_ex_regn *xer = (ext_ex_regn *)est_frame;

          /* Unwind from the inner function assuming the standard */
          /* function prologue.                                   */
          /* Assumes code has not been compiled with              */
          /* -fomit-frame-pointer.                                */
          context->Esp = context->Ebp;
          context->Ebp = *((DWORD *)context->Esp);
          context->Esp = context->Esp - 8;

          /* Resume execution at the "real" handler within the    */
          /* wrapper function.                                    */
          context->Eip = (DWORD )(xer->alt_path);

          return ExceptionContinueExecution;

        } else {
            return ExceptionContinueSearch;
        }
    }
# endif /* __GNUC__ */


  GC_bool GC_mark_some(cold_gc_frame)
  ptr_t cold_gc_frame;
  {
      GC_bool ret_val;

#   ifndef __GNUC__
      /* Windows 98 appears to asynchronously create and remove  */
      /* writable memory mappings, for reasons we haven't yet    */
      /* understood.  Since we look for writable regions to      */
      /* determine the root set, we may try to mark from an      */
      /* address range that disappeared since we started the     */
      /* collection.  Thus we have to recover from faults here.  */
      /* This code does not appear to be necessary for Windows   */
      /* 95/NT/2000. Note that this code should never generate   */
      /* an incremental GC write fault.                          */

      __try {

#   else /* __GNUC__ */

      /* Manually install an exception handler since GCC does    */
      /* not yet support Structured Exception Handling (SEH) on  */
      /* Win32.                                                  */

      ext_ex_regn er;

      er.alt_path = &&handle_ex;
      er.ex_reg.handler = mark_ex_handler;
      asm volatile ("movl %%fs:0, %0" : "=r" (er.ex_reg.prev));
      asm volatile ("movl %0, %%fs:0" : : "r" (&er));

#   endif /* __GNUC__ */

          ret_val = GC_mark_some_inner(cold_gc_frame);

#   ifndef __GNUC__

      } __except (GetExceptionCode() == EXCEPTION_ACCESS_VIOLATION ?
                EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH) {

#   else /* __GNUC__ */

          /* Prevent GCC from considering the following code unreachable */
          /* and thus eliminating it.                                    */
          if (er.alt_path != 0)
              goto rm_handler;

handle_ex:
          /* Execution resumes from here on an access violation. */

#   endif /* __GNUC__ */

#         ifdef CONDPRINT
            if (GC_print_stats) {
	      GC_printf0("Caught ACCESS_VIOLATION in marker. "
		         "Memory mapping disappeared.\n");
            }
#         endif /* CONDPRINT */

          /* We have bad roots on the stack.  Discard mark stack.  */
          /* Rescan from marked objects.  Redetermine roots.	 */
          GC_invalidate_mark_state();	
          scan_ptr = 0;

          ret_val = FALSE;

#   ifndef __GNUC__

      }

#   else /* __GNUC__ */

rm_handler:
      /* Uninstall the exception handler */
      asm volatile ("mov %0, %%fs:0" : : "r" (er.ex_reg.prev));

#   endif /* __GNUC__ */

      return ret_val;
  }
#endif /* MSWIN32 */


GC_bool GC_mark_stack_empty()
{
    return(GC_mark_stack_top < GC_mark_stack);
}	

#ifdef PROF_MARKER
    word GC_prof_array[10];
#   define PROF(n) GC_prof_array[n]++
#else
#   define PROF(n)
#endif

/* Given a pointer to someplace other than a small object page or the	*/
/* first page of a large object, either:				*/
/*	- return a pointer to somewhere in the first page of the large	*/
/*	  object, if current points to a large object.			*/
/*	  In this case *hhdr is replaced with a pointer to the header	*/
/*	  for the large object.						*/
/*	- just return current if it does not point to a large object.	*/
/*ARGSUSED*/
ptr_t GC_find_start(current, hhdr, new_hdr_p)
register ptr_t current;
register hdr *hhdr, **new_hdr_p;
{
    if (GC_all_interior_pointers) {
	if (hhdr != 0) {
	    register ptr_t orig = current;
	    
	    current = (ptr_t)HBLKPTR(current);
	    do {
	      current = current - HBLKSIZE*(word)hhdr;
	      hhdr = HDR(current);
	    } while(IS_FORWARDING_ADDR_OR_NIL(hhdr));
	    /* current points to near the start of the large object */
	    if (hhdr -> hb_flags & IGNORE_OFF_PAGE) return(orig);
	    if ((word *)orig - (word *)current
	         >= (ptrdiff_t)(hhdr->hb_sz)) {
	        /* Pointer past the end of the block */
	        return(orig);
	    }
	    *new_hdr_p = hhdr;
	    return(current);
	} else {
	    return(current);
        }
    } else {
        return(current);
    }
}

void GC_invalidate_mark_state()
{
    GC_mark_state = MS_INVALID;
    GC_mark_stack_top = GC_mark_stack-1;
}

mse * GC_signal_mark_stack_overflow(msp)
mse * msp;
{
    GC_mark_state = MS_INVALID;
    GC_mark_stack_too_small = TRUE;
#   ifdef CONDPRINT
      if (GC_print_stats) {
	GC_printf1("Mark stack overflow; current size = %lu entries\n",
	    	    GC_mark_stack_size);
      }
#   endif
    return(msp - GC_MARK_STACK_DISCARDS);
}

/*
 * Mark objects pointed to by the regions described by
 * mark stack entries between GC_mark_stack and GC_mark_stack_top,
 * inclusive.  Assumes the upper limit of a mark stack entry
 * is never 0.  A mark stack entry never has size 0.
 * We try to traverse on the order of a hblk of memory before we return.
 * Caller is responsible for calling this until the mark stack is empty.
 * Note that this is the most performance critical routine in the
 * collector.  Hence it contains all sorts of ugly hacks to speed
 * things up.  In particular, we avoid procedure calls on the common
 * path, we take advantage of peculiarities of the mark descriptor
 * encoding, we optionally maintain a cache for the block address to
 * header mapping, we prefetch when an object is "grayed", etc. 
 */
mse * GC_mark_from(mark_stack_top, mark_stack, mark_stack_limit)
mse * mark_stack_top;
mse * mark_stack;
mse * mark_stack_limit;
{
  int credit = HBLKSIZE;	/* Remaining credit for marking work	*/
  register word * current_p;	/* Pointer to current candidate ptr.	*/
  register word current;	/* Candidate pointer.			*/
  register word * limit;	/* (Incl) limit of current candidate 	*/
  				/* range				*/
  register word descr;
  register ptr_t greatest_ha = GC_greatest_plausible_heap_addr;
  register ptr_t least_ha = GC_least_plausible_heap_addr;
  DECLARE_HDR_CACHE;

# define SPLIT_RANGE_WORDS 128  /* Must be power of 2.		*/

  GC_objects_are_marked = TRUE;
  INIT_HDR_CACHE;
# ifdef OS2 /* Use untweaked version to circumvent compiler problem */
  while (mark_stack_top >= mark_stack && credit >= 0) {
# else
  while ((((ptr_t)mark_stack_top - (ptr_t)mark_stack) | credit)
  	>= 0) {
# endif
    current_p = mark_stack_top -> mse_start;
    descr = mark_stack_top -> mse_descr;
  retry:
    /* current_p and descr describe the current object.		*/
    /* *mark_stack_top is vacant.				*/
    /* The following is 0 only for small objects described by a simple	*/
    /* length descriptor.  For many applications this is the common	*/
    /* case, so we try to detect it quickly.				*/
    if (descr & ((~(WORDS_TO_BYTES(SPLIT_RANGE_WORDS) - 1)) | GC_DS_TAGS)) {
      word tag = descr & GC_DS_TAGS;
      
      switch(tag) {
        case GC_DS_LENGTH:
          /* Large length.					        */
          /* Process part of the range to avoid pushing too much on the	*/
          /* stack.							*/
	  GC_ASSERT(descr < (word)GC_greatest_plausible_heap_addr
			    - (word)GC_least_plausible_heap_addr);
#	  ifdef PARALLEL_MARK
#	    define SHARE_BYTES 2048
	    if (descr > SHARE_BYTES && GC_parallel
		&& mark_stack_top < mark_stack_limit - 1) {
	      int new_size = (descr/2) & ~(sizeof(word)-1);
	      mark_stack_top -> mse_start = current_p;
	      mark_stack_top -> mse_descr = new_size + sizeof(word);
					/* makes sure we handle 	*/
					/* misaligned pointers.		*/
	      mark_stack_top++;
	      current_p = (word *) ((char *)current_p + new_size);
	      descr -= new_size;
	      goto retry;
	    }
#	  endif /* PARALLEL_MARK */
          mark_stack_top -> mse_start =
         	limit = current_p + SPLIT_RANGE_WORDS-1;
          mark_stack_top -> mse_descr =
          		descr - WORDS_TO_BYTES(SPLIT_RANGE_WORDS-1);
          /* Make sure that pointers overlapping the two ranges are	*/
          /* considered. 						*/
          limit = (word *)((char *)limit + sizeof(word) - ALIGNMENT);
          break;
        case GC_DS_BITMAP:
          mark_stack_top--;
          descr &= ~GC_DS_TAGS;
          credit -= WORDS_TO_BYTES(WORDSZ/2); /* guess */
          while (descr != 0) {
            if ((signed_word)descr < 0) {
              current = *current_p;
	      FIXUP_POINTER(current);
	      if ((ptr_t)current >= least_ha && (ptr_t)current < greatest_ha) {
		PREFETCH((ptr_t)current);
                HC_PUSH_CONTENTS((ptr_t)current, mark_stack_top,
			      mark_stack_limit, current_p, exit1);
	      }
            }
	    descr <<= 1;
	    ++ current_p;
          }
          continue;
        case GC_DS_PROC:
          mark_stack_top--;
          credit -= GC_PROC_BYTES;
          mark_stack_top =
              (*PROC(descr))
              	    (current_p, mark_stack_top,
              	    mark_stack_limit, ENV(descr));
          continue;
        case GC_DS_PER_OBJECT:
	  if ((signed_word)descr >= 0) {
	    /* Descriptor is in the object.	*/
            descr = *(word *)((ptr_t)current_p + descr - GC_DS_PER_OBJECT);
	  } else {
	    /* Descriptor is in type descriptor pointed to by first	*/
	    /* word in object.						*/
	    ptr_t type_descr = *(ptr_t *)current_p;
	    /* type_descr is either a valid pointer to the descriptor	*/
	    /* structure, or this object was on a free list.  If it 	*/
	    /* it was anything but the last object on the free list,	*/
	    /* we will misinterpret the next object on the free list as */
	    /* the type descriptor, and get a 0 GC descriptor, which	*/
	    /* is ideal.  Unfortunately, we need to check for the last	*/
	    /* object case explicitly.					*/
	    if (0 == type_descr) {
		/* Rarely executed.	*/
		mark_stack_top--;
		continue;
	    }
            descr = *(word *)(type_descr
			      - (descr - (GC_DS_PER_OBJECT
					  - GC_INDIR_PER_OBJ_BIAS)));
	  }
	  if (0 == descr) {
	      /* Can happen either because we generated a 0 descriptor	*/
	      /* or we saw a pointer to a free object.			*/
	      mark_stack_top--;
	      continue;
	  }
          goto retry;
      }
    } else /* Small object with length descriptor */ {
      mark_stack_top--;
      limit = (word *)(((ptr_t)current_p) + (word)descr);
    }
    /* The simple case in which we're scanning a range.	*/
    GC_ASSERT(!((word)current_p & (ALIGNMENT-1)));
    credit -= (ptr_t)limit - (ptr_t)current_p;
    limit -= 1;
    {
#     define PREF_DIST 4

#     ifndef SMALL_CONFIG
        word deferred;

	/* Try to prefetch the next pointer to be examined asap.	*/
	/* Empirically, this also seems to help slightly without	*/
	/* prefetches, at least on linux/X86.  Presumably this loop 	*/
	/* ends up with less register pressure, and gcc thus ends up 	*/
	/* generating slightly better code.  Overall gcc code quality	*/
	/* for this loop is still not great.				*/
	for(;;) {
	  PREFETCH((ptr_t)limit - PREF_DIST*CACHE_LINE_SIZE);
	  GC_ASSERT(limit >= current_p);
	  deferred = *limit;
	  FIXUP_POINTER(deferred);
	  limit = (word *)((char *)limit - ALIGNMENT);
	  if ((ptr_t)deferred >= least_ha && (ptr_t)deferred <  greatest_ha) {
	    PREFETCH((ptr_t)deferred);
	    break;
	  }
	  if (current_p > limit) goto next_object;
	  /* Unroll once, so we don't do too many of the prefetches 	*/
	  /* based on limit.						*/
	  deferred = *limit;
	  FIXUP_POINTER(deferred);
	  limit = (word *)((char *)limit - ALIGNMENT);
	  if ((ptr_t)deferred >= least_ha && (ptr_t)deferred <  greatest_ha) {
	    PREFETCH((ptr_t)deferred);
	    break;
	  }
	  if (current_p > limit) goto next_object;
	}
#     endif

      while (current_p <= limit) {
	/* Empirically, unrolling this loop doesn't help a lot.	*/
	/* Since HC_PUSH_CONTENTS expands to a lot of code,	*/
	/* we don't.						*/
        current = *current_p;
	FIXUP_POINTER(current);
        PREFETCH((ptr_t)current_p + PREF_DIST*CACHE_LINE_SIZE);
        if ((ptr_t)current >= least_ha && (ptr_t)current <  greatest_ha) {
  	  /* Prefetch the contents of the object we just pushed.  It's	*/
  	  /* likely we will need them soon.				*/
  	  PREFETCH((ptr_t)current);
          HC_PUSH_CONTENTS((ptr_t)current, mark_stack_top,
  		           mark_stack_limit, current_p, exit2);
        }
        current_p = (word *)((char *)current_p + ALIGNMENT);
      }

#     ifndef SMALL_CONFIG
	/* We still need to mark the entry we previously prefetched.	*/
	/* We alrady know that it passes the preliminary pointer	*/
	/* validity test.						*/
        HC_PUSH_CONTENTS((ptr_t)deferred, mark_stack_top,
  		         mark_stack_limit, current_p, exit4);
	next_object:;
#     endif
    }
  }
  return mark_stack_top;
}

#ifdef PARALLEL_MARK

/* We assume we have an ANSI C Compiler.	*/
GC_bool GC_help_wanted = FALSE;
unsigned GC_helper_count = 0;
unsigned GC_active_count = 0;
mse * VOLATILE GC_first_nonempty;
word GC_mark_no = 0;

#define LOCAL_MARK_STACK_SIZE HBLKSIZE
	/* Under normal circumstances, this is big enough to guarantee	*/
	/* We don't overflow half of it in a single call to 		*/
	/* GC_mark_from.						*/


/* Steal mark stack entries starting at mse low into mark stack local	*/
/* until we either steal mse high, or we have max entries.		*/
/* Return a pointer to the top of the local mark stack.		        */
/* *next is replaced by a pointer to the next unscanned mark stack	*/
/* entry.								*/
mse * GC_steal_mark_stack(mse * low, mse * high, mse * local,
			  unsigned max, mse **next)
{
    mse *p;
    mse *top = local - 1;
    unsigned i = 0;

    /* Make sure that prior writes to the mark stack are visible. */
    /* On some architectures, the fact that the reads are 	  */
    /* volatile should suffice.					  */
#   if !defined(IA64) && !defined(HP_PA) && !defined(I386)
      GC_memory_barrier();
#   endif
    GC_ASSERT(high >= low-1 && high - low + 1 <= GC_mark_stack_size);
    for (p = low; p <= high && i <= max; ++p) {
	word descr = *(volatile word *) &(p -> mse_descr);
	/* In the IA64 memory model, the following volatile store is	*/
	/* ordered after this read of descr.  Thus a thread must read 	*/
	/* the original nonzero value.  HP_PA appears to be similar,	*/
	/* and if I'm reading the P4 spec correctly, X86 is probably 	*/
	/* also OK.  In some other cases we need a barrier.		*/
#       if !defined(IA64) && !defined(HP_PA) && !defined(I386)
          GC_memory_barrier();
#       endif
	if (descr != 0) {
	    *(volatile word *) &(p -> mse_descr) = 0;
	    /* More than one thread may get this entry, but that's only */
	    /* a minor performance problem.				*/
	    ++top;
	    top -> mse_descr = descr;
	    top -> mse_start = p -> mse_start;
	    GC_ASSERT(  (top -> mse_descr & GC_DS_TAGS) != GC_DS_LENGTH || 
			top -> mse_descr < (ptr_t)GC_greatest_plausible_heap_addr
			                   - (ptr_t)GC_least_plausible_heap_addr);
	    /* If this is a big object, count it as			*/
	    /* size/256 + 1 objects.					*/
	    ++i;
	    if ((descr & GC_DS_TAGS) == GC_DS_LENGTH) i += (descr >> 8);
	}
    }
    *next = p;
    return top;
}

/* Copy back a local mark stack.	*/
/* low and high are inclusive bounds.	*/
void GC_return_mark_stack(mse * low, mse * high)
{
    mse * my_top;
    mse * my_start;
    size_t stack_size;

    if (high < low) return;
    stack_size = high - low + 1;
    GC_acquire_mark_lock();
    my_top = GC_mark_stack_top;
    my_start = my_top + 1;
    if (my_start - GC_mark_stack + stack_size > GC_mark_stack_size) {
#     ifdef CONDPRINT
	if (GC_print_stats) {
	  GC_printf0("No room to copy back mark stack.");
	}
#     endif
      GC_mark_state = MS_INVALID;
      GC_mark_stack_too_small = TRUE;
      /* We drop the local mark stack.  We'll fix things later.	*/
    } else {
      BCOPY(low, my_start, stack_size * sizeof(mse));
      GC_ASSERT(GC_mark_stack_top = my_top);
#     if !defined(IA64) && !defined(HP_PA)
        GC_memory_barrier();
#     endif
	/* On IA64, the volatile write acts as a release barrier. */
      GC_mark_stack_top = my_top + stack_size;
    }
    GC_release_mark_lock();
    GC_notify_all_marker();
}

/* Mark from the local mark stack.		*/
/* On return, the local mark stack is empty.	*/
/* But this may be achieved by copying the	*/
/* local mark stack back into the global one.	*/
void GC_do_local_mark(mse *local_mark_stack, mse *local_top)
{
    unsigned n;
#   define N_LOCAL_ITERS 1

#   ifdef GC_ASSERTIONS
      /* Make sure we don't hold mark lock. */
	GC_acquire_mark_lock();
	GC_release_mark_lock();
#   endif
    for (;;) {
        for (n = 0; n < N_LOCAL_ITERS; ++n) {
	    local_top = GC_mark_from(local_top, local_mark_stack,
				     local_mark_stack + LOCAL_MARK_STACK_SIZE);
	    if (local_top < local_mark_stack) return;
	    if (local_top - local_mark_stack >= LOCAL_MARK_STACK_SIZE/2) {
	 	GC_return_mark_stack(local_mark_stack, local_top);
		return;
	    }
	}
	if (GC_mark_stack_top < GC_first_nonempty &&
	    GC_active_count < GC_helper_count
	    && local_top > local_mark_stack + 1) {
	    /* Try to share the load, since the main stack is empty,	*/
	    /* and helper threads are waiting for a refill.		*/
	    /* The entries near the bottom of the stack are likely	*/
	    /* to require more work.  Thus we return those, eventhough	*/
	    /* it's harder.						*/
	    mse * p;
 	    mse * new_bottom = local_mark_stack
				+ (local_top - local_mark_stack)/2;
	    GC_ASSERT(new_bottom > local_mark_stack
		      && new_bottom < local_top);
	    GC_return_mark_stack(local_mark_stack, new_bottom - 1);
	    memmove(local_mark_stack, new_bottom,
		    (local_top - new_bottom + 1) * sizeof(mse));
	    local_top -= (new_bottom - local_mark_stack);
	}
    }
}

#define ENTRIES_TO_GET 5

long GC_markers = 2;		/* Normally changed by thread-library-	*/
				/* -specific code.			*/

/* Mark using the local mark stack until the global mark stack is empty	*/
/* and there are no active workers. Update GC_first_nonempty to reflect	*/
/* progress.								*/
/* Caller does not hold mark lock.					*/
/* Caller has already incremented GC_helper_count.  We decrement it,	*/
/* and maintain GC_active_count.					*/
void GC_mark_local(mse *local_mark_stack, int id)
{
    mse * my_first_nonempty;

    GC_acquire_mark_lock();
    GC_active_count++;
    my_first_nonempty = GC_first_nonempty;
    GC_ASSERT(GC_first_nonempty >= GC_mark_stack && 
	      GC_first_nonempty <= GC_mark_stack_top + 1);
#   ifdef PRINTSTATS
	GC_printf1("Starting mark helper %lu\n", (unsigned long)id);
#   endif
    GC_release_mark_lock();
    for (;;) {
  	size_t n_on_stack;
        size_t n_to_get;
	mse *next;
	mse * my_top;
	mse * local_top;
        mse * global_first_nonempty = GC_first_nonempty;

    	GC_ASSERT(my_first_nonempty >= GC_mark_stack && 
		  my_first_nonempty <= GC_mark_stack_top + 1);
    	GC_ASSERT(global_first_nonempty >= GC_mark_stack && 
		  global_first_nonempty <= GC_mark_stack_top + 1);
	if (my_first_nonempty < global_first_nonempty) {
	    my_first_nonempty = global_first_nonempty;
        } else if (global_first_nonempty < my_first_nonempty) {
	    GC_compare_and_exchange((word *)(&GC_first_nonempty), 
				   (word) global_first_nonempty,
				   (word) my_first_nonempty);
	    /* If this fails, we just go ahead, without updating	*/
	    /* GC_first_nonempty.					*/
	}
	/* Perhaps we should also update GC_first_nonempty, if it */
	/* is less.  But that would require using atomic updates. */
	my_top = GC_mark_stack_top;
	n_on_stack = my_top - my_first_nonempty + 1;
        if (0 == n_on_stack) {
	    GC_acquire_mark_lock();
            my_top = GC_mark_stack_top;
            n_on_stack = my_top - my_first_nonempty + 1;
	    if (0 == n_on_stack) {
		GC_active_count--;
		GC_ASSERT(GC_active_count <= GC_helper_count);
		/* Other markers may redeposit objects	*/
		/* on the stack.				*/
		if (0 == GC_active_count) GC_notify_all_marker();
		while (GC_active_count > 0
		       && GC_first_nonempty > GC_mark_stack_top) {
		    /* We will be notified if either GC_active_count	*/
		    /* reaches zero, or if more objects are pushed on	*/
		    /* the global mark stack.				*/
		    GC_wait_marker();
		}
		if (GC_active_count == 0 &&
		    GC_first_nonempty > GC_mark_stack_top) { 
		    GC_bool need_to_notify = FALSE;
		    /* The above conditions can't be falsified while we	*/
		    /* hold the mark lock, since neither 		*/
		    /* GC_active_count nor GC_mark_stack_top can	*/
		    /* change.  GC_first_nonempty can only be		*/
		    /* incremented asynchronously.  Thus we know that	*/
		    /* both conditions actually held simultaneously.	*/
		    GC_helper_count--;
		    if (0 == GC_helper_count) need_to_notify = TRUE;
#		    ifdef PRINTSTATS
		      GC_printf1(
		        "Finished mark helper %lu\n", (unsigned long)id);
#   		    endif
		    GC_release_mark_lock();
		    if (need_to_notify) GC_notify_all_marker();
		    return;
		}
		/* else there's something on the stack again, or	*/
		/* another helper may push something.			*/
		GC_active_count++;
	        GC_ASSERT(GC_active_count > 0);
		GC_release_mark_lock();
		continue;
	    } else {
		GC_release_mark_lock();
	    }
	}
	n_to_get = ENTRIES_TO_GET;
	if (n_on_stack < 2 * ENTRIES_TO_GET) n_to_get = 1;
	local_top = GC_steal_mark_stack(my_first_nonempty, my_top,
					local_mark_stack, n_to_get,
				        &my_first_nonempty);
        GC_ASSERT(my_first_nonempty >= GC_mark_stack && 
	          my_first_nonempty <= GC_mark_stack_top + 1);
	GC_do_local_mark(local_mark_stack, local_top);
    }
}

/* Perform Parallel mark.			*/
/* We hold the GC lock, not the mark lock.	*/
/* Currently runs until the mark stack is	*/
/* empty.					*/
void GC_do_parallel_mark()
{
    mse local_mark_stack[LOCAL_MARK_STACK_SIZE];
    mse * local_top;
    mse * my_top;

    GC_acquire_mark_lock();
    GC_ASSERT(I_HOLD_LOCK());
    /* This could be a GC_ASSERT, but it seems safer to keep it on	*/
    /* all the time, especially since it's cheap.			*/
    if (GC_help_wanted || GC_active_count != 0 || GC_helper_count != 0)
	ABORT("Tried to start parallel mark in bad state");
#   ifdef PRINTSTATS
	GC_printf1("Starting marking for mark phase number %lu\n",
		   (unsigned long)GC_mark_no);
#   endif
    GC_first_nonempty = GC_mark_stack;
    GC_active_count = 0;
    GC_helper_count = 1;
    GC_help_wanted = TRUE;
    GC_release_mark_lock();
    GC_notify_all_marker();
	/* Wake up potential helpers.	*/
    GC_mark_local(local_mark_stack, 0);
    GC_acquire_mark_lock();
    GC_help_wanted = FALSE;
    /* Done; clean up.	*/
    while (GC_helper_count > 0) GC_wait_marker();
    /* GC_helper_count cannot be incremented while GC_help_wanted == FALSE */
#   ifdef PRINTSTATS
	GC_printf1(
	    "Finished marking for mark phase number %lu\n",
	    (unsigned long)GC_mark_no);
#   endif
    GC_mark_no++;
    GC_release_mark_lock();
    GC_notify_all_marker();
}


/* Try to help out the marker, if it's running.	        */
/* We do not hold the GC lock, but the requestor does.	*/
void GC_help_marker(word my_mark_no)
{
    mse local_mark_stack[LOCAL_MARK_STACK_SIZE];
    unsigned my_id;
    mse * my_first_nonempty;

    if (!GC_parallel) return;
    GC_acquire_mark_lock();
    while (GC_mark_no < my_mark_no
           || !GC_help_wanted && GC_mark_no == my_mark_no) {
      GC_wait_marker();
    }
    my_id = GC_helper_count;
    if (GC_mark_no != my_mark_no || my_id >= GC_markers) {
      /* Second test is useful only if original threads can also	*/
      /* act as helpers.  Under Linux they can't.			*/
      GC_release_mark_lock();
      return;
    }
    GC_helper_count = my_id + 1;
    GC_release_mark_lock();
    GC_mark_local(local_mark_stack, my_id);
    /* GC_mark_local decrements GC_helper_count. */
}

#endif /* PARALLEL_MARK */

/* Allocate or reallocate space for mark stack of size s words  */
/* May silently fail.						*/
static void alloc_mark_stack(n)
word n;
{
    mse * new_stack = (mse *)GC_scratch_alloc(n * sizeof(struct GC_ms_entry));
    
    GC_mark_stack_too_small = FALSE;
    if (GC_mark_stack_size != 0) {
        if (new_stack != 0) {
          word displ = (word)GC_mark_stack & (GC_page_size - 1);
          signed_word size = GC_mark_stack_size * sizeof(struct GC_ms_entry);
          
          /* Recycle old space */
	      if (0 != displ) displ = GC_page_size - displ;
	      size = (size - displ) & ~(GC_page_size - 1);
	      if (size > 0) {
	        GC_add_to_heap((struct hblk *)
	      			((word)GC_mark_stack + displ), (word)size);
	      }
          GC_mark_stack = new_stack;
          GC_mark_stack_size = n;
	  GC_mark_stack_limit = new_stack + n;
#	  ifdef CONDPRINT
	    if (GC_print_stats) {
	      GC_printf1("Grew mark stack to %lu frames\n",
		    	 (unsigned long) GC_mark_stack_size);
	    }
#	  endif
        } else {
#	  ifdef CONDPRINT
	    if (GC_print_stats) {
	      GC_printf1("Failed to grow mark stack to %lu frames\n",
		    	 (unsigned long) n);
	    }
#	  endif
        }
    } else {
        if (new_stack == 0) {
            GC_err_printf0("No space for mark stack\n");
            EXIT();
        }
        GC_mark_stack = new_stack;
        GC_mark_stack_size = n;
	GC_mark_stack_limit = new_stack + n;
    }
    GC_mark_stack_top = GC_mark_stack-1;
}

void GC_mark_init()
{
    alloc_mark_stack(INITIAL_MARK_STACK_SIZE);
}

/*
 * Push all locations between b and t onto the mark stack.
 * b is the first location to be checked. t is one past the last
 * location to be checked.
 * Should only be used if there is no possibility of mark stack
 * overflow.
 */
void GC_push_all(bottom, top)
ptr_t bottom;
ptr_t top;
{
    register word length;
    
    bottom = (ptr_t)(((word) bottom + ALIGNMENT-1) & ~(ALIGNMENT-1));
    top = (ptr_t)(((word) top) & ~(ALIGNMENT-1));
    if (top == 0 || bottom == top) return;
    GC_mark_stack_top++;
    if (GC_mark_stack_top >= GC_mark_stack_limit) {
	ABORT("unexpected mark stack overflow");
    }
    length = top - bottom;
#   if GC_DS_TAGS > ALIGNMENT - 1
	length += GC_DS_TAGS;
	length &= ~GC_DS_TAGS;
#   endif
    GC_mark_stack_top -> mse_start = (word *)bottom;
    GC_mark_stack_top -> mse_descr = length;
}

/*
 * Analogous to the above, but push only those pages h with dirty_fn(h) != 0.
 * We use push_fn to actually push the block.
 * Used both to selectively push dirty pages, or to push a block
 * in piecemeal fashion, to allow for more marking concurrency.
 * Will not overflow mark stack if push_fn pushes a small fixed number
 * of entries.  (This is invoked only if push_fn pushes a single entry,
 * or if it marks each object before pushing it, thus ensuring progress
 * in the event of a stack overflow.)
 */
void GC_push_selected(bottom, top, dirty_fn, push_fn)
ptr_t bottom;
ptr_t top;
int (*dirty_fn) GC_PROTO((struct hblk * h));
void (*push_fn) GC_PROTO((ptr_t bottom, ptr_t top));
{
    register struct hblk * h;

    bottom = (ptr_t)(((long) bottom + ALIGNMENT-1) & ~(ALIGNMENT-1));
    top = (ptr_t)(((long) top) & ~(ALIGNMENT-1));

    if (top == 0 || bottom == top) return;
    h = HBLKPTR(bottom + HBLKSIZE);
    if (top <= (ptr_t) h) {
  	if ((*dirty_fn)(h-1)) {
	    (*push_fn)(bottom, top);
	}
	return;
    }
    if ((*dirty_fn)(h-1)) {
        (*push_fn)(bottom, (ptr_t)h);
    }
    while ((ptr_t)(h+1) <= top) {
	if ((*dirty_fn)(h)) {
	    if ((word)(GC_mark_stack_top - GC_mark_stack)
		> 3 * GC_mark_stack_size / 4) {
	 	/* Danger of mark stack overflow */
		(*push_fn)((ptr_t)h, top);
		return;
	    } else {
		(*push_fn)((ptr_t)h, (ptr_t)(h+1));
	    }
	}
	h++;
    }
    if ((ptr_t)h != top) {
	if ((*dirty_fn)(h)) {
            (*push_fn)((ptr_t)h, top);
        }
    }
    if (GC_mark_stack_top >= GC_mark_stack_limit) {
        ABORT("unexpected mark stack overflow");
    }
}

# ifndef SMALL_CONFIG

#ifdef PARALLEL_MARK
    /* Break up root sections into page size chunks to better spread 	*/
    /* out work.							*/
    GC_bool GC_true_func(struct hblk *h) { return TRUE; }
#   define GC_PUSH_ALL(b,t) GC_push_selected(b,t,GC_true_func,GC_push_all);
#else
#   define GC_PUSH_ALL(b,t) GC_push_all(b,t);
#endif


void GC_push_conditional(bottom, top, all)
ptr_t bottom;
ptr_t top;
int all;
{
    if (all) {
      if (GC_dirty_maintained) {
#	ifdef PROC_VDB
	    /* Pages that were never dirtied cannot contain pointers	*/
	    GC_push_selected(bottom, top, GC_page_was_ever_dirty, GC_push_all);
#	else
	    GC_push_all(bottom, top);
#	endif
      } else {
      	GC_push_all(bottom, top);
      }
    } else {
	GC_push_selected(bottom, top, GC_page_was_dirty, GC_push_all);
    }
}
#endif

# if defined(MSWIN32) || defined(MSWINCE)
  void __cdecl GC_push_one(p)
# else
  void GC_push_one(p)
# endif
word p;
{
    GC_PUSH_ONE_STACK(p, MARKED_FROM_REGISTER);
}

struct GC_ms_entry *GC_mark_and_push(obj, mark_stack_ptr, mark_stack_limit, src)
GC_PTR obj;
struct GC_ms_entry * mark_stack_ptr;
struct GC_ms_entry * mark_stack_limit;
GC_PTR *src;
{
   PREFETCH(obj);
   PUSH_CONTENTS(obj, mark_stack_ptr /* modified */, mark_stack_limit, src,
		 was_marked /* internally generated exit label */);
   return mark_stack_ptr;
}

# ifdef __STDC__
#   define BASE(p) (word)GC_base((void *)(p))
# else
#   define BASE(p) (word)GC_base((char *)(p))
# endif

/* Mark and push (i.e. gray) a single object p onto the main	*/
/* mark stack.  Consider p to be valid if it is an interior	*/
/* pointer.							*/
/* The object p has passed a preliminary pointer validity	*/
/* test, but we do not definitely know whether it is valid.	*/
/* Mark bits are NOT atomically updated.  Thus this must be the	*/
/* only thread setting them.					*/
# if defined(PRINT_BLACK_LIST) || defined(KEEP_BACK_PTRS)
    void GC_mark_and_push_stack(p, source)
    ptr_t source;
# else
    void GC_mark_and_push_stack(p)
#   define source 0
# endif
register word p;
{
    register word r;
    register hdr * hhdr; 
    register int displ;
  
    GET_HDR(p, hhdr);
    if (IS_FORWARDING_ADDR_OR_NIL(hhdr)) {
        if (hhdr != 0) {
          r = BASE(p);
	  hhdr = HDR(r);
	  displ = BYTES_TO_WORDS(HBLKDISPL(r));
	}
    } else {
        register map_entry_type map_entry;
        
        displ = HBLKDISPL(p);
        map_entry = MAP_ENTRY((hhdr -> hb_map), displ);
        if (map_entry >= MAX_OFFSET) {
          if (map_entry == OFFSET_TOO_BIG || !GC_all_interior_pointers) {
              r = BASE(p);
	      displ = BYTES_TO_WORDS(HBLKDISPL(r));
	      if (r == 0) hhdr = 0;
          } else {
	      /* Offset invalid, but map reflects interior pointers 	*/
              hhdr = 0;
          }
        } else {
          displ = BYTES_TO_WORDS(displ);
          displ -= map_entry;
          r = (word)((word *)(HBLKPTR(p)) + displ);
        }
    }
    /* If hhdr != 0 then r == GC_base(p), only we did it faster. */
    /* displ is the word index within the block.		 */
    if (hhdr == 0) {
#	ifdef PRINT_BLACK_LIST
	  GC_add_to_black_list_stack(p, source);
#	else
	  GC_add_to_black_list_stack(p);
#	endif
#	undef source  /* In case we had to define it. */
    } else {
	if (!mark_bit_from_hdr(hhdr, displ)) {
	    set_mark_bit_from_hdr(hhdr, displ);
 	    GC_STORE_BACK_PTR(source, (ptr_t)r);
	    PUSH_OBJ((word *)r, hhdr, GC_mark_stack_top,
	             GC_mark_stack_limit);
	}
    }
}

# ifdef TRACE_BUF

# define TRACE_ENTRIES 1000

struct trace_entry {
    char * kind;
    word gc_no;
    word words_allocd;
    word arg1;
    word arg2;
} GC_trace_buf[TRACE_ENTRIES];

int GC_trace_buf_ptr = 0;

void GC_add_trace_entry(char *kind, word arg1, word arg2)
{
    GC_trace_buf[GC_trace_buf_ptr].kind = kind;
    GC_trace_buf[GC_trace_buf_ptr].gc_no = GC_gc_no;
    GC_trace_buf[GC_trace_buf_ptr].words_allocd = GC_words_allocd;
    GC_trace_buf[GC_trace_buf_ptr].arg1 = arg1 ^ 0x80000000;
    GC_trace_buf[GC_trace_buf_ptr].arg2 = arg2 ^ 0x80000000;
    GC_trace_buf_ptr++;
    if (GC_trace_buf_ptr >= TRACE_ENTRIES) GC_trace_buf_ptr = 0;
}

void GC_print_trace(word gc_no, GC_bool lock)
{
    int i;
    struct trace_entry *p;
    
    if (lock) LOCK();
    for (i = GC_trace_buf_ptr-1; i != GC_trace_buf_ptr; i--) {
    	if (i < 0) i = TRACE_ENTRIES-1;
    	p = GC_trace_buf + i;
    	if (p -> gc_no < gc_no || p -> kind == 0) return;
    	printf("Trace:%s (gc:%d,words:%d) 0x%X, 0x%X\n",
    		p -> kind, p -> gc_no, p -> words_allocd,
    		(p -> arg1) ^ 0x80000000, (p -> arg2) ^ 0x80000000);
    }
    printf("Trace incomplete\n");
    if (lock) UNLOCK();
}

# endif /* TRACE_BUF */

/*
 * A version of GC_push_all that treats all interior pointers as valid
 * and scans the entire region immediately, in case the contents
 * change.
 */
void GC_push_all_eager(bottom, top)
ptr_t bottom;
ptr_t top;
{
    word * b = (word *)(((word) bottom + ALIGNMENT-1) & ~(ALIGNMENT-1));
    word * t = (word *)(((word) top) & ~(ALIGNMENT-1));
    register word *p;
    register word q;
    register word *lim;
    register ptr_t greatest_ha = GC_greatest_plausible_heap_addr;
    register ptr_t least_ha = GC_least_plausible_heap_addr;
#   define GC_greatest_plausible_heap_addr greatest_ha
#   define GC_least_plausible_heap_addr least_ha

    if (top == 0) return;
    /* check all pointers in range and push if they appear	*/
    /* to be valid.						*/
      lim = t - 1 /* longword */;
      for (p = b; p <= lim; p = (word *)(((char *)p) + ALIGNMENT)) {
	q = *p;
	GC_PUSH_ONE_STACK(q, p);
      }
#   undef GC_greatest_plausible_heap_addr
#   undef GC_least_plausible_heap_addr
}

#ifndef THREADS
/*
 * A version of GC_push_all that treats all interior pointers as valid
 * and scans part of the area immediately, to make sure that saved
 * register values are not lost.
 * Cold_gc_frame delimits the stack section that must be scanned
 * eagerly.  A zero value indicates that no eager scanning is needed.
 */
void GC_push_all_stack_partially_eager(bottom, top, cold_gc_frame)
ptr_t bottom;
ptr_t top;
ptr_t cold_gc_frame;
{
  if (!NEED_FIXUP_POINTER && GC_all_interior_pointers) {
#   define EAGER_BYTES 1024
    /* Push the hot end of the stack eagerly, so that register values   */
    /* saved inside GC frames are marked before they disappear.		*/
    /* The rest of the marking can be deferred until later.		*/
    if (0 == cold_gc_frame) {
	GC_push_all_stack(bottom, top);
	return;
    }
    GC_ASSERT(bottom <= cold_gc_frame && cold_gc_frame <= top);
#   ifdef STACK_GROWS_DOWN
	GC_push_all(cold_gc_frame - sizeof(ptr_t), top);
	GC_push_all_eager(bottom, cold_gc_frame);
#   else /* STACK_GROWS_UP */
	GC_push_all(bottom, cold_gc_frame + sizeof(ptr_t));
	GC_push_all_eager(cold_gc_frame, top);
#   endif /* STACK_GROWS_UP */
  } else {
    GC_push_all_eager(bottom, top);
  }
# ifdef TRACE_BUF
      GC_add_trace_entry("GC_push_all_stack", bottom, top);
# endif
}
#endif /* !THREADS */

void GC_push_all_stack(bottom, top)
ptr_t bottom;
ptr_t top;
{
  if (!NEED_FIXUP_POINTER && GC_all_interior_pointers) {
    GC_push_all(bottom, top);
  } else {
    GC_push_all_eager(bottom, top);
  }
}

#if !defined(SMALL_CONFIG) && !defined(USE_MARK_BYTES)
/* Push all objects reachable from marked objects in the given block */
/* of size 1 objects.						     */
void GC_push_marked1(h, hhdr)
struct hblk *h;
register hdr * hhdr;
{
    word * mark_word_addr = &(hhdr->hb_marks[0]);
    register word *p;
    word *plim;
    register int i;
    register word q;
    register word mark_word;
    register ptr_t greatest_ha = GC_greatest_plausible_heap_addr;
    register ptr_t least_ha = GC_least_plausible_heap_addr;
    register mse * mark_stack_top = GC_mark_stack_top;
    register mse * mark_stack_limit = GC_mark_stack_limit;
#   define GC_mark_stack_top mark_stack_top
#   define GC_mark_stack_limit mark_stack_limit
#   define GC_greatest_plausible_heap_addr greatest_ha
#   define GC_least_plausible_heap_addr least_ha
    
    p = (word *)(h->hb_body);
    plim = (word *)(((word)h) + HBLKSIZE);

    /* go through all words in block */
	while( p < plim )  {
	    mark_word = *mark_word_addr++;
	    i = 0;
	    while(mark_word != 0) {
	      if (mark_word & 1) {
	          q = p[i];
	          GC_PUSH_ONE_HEAP(q, p + i);
	      }
	      i++;
	      mark_word >>= 1;
	    }
	    p += WORDSZ;
	}
#   undef GC_greatest_plausible_heap_addr
#   undef GC_least_plausible_heap_addr        
#   undef GC_mark_stack_top
#   undef GC_mark_stack_limit
    GC_mark_stack_top = mark_stack_top;
}


#ifndef UNALIGNED

/* Push all objects reachable from marked objects in the given block */
/* of size 2 objects.						     */
void GC_push_marked2(h, hhdr)
struct hblk *h;
register hdr * hhdr;
{
    word * mark_word_addr = &(hhdr->hb_marks[0]);
    register word *p;
    word *plim;
    register int i;
    register word q;
    register word mark_word;
    register ptr_t greatest_ha = GC_greatest_plausible_heap_addr;
    register ptr_t least_ha = GC_least_plausible_heap_addr;
    register mse * mark_stack_top = GC_mark_stack_top;
    register mse * mark_stack_limit = GC_mark_stack_limit;
#   define GC_mark_stack_top mark_stack_top
#   define GC_mark_stack_limit mark_stack_limit
#   define GC_greatest_plausible_heap_addr greatest_ha
#   define GC_least_plausible_heap_addr least_ha
    
    p = (word *)(h->hb_body);
    plim = (word *)(((word)h) + HBLKSIZE);

    /* go through all words in block */
	while( p < plim )  {
	    mark_word = *mark_word_addr++;
	    i = 0;
	    while(mark_word != 0) {
	      if (mark_word & 1) {
	          q = p[i];
	          GC_PUSH_ONE_HEAP(q, p + i);
	          q = p[i+1];
	          GC_PUSH_ONE_HEAP(q, p + i);
	      }
	      i += 2;
	      mark_word >>= 2;
	    }
	    p += WORDSZ;
	}
#   undef GC_greatest_plausible_heap_addr
#   undef GC_least_plausible_heap_addr        
#   undef GC_mark_stack_top
#   undef GC_mark_stack_limit
    GC_mark_stack_top = mark_stack_top;
}

/* Push all objects reachable from marked objects in the given block */
/* of size 4 objects.						     */
/* There is a risk of mark stack overflow here.  But we handle that. */
/* And only unmarked objects get pushed, so it's not very likely.    */
void GC_push_marked4(h, hhdr)
struct hblk *h;
register hdr * hhdr;
{
    word * mark_word_addr = &(hhdr->hb_marks[0]);
    register word *p;
    word *plim;
    register int i;
    register word q;
    register word mark_word;
    register ptr_t greatest_ha = GC_greatest_plausible_heap_addr;
    register ptr_t least_ha = GC_least_plausible_heap_addr;
    register mse * mark_stack_top = GC_mark_stack_top;
    register mse * mark_stack_limit = GC_mark_stack_limit;
#   define GC_mark_stack_top mark_stack_top
#   define GC_mark_stack_limit mark_stack_limit
#   define GC_greatest_plausible_heap_addr greatest_ha
#   define GC_least_plausible_heap_addr least_ha
    
    p = (word *)(h->hb_body);
    plim = (word *)(((word)h) + HBLKSIZE);

    /* go through all words in block */
	while( p < plim )  {
	    mark_word = *mark_word_addr++;
	    i = 0;
	    while(mark_word != 0) {
	      if (mark_word & 1) {
	          q = p[i];
	          GC_PUSH_ONE_HEAP(q, p + i);
	          q = p[i+1];
	          GC_PUSH_ONE_HEAP(q, p + i + 1);
	          q = p[i+2];
	          GC_PUSH_ONE_HEAP(q, p + i + 2);
	          q = p[i+3];
	          GC_PUSH_ONE_HEAP(q, p + i + 3);
	      }
	      i += 4;
	      mark_word >>= 4;
	    }
	    p += WORDSZ;
	}
#   undef GC_greatest_plausible_heap_addr
#   undef GC_least_plausible_heap_addr        
#   undef GC_mark_stack_top
#   undef GC_mark_stack_limit
    GC_mark_stack_top = mark_stack_top;
}

#endif /* UNALIGNED */

#endif /* SMALL_CONFIG */

/* Push all objects reachable from marked objects in the given block */
void GC_push_marked(h, hhdr)
struct hblk *h;
register hdr * hhdr;
{
    register int sz = hhdr -> hb_sz;
    register int descr = hhdr -> hb_descr;
    register word * p;
    register int word_no;
    register word * lim;
    register mse * GC_mark_stack_top_reg;
    register mse * mark_stack_limit = GC_mark_stack_limit;
    
    /* Some quick shortcuts: */
	if ((0 | GC_DS_LENGTH) == descr) return;
        if (GC_block_empty(hhdr)/* nothing marked */) return;
    GC_n_rescuing_pages++;
    GC_objects_are_marked = TRUE;
    if (sz > MAXOBJSZ) {
        lim = (word *)h;
    } else {
        lim = (word *)(h + 1) - sz;
    }
    
    switch(sz) {
#   if !defined(SMALL_CONFIG) && !defined(USE_MARK_BYTES)   
     case 1:
       GC_push_marked1(h, hhdr);
       break;
#   endif
#   if !defined(SMALL_CONFIG) && !defined(UNALIGNED) && \
       !defined(USE_MARK_BYTES)
     case 2:
       GC_push_marked2(h, hhdr);
       break;
     case 4:
       GC_push_marked4(h, hhdr);
       break;
#   endif       
     default:
      GC_mark_stack_top_reg = GC_mark_stack_top;
      for (p = (word *)h, word_no = 0; p <= lim; p += sz, word_no += sz) {
         if (mark_bit_from_hdr(hhdr, word_no)) {
           /* Mark from fields inside the object */
             PUSH_OBJ((word *)p, hhdr, GC_mark_stack_top_reg, mark_stack_limit);
#	     ifdef GATHERSTATS
		/* Subtract this object from total, since it was	*/
		/* added in twice.					*/
		GC_composite_in_use -= sz;
#	     endif
         }
      }
      GC_mark_stack_top = GC_mark_stack_top_reg;
    }
}

#ifndef SMALL_CONFIG
/* Test whether any page in the given block is dirty	*/
GC_bool GC_block_was_dirty(h, hhdr)
struct hblk *h;
register hdr * hhdr;
{
    register int sz = hhdr -> hb_sz;
    
    if (sz <= MAXOBJSZ) {
         return(GC_page_was_dirty(h));
    } else {
    	 register ptr_t p = (ptr_t)h;
         sz = WORDS_TO_BYTES(sz);
         while (p < (ptr_t)h + sz) {
             if (GC_page_was_dirty((struct hblk *)p)) return(TRUE);
             p += HBLKSIZE;
         }
         return(FALSE);
    }
}
#endif /* SMALL_CONFIG */

/* Similar to GC_push_next_marked, but return address of next block	*/
struct hblk * GC_push_next_marked(h)
struct hblk *h;
{
    register hdr * hhdr;
    
    h = GC_next_used_block(h);
    if (h == 0) return(0);
    hhdr = HDR(h);
    GC_push_marked(h, hhdr);
    return(h + OBJ_SZ_TO_BLOCKS(hhdr -> hb_sz));
}

#ifndef SMALL_CONFIG
/* Identical to above, but mark only from dirty pages	*/
struct hblk * GC_push_next_marked_dirty(h)
struct hblk *h;
{
    register hdr * hhdr;
    
    if (!GC_dirty_maintained) { ABORT("dirty bits not set up"); }
    for (;;) {
        h = GC_next_used_block(h);
        if (h == 0) return(0);
        hhdr = HDR(h);
#	ifdef STUBBORN_ALLOC
          if (hhdr -> hb_obj_kind == STUBBORN) {
            if (GC_page_was_changed(h) && GC_block_was_dirty(h, hhdr)) {
                break;
            }
          } else {
            if (GC_block_was_dirty(h, hhdr)) break;
          }
#	else
	  if (GC_block_was_dirty(h, hhdr)) break;
#	endif
        h += OBJ_SZ_TO_BLOCKS(hhdr -> hb_sz);
    }
    GC_push_marked(h, hhdr);
    return(h + OBJ_SZ_TO_BLOCKS(hhdr -> hb_sz));
}
#endif

/* Similar to above, but for uncollectable pages.  Needed since we	*/
/* do not clear marks for such pages, even for full collections.	*/
struct hblk * GC_push_next_marked_uncollectable(h)
struct hblk *h;
{
    register hdr * hhdr = HDR(h);
    
    for (;;) {
        h = GC_next_used_block(h);
        if (h == 0) return(0);
        hhdr = HDR(h);
	if (hhdr -> hb_obj_kind == UNCOLLECTABLE) break;
        h += OBJ_SZ_TO_BLOCKS(hhdr -> hb_sz);
    }
    GC_push_marked(h, hhdr);
    return(h + OBJ_SZ_TO_BLOCKS(hhdr -> hb_sz));
}


