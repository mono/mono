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

#include "object.h"
#include "class.h"
#include "class-internals.h"
#include "threads-types.h"

#include "mono/utils/mono-threads-coop.h"

G_BEGIN_DECLS

typedef struct _MonoHandle MonoHandle;

/*
 * DO NOT ACCESS DIRECTLY
 * USE mono_handle_obj BELOW TO ACCESS OBJ
 *
 * The field obj is not private as there is no way to do that
 * in C, but using a C++ template would simplify that a lot
 */
struct _MonoHandle {
	MonoObject *obj;
};

MonoHandle*
mono_handle_new (MonoObject *obj);

MonoHandle*
mono_handle_elevate (MonoHandle *handle);

#ifndef CHECKED_BUILD

#define mono_handle_obj(handle) ((handle)->obj)

#define mono_handle_assign(handle,rawptr) do { (handle)->obj = (rawptr); } while(0)

#else

static inline void
mono_handle_check_in_critical_section (const gchar* file, const gint lineno)
{
	MonoThreadInfo *info = (MonoThreadInfo*) mono_thread_info_current_unchecked ();
	if (info && !info->inside_critical_region)
		g_error ("Assertion at %s:%d: mono_handle_check_in_critical_section failed", file, lineno);
}

#define mono_handle_obj(handle) (mono_handle_check_in_critical_section (__FILE__, __LINE__), (handle)->obj)

#define mono_handle_assign(handle,rawptr) do { mono_handle_check_in_critical_section (__FILE__, __LINE__); (handle)->obj = (rawptr); } while (0)

#endif

static inline MonoClass*
mono_handle_class (MonoHandle *handle)
{
	return handle->obj->vtable->klass;
}

static inline MonoDomain*
mono_handle_domain (MonoHandle *handle)
{
	return handle->obj->vtable->domain;
}

#define MONO_HANDLE_TYPE_DECL(type)      typedef struct { type *obj; } type ## Handle
#define MONO_HANDLE_TYPE(type)           type ## Handle
#define MONO_HANDLE_NEW(type,obj)        ((type ## Handle*) mono_handle_new ((MonoObject*) (obj)))
#define MONO_HANDLE_ELEVATE(type,handle) ((type ## Handle*) mono_handle_elevate ((MonoObject*) (handle)->obj))

#define MONO_HANDLE_ASSIGN(handle,rawptr)	\
	do {	\
		mono_handle_assign ((handle), (rawptr));	\
	} while (0)

#define MONO_HANDLE_SETREF(handle,fieldname,value)	\
	do {	\
		gpointer __value = (value);	\
		MONO_PREPARE_CRITICAL;	\
		MONO_OBJECT_SETREF (mono_handle_obj ((handle)), fieldname, __value);	\
		MONO_FINISH_CRITICAL;	\
	} while (0)

#define MONO_HANDLE_SET(handle,fieldname,value)	\
	do {	\
		MONO_PREPARE_CRITICAL;	\
		mono_handle_obj ((handle))->fieldname = (value);	\
		MONO_FINISH_CRITICAL;	\
	} while (0)

#define MONO_HANDLE_ARRAY_SETREF(handle,index,value)	\
	do {	\
		gpointer __value = (value);	\
		MONO_PREPARE_CRITICAL;	\
		mono_array_setref (mono_handle_obj ((handle)), (index), __value);	\
		MONO_FINISH_CRITICAL;	\
	} while (0)

#define MONO_HANDLE_ARRAY_SET(handle,type,index,value)	\
	do {	\
		MONO_PREPARE_CRITICAL;	\
		mono_array_set (mono_handle_obj ((handle)), (type), (index), (value));	\
		MONO_FINISH_CRITICAL;	\
	} while (0)

/* handle arena specific functions */

typedef struct _MonoHandleArena MonoHandleArena;

gsize
mono_handle_arena_size (gsize nb_handles);

void
mono_handle_arena_push (MonoHandleArena *arena, gsize nb_handles);

void
mono_handle_arena_pop (MonoHandleArena *arena, gsize nb_handles);

void
mono_handle_arena_init_thread (MonoInternalThread *thread);

void
mono_handle_arena_deinit_thread (MonoInternalThread *thread);

#define MONO_HANDLE_ARENA_PUSH(nb_handles)	\
	do {	\
		gsize __arena_nb_handles = (nb_handles);	\
		MonoHandleArena *__arena = (MonoHandleArena*) g_alloca (mono_handle_arena_size (__arena_nb_handles));	\
		mono_handle_arena_push (__arena, __arena_nb_handles)

#define MONO_HANDLE_ARENA_POP	\
		mono_handle_arena_pop (__arena, __arena_nb_handles);	\
	} while (0)

#define MONO_HANDLE_ARENA_POP_RETURN(handle,ret)	\
		(ret) = (handle)->obj;	\
		mono_handle_arena_pop (__arena, __arena_nb_handles);	\
	} while (0)

#define MONO_HANDLE_ARENA_POP_RETURN_ELEVATE(handle,ret_handle)	\
		*((MonoHandle**)(&(ret_handle))) = mono_handle_elevate ((MonoHandle*)(handle)); \
		mono_handle_arena_pop(__arena, __arena_nb_handles);	\
	} while (0)

/* Some common handle types */

MONO_HANDLE_TYPE_DECL (MonoArray);
MONO_HANDLE_TYPE_DECL (MonoString);

G_END_DECLS

#endif /* __MONO_HANDLE_H__ */
