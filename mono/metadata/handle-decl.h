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
#if defined (HOST_DARWIN) || defined (HOST_WIN32) || defined (HOST_ARM64) || defined (HOST_ARM) || defined (HOST_AMD64)
#define MONO_TYPE_SAFE_HANDLES 1
#else
#define MONO_TYPE_SAFE_HANDLES 0 // PowerPC, S390X, SPARC, MIPS, Linux/x86, BSD/x86, etc.
#endif

/*
Handle macros/functions
*/

#define TYPED_HANDLE_PAYLOAD_NAME(TYPE) TYPE ## HandlePayload
#define TYPED_HANDLE_NAME(TYPE) TYPE ## Handle
#define TYPED_OUT_HANDLE_NAME(TYPE) TYPE ## HandleOut

// internal helpers:
#define MONO_HANDLE_CAST_FOR(type) mono_handle_cast_##type
#define MONO_HANDLE_TYPECHECK_FOR(type) mono_handle_typecheck_##type

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
#define MONO_IF_CPLUSPLUS(x) x
#else
#define MONO_IF_CPLUSPLUS(x) /* nothing */
#endif

#if MONO_TYPE_SAFE_HANDLES
#define TYPED_HANDLE_DECL(TYPE)							\
	typedef struct {							\
		MONO_IF_CPLUSPLUS (						\
			MONO_ALWAYS_INLINE					\
			TYPE * GetRaw () { return __raw ? *__raw : NULL; }	\
		)								\
		TYPE **__raw;							\
	} TYPED_HANDLE_PAYLOAD_NAME (TYPE),					\
	  TYPED_HANDLE_NAME (TYPE),						\
	  TYPED_OUT_HANDLE_NAME (TYPE);						\
/* Do not call these functions directly. Use MONO_HANDLE_NEW and MONO_HANDLE_CAST. */ \
/* Another way to do this involved casting mono_handle_new function to a different type. */ \
static inline MONO_ALWAYS_INLINE TYPED_HANDLE_NAME (TYPE) 	\
MONO_HANDLE_CAST_FOR (TYPE) (gpointer a)			\
{								\
	TYPED_HANDLE_NAME (TYPE) b = { (TYPE**)a };		\
	return b;						\
}								\
static inline MONO_ALWAYS_INLINE MonoObject* 			\
MONO_HANDLE_TYPECHECK_FOR (TYPE) (TYPE *a)			\
{								\
	return (MonoObject*)a;					\
}

#else
#define TYPED_HANDLE_DECL(TYPE)						\
	typedef struct { TYPE *__raw; } TYPED_HANDLE_PAYLOAD_NAME (TYPE) ; \
	typedef TYPED_HANDLE_PAYLOAD_NAME (TYPE) * TYPED_HANDLE_NAME (TYPE); \
	typedef TYPED_HANDLE_PAYLOAD_NAME (TYPE) * TYPED_OUT_HANDLE_NAME (TYPE);
#endif

/*
 * TYPED_VALUE_HANDLE_DECL(SomeType):
 *   Expands to a decl for handles to SomeType (which is a managed valuetype (likely a struct) of some sort) and to an internal payload struct.
 * It is currently identical to TYPED_HANDLE_DECL (valuetypes vs. referencetypes).
 */
#define TYPED_VALUE_HANDLE_DECL(TYPE) TYPED_HANDLE_DECL(TYPE)

#ifdef __cplusplus //experimental

template <typename T> struct MonoPtr;
template <typename T> struct MonoHandle;

template <typename T> struct MonoPtr
{
	MonoPtr& operator = (MonoHandle<T> h) { p = *h.raw; return *this; }
	MonoPtr& operator = (MonoPtr<T> q) { p = q.p; return *this; }
	MonoPtr& operator = (T* q) { p = q; return *this; }
	operator T * () { return p; }
	T* operator -> () { return p; }

	struct OperatorAmpersandResult
	{
		MonoPtr<T>* p;

		operator T** () { return &p->p; }
		operator void** () { return (void**)&p->p; }
		operator void* () { return (void*)&p->p; } // FIXME? MONO_HANDLE_SET MONO_OBJECT_SETREF mono_gc_wbarrier_set_field
		//operator MonoPtr<T>* () { return p; }
	};

	OperatorAmpersandResult operator & () { return OperatorAmpersandResult {this}; }

	// hopefully used sparingly, but e.g. printf ("%p", x.get ());
	// printf ("%p", x) will work on some ABIs/compilers but probably not all.
	T* get () { return p; }

//private:
	T * p;
};

#define MonoPtr(x) MonoPtr<x>

#else

#define MonoPtr(x) x*

#endif

#ifdef __cplusplus //experimental

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

//void mono_stack_mark_record_size (MonoThreadInfo *info, HandleStackMark *stackmark, const char *func_name);

extern "C++" {

struct MonoHandleFrame
{
	void **allocate_handle_in_caller (void* value = 0);

	MonoHandleFrame ();

	~MonoHandleFrame ();

	void** allocate_handle (void* value = 0);

private:
	HandleStackMark stackmark;
	MonoThreadInfo *threadinfo;
};

template <typename T>
struct MonoHandle
{
	MonoHandle return_handle (MonoHandleFrame& frame)
	{
		return MonoHandle{(T**)frame.allocate_handle_in_caller (*raw)};
	}

	T* return_ptr ()
	{
		return *raw;
	}

	void New (MonoHandleFrame & frame, T * value = 0)
	{
		raw = (T**)frame.allocate_handle (value);
	}

	static MonoHandle static_new (MonoHandleFrame & frame, T * value = 0)
	{
		return MonoHandle {(T**)frame.allocate_handle (value)};
	}

	static void new_pinned (MonoDomain *domain, MonoClass *klass, MonoError *error);

	void operator=(MonoHandle p) { raw = p.raw; }
	void operator=(MonoPtr<T> p) { *raw = p; }
	void operator=(T* p) { *raw = p; }
	T* operator-> () { return *raw; }

//private:
	T ** raw;
};

} // extern C++
#endif // __cplusplus experimental

#endif /* __MONO_HANDLE_DECL_H__ */
