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
	gpointer stackpointer;
	const char *function_name;
} MonoStackData;

static inline void
mono_stackdata_init (MonoStackData *stackdata, gpointer stackpointer, const char *function_name)
{
	memset (stackdata, 0, sizeof (*stackdata));
	stackdata->stackpointer = stackpointer;
	stackdata->function_name = function_name;
}

// FIXME an ifdef to change __func__ to empty or further minimization.
#define MONO_STACKDATA(x) MonoStackData x; mono_stackdata_init (&x, &x, __func__)

static inline const char*
mono_stackdata_get_function_name (const MonoStackData *stackdata, const char *fallback)
{
	// While NULL is a typical fallback, "" turns out often useful also.
	// Let the caller decide.
	const char *function_name = stackdata->function_name;
	return function_name ? function_name : fallback;
}

static inline gpointer
mono_stackdata_get_stackpointer (const MonoStackData *stackdata)
{
	return stackdata->stackpointer;
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
		MONO_STACKDATA (__gc_unsafe_dummy); \
		gpointer __gc_unsafe_cookie = mono_threads_enter_gc_unsafe_region_internal (&__gc_unsafe_dummy)

#define MONO_EXIT_GC_UNSAFE	\
		mono_threads_exit_gc_unsafe_region_internal (__gc_unsafe_cookie, &__gc_unsafe_dummy);	\
	} while (0)

#define MONO_ENTER_GC_UNSAFE_UNBALANCED	\
	do {	\
		MONO_STACKDATA (__gc_unsafe_unbalanced_dummy); \
		gpointer __gc_unsafe_unbalanced_cookie = mono_threads_enter_gc_unsafe_region_unbalanced_internal (&__gc_unsafe_unbalanced_dummy)

#define MONO_EXIT_GC_UNSAFE_UNBALANCED	\
		mono_threads_exit_gc_unsafe_region_unbalanced_internal (__gc_unsafe_unbalanced_cookie, &__gc_unsafe_unbalanced_dummy);	\
	} while (0)

#define MONO_ENTER_GC_SAFE	\
	do {	\
		MONO_STACKDATA (__gc_safe_dummy); \
		gpointer __gc_safe_cookie = mono_threads_enter_gc_safe_region_internal (&__gc_safe_dummy)

#define MONO_EXIT_GC_SAFE	\
		mono_threads_exit_gc_safe_region_internal (__gc_safe_cookie, &__gc_safe_dummy);	\
	} while (0)

#define MONO_ENTER_GC_SAFE_UNBALANCED	\
	do {	\
		MONO_STACKDATA (__gc_safe_unbalanced_dummy); \
		gpointer __gc_safe_unbalanced_cookie = mono_threads_enter_gc_safe_region_unbalanced_internal (&__gc_safe_unbalanced_dummy)

#define MONO_EXIT_GC_SAFE_UNBALANCED	\
		mono_threads_exit_gc_safe_region_unbalanced_internal (__gc_safe_unbalanced_cookie, &__gc_safe_unbalanced_dummy);	\
	} while (0)

MONO_END_DECLS

#endif /* __MONO_LOGGER_H__ */
