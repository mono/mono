/***************************************************************************

Interface between g++ and Boehm GC

    Copyright (c) 1991-1995 by Xerox Corporation.  All rights reserved.

    THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
    OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.

    Permission is hereby granted to copy this code for any purpose,
    provided the above notices are retained on all copies.

    Last modified on Sun Jul 16 23:21:14 PDT 1995 by ellis

This module provides runtime support for implementing the
Ellis/Detlefs GC proposal, "Safe, Efficient Garbage Collection for
C++", within g++, using its -fgc-keyword extension.  It defines
versions of __builtin_new, __builtin_new_gc, __builtin_vec_new,
__builtin_vec_new_gc, __builtin_delete, and __builtin_vec_delete that
invoke the Bohem GC.  It also implements the WeakPointer.h interface.

This module assumes the following configuration options of the Boehm GC:

    -DALL_INTERIOR_POINTERS
    -DDONT_ADD_BYTE_AT_END   

This module adds its own required padding to the end of objects to
support C/C++ "one-past-the-object" pointer semantics.

****************************************************************************/

#include <stddef.h>
#include "gc.h"

#if defined(__STDC__) 
#   define PROTO( args ) args
#else
#    define PROTO( args ) ()
#    endif

#define BITSPERBYTE 8     
    /* What's the portable way to do this? */


typedef void (*vfp) PROTO(( void ));
extern vfp __new_handler;
extern void __default_new_handler PROTO(( void ));


/* A destructor_proc is the compiler generated procedure representing a 
C++ destructor.  The "flag" argument is a hidden argument following some
compiler convention. */

typedef (*destructor_proc) PROTO(( void* this, int flag ));


/***************************************************************************

A BI_header is the header the compiler adds to the front of
new-allocated arrays of objects with destructors.  The header is
padded out to a double, because that's what the compiler does to
ensure proper alignment of array elements on some architectures.  

int NUM_ARRAY_ELEMENTS (void* o)
    returns the number of array elements for array object o.

char* FIRST_ELEMENT_P (void* o)
    returns the address of the first element of array object o.

***************************************************************************/

typedef struct BI_header {
    int nelts;
    char padding [sizeof( double ) - sizeof( int )]; 
        /* Better way to do this? */
} BI_header;

#define NUM_ARRAY_ELEMENTS( o ) \
  (((BI_header*) o)->nelts)

#define FIRST_ELEMENT_P( o ) \
  ((char*) o + sizeof( BI_header ))


/***************************************************************************

The __builtin_new routines add a descriptor word to the end of each
object.   The descriptor serves two purposes.  

First, the descriptor acts as padding, implementing C/C++ pointer
semantics.  C and C++ allow a valid array pointer to be incremented
one past the end of an object.  The extra padding ensures that the
collector will recognize that such a pointer points to the object and
not the next object in memory.

Second, the descriptor stores three extra pieces of information,
whether an object has a registered finalizer (destructor), whether it
may have any weak pointers referencing it, and for collectible arrays,
the element size of the array.  The element size is required for the
array's finalizer to iterate through the elements of the array.  (An
alternative design would have the compiler generate a finalizer
procedure for each different array type.  But given the overhead of
finalization, there isn't any efficiency to be gained by that.)

The descriptor must be added to non-collectible as well as collectible
objects, since the Ellis/Detlefs proposal allows "pointer to gc T" to
be assigned to a "pointer to T", which could then be deleted.  Thus,
__builtin_delete must determine at runtime whether an object is
collectible, whether it has weak pointers referencing it, and whether
it may have a finalizer that needs unregistering.  Though
GC_REGISTER_FINALIZER doesn't care if you ask it to unregister a
finalizer for an object that doesn't have one, it is a non-trivial
procedure that does a hash look-up, etc.  The descriptor trades a
little extra space for a significant increase in time on the fast path
through delete.  (A similar argument applies to
GC_UNREGISTER_DISAPPEARING_LINK).

For non-array types, the space for the descriptor could be shrunk to a
single byte for storing the "has finalizer" flag.  But this would save
space only on arrays of char (whose size is not a multiple of the word
size) and structs whose largest member is less than a word in size
(very infrequent).  And it would require that programmers actually
remember to call "delete[]" instead of "delete" (which they should,
but there are probably lots of buggy programs out there).  For the
moment, the space savings seems not worthwhile, especially considering
that the Boehm GC is already quite space competitive with other
malloc's.


Given a pointer o to the base of an object:

Descriptor* DESCRIPTOR (void* o) 
     returns a pointer to the descriptor for o.

The implementation of descriptors relies on the fact that the GC
implementation allocates objects in units of the machine's natural
word size (e.g. 32 bits on a SPARC, 64 bits on an Alpha).

**************************************************************************/

typedef struct Descriptor {
    unsigned has_weak_pointers: 1;
    unsigned has_finalizer: 1;
    unsigned element_size: BITSPERBYTE * sizeof( unsigned ) - 2; 
} Descriptor;

#define DESCRIPTOR( o ) \
  ((Descriptor*) ((char*)(o) + GC_size( o ) - sizeof( Descriptor )))


/**************************************************************************

Implementations of global operator new() and operator delete()

***************************************************************************/


void* __builtin_new( size ) 
    size_t size;
    /* 
    For non-gc non-array types, the compiler generates calls to
    __builtin_new, which allocates non-collected storage via
    GC_MALLOC_UNCOLLECTABLE.  This ensures that the non-collected
    storage will be part of the collector's root set, required by the
    Ellis/Detlefs semantics. */
{
    vfp handler = __new_handler ? __new_handler : __default_new_handler;

    while (1) {
        void* o = GC_MALLOC_UNCOLLECTABLE( size + sizeof( Descriptor ) );
        if (o != 0) return o;
        (*handler) ();}}


void* __builtin_vec_new( size ) 
    size_t size;
    /* 
    For non-gc array types, the compiler generates calls to
    __builtin_vec_new. */
{
    return __builtin_new( size );}


void* __builtin_new_gc( size )
    size_t size;
    /* 
    For gc non-array types, the compiler generates calls to
    __builtin_new_gc, which allocates collected storage via
    GC_MALLOC. */
{
    vfp handler = __new_handler ? __new_handler : __default_new_handler;

    while (1) {
        void* o = GC_MALLOC( size + sizeof( Descriptor ) );
        if (o != 0) return o;
        (*handler) ();}}


void* __builtin_new_gc_a( size )
    size_t size;
    /* 
    For non-pointer-containing gc non-array types, the compiler
    generates calls to __builtin_new_gc_a, which allocates collected
    storage via GC_MALLOC_ATOMIC. */
{
    vfp handler = __new_handler ? __new_handler : __default_new_handler;

    while (1) {
        void* o = GC_MALLOC_ATOMIC( size + sizeof( Descriptor ) );
        if (o != 0) return o;
        (*handler) ();}}


void* __builtin_vec_new_gc( size )
    size_t size;
    /*
    For gc array types, the compiler generates calls to
    __builtin_vec_new_gc. */
{
    return __builtin_new_gc( size );}


void* __builtin_vec_new_gc_a( size )
    size_t size;
    /*
    For non-pointer-containing gc array types, the compiler generates
    calls to __builtin_vec_new_gc_a. */
{
    return __builtin_new_gc_a( size );}


static void call_destructor( o, data )
    void* o;
    void* data;
    /* 
    call_destructor is the GC finalizer proc registered for non-array
    gc objects with destructors.  Its client data is the destructor
    proc, which it calls with the magic integer 2, a special flag
    obeying the compiler convention for destructors. */
{
    ((destructor_proc) data)( o, 2 );}


void* __builtin_new_gc_dtor( o, d )
    void* o;
    destructor_proc d;
    /* 
    The compiler generates a call to __builtin_new_gc_dtor to register
    the destructor "d" of a non-array gc object "o" as a GC finalizer.
    The destructor is registered via
    GC_REGISTER_FINALIZER_IGNORE_SELF, which causes the collector to
    ignore pointers from the object to itself when determining when
    the object can be finalized.  This is necessary due to the self
    pointers used in the internal representation of multiply-inherited
    objects. */
{
    Descriptor* desc = DESCRIPTOR( o );

    GC_REGISTER_FINALIZER_IGNORE_SELF( o, call_destructor, d, 0, 0 );
    desc->has_finalizer = 1;}


static void call_array_destructor( o, data )
    void* o;
    void* data;
    /*
    call_array_destructor is the GC finalizer proc registered for gc
    array objects whose elements have destructors. Its client data is
    the destructor proc.  It iterates through the elements of the
    array in reverse order, calling the destructor on each. */
{
    int num = NUM_ARRAY_ELEMENTS( o );
    Descriptor* desc = DESCRIPTOR( o );
    size_t size = desc->element_size;
    char* first_p = FIRST_ELEMENT_P( o );
    char* p = first_p + (num - 1) * size;

    if (num > 0) {
        while (1) {
            ((destructor_proc) data)( p, 2 );
            if (p == first_p) break;
            p -= size;}}}


void* __builtin_vec_new_gc_dtor( first_elem, d, element_size )
    void* first_elem;
    destructor_proc d;
    size_t element_size;
    /* 
    The compiler generates a call to __builtin_vec_new_gc_dtor to
    register the destructor "d" of a gc array object as a GC
    finalizer.  "first_elem" points to the first element of the array,
    *not* the beginning of the object (this makes the generated call
    to this function smaller).  The elements of the array are of size
    "element_size".  The destructor is registered as in
    _builtin_new_gc_dtor. */
{
    void* o = (char*) first_elem - sizeof( BI_header );
    Descriptor* desc = DESCRIPTOR( o );

    GC_REGISTER_FINALIZER_IGNORE_SELF( o, call_array_destructor, d, 0, 0 );
    desc->element_size = element_size;
    desc->has_finalizer = 1;}


void __builtin_delete( o )
    void* o;
    /* 
    The compiler generates calls to __builtin_delete for operator
    delete().  The GC currently requires that any registered
    finalizers be unregistered before explicitly freeing an object.
    If the object has any weak pointers referencing it, we can't
    actually free it now. */
{
  if (o != 0) { 
      Descriptor* desc = DESCRIPTOR( o );
      if (desc->has_finalizer) GC_REGISTER_FINALIZER( o, 0, 0, 0, 0 );
      if (! desc->has_weak_pointers) GC_FREE( o );}}


void __builtin_vec_delete( o )
    void* o;
    /* 
    The compiler generates calls to __builitn_vec_delete for operator
    delete[](). */
{
  __builtin_delete( o );}


/**************************************************************************

Implementations of the template class WeakPointer from WeakPointer.h

***************************************************************************/

typedef struct WeakPointer {
    void* pointer; 
} WeakPointer;


void* _WeakPointer_New( t )
    void* t;
{
    if (t == 0) {
        return 0;}
    else {
        void* base = GC_base( t );
        WeakPointer* wp = 
            (WeakPointer*) GC_MALLOC_ATOMIC( sizeof( WeakPointer ) );
        Descriptor* desc = DESCRIPTOR( base );

        wp->pointer = t;
        desc->has_weak_pointers = 1;
        GC_general_register_disappearing_link( &wp->pointer, base );
        return wp;}}


static void* PointerWithLock( wp ) 
    WeakPointer* wp;
{
    if (wp == 0 || wp->pointer == 0) {
      return 0;}
    else {
        return (void*) wp->pointer;}}


void* _WeakPointer_Pointer( wp )
    WeakPointer* wp;
{
    return (void*) GC_call_with_alloc_lock( PointerWithLock, wp );}


typedef struct EqualClosure {
    WeakPointer* wp1;
    WeakPointer* wp2;
} EqualClosure;


static void* EqualWithLock( ec )
    EqualClosure* ec;
{
    if (ec->wp1 == 0 || ec->wp2 == 0) {
        return (void*) (ec->wp1 == ec->wp2);}
    else {
      return (void*) (ec->wp1->pointer == ec->wp2->pointer);}}


int _WeakPointer_Equal( wp1,  wp2 )
    WeakPointer* wp1;
    WeakPointer* wp2;
{
    EqualClosure ec;

    ec.wp1 = wp1;
    ec.wp2 = wp2;
    return (int) GC_call_with_alloc_lock( EqualWithLock, &ec );}


int _WeakPointer_Hash( wp )
    WeakPointer* wp;
{
    return (int) _WeakPointer_Pointer( wp );}


/**************************************************************************

Implementations of the template class CleanUp from WeakPointer.h

***************************************************************************/

typedef struct Closure {
    void (*c) PROTO(( void* d, void* t ));
    ptrdiff_t t_offset; 
    void* d;
} Closure;


static void _CleanUp_CallClosure( obj, data ) 
    void* obj;
    void* data;
{
    Closure* closure = (Closure*) data;
    closure->c( closure->d, (char*) obj + closure->t_offset );}


void _CleanUp_Set( t, c, d ) 
    void* t;
    void (*c) PROTO(( void* d, void* t ));
    void* d;
{
    void* base = GC_base( t );
    Descriptor* desc = DESCRIPTOR( t );

    if (c == 0) {
        GC_REGISTER_FINALIZER_IGNORE_SELF( base, 0, 0, 0, 0 );
        desc->has_finalizer = 0;}
    else {
        Closure* closure = (Closure*) GC_MALLOC( sizeof( Closure ) );
        closure->c = c;
        closure->t_offset = (char*) t - (char*) base;
        closure->d = d;
        GC_REGISTER_FINALIZER_IGNORE_SELF( base, _CleanUp_CallClosure, 
                                           closure, 0, 0 );
        desc->has_finalizer = 1;}}


void _CleanUp_Call( t ) 
    void* t;
{
      /* ? Aren't we supposed to deactivate weak pointers to t too? 
         Why? */
    void* base = GC_base( t );
    void* d;
    GC_finalization_proc f;

    GC_REGISTER_FINALIZER( base, 0, 0, &f, &d );
    f( base, d );}


typedef struct QueueElem {
    void* o;
    GC_finalization_proc f;
    void* d;
    struct QueueElem* next; 
} QueueElem;


void* _CleanUp_Queue_NewHead()
{
    return GC_MALLOC( sizeof( QueueElem ) );}
    
     
static void _CleanUp_Queue_Enqueue( obj, data )
    void* obj; 
    void* data;
{
    QueueElem* q = (QueueElem*) data;
    QueueElem* head = q->next;

    q->o = obj;
    q->next = head->next;
    head->next = q;}
    
    
void _CleanUp_Queue_Set( h, t ) 
    void* h;
    void* t;
{
    QueueElem* head = (QueueElem*) h;
    void* base = GC_base( t );
    void* d;
    GC_finalization_proc f;
    QueueElem* q = (QueueElem*) GC_MALLOC( sizeof( QueueElem ) );
     
    GC_REGISTER_FINALIZER( base, _CleanUp_Queue_Enqueue, q, &f, &d );
    q->f = f;
    q->d = d;
    q->next = head;}
    

int _CleanUp_Queue_Call( h ) 
    void* h;
{
    QueueElem* head = (QueueElem*) h;
    QueueElem* q = head->next;

    if (q == 0) {
        return 0;}
    else {
        head->next = q->next;
        q->next = 0;
        if (q->f != 0) q->f( q->o, q->d );
        return 1;}}



