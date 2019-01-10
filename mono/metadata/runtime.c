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
#include <mono/metadata/class-init.h>
#include <mono/metadata/runtime.h>
#include <mono/metadata/monitor.h>
#include <mono/metadata/threads-types.h>
#include <mono/metadata/threadpool.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/gc-internals.h>
#include <mono/metadata/attach.h>
#include <mono/metadata/w32socket.h>
#include <mono/metadata/console-io.h>
#include <mono/metadata/lock-tracer.h>
#include <mono/utils/atomic.h>
#include <mono/utils/unlocked.h>
#include <mono/utils/mono-io-portability.h>

static gboolean shutting_down_inited = FALSE;
static gboolean shutting_down = FALSE;
static gboolean no_exec = FALSE;
static MonoLoadFunc load_function = NULL;
static MonoDomainFunc quit_function = NULL;

static const char *
mono_check_corlib_version_internal (void);

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
	ERROR_DECL (error);
	MonoClassField *field;
	gpointer pa [2];
	MonoObject *delegate, *exc;

	field = mono_class_get_field_from_name_full (mono_defaults.appdomain_class, "ProcessExit", NULL);
	g_assert (field);

	delegate = *(MonoObject **)(((char *)domain->domain) + field->offset);
	if (delegate == NULL)
		return;

	pa [0] = domain;
	pa [1] = NULL;
	mono_runtime_delegate_try_invoke (delegate, pa, &exc, error);
	mono_error_cleanup (error);
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
	if (mono_atomic_cas_i32 (&shutting_down_inited, TRUE, FALSE))
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

void
mono_install_runtime_load (MonoLoadFunc func)
{
	load_function = func;
}

MonoDomain*
mono_runtime_load (const char *filename, const char *runtime_version)
{
	g_assert (load_function);
	return load_function (filename, runtime_version);
}

/**
 * mono_runtime_set_no_exec:
 *
 * Instructs the runtime to operate in static mode, i.e. avoid/do not
 * allow managed code execution. This is useful for running the AOT
 * compiler on platforms which allow full-aot execution only.  This
 * should be called before mono_runtime_init ().
 */
void
mono_runtime_set_no_exec (gboolean val)
{
	no_exec = val;
}

/**
 * mono_runtime_get_no_exec:
 *
 * If true, then the runtime will not allow managed code execution.
 */
gboolean
mono_runtime_get_no_exec (void)
{
	return no_exec;
}

static char*
mono_get_corlib_version (void)
{
	ERROR_DECL (error);

	MonoClass *klass;
	MonoClassField *field;

	klass = mono_class_load_from_name (mono_defaults.corlib, "System", "Environment");
	mono_class_init_internal (klass);
	field = mono_class_get_field_from_name_full (klass, "mono_corlib_version", NULL);
	if (!field)
		return NULL;

	if (! (field->type->attrs & (FIELD_ATTRIBUTE_STATIC | FIELD_ATTRIBUTE_LITERAL)))
		return NULL;

	char *value;
	MonoTypeEnum field_type;
	const char *data = mono_class_get_field_default_value (field, &field_type);
	if (field_type != MONO_TYPE_STRING)
		return NULL;
	mono_metadata_read_constant_value (data, field_type, &value, error);
	mono_error_assert_ok (error);

	char *res = mono_string_from_blob (value, error);
	mono_error_assert_ok (error);

	return res;
}

/**
 * mono_check_corlib_version:
 * Checks that the corlib that is loaded matches the version of this runtime.
 * \returns NULL if the runtime will work with the corlib, or a \c g_malloc
 * allocated string with the error otherwise.
 */
const char*
mono_check_corlib_version (void)
{
	const char* res;
	MONO_ENTER_GC_UNSAFE;
	res = mono_check_corlib_version_internal ();
	MONO_EXIT_GC_UNSAFE;
	return res;
}

static const char *
mono_check_corlib_version_internal (void)
{
#if defined(MONO_CROSS_COMPILE)
	/* Can't read the corlib version because we only have the target class layouts */
	return NULL;
#endif

	char *result = NULL;
	char *version = mono_get_corlib_version ();
	if (!version) {
		result = g_strdup_printf ("expected corlib string (%s) but not found or not string", MONO_CORLIB_VERSION);
		goto exit;
	}
	if (strcmp (version, MONO_CORLIB_VERSION) != 0) {
		result = g_strdup_printf ("The runtime did not find the mscorlib.dll it expected. "
					  "Expected interface version %s but found %s. Check that "
					  "your runtime and class libraries are matching.",
					  MONO_CORLIB_VERSION, version);
		goto exit;
	}

	/* Check that the managed and unmanaged layout of MonoInternalThread matches */
	guint32 native_offset;
	guint32 managed_offset;
	native_offset = (guint32) MONO_STRUCT_OFFSET (MonoInternalThread, last);
	managed_offset = mono_field_get_offset (mono_class_get_field_from_name_full (mono_defaults.internal_thread_class, "last", NULL));
	if (native_offset != managed_offset)
		result = g_strdup_printf ("expected InternalThread.last field offset %u, found %u. See InternalThread.last comment", native_offset, managed_offset);
exit:
	g_free (version);
	return result;
}

/**
 * mono_runtime_init:
 * \param domain domain returned by \c mono_init
 *
 * Initialize the core AppDomain: this function will run also some
 * IL initialization code, so it needs the execution engine to be fully
 * operational.
 *
 * \c AppDomain.SetupInformation is set up in \c mono_runtime_exec_main, where
 * we know the \c entry_assembly.
 *
 */
void
mono_runtime_init (MonoDomain *domain, MonoThreadStartCB start_cb, MonoThreadAttachCB attach_cb)
{
	ERROR_DECL (error);
	mono_runtime_init_checked (domain, start_cb, attach_cb, error);
	mono_error_cleanup (error);
}

void
mono_runtime_init_checked (MonoDomain *domain, MonoThreadStartCB start_cb, MonoThreadAttachCB attach_cb, MonoError *error)
{
	HANDLE_FUNCTION_ENTER ();

	MonoAppDomainSetupHandle setup;
	MonoAppDomainHandle ad;

	error_init (error);

	mono_portability_helpers_init ();

	mono_gc_base_init ();
	mono_monitor_init ();
	mono_marshal_init ();
	mono_gc_init_icalls ();

	mono_domain_install_callbacks ();

	mono_thread_init (start_cb, attach_cb);

	if (!mono_runtime_get_no_exec ()) {
		MonoClass *klass = mono_class_load_from_name (mono_defaults.corlib, "System", "AppDomainSetup");
		setup = MONO_HANDLE_CAST (MonoAppDomainSetup, mono_object_new_pinned_handle (domain, klass, error));
		goto_if_nok (error, exit);

		klass = mono_class_load_from_name (mono_defaults.corlib, "System", "AppDomain");

		ad = MONO_HANDLE_CAST (MonoAppDomain, mono_object_new_pinned_handle (domain, klass, error));
		goto_if_nok (error, exit);

		MONO_HANDLE_SETVAL (ad, data, MonoDomain*, domain);
		domain->domain = MONO_HANDLE_RAW (ad);
		domain->setup = MONO_HANDLE_RAW (setup);
	}

	mono_thread_attach (domain);

	mono_type_initialization_init ();

	if (!mono_runtime_get_no_exec ())
		mono_create_domain_objects (domain);

	/* GC init has to happen after thread init */
	mono_gc_init ();

	/* contexts use GC handles, so they must be initialized after the GC */
	mono_context_init_checked (domain, error);
	goto_if_nok (error, exit);
	mono_context_set_default_context (domain);

#ifndef DISABLE_SOCKETS
	mono_network_init ();
#endif

	mono_console_init ();
	mono_attach_init ();

	mono_locks_tracer_init ();

	/* mscorlib is loaded before we install the load hook */
	mono_domain_fire_assembly_load (mono_defaults.corlib->assembly, NULL);

exit:
	HANDLE_FUNCTION_RETURN ();
}

/**
 * mono_runtime_cleanup:
 * \param domain unused.
 *
 * Internal routine.
 *
 * This must not be called while there are still running threads executing
 * managed code.
 */
void
mono_runtime_cleanup (MonoDomain *domain)
{
	mono_attach_cleanup ();

	/* This ends up calling any pending pending (for at most 2 seconds) */
	mono_gc_cleanup ();

	mono_thread_cleanup ();

#ifndef DISABLE_SOCKETS
	mono_network_cleanup ();
#endif
	mono_marshal_cleanup ();

	mono_type_initialization_cleanup ();

	mono_monitor_cleanup ();

	mono_icall_cleanup ();
}

/**
 * mono_install_runtime_cleanup:
 */
void
mono_install_runtime_cleanup (MonoDomainFunc func)
{
	quit_function = func;
}

/**
 * mono_runtime_quit:
 */
void
mono_runtime_quit ()
{
	if (quit_function != NULL)
		quit_function (mono_get_root_domain (), NULL);
}
