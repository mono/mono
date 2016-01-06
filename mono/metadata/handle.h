/*
 * handle.h: Handle to object in native code
 *
 * Authors:
 *  - Ludovic Henry <ludovic@xamarin.com>
 *
 * Copyright 2015 Xamarin, Inc. (www.xamarin.com)
 */

#ifndef __MONO_HANDLE_H__
#define __MONO_HANDLE_H__

#include <config.h>
#include <glib.h>

#include <mono/metadata/object.h>
#include <mono/metadata/class.h>

/*
 * DO NOT ACCESS DIRECTLY
 * USE mono_handle_obj BELOW TO ACCESS OBJ
 *
 * The field obj is not private as there is no way to do that
 * in C, but using a C++ template would simplify that a lot
 */
typedef struct {
	MonoObject *obj;
} MonoHandleStorage;

typedef MonoHandleStorage* MonoHandle;

#include "handle-arena.h"

G_BEGIN_DECLS

static inline MonoHandle
mono_handle_new (MonoObject *obj)
{
	return mono_handle_arena_new (mono_handle_arena_current (), obj);
}

static inline MonoHandle
mono_handle_elevate (MonoHandle handle)
{
	return mono_handle_arena_elevate (mono_handle_arena_current (), handle);
}

#ifndef CHECKED_BUILD

#define mono_handle_obj(handle) ((handle)->obj)

#define mono_handle_assign(handle,rawptr) do { (handle)->obj = (rawptr); } while(0)

#else

static inline void
mono_handle_check_in_critical_section ()
{
	MONO_REQ_GC_UNSAFE_MODE;
}

#define mono_handle_obj(handle) (mono_handle_check_in_critical_section (), (handle)->obj)

#define mono_handle_assign(handle,rawptr) do { mono_handle_check_in_critical_section (); (handle)->obj = (rawptr); } while (0)

#endif

static inline MonoClass*
mono_handle_class (MonoHandle handle)
{
	return mono_object_get_class (handle->obj);
}

static inline MonoDomain*
mono_handle_domain (MonoHandle handle)
{
	return mono_object_get_domain (handle->obj);
}

#define MONO_HANDLE_TYPE_DECL(type)      typedef struct { type *obj; } type ## HandleStorage ; \
	typedef type ## HandleStorage * type ## Handle
#define MONO_HANDLE_TYPE(type)           type ## Handle
#define MONO_HANDLE_NEW(type,obj)        ((type ## Handle) mono_handle_new ((MonoObject*) (obj)))
#define MONO_HANDLE_ELEVATE(type,handle) ((type ## Handle) mono_handle_elevate ((MonoObject*) (handle)->obj))

#define MONO_HANDLE_ASSIGN(handle,rawptr)	\
	do {	\
		mono_handle_assign ((handle), (rawptr));	\
	} while (0)

#define MONO_HANDLE_SETREF(handle,fieldname,value)			\
	do {								\
		MonoHandle __value = (MonoHandle) (value);		\
		MONO_PREPARE_GC_CRITICAL_REGION;					\
		MONO_OBJECT_SETREF (mono_handle_obj ((handle)), fieldname, mono_handle_obj (__value)); \
		MONO_FINISH_GC_CRITICAL_REGION;					\
	} while (0)

#define MONO_HANDLE_SET(handle,fieldname,value)	\
	do {	\
		MONO_PREPARE_GC_CRITICAL_REGION;	\
		mono_handle_obj ((handle))->fieldname = (value);	\
		MONO_FINISH_GC_CRITICAL_REGION;	\
	} while (0)

#define MONO_HANDLE_ARRAY_SETREF(handle,index,value)			\
	do {								\
		MonoHandle __value = (MonoHandle) (value);		\
		MONO_PREPARE_GC_CRITICAL_REGION;					\
		mono_array_setref (mono_handle_obj ((handle)), (index), mono_handle_obj (__value)); \
		MONO_FINISH_GC_CRITICAL_REGION;					\
	} while (0)

#define MONO_HANDLE_ARRAY_SET(handle,type,index,value)	\
	do {	\
		MONO_PREPARE_GC_CRITICAL_REGION;	\
		mono_array_set (mono_handle_obj ((handle)), type, (index), (value));	\
		MONO_FINISH_GC_CRITICAL_REGION;	\
	} while (0)

/* Some common handle types */

MONO_HANDLE_TYPE_DECL (MonoArray);

MONO_HANDLE_TYPE (MonoArray)
mono_handle_array_new (MonoDomain *domain, MonoClass *element_class, uintptr_t size);

MONO_HANDLE_TYPE (MonoArray)
mono_handle_array_new_specific (MonoVTable *vtable, uintptr_t size);

MONO_HANDLE_TYPE_DECL (MonoString);

MONO_HANDLE_TYPE (MonoString)
mono_handle_string_new (MonoDomain *domain, const gchar *text);

MONO_HANDLE_TYPE (MonoString)
mono_handle_string_new_size (MonoDomain *domain, gint32 len);

MONO_HANDLE_TYPE (MonoString)
mono_handle_string_new_utf16 (MonoDomain *domain, const guint16 *text, glong len);

gunichar2*
mono_handle_string_chars (MONO_HANDLE_TYPE (MonoString) handle);

gint32
mono_handle_string_length (MONO_HANDLE_TYPE (MonoString) handle);

gchar*
mono_handle_string_to_utf8 (MONO_HANDLE_TYPE (MonoString) handle);

gchar*
mono_handle_string_to_utf8_checked (MONO_HANDLE_TYPE (MonoString) handle, MonoError *error);

G_END_DECLS

#endif /* __MONO_HANDLE_H__ */
