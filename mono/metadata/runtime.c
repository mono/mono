/**
 * \file
 * Runtime functions
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
#include <mono/utils/unlocked.h>

static gboolean shutting_down_inited = FALSE;
static gboolean shutting_down = FALSE;

/**
 * mono_runtime_set_shutting_down:
 * \deprecated This function can break the shutdown sequence.
 *
 * Invoked by \c System.Environment.Exit to flag that the runtime
 * is shutting down.
 */
void
mono_runtime_set_shutting_down (void)
{
	UnlockedWriteBool (&shutting_down, TRUE);
}

/**
 * mono_runtime_is_shutting_down:
 * This is consumed by the \c P:System.Environment.HasShutdownStarted property.
 * \returns whether the runtime has been flagged for shutdown.
 */
gboolean
mono_runtime_is_shutting_down (void)
{
	return UnlockedReadBool (&shutting_down);
}

static void
fire_process_exit_event (MonoDomain *domain, gpointer user_data)
{
	MonoMethod * method = mono_class_get_method_from_name_flags (
		mono_defaults.appdomain_class, "QueueProcessExitEvent", 
		0, METHOD_ATTRIBUTE_PRIVATE
	);

	// FIXME: The assert causes a crash during make... maybe because mscorlib is old?
	if (!method)
		return;

	g_assert (method);

	MonoError error;
	MonoObject * exc = NULL;
	mono_runtime_try_invoke (method, domain->domain, NULL, &exc, &error);

	if (!mono_error_ok (&error)) {
		if (exc)
			mono_error_cleanup (&error);
		else
			exc = (MonoObject*)mono_error_convert_to_exception (&error);
	}

	if (exc)
		mono_print_unhandled_exception (exc);
}

static void
mono_runtime_fire_process_exit_event (void)
{
#ifndef MONO_CROSS_COMPILE
	mono_domain_foreach (fire_process_exit_event, NULL);

	MonoMethod * method = mono_class_get_method_from_name_flags (
		mono_defaults.appdomain_class, "SetProcessExitEventQueueReady", 
		0, METHOD_ATTRIBUTE_PRIVATE | METHOD_ATTRIBUTE_STATIC
	);
	MonoError error;
	MonoObject * exc = NULL;

	if (method) {
		// This operation can't fail
		mono_runtime_try_invoke (method, NULL, NULL, &exc, &error);
		exc = NULL;
	}

	mono_gc_finalize_notify ();

	method = mono_class_get_method_from_name_flags (
		mono_defaults.appdomain_class, "WaitForProcessExitEventQueueToDrain", 
		0, METHOD_ATTRIBUTE_PRIVATE | METHOD_ATTRIBUTE_STATIC
	);

	// If mscorlib is outdated this method doesn't exist, and requiring it
	//  will cause builds to fail before they can update mscorlib.
	if (!method)
		return;

	mono_runtime_try_invoke (method, NULL, NULL, &exc, &error);

	if (!mono_error_ok (&error)) {
		if (exc)
			mono_error_cleanup (&error);
		else
			exc = (MonoObject*)mono_error_convert_to_exception (&error);
	}

	if (exc)
		mono_print_unhandled_exception (exc);
#endif
}

void mono_runtime_flush_appdomain_processexit_queue (void)
{
	MonoMethod * method = mono_class_get_method_from_name_flags (
		mono_defaults.appdomain_class, "InvokeQueuedProcessExitEvents",
		0, METHOD_ATTRIBUTE_PRIVATE | METHOD_ATTRIBUTE_STATIC
	);

	// If mscorlib is outdated this method doesn't exist, and requiring it
	//  will cause builds to fail before they can update mscorlib.
	if (!method)
		return;

	MonoError error;
	MonoObject * exc = NULL;
	mono_runtime_try_invoke (method, NULL, NULL, &exc, &error);

	if (!mono_error_ok (&error)) {
		if (exc)
			mono_error_cleanup (&error);
		else
			exc = (MonoObject*)mono_error_convert_to_exception (&error);
	}

	if (exc)
		mono_print_unhandled_exception (exc);
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

	mono_runtime_set_shutting_down ();

	mono_threads_set_shutting_down ();

	/* No new threads will be created after this point */

	/*TODO move the follow to here:
	mono_thread_suspend_all_other_threads (); OR  mono_thread_wait_all_other_threads

	mono_runtime_quit ();
	*/

	return TRUE;
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
