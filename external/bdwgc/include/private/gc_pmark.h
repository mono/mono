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
 */
#ifndef GC_PMARK_H
#define GC_PMARK_H

#if defined(HAVE_CONFIG_H) && !defined(GC_PRIVATE_H)
# include "config.h"
#endif

#ifndef GC_BUILD
# define GC_BUILD
#endif

#if (defined(__linux__) || defined(__GLIBC__) || defined(__GNU__)) \
    && !defined(_GNU_SOURCE) && defined(GC_PTHREADS) \
    && !defined(GC_NO_PTHREAD_SIGMASK)
# define _GNU_SOURCE 1
#endif

#if defined(KEEP_BACK_PTRS) || defined(PRINT_BLACK_LIST)
# include "dbg_mlc.h"
#endif

#ifndef GC_MARK_H
# include "../gc_mark.h"
#endif

#ifndef GC_PRIVATE_H
# include "gc_priv.h"
#endif

EXTERN_C_BEGIN

/* The real declarations of the following is in gc_priv.h, so that      */
/* we can avoid scanning the following table.                           */
/*
mark_proc GC_mark_procs[MAX_MARK_PROCS];
*/

#ifndef MARK_DESCR_OFFSET
# define MARK_DESCR_OFFSET sizeof(word)
#endif

/*
 * Mark descriptor stuff that should remain private for now, mostly
 * because it's hard to export WORDSZ without including gcconfig.h.
 */
#define BITMAP_BITS (WORDSZ - GC_DS_TAG_BITS)
#define PROC(descr) \
      (GC_mark_procs[((descr) >> GC_DS_TAG_BITS) & (GC_MAX_MARK_PROCS-1)])
#define ENV(descr) \
      ((descr) >> (GC_DS_TAG_BITS + GC_LOG_MAX_MARK_PROCS))
#define MAX_ENV \
      (((word)1 << (WORDSZ - GC_DS_TAG_BITS - GC_LOG_MAX_MARK_PROCS)) - 1)

GC_EXTERN unsigned GC_n_mark_procs;

/* Number of mark stack entries to discard on overflow. */
#define GC_MARK_STACK_DISCARDS (INITIAL_MARK_STACK_SIZE/8)

GC_EXTERN size_t GC_mark_stack_size;

#ifdef PARALLEL_MARK
    /*
     * Allow multiple threads to participate in the marking process.
     * This works roughly as follows:
     *  The main mark stack never shrinks, but it can grow.
     *
     *  The initiating threads holds the GC lock, and sets GC_help_wanted.
     *
     *  Other threads:
     *     1) update helper_count (while holding mark_lock.)
     *     2) allocate a local mark stack
     *     repeatedly:
     *          3) Steal a global mark stack entry by atomically replacing
     *             its descriptor with 0.
     *          4) Copy it to the local stack.
     *          5) Mark on the local stack until it is empty, or
     *             it may be profitable to copy it back.
     *          6) If necessary, copy local stack to global one,
     *             holding mark lock.
     *    7) Stop when the global mark stack is empty.
     *    8) decrement helper_count (holding mark_lock).
     *
     * This is an experiment to see if we can do something along the lines
     * of the University of Tokyo SGC in a less intrusive, though probably
     * also less performant, way.
     */

    /* GC_mark_stack_top is protected by mark lock.     */

    /*
     * GC_notify_all_marker() is used when GC_help_wanted is first set,
     * when the last helper becomes inactive,
     * when something is added to the global mark stack, and just after
     * GC_mark_no is incremented.
     * This could be split into multiple CVs (and probably should be to
     * scale to really large numbers of processors.)
     */
#endif /* PARALLEL_MARK */

GC_INNER mse * GC_signal_mark_stack_overflow(mse *msp);

/* Push the object obj with corresponding heap block header hhdr onto   */
/* the mark stack.                                                      */
#define PUSH_OBJ(obj, hhdr, mark_stack_top, mark_stack_limit) \
  do { \
    word _descr = (hhdr) -> hb_descr; \
    GC_ASSERT(!HBLK_IS_FREE(hhdr)); \
    if (_descr != 0) { \
        mark_stack_top++; \
        if ((word)mark_stack_top >= (word)(mark_stack_limit)) { \
          mark_stack_top = GC_signal_mark_stack_overflow(mark_stack_top); \
        } \
        mark_stack_top -> mse_start = (obj); \
        mark_stack_top -> mse_descr.w = _descr; \
    } \
  } while (0)

/* Push the contents of current onto the mark stack if it is a valid    */
/* ptr to a currently unmarked object.  Mark it.                        */
#define PUSH_CONTENTS(current, mark_stack_top, mark_stack_limit, source) \
  do { \
    hdr * my_hhdr; \
    HC_GET_HDR(current, my_hhdr, source); /* contains "break" */ \
    PUSH_CONTENTS_HDR(current, mark_stack_top, mark_stack_limit, \
                      source, my_hhdr, TRUE); \
  } while (0)

/* Set mark bit, exit (using "break" statement) if it is already set.   */
#ifdef USE_MARK_BYTES
# if defined(PARALLEL_MARK) && defined(AO_HAVE_char_store) \
     && !defined(AO_USE_PTHREAD_DEFS)
    /* There is a race here, and we may set the bit twice in the        */
    /* concurrent case.  This can result in the object being pushed     */
    /* twice.  But that is only a performance issue.                    */
#   define SET_MARK_BIT_EXIT_IF_SET(hhdr, bit_no) \
      { /* cannot use do-while(0) here */ \
        volatile unsigned char * mark_byte_addr = \
                        (unsigned char *)(hhdr)->hb_marks + (bit_no); \
        /* Unordered atomic load and store are sufficient here. */ \
        if (AO_char_load(mark_byte_addr) != 0) \
          break; /* go to the enclosing loop end */ \
        AO_char_store(mark_byte_addr, 1); \
      }
# else
#   define SET_MARK_BIT_EXIT_IF_SET(hhdr, bit_no) \
      { /* cannot use do-while(0) here */ \
        char * mark_byte_addr = (char *)(hhdr)->hb_marks + (bit_no); \
        if (*mark_byte_addr != 0) break; /* go to the enclosing loop end */ \
        *mark_byte_addr = 1; \
      }
# endif /* !PARALLEL_MARK */
#else
# ifdef PARALLEL_MARK
    /* This is used only if we explicitly set USE_MARK_BITS.            */
    /* The following may fail to exit even if the bit was already set.  */
    /* For our uses, that's benign:                                     */
#   ifdef THREAD_SANITIZER
#     define OR_WORD_EXIT_IF_SET(addr, bits) \
        { /* cannot use do-while(0) here */ \
          if (!((word)AO_load((volatile AO_t *)(addr)) & (bits))) { \
                /* Atomic load is just to avoid TSan false positive. */ \
            AO_or((volatile AO_t *)(addr), (AO_t)(bits)); \
          } else { \
            break; /* go to the enclosing loop end */ \
          } \
        }
#   else
#     define OR_WORD_EXIT_IF_SET(addr, bits) \
        { /* cannot use do-while(0) here */ \
          if (!(*(addr) & (bits))) { \
            AO_or((volatile AO_t *)(addr), (AO_t)(bits)); \
          } else { \
            break; /* go to the enclosing loop end */ \
          } \
        }
#   endif /* !THREAD_SANITIZER */
# else
#   define OR_WORD_EXIT_IF_SET(addr, bits) \
        { /* cannot use do-while(0) here */ \
           word old = *(addr); \
           word my_bits = (bits); \
           if ((old & my_bits) != 0) \
             break; /* go to the enclosing loop end */ \
           *(addr) = old | my_bits; \
        }
# endif /* !PARALLEL_MARK */
# define SET_MARK_BIT_EXIT_IF_SET(hhdr, bit_no) \
    { /* cannot use do-while(0) here */ \
        word * mark_word_addr = (hhdr)->hb_marks + divWORDSZ(bit_no); \
        OR_WORD_EXIT_IF_SET(mark_word_addr, \
                (word)1 << modWORDSZ(bit_no)); /* contains "break" */ \
    }
#endif /* !USE_MARK_BYTES */

#ifdef PARALLEL_MARK
# define INCR_MARKS(hhdr) \
                AO_store(&hhdr->hb_n_marks, AO_load(&hhdr->hb_n_marks) + 1)
#else
# define INCR_MARKS(hhdr) (void)(++hhdr->hb_n_marks)
#endif

#ifdef ENABLE_TRACE
# define TRACE(source, cmd) \
        if (GC_trace_addr != 0 && (ptr_t)(source) == GC_trace_addr) cmd
# define TRACE_TARGET(target, cmd) \
        if (GC_trace_addr != 0 && (target) == *(ptr_t *)GC_trace_addr) cmd
#else
# define TRACE(source, cmd)
# define TRACE_TARGET(source, cmd)
#endif

#if defined(I386) && defined(__GNUC__) && !defined(NACL)
# define LONG_MULT(hprod, lprod, x, y) \
    do { \
        __asm__ __volatile__("mull %2" : "=a"(lprod), "=d"(hprod) \
                             : "g"(y), "0"(x)); \
    } while (0)
#else
# if defined(__int64) && !defined(__GNUC__) && !defined(CPPCHECK)
#   define ULONG_MULT_T unsigned __int64
# else
#   define ULONG_MULT_T unsigned long long
# endif
# define LONG_MULT(hprod, lprod, x, y) \
    do { \
        ULONG_MULT_T prod = (ULONG_MULT_T)(x) * (ULONG_MULT_T)(y); \
        GC_STATIC_ASSERT(sizeof(x) + sizeof(y) <= sizeof(prod)); \
        hprod = prod >> 32; \
        lprod = (unsigned32)prod; \
    } while (0)
#endif /* !I386 */

/* If the mark bit corresponding to current is not set, set it, and     */
/* push the contents of the object on the mark stack.  Current points   */
/* to the beginning of the object.  We rely on the fact that the        */
/* preceding header calculation will succeed for a pointer past the     */
/* first page of an object, only if it is in fact a valid pointer       */
/* to the object.  Thus we can omit the otherwise necessary tests       */
/* here.  Note in particular that the "displ" value is the displacement */
/* from the beginning of the heap block, which may itself be in the     */
/* interior of a large object.                                          */
#ifdef MARK_BIT_PER_GRANULE
# define PUSH_CONTENTS_HDR(current, mark_stack_top, mark_stack_limit, \
                           source, hhdr, do_offset_check) \
  do { \
    size_t displ = HBLKDISPL(current); /* Displacement in block; in bytes. */\
    /* displ is always within range.  If current doesn't point to       */ \
    /* first block, then we are in the all_interior_pointers case, and  */ \
    /* it is safe to use any displacement value.                        */ \
    size_t gran_displ = BYTES_TO_GRANULES(displ); \
    size_t gran_offset = hhdr -> hb_map[gran_displ]; \
    size_t byte_offset = displ & (GRANULE_BYTES - 1); \
    ptr_t base = (ptr_t)(current); \
    /* The following always fails for large block references. */ \
    if (EXPECT((gran_offset | byte_offset) != 0, FALSE))  { \
        if ((hhdr -> hb_flags & LARGE_BLOCK) != 0) { \
          /* gran_offset is bogus.      */ \
          size_t obj_displ; \
          base = (ptr_t)(hhdr -> hb_block); \
          obj_displ = (ptr_t)(current) - base; \
          if (obj_displ != displ) { \
            GC_ASSERT(obj_displ < hhdr -> hb_sz); \
            /* Must be in all_interior_pointer case, not first block */ \
            /* already did validity check on cache miss.             */ \
          } else { \
            if (do_offset_check && !GC_valid_offsets[obj_displ]) { \
              GC_ADD_TO_BLACK_LIST_NORMAL(current, source); \
              break; /* go to the end of PUSH_CONTENTS_HDR */ \
            } \
          } \
          gran_displ = 0; \
          GC_ASSERT(hhdr -> hb_sz > HBLKSIZE || \
                    hhdr -> hb_block == HBLKPTR(current)); \
          GC_ASSERT((word)hhdr->hb_block <= (word)(current)); \
        } else { \
          size_t obj_displ = GRANULES_TO_BYTES(gran_offset) \
                             + byte_offset; \
          if (do_offset_check && !GC_valid_offsets[obj_displ]) { \
            GC_ADD_TO_BLACK_LIST_NORMAL(current, source); \
            break; \
          } \
          gran_displ -= gran_offset; \
          base -= obj_displ; \
        } \
    } \
    GC_ASSERT(hhdr == GC_find_header(base)); \
    GC_ASSERT(gran_displ % BYTES_TO_GRANULES(hhdr -> hb_sz) == 0); \
    TRACE(source, GC_log_printf("GC #%u: passed validity tests\n", \
                                (unsigned)GC_gc_no)); \
    SET_MARK_BIT_EXIT_IF_SET(hhdr, gran_displ); \
    TRACE(source, GC_log_printf("GC #%u: previously unmarked\n", \
                                (unsigned)GC_gc_no)); \
    TRACE_TARGET(base, \
        GC_log_printf("GC #%u: marking %p from %p instead\n", \
                      (unsigned)GC_gc_no, (void *)base, (void *)(source))); \
    INCR_MARKS(hhdr); \
    GC_STORE_BACK_PTR((ptr_t)(source), base); \
    PUSH_OBJ(base, hhdr, mark_stack_top, mark_stack_limit); \
  } while (0)
#endif /* MARK_BIT_PER_GRANULE */

#ifdef MARK_BIT_PER_OBJ
# define PUSH_CONTENTS_HDR(current, mark_stack_top, mark_stack_limit, \
                           source, hhdr, do_offset_check) \
  do { \
    size_t displ = HBLKDISPL(current); /* Displacement in block; in bytes. */\
    unsigned32 low_prod, high_prod; \
    unsigned32 inv_sz = hhdr -> hb_inv_sz; \
    ptr_t base = (ptr_t)(current); \
    LONG_MULT(high_prod, low_prod, (unsigned32)displ, inv_sz); \
    /* product is > and within sz_in_bytes of displ * sz_in_bytes * 2**32 */ \
    if (EXPECT(low_prod >> 16 != 0, FALSE)) { \
      /* FIXME: fails if offset is a multiple of HBLKSIZE which becomes 0 */ \
        if (inv_sz == LARGE_INV_SZ) { \
          size_t obj_displ; \
          base = (ptr_t)(hhdr -> hb_block); \
          obj_displ = (ptr_t)(current) - base; \
          if (obj_displ != displ) { \
            GC_ASSERT(obj_displ < hhdr -> hb_sz); \
            /* Must be in all_interior_pointer case, not first block */ \
            /* already did validity check on cache miss.             */ \
          } else { \
            if (do_offset_check && !GC_valid_offsets[obj_displ]) { \
              GC_ADD_TO_BLACK_LIST_NORMAL(current, source); \
              break; /* go to the end of PUSH_CONTENTS_HDR */ \
            } \
          } \
          GC_ASSERT(hhdr -> hb_sz > HBLKSIZE || \
                    hhdr -> hb_block == HBLKPTR(current)); \
          GC_ASSERT((word)hhdr->hb_block < (word)(current)); \
        } else { \
          size_t obj_displ; \
          /* Accurate enough if HBLKSIZE <= 2**15.      */ \
          GC_STATIC_ASSERT(HBLKSIZE <= (1 << 15)); \
          obj_displ = (((low_prod >> 16) + 1) * (size_t)hhdr->hb_sz) >> 16; \
          if (do_offset_check && !GC_valid_offsets[obj_displ]) { \
            GC_ADD_TO_BLACK_LIST_NORMAL(current, source); \
            break; \
          } \
          base -= obj_displ; \
        } \
    } \
    /* May get here for pointer to start of block not at        */ \
    /* beginning of object.  If so, it's valid, and we're fine. */ \
    GC_ASSERT(high_prod <= HBLK_OBJS(hhdr -> hb_sz)); \
    TRACE(source, GC_log_printf("GC #%u: passed validity tests\n", \
                                (unsigned)GC_gc_no)); \
    SET_MARK_BIT_EXIT_IF_SET(hhdr, high_prod); \
    TRACE(source, GC_log_printf("GC #%u: previously unmarked\n", \
                                (unsigned)GC_gc_no)); \
    TRACE_TARGET(base, \
        GC_log_printf("GC #%u: marking %p from %p instead\n", \
                      (unsigned)GC_gc_no, (void *)base, (void *)(source))); \
    INCR_MARKS(hhdr); \
    GC_STORE_BACK_PTR((ptr_t)(source), base); \
    PUSH_OBJ(base, hhdr, mark_stack_top, mark_stack_limit); \
  } while (0)
#endif /* MARK_BIT_PER_OBJ */

#if defined(PRINT_BLACK_LIST) || defined(KEEP_BACK_PTRS)
# define PUSH_ONE_CHECKED_STACK(p, source) \
        GC_mark_and_push_stack((ptr_t)(p), (ptr_t)(source))
#else
# define PUSH_ONE_CHECKED_STACK(p, source) \
        GC_mark_and_push_stack((ptr_t)(p))
#endif

/*
 * Push a single value onto mark stack. Mark from the object pointed to by p.
 * Invoke FIXUP_POINTER(p) before any further processing.
 * P is considered valid even if it is an interior pointer.
 * Previously marked objects are not pushed.  Hence we make progress even
 * if the mark stack overflows.
 */

#ifdef NEED_FIXUP_POINTER
    /* Try both the raw version and the fixed up one.   */
# define GC_PUSH_ONE_STACK(p, source) \
    do { \
      if ((word)(p) >= (word)GC_least_plausible_heap_addr \
          && (word)(p) < (word)GC_greatest_plausible_heap_addr) { \
         PUSH_ONE_CHECKED_STACK(p, source); \
      } \
      FIXUP_POINTER(p); \
      if ((word)(p) >= (word)GC_least_plausible_heap_addr \
          && (word)(p) < (word)GC_greatest_plausible_heap_addr) { \
         PUSH_ONE_CHECKED_STACK(p, source); \
      } \
    } while (0)
#else /* !NEED_FIXUP_POINTER */
# define GC_PUSH_ONE_STACK(p, source) \
    do { \
      if ((word)(p) >= (word)GC_least_plausible_heap_addr \
          && (word)(p) < (word)GC_greatest_plausible_heap_addr) { \
         PUSH_ONE_CHECKED_STACK(p, source); \
      } \
    } while (0)
#endif

/* As above, but interior pointer recognition as for normal heap pointers. */
#define GC_PUSH_ONE_HEAP(p,source,mark_stack_top) \
    do { \
      FIXUP_POINTER(p); \
      if ((word)(p) >= (word)GC_least_plausible_heap_addr \
          && (word)(p) < (word)GC_greatest_plausible_heap_addr) \
        mark_stack_top = GC_mark_and_push((void *)(p), mark_stack_top, \
                                GC_mark_stack_limit, (void * *)(source)); \
    } while (0)

/* Mark starting at mark stack entry top (incl.) down to        */
/* mark stack entry bottom (incl.).  Stop after performing      */
/* about one page worth of work.  Return the new mark stack     */
/* top entry.                                                   */
GC_INNER mse * GC_mark_from(mse * top, mse * bottom, mse *limit);

#define MARK_FROM_MARK_STACK() \
        GC_mark_stack_top = GC_mark_from(GC_mark_stack_top, \
                                         GC_mark_stack, \
                                         GC_mark_stack + GC_mark_stack_size);

#define GC_mark_stack_empty() ((word)GC_mark_stack_top < (word)GC_mark_stack)

/*
 * Mark from one finalizable object using the specified
 * mark proc. May not mark the object pointed to by
 * real_ptr. That is the job of the caller, if appropriate.
 * Note that this is called with the mutator running, but
 * with us holding the allocation lock.  This is safe only if the
 * mutator needs the allocation lock to reveal hidden pointers.
 * FIXME: Why do we need the GC_mark_state test below?
 */
#define GC_MARK_FO(real_ptr, mark_proc) \
  do { \
    (*(mark_proc))(real_ptr); \
    while (!GC_mark_stack_empty()) MARK_FROM_MARK_STACK(); \
    if (GC_mark_state != MS_NONE) { \
        GC_set_mark_bit(real_ptr); \
        while (!GC_mark_some((ptr_t)0)) { /* empty */ } \
    } \
  } while (0)

GC_EXTERN GC_bool GC_mark_stack_too_small;
                                /* We need a larger mark stack.  May be */
                                /* set by client supplied mark routines.*/

typedef int mark_state_t;       /* Current state of marking, as follows:*/
                                /* Used to remember where we are during */
                                /* concurrent marking.                  */

                                /* We say something is dirty if it was  */
                                /* written since the last time we       */
                                /* retrieved dirty bits.  We say it's   */
                                /* grungy if it was marked dirty in the */
                                /* last set of bits we retrieved.       */

                                /* Invariant "I": all roots and marked  */
                                /* objects p are either dirty, or point */
                                /* to objects q that are either marked  */
                                /* or a pointer to q appears in a range */
                                /* on the mark stack.                   */

#define MS_NONE 0               /* No marking in progress. "I" holds.   */
                                /* Mark stack is empty.                 */

#define MS_PUSH_RESCUERS 1      /* Rescuing objects are currently       */
                                /* being pushed.  "I" holds, except     */
                                /* that grungy roots may point to       */
                                /* unmarked objects, as may marked      */
                                /* grungy objects above scan_ptr.       */

#define MS_PUSH_UNCOLLECTABLE 2 /* "I" holds, except that marked        */
                                /* uncollectible objects above scan_ptr */
                                /* may point to unmarked objects.       */
                                /* Roots may point to unmarked objects  */

#define MS_ROOTS_PUSHED 3       /* "I" holds, mark stack may be nonempty. */

#define MS_PARTIALLY_INVALID 4  /* "I" may not hold, e.g. because of    */
                                /* the mark stack overflow.  However    */
                                /* marked heap objects below scan_ptr   */
                                /* point to marked or stacked objects.  */

#define MS_INVALID 5            /* "I" may not hold.                    */

GC_EXTERN mark_state_t GC_mark_state;

EXTERN_C_END

#endif  /* GC_PMARK_H */
