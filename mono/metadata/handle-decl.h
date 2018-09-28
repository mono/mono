/**
 * \file
 * Handle to object in native code
 *
 * Authors:
 *  - Ludovic Henry <ludovic@xamarin.com>
 *  - Aleksey Klieger <aleksey.klieger@xamarin.com>
 *  - Rodrigo Kumpera <kumpera@xamarin.com>
 *
 * Copyright 2016 Dot net foundation.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#ifndef __MONO_HANDLE_DECL_H__
#define __MONO_HANDLE_DECL_H__

#include <config.h>
#include <glib.h>
#include <mono/metadata/object-forward.h>
#include <mono/utils/mono-compiler.h>

// FIXME no duplicate declaration
typedef union _MonoError MonoError;

#ifdef __cplusplus
#define MONO_IF_CPLUSPLUS(x) x
#else
#define MONO_IF_CPLUSPLUS(x) /* nothing */
#endif

// Type-safe handles are a struct with a pointer to pointer.
// The only operations allowed on them are the functions/macros in this file, and assignment
// from same handle type to same handle type.
//
// Type-unsafe handles are a pointer to a struct with a pointer.
// Besides the type-safe operations, these can also be:
//  1. compared to NULL, instead of only MONO_HANDLE_IS_NULL
//  2. assigned from NULL, instead of only a handle
//  3. MONO_HANDLE_NEW (T) from anything, instead of only a T*
//  4. MONO_HANDLE_CAST from anything, instead of only another handle type
//  5. assigned from any void*, at least in C
//  6. Cast from any handle type to any handle type, without using MONO_HANDLE_CAST.
//  7. Cast from any handle type to any pointer type and vice versa, such as incorrect unboxing.
//  8. mono_object_class (handle), instead of mono_handle_class
//
// None of those operations were likely intended.
//
// FIXME Do this only on checked builds? Or certain architectures?
// There is not runtime cost.
// NOTE: Running this code depends on the ABI to pass a struct
// with a pointer the same as a pointer. This is tied in with
// marshaling. If this is not the case, turn off type-safety, perhaps per-OS per-CPU.
// FIXME
//#if defined (HOST_DARWIN) || defined (HOST_WIN32) || defined (HOST_ARM64) || defined (HOST_ARM) || defined (HOST_AMD64)
#define MONO_TYPE_SAFE_HANDLES 1
//#else
//#define MONO_TYPE_SAFE_HANDLES 0 // PowerPC, S390X, SPARC, MIPS, Linux/x86, BSD/x86, etc.
//#endif

/*
Handle macros/functions
*/

#define TYPED_HANDLE_PAYLOAD_NAME(TYPE) TYPE ## HandlePayload
#define TYPED_HANDLE_NAME(TYPE) TYPE ## Handle
#define TYPED_OUT_HANDLE_NAME(TYPE) TYPE ## HandleOut

// internal helpers:
#define MONO_HANDLE_CAST_FOR(type) mono_handle_cast_##type
#define MONO_HANDLE_TYPECHECK_FOR(type) mono_handle_typecheck_##type

struct _MonoThreadInfo;
typedef struct _MonoThreadInfo MonoThreadInfo;

/*
Handle stack.

The handle stack is designed so it's efficient to pop a large amount of entries at once.
The stack is made out of a series of fixed size segments.

To do bulk operations you use a stack mark.
	
*/

/*
3 is the number of fields besides the data in the struct;
128 words makes each chunk 512 or 1024 bytes each
*/
#define OBJECTS_PER_HANDLES_CHUNK (128 - 3)

typedef struct _HandleChunk HandleChunk;

/*
 * Define MONO_HANDLE_TRACK_OWNER to store the file and line number of each call to MONO_HANDLE_NEW
 * in the handle stack.  (This doubles the amount of memory used for handles, so it's only useful for debugging).
 */
/*#define MONO_HANDLE_TRACK_OWNER*/

/*
 * Define MONO_HANDLE_TRACK_SP to record the C stack pointer at the time of each HANDLE_FUNCTION_ENTER and
 * to ensure that when a new handle is allocated the previous newest handle is not lower in the stack.
 * This is useful to catch missing HANDLE_FUNCTION_ENTER / HANDLE_FUNCTION_RETURN pairs which could cause
 * handle leaks.
 *
 * If defined, keep HandleStackMark in sync in RuntimeStructs.cs
 */
/*#define MONO_HANDLE_TRACK_SP*/

typedef struct {
	gpointer o; /* MonoObject ptr or interior ptr */
#ifdef MONO_HANDLE_TRACK_OWNER
	const char *owner;
	gpointer backtrace_ips[7]; /* result of backtrace () at time of allocation */
#endif
#ifdef MONO_HANDLE_TRACK_SP
	gpointer alloc_sp; /* sp from HandleStack:stackmark_sp at time of allocation */
#endif
} HandleChunkElem;

struct _HandleChunk {
	int size; //number of handles
	HandleChunk *prev, *next;
	HandleChunkElem elems [OBJECTS_PER_HANDLES_CHUNK];
};

// Keep this in sync with RuntimeStructs.cs
typedef struct {
	int size, interior_size;
	HandleChunk *chunk;
#ifdef MONO_HANDLE_TRACK_SP
	gpointer prev_sp; // C stack pointer from prior mono_stack_mark_init
#endif
} HandleStackMark;

/*
 * TYPED_HANDLE_DECL(SomeType):
 *   Expands to a decl for handles to SomeType and to an internal payload struct.
 *
 * For example, TYPED_HANDLE_DECL(MonoObject) (see below) expands to:
 *
 * #if MONO_TYPE_SAFE_HANDLES
 *
 * typedef struct {
 *   MonoObject **__raw;
 * } MonoObjectHandlePayload,
 *   MonoObjectHandle,
 *   MonoObjectHandleOut;
 *
 * Internal helper functions are also generated.
 *
 * #else
 *
 * typedef struct {
 *   MonoObject *__raw;
 * } MonoObjectHandlePayload;
 *
 * typedef MonoObjectHandlePayload* MonoObjectHandle;
 * typedef MonoObjectHandlePayload* MonoObjectHandleOut;
 *
 * #endif
 */

#ifdef __cplusplus
template <typename T> struct MonoHandle;
#endif

#ifdef __cplusplus //experimental

//template <typename T> struct MonoPtr;

#include <type_traits>

template <typename T> struct MonoPtr
{
	bool operator==(const void* q) const { return p == q; }
	bool operator!=(const void* q) const { return p != q; }
	friend bool operator==(const void* q, const MonoPtr p) { return p.p == q; }
	friend bool operator!=(const void* q, const MonoPtr p) { return p.p != q; }

	//template <typename U = T> MonoHandle<U> NewHandle () { return MonoHandle<U> ().New ((U*)GetRaw()); }
	MonoHandle<T> NewHandle () { return MonoHandle<T> ().New (GetRaw()); }

	explicit operator bool () const { return !!p; }

	// This works but is perhaps too automatic.
	//MonoHandle<T> operator -> () { return NewHandle <T> (); }

	MonoPtr& operator = (MonoHandle<T> h) { return operator = (h.GetRaw ()); }
	//MonoPtr& operator = (MonoPtr q) { return operator =(q.p); }
	MonoPtr& operator = (T* q)
	{
		// NOTE The first parameter is not used and is not available.
		mono_gc_wbarrier_set_field (NULL, &p, (MonoObject*)q);
		return *this;
	}

	T* GetRaw () const { return p; }
	T** GetRawAddress () { return &p; }
	MonoObject* GetRawObj () const { return (MonoObject*)GetRaw(); }

	struct OperatorAmpersandResult
	{
		MonoPtr<T>* p;
		operator T** () { return &p->p; }
		operator void** () { return (void**)&p->p; }
		operator void* () { return (void*)&p->p; }
	};
	//OperatorAmpersandResult operator & () { return OperatorAmpersandResult {this}; }

/******************************************************************************************
This is important, to not have.
However it is useful to add, if you do not want to sprinkle GetRaw in non-coop code.
And then remove for crude static analysis.
******************************************************************************************/
	//operator T * () { return p; }

private:
	T * p;
};

#define MonoPtr(x) MonoPtr<x>

#else

#define MonoPtr(x) x*

#endif

#ifdef __cplusplus

struct MonoHandleFrame
{
	MonoHandleFrame ();
	~MonoHandleFrame ();
	void **allocate_handle_in_caller (void* value);
private:
	void pop ();
	bool do_pop;
	HandleStackMark stackmark;
	MonoThreadInfo *threadinfo;
};

template <typename T>
struct MonoHandle
{
	// FIXME in future this should have a constructor
	// It lacks constructor and destructor for JIT interop.
	// FIXME? Naming style: Init or init or other?
	MonoHandle& Init () { __raw = 0; return *this; }

	template <typename U>
	bool operator== (MonoHandle <U> q) const { return GetRaw () == q.GetRaw (); }

	template <typename U>
	bool operator!= (MonoHandle <U> q) const { return GetRaw () != q.GetRaw (); }

	template <typename U>
	bool operator== (MonoPtr <U> q) const { return GetRaw () == q.GetRaw (); }

	template <typename U>
	bool operator!= (MonoPtr <U> q) const { return GetRaw () != q.GetRaw (); }

	bool operator== (const void* q) const { return GetRaw () == q; }
	bool operator!= (const void* q) const  { return GetRaw () != q; }
	friend bool operator== (const void* q, const MonoHandle p) { return p.GetRaw () == q; }
	friend bool operator!= (const void* q, const MonoHandle p) { return p.GetRaw () != q; }

	// FIXME? Naming style: return_handle or ReturnHandle or other?
	MonoHandle return_handle (MonoHandleFrame& frame)
	{
		// FIXME NULL is NULL or always allocate?
		return MonoHandle{(T**)frame.allocate_handle_in_caller (GetRaw ())};
	}

	// FIXME? Naming style: Cast or cast?
	template <typename T2>
	MonoHandle<T2> cast () const
	{
		return MonoHandle<T2>{(T2**)__raw};
	}

	// This enables `if (handle)`, as the universal validity check.
	// No need for `if (p != NULL)` or `if (IsValid(p))` or `if (p.IsValid())`
	// Those are all ok in general, but the goal is uniformity across types, so generally `if (p)`.
	// The exception is Windows INVALID_HANDLE_VALUE on raw types, but classes can fix that.
	explicit operator bool () const { return !!GetRaw (); }

	// FIXME? Is this too search? Prefer NewHandle or new_handle for search?
	MonoHandle& New (T* value = 0);
	MonoHandle& New (MonoPtr<T> value) { return New (value.GetRaw ()); }

	// FIXME? Not safe but has its current uses.
	void* ForInvoke () { return GetRaw (); }

	// Silent conversion to MonoObjectHandle, if T != MonoObject.
	template < typename U,
	    	   typename = typename std::enable_if<!std::is_same<T, MonoObject>::value &&
						       std::is_same<U, MonoObject>::value >::type>
	operator MonoHandle<U> () { return cast <MonoObject> (); }

	// FIXME? Naming style: GetRaw or get_raw or getRaw?
	T* GetRaw () const { return __raw ? *__raw : NULL; }
	MonoObject* GetRawObj () const { return (MonoObject*)GetRaw(); }

	// FIXME? Naming style: NewPinned or new_pinned or newPinned or other?
	void new_pinned (MonoDomain *domain, MonoClass *klass, MonoError *error);

	// FIXME? Naming style: Assign or assign?
	MonoHandle& assign (MonoHandle p)
	// i.e. mono_handle_assign
	// Note this is different than operator =, which is defaulted, same as C.
	{
		g_assert (__raw);
		*__raw = p.GetRaw ();
		return *this;
	}

	MonoHandle& operator=(MonoPtr<T> p) { return operator = (p.GetRaw ()); }
	MonoHandle& operator=(T* p) { g_assert (__raw); *__raw = p; return *this; }

/******************************************************************************************
Note. This is both the point and the danger.
The safety of this, depends on T* itself not having raw pointers, but only MonoPtr().
******************************************************************************************/
	T* operator-> () { g_assert (__raw); return *__raw; }

//private: // FIXME
	T ** __raw;
};

#define TYPED_HANDLE_DECL(TYPE)							\
	typedef MonoHandle<TYPE> 						\
	TYPED_HANDLE_PAYLOAD_NAME (TYPE),					\
	  TYPED_HANDLE_NAME (TYPE),						\
	  TYPED_OUT_HANDLE_NAME (TYPE);						\
/* Do not call these functions directly. Use MONO_HANDLE_NEW and MONO_HANDLE_CAST. */ \
/* Another way to do this involved casting mono_handle_new function to a different type. */ \
/* FIXME Are these needed in C++? */ \
static inline TYPED_HANDLE_NAME (TYPE) 	\
MONO_HANDLE_CAST_FOR (TYPE) (gpointer a)			\
{								\
	TYPED_HANDLE_NAME (TYPE) b = { (TYPE**)a };		\
	return b;						\
}								\
static inline MonoObject* 			\
MONO_HANDLE_TYPECHECK_FOR (TYPE) (TYPE *a)			\
{								\
	return (MonoObject*)a;					\
}

#else

#if MONO_TYPE_SAFE_HANDLES
#define TYPED_HANDLE_DECL(TYPE)							\
	typedef struct {							\
		TYPE **__raw;							\
	} TYPED_HANDLE_PAYLOAD_NAME (TYPE),					\
	  TYPED_HANDLE_NAME (TYPE),						\
	  TYPED_OUT_HANDLE_NAME (TYPE);						\
/* Do not call these functions directly. Use MONO_HANDLE_NEW and MONO_HANDLE_CAST. */ \
/* Another way to do this involved casting mono_handle_new function to a different type. */ \
static inline TYPED_HANDLE_NAME (TYPE) 	\
MONO_HANDLE_CAST_FOR (TYPE) (gpointer a)			\
{								\
	TYPED_HANDLE_NAME (TYPE) b = { (TYPE**)a };		\
	return b;						\
}								\
static inline MonoObject* 			\
MONO_HANDLE_TYPECHECK_FOR (TYPE) (TYPE *a)			\
{								\
	return (MonoObject*)a;					\
}

#else

#define TYPED_HANDLE_DECL(TYPE)						\
	typedef struct { TYPE *__raw; } TYPED_HANDLE_PAYLOAD_NAME (TYPE) ; \
	typedef TYPED_HANDLE_PAYLOAD_NAME (TYPE) * TYPED_HANDLE_NAME (TYPE); \
	typedef TYPED_HANDLE_PAYLOAD_NAME (TYPE) * TYPED_OUT_HANDLE_NAME (TYPE);
#endif // typesafe handles
#endif

/*
 * TYPED_VALUE_HANDLE_DECL(SomeType):
 *   Expands to a decl for handles to SomeType (which is a managed valuetype (likely a struct) of some sort) and to an internal payload struct.
 * It is currently identical to TYPED_HANDLE_DECL (valuetypes vs. referencetypes).
 */
#define TYPED_VALUE_HANDLE_DECL(TYPE) TYPED_HANDLE_DECL(TYPE)

#endif /* __MONO_HANDLE_DECL_H__ */
