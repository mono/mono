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

/* This is a private GC header which provides an implementation of      */
/* libatomic_ops subset primitives sufficient for GC assuming that C11  */
/* atomic intrinsics are available (and have correct implementation).   */
/* This is enabled by defining GC_BUILTIN_ATOMIC macro.  Otherwise,     */
/* libatomic_ops library is used to define the primitives.              */

#ifndef GC_ATOMIC_OPS_H
#define GC_ATOMIC_OPS_H

#ifdef GC_BUILTIN_ATOMIC

# include "gc.h" /* for GC_word */

# ifdef __cplusplus
    extern "C" {
# endif

  typedef GC_word AO_t;

# ifdef GC_PRIVATE_H /* have GC_INLINE */
#   define AO_INLINE GC_INLINE
# else
#   define AO_INLINE static __inline
# endif

  typedef unsigned char AO_TS_t;
# define AO_TS_CLEAR 0
# define AO_TS_INITIALIZER (AO_TS_t)AO_TS_CLEAR
# if defined(__GCC_ATOMIC_TEST_AND_SET_TRUEVAL) && !defined(CPPCHECK)
#   define AO_TS_SET __GCC_ATOMIC_TEST_AND_SET_TRUEVAL
# else
#   define AO_TS_SET (AO_TS_t)1 /* true */
# endif
# define AO_CLEAR(p) __atomic_clear(p, __ATOMIC_RELEASE)
# define AO_test_and_set_acquire(p) __atomic_test_and_set(p, __ATOMIC_ACQUIRE)
# define AO_HAVE_test_and_set_acquire

# define AO_compiler_barrier() __atomic_signal_fence(__ATOMIC_SEQ_CST)
# define AO_nop_full() __atomic_thread_fence(__ATOMIC_SEQ_CST)
# define AO_HAVE_nop_full

# define AO_fetch_and_add(p, v) __atomic_fetch_add(p, v, __ATOMIC_RELAXED)
# define AO_HAVE_fetch_and_add
# define AO_fetch_and_add1(p) AO_fetch_and_add(p, 1)
# define AO_HAVE_fetch_and_add1

# define AO_or(p, v) (void)__atomic_or_fetch(p, v, __ATOMIC_RELAXED)
# define AO_HAVE_or

# define AO_load(p) __atomic_load_n(p, __ATOMIC_RELAXED)
# define AO_HAVE_load
# define AO_load_acquire(p) __atomic_load_n(p, __ATOMIC_ACQUIRE)
# define AO_HAVE_load_acquire
# define AO_load_acquire_read(p) AO_load_acquire(p)
# define AO_HAVE_load_acquire_read

# define AO_store(p, v) __atomic_store_n(p, v, __ATOMIC_RELAXED)
# define AO_HAVE_store
# define AO_store_release(p, v) __atomic_store_n(p, v, __ATOMIC_RELEASE)
# define AO_HAVE_store_release
# define AO_store_release_write(p, v) AO_store_release(p, v)
# define AO_HAVE_store_release_write

# define AO_char_load(p) __atomic_load_n(p, __ATOMIC_RELAXED)
# define AO_HAVE_char_load
# define AO_char_store(p, v) __atomic_store_n(p, v, __ATOMIC_RELAXED)
# define AO_HAVE_char_store

# ifdef AO_REQUIRE_CAS
    AO_INLINE int
    AO_compare_and_swap(volatile AO_t *p, AO_t ov, AO_t nv)
    {
      return (int)__atomic_compare_exchange_n(p, &ov, nv, 0,
                                        __ATOMIC_RELAXED, __ATOMIC_RELAXED);
    }

    AO_INLINE int
    AO_compare_and_swap_release(volatile AO_t *p, AO_t ov, AO_t nv)
    {
      return (int)__atomic_compare_exchange_n(p, &ov, nv, 0,
                                        __ATOMIC_RELEASE, __ATOMIC_RELAXED);
    }
#   define AO_HAVE_compare_and_swap_release
# endif

# ifdef __cplusplus
    } /* extern "C" */
# endif

#elif !defined(NN_PLATFORM_CTR)
  /* Fallback to libatomic_ops. */
# include "atomic_ops.h"

  /* AO_compiler_barrier, AO_load and AO_store should be defined for    */
  /* all targets; the rest of the primitives are guaranteed to exist    */
  /* only if AO_REQUIRE_CAS is defined (or if the corresponding         */
  /* AO_HAVE_x macro is defined).  x86/x64 targets have AO_nop_full,    */
  /* AO_load_acquire, AO_store_release, at least.                       */
# if !defined(AO_HAVE_load) || !defined(AO_HAVE_store)
#   error AO_load or AO_store is missing; probably old version of atomic_ops
# endif

#endif /* !GC_BUILTIN_ATOMIC */

#endif /* GC_ATOMIC_OPS_H */
