/*
 * w32event-unix.c: Runtime support for managed Event on Unix
 *
 * Author:
 *	Ludovic Henry (luhenry@microsoft.com)
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include "w32event.h"

#include "mono/io-layer/io-layer.h"
#include "mono/io-layer/event-private.h"
#include "mono/utils/mono-logger-internals.h"
#include "mono/utils/w32handle.h"

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

static gpointer event_handle_create (struct _WapiHandle_event *event_handle, MonoW32HandleType type, gboolean manual, gboolean initial)
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
	struct _WapiHandle_event event_handle;
	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: creating %s handle",
		__func__, mono_w32handle_ops_typename (MONO_W32HANDLE_EVENT));
	return event_handle_create (&event_handle, MONO_W32HANDLE_EVENT, manual, initial);
}

static gpointer namedevent_create (gboolean manual, gboolean initial, const gunichar2 *name G_GNUC_UNUSED)
{
	gpointer handle;
	gchar *utf8_name;
	int thr_ret;

	mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: creating %s handle",
		__func__, mono_w32handle_ops_typename (MONO_W32HANDLE_NAMEDEVENT));

	/* w32 seems to guarantee that opening named objects can't race each other */
	thr_ret = wapi_namespace_lock ();
	g_assert (thr_ret == 0);

	utf8_name = g_utf16_to_utf8 (name, -1, NULL, NULL, NULL);

	handle = mono_w32handle_namespace_search_handle (MONO_W32HANDLE_NAMEDEVENT, utf8_name);
	if (handle == INVALID_HANDLE_VALUE) {
		/* The name has already been used for a different object. */
		handle = NULL;
		SetLastError (ERROR_INVALID_HANDLE);
	} else if (handle) {
		/* Not an error, but this is how the caller is informed that the event wasn't freshly created */
		SetLastError (ERROR_ALREADY_EXISTS);

		/* this is used as creating a new handle */
		mono_w32handle_ref (handle);
	} else {
		/* A new named event */
		struct _WapiHandle_namedevent namedevent_handle;

		strncpy (&namedevent_handle.sharedns.name [0], utf8_name, MAX_PATH);
		namedevent_handle.sharedns.name [MAX_PATH] = '\0';

		handle = event_handle_create ((struct _WapiHandle_event*) &namedevent_handle, MONO_W32HANDLE_NAMEDEVENT, manual, initial);
	}

	g_free (utf8_name);

	thr_ret = wapi_namespace_unlock (NULL);
	g_assert (thr_ret == 0);

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
	return SetEvent (handle);
}

gboolean
ves_icall_System_Threading_Events_ResetEvent_internal (gpointer handle)
{
	return ResetEvent (handle);
}

void
ves_icall_System_Threading_Events_CloseEvent_internal (gpointer handle)
{
	CloseHandle (handle);
}

gpointer
ves_icall_System_Threading_Events_OpenEvent_internal (MonoString *name, gint32 rights, gint32 *error)
{
	gpointer handle;

	*error = ERROR_SUCCESS;

	handle = OpenEvent (rights, FALSE, mono_string_chars (name));
	if (!handle)
		*error = GetLastError ();

	return handle;
}
