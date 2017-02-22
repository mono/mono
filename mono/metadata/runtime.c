/*
 * runtime.c: Runtime functions
 *
 * Authors:
 *  Jonathan Pryor 
 *
 * Copyright 2010 Novell, Inc (http://www.novell.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <config.h>

#include <glib.h>

#include <mono/metadata/appdomain.h>
#include <mono/metadata/class.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/runtime.h>
#include <mono/metadata/monitor.h>
#include <mono/metadata/threads-types.h>
#include <mono/metadata/threadpool.h>
#include <mono/metadata/marshal.h>
#include <mono/utils/atomic.h>

static gboolean shutting_down_inited = FALSE;
static gboolean shutting_down = FALSE;

/** 
 * mono_runtime_set_shutting_down:
 *
 * Invoked by System.Environment.Exit to flag that the runtime
 * is shutting down.
 *
 * Deprecated. This function can break the shutdown sequence.
 */
void
mono_runtime_set_shutting_down (void)
{
	shutting_down = TRUE;
}

/**
 * mono_runtime_is_shutting_down:
 *
 * Returns whether the runtime has been flagged for shutdown.
 *
 * This is consumed by the P:System.Environment.HasShutdownStarted
 * property.
 *
 */
gboolean
mono_runtime_is_shutting_down (void)
{
	return shutting_down;
}

static void
fire_process_exit_event (MonoDomain *domain, gpointer user_data)
{
	MonoError error;
	MonoClassField *field;
	gpointer pa [2];
	MonoObject *delegate, *exc;

	field = mono_class_get_field_from_name (mono_defaults.appdomain_class, "ProcessExit");
	g_assert (field);

	delegate = *(MonoObject **)(((char *)domain->domain) + field->offset); 
	if (delegate == NULL)
		return;

	pa [0] = domain;
	pa [1] = NULL;
	mono_runtime_delegate_try_invoke (delegate, pa, &exc, &error);
	mono_error_cleanup (&error);
}

static void
mono_runtime_fire_process_exit_event (void)
{
#ifndef MONO_CROSS_COMPILE
	mono_domain_foreach (fire_process_exit_event, NULL);
#endif
}


/**
 * mono_runtime_try_shutdown:
 *
 * Try to initialize runtime shutdown.
 *
 * After this call completes the thread pool will stop accepting new jobs and no further threads will be created.
 *
 * Returns: TRUE if shutdown was initiated by this call or false is other thread beat this one.
 */
gboolean
mono_runtime_try_shutdown (void)
{
	if (InterlockedCompareExchange (&shutting_down_inited, TRUE, FALSE))
		return FALSE;

	mono_runtime_fire_process_exit_event ();

	shutting_down = TRUE;

	mono_threads_set_shutting_down ();

	/* No new threads will be created after this point */

	mono_runtime_set_shutting_down ();

	/*TODO move the follow to here:
	mono_thread_suspend_all_other_threads (); OR  mono_thread_wait_all_other_threads

	mono_runtime_quit ();
	*/

	return TRUE;
}


gboolean
mono_runtime_is_critical_method (MonoMethod *method)
{
	return FALSE;
}

/*
Coordinate the creation of all remaining TLS slots in the runtime.
No further TLS slots should be created after this function finishes.
This restriction exists because AOT requires offsets to be constant
across runs.
*/
void
mono_runtime_init_tls (void)
{
	mono_marshal_init_tls ();
}

char*
mono_runtime_get_aotid (void)
{
	int i;
	guint8 aotid_sum = 0;
	MonoDomain* domain = mono_domain_get ();

	if (!domain->entry_assembly || !domain->entry_assembly->image)
		return NULL;

	guint8 (*aotid)[16] = &domain->entry_assembly->image->aotid;

	for (i = 0; i < 16; ++i)
		aotid_sum |= (*aotid)[i];

	if (aotid_sum == 0)
		return NULL;

	return mono_guid_to_string ((guint8*) aotid);
}