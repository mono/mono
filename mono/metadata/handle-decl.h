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
	G_ALWAYS_INLINE bool operator==(std::nullptr_t) { return !p; }
	G_ALWAYS_INLINE bool operator!=(std::nullptr_t) { return !!p; }

	// Allow silent conversion to matching MonoHandle.
	G_ALWAYS_INLINE operator MonoHandle<T> () { return NewHandle<T>(); }

	// Allow silent conversion to MonoObjectHandle, no matter what T is, unless T is
	// already MonoObject.
	template < typename U,
	    	   typename = typename std::enable_if<!std::is_same<T, MonoObject>::value &&
						      std::is_same<U, MonoObject>::value >::type>
	G_ALWAYS_INLINE operator MonoHandle<U> () { return NewHandle<U>(); }

	G_ALWAYS_INLINE bool operator==(const void* q) const { return p == q; }
	G_ALWAYS_INLINE bool operator!=(const void* q) const { return p != q; }
	friend bool operator==(const void* q, const MonoPtr p) { return p.p == q; }
	friend bool operator!=(const void* q, const MonoPtr p) { return p.p != q; }

	template <typename U> MonoHandle<U> G_ALWAYS_INLINE NewHandle () { return MonoHandle<U> ().New ((U*)GetRaw()); }

	G_ALWAYS_INLINE MonoPtr& operator = (MonoHandle<T> h) { p = h.__raw ? *h.__raw : 0; return *this; }

	G_ALWAYS_INLINE explicit operator bool () const { return !!p; }

/******************************************************************************************
This is important. But this also hides costs. We will consider removing this.
auto a = b->c->d create two handles
maybe prefer:
auto c = mono_new_handle (b->c);
auto d = mono_new_handle (c->d);

It might also help if in
	auto a = b->c->d
c was temporary and replaced by d/a.
******************************************************************************************/
	MonoHandle<T> operator -> () { return NewHandle<T> (); }

	//MonoPtr& operator = (std::nullptr_t) { p = 0; return *this; }
	MonoPtr& operator = (MonoPtr q) { return operator =(q.p); }
	MonoPtr& operator = (T* q)
	{
		// NOTE The first parameter is not used and is not available.
		mono_gc_wbarrier_set_field (NULL, &p, (MonoObject*)q);
		return *this;
	}

	// hopefully used sparingly, but e.g. printf ("%p", x.get ());
	// printf ("%p", x) will work on some ABIs/compilers but probably not all.
	//T* get () { return p; }

	//FIXME? This is used in non-coop-converted code.
	// It might also be a good printf("%p") piece.
	// This should not be called something innocuous like "get" as it is critical to audit for.
	G_ALWAYS_INLINE T* GetRaw () const { return p; }
	G_ALWAYS_INLINE MonoObject* GetRawObj () const { return (MonoObject*)GetRaw(); }

	struct OperatorAmpersandResult
	{
		MonoPtr<T>* p;

		operator T** () { return &p->p; }
		operator void** () { return (void**)&p->p; }
		operator void* () { return (void*)&p->p; } // FIXME? MONO_HANDLE_SET MONO_OBJECT_SETREF mono_gc_wbarrier_set_field
		//operator MonoPtr<T>* () { return p; }
	};
	OperatorAmpersandResult operator & () { return OperatorAmpersandResult {this}; }

/******************************************************************************************
This is important, to not have.
However it is useful to add, if you do not want to sprinkle GetRaw in non-coop code.
And then remove for crude static analysis.
******************************************************************************************/
	//operator T * () { return p; }

	//MonoHandle<MonoObject> AsMonoObjectHandle();

//private:
	T * p;
};

#define MonoPtr(x) MonoPtr<x>

#else

#define MonoPtr(x) x*

#endif

#ifdef __cplusplus //experimental

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

	G_ALWAYS_INLINE MonoHandle& Init () { __raw = 0; return *this; }

	MonoHandle NewHandle ();

	//G_ALWAYS_INLINE bool operator==(std::nullptr_t) { return !GetRaw (); }
	//G_ALWAYS_INLINE bool operator!=(std::nullptr_t) { return !!GetRaw (); }

	G_ALWAYS_INLINE bool operator==(const void* q) const { return GetRaw () == q; }
	G_ALWAYS_INLINE bool operator!=(const void* q) const  { return GetRaw () != q; }

/*
	// Compare anything to MonoObject, but not MonoObject as that is already handled.
	template <typename U,
	    	  typename = typename std::enable_if <!std::is_same <T, MonoObject>::value &&
						      std::is_same <U, MonoObject>::value>::type>
	G_ALWAYS_INLINE bool operator==(U* q) { return GetRaw () == (void*)q; }

	// Compare anything to MonoObject, but not MonoObject as that is already handled.
	template <typename U,
	    	  typename = typename std::enable_if <!std::is_same <T, MonoObject>::value &&
						      std::is_same <U, MonoObject>::value>::type>
	G_ALWAYS_INLINE bool operator!=(U* q) { return GetRaw () != (void*)q; }
*/
	G_ALWAYS_INLINE MonoHandle return_handle (MonoHandleFrame& frame)
	{
		// FIXME NULL is NULL or always allocate?
		return MonoHandle{(T**)frame.allocate_handle_in_caller (GetRaw ())};
	}

	template <typename T2>
	G_ALWAYS_INLINE MonoHandle<T2> cast () const
	{
		return MonoHandle<T2>{(T2**)__raw};
	}

	G_ALWAYS_INLINE explicit operator bool () const { return __raw && *__raw; }

	MonoHandle& New (T* value = 0);

	G_ALWAYS_INLINE MonoHandle& New (MonoPtr<T> value) { return New (value.GetRaw ()); }

	// Overload for type mismatches. MonoObjectHandle.New (MonoRealProxy*).
	// FIXME This is handled better in future by MonoRealProxy derives from MonoObject.
	template <typename U,
	    	  typename = typename std::enable_if <std::is_same <T, MonoObject>::value &&
						      std::is_same <U, MonoRealProxy>::value>::type>
	G_ALWAYS_INLINE MonoHandle& New (U* value)
	{
		return New ((MonoObject*)value);
	}

	static MonoHandle static_new (T * value = 0);

	// FIXME? Not safe but has its current uses.
	G_ALWAYS_INLINE void* ForInvoke () { return GetRaw (); }

	// FIXME? G_ALWAYS_INLINE MonoHandle<MonoObject> AsMonoObjectHandle () { return cast <MonoObject> (); }

	// Silent conversion to MonoObjectHandle, if T != MonoObject.
	template < typename U,
	    	   typename = typename std::enable_if<!std::is_same<T, MonoObject>::value &&
						       std::is_same<U, MonoObject>::value >::type>
	operator MonoHandle<U> () { return cast <MonoObject> (); }

	G_ALWAYS_INLINE T* GetRaw () const { return __raw ? *__raw : NULL; }
	G_ALWAYS_INLINE MonoObject* GetRawObj () const { return (MonoObject*)GetRaw(); }

	// not safe -- use GetRaw and possibly cast.
	//G_ALWAYS_INLINE MonoObject* AsMonoObjectPtr () { return (MonoObject*)GetRaw();}

	// not safe -- use GetRaw.
	//G_ALWAYS_INLINE operator T * () { return get (); } // FIXME?

	// not safe -- use GetRaw.
	//G_ALWAYS_INLINE T * get() { return __raw ? *__raw : NULL; } // FIXME?

	// not safe -- use GetRaw and possibly cast.
	// if T != MonoObject, provide operator MonoObject*
	//template < typename U = T,
	//    	   typename = typename std::enable_if< !std::is_same<U, MonoObject>::value >::type>
	//G_ALWAYS_INLINE
	//operator MonoObject* () { return (MonoObject*)get (); } // FIXME?

	void new_pinned (MonoDomain *domain, MonoClass *klass, MonoError *error);

	G_ALWAYS_INLINE MonoHandle& cross_frame_assign (MonoHandle p) // i.e. mono_handle_assign
	{
		g_assert (__raw);
		*__raw = p.GetRaw ();
		return *this;
	}

	G_ALWAYS_INLINE
	MonoHandle& operator=(MonoHandle p)
	{
		// FIXME *__raw = *p.__raw; ?
		// This is the old behavior and it is not obvious
		// which is better. They each have advantages and disadvantages.
		// *__raw is an extra dereference, and requires New().
		// __raw is old behaviorr
		// *__raw moves the handle across frames (if the handles
		// are in different frames) which can be useful.
		// *__raw is what mono_handle_assign does.
		__raw = p.__raw;
		return *this;
	}

	//G_ALWAYS_INLINE MonoHandle& operator=(MonoPtr<T> p) { g_assert (__raw); *__raw = p; return *this; }
	G_ALWAYS_INLINE MonoHandle& operator=(T* p) { g_assert (__raw); *__raw = p; return *this; }

/******************************************************************************************
Note. This is both the point and the danger.
The safety of this, depends on T* itself not having raw pointers, but only MonoPtr().
******************************************************************************************/
	G_ALWAYS_INLINE
	T* operator-> () { g_assert (__raw); return *__raw; }

//private:
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
static inline G_ALWAYS_INLINE TYPED_HANDLE_NAME (TYPE) 	\
MONO_HANDLE_CAST_FOR (TYPE) (gpointer a)			\
{								\
	TYPED_HANDLE_NAME (TYPE) b = { (TYPE**)a };		\
	return b;						\
}								\
static inline G_ALWAYS_INLINE MonoObject* 			\
MONO_HANDLE_TYPECHECK_FOR (TYPE) (TYPE *a)			\
{								\
	return (MonoObject*)a;					\
}

#else

#if MONO_TYPE_SAFE_HANDLES
#define TYPED_HANDLE_DECL(TYPE)							\
	typedef struct {							\
		MONO_IF_CPLUSPLUS (						\
			MONO_ALWAYS_INL130INE					\
			TYPE * GetRaw () { return __raw ? *__raw : NULL; }	\
		)								\
		TYPE **__raw;							\
	} TYPED_HANDLE_PAYLOAD_NAME (TYPE),					\
	  TYPED_HANDLE_NAME (TYPE),						\
	  TYPED_OUT_HANDLE_NAME (TYPE);						\
/* Do not call these functions directly. Use MONO_HANDLE_NEW and MONO_HANDLE_CAST. */ \
/* Another way to do this involved casting mono_handle_new function to a different type. */ \
static inline G_ALWAYS_INLINE TYPED_HANDLE_NAME (TYPE) 	\
MONO_HANDLE_CAST_FOR (TYPE) (gpointer a)			\
{								\
	TYPED_HANDLE_NAME (TYPE) b = { (TYPE**)a };		\
	return b;						\
}								\
static inline G_ALWAYS_INLINE MonoObject* 			\
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

#endif // experimental C++

/*
 * TYPED_VALUE_HANDLE_DECL(SomeType):
 *   Expands to a decl for handles to SomeType (which is a managed valuetype (likely a struct) of some sort) and to an internal payload struct.
 * It is currently identical to TYPED_HANDLE_DECL (valuetypes vs. referencetypes).
 */
#define TYPED_VALUE_HANDLE_DECL(TYPE) TYPED_HANDLE_DECL(TYPE)

#endif /* __MONO_HANDLE_DECL_H__ */
