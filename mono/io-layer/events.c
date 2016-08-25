/*
 * events.c:  Event handles
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#include <config.h>
#include <glib.h>
#include <pthread.h>
#include <string.h>

#include <mono/io-layer/wapi.h>
#include <mono/io-layer/wapi-private.h>
#include <mono/io-layer/event-private.h>
#include <mono/io-layer/io-trace.h>
#include <mono/utils/mono-once.h>
#include <mono/utils/mono-logger-internals.h>
#include <mono/utils/w32handle.h>
#include <mono/metadata/w32event.h>

static void event_signal(gpointer handle);
static gboolean event_own (gpointer handle);
static void event_details (gpointer data);
static const gchar* event_typename (void);
static gsize event_typesize (void);

static void namedevent_signal (gpointer handle);
static gboolean namedevent_own (gpointer handle);
static void namedevent_details (gpointer data);
static const gchar* namedevent_typename (void);
static gsize namedevent_typesize (void);

static MonoW32HandleOps _wapi_event_ops = {
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

static MonoW32HandleOps _wapi_namedevent_ops = {
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

void
_wapi_event_init (void)
{
	mono_w32handle_register_ops (MONO_W32HANDLE_EVENT,      &_wapi_event_ops);
	mono_w32handle_register_ops (MONO_W32HANDLE_NAMEDEVENT, &_wapi_namedevent_ops);

	mono_w32handle_register_capabilities (MONO_W32HANDLE_EVENT,
		(MonoW32HandleCapability)(MONO_W32HANDLE_CAP_WAIT | MONO_W32HANDLE_CAP_SIGNAL));
	mono_w32handle_register_capabilities (MONO_W32HANDLE_NAMEDEVENT,
		(MonoW32HandleCapability)(MONO_W32HANDLE_CAP_WAIT | MONO_W32HANDLE_CAP_SIGNAL));
}

static const char* event_handle_type_to_string (MonoW32HandleType type)
{
	switch (type) {
	case MONO_W32HANDLE_EVENT: return "event";
	case MONO_W32HANDLE_NAMEDEVENT: return "named event";
	default:
		g_assert_not_reached ();
	}
}

static gboolean event_handle_own (gpointer handle, MonoW32HandleType type)
{
	struct _WapiHandle_event *event_handle;
	gboolean ok;

	ok = mono_w32handle_lookup (handle, type, (gpointer *)&event_handle);
	if (!ok) {
		g_warning ("%s: error looking up %s handle %p",
			__func__, event_handle_type_to_string (type), handle);
		return FALSE;
	}

	MONO_TRACE (G_LOG_LEVEL_DEBUG, MONO_TRACE_IO_LAYER, "%s: owning %s handle %p",
		__func__, event_handle_type_to_string (type), handle);

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
	struct _WapiHandle_event *event = (struct _WapiHandle_event *)data;
	g_print ("manual: %s, set_count: %d",
		event->manual ? "TRUE" : "FALSE", event->set_count);
}

static void namedevent_details (gpointer data)
{
	struct _WapiHandle_namedevent *namedevent = (struct _WapiHandle_namedevent *)data;
	g_print ("manual: %s, set_count: %d, name: \"%s\"",
		namedevent->e.manual ? "TRUE" : "FALSE", namedevent->e.set_count, namedevent->sharedns.name);
}

static const gchar* event_typename (void)
{
	return "Event";
}

static gsize event_typesize (void)
{
	return sizeof (struct _WapiHandle_event);
}

static const gchar* namedevent_typename (void)
{
	return "N.Event";
}

static gsize namedevent_typesize (void)
{
	return sizeof (struct _WapiHandle_namedevent);
}
