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
	union {
		int as_int;
		struct {
			gboolean has_stack_pointer : 1;
			gboolean has_function_name : 1;
		};
	};
	int reserved; // padding for alignment on 64bit
	gpointer stack_pointer;
	const char *function_name;
} MonoStackData;

static inline void
mono_stack_data_init (MonoStackData *stack_data, gpointer stack_pointer, const char *function_name)
{
	memset (stack_data, 0, sizeof (*stack_data));
	stack_data->stack_pointer = stack_pointer;
	stack_data->function_name = function_name;
	stack_data->has_stack_pointer = stack_pointer != NULL;
	stack_data->has_function_name = function_name != NULL;
}

// FIXME an ifdef to change __func__ to empty or further minimization.
#define MONO_STACK_DATA(x) MonoStackData x; mono_stack_data_init (&x, &x, __func__)

static inline const char*
mono_stack_data_get_function_name (const MonoStackData *stack_data, const char *fallback)
{
	// While NULL is a typical fallback, "" turns out often useful also.
	// Let the caller decide.
	return stack_data->has_function_name ? stack_data->function_name : fallback;
}

static inline gpointer
mono_stack_data_get_stack_pointer (const MonoStackData *stack_data)
{
	return stack_data->has_stack_pointer ? stack_data->stack_pointer : NULL;
}

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
