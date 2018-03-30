/**
 * \file
 * Low level access to thread state.
 *
 * Author:
 *	Rodrigo Kumpera (kumpera@gmail.com)
 *
 * (C) 2015 Xamarin
 */

#ifndef __MONO_THREADS_API_H__
#define __MONO_THREADS_API_H__

#include <glib.h>
#include <mono/utils/mono-publib.h>

MONO_BEGIN_DECLS

/*
>>>> WARNING WARNING WARNING <<<<

This API is experimental. It will eventually be required to properly use the rest of the raw-omp embedding API.
*/

typedef struct _MonoStackData {
	const char *function_name;
} MonoStackData;
// const here triggers a clang bug, auto is changed to static
#define MONO_STACK_DATA(x) /*const*/ MonoStackData x = { __func__ };

MONO_API gpointer
mono_threads_enter_gc_unsafe_region (gpointer* stackdata);

gpointer
mono_threads_enter_gc_unsafe_region_internal (MonoStackData *stackdata);

MONO_API void
mono_threads_exit_gc_unsafe_region (gpointer cookie, gpointer* stackdata);

void
mono_threads_exit_gc_unsafe_region_internal (gpointer cookie, MonoStackData *stackdata);

MONO_API gpointer
mono_threads_enter_gc_unsafe_region_unbalanced (gpointer* stackdata);

gpointer
mono_threads_enter_gc_unsafe_region_unbalanced_internal (MonoStackData *stackdata);

MONO_API void
mono_threads_exit_gc_unsafe_region_unbalanced (gpointer cookie, gpointer* stackdata);

void
mono_threads_exit_gc_unsafe_region_unbalanced_internal (gpointer cookie, MonoStackData *stackdata);

MONO_API void
mono_threads_assert_gc_unsafe_region (void);

MONO_API gpointer
mono_threads_enter_gc_safe_region (gpointer *stackdata);

gpointer
mono_threads_enter_gc_safe_region_internal (MonoStackData *stackdata);

MONO_API void
mono_threads_exit_gc_safe_region (gpointer cookie, gpointer *stackdata);

void
mono_threads_exit_gc_safe_region_internal (gpointer cookie, MonoStackData *stackdata);

MONO_API gpointer
mono_threads_enter_gc_safe_region_unbalanced (gpointer *stackdata);

gpointer
mono_threads_enter_gc_safe_region_unbalanced_internal (MonoStackData *stackdata);

MONO_API void
mono_threads_exit_gc_safe_region_unbalanced (gpointer cookie, gpointer *stackdata);

void
mono_threads_exit_gc_safe_region_unbalanced_internal (gpointer cookie, MonoStackData *stackdata);

MONO_API void
mono_threads_assert_gc_safe_region (void);

/*
Use those macros to limit regions of code that interact with managed memory or use the embedding API.
This will put the current thread in GC Unsafe mode.

For further explanation of what can and can't be done in GC unsafe mode:
http://www.mono-project.com/docs/advanced/runtime/docs/coop-suspend/#gc-unsafe-mode
*/
#define MONO_ENTER_GC_UNSAFE	\
	do {	\
		MONO_STACK_DATA (__gc_unsafe_dummy); \
		gpointer __gc_unsafe_cookie = mono_threads_enter_gc_unsafe_region_internal (&__gc_unsafe_dummy)

#define MONO_EXIT_GC_UNSAFE	\
		mono_threads_exit_gc_unsafe_region_internal (__gc_unsafe_cookie, &__gc_unsafe_dummy);	\
	} while (0)

#define MONO_ENTER_GC_UNSAFE_UNBALANCED	\
	do {	\
		MONO_STACK_DATA (__gc_unsafe_unbalanced_dummy); \
		gpointer __gc_unsafe_unbalanced_cookie = mono_threads_enter_gc_unsafe_region_unbalanced_internal (&__gc_unsafe_unbalanced_dummy)

#define MONO_EXIT_GC_UNSAFE_UNBALANCED	\
		mono_threads_exit_gc_unsafe_region_unbalanced_internal (__gc_unsafe_unbalanced_cookie, &__gc_unsafe_unbalanced_dummy);	\
	} while (0)

#define MONO_ENTER_GC_SAFE	\
	do {	\
		MONO_STACK_DATA (__gc_safe_dummy); \
		gpointer __gc_safe_cookie = mono_threads_enter_gc_safe_region_internal (&__gc_safe_dummy)

#define MONO_EXIT_GC_SAFE	\
		mono_threads_exit_gc_safe_region_internal (__gc_safe_cookie, &__gc_safe_dummy);	\
	} while (0)

#define MONO_ENTER_GC_SAFE_UNBALANCED	\
	do {	\
		MONO_STACK_DATA (__gc_safe_unbalanced_dummy); \
		gpointer __gc_safe_unbalanced_cookie = mono_threads_enter_gc_safe_region_unbalanced_internal (&__gc_safe_unbalanced_dummy)

#define MONO_EXIT_GC_SAFE_UNBALANCED	\
		mono_threads_exit_gc_safe_region_unbalanced_internal (__gc_safe_unbalanced_cookie, &__gc_safe_unbalanced_dummy);	\
	} while (0)

MONO_END_DECLS

#endif /* __MONO_LOGGER_H__ */
