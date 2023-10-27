/*
 * Copyright (c) 1994 by Xerox Corporation.  All rights reserved.
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to copy this code for any purpose,
 * provided the above notices are retained on all copies.
 */

/*************************************************************************
This implementation module for gc_c++.h provides an implementation of
the global operators "new" and "delete" that calls the Boehm
allocator.  All objects allocated by this implementation will be
uncollectible but part of the root set of the collector.

You should ensure (using implementation-dependent techniques) that the
linker finds this module before the library that defines the default
built-in "new" and "delete".
**************************************************************************/

#ifdef HAVE_CONFIG_H
# include "config.h"
#endif

#ifndef GC_BUILD
# define GC_BUILD
#endif

#include "gc_cpp.h"

#if GC_GNUC_PREREQ(4, 2) && !defined(GC_NEW_DELETE_NEED_THROW)
# define GC_NEW_DELETE_NEED_THROW
#endif

#ifdef GC_NEW_DELETE_NEED_THROW
# include <new> /* for std::bad_alloc */
# define GC_DECL_NEW_THROW throw(std::bad_alloc)
# define GC_DECL_DELETE_THROW throw()
#else
# define GC_DECL_NEW_THROW /* empty */
# define GC_DECL_DELETE_THROW /* empty */
#endif // !GC_NEW_DELETE_NEED_THROW

#ifndef _MSC_VER

  void* operator new(size_t size) GC_DECL_NEW_THROW {
    return GC_MALLOC_UNCOLLECTABLE(size);
  }

  void operator delete(void* obj) GC_DECL_DELETE_THROW {
    GC_FREE(obj);
  }

# if defined(GC_OPERATOR_NEW_ARRAY) && !defined(CPPCHECK)
    void* operator new[](size_t size) GC_DECL_NEW_THROW {
      return GC_MALLOC_UNCOLLECTABLE(size);
    }

    void operator delete[](void* obj) GC_DECL_DELETE_THROW {
      GC_FREE(obj);
    }
# endif // GC_OPERATOR_NEW_ARRAY

# if __cplusplus > 201103L // C++14
    void operator delete(void* obj, size_t size) GC_DECL_DELETE_THROW {
      (void)size; // size is ignored
      GC_FREE(obj);
    }

#   if defined(GC_OPERATOR_NEW_ARRAY) && !defined(CPPCHECK)
      void operator delete[](void* obj, size_t size) GC_DECL_DELETE_THROW {
        (void)size;
        GC_FREE(obj);
      }
#   endif
# endif // C++14

#endif // !_MSC_VER
