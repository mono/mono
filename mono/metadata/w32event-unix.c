/*
 * w32event-unix.c: Runtime support for managed Event on Unix
 *
 * Author:
 *	Ludovic Henry (luhenry@microsoft.com)
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include "w32event.h"

#include "w32handle-namespace.h"
#include "mono/io-layer/io-layer.h"
#include "mono/utils/mono-logger-internals.h"
#include "mono/utils/w32handle.h"

typedef struct {
	gboolean manual;
	guint32 set_count;
} MonoW32HandleEvent;

struct MonoW32HandleNamedEvent {
	MonoW32HandleEvent e;
	MonoW32HandleNamespace sharedns;
};

static gboolean event_handle_own (gpointer handle, MonoW32HandleType type)
{
	MonoW32HandleEvent *event_handle;
	gboolean ok;

	ok = mono_w32handle_lookup (handle, type, (gpointer *)&event_handle);
	if (!ok) {
		g_warning ("%s: error looking up %s handle %p",
			__func__, mono_w32handle_ops_typename (type), handle);
		return FALSE;
	}

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: owning %s handle %p",
		__func__, mono_w32handle_ops_typename (type), handle);

	if (!event_handle->manual) {
		g_assert (event_handle->set_count > 0);
		event_handle->set_count --;

		if (event_handle->set_count == 0)
			mono_w32handle_set_signal_state (handle, FALSE, FALSE);
	}

	return TRUE;
}

static void event_signal(gpointer handle)
{
	ves_icall_System_Threading_Events_SetEvent_internal (handle);
}

static gboolean event_own (gpointer handle)
{
	return event_handle_own (handle, MONO_W32HANDLE_EVENT);
}

static void namedevent_signal (gpointer handle)
{
	ves_icall_System_Threading_Events_SetEvent_internal (handle);
}

/* NB, always called with the shared handle lock held */
static gboolean namedevent_own (gpointer handle)
{
	return event_handle_own (handle, MONO_W32HANDLE_NAMEDEVENT);
}

static void event_details (gpointer data)
{
	MonoW32HandleEvent *event = (MonoW32HandleEvent *)data;
	g_print ("manual: %s, set_count: %d",
		event->manual ? "TRUE" : "FALSE", event->set_count);
}

static void namedevent_details (gpointer data)
{
	MonoW32HandleNamedEvent *namedevent = (MonoW32HandleNamedEvent *)data;
	g_print ("manual: %s, set_count: %d, name: \"%s\"",
		namedevent->e.manual ? "TRUE" : "FALSE", namedevent->e.set_count, namedevent->sharedns.name);
}

static const gchar* event_typename (void)
{
	return "Event";
}

static gsize event_typesize (void)
{
	return sizeof (MonoW32HandleEvent);
}

static const gchar* namedevent_typename (void)
{
	return "N.Event";
}

static gsize namedevent_typesize (void)
{
	return sizeof (MonoW32HandleNamedEvent);
}

void
mono_w32event_init (void)
{
	static MonoW32HandleOps event_ops = {
		NULL,			/* close */
		event_signal,		/* signal */
		event_own,		/* own */
		NULL,			/* is_owned */
		NULL,			/* special_wait */
		NULL,			/* prewait */
		event_details,	/* details */
		event_typename, /* typename */
		event_typesize, /* typesize */
	};

	static MonoW32HandleOps namedevent_ops = {
		NULL,			/* close */
		namedevent_signal,	/* signal */
		namedevent_own,		/* own */
		NULL,			/* is_owned */
		NULL,			/* special_wait */
		NULL,			/* prewait */
		namedevent_details,	/* details */
		namedevent_typename, /* typename */
		namedevent_typesize, /* typesize */
	};

	mono_w32handle_register_ops (MONO_W32HANDLE_EVENT,      &event_ops);
	mono_w32handle_register_ops (MONO_W32HANDLE_NAMEDEVENT, &namedevent_ops);

	mono_w32handle_register_capabilities (MONO_W32HANDLE_EVENT,
		(MonoW32HandleCapability)(MONO_W32HANDLE_CAP_WAIT | MONO_W32HANDLE_CAP_SIGNAL));
	mono_w32handle_register_capabilities (MONO_W32HANDLE_NAMEDEVENT,
		(MonoW32HandleCapability)(MONO_W32HANDLE_CAP_WAIT | MONO_W32HANDLE_CAP_SIGNAL));
}

gpointer
mono_w32event_create (gboolean manual, gboolean initial)
{
	gpointer handle;
	gint32 error;

	handle = ves_icall_System_Threading_Events_CreateEvent_internal (manual, initial, NULL, &error);
	if (error != ERROR_SUCCESS)
		g_assert (!handle);

	return handle;
}

void
mono_w32event_set (gpointer handle)
{
	ves_icall_System_Threading_Events_SetEvent_internal (handle);
}

void
mono_w32event_reset (gpointer handle)
{
	ves_icall_System_Threading_Events_ResetEvent_internal (handle);
}

static gpointer event_handle_create (MonoW32HandleEvent *event_handle, MonoW32HandleType type, gboolean manual, gboolean initial)
{
	gpointer handle;
	int thr_ret;

	event_handle->manual = manual;
	event_handle->set_count = (initial && !manual) ? 1 : 0;

	handle = mono_w32handle_new (type, event_handle);
	if (handle == INVALID_HANDLE_VALUE) {
		g_warning ("%s: error creating %s handle",
			__func__, mono_w32handle_ops_typename (type));
		SetLastError (ERROR_GEN_FAILURE);
		return NULL;
	}

	thr_ret = mono_w32handle_lock_handle (handle);
	g_assert (thr_ret == 0);

	if (initial)
		mono_w32handle_set_signal_state (handle, TRUE, FALSE);

	thr_ret = mono_w32handle_unlock_handle (handle);
	g_assert (thr_ret == 0);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: created %s handle %p",
		__func__, mono_w32handle_ops_typename (type), handle);

	return handle;
}

static gpointer event_create (gboolean manual, gboolean initial)
{
	MonoW32HandleEvent event_handle;
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: creating %s handle",
		__func__, mono_w32handle_ops_typename (MONO_W32HANDLE_EVENT));
	return event_handle_create (&event_handle, MONO_W32HANDLE_EVENT, manual, initial);
}

static gpointer namedevent_create (gboolean manual, gboolean initial, const gunichar2 *name G_GNUC_UNUSED)
{
	gpointer handle;
	gchar *utf8_name;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: creating %s handle",
		__func__, mono_w32handle_ops_typename (MONO_W32HANDLE_NAMEDEVENT));

	/* w32 seems to guarantee that opening named objects can't race each other */
	mono_w32handle_namespace_lock ();

	utf8_name = g_utf16_to_utf8 (name, -1, NULL, NULL, NULL);

	handle = mono_w32handle_namespace_search_handle (MONO_W32HANDLE_NAMEDEVENT, utf8_name);
	if (handle == INVALID_HANDLE_VALUE) {
		/* The name has already been used for a different object. */
		handle = NULL;
		SetLastError (ERROR_INVALID_HANDLE);
	} else if (handle) {
		/* Not an error, but this is how the caller is informed that the event wasn't freshly created */
		SetLastError (ERROR_ALREADY_EXISTS);

		/* mono_w32handle_namespace_search_handle already adds a ref to the handle */
	} else {
		/* A new named event */
		MonoW32HandleNamedEvent namedevent_handle;

		strncpy (&namedevent_handle.sharedns.name [0], utf8_name, MAX_PATH);
		namedevent_handle.sharedns.name [MAX_PATH] = '\0';

		handle = event_handle_create ((MonoW32HandleEvent*) &namedevent_handle, MONO_W32HANDLE_NAMEDEVENT, manual, initial);
	}

	g_free (utf8_name);

	mono_w32handle_namespace_unlock ();

	return handle;
}

gpointer
ves_icall_System_Threading_Events_CreateEvent_internal (MonoBoolean manual, MonoBoolean initial, MonoString *name, gint32 *error)
{
	gpointer event;

	/* Need to blow away any old errors here, because code tests
	 * for ERROR_ALREADY_EXISTS on success (!) to see if an event
	 * was freshly created */
	SetLastError (ERROR_SUCCESS);

	event = name ? namedevent_create (manual, initial, mono_string_chars (name)) : event_create (manual, initial);

	*error = GetLastError ();

	return event;
}

gboolean
ves_icall_System_Threading_Events_SetEvent_internal (gpointer handle)
{
	MonoW32HandleType type;
	MonoW32HandleEvent *event_handle;
	int thr_ret;

	if (handle == NULL) {
		SetLastError (ERROR_INVALID_HANDLE);
		return(FALSE);
	}

	switch (type = mono_w32handle_get_type (handle)) {
	case MONO_W32HANDLE_EVENT:
	case MONO_W32HANDLE_NAMEDEVENT:
		break;
	default:
		SetLastError (ERROR_INVALID_HANDLE);
		return FALSE;
	}

	if (!mono_w32handle_lookup (handle, type, (gpointer *)&event_handle)) {
		g_warning ("%s: error looking up %s handle %p",
			__func__, mono_w32handle_ops_typename (type), handle);
		return FALSE;
	}

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: setting %s handle %p",
		__func__, mono_w32handle_ops_typename (type), handle);

	thr_ret = mono_w32handle_lock_handle (handle);
	g_assert (thr_ret == 0);

	if (!event_handle->manual) {
		event_handle->set_count = 1;
		mono_w32handle_set_signal_state (handle, TRUE, FALSE);
	} else {
		mono_w32handle_set_signal_state (handle, TRUE, TRUE);
	}

	thr_ret = mono_w32handle_unlock_handle (handle);
	g_assert (thr_ret == 0);

	return TRUE;
}

gboolean
ves_icall_System_Threading_Events_ResetEvent_internal (gpointer handle)
{
	MonoW32HandleType type;
	MonoW32HandleEvent *event_handle;
	int thr_ret;

	SetLastError (ERROR_SUCCESS);

	if (handle == NULL) {
		SetLastError (ERROR_INVALID_HANDLE);
		return(FALSE);
	}

	switch (type = mono_w32handle_get_type (handle)) {
	case MONO_W32HANDLE_EVENT:
	case MONO_W32HANDLE_NAMEDEVENT:
		break;
	default:
		SetLastError (ERROR_INVALID_HANDLE);
		return FALSE;
	}

	if (!mono_w32handle_lookup (handle, type, (gpointer *)&event_handle)) {
		g_warning ("%s: error looking up %s handle %p",
			__func__, mono_w32handle_ops_typename (type), handle);
		return FALSE;
	}

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: resetting %s handle %p",
		__func__, mono_w32handle_ops_typename (type), handle);

	thr_ret = mono_w32handle_lock_handle (handle);
	g_assert (thr_ret == 0);

	if (!mono_w32handle_issignalled (handle)) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: no need to reset %s handle %p",
			__func__, mono_w32handle_ops_typename (type), handle);
	} else {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: obtained write lock on %s handle %p",
			__func__, mono_w32handle_ops_typename (type), handle);

		mono_w32handle_set_signal_state (handle, FALSE, FALSE);
	}

	event_handle->set_count = 0;

	thr_ret = mono_w32handle_unlock_handle (handle);
	g_assert (thr_ret == 0);

	return TRUE;
}

void
ves_icall_System_Threading_Events_CloseEvent_internal (gpointer handle)
{
	CloseHandle (handle);
}

gpointer
ves_icall_System_Threading_Events_OpenEvent_internal (MonoString *name, gint32 rights G_GNUC_UNUSED, gint32 *error)
{
	gpointer handle;
	gchar *utf8_name;

	*error = ERROR_SUCCESS;

	/* w32 seems to guarantee that opening named objects can't race each other */
	mono_w32handle_namespace_lock ();

	utf8_name = g_utf16_to_utf8 (mono_string_chars (name), -1, NULL, NULL, NULL);

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: Opening named event [%s]", __func__, utf8_name);

	handle = mono_w32handle_namespace_search_handle (MONO_W32HANDLE_NAMEDEVENT, utf8_name);
	if (handle == INVALID_HANDLE_VALUE) {
		/* The name has already been used for a different object. */
		*error = ERROR_INVALID_HANDLE;
		goto cleanup;
	} else if (!handle) {
		/* This name doesn't exist */
		*error = ERROR_FILE_NOT_FOUND;
		goto cleanup;
	}

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: returning named event handle %p", __func__, handle);

cleanup:
	g_free (utf8_name);

	mono_w32handle_namespace_unlock ();

	return handle;
}

MonoW32HandleNamespace*
mono_w32event_get_namespace (MonoW32HandleNamedEvent *event)
{
	return &event->sharedns;
}
