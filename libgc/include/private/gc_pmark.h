/*
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 2001 by Hewlett-Packard Company. All rights reserved.
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

/* Private declarations of GC marker data structures and macros */

/*
 * Declarations of mark stack.  Needed by marker and client supplied mark
 * routines.  Transitively include gc_priv.h.
 * (Note that gc_priv.h should not be included before this, since this
 * includes dbg_mlc.h, which wants to include gc_priv.h AFTER defining
 * I_HIDE_POINTERS.)
 */
#ifndef GC_PMARK_H
# define GC_PMARK_H

# if defined(KEEP_BACK_PTRS) || defined(PRINT_BLACK_LIST)
#   include "dbg_mlc.h"
# endif
# ifndef GC_MARK_H
#   include "../gc_mark.h"
# endif
# ifndef GC_PRIVATE_H
#   include "gc_priv.h"
# endif

/* The real declarations of the following is in gc_priv.h, so that	*/
/* we can avoid scanning the following table.				*/
/*
extern mark_proc GC_mark_procs[MAX_MARK_PROCS];
*/

/*
 * Mark descriptor stuff that should remain private for now, mostly
 * because it's hard to export WORDSZ without including gcconfig.h.
 */
# define BITMAP_BITS (WORDSZ - GC_DS_TAG_BITS)
# define PROC(descr) \
	(GC_mark_procs[((descr) >> GC_DS_TAG_BITS) & (GC_MAX_MARK_PROCS-1)])
# define ENV(descr) \
	((descr) >> (GC_DS_TAG_BITS + GC_LOG_MAX_MARK_PROCS))
# define MAX_ENV \
  	(((word)1 << (WORDSZ - GC_DS_TAG_BITS - GC_LOG_MAX_MARK_PROCS)) - 1)


extern word GC_n_mark_procs;

/* Number of mark stack entries to discard on overflow.	*/
#define GC_MARK_STACK_DISCARDS (INITIAL_MARK_STACK_SIZE/8)

typedef struct GC_ms_entry {
    GC_word * mse_start;   /* First word of object */
    GC_word mse_descr;	/* Descriptor; low order two bits are tags,	*/
    			/* identifying the upper 30 bits as one of the	*/
    			/* following:					*/
} mse;

extern word GC_mark_stack_size;

extern mse * GC_mark_stack_limit;

#ifdef PARALLEL_MARK
  extern mse * VOLATILE GC_mark_stack_top;
#else
  extern mse * GC_mark_stack_top;
#endif

extern mse * GC_mark_stack;

#ifdef PARALLEL_MARK
    /*
     * Allow multiple threads to participate in the marking process.
     * This works roughly as follows:
     *  The main mark stack never shrinks, but it can grow.
     *
     *	The initiating threads holds the GC lock, and sets GC_help_wanted.
     *  
     *  Other threads:
     *     1) update helper_count (while holding mark_lock.)
     *	   2) allocate a local mark stack
     *     repeatedly:
     *		3) Steal a global mark stack entry by atomically replacing
     *		   its descriptor with 0.
     *		4) Copy it to the local stack.
     *	        5) Mark on the local stack until it is empty, or
     *		   it may be profitable to copy it back.
     *	        6) If necessary, copy local stack to global one,
     *		   holding mark lock.
     *    7) Stop when the global mark stack is empty.
     *    8) decrement helper_count (holding mark_lock).
     *
     * This is an experiment to see if we can do something along the lines
     * of the University of Tokyo SGC in a less intrusive, though probably
     * also less performant, way.
     */
    void GC_do_parallel_mark();
		/* inititate parallel marking.	*/

    extern GC_bool GC_help_wanted;	/* Protected by mark lock	*/
    extern unsigned GC_helper_count;	/* Number of running helpers.	*/
					/* Protected by mark lock	*/
    extern unsigned GC_active_count;	/* Number of active helpers.	*/
					/* Protected by mark lock	*/
					/* May increase and decrease	*/
					/* within each mark cycle.  But	*/
					/* once it returns to 0, it	*/
					/* stays zero for the cycle.	*/
    /* GC_mark_stack_top is also protected by mark lock.	*/
    extern mse * VOLATILE GC_first_nonempty;
					/* Lowest entry on mark stack	*/
					/* that may be nonempty.	*/
					/* Updated only by initiating 	*/
					/* thread.			*/
    /*
     * GC_notify_all_marker() is used when GC_help_wanted is first set,
     * when the last helper becomes inactive,
     * when something is added to the global mark stack, and just after
     * GC_mark_no is incremented.
     * This could be split into multiple CVs (and probably should be to
     * scale to really large numbers of processors.)
     */
#endif /* PARALLEL_MARK */

/* Return a pointer to within 1st page of object.  	*/
/* Set *new_hdr_p to corr. hdr.				*/
#ifdef __STDC__
  ptr_t GC_find_start(ptr_t current, hdr *hhdr, hdr **new_hdr_p);
#else
  ptr_t GC_find_start();
#endif

mse * GC_signal_mark_stack_overflow GC_PROTO((mse *msp));

# ifdef GATHERSTATS
#   define ADD_TO_ATOMIC(sz) GC_atomic_in_use += (sz)
#   define ADD_TO_COMPOSITE(sz) GC_composite_in_use += (sz)
# else
#   define ADD_TO_ATOMIC(sz)
#   define ADD_TO_COMPOSITE(sz)
# endif

/* Push the object obj with corresponding heap block header hhdr onto 	*/
/* the mark stack.							*/
# define PUSH_OBJ(obj, hhdr, mark_stack_top, mark_stack_limit) \
{ \
    register word _descr = (hhdr) -> hb_descr; \
        \
    if (_descr == 0) { \
    	ADD_TO_ATOMIC((hhdr) -> hb_sz); \
    } else { \
        ADD_TO_COMPOSITE((hhdr) -> hb_sz); \
        mark_stack_top++; \
        if (mark_stack_top >= mark_stack_limit) { \
          mark_stack_top = GC_signal_mark_stack_overflow(mark_stack_top); \
        } \
        mark_stack_top -> mse_start = (obj); \
        mark_stack_top -> mse_descr = _descr; \
    } \
}

/* Push the contents of current onto the mark stack if it is a valid	*/
/* ptr to a currently unmarked object.  Mark it.			*/
/* If we assumed a standard-conforming compiler, we could probably	*/
/* generate the exit_label transparently.				*/
# define PUSH_CONTENTS(current, mark_stack_top, mark_stack_limit, \
		       source, exit_label) \
{ \
    hdr * my_hhdr; \
    ptr_t my_current = current; \
 \
    GET_HDR(my_current, my_hhdr); \
    if (IS_FORWARDING_ADDR_OR_NIL(my_hhdr)) { \
	 hdr * new_hdr = GC_invalid_header; \
         my_current = GC_find_start(my_current, my_hhdr, &new_hdr); \
         my_hhdr = new_hdr; \
    } \
    PUSH_CONTENTS_HDR(my_current, mark_stack_top, mark_stack_limit, \
		  source, exit_label, my_hhdr);	\
exit_label: ; \
}

/* As above, but use header cache for header lookup.	*/
# define HC_PUSH_CONTENTS(current, mark_stack_top, mark_stack_limit, \
		       source, exit_label) \
{ \
    hdr * my_hhdr; \
    ptr_t my_current = current; \
 \
    HC_GET_HDR(my_current, my_hhdr, source); \
    PUSH_CONTENTS_HDR(my_current, mark_stack_top, mark_stack_limit, \
		  source, exit_label, my_hhdr);	\
exit_label: ; \
}

/* Set mark bit, exit if it was already set.	*/

# ifdef USE_MARK_BYTES
    /* Unlike the mark bit case, there is a race here, and we may set	*/
    /* the bit twice in the concurrent case.  This can result in the	*/
    /* object being pushed twice.  But that's only a performance issue.	*/
#   define SET_MARK_BIT_EXIT_IF_SET(hhdr,displ,exit_label) \
    { \
        register VOLATILE char * mark_byte_addr = \
				hhdr -> hb_marks + ((displ) >> 1); \
        register char mark_byte = *mark_byte_addr; \
          \
	if (mark_byte) goto exit_label; \
	*mark_byte_addr = 1;  \
    } 
# else
#   define SET_MARK_BIT_EXIT_IF_SET(hhdr,displ,exit_label) \
    { \
        register word * mark_word_addr = hhdr -> hb_marks + divWORDSZ(displ); \
          \
        OR_WORD_EXIT_IF_SET(mark_word_addr, (word)1 << modWORDSZ(displ), \
			    exit_label); \
    } 
# endif /* USE_MARK_BYTES */

/* If the mark bit corresponding to current is not set, set it, and 	*/
/* push the contents of the object on the mark stack.  For a small 	*/
/* object we assume that current is the (possibly interior) pointer	*/
/* to the object.  For large objects we assume that current points	*/
/* to somewhere inside the first page of the object.  If		*/
/* GC_all_interior_pointers is set, it may have been previously 	*/
/* adjusted to make that true.						*/
# define PUSH_CONTENTS_HDR(current, mark_stack_top, mark_stack_limit, \
		           source, exit_label, hhdr) \
{ \
    int displ;  /* Displacement in block; first bytes, then words */ \
    int map_entry; \
    \
    displ = HBLKDISPL(current); \
    map_entry = MAP_ENTRY((hhdr -> hb_map), displ); \
    displ = BYTES_TO_WORDS(displ); \
    if (map_entry > CPP_MAX_OFFSET) { \
	if (map_entry == OFFSET_TOO_BIG) { \
	  map_entry = displ % (hhdr -> hb_sz); \
	  displ -= map_entry; \
	  if (displ + (hhdr -> hb_sz) > BYTES_TO_WORDS(HBLKSIZE)) { \
	    GC_ADD_TO_BLACK_LIST_NORMAL((word)current, source); \
	    goto exit_label; \
	  } \
	} else { \
          GC_ADD_TO_BLACK_LIST_NORMAL((word)current, source); goto exit_label; \
	} \
    } else { \
        displ -= map_entry; \
    } \
    GC_ASSERT(displ >= 0 && displ < MARK_BITS_PER_HBLK); \
    SET_MARK_BIT_EXIT_IF_SET(hhdr, displ, exit_label); \
    GC_STORE_BACK_PTR((ptr_t)source, (ptr_t)HBLKPTR(current) \
				      + WORDS_TO_BYTES(displ)); \
    PUSH_OBJ(((word *)(HBLKPTR(current)) + displ), hhdr, \
    	     mark_stack_top, mark_stack_limit) \
}

#if defined(PRINT_BLACK_LIST) || defined(KEEP_BACK_PTRS)
#   define PUSH_ONE_CHECKED_STACK(p, source) \
	GC_mark_and_push_stack(p, (ptr_t)(source))
#else
#   define PUSH_ONE_CHECKED_STACK(p, source) \
	GC_mark_and_push_stack(p)
#endif

/*
 * Push a single value onto mark stack. Mark from the object pointed to by p.
 * Invoke FIXUP_POINTER(p) before any further processing.
 * P is considered valid even if it is an interior pointer.
 * Previously marked objects are not pushed.  Hence we make progress even
 * if the mark stack overflows.
 */

# if NEED_FIXUP_POINTER
    /* Try both the raw version and the fixed up one.	*/
#   define GC_PUSH_ONE_STACK(p, source) \
      if ((ptr_t)(p) >= (ptr_t)GC_least_plausible_heap_addr 	\
	 && (ptr_t)(p) < (ptr_t)GC_greatest_plausible_heap_addr) {	\
	 PUSH_ONE_CHECKED_STACK(p, source);	\
      } \
      FIXUP_POINTER(p); \
      if ((ptr_t)(p) >= (ptr_t)GC_least_plausible_heap_addr 	\
	 && (ptr_t)(p) < (ptr_t)GC_greatest_plausible_heap_addr) {	\
	 PUSH_ONE_CHECKED_STACK(p, source);	\
      }
# else /* !NEED_FIXUP_POINTER */
#   define GC_PUSH_ONE_STACK(p, source) \
      if ((ptr_t)(p) >= (ptr_t)GC_least_plausible_heap_addr 	\
	 && (ptr_t)(p) < (ptr_t)GC_greatest_plausible_heap_addr) {	\
	 PUSH_ONE_CHECKED_STACK(p, source);	\
      }
# endif


/*
 * As above, but interior pointer recognition as for
 * normal for heap pointers.
 */
# define GC_PUSH_ONE_HEAP(p,source) \
    FIXUP_POINTER(p); \
    if ((ptr_t)(p) >= (ptr_t)GC_least_plausible_heap_addr 	\
	 && (ptr_t)(p) < (ptr_t)GC_greatest_plausible_heap_addr) {	\
	    GC_mark_stack_top = GC_mark_and_push( \
			    (GC_PTR)(p), GC_mark_stack_top, \
			    GC_mark_stack_limit, (GC_PTR *)(source)); \
    }

/* Mark starting at mark stack entry top (incl.) down to	*/
/* mark stack entry bottom (incl.).  Stop after performing	*/
/* about one page worth of work.  Return the new mark stack	*/
/* top entry.							*/
mse * GC_mark_from GC_PROTO((mse * top, mse * bottom, mse *limit));

#define MARK_FROM_MARK_STACK() \
	GC_mark_stack_top = GC_mark_from(GC_mark_stack_top, \
					 GC_mark_stack, \
					 GC_mark_stack + GC_mark_stack_size);

/*
 * Mark from one finalizable object using the specified
 * mark proc. May not mark the object pointed to by 
 * real_ptr. That is the job of the caller, if appropriate
 */
# define GC_MARK_FO(real_ptr, mark_proc) \
{ \
    (*(mark_proc))(real_ptr); \
    while (!GC_mark_stack_empty()) MARK_FROM_MARK_STACK(); \
    if (GC_mark_state != MS_NONE) { \
        GC_set_mark_bit(real_ptr); \
        while (!GC_mark_some((ptr_t)0)) {} \
    } \
}

extern GC_bool GC_mark_stack_too_small;
				/* We need a larger mark stack.  May be	*/
				/* set by client supplied mark routines.*/

typedef int mark_state_t;	/* Current state of marking, as follows:*/
				/* Used to remember where we are during */
				/* concurrent marking.			*/

				/* We say something is dirty if it was	*/
				/* written since the last time we	*/
				/* retrieved dirty bits.  We say it's 	*/
				/* grungy if it was marked dirty in the	*/
				/* last set of bits we retrieved.	*/
				
				/* Invariant I: all roots and marked	*/
				/* objects p are either dirty, or point */
				/* to objects q that are either marked 	*/
				/* or a pointer to q appears in a range	*/
				/* on the mark stack.			*/

# define MS_NONE 0		/* No marking in progress. I holds.	*/
				/* Mark stack is empty.			*/

# define MS_PUSH_RESCUERS 1	/* Rescuing objects are currently 	*/
				/* being pushed.  I holds, except	*/
				/* that grungy roots may point to 	*/
				/* unmarked objects, as may marked	*/
				/* grungy objects above scan_ptr.	*/

# define MS_PUSH_UNCOLLECTABLE 2
				/* I holds, except that marked 		*/
				/* uncollectable objects above scan_ptr */
				/* may point to unmarked objects.	*/
				/* Roots may point to unmarked objects	*/

# define MS_ROOTS_PUSHED 3	/* I holds, mark stack may be nonempty  */

# define MS_PARTIALLY_INVALID 4	/* I may not hold, e.g. because of M.S. */
				/* overflow.  However marked heap	*/
				/* objects below scan_ptr point to	*/
				/* marked or stacked objects.		*/

# define MS_INVALID 5		/* I may not hold.			*/

extern mark_state_t GC_mark_state;

#endif  /* GC_PMARK_H */

