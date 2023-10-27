/*
 * Copyright (c) 2017 Ivan Maidanski
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

/* Minimal testing of atomic operations used by the BDWGC.  Primary use */
/* is to determine whether compiler atomic intrinsics can be relied on. */

#ifdef HAVE_CONFIG_H
# include "config.h"
#endif

#include <stdio.h>

#if defined(GC_BUILTIN_ATOMIC) || defined(GC_THREADS)

# include <stdlib.h>

# ifdef PARALLEL_MARK
#   define AO_REQUIRE_CAS
# endif

# include "private/gc_atomic_ops.h"

# define TA_assert(e) \
    if (!(e)) { \
        fprintf(stderr, "Assertion failure, line %d: " #e "\n", __LINE__); \
        exit(-1); \
    }

  int main(void) {
    AO_t x = 13;
#   if defined(AO_HAVE_char_load) || defined(AO_HAVE_char_store)
      unsigned char c = 117;
#   endif
#   ifdef AO_HAVE_test_and_set_acquire
        AO_TS_t z = AO_TS_INITIALIZER;

        TA_assert(AO_test_and_set_acquire(&z) == AO_TS_CLEAR);
        TA_assert(AO_test_and_set_acquire(&z) == AO_TS_SET);
        AO_CLEAR(&z);
#   endif
    AO_compiler_barrier();
#   ifdef AO_HAVE_nop_full
      AO_nop_full();
#   endif
#   ifdef AO_HAVE_char_load
      TA_assert(AO_char_load(&c) == 117);
#   endif
#   ifdef AO_HAVE_char_store
      AO_char_store(&c, 119);
      TA_assert(c == 119);
#   endif
#   ifdef AO_HAVE_load_acquire
      TA_assert(AO_load_acquire(&x) == 13);
#   endif
#   if defined(AO_HAVE_fetch_and_add) && defined(AO_HAVE_fetch_and_add1)
      TA_assert(AO_fetch_and_add(&x, 42) == 13);
      TA_assert(AO_fetch_and_add(&x, (AO_t)(-43)) == 55);
      TA_assert(AO_fetch_and_add1(&x) == 12);
#   endif
#   ifdef AO_HAVE_compare_and_swap_release
      TA_assert(!AO_compare_and_swap(&x, 14, 42));
      TA_assert(x == 13);
      TA_assert(AO_compare_and_swap_release(&x, 13, 42));
      TA_assert(x == 42);
#   else
      if (*(volatile AO_t *)&x == 13)
        *(volatile AO_t *)&x = 42;
#   endif
#   ifdef AO_HAVE_or
      AO_or(&x, 66);
      TA_assert(x == 106);
#   endif
#   ifdef AO_HAVE_store_release
      AO_store_release(&x, 113);
      TA_assert(x == 113);
#   endif
    return 0;
  }

#else

  int main(void)
  {
    printf("test_atomic_ops skipped\n");
    return 0;
  }

#endif
