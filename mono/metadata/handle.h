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

#ifndef __MONO_HANDLE_H__
#define __MONO_HANDLE_H__

#include <config.h>
#include <glib.h>

#include <mono/metadata/handle-decl.h>
#include <mono/metadata/object.h>
#include <mono/metadata/class.h>
#include <mono/utils/mono-error-internals.h>
#include <mono/utils/mono-threads.h>
#include <mono/utils/checked-build.h>
#include <mono/metadata/class-internals.h>

G_BEGIN_DECLS

/*
Whether this config needs stack watermark recording to know where to start scanning from.
*/
#ifdef HOST_WATCHOS
#define MONO_NEEDS_STACK_WATERMARK 1
#endif

typedef struct MonoHandleStack {
	HandleChunk *top; //alloc from here
	HandleChunk *bottom; //scan from here
#ifdef MONO_HANDLE_TRACK_SP
	gpointer stackmark_sp; // C stack pointer top when from most recent mono_stack_mark_init
#endif
	/* Chunk for storing interior pointers. Not extended right now */
	HandleChunk *interior;
} HandleStack;

typedef void *MonoRawHandle;

typedef void (*GcScanFunc) (gpointer*, gpointer);


/* If Centrinel is analyzing Mono, use the SUPPRESS macros to mark the bodies
 * of the handle macros as allowed to perform operations on raw pointers to
 * managed objects.  Take care to UNSUPPRESS the _arguments_ to the macros - we
 * want warnings if the argument uses pointers unsafely.
 */
#ifdef __CENTRINEL__
#define MONO_HANDLE_SUPPRESS_SCOPE(b) __CENTRINEL_SUPPRESS_SCOPE(b)
#define MONO_HANDLE_SUPPRESS(expr) __CENTRINEL_SUPPRESS(expr)
#define MONO_HANDLE_UNSUPPRESS(expr) __CENTRINEL_UNSUPPRESS(expr)
#else
#define MONO_HANDLE_SUPPRESS_SCOPE(b) ;
#define MONO_HANDLE_SUPPRESS(expr) (expr)
#define MONO_HANDLE_UNSUPPRESS(expr) (expr)
#endif

#ifndef MONO_HANDLE_TRACK_OWNER
MonoRawHandle mono_handle_new (MonoObject *object);
gpointer mono_handle_new_interior (gpointer rawptr);
#else
MonoRawHandle mono_handle_new (MonoObject *object, const char* owner);
gpointer mono_handle_new_interior (gpointer rawptr, const char *owner);
#endif

void mono_handle_stack_scan (HandleStack *stack, GcScanFunc func, gpointer gc_data, gboolean precise, gboolean check);
gboolean mono_handle_stack_is_empty (HandleStack *stack);
HandleStack* mono_handle_stack_alloc (void);
void mono_handle_stack_free (HandleStack *handlestack);
MonoRawHandle mono_stack_mark_pop_value (MonoThreadInfo *info, HandleStackMark *stackmark, MonoRawHandle value);
void mono_stack_mark_record_size (MonoThreadInfo *info, HandleStackMark *stackmark, const char *func_name);
void mono_handle_stack_free_domain (HandleStack *stack, MonoDomain *domain);

#ifdef MONO_HANDLE_TRACK_SP
void mono_handle_chunk_leak_check (HandleStack *handles);
#endif

static inline void
mono_stack_mark_init (MonoThreadInfo *info, HandleStackMark *stackmark)
{
#ifdef MONO_HANDLE_TRACK_SP
	gpointer sptop = &stackmark;
#endif
	HandleStack *handles = info->handle_stack;
	stackmark->size = handles->top->size;
	stackmark->chunk = handles->top;
	stackmark->interior_size = handles->interior->size;
#ifdef MONO_HANDLE_TRACK_SP
	stackmark->prev_sp = handles->stackmark_sp;
	handles->stackmark_sp = sptop;
#endif
}

static inline void
mono_stack_mark_pop (MonoThreadInfo *info, HandleStackMark *stackmark)
{
	HandleStack *handles = info->handle_stack;
	HandleChunk *old_top = stackmark->chunk;
	old_top->size = stackmark->size;
	mono_memory_write_barrier ();
	handles->top = old_top;
	handles->interior->size = stackmark->interior_size;
#ifdef MONO_HANDLE_TRACK_SP
	mono_memory_write_barrier (); /* write to top before prev_sp */
	handles->stackmark_sp = stackmark->prev_sp;
#endif
}

/*
Icall macros
*/
#define SETUP_ICALL_COMMON	\
	do { \
		ERROR_DECL (error);	\
		MonoThreadInfo *threadinfo = mono_thread_info_current ();	\
		error_init (error);	\

#define CLEAR_ICALL_COMMON	\
	mono_error_set_pending_exception (error);

#define SETUP_ICALL_FRAME	\
	HandleStackMark stackmark;	\
	mono_stack_mark_init (threadinfo, &stackmark);

#define CLEAR_ICALL_FRAME	\
	mono_stack_mark_record_size (threadinfo, &stackmark, __FUNCTION__);	\
	mono_stack_mark_pop (threadinfo, &stackmark);

#define CLEAR_ICALL_FRAME_VALUE(RESULT, HANDLE)				\
	mono_stack_mark_record_size (threadinfo, &stackmark, __FUNCTION__);	\
	(RESULT) = g_cast (mono_stack_mark_pop_value (threadinfo, &stackmark, (HANDLE)));

#define HANDLE_FUNCTION_ENTER() do {				\
	MonoThreadInfo *threadinfo = mono_thread_info_current ();	\
	SETUP_ICALL_FRAME					\

#define HANDLE_FUNCTION_RETURN()		\
	CLEAR_ICALL_FRAME;			\
	} while (0)

// Return a non-pointer or non-managed pointer, e.g. gboolean.
#define HANDLE_FUNCTION_RETURN_VAL(VAL)		\
	CLEAR_ICALL_FRAME;			\
	return (VAL);				\
	} while (0)

// Return a raw pointer from coop handle.
#define HANDLE_FUNCTION_RETURN_OBJ(HANDLE)			\
	do {							\
		void* __result = MONO_HANDLE_RAW (HANDLE);	\
		CLEAR_ICALL_FRAME;				\
		return g_cast (__result);			\
	} while (0); } while (0);

#if MONO_TYPE_SAFE_HANDLES

// Return a coop handle from coop handle.
#define HANDLE_FUNCTION_RETURN_REF(TYPE, HANDLE)			\
	do {								\
		MonoObjectHandle __result;				\
		CLEAR_ICALL_FRAME_VALUE (__result.__raw, (HANDLE).__raw); \
		return MONO_HANDLE_CAST (TYPE, __result);		\
	} while (0); } while (0);

#else

// Return a coop handle from coop handle.
#define HANDLE_FUNCTION_RETURN_REF(TYPE, HANDLE)			\
	do {								\
		MonoRawHandle __result;					\
		CLEAR_ICALL_FRAME_VALUE (__result, ((MonoRawHandle) (HANDLE))); \
		return MONO_HANDLE_CAST (TYPE, __result);		\
	} while (0); } while (0);

#endif

#ifdef MONO_NEEDS_STACK_WATERMARK

static void
mono_thread_info_pop_stack_mark (MonoThreadInfo *info, void *old_mark)
{
	info->stack_mark = old_mark;
}

static void*
mono_thread_info_push_stack_mark (MonoThreadInfo *info, void *mark)
{
	void *old = info->stack_mark;
	info->stack_mark = mark;
	return old;
}

#define SETUP_STACK_WATERMARK	\
	int __dummy;	\
	__builtin_unwind_init ();	\
	void *__old_stack_mark = mono_thread_info_push_stack_mark (threadinfo, &__dummy);

#define CLEAR_STACK_WATERMARK	\
	mono_thread_info_pop_stack_mark (threadinfo, __old_stack_mark);

#else
#define SETUP_STACK_WATERMARK
#define CLEAR_STACK_WATERMARK
#endif

#define ICALL_ENTRY()	\
	SETUP_ICALL_COMMON	\
	SETUP_ICALL_FRAME	\
	SETUP_STACK_WATERMARK

#define ICALL_RETURN()	\
	do {	\
		CLEAR_STACK_WATERMARK	\
		CLEAR_ICALL_COMMON	\
		CLEAR_ICALL_FRAME	\
		return;	\
	} while (0); } while (0)

#define ICALL_RETURN_VAL(VAL)	\
	do {	\
		CLEAR_STACK_WATERMARK	\
		CLEAR_ICALL_COMMON	\
		CLEAR_ICALL_FRAME	\
		return VAL;	\
	} while (0); } while (0)

#define ICALL_RETURN_OBJ(HANDLE)	\
	do {	\
		CLEAR_STACK_WATERMARK	\
		CLEAR_ICALL_COMMON	\
		void* __ret = MONO_HANDLE_RAW (HANDLE);	\
		CLEAR_ICALL_FRAME	\
		return g_cast (__ret);	\
	} while (0); } while (0)

/*
Handle macros/functions
*/
#ifdef MONO_HANDLE_TRACK_OWNER
#define STRINGIFY_(x) #x
#define STRINGIFY(x) STRINGIFY_(x)
#define HANDLE_OWNER (__FILE__ ":" STRINGIFY (__LINE__))
#endif

/* Have to double expand because MONO_STRUCT_OFFSET is doing token pasting on cross-compilers. */
#define MONO_HANDLE_PAYLOAD_OFFSET_(PayloadType) MONO_STRUCT_OFFSET(PayloadType, __raw)
#define MONO_HANDLE_PAYLOAD_OFFSET(TYPE) MONO_HANDLE_PAYLOAD_OFFSET_(TYPED_HANDLE_PAYLOAD_NAME (TYPE))

//XXX add functions to get/set raw, set field, set field to null, set array, set array to null
#define MONO_HANDLE_DCL(TYPE, NAME) TYPED_HANDLE_NAME(TYPE) NAME = MONO_HANDLE_NEW (TYPE, (NAME ## _raw))

// With Visual C++ compiling as C, the type of a ternary expression
// yielding two unrelated non-void pointers is the type of the first, plus a warning.
// This can be used to simulate gcc typeof extension.
// Otherwise we are forced to evaluate twice, or use C++.
#ifdef _MSC_VER
typedef struct _MonoTypeofCastHelper *MonoTypeofCastHelper; // a pointer type unrelated to anything else
#define MONO_TYPEOF_CAST(typeexpr, expr) __pragma(warning(suppress:4133))(0 ? (typeexpr) : (MonoTypeofCastHelper)(expr))
#else
#define MONO_TYPEOF_CAST(typeexpr, expr) ((__typeof__ (typeexpr))(expr))
#endif

#if MONO_TYPE_SAFE_HANDLES

#ifndef MONO_HANDLE_TRACK_OWNER

#define MONO_HANDLE_NEW(type, object) \
	(MONO_HANDLE_CAST_FOR (type) (mono_handle_new (MONO_HANDLE_TYPECHECK_FOR (type) (object))))

#else

#define MONO_HANDLE_NEW(type, object) \
	(MONO_HANDLE_CAST_FOR (type) (mono_handle_new (MONO_HANDLE_TYPECHECK_FOR (type) (object), HANDLE_OWNER)))

#endif

#define MONO_HANDLE_CAST(type, value) (MONO_HANDLE_CAST_FOR (type) ((value).__raw))
#ifdef __cplusplus
#define MONO_HANDLE_RAW(handle)     ((handle).GetRaw())
#else
#define MONO_HANDLE_RAW(handle)     (MONO_TYPEOF_CAST (*(handle).__raw, mono_handle_raw ((handle).__raw)))
#endif
#define MONO_HANDLE_IS_NULL(handle) (mono_handle_is_null ((handle).__raw))

#else // MONO_TYPE_SAFE_HANDLES

#ifndef MONO_HANDLE_TRACK_OWNER
#define MONO_HANDLE_NEW(TYPE, VALUE) (TYPED_HANDLE_NAME(TYPE))( mono_handle_new ((MonoObject*)(VALUE)) )
#else
#define MONO_HANDLE_NEW(TYPE, VALUE) (TYPED_HANDLE_NAME(TYPE))( mono_handle_new ((MonoObject*)(VALUE), HANDLE_OWNER))
#endif
#define MONO_HANDLE_CAST(TYPE, VALUE) (TYPED_HANDLE_NAME(TYPE))( VALUE )

#define MONO_HANDLE_RAW(handle)     (MONO_TYPEOF_CAST ((handle)->__raw, mono_handle_raw (handle)))
#define MONO_HANDLE_IS_NULL(handle) (mono_handle_is_null (handle))

#endif // MONO_TYPE_SAFE_HANDLES

#define MONO_BOOL(x)             (!!MONO_HANDLE_SUPPRESS (x))
#define MONO_HANDLE_BOOL(handle) (MONO_BOOL (!MONO_HANDLE_IS_NULL (handle)))

/*
WARNING WARNING WARNING

The following functions require a particular evaluation ordering to ensure correctness.
We must not have exposed handles while any sort of evaluation is happening as that very evaluation might trigger
a safepoint and break us.

This is why we evaluate index and value before any call to MONO_HANDLE_RAW or other functions that deal with naked objects.
*/
#define MONO_HANDLE_SETRAW(HANDLE, FIELD, VALUE) do {			\
		MONO_HANDLE_SUPPRESS_SCOPE(1);				\
		MonoObject *__val = MONO_HANDLE_SUPPRESS ((MonoObject*)(MONO_HANDLE_UNSUPPRESS (VALUE))); \
		MONO_OBJECT_SETREF (MONO_HANDLE_RAW (MONO_HANDLE_UNSUPPRESS (HANDLE)), FIELD, __val); \
	} while (0)

#define MONO_HANDLE_SET(HANDLE, FIELD, VALUE) do {			\
		MonoObjectHandle __val = MONO_HANDLE_CAST (MonoObject, VALUE);	\
		do {							\
			MONO_HANDLE_SUPPRESS_SCOPE(1);			\
			MONO_OBJECT_SETREF (MONO_HANDLE_RAW (MONO_HANDLE_UNSUPPRESS (HANDLE)), FIELD, MONO_HANDLE_RAW (__val)); \
		} while (0);						\
	} while (0)

#if MONO_TYPE_SAFE_HANDLES

/* N.B. RESULT is evaluated before HANDLE */
#define MONO_HANDLE_GET(RESULT, HANDLE, FIELD) do {			\
		MonoObjectHandle __dest = MONO_HANDLE_CAST (MonoObject, RESULT);	\
		MONO_HANDLE_SUPPRESS (*(gpointer*)__dest.__raw = (gpointer)MONO_HANDLE_RAW (MONO_HANDLE_UNSUPPRESS (HANDLE))->FIELD); \
	} while (0)

#else

/* N.B. RESULT is evaluated before HANDLE */
#define MONO_HANDLE_GET(RESULT, HANDLE, FIELD) do {			\
		MonoObjectHandle __dest = MONO_HANDLE_CAST(MonoObject, RESULT);	\
		MONO_HANDLE_SUPPRESS (*(gpointer*)&__dest->__raw = (gpointer)MONO_HANDLE_RAW (MONO_HANDLE_UNSUPPRESS (HANDLE))->FIELD); \
	} while (0)

#endif

// Get handle->field as a type-handle.
#define MONO_HANDLE_NEW_GET(TYPE,HANDLE,FIELD) (MONO_HANDLE_NEW(TYPE,MONO_HANDLE_SUPPRESS (MONO_HANDLE_RAW (MONO_HANDLE_UNSUPPRESS (HANDLE))->FIELD)))

// Get handle->field, where field is not a pointer (an integer or non-managed pointer).
#define MONO_HANDLE_GETVAL(HANDLE, FIELD) MONO_HANDLE_SUPPRESS (MONO_HANDLE_RAW (MONO_HANDLE_UNSUPPRESS (HANDLE))->FIELD)

// Get handle->field as a boolean, i.e. typically compare managed pointer to NULL,
// though any type is ok.
#define MONO_HANDLE_GET_BOOL(handle, field) (MONO_BOOL (MONO_HANDLE_GETVAL (handle, field)))

// handle->field = (type)value, for non-managed pointers
// This would be easier to write with the gcc extension typeof,
// but it is not widely enough implemented (i.e. Microsoft C).
#define MONO_HANDLE_SETVAL(HANDLE, FIELD, TYPE, VALUE) do {	\
		TYPE __val = (VALUE);	\
		MONO_HANDLE_SUPPRESS (MONO_HANDLE_RAW (MONO_HANDLE_UNSUPPRESS (HANDLE))->FIELD = __val); \
	 } while (0)

#define MONO_HANDLE_ARRAY_SETREF(HANDLE, IDX, VALUE) do {	\
		uintptr_t __idx = (IDX);	\
   		MonoObjectHandle __val = MONO_HANDLE_CAST (MonoObject, VALUE);		\
		{	/* FIXME scope needed by Centrinel */		\
			/* FIXME mono_array_setref_fast is not an expression. */ \
			MONO_HANDLE_SUPPRESS_SCOPE(1);			\
			mono_array_setref_fast (MONO_HANDLE_RAW (MONO_HANDLE_UNSUPPRESS (HANDLE)), __idx, MONO_HANDLE_RAW (__val)); \
		}							\
	} while (0)

#define MONO_HANDLE_ARRAY_SETVAL(HANDLE, TYPE, IDX, VALUE) do {		\
		uintptr_t __idx = (IDX);				\
   		TYPE __val = (VALUE);					\
		{	/* FIXME scope needed by Centrinel */		\
			/* FIXME mono_array_set is not an expression. */ \
			MONO_HANDLE_SUPPRESS_SCOPE(1);			\
			mono_array_set (MONO_HANDLE_RAW (MONO_HANDLE_UNSUPPRESS (HANDLE)), TYPE, __idx, __val); \
		}							\
	} while (0)

#define MONO_HANDLE_ARRAY_SETRAW(HANDLE, IDX, VALUE) do {	\
		MONO_HANDLE_SUPPRESS_SCOPE(1);			\
		uintptr_t __idx = MONO_HANDLE_UNSUPPRESS(IDX);	\
		MonoObject *__val = (MonoObject*)(VALUE);	\
		mono_array_setref_fast (MONO_HANDLE_RAW (MONO_HANDLE_UNSUPPRESS (HANDLE)), __idx, __val); \
	} while (0)

/* N.B. DEST is evaluated AFTER all the other arguments */
#define MONO_HANDLE_ARRAY_GETVAL(DEST, HANDLE, TYPE, IDX) do {		\
		MonoArrayHandle __arr = (HANDLE);			\
		uintptr_t __idx = (IDX);				\
		TYPE __result = MONO_HANDLE_SUPPRESS (mono_array_get (MONO_HANDLE_RAW(__arr), TYPE, __idx)); \
		(DEST) =  __result;					\
	} while (0)

#define MONO_HANDLE_ARRAY_GETREF(DEST, HANDLE, IDX) do {		\
		mono_handle_array_getref (MONO_HANDLE_CAST(MonoObject, (DEST)), (HANDLE), (IDX)); \
	} while (0)

#define MONO_HANDLE_ASSIGN(DESTH, SRCH)				\
	mono_handle_assign (MONO_HANDLE_CAST (MonoObject, (DESTH)), MONO_HANDLE_CAST(MonoObject, (SRCH)))

#define MONO_HANDLE_DOMAIN(HANDLE) MONO_HANDLE_SUPPRESS (mono_object_domain (MONO_HANDLE_RAW (MONO_HANDLE_CAST (MonoObject, MONO_HANDLE_UNSUPPRESS (HANDLE)))))

/* Given an object and a MonoClassField, return the value (must be non-object)
 * of the field.  It's the caller's responsibility to check that the object is
 * of the correct class. */
#define MONO_HANDLE_GET_FIELD_VAL(HANDLE,TYPE,FIELD) (*(TYPE *)(mono_handle_unsafe_field_addr (MONO_HANDLE_CAST (MonoObject, (HANDLE)), (FIELD))))
#define MONO_HANDLE_GET_FIELD_BOOL(handle, type, field) (MONO_BOOL (MONO_HANDLE_GET_FIELD_VAL ((handle), type, (field))))

#define MONO_HANDLE_NEW_GET_FIELD(HANDLE,TYPE,FIELD) MONO_HANDLE_NEW (TYPE, MONO_HANDLE_SUPPRESS (*(TYPE**)(mono_handle_unsafe_field_addr (MONO_HANDLE_CAST (MonoObject, MONO_HANDLE_UNSUPPRESS (HANDLE)), (FIELD)))))

#define MONO_HANDLE_SET_FIELD_VAL(HANDLE,TYPE,FIELD,VAL) do {		\
		MonoObjectHandle __obj = (HANDLE);			\
		MonoClassField *__field = (FIELD);			\
		TYPE __value = (VAL);					\
		*(TYPE*)(mono_handle_unsafe_field_addr (__obj, __field)) = __value; \
	} while (0)

#define MONO_HANDLE_SET_FIELD_REF(HANDLE,FIELD,VALH) do {		\
		MonoObjectHandle __obj = MONO_HANDLE_CAST (MonoObject, (HANDLE)); \
		MonoClassField *__field = (FIELD);			\
		MonoObjectHandle __value = MONO_HANDLE_CAST (MonoObject, (VALH)); \
		MONO_HANDLE_SUPPRESS (mono_gc_wbarrier_generic_store (mono_handle_unsafe_field_addr (__obj, __field), MONO_HANDLE_RAW (__value))); \
	} while (0)

/* Baked typed handles we all want */
TYPED_HANDLE_DECL (MonoString);
TYPED_HANDLE_DECL (MonoArray);
TYPED_HANDLE_DECL (MonoObject);
TYPED_HANDLE_DECL (MonoException);
TYPED_HANDLE_DECL (MonoAppContext);

#if MONO_TYPE_SAFE_HANDLES

// Structs cannot be cast to structs.
// Therefore, cast the function pointer to change its return type.
// As well, a function is needed because an anonymous struct cannot be initialized in C.
static inline MonoObjectHandle
mono_handle_cast (gpointer a)
{
	return *(MonoObjectHandle*)&a;
}

#endif

static inline MONO_ALWAYS_INLINE gboolean
mono_handle_is_null (MonoRawHandle raw_handle)
{
	MONO_HANDLE_SUPPRESS_SCOPE (1);
#if MONO_TYPE_SAFE_HANDLES
	MonoObjectHandle *handle = (MonoObjectHandle*)&raw_handle;
	return !handle->__raw || !*handle->__raw;
#else
	MonoObjectHandle handle = (MonoObjectHandle)raw_handle;
	return !handle || !handle->__raw;
#endif
}

static inline MONO_ALWAYS_INLINE gpointer
mono_handle_raw (MonoRawHandle raw_handle)
{
	MONO_HANDLE_SUPPRESS_SCOPE (1);
#if MONO_TYPE_SAFE_HANDLES
	MonoObjectHandle *handle = (MonoObjectHandle*)&raw_handle;
	return handle->__raw ? *handle->__raw : NULL;
#else
	MonoObjectHandle handle = (MonoObjectHandle)raw_handle;
	return handle ? handle->__raw : NULL;
#endif
}

/* Unfortunately MonoThreadHandle is already a typedef used for something unrelated.  So
 * the coop handle for MonoThread* is MonoThreadObjectHandle.
 */
typedef MonoThread MonoThreadObject;
TYPED_HANDLE_DECL (MonoThreadObject);

/*
This is the constant for a handle that points nowhere.
Constant handles may be initialized to it, but non-constant
handles must be NEW'ed. Uses of these are suspicious and should
be reviewed and probably changed FIXME.
*/
extern const MonoObjectHandle mono_null_value_handle;
#define NULL_HANDLE mono_null_value_handle
#define NULL_HANDLE_INIT { 0 }
#define NULL_HANDLE_STRING (MONO_HANDLE_CAST (MonoString, NULL_HANDLE))
#define NULL_HANDLE_ARRAY  (MONO_HANDLE_CAST (MonoArray,  NULL_HANDLE))

#if MONO_TYPE_SAFE_HANDLES

static inline void
mono_handle_assign (MonoObjectHandleOut dest, MonoObjectHandle src)
{
	g_assert (dest.__raw);
	MONO_HANDLE_SUPPRESS (*dest.__raw = src.__raw ? *src.__raw : NULL);
}

#else

static inline void
mono_handle_assign (MonoObjectHandleOut dest, MonoObjectHandle src)
{
	g_assert (dest);
	MONO_HANDLE_SUPPRESS (dest->__raw = (MonoObject *)(src ? MONO_HANDLE_RAW (src) : NULL));
}

#endif

/* It is unsafe to call this function directly - it does not pin the handle!  Use MONO_HANDLE_GET_FIELD_VAL(). */
static inline gpointer
mono_handle_unsafe_field_addr (MonoObjectHandle h, MonoClassField *field)
{
	return MONO_HANDLE_SUPPRESS (((gchar *)MONO_HANDLE_RAW (h)) + field->offset);
}

//FIXME this should go somewhere else
MonoStringHandle mono_string_new_handle (MonoDomain *domain, const char *data, MonoError *error);
MonoArrayHandle mono_array_new_handle (MonoDomain *domain, MonoClass *eclass, uintptr_t n, MonoError *error);
MonoArrayHandle
mono_array_new_full_handle (MonoDomain *domain, MonoClass *array_class, uintptr_t *lengths, intptr_t *lower_bounds, MonoError *error);

uintptr_t
mono_array_handle_length (MonoArrayHandle arr);

static inline void
mono_handle_array_getref (MonoObjectHandleOut dest, MonoArrayHandle array, uintptr_t index)
{
#if MONO_TYPE_SAFE_HANDLES
	MONO_HANDLE_SUPPRESS (g_assert (dest.__raw));
	MONO_HANDLE_SUPPRESS (*dest.__raw = (MonoObject*)mono_array_get(MONO_HANDLE_RAW (array), gpointer, index));
#else
	MONO_HANDLE_SUPPRESS (dest->__raw = (MonoObject *)mono_array_get(MONO_HANDLE_RAW (array), gpointer, index));
#endif
}

//FIXME
#define mono_handle_class(o) MONO_HANDLE_SUPPRESS (mono_object_class (MONO_HANDLE_RAW (MONO_HANDLE_UNSUPPRESS (o))))

/* Local handles to global GC handles and back */

uint32_t
mono_gchandle_from_handle (MonoObjectHandle handle, mono_bool pinned);

MonoObjectHandle
mono_gchandle_get_target_handle (uint32_t gchandle);

static inline gboolean
mono_gchandle_target_equal (uint32_t gchandle, MonoObjectHandle equal)
{
	// This function serves to reduce coop handle creation.
	MONO_HANDLE_SUPPRESS_SCOPE (1);
	return mono_gchandle_get_target (gchandle) == MONO_HANDLE_RAW (equal);
}

static inline void
mono_gchandle_target_is_null_or_equal (uint32_t gchandle, MonoObjectHandle equal, gboolean *is_null,
	gboolean *is_equal)
{
	// This function serves to reduce coop handle creation.
	MONO_HANDLE_SUPPRESS_SCOPE (1);
	MonoObject *target = mono_gchandle_get_target (gchandle);
	*is_null = target == NULL;
	*is_equal = target == MONO_HANDLE_RAW (equal);
}

void
mono_gchandle_set_target_handle (guint32 gchandle, MonoObjectHandle obj);

void
mono_array_handle_memcpy_refs (MonoArrayHandle dest, uintptr_t dest_idx, MonoArrayHandle src, uintptr_t src_idx, uintptr_t len);

/* Pins the MonoArray using a gchandle and returns a pointer to the
 * element with the given index (where each element is of the given
 * size.  Call mono_gchandle_free to unpin.
 */
gpointer
mono_array_handle_pin_with_size (MonoArrayHandle handle, int size, uintptr_t index, uint32_t *gchandle);

#define MONO_ARRAY_HANDLE_PIN(handle,type,index,gchandle_out) ((type*)mono_array_handle_pin_with_size (MONO_HANDLE_CAST(MonoArray,(handle)), sizeof (type), (index), (gchandle_out)))

gunichar2 *
mono_string_handle_pin_chars (MonoStringHandle s, uint32_t *gchandle_out);

gpointer
mono_object_handle_pin_unbox (MonoObjectHandle boxed_valuetype_obj, uint32_t *gchandle_out);

#ifdef __cplusplus
extern "C++" {

template <typename T>
inline gpointer
mono_handle_unbox_unsafe (MonoHandle<T> h) // unsafe
{
	MONO_HANDLE_SUPPRESS (g_assert (m_class_is_valuetype (h.GetRawObj ()->vtable->klass)));
	return MONO_HANDLE_SUPPRESS (h.GetRawObj () + 1);
}

template <typename T>
inline MonoClass*
mono_object_get_class (MonoHandle<T> h)
{
	return MONO_HANDLE_SUPPRESS (mono_object_get_class (h.GetRawObj ()));
}
}
#endif

void
mono_error_set_exception_handle (MonoError *error, MonoExceptionHandle exc);

MonoAppContextHandle
mono_context_get_handle (void);

void
mono_context_set_handle (MonoAppContextHandle new_context);

static inline guint32
mono_gchandle_new_weakref_from_handle (MonoObjectHandle handle)
{
	return mono_gchandle_new_weakref (MONO_HANDLE_SUPPRESS (MONO_HANDLE_RAW (handle)), FALSE);
}

static inline int
mono_handle_hash (MonoObjectHandle object)
{
	return mono_object_hash (MONO_HANDLE_SUPPRESS (MONO_HANDLE_RAW (object)));
}

static inline guint32
mono_gchandle_new_weakref_from_handle_track_resurrection (MonoObjectHandle handle)
{
	return mono_gchandle_new_weakref (MONO_HANDLE_SUPPRESS (MONO_HANDLE_RAW (handle)), TRUE);
}


/* experimental C++ coop handles

Goals:
1. Shared runtime -- handle allocation is the same.
2. New versions of the macros, to optionally use, with same interface.
3. Macros are so simple, you can mostly skip them, except for HANDLE_FUNCTION_ENTER. 

Depends on MonoPtr in object-internals.h etc.
Requires typesafe handles.
*/

#ifdef __cplusplus //experimental

extern "C++" {

inline
void **MonoHandleFrame::allocate_handle_in_caller (void* value)
{
	pop();
	return (void**)mono_handle_new ((MonoObject*)value);
}

inline
MonoHandleFrame::MonoHandleFrame () : do_pop(true)
{
	threadinfo = (MonoThreadInfo*)mono_thread_info_current ();
	mono_stack_mark_init (threadinfo, &stackmark);
}

inline
MonoHandleFrame::~MonoHandleFrame ()
{
	pop();
}

inline void
MonoHandleFrame::pop ()
{
	if (!do_pop) return;
	CLEAR_ICALL_FRAME
	do_pop = false;
}

template <typename T>
inline MonoHandle<T>&
MonoHandle<T>::New (T * value)
{
	__raw = (T**)mono_handle_new ((MonoObject*)value);
	return *this;
}

template <typename T>
inline MonoHandle<T>
MonoHandle<T>::static_new (T * value)
{
	return MonoHandle<T> (). New (value);
}


// FIXME duplicate declaration
MonoObjectHandle mono_object_new_pinned_handle (MonoDomain *domain, MonoClass *klass, MonoError *error);

template <typename T>
inline void
MonoHandle<T>::new_pinned (MonoDomain *domain, MonoClass *klass, MonoError *error)
{
	__raw = (T**)mono_object_new_pinned_handle (domain, klass, error).__raw;
}

/*
1. These should be drop-in replacements for the old macros -- so we only have one system.
2. They should be trivial enough that there is not really a need for them, esp.
   in the most common scenarios.
3. They are not yet complete. But given drop-in replacement,
   and the old macros work, the old macros remain for less common scenarios.
4. Requires typesafe handles.
*/
#undef HANDLE_FUNCTION_ENTER
#undef MONO_HANDLE_CAST
#undef MONO_HANDLE_GET
#undef MONO_HANDLE_GETVAL
#undef MONO_HANDLE_SETVAL
#undef MONO_HANDLE_SETRAW
#undef MONO_HANDLE_SET
#undef MONO_HANDLE_IS_NULL
#undef MONO_HANDLE_BOOL
#undef HANDLE_FUNCTION_RETURN
#undef HANDLE_FUNCTION_RETURN_VAL
#undef HANDLE_FUNCTION_RETURN_OBJ
#undef HANDLE_FUNCTION_RETURN_REF
#undef MONO_HANDLE_NEW
#undef MONO_HANDLE_NEW_GET

// A macro remains.
#define HANDLE_FUNCTION_ENTER() MonoHandleFrame local_handle_frame;

// This the point -- these are largely idiomatic
// and work with preexisting unchanged code, and do not merit macros.
// Just change the types of things.
#define MONO_HANDLE_CAST(type, value) 		((value).cast<type>())
#define MONO_HANDLE_GET(result, handle, field)	(result = (handle)->field)
#define MONO_HANDLE_GETVAL(handle, field) 	((handle)->field)
#define MONO_HANDLE_SETVAL(handle, field, type, value) (((handle)->field) = (type)value)
#define MONO_HANDLE_SETRAW(handle, field, value) ((handle)->field = (value))
#define MONO_HANDLE_SET(handle, field, value) 	((handle)->field = (value))
#define MONO_HANDLE_IS_NULL(handle) 		(!(handle))
#define MONO_HANDLE_BOOL(handle) 		(handle)

// new names
#define MONO_RETURN_RAW(handle) 		 return (handle).GetRaw ()
#define MONO_RETURN_HANDLE(handle) 		 return (handle).return_handle (local_handle_frame)

// Early/multiple return is ok.
#define HANDLE_FUNCTION_RETURN() 		/* nothing */
#define HANDLE_FUNCTION_RETURN_VAL(x) 		 return (x)
#define HANDLE_FUNCTION_RETURN_OBJ(handle) 	 MONO_RETURN_RAW (handle)
#define HANDLE_FUNCTION_RETURN_REF(type, handle) MONO_RETURN_HANDLE (handle)
#define MONO_HANDLE_NEW(type, value) 		 (MonoHandle <type> ().New (value))
//#define MONO_HANDLE_NEW(type, value) 		 ((value).NewHandle())
#define MONO_HANDLE_NEW_GET(type, handle, field) (MonoHandle <type> ().New ((handle)->field))
//#define MONO_HANDLE_NEW_GET(type, handle, field) ((handle)->field.GetRaw())
//#define MONO_HANDLE_NEW_GET(type, handle, field) ((handle)->field) // ideal
//#define MONO_HANDLE_NEW_GET(type, handle, field) (mono_new_handle ((handle)->field.GetRaw ()))

// This is just less punctuation. FIXME mono_new_handle vs. mono_handle_new.
template <typename T> inline MonoHandle<T> mono_new_handle (T* a) { return MonoHandle <T> ().New (a); }

/*
template <typename T>
inline
MonoHandle<T> MonoPtr<T>::NewHandle()
{
	return MonoHandle<T> ().New (GetRaw());
}
*/

} // extern C++

#endif // experimental C++

G_END_DECLS

#endif /* __MONO_HANDLE_H__ */
