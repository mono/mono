/*
 * semaphores.c:  Semaphore handles
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#include <config.h>
#include <glib.h>
#include <pthread.h>
#ifdef HAVE_SEMAPHORE_H
#include <semaphore.h>
#endif
#include <errno.h>
#include <string.h>
#include <sys/time.h>

#include <mono/io-layer/wapi.h>
#include <mono/io-layer/wapi-private.h>
#include <mono/io-layer/semaphore-private.h>
#include <mono/io-layer/io-trace.h>
#include <mono/utils/mono-once.h>
#include <mono/utils/mono-logger-internals.h>
#include <mono/utils/w32handle.h>
#include <mono/metadata/w32semaphore.h>

static void sema_signal(gpointer handle);
static gboolean sema_own (gpointer handle);
static void sema_details (gpointer data);
static const gchar* sema_typename (void);
static gsize sema_typesize (void);

static void namedsema_signal (gpointer handle);
static gboolean namedsema_own (gpointer handle);
static void namedsema_details (gpointer data);
static const gchar* namedsema_typename (void);
static gsize namedsema_typesize (void);

static MonoW32HandleOps _wapi_sem_ops = {
	NULL,			/* close */
	sema_signal,		/* signal */
	sema_own,		/* own */
	NULL,			/* is_owned */
	NULL,			/* special_wait */
	NULL,			/* prewait */
	sema_details,	/* details */
	sema_typename,	/* typename */
	sema_typesize,	/* typesize */
};

static MonoW32HandleOps _wapi_namedsem_ops = {
	NULL,			/* close */
	namedsema_signal,	/* signal */
	namedsema_own,		/* own */
	NULL,			/* is_owned */
	NULL,			/* special_wait */
	NULL,			/* prewait */
	namedsema_details,	/* details */
	namedsema_typename,	/* typename */
	namedsema_typesize,	/* typesize */
};

void
_wapi_semaphore_init (void)
{
	mono_w32handle_register_ops (MONO_W32HANDLE_SEM,      &_wapi_sem_ops);
	mono_w32handle_register_ops (MONO_W32HANDLE_NAMEDSEM, &_wapi_namedsem_ops);

	mono_w32handle_register_capabilities (MONO_W32HANDLE_SEM,
		(MonoW32HandleCapability)(MONO_W32HANDLE_CAP_WAIT | MONO_W32HANDLE_CAP_SIGNAL));
	mono_w32handle_register_capabilities (MONO_W32HANDLE_NAMEDSEM,
		(MonoW32HandleCapability)(MONO_W32HANDLE_CAP_WAIT | MONO_W32HANDLE_CAP_SIGNAL));
}

static const char* sem_handle_type_to_string (MonoW32HandleType type)
{
	switch (type) {
	case MONO_W32HANDLE_SEM: return "sem";
	case MONO_W32HANDLE_NAMEDSEM: return "named sem";
	default:
		g_assert_not_reached ();
	}
}

static gboolean sem_handle_own (gpointer handle, MonoW32HandleType type)
{
	struct _WapiHandle_sem *sem_handle;

	if (!mono_w32handle_lookup (handle, type, (gpointer *)&sem_handle)) {
		g_warning ("%s: error looking up %s handle %p",
			__func__, sem_handle_type_to_string (type), handle);
		return FALSE;
	}

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: owning %s handle %p",
		__func__, sem_handle_type_to_string (type), handle);

	sem_handle->val--;

	if (sem_handle->val == 0)
		mono_w32handle_set_signal_state (handle, FALSE, FALSE);

	return TRUE;
}

static void sema_signal(gpointer handle)
{
	ves_icall_System_Threading_Semaphore_ReleaseSemaphore_internal(handle, 1, NULL);
}

static gboolean sema_own (gpointer handle)
{
	return sem_handle_own (handle, MONO_W32HANDLE_SEM);
}

static void namedsema_signal (gpointer handle)
{
	ves_icall_System_Threading_Semaphore_ReleaseSemaphore_internal (handle, 1, NULL);
}

/* NB, always called with the shared handle lock held */
static gboolean namedsema_own (gpointer handle)
{
	return sem_handle_own (handle, MONO_W32HANDLE_NAMEDSEM);
}

static void sema_details (gpointer data)
{
	struct _WapiHandle_sem *sem = (struct _WapiHandle_sem *)data;
	g_print ("val: %5u, max: %5d", sem->val, sem->max);
}

static void namedsema_details (gpointer data)
{
	struct _WapiHandle_namedsem *namedsem = (struct _WapiHandle_namedsem *)data;
	g_print ("val: %5u, max: %5d, name: \"%s\"", namedsem->s.val, namedsem->s.max, namedsem->sharedns.name);
}

static const gchar* sema_typename (void)
{
	return "Semaphore";
}

static gsize sema_typesize (void)
{
	return sizeof (struct _WapiHandle_sem);
}

static const gchar* namedsema_typename (void)
{
	return "N.Semaphore";
}

static gsize namedsema_typesize (void)
{
	return sizeof (struct _WapiHandle_namedsem);
}

gpointer OpenSemaphore (guint32 access G_GNUC_UNUSED, gboolean inherit G_GNUC_UNUSED,
			const gunichar2 *name)
{
	gpointer handle;
	gchar *utf8_name;
	int thr_ret;

	/* w32 seems to guarantee that opening named objects can't
	 * race each other
	 */
	thr_ret = wapi_namespace_lock ();
	g_assert (thr_ret == 0);
	
	utf8_name = g_utf16_to_utf8 (name, -1, NULL, NULL, NULL);
	
	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Opening named sem [%s]", __func__, utf8_name);

	handle = mono_w32handle_namespace_search_handle (MONO_W32HANDLE_NAMEDSEM,
						utf8_name);
	if (handle == INVALID_HANDLE_VALUE) {
		/* The name has already been used for a different
		 * object.
		 */
		SetLastError (ERROR_INVALID_HANDLE);
		goto cleanup;
	} else if (!handle) {
		/* This name doesn't exist */
		SetLastError (ERROR_FILE_NOT_FOUND);	/* yes, really */
		goto cleanup;
	}

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: returning named sem handle %p", __func__, handle);

cleanup:
	g_free (utf8_name);
	
	wapi_namespace_unlock (NULL);
	
	return handle;
}
