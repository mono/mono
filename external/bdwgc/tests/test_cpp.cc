/****************************************************************************
Copyright (c) 1994 by Xerox Corporation.  All rights reserved.

THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.

Permission is hereby granted to use or copy this program for any
purpose, provided the above notices are retained on all copies.
Permission to modify the code and to distribute modified code is
granted, provided the above notices are retained, and a notice that
the code was modified is included with the above copyright notice.
****************************************************************************

usage: test_cpp number-of-iterations

This program tries to test the specific C++ functionality provided by
gc_c++.h that isn't tested by the more general test routines of the
collector.

A recommended value for number-of-iterations is 10, which will take a
few minutes to complete.

***************************************************************************/

#ifdef HAVE_CONFIG_H
# include "config.h"
#endif

#undef GC_BUILD

#include "gc_cpp.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifndef DONT_USE_STD_ALLOCATOR
# include "gc_allocator.h"
#else
  /* Note: This works only for ancient STL versions.    */
# include "new_gc_alloc.h"
#endif

# include "private/gcconfig.h"

# ifndef GC_API_PRIV
#   define GC_API_PRIV GC_API
# endif
extern "C" {
  GC_API_PRIV void GC_printf(const char * format, ...);
  /* Use GC private output to reach the same log file.  */
  /* Don't include gc_priv.h, since that may include Windows system     */
  /* header files that don't take kindly to this context.               */
}

#ifdef MSWIN32
# include <windows.h>
#endif

#ifdef GC_NAME_CONFLICT
# define USE_GC GC_NS_QUALIFY(UseGC)
  struct foo * GC;
#else
# define USE_GC GC_NS_QUALIFY(GC)
#endif

#define my_assert( e ) \
    if (! (e)) { \
        GC_printf( "Assertion failure in " __FILE__ ", line %d: " #e "\n", \
                    __LINE__ ); \
        exit( 1 ); }

#ifndef GC_ATTR_EXPLICIT
# if (__cplusplus >= 201103L) || defined(CPPCHECK)
#   define GC_ATTR_EXPLICIT explicit
# else
#   define GC_ATTR_EXPLICIT /* empty */
# endif
#endif

class A {public:
    /* An uncollectible class. */

    GC_ATTR_EXPLICIT A( int iArg ): i( iArg ) {}
    void Test( int iArg ) {
        my_assert( i == iArg );}
    int i;};


class B: public GC_NS_QUALIFY(gc), public A { public:
    /* A collectible class. */

    GC_ATTR_EXPLICIT B( int j ): A( j ) {}
    virtual ~B() {
        my_assert( deleting );}
    static void Deleting( int on ) {
        deleting = on;}
    static int deleting;};

int B::deleting = 0;


class C: public GC_NS_QUALIFY(gc_cleanup), public A { public:
    /* A collectible class with cleanup and virtual multiple inheritance. */

    // The class uses dynamic memory/resource allocation, so provide both
    // a copy constructor and an assignment operator to workaround a cppcheck
    // warning.
    C(const C& c) : A(c.i), level(c.level) {
        left = c.left ? new C(*c.left) : 0;
        right = c.right ? new C(*c.right) : 0;
    }

    C& operator=(const C& c) {
        if (this != &c) {
            delete left;
            delete right;
            i = c.i;
            level = c.level;
            left = c.left ? new C(*c.left) : 0;
            right = c.right ? new C(*c.right) : 0;
        }
        return *this;
    }

    GC_ATTR_EXPLICIT C( int levelArg ): A( levelArg ), level( levelArg ) {
        nAllocated++;
        if (level > 0) {
            left = new C( level - 1 );
            right = new C( level - 1 );}
        else {
            left = right = 0;}}
    ~C() {
        this->A::Test( level );
        nFreed++;
        my_assert( level == 0 ?
                   left == 0 && right == 0 :
                   level == left->level + 1 && level == right->level + 1 );
        left = right = 0;
        level = -123456;}
    static void Test() {
        my_assert( nFreed <= nAllocated && nFreed >= .8 * nAllocated );}

    static int nFreed;
    static int nAllocated;
    int level;
    C* left;
    C* right;};

int C::nFreed = 0;
int C::nAllocated = 0;


class D: public GC_NS_QUALIFY(gc) { public:
    /* A collectible class with a static member function to be used as
    an explicit clean-up function supplied to ::new. */

    GC_ATTR_EXPLICIT D( int iArg ): i( iArg ) {
        nAllocated++;}
    static void CleanUp( void* obj, void* data ) {
        D* self = static_cast<D*>(obj);
        nFreed++;
        my_assert( (GC_word)self->i == (GC_word)data );}
    static void Test() {
        my_assert( nFreed >= .8 * nAllocated );}

    int i;
    static int nFreed;
    static int nAllocated;};

int D::nFreed = 0;
int D::nAllocated = 0;


class E: public GC_NS_QUALIFY(gc_cleanup) { public:
    /* A collectible class with clean-up for use by F. */

    E() {
        nAllocated++;}
    ~E() {
        nFreed++;}

    static int nFreed;
    static int nAllocated;};

int E::nFreed = 0;
int E::nAllocated = 0;


class F: public E {public:
    /* A collectible class with clean-up, a base with clean-up, and a
    member with clean-up. */

    F() {
        nAllocatedF++;
    }

    ~F() {
        nFreedF++;
    }

    static void Test() {
        my_assert(nFreedF >= .8 * nAllocatedF);
        my_assert(2 * nFreedF == nFreed);
    }

    E e;
    static int nFreedF;
    static int nAllocatedF;
};

int F::nFreedF = 0;
int F::nAllocatedF = 0;


GC_word Disguise( void* p ) {
    return ~ (GC_word) p;}

void* Undisguise( GC_word i ) {
    return (void*) ~ i;}

#define GC_CHECKED_DELETE(p) \
    do { \
      size_t freed_before = GC_get_expl_freed_bytes_since_gc(); \
      delete p; /* the operator should invoke GC_FREE() */ \
      size_t freed_after = GC_get_expl_freed_bytes_since_gc(); \
      my_assert(freed_before != freed_after); \
    } while (0)

#if ((defined(MSWIN32) && !defined(__MINGW32__)) || defined(MSWINCE)) \
    && !defined(NO_WINMAIN_ENTRY)
  int APIENTRY WinMain( HINSTANCE /* instance */, HINSTANCE /* prev */,
                       LPSTR cmd, int /* cmdShow */)
  {
    int argc = 0;
    char* argv[ 3 ];

#   if defined(CPPCHECK)
      GC_noop1((GC_word)&WinMain);
#   endif
    if (cmd != 0)
      for (argc = 1; argc < (int)(sizeof(argv) / sizeof(argv[0])); argc++) {
        // Parse the command-line string.  Non-reentrant strtok() is not used
        // to avoid complains of static analysis tools.  (And, strtok_r() is
        // not available on some platforms.)  The code is equivalent to:
        //   if (!(argv[argc] = strtok(argc == 1 ? cmd : 0, " \t"))) break;
        if (NULL == cmd) {
          argv[argc] = NULL;
          break;
        }
        argv[argc] = cmd;
        for (; *cmd != '\0'; cmd++) {
          if (*cmd != ' ' && *cmd != '\t')
            break;
        }
        if ('\0' == *cmd) {
          argv[argc] = NULL;
          break;
        }
        argv[argc] = cmd;
        while (*(++cmd) != '\0') {
          if (*cmd == ' ' || *cmd == '\t')
            break;
        }
        if (*cmd != '\0') {
          *(cmd++) = '\0';
        } else {
          cmd = NULL;
        }
      }
#elif defined(MACOS)
  int main() {
    char* argv_[] = {"test_cpp", "10"}; // MacOS doesn't have a command line
    argv = argv_;
    argc = sizeof(argv_)/sizeof(argv_[0]);
#else
  int main( int argc, char* argv[] ) {
#endif

    GC_set_all_interior_pointers(1);
                        /* needed due to C++ multiple inheritance used  */

    GC_INIT();

    int i, iters, n;
#   ifndef DONT_USE_STD_ALLOCATOR
      int *x = gc_allocator<int>().allocate(1);
      int *xio;
      xio = gc_allocator_ignore_off_page<int>().allocate(1);
      (void)xio;
      int **xptr = traceable_allocator<int *>().allocate(1);
#   else
      int *x = (int *)gc_alloc::allocate(sizeof(int));
#   endif
    *x = 29;
#   ifndef DONT_USE_STD_ALLOCATOR
      if (!xptr) {
        fprintf(stderr, "Out of memory!\n");
        exit(3);
      }
      *xptr = x;
      x = 0;
#   endif
    if (argc != 2
        || (n = (int)COVERT_DATAFLOW(atoi(argv[1]))) <= 0) {
      GC_printf("usage: test_cpp number-of-iterations\n"
                "Assuming 10 iters\n");
      n = 10;
    }

    for (iters = 1; iters <= n; iters++) {
        GC_printf( "Starting iteration %d\n", iters );

            /* Allocate some uncollectible As and disguise their pointers.
            Later we'll check to see if the objects are still there.  We're
            checking to make sure these objects really are uncollectible. */
        GC_word as[ 1000 ];
        GC_word bs[ 1000 ];
        for (i = 0; i < 1000; i++) {
            as[ i ] = Disguise( new (GC_NS_QUALIFY(NoGC)) A(i) );
            bs[ i ] = Disguise( new (GC_NS_QUALIFY(NoGC)) B(i) ); }

            /* Allocate a fair number of finalizable Cs, Ds, and Fs.
            Later we'll check to make sure they've gone away. */
        for (i = 0; i < 1000; i++) {
            C* c = new C( 2 );
            C c1( 2 );           /* stack allocation should work too */
            D* d;
            F* f;
            d = ::new (USE_GC, D::CleanUp, (void*)(GC_word)i) D( i );
            (void)d;
            f = new F;
            F** fa = new F*[1];
            fa[0] = f;
            (void)fa;
            delete[] fa;
            if (0 == i % 10)
                GC_CHECKED_DELETE(c);
        }

            /* Allocate a very large number of collectible As and Bs and
            drop the references to them immediately, forcing many
            collections. */
        for (i = 0; i < 1000000; i++) {
            A* a;
            a = new (USE_GC) A( i );
            (void)a;
            B* b;
            b = new B( i );
            (void)b;
            b = new (USE_GC) B( i );
            if (0 == i % 10) {
                B::Deleting( 1 );
                GC_CHECKED_DELETE(b);
                B::Deleting( 0 );}
#           ifdef FINALIZE_ON_DEMAND
              GC_invoke_finalizers();
#           endif
            }

            /* Make sure the uncollectible As and Bs are still there. */
        for (i = 0; i < 1000; i++) {
            A* a = static_cast<A*>(Undisguise(as[i]));
            B* b = static_cast<B*>(Undisguise(bs[i]));
            a->Test( i );
#           if defined(ADDRESS_SANITIZER) || defined(MEMORY_SANITIZER)
              // Workaround for ASan/MSan: the linker uses operator delete
              // implementation from libclang_rt instead of gc_cpp (thus
              // causing incompatible alloc/free).
              GC_FREE(a);
#           else
              GC_CHECKED_DELETE(a);
#           endif
            b->Test( i );
            B::Deleting( 1 );
            GC_CHECKED_DELETE(b);
            B::Deleting( 0 );
#           ifdef FINALIZE_ON_DEMAND
                 GC_invoke_finalizers();
#           endif
            }

            /* Make sure most of the finalizable Cs, Ds, and Fs have
            gone away. */
        C::Test();
        D::Test();
        F::Test();}

#   ifndef DONT_USE_STD_ALLOCATOR
      x = *xptr;
#   endif
    my_assert (29 == x[0]);
    GC_printf( "The test appears to have succeeded.\n" );
    return( 0 );
}
