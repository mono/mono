/**
 * \file
 * Cooperative suspend thread helpers
 *
 * Author:
 *	Rodrigo Kumpera (kumpera@gmail.com)
 *
 * (C) 2015 Xamarin
 */

#ifndef __MONO_THREADS_COOP_H__
#define __MONO_THREADS_COOP_H__

#include <config.h>
#include <glib.h>

#include "checked-build.h"
#include "mono-threads.h"
#include "mono-threads-api.h"

G_BEGIN_DECLS

/* JIT specific interface */
extern volatile size_t mono_polling_required;

/* Internal API */

void
mono_threads_state_poll (void);

const char*
mono_threads_suspend_policy_name (void);

gboolean
mono_threads_is_blocking_transition_enabled (void);

gboolean
mono_threads_is_cooperative_suspension_enabled (void);

gboolean
mono_threads_is_hybrid_suspension_enabled (void);

static inline gboolean
mono_threads_are_safepoints_enabled (void)
{
	return mono_threads_is_cooperative_suspension_enabled () || mono_threads_is_hybrid_suspension_enabled ();
}

static inline void
mono_threads_safepoint (void)
{
	if (G_UNLIKELY (mono_polling_required))
		mono_threads_state_poll ();
}

// 0 also used internally for uninitialized
typedef enum {
	MONO_THREADS_SUSPEND_FULL_PREEMPTIVE = 1,
	MONO_THREADS_SUSPEND_FULL_COOP,
	MONO_THREADS_SUSPEND_HYBRID
} MonoThreadsSuspendPolicy;

/* Don't use this. */
void mono_threads_suspend_override_policy (MonoThreadsSuspendPolicy new_policy);


/*
 * The following are used when detaching a thread. We need to pass the MonoThreadInfo*
 * as a paramater as the thread info TLS key is being destructed, meaning that
 * mono_thread_info_current_unchecked will return NULL, which would lead to a
 * runtime assertion error when trying to switch the state of the current thread.
 */

gpointer
mono_threads_enter_gc_safe_region_with_info (THREAD_INFO_TYPE *info, MonoStackData *stackdata);

gpointer
mono_threads_enter_gc_safe_region_with_info (THREAD_INFO_TYPE *info, MonoStackData *stackdata);

#define MONO_ENTER_GC_SAFE_WITH_INFO(info)	\
	do {	\
		MONO_STACKDATA (__gc_safe_dummy); \
		gpointer __gc_safe_cookie = mono_threads_enter_gc_safe_region_with_info ((info), &__gc_safe_dummy)

#define MONO_EXIT_GC_SAFE_WITH_INFO	MONO_EXIT_GC_SAFE

gpointer
mono_threads_enter_gc_unsafe_region_with_info (THREAD_INFO_TYPE *, MonoStackData *stackdata);

gpointer
mono_threads_enter_gc_unsafe_region_with_info (THREAD_INFO_TYPE *, MonoStackData *stackdata);

#define MONO_ENTER_GC_UNSAFE_WITH_INFO(info)	\
	do {	\
		MONO_STACKDATA (__gc_unsafe_dummy); \
		gpointer __gc_unsafe_cookie = mono_threads_enter_gc_unsafe_region_with_info ((info), &__gc_unsafe_dummy)

#define MONO_EXIT_GC_UNSAFE_WITH_INFO	MONO_EXIT_GC_UNSAFE

gpointer
mono_threads_enter_gc_unsafe_region_unbalanced_with_info (THREAD_INFO_TYPE *info, MonoStackData *stackdata);

G_END_DECLS

#endif
