/*
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1995 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1997 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 1999-2004 Hewlett-Packard Development Company, L.P.
 * Copyright (C) 2007 Free Software Foundation, Inc
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

#include "private/dbg_mlc.h"

#ifndef MSWINCE
# include <errno.h>
#endif
#include <string.h>

#ifndef SHORT_DBG_HDRS
  /* Check whether object with base pointer p has debugging info. */
  /* p is assumed to point to a legitimate object in our part     */
  /* of the heap.                                                 */
  /* This excludes the check as to whether the back pointer is    */
  /* odd, which is added by the GC_HAS_DEBUG_INFO macro.          */
  /* Note that if DBG_HDRS_ALL is set, uncollectible objects      */
  /* on free lists may not have debug information set.  Thus it's */
  /* not always safe to return TRUE (1), even if the client does  */
  /* its part.  Return -1 if the object with debug info has been  */
  /* marked as deallocated.                                       */
  GC_INNER int GC_has_other_debug_info(ptr_t p)
  {
    ptr_t body = (ptr_t)((oh *)p + 1);
    word sz = GC_size(p);

    if (HBLKPTR(p) != HBLKPTR((ptr_t)body)
        || sz < DEBUG_BYTES + EXTRA_BYTES) {
      return 0;
    }
    if (((oh *)p) -> oh_sf != (START_FLAG ^ (word)body)
        && ((word *)p)[BYTES_TO_WORDS(sz)-1] != (END_FLAG ^ (word)body)) {
      return 0;
    }
    if (((oh *)p)->oh_sz == sz) {
      /* Object may have had debug info, but has been deallocated     */
      return -1;
    }
    return 1;
  }
#endif /* !SHORT_DBG_HDRS */

#ifdef LINT2
  long GC_random(void)
  {
    static unsigned seed = 1; /* not thread-safe */

    /* Linear congruential pseudo-random numbers generator.     */
    seed = (seed * 1103515245U + 12345) & GC_RAND_MAX; /* overflow is ok */
    return (long)seed;
  }
#endif

#ifdef KEEP_BACK_PTRS

#ifdef LINT2
# define RANDOM() GC_random()
#else
# include <stdlib.h>
# define GC_RAND_MAX RAND_MAX

# if defined(__GLIBC__) || defined(SOLARIS) \
     || defined(HPUX) || defined(IRIX5) || defined(OSF1)
#   define RANDOM() random()
# else
#   define RANDOM() (long)rand()
# endif
#endif /* !LINT2 */

  /* Store back pointer to source in dest, if that appears to be possible. */
  /* This is not completely safe, since we may mistakenly conclude that    */
  /* dest has a debugging wrapper.  But the error probability is very      */
  /* small, and this shouldn't be used in production code.                 */
  /* We assume that dest is the real base pointer.  Source will usually    */
  /* be a pointer to the interior of an object.                            */
  GC_INNER void GC_store_back_pointer(ptr_t source, ptr_t dest)
  {
    if (GC_HAS_DEBUG_INFO(dest)) {
#     ifdef PARALLEL_MARK
        AO_store((volatile AO_t *)&((oh *)dest)->oh_back_ptr,
                 (AO_t)HIDE_BACK_PTR(source));
#     else
        ((oh *)dest) -> oh_back_ptr = HIDE_BACK_PTR(source);
#     endif
    }
  }

  GC_INNER void GC_marked_for_finalization(ptr_t dest)
  {
    GC_store_back_pointer(MARKED_FOR_FINALIZATION, dest);
  }

  /* Store information about the object referencing dest in *base_p     */
  /* and *offset_p.                                                     */
  /*   source is root ==> *base_p = address, *offset_p = 0              */
  /*   source is heap object ==> *base_p != 0, *offset_p = offset       */
  /*   Returns 1 on success, 0 if source couldn't be determined.        */
  /* Dest can be any address within a heap object.                      */
  GC_API GC_ref_kind GC_CALL GC_get_back_ptr_info(void *dest, void **base_p,
                                                  size_t *offset_p)
  {
    oh * hdr = (oh *)GC_base(dest);
    ptr_t bp;
    ptr_t bp_base;

#   ifdef LINT2
      /* Explicitly instruct the code analysis tool that                */
      /* GC_get_back_ptr_info is not expected to be called with an      */
      /* incorrect "dest" value.                                        */
      if (!hdr) ABORT("Invalid GC_get_back_ptr_info argument");
#   endif
    if (!GC_HAS_DEBUG_INFO((ptr_t) hdr)) return GC_NO_SPACE;
    bp = (ptr_t)GC_REVEAL_POINTER(hdr -> oh_back_ptr);
    if (MARKED_FOR_FINALIZATION == bp) return GC_FINALIZER_REFD;
    if (MARKED_FROM_REGISTER == bp) return GC_REFD_FROM_REG;
    if (NOT_MARKED == bp) return GC_UNREFERENCED;
#   if ALIGNMENT == 1
      /* Heuristically try to fix off by 1 errors we introduced by      */
      /* insisting on even addresses.                                   */
      {
        ptr_t alternate_ptr = bp + 1;
        ptr_t target = *(ptr_t *)bp;
        ptr_t alternate_target = *(ptr_t *)alternate_ptr;

        if ((word)alternate_target >= (word)GC_least_plausible_heap_addr
            && (word)alternate_target <= (word)GC_greatest_plausible_heap_addr
            && ((word)target < (word)GC_least_plausible_heap_addr
                || (word)target > (word)GC_greatest_plausible_heap_addr)) {
            bp = alternate_ptr;
        }
      }
#   endif
    bp_base = (ptr_t)GC_base(bp);
    if (NULL == bp_base) {
      *base_p = bp;
      *offset_p = 0;
      return GC_REFD_FROM_ROOT;
    } else {
      if (GC_HAS_DEBUG_INFO(bp_base)) bp_base += sizeof(oh);
      *base_p = bp_base;
      *offset_p = bp - bp_base;
      return GC_REFD_FROM_HEAP;
    }
  }

  /* Generate a random heap address.            */
  /* The resulting address is in the heap, but  */
  /* not necessarily inside a valid object.     */
  GC_API void * GC_CALL GC_generate_random_heap_address(void)
  {
    size_t i;
    word heap_offset = RANDOM();

    if (GC_heapsize > GC_RAND_MAX) {
        heap_offset *= GC_RAND_MAX;
        heap_offset += RANDOM();
    }
    heap_offset %= GC_heapsize;
        /* This doesn't yield a uniform distribution, especially if     */
        /* e.g. RAND_MAX = 1.5* GC_heapsize.  But for typical cases,    */
        /* it's not too bad.                                            */
    for (i = 0;; ++i) {
        size_t size;

        if (i >= GC_n_heap_sects)
          ABORT("GC_generate_random_heap_address: size inconsistency");

        size = GC_heap_sects[i].hs_bytes;
        if (heap_offset < size) {
            break;
        } else {
            heap_offset -= size;
        }
    }
    return GC_heap_sects[i].hs_start + heap_offset;
  }

  /* Generate a random address inside a valid marked heap object. */
  GC_API void * GC_CALL GC_generate_random_valid_address(void)
  {
    ptr_t result;
    ptr_t base;
    do {
      result = (ptr_t)GC_generate_random_heap_address();
      base = (ptr_t)GC_base(result);
    } while (NULL == base || !GC_is_marked(base));
    return result;
  }

  /* Print back trace for p */
  GC_API void GC_CALL GC_print_backtrace(void *p)
  {
    void *current = p;
    int i;
    GC_ref_kind source;
    size_t offset;
    void *base;

    GC_print_heap_obj((ptr_t)GC_base(current));

    for (i = 0; ; ++i) {
      source = GC_get_back_ptr_info(current, &base, &offset);
      if (GC_UNREFERENCED == source) {
        GC_err_printf("Reference could not be found\n");
        goto out;
      }
      if (GC_NO_SPACE == source) {
        GC_err_printf("No debug info in object: Can't find reference\n");
        goto out;
      }
      GC_err_printf("Reachable via %d levels of pointers from ", i);
      switch(source) {
        case GC_REFD_FROM_ROOT:
          GC_err_printf("root at %p\n\n", base);
          goto out;
        case GC_REFD_FROM_REG:
          GC_err_printf("root in register\n\n");
          goto out;
        case GC_FINALIZER_REFD:
          GC_err_printf("list of finalizable objects\n\n");
          goto out;
        case GC_REFD_FROM_HEAP:
          GC_err_printf("offset %ld in object:\n", (long)offset);
          /* Take GC_base(base) to get real base, i.e. header. */
          GC_print_heap_obj((ptr_t)GC_base(base));
          break;
        default:
          GC_err_printf("INTERNAL ERROR: UNEXPECTED SOURCE!!!!\n");
          goto out;
      }
      current = base;
    }
    out:;
  }

  /* Force a garbage collection and generate/print a backtrace  */
  /* from a random heap address.                                */
  GC_INNER void GC_generate_random_backtrace_no_gc(void)
  {
    void * current;
    current = GC_generate_random_valid_address();
    GC_printf("\n****Chosen address %p in object\n", current);
    GC_print_backtrace(current);
  }

  GC_API void GC_CALL GC_generate_random_backtrace(void)
  {
    if (GC_try_to_collect(GC_never_stop_func) == 0) {
      GC_err_printf("Cannot generate a backtrace: "
                    "garbage collection is disabled!\n");
      return;
    }
    GC_generate_random_backtrace_no_gc();
  }

#endif /* KEEP_BACK_PTRS */

# define CROSSES_HBLK(p, sz) \
        (((word)((p) + sizeof(oh) + (sz) - 1) ^ (word)(p)) >= HBLKSIZE)

GC_INNER void *GC_store_debug_info_inner(void *p, word sz GC_ATTR_UNUSED,
                                         const char *string, int linenum)
{
    word * result = (word *)((oh *)p + 1);

    GC_ASSERT(I_HOLD_LOCK());
    GC_ASSERT(GC_size(p) >= sizeof(oh) + sz);
    GC_ASSERT(!(SMALL_OBJ(sz) && CROSSES_HBLK((ptr_t)p, sz)));
#   ifdef KEEP_BACK_PTRS
      ((oh *)p) -> oh_back_ptr = HIDE_BACK_PTR(NOT_MARKED);
#   endif
#   ifdef MAKE_BACK_GRAPH
      ((oh *)p) -> oh_bg_ptr = HIDE_BACK_PTR((ptr_t)0);
#   endif
    ((oh *)p) -> oh_string = string;
    ((oh *)p) -> oh_int = (word)linenum;
#   ifndef SHORT_DBG_HDRS
      ((oh *)p) -> oh_sz = sz;
      ((oh *)p) -> oh_sf = START_FLAG ^ (word)result;
      ((word *)p)[BYTES_TO_WORDS(GC_size(p))-1] =
         result[SIMPLE_ROUNDED_UP_WORDS(sz)] = END_FLAG ^ (word)result;
#   endif
    return result;
}

/* Check the allocation is successful, store debugging info into p,     */
/* start the debugging mode (if not yet), and return displaced pointer. */
static void *store_debug_info(void *p, size_t lb,
                              const char *fn, GC_EXTRA_PARAMS)
{
    void *result;
    DCL_LOCK_STATE;

    if (NULL == p) {
        GC_err_printf("%s(%lu) returning NULL (%s:%d)\n",
                      fn, (unsigned long)lb, s, i);
        return NULL;
    }
    LOCK();
    if (!GC_debugging_started)
        GC_start_debugging_inner();
    ADD_CALL_CHAIN(p, ra);
    result = GC_store_debug_info_inner(p, (word)lb, s, i);
    UNLOCK();
    return result;
}

#ifndef SHORT_DBG_HDRS
  /* Check the object with debugging info at ohdr.      */
  /* Return NULL if it's OK.  Else return clobbered     */
  /* address.                                           */
  STATIC ptr_t GC_check_annotated_obj(oh *ohdr)
  {
    ptr_t body = (ptr_t)(ohdr + 1);
    word gc_sz = GC_size((ptr_t)ohdr);
    if (ohdr -> oh_sz + DEBUG_BYTES > gc_sz) {
        return((ptr_t)(&(ohdr -> oh_sz)));
    }
    if (ohdr -> oh_sf != (START_FLAG ^ (word)body)) {
        return((ptr_t)(&(ohdr -> oh_sf)));
    }
    if (((word *)ohdr)[BYTES_TO_WORDS(gc_sz)-1] != (END_FLAG ^ (word)body)) {
        return((ptr_t)((word *)ohdr + BYTES_TO_WORDS(gc_sz)-1));
    }
    if (((word *)body)[SIMPLE_ROUNDED_UP_WORDS(ohdr -> oh_sz)]
        != (END_FLAG ^ (word)body)) {
        return((ptr_t)((word *)body + SIMPLE_ROUNDED_UP_WORDS(ohdr->oh_sz)));
    }
    return(0);
  }
#endif /* !SHORT_DBG_HDRS */

STATIC GC_describe_type_fn GC_describe_type_fns[MAXOBJKINDS] = {0};

GC_API void GC_CALL GC_register_describe_type_fn(int kind,
                                                 GC_describe_type_fn fn)
{
  GC_describe_type_fns[kind] = fn;
}

#define GET_OH_LINENUM(ohdr) ((int)(ohdr)->oh_int)

#ifndef SHORT_DBG_HDRS
# define IF_NOT_SHORTDBG_HDRS(x) x
# define COMMA_IFNOT_SHORTDBG_HDRS(x) /* comma */, x
#else
# define IF_NOT_SHORTDBG_HDRS(x) /* empty */
# define COMMA_IFNOT_SHORTDBG_HDRS(x) /* empty */
#endif

/* Print a human-readable description of the object to stderr.          */
/* p points to somewhere inside an object with the debugging info.      */
STATIC void GC_print_obj(ptr_t p)
{
    oh * ohdr = (oh *)GC_base(p);
    ptr_t q;
    hdr * hhdr;
    int kind;
    const char *kind_str;
    char buffer[GC_TYPE_DESCR_LEN + 1];

    GC_ASSERT(I_DONT_HOLD_LOCK());
#   ifdef LINT2
      if (!ohdr) ABORT("Invalid GC_print_obj argument");
#   endif

    q = (ptr_t)(ohdr + 1);
    /* Print a type description for the object whose client-visible     */
    /* address is q.                                                    */
    hhdr = GC_find_header(q);
    kind = hhdr -> hb_obj_kind;
    if (0 != GC_describe_type_fns[kind] && GC_is_marked(ohdr)) {
        /* This should preclude free list objects except with   */
        /* thread-local allocation.                             */
        buffer[GC_TYPE_DESCR_LEN] = 0;
        (GC_describe_type_fns[kind])(q, buffer);
        GC_ASSERT(buffer[GC_TYPE_DESCR_LEN] == 0);
        kind_str = buffer;
    } else {
        switch(kind) {
          case PTRFREE:
            kind_str = "PTRFREE";
            break;
          case NORMAL:
            kind_str = "NORMAL";
            break;
          case UNCOLLECTABLE:
            kind_str = "UNCOLLECTABLE";
            break;
#         ifdef GC_ATOMIC_UNCOLLECTABLE
            case AUNCOLLECTABLE:
              kind_str = "ATOMIC_UNCOLLECTABLE";
              break;
#         endif
          default:
            kind_str = NULL;
                /* The alternative is to use snprintf(buffer) but it is */
                /* not quite portable (see vsnprintf in misc.c).        */
        }
    }

    if (NULL != kind_str) {
        GC_err_printf("%p (%s:%d," IF_NOT_SHORTDBG_HDRS(" sz=%lu,") " %s)\n",
                      (void *)((ptr_t)ohdr + sizeof(oh)),
                      ohdr->oh_string, GET_OH_LINENUM(ohdr) /*, */
                      COMMA_IFNOT_SHORTDBG_HDRS((unsigned long)ohdr->oh_sz),
                      kind_str);
    } else {
        GC_err_printf("%p (%s:%d," IF_NOT_SHORTDBG_HDRS(" sz=%lu,")
                      " kind=%d descr=0x%lx)\n",
                      (void *)((ptr_t)ohdr + sizeof(oh)),
                      ohdr->oh_string, GET_OH_LINENUM(ohdr) /*, */
                      COMMA_IFNOT_SHORTDBG_HDRS((unsigned long)ohdr->oh_sz),
                      kind, (unsigned long)hhdr->hb_descr);
    }
    PRINT_CALL_CHAIN(ohdr);
}

STATIC void GC_debug_print_heap_obj_proc(ptr_t p)
{
    GC_ASSERT(I_DONT_HOLD_LOCK());
    if (GC_HAS_DEBUG_INFO(p)) {
        GC_print_obj(p);
    } else {
        GC_default_print_heap_obj_proc(p);
    }
}

#ifndef SHORT_DBG_HDRS
  /* Use GC_err_printf and friends to print a description of the object */
  /* whose client-visible address is p, and which was smashed at        */
  /* clobbered_addr.                                                    */
  STATIC void GC_print_smashed_obj(const char *msg, void *p,
                                   ptr_t clobbered_addr)
  {
    oh * ohdr = (oh *)GC_base(p);

    GC_ASSERT(I_DONT_HOLD_LOCK());
#   ifdef LINT2
      if (!ohdr) ABORT("Invalid GC_print_smashed_obj argument");
#   endif
    if ((word)clobbered_addr <= (word)(&ohdr->oh_sz)
        || ohdr -> oh_string == 0) {
        GC_err_printf(
                "%s %p in or near object at %p(<smashed>, appr. sz = %lu)\n",
                msg, (void *)clobbered_addr, p,
                (unsigned long)(GC_size((ptr_t)ohdr) - DEBUG_BYTES));
    } else {
        GC_err_printf("%s %p in or near object at %p (%s:%d, sz=%lu)\n",
                msg, (void *)clobbered_addr, p,
                (word)(ohdr -> oh_string) < HBLKSIZE ? "(smashed string)" :
                ohdr -> oh_string[0] == '\0' ? "EMPTY(smashed?)" :
                                                ohdr -> oh_string,
                GET_OH_LINENUM(ohdr), (unsigned long)(ohdr -> oh_sz));
        PRINT_CALL_CHAIN(ohdr);
    }
  }

  STATIC void GC_check_heap_proc (void);
  STATIC void GC_print_all_smashed_proc (void);
#else
  STATIC void GC_do_nothing(void) {}
#endif

GC_INNER void GC_start_debugging_inner(void)
{
  GC_ASSERT(I_HOLD_LOCK());
# ifndef SHORT_DBG_HDRS
    GC_check_heap = GC_check_heap_proc;
    GC_print_all_smashed = GC_print_all_smashed_proc;
# else
    GC_check_heap = GC_do_nothing;
    GC_print_all_smashed = GC_do_nothing;
# endif
  GC_print_heap_obj = GC_debug_print_heap_obj_proc;
  GC_debugging_started = TRUE;
  GC_register_displacement_inner((word)sizeof(oh));
}

size_t GC_debug_header_size = sizeof(oh);

GC_API void GC_CALL GC_debug_register_displacement(size_t offset)
{
  DCL_LOCK_STATE;

  LOCK();
  GC_register_displacement_inner(offset);
  GC_register_displacement_inner((word)sizeof(oh) + offset);
  UNLOCK();
}

#ifdef GC_ADD_CALLER
# if defined(HAVE_DLADDR) && defined(GC_HAVE_RETURN_ADDR_PARENT)
#   include <dlfcn.h>

    STATIC void GC_caller_func_offset(word ad, const char **symp, int *offp)
    {
      Dl_info caller;

      if (ad && dladdr((void *)ad, &caller) && caller.dli_sname != NULL) {
        *symp = caller.dli_sname;
        *offp = (int)((char *)ad - (char *)caller.dli_saddr);
      }
      if (NULL == *symp) {
        *symp = "unknown";
      }
    }
# else
#   define GC_caller_func_offset(ad, symp, offp) (void)(*(symp) = "unknown")
# endif
#endif /* GC_ADD_CALLER */

GC_API GC_ATTR_MALLOC void * GC_CALL GC_debug_malloc(size_t lb,
                                                     GC_EXTRA_PARAMS)
{
    void * result;

    /* Note that according to malloc() specification, if size is 0 then */
    /* malloc() returns either NULL, or a unique pointer value that can */
    /* later be successfully passed to free(). We always do the latter. */
    result = GC_malloc(SIZET_SAT_ADD(lb, DEBUG_BYTES));
#   ifdef GC_ADD_CALLER
      if (s == NULL) {
        GC_caller_func_offset(ra, &s, &i);
      }
#   endif
    return store_debug_info(result, lb, "GC_debug_malloc", OPT_RA s, i);
}

GC_API GC_ATTR_MALLOC void * GC_CALL
    GC_debug_malloc_ignore_off_page(size_t lb, GC_EXTRA_PARAMS)
{
    void * result = GC_malloc_ignore_off_page(SIZET_SAT_ADD(lb, DEBUG_BYTES));

    return store_debug_info(result, lb, "GC_debug_malloc_ignore_off_page",
                            OPT_RA s, i);
}

GC_API GC_ATTR_MALLOC void * GC_CALL
    GC_debug_malloc_atomic_ignore_off_page(size_t lb, GC_EXTRA_PARAMS)
{
    void * result = GC_malloc_atomic_ignore_off_page(
                                SIZET_SAT_ADD(lb, DEBUG_BYTES));

    return store_debug_info(result, lb,
                            "GC_debug_malloc_atomic_ignore_off_page",
                            OPT_RA s, i);
}

STATIC void * GC_debug_generic_malloc(size_t lb, int knd, GC_EXTRA_PARAMS)
{
    void * result = GC_generic_malloc(SIZET_SAT_ADD(lb, DEBUG_BYTES), knd);

    return store_debug_info(result, lb, "GC_debug_generic_malloc",
                            OPT_RA s, i);
}

#ifdef DBG_HDRS_ALL
  /* An allocation function for internal use.  Normally internally      */
  /* allocated objects do not have debug information.  But in this      */
  /* case, we need to make sure that all objects have debug headers.    */
  GC_INNER void * GC_debug_generic_malloc_inner(size_t lb, int k)
  {
    void * result;

    GC_ASSERT(I_HOLD_LOCK());
    result = GC_generic_malloc_inner(SIZET_SAT_ADD(lb, DEBUG_BYTES), k);
    if (NULL == result) {
        GC_err_printf("GC internal allocation (%lu bytes) returning NULL\n",
                       (unsigned long) lb);
        return(0);
    }
    if (!GC_debugging_started) {
        GC_start_debugging_inner();
    }
    ADD_CALL_CHAIN(result, GC_RETURN_ADDR);
    return (GC_store_debug_info_inner(result, (word)lb, "INTERNAL", 0));
  }

  GC_INNER void * GC_debug_generic_malloc_inner_ignore_off_page(size_t lb,
                                                                int k)
  {
    void * result;

    GC_ASSERT(I_HOLD_LOCK());
    result = GC_generic_malloc_inner_ignore_off_page(
                                SIZET_SAT_ADD(lb, DEBUG_BYTES), k);
    if (NULL == result) {
        GC_err_printf("GC internal allocation (%lu bytes) returning NULL\n",
                       (unsigned long) lb);
        return(0);
    }
    if (!GC_debugging_started) {
        GC_start_debugging_inner();
    }
    ADD_CALL_CHAIN(result, GC_RETURN_ADDR);
    return (GC_store_debug_info_inner(result, (word)lb, "INTERNAL", 0));
  }
#endif /* DBG_HDRS_ALL */

GC_API void * GC_CALL GC_debug_malloc_stubborn(size_t lb, GC_EXTRA_PARAMS)
{
    return GC_debug_malloc(lb, OPT_RA s, i);
}

GC_API void GC_CALL GC_debug_change_stubborn(
                                const void * p GC_ATTR_UNUSED) {}

GC_API void GC_CALL GC_debug_end_stubborn_change(const void *p)
{
    const void * q = GC_base_C(p);

    if (NULL == q) {
        ABORT_ARG1("GC_debug_end_stubborn_change: bad arg", ": %p", p);
    }
    GC_end_stubborn_change(q);
}

GC_API GC_ATTR_MALLOC void * GC_CALL GC_debug_malloc_atomic(size_t lb,
                                                            GC_EXTRA_PARAMS)
{
    void * result = GC_malloc_atomic(SIZET_SAT_ADD(lb, DEBUG_BYTES));

    return store_debug_info(result, lb, "GC_debug_malloc_atomic",
                            OPT_RA s, i);
}

GC_API GC_ATTR_MALLOC char * GC_CALL GC_debug_strdup(const char *str,
                                                     GC_EXTRA_PARAMS)
{
  char *copy;
  size_t lb;
  if (str == NULL) {
    if (GC_find_leak)
      GC_err_printf("strdup(NULL) behavior is undefined\n");
    return NULL;
  }

  lb = strlen(str) + 1;
  copy = (char *)GC_debug_malloc_atomic(lb, OPT_RA s, i);
  if (copy == NULL) {
#   ifndef MSWINCE
      errno = ENOMEM;
#   endif
    return NULL;
  }
  BCOPY(str, copy, lb);
  return copy;
}

GC_API GC_ATTR_MALLOC char * GC_CALL GC_debug_strndup(const char *str,
                                                size_t size, GC_EXTRA_PARAMS)
{
  char *copy;
  size_t len = strlen(str); /* str is expected to be non-NULL  */
  if (len > size)
    len = size;
  copy = (char *)GC_debug_malloc_atomic(len + 1, OPT_RA s, i);
  if (copy == NULL) {
#   ifndef MSWINCE
      errno = ENOMEM;
#   endif
    return NULL;
  }
  if (len > 0)
    BCOPY(str, copy, len);
  copy[len] = '\0';
  return copy;
}

#ifdef GC_REQUIRE_WCSDUP
# include <wchar.h> /* for wcslen() */

  GC_API GC_ATTR_MALLOC wchar_t * GC_CALL GC_debug_wcsdup(const wchar_t *str,
                                                          GC_EXTRA_PARAMS)
  {
    size_t lb = (wcslen(str) + 1) * sizeof(wchar_t);
    wchar_t *copy = (wchar_t *)GC_debug_malloc_atomic(lb, OPT_RA s, i);
    if (copy == NULL) {
#     ifndef MSWINCE
        errno = ENOMEM;
#     endif
      return NULL;
    }
    BCOPY(str, copy, lb);
    return copy;
  }
#endif /* GC_REQUIRE_WCSDUP */

GC_API GC_ATTR_MALLOC void * GC_CALL GC_debug_malloc_uncollectable(size_t lb,
                                                        GC_EXTRA_PARAMS)
{
    void * result = GC_malloc_uncollectable(
                                SIZET_SAT_ADD(lb, UNCOLLECTABLE_DEBUG_BYTES));

    return store_debug_info(result, lb, "GC_debug_malloc_uncollectable",
                            OPT_RA s, i);
}

#ifdef GC_ATOMIC_UNCOLLECTABLE
  GC_API GC_ATTR_MALLOC void * GC_CALL
        GC_debug_malloc_atomic_uncollectable(size_t lb, GC_EXTRA_PARAMS)
  {
    void * result = GC_malloc_atomic_uncollectable(
                                SIZET_SAT_ADD(lb, UNCOLLECTABLE_DEBUG_BYTES));

    return store_debug_info(result, lb,
                            "GC_debug_malloc_atomic_uncollectable",
                            OPT_RA s, i);
  }
#endif /* GC_ATOMIC_UNCOLLECTABLE */

#ifndef GC_FREED_MEM_MARKER
# if CPP_WORDSZ == 32
#   define GC_FREED_MEM_MARKER 0xdeadbeef
# else
#   define GC_FREED_MEM_MARKER GC_WORD_C(0xEFBEADDEdeadbeef)
# endif
#endif

GC_API void GC_CALL GC_debug_free(void * p)
{
    ptr_t base;
    if (0 == p) return;

    base = (ptr_t)GC_base(p);
    if (NULL == base) {
#     if defined(REDIRECT_MALLOC) \
         && ((defined(NEED_CALLINFO) && defined(GC_HAVE_BUILTIN_BACKTRACE)) \
             || defined(GC_LINUX_THREADS) || defined(GC_SOLARIS_THREADS) \
             || defined(MSWIN32))
        /* In some cases, we should ignore objects that do not belong   */
        /* to the GC heap.  See the comment in GC_free.                 */
        if (!GC_is_heap_ptr(p)) return;
#     endif
      ABORT_ARG1("Invalid pointer passed to free()", ": %p", p);
    }
    if ((ptr_t)p - (ptr_t)base != sizeof(oh)) {
#     if defined(REDIRECT_FREE) && defined(USE_PROC_FOR_LIBRARIES)
        /* TODO: Suppress the warning if free() caller is in libpthread */
        /* or libdl.                                                    */
#     endif
      GC_err_printf(
               "GC_debug_free called on pointer %p w/o debugging info\n", p);
    } else {
#     ifndef SHORT_DBG_HDRS
        ptr_t clobbered = GC_check_annotated_obj((oh *)base);
        word sz = GC_size(base);
        if (clobbered != 0) {
          GC_have_errors = TRUE;
          if (((oh *)base) -> oh_sz == sz) {
            GC_print_smashed_obj(
                  "GC_debug_free: found previously deallocated (?) object at",
                  p, clobbered);
            return; /* ignore double free */
          } else {
            GC_print_smashed_obj("GC_debug_free: found smashed location at",
                                 p, clobbered);
          }
        }
        /* Invalidate size (mark the object as deallocated) */
        ((oh *)base) -> oh_sz = sz;
#     endif /* SHORT_DBG_HDRS */
    }
    if (GC_find_leak
#       ifndef SHORT_DBG_HDRS
          && ((ptr_t)p - (ptr_t)base != sizeof(oh) || !GC_findleak_delay_free)
#       endif
        ) {
      GC_free(base);
    } else {
      hdr * hhdr = HDR(p);
      if (hhdr -> hb_obj_kind == UNCOLLECTABLE
#         ifdef GC_ATOMIC_UNCOLLECTABLE
            || hhdr -> hb_obj_kind == AUNCOLLECTABLE
#         endif
          ) {
        GC_free(base);
      } else {
        word i;
        word obj_sz = BYTES_TO_WORDS(hhdr->hb_sz - sizeof(oh));

        for (i = 0; i < obj_sz; ++i)
          ((word *)p)[i] = GC_FREED_MEM_MARKER;
        GC_ASSERT((word *)p + i == (word *)(base + hhdr -> hb_sz));
      }
    } /* !GC_find_leak */
}

#if defined(THREADS) && defined(DBG_HDRS_ALL)
  /* Used internally; we assume it's called correctly.    */
  GC_INNER void GC_debug_free_inner(void * p)
  {
    ptr_t base = (ptr_t)GC_base(p);
    GC_ASSERT((ptr_t)p - (ptr_t)base == sizeof(oh));
#   ifdef LINT2
      if (!base) ABORT("Invalid GC_debug_free_inner argument");
#   endif
#   ifndef SHORT_DBG_HDRS
      /* Invalidate size */
      ((oh *)base) -> oh_sz = GC_size(base);
#   endif
    GC_free_inner(base);
  }
#endif

GC_API void * GC_CALL GC_debug_realloc(void * p, size_t lb, GC_EXTRA_PARAMS)
{
    void * base;
    void * result;
    hdr * hhdr;

    if (p == 0) {
      return GC_debug_malloc(lb, OPT_RA s, i);
    }
    if (0 == lb) /* and p != NULL */ {
      GC_debug_free(p);
      return NULL;
    }

#   ifdef GC_ADD_CALLER
      if (s == NULL) {
        GC_caller_func_offset(ra, &s, &i);
      }
#   endif
    base = GC_base(p);
    if (base == 0) {
        ABORT_ARG1("Invalid pointer passed to realloc()", ": %p", p);
    }
    if ((ptr_t)p - (ptr_t)base != sizeof(oh)) {
        GC_err_printf(
              "GC_debug_realloc called on pointer %p w/o debugging info\n", p);
        return(GC_realloc(p, lb));
    }
    hhdr = HDR(base);
    switch (hhdr -> hb_obj_kind) {
      case NORMAL:
        result = GC_debug_malloc(lb, OPT_RA s, i);
        break;
      case PTRFREE:
        result = GC_debug_malloc_atomic(lb, OPT_RA s, i);
        break;
      case UNCOLLECTABLE:
        result = GC_debug_malloc_uncollectable(lb, OPT_RA s, i);
        break;
#    ifdef GC_ATOMIC_UNCOLLECTABLE
      case AUNCOLLECTABLE:
        result = GC_debug_malloc_atomic_uncollectable(lb, OPT_RA s, i);
        break;
#    endif
      default:
        result = NULL; /* initialized to prevent warning. */
        ABORT_RET("GC_debug_realloc: encountered bad kind");
    }

    if (result != NULL) {
      size_t old_sz;
#     ifdef SHORT_DBG_HDRS
        old_sz = GC_size(base) - sizeof(oh);
#     else
        old_sz = ((oh *)base) -> oh_sz;
#     endif
      if (old_sz > 0)
        BCOPY(p, result, old_sz < lb ? old_sz : lb);
      GC_debug_free(p);
    }
    return(result);
}

GC_API GC_ATTR_MALLOC void * GC_CALL
    GC_debug_generic_or_special_malloc(size_t lb, int knd, GC_EXTRA_PARAMS)
{
    switch (knd) {
        case PTRFREE:
            return GC_debug_malloc_atomic(lb, OPT_RA s, i);
        case NORMAL:
            return GC_debug_malloc(lb, OPT_RA s, i);
        case UNCOLLECTABLE:
            return GC_debug_malloc_uncollectable(lb, OPT_RA s, i);
#     ifdef GC_ATOMIC_UNCOLLECTABLE
        case AUNCOLLECTABLE:
            return GC_debug_malloc_atomic_uncollectable(lb, OPT_RA s, i);
#     endif
        default:
            return GC_debug_generic_malloc(lb, knd, OPT_RA s, i);
    }
}

#ifndef SHORT_DBG_HDRS

/* List of smashed (clobbered) locations.  We defer printing these,     */
/* since we can't always print them nicely with the allocation lock     */
/* held.  We put them here instead of in GC_arrays, since it may be     */
/* useful to be able to look at them with the debugger.                 */
#ifndef MAX_SMASHED
# define MAX_SMASHED 20
#endif
STATIC ptr_t GC_smashed[MAX_SMASHED] = {0};
STATIC unsigned GC_n_smashed = 0;

STATIC void GC_add_smashed(ptr_t smashed)
{
    GC_ASSERT(GC_is_marked(GC_base(smashed)));
    /* FIXME: Prevent adding an object while printing smashed list.     */
    GC_smashed[GC_n_smashed] = smashed;
    if (GC_n_smashed < MAX_SMASHED - 1) ++GC_n_smashed;
      /* In case of overflow, we keep the first MAX_SMASHED-1   */
      /* entries plus the last one.                             */
    GC_have_errors = TRUE;
}

/* Print all objects on the list.  Clear the list.      */
STATIC void GC_print_all_smashed_proc(void)
{
    unsigned i;

    GC_ASSERT(I_DONT_HOLD_LOCK());
    if (GC_n_smashed == 0) return;
    GC_err_printf("GC_check_heap_block: found %u smashed heap objects:\n",
                  GC_n_smashed);
    for (i = 0; i < GC_n_smashed; ++i) {
        ptr_t base = (ptr_t)GC_base(GC_smashed[i]);

#       ifdef LINT2
          if (!base) ABORT("Invalid GC_smashed element");
#       endif
        GC_print_smashed_obj("", base + sizeof(oh), GC_smashed[i]);
        GC_smashed[i] = 0;
    }
    GC_n_smashed = 0;
}

/* Check all marked objects in the given block for validity     */
/* Avoid GC_apply_to_each_object for performance reasons.       */
STATIC void GC_check_heap_block(struct hblk *hbp, word dummy GC_ATTR_UNUSED)
{
    struct hblkhdr * hhdr = HDR(hbp);
    word sz = hhdr -> hb_sz;
    word bit_no;
    char *p, *plim;

    p = hbp->hb_body;
    if (sz > MAXOBJBYTES) {
      plim = p;
    } else {
      plim = hbp->hb_body + HBLKSIZE - sz;
    }
    /* go through all words in block */
    for (bit_no = 0; (word)p <= (word)plim;
         bit_no += MARK_BIT_OFFSET(sz), p += sz) {
      if (mark_bit_from_hdr(hhdr, bit_no) && GC_HAS_DEBUG_INFO((ptr_t)p)) {
        ptr_t clobbered = GC_check_annotated_obj((oh *)p);
        if (clobbered != 0)
          GC_add_smashed(clobbered);
      }
    }
}

/* This assumes that all accessible objects are marked, and that        */
/* I hold the allocation lock.  Normally called by collector.           */
STATIC void GC_check_heap_proc(void)
{
  GC_STATIC_ASSERT((sizeof(oh) & (GRANULE_BYTES - 1)) == 0);
  /* FIXME: Should we check for twice that alignment?   */
  GC_apply_to_all_blocks(GC_check_heap_block, 0);
}

GC_INNER GC_bool GC_check_leaked(ptr_t base)
{
  word i;
  word obj_sz;
  word *p;

  if (
#     if defined(KEEP_BACK_PTRS) || defined(MAKE_BACK_GRAPH)
        (*(word *)base & 1) != 0 &&
#     endif
      GC_has_other_debug_info(base) >= 0)
    return TRUE; /* object has leaked */

  /* Validate freed object's content. */
  p = (word *)(base + sizeof(oh));
  obj_sz = BYTES_TO_WORDS(HDR(base)->hb_sz - sizeof(oh));
  for (i = 0; i < obj_sz; ++i)
    if (p[i] != GC_FREED_MEM_MARKER) {
        GC_set_mark_bit(base); /* do not reclaim it in this cycle */
        GC_add_smashed((ptr_t)(&p[i])); /* alter-after-free detected */
        break; /* don't report any other smashed locations in the object */
    }

  return FALSE; /* GC_debug_free() has been called */
}

#endif /* !SHORT_DBG_HDRS */

#ifndef GC_NO_FINALIZATION

struct closure {
    GC_finalization_proc cl_fn;
    void * cl_data;
};

STATIC void * GC_make_closure(GC_finalization_proc fn, void * data)
{
    struct closure * result =
#   ifdef DBG_HDRS_ALL
      (struct closure *) GC_debug_malloc(sizeof (struct closure),
                                         GC_EXTRAS);
#   else
      (struct closure *) GC_malloc(sizeof (struct closure));
#   endif
    if (result != 0) {
      result -> cl_fn = fn;
      result -> cl_data = data;
    }
    return((void *)result);
}

/* An auxiliary fns to make finalization work correctly with displaced  */
/* pointers introduced by the debugging allocators.                     */
STATIC void GC_CALLBACK GC_debug_invoke_finalizer(void * obj, void * data)
{
    struct closure * cl = (struct closure *) data;
    (*(cl -> cl_fn))((void *)((char *)obj + sizeof(oh)), cl -> cl_data);
}

/* Special finalizer_proc value to detect GC_register_finalizer() failure. */
#define OFN_UNSET ((GC_finalization_proc)~(signed_word)0)

/* Set ofn and ocd to reflect the values we got back.   */
static void store_old(void *obj, GC_finalization_proc my_old_fn,
                      struct closure *my_old_cd, GC_finalization_proc *ofn,
                      void **ocd)
{
    if (0 != my_old_fn) {
      if (my_old_fn == OFN_UNSET) {
        /* register_finalizer() failed; (*ofn) and (*ocd) are unchanged. */
        return;
      }
      if (my_old_fn != GC_debug_invoke_finalizer) {
        GC_err_printf("Debuggable object at %p had a non-debug finalizer\n",
                      obj);
        /* This should probably be fatal. */
      } else {
        if (ofn) *ofn = my_old_cd -> cl_fn;
        if (ocd) *ocd = my_old_cd -> cl_data;
      }
    } else {
      if (ofn) *ofn = 0;
      if (ocd) *ocd = 0;
    }
}

GC_API void GC_CALL GC_debug_register_finalizer(void * obj,
                                        GC_finalization_proc fn,
                                        void * cd, GC_finalization_proc *ofn,
                                        void * *ocd)
{
    GC_finalization_proc my_old_fn = OFN_UNSET;
    void * my_old_cd;
    ptr_t base = (ptr_t)GC_base(obj);
    if (NULL == base) {
        /* We won't collect it, hence finalizer wouldn't be run. */
        if (ocd) *ocd = 0;
        if (ofn) *ofn = 0;
        return;
    }
    if ((ptr_t)obj - base != sizeof(oh)) {
        GC_err_printf("GC_debug_register_finalizer called with"
                      " non-base-pointer %p\n", obj);
    }
    if (0 == fn) {
      GC_register_finalizer(base, 0, 0, &my_old_fn, &my_old_cd);
    } else {
      cd = GC_make_closure(fn, cd);
      if (cd == 0) return; /* out of memory */
      GC_register_finalizer(base, GC_debug_invoke_finalizer,
                            cd, &my_old_fn, &my_old_cd);
    }
    store_old(obj, my_old_fn, (struct closure *)my_old_cd, ofn, ocd);
}

GC_API void GC_CALL GC_debug_register_finalizer_no_order
                                    (void * obj, GC_finalization_proc fn,
                                     void * cd, GC_finalization_proc *ofn,
                                     void * *ocd)
{
    GC_finalization_proc my_old_fn = OFN_UNSET;
    void * my_old_cd;
    ptr_t base = (ptr_t)GC_base(obj);
    if (NULL == base) {
        /* We won't collect it, hence finalizer wouldn't be run. */
        if (ocd) *ocd = 0;
        if (ofn) *ofn = 0;
        return;
    }
    if ((ptr_t)obj - base != sizeof(oh)) {
        GC_err_printf("GC_debug_register_finalizer_no_order called with"
                      " non-base-pointer %p\n", obj);
    }
    if (0 == fn) {
      GC_register_finalizer_no_order(base, 0, 0, &my_old_fn, &my_old_cd);
    } else {
      cd = GC_make_closure(fn, cd);
      if (cd == 0) return; /* out of memory */
      GC_register_finalizer_no_order(base, GC_debug_invoke_finalizer,
                                     cd, &my_old_fn, &my_old_cd);
    }
    store_old(obj, my_old_fn, (struct closure *)my_old_cd, ofn, ocd);
}

GC_API void GC_CALL GC_debug_register_finalizer_unreachable
                                    (void * obj, GC_finalization_proc fn,
                                     void * cd, GC_finalization_proc *ofn,
                                     void * *ocd)
{
    GC_finalization_proc my_old_fn = OFN_UNSET;
    void * my_old_cd;
    ptr_t base = (ptr_t)GC_base(obj);
    if (NULL == base) {
        /* We won't collect it, hence finalizer wouldn't be run. */
        if (ocd) *ocd = 0;
        if (ofn) *ofn = 0;
        return;
    }
    if ((ptr_t)obj - base != sizeof(oh)) {
        GC_err_printf("GC_debug_register_finalizer_unreachable called with"
                      " non-base-pointer %p\n", obj);
    }
    if (0 == fn) {
      GC_register_finalizer_unreachable(base, 0, 0, &my_old_fn, &my_old_cd);
    } else {
      cd = GC_make_closure(fn, cd);
      if (cd == 0) return; /* out of memory */
      GC_register_finalizer_unreachable(base, GC_debug_invoke_finalizer,
                                        cd, &my_old_fn, &my_old_cd);
    }
    store_old(obj, my_old_fn, (struct closure *)my_old_cd, ofn, ocd);
}

GC_API void GC_CALL GC_debug_register_finalizer_ignore_self
                                    (void * obj, GC_finalization_proc fn,
                                     void * cd, GC_finalization_proc *ofn,
                                     void * *ocd)
{
    GC_finalization_proc my_old_fn = OFN_UNSET;
    void * my_old_cd;
    ptr_t base = (ptr_t)GC_base(obj);
    if (NULL == base) {
        /* We won't collect it, hence finalizer wouldn't be run. */
        if (ocd) *ocd = 0;
        if (ofn) *ofn = 0;
        return;
    }
    if ((ptr_t)obj - base != sizeof(oh)) {
        GC_err_printf("GC_debug_register_finalizer_ignore_self called with"
                      " non-base-pointer %p\n", obj);
    }
    if (0 == fn) {
      GC_register_finalizer_ignore_self(base, 0, 0, &my_old_fn, &my_old_cd);
    } else {
      cd = GC_make_closure(fn, cd);
      if (cd == 0) return; /* out of memory */
      GC_register_finalizer_ignore_self(base, GC_debug_invoke_finalizer,
                                        cd, &my_old_fn, &my_old_cd);
    }
    store_old(obj, my_old_fn, (struct closure *)my_old_cd, ofn, ocd);
}

#endif /* !GC_NO_FINALIZATION */

GC_API GC_ATTR_MALLOC void * GC_CALL GC_debug_malloc_replacement(size_t lb)
{
    return GC_debug_malloc(lb, GC_DBG_EXTRAS);
}

GC_API void * GC_CALL GC_debug_realloc_replacement(void *p, size_t lb)
{
    return GC_debug_realloc(p, lb, GC_DBG_EXTRAS);
}
