/*
 * Copyright (c) 1996-1998 by Silicon Graphics.  All rights reserved.
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

//
// This is a C++ header file that is intended to replace the SGI STL
// alloc.h.  This assumes SGI STL version < 3.0.
//
// This assumes the collector has been compiled with -DATOMIC_UNCOLLECTABLE
// and -DALL_INTERIOR_POINTERS.  We also recommend
// -DREDIRECT_MALLOC=GC_uncollectable_malloc.
//
// Some of this could be faster in the explicit deallocation case.  In particular,
// we spend too much time clearing objects on the free lists.  That could be avoided.
//
// This uses template classes with static members, and hence does not work
// with g++ 2.7.2 and earlier.
//
// This code assumes that the collector itself has been compiled with a
// compiler that defines __STDC__ .
//

#include "gc.h"

#ifndef GC_ALLOC_H

#define GC_ALLOC_H
#define __ALLOC_H	// Prevent inclusion of the default version.  Ugly.
#define __SGI_STL_ALLOC_H
#define __SGI_STL_INTERNAL_ALLOC_H

#ifndef __ALLOC
#   define __ALLOC alloc
#endif

#include <stddef.h>
#include <string.h>

// The following is just replicated from the conventional SGI alloc.h:

template<class T, class alloc>
class simple_alloc {

public:
    static T *allocate(size_t n)
                { return 0 == n? 0 : (T*) alloc::allocate(n * sizeof (T)); }
    static T *allocate(void)
                { return (T*) alloc::allocate(sizeof (T)); }
    static void deallocate(T *p, size_t n)
                { if (0 != n) alloc::deallocate(p, n * sizeof (T)); }
    static void deallocate(T *p)
                { alloc::deallocate(p, sizeof (T)); }
};

#include "gc.h"

// The following need to match collector data structures.
// We can't include gc_priv.h, since that pulls in way too much stuff.
// This should eventually be factored out into another include file.

extern "C" {
    extern void ** const GC_objfreelist_ptr;
    extern void ** const GC_aobjfreelist_ptr;
    extern void ** const GC_uobjfreelist_ptr;
    extern void ** const GC_auobjfreelist_ptr;

    extern void GC_incr_words_allocd(size_t words);
    extern void GC_incr_mem_freed(size_t words);

    extern char * GC_generic_malloc_words_small(size_t word, int kind);
}

// Object kinds; must match PTRFREE, NORMAL, UNCOLLECTABLE, and
// AUNCOLLECTABLE in gc_priv.h.

enum { GC_PTRFREE = 0, GC_NORMAL = 1, GC_UNCOLLECTABLE = 2,
       GC_AUNCOLLECTABLE = 3 };

enum { GC_max_fast_bytes = 255 };

enum { GC_bytes_per_word = sizeof(char *) };

enum { GC_byte_alignment = 8 };

enum { GC_word_alignment = GC_byte_alignment/GC_bytes_per_word };

inline void * &GC_obj_link(void * p)
{   return *(void **)p;  }

// Compute a number of words >= n+1 bytes.
// The +1 allows for pointers one past the end.
inline size_t GC_round_up(size_t n)
{
    return ((n + GC_byte_alignment)/GC_byte_alignment)*GC_word_alignment;
}

// The same but don't allow for extra byte.
inline size_t GC_round_up_uncollectable(size_t n)
{
    return ((n + GC_byte_alignment - 1)/GC_byte_alignment)*GC_word_alignment;
}

template <int dummy>
class GC_aux_template {
public:
  // File local count of allocated words.  Occasionally this is
  // added into the global count.  A separate count is necessary since the
  // real one must be updated with a procedure call.
  static size_t GC_words_recently_allocd;

  // Same for uncollectable mmory.  Not yet reflected in either
  // GC_words_recently_allocd or GC_non_gc_bytes.
  static size_t GC_uncollectable_words_recently_allocd;

  // Similar counter for explicitly deallocated memory.
  static size_t GC_mem_recently_freed;

  // Again for uncollectable memory.
  static size_t GC_uncollectable_mem_recently_freed;

  static void * GC_out_of_line_malloc(size_t nwords, int kind);
};

template <int dummy>
size_t GC_aux_template<dummy>::GC_words_recently_allocd = 0;

template <int dummy>
size_t GC_aux_template<dummy>::GC_uncollectable_words_recently_allocd = 0;

template <int dummy>
size_t GC_aux_template<dummy>::GC_mem_recently_freed = 0;

template <int dummy>
size_t GC_aux_template<dummy>::GC_uncollectable_mem_recently_freed = 0;

template <int dummy>
void * GC_aux_template<dummy>::GC_out_of_line_malloc(size_t nwords, int kind)
{
    GC_words_recently_allocd += GC_uncollectable_words_recently_allocd;
    GC_non_gc_bytes +=
                GC_bytes_per_word * GC_uncollectable_words_recently_allocd;
    GC_uncollectable_words_recently_allocd = 0;

    GC_mem_recently_freed += GC_uncollectable_mem_recently_freed;
    GC_non_gc_bytes -= 
                GC_bytes_per_word * GC_uncollectable_mem_recently_freed;
    GC_uncollectable_mem_recently_freed = 0;

    GC_incr_words_allocd(GC_words_recently_allocd);
    GC_words_recently_allocd = 0;

    GC_incr_mem_freed(GC_mem_recently_freed);
    GC_mem_recently_freed = 0;

    return GC_generic_malloc_words_small(nwords, kind);
}

typedef GC_aux_template<0> GC_aux;

// A fast, single-threaded, garbage-collected allocator
// We assume the first word will be immediately overwritten.
// In this version, deallocation is not a noop, and explicit
// deallocation is likely to help performance.
template <int dummy>
class single_client_gc_alloc_template {
    public:
     	static void * allocate(size_t n)
        {
	    size_t nwords = GC_round_up(n);
	    void ** flh;
	    void * op;

  	    if (n > GC_max_fast_bytes) return GC_malloc(n);
	    flh = GC_objfreelist_ptr + nwords;
	    if (0 == (op = *flh)) {
		return GC_aux::GC_out_of_line_malloc(nwords, GC_NORMAL);
	    }
	    *flh = GC_obj_link(op);
	    GC_aux::GC_words_recently_allocd += nwords;
	    return op;
        }
     	static void * ptr_free_allocate(size_t n)
        {
	    size_t nwords = GC_round_up(n);
	    void ** flh;
	    void * op;

  	    if (n > GC_max_fast_bytes) return GC_malloc_atomic(n);
	    flh = GC_aobjfreelist_ptr + nwords;
	    if (0 == (op = *flh)) {
		return GC_aux::GC_out_of_line_malloc(nwords, GC_PTRFREE);
	    }
	    *flh = GC_obj_link(op);
	    GC_aux::GC_words_recently_allocd += nwords;
	    return op;
        }
	static void deallocate(void *p, size_t n)
	{
            size_t nwords = GC_round_up(n);
            void ** flh;
	   
	    if (n > GC_max_fast_bytes)  {
		GC_free(p);
	    } else {
	        flh = GC_objfreelist_ptr + nwords;
	        GC_obj_link(p) = *flh;
		memset((char *)p + GC_bytes_per_word, 0,
		       GC_bytes_per_word * (nwords - 1));
	        *flh = p;
	        GC_aux::GC_mem_recently_freed += nwords;
	    }
	}
	static void ptr_free_deallocate(void *p, size_t n)
	{
            size_t nwords = GC_round_up(n);
            void ** flh;
	   
	    if (n > GC_max_fast_bytes) {
		GC_free(p);
	    } else {
	    	flh = GC_aobjfreelist_ptr + nwords;
	    	GC_obj_link(p) = *flh;
	    	*flh = p;
	    	GC_aux::GC_mem_recently_freed += nwords;
	    }
	}
};

typedef single_client_gc_alloc_template<0> single_client_gc_alloc;

// Once more, for uncollectable objects.
template <int dummy>
class single_client_alloc_template {
    public:
     	static void * allocate(size_t n)
        {
	    size_t nwords = GC_round_up_uncollectable(n);
	    void ** flh;
	    void * op;

  	    if (n > GC_max_fast_bytes) return GC_malloc_uncollectable(n);
	    flh = GC_uobjfreelist_ptr + nwords;
	    if (0 == (op = *flh)) {
		return GC_aux::GC_out_of_line_malloc(nwords, GC_UNCOLLECTABLE);
	    }
	    *flh = GC_obj_link(op);
	    GC_aux::GC_uncollectable_words_recently_allocd += nwords;
	    return op;
        }
     	static void * ptr_free_allocate(size_t n)
        {
	    size_t nwords = GC_round_up_uncollectable(n);
	    void ** flh;
	    void * op;

  	    if (n > GC_max_fast_bytes) return GC_malloc_atomic_uncollectable(n);
	    flh = GC_auobjfreelist_ptr + nwords;
	    if (0 == (op = *flh)) {
		return GC_aux::GC_out_of_line_malloc(nwords, GC_AUNCOLLECTABLE);
	    }
	    *flh = GC_obj_link(op);
	    GC_aux::GC_uncollectable_words_recently_allocd += nwords;
	    return op;
        }
	static void deallocate(void *p, size_t n)
	{
            size_t nwords = GC_round_up_uncollectable(n);
            void ** flh;
	   
	    if (n > GC_max_fast_bytes)  {
		GC_free(p);
	    } else {
	        flh = GC_uobjfreelist_ptr + nwords;
	        GC_obj_link(p) = *flh;
	        *flh = p;
	        GC_aux::GC_uncollectable_mem_recently_freed += nwords;
	    }
	}
	static void ptr_free_deallocate(void *p, size_t n)
	{
            size_t nwords = GC_round_up_uncollectable(n);
            void ** flh;
	   
	    if (n > GC_max_fast_bytes) {
		GC_free(p);
	    } else {
	    	flh = GC_auobjfreelist_ptr + nwords;
	    	GC_obj_link(p) = *flh;
	    	*flh = p;
	    	GC_aux::GC_uncollectable_mem_recently_freed += nwords;
	    }
	}
};

typedef single_client_alloc_template<0> single_client_alloc;

template < int dummy >
class gc_alloc_template {
    public:
     	static void * allocate(size_t n) { return GC_malloc(n); }
     	static void * ptr_free_allocate(size_t n)
		{ return GC_malloc_atomic(n); }
	static void deallocate(void *, size_t) { }
	static void ptr_free_deallocate(void *, size_t) { }
};

typedef gc_alloc_template < 0 > gc_alloc;

template < int dummy >
class alloc_template {
    public:
     	static void * allocate(size_t n) { return GC_malloc_uncollectable(n); }
     	static void * ptr_free_allocate(size_t n)
		{ return GC_malloc_atomic_uncollectable(n); }
	static void deallocate(void *p, size_t) { GC_free(p); }
	static void ptr_free_deallocate(void *p, size_t) { GC_free(p); }
};

typedef alloc_template < 0 > alloc;

#ifdef _SGI_SOURCE

// We want to specialize simple_alloc so that it does the right thing
// for all pointerfree types.  At the moment there is no portable way to
// even approximate that.  The following approximation should work for
// SGI compilers, and perhaps some others.

# define __GC_SPECIALIZE(T,alloc) \
class simple_alloc<T, alloc> { \
public: \
    static T *allocate(size_t n) \
	{ return 0 == n? 0 : \
			 (T*) alloc::ptr_free_allocate(n * sizeof (T)); } \
    static T *allocate(void) \
	{ return (T*) alloc::ptr_free_allocate(sizeof (T)); } \
    static void deallocate(T *p, size_t n) \
	{ if (0 != n) alloc::ptr_free_deallocate(p, n * sizeof (T)); } \
    static void deallocate(T *p) \
	{ alloc::ptr_free_deallocate(p, sizeof (T)); } \
};

__GC_SPECIALIZE(char, gc_alloc)
__GC_SPECIALIZE(int, gc_alloc)
__GC_SPECIALIZE(unsigned, gc_alloc)
__GC_SPECIALIZE(float, gc_alloc)
__GC_SPECIALIZE(double, gc_alloc)

__GC_SPECIALIZE(char, alloc)
__GC_SPECIALIZE(int, alloc)
__GC_SPECIALIZE(unsigned, alloc)
__GC_SPECIALIZE(float, alloc)
__GC_SPECIALIZE(double, alloc)

__GC_SPECIALIZE(char, single_client_gc_alloc)
__GC_SPECIALIZE(int, single_client_gc_alloc)
__GC_SPECIALIZE(unsigned, single_client_gc_alloc)
__GC_SPECIALIZE(float, single_client_gc_alloc)
__GC_SPECIALIZE(double, single_client_gc_alloc)

__GC_SPECIALIZE(char, single_client_alloc)
__GC_SPECIALIZE(int, single_client_alloc)
__GC_SPECIALIZE(unsigned, single_client_alloc)
__GC_SPECIALIZE(float, single_client_alloc)
__GC_SPECIALIZE(double, single_client_alloc)

#ifdef __STL_USE_STD_ALLOCATORS

???copy stuff from stl_alloc.h or remove it to a different file ???

#endif /* __STL_USE_STD_ALLOCATORS */

#endif /* _SGI_SOURCE */

#endif /* GC_ALLOC_H */
