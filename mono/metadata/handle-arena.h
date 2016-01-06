#ifndef __MONO_HANDLE_PRIVATE_H__
#define __MONO_HANDLE_PRIVATE_H__

#include <mono/metadata/handle.h>

typedef struct _MonoHandleArena MonoHandleArena;

gsize
mono_handle_arena_size (void);

MonoHandle
mono_handle_arena_new (MonoHandleArena *arena, MonoObject *obj);

MonoHandle
mono_handle_arena_elevate (MonoHandleArena *arena, MonoHandle handle);

void
mono_handle_arena_stack_push (MonoHandleArena **arena_stack, MonoHandleArena *arena);

void
mono_handle_arena_stack_pop (MonoHandleArena **arena_stack, MonoHandleArena *arena);

void
mono_handle_arena_initialize (MonoHandleArena **arena_stack);

void
mono_handle_arena_deinitialize (MonoHandleArena **arena_stack);

MonoHandleArena*
mono_handle_arena_current (void);

MonoHandleArena**
mono_handle_arena_current_addr (void);

#define MONO_HANDLE_ARENA_PUSH()	\
	do {	\
		MonoHandleArena **__arena_stack = mono_handle_arena_current_addr ();	\
		MonoHandleArena *__arena = (MonoHandleArena*) g_alloca (mono_handle_arena_size ());	\
		mono_handle_arena_stack_push (__arena_stack, __arena)

#define MONO_HANDLE_ARENA_POP	\
		mono_handle_arena_stack_pop (__arena_stack, __arena);	\
	} while (0)

#define MONO_HANDLE_ARENA_POP_RETURN(handle,ret)	\
		(ret) = (handle)->obj;	\
		mono_handle_arena_stack_pop (__arena_stack, __arena);	\
	} while (0)

#define MONO_HANDLE_ARENA_POP_RETURN_ELEVATE(handle,ret_handle)	\
		*((MonoHandle**)(&(ret_handle))) = mono_handle_elevate ((MonoHandle*)(handle)); \
		mono_handle_arena_stack_pop(__arena_stack, __arena);	\
	} while (0)

#endif/*__MONO_HANDLE_PRIVATE_H__*/
