#include "w32event.h"
#include "Handle-c-api.h"
#include "Event-c-api.h"
#include "Error-c-api.h"

void
mono_w32event_init (void)
{
}

gpointer
mono_w32event_create (gboolean manual, gboolean initial)
{
	UnityPalEvent* event = UnityPalEventNew(manual, initial);
	return UnityPalEventHandleNew(event);
}

gboolean
mono_w32event_close (gpointer handle)
{
	UnityPalHandleDestroy(handle);
	return TRUE;
}

void
mono_w32event_set (gpointer handle)
{
	UnityPalEventSet(UnityPalEventHandleGet(handle));
}

void
mono_w32event_reset (gpointer handle)
{
	UnityPalEventReset(UnityPalEventHandleGet(handle));
}

gpointer
ves_icall_System_Threading_Events_CreateEvent_internal (MonoBoolean manual, MonoBoolean initial, MonoStringHandle name, gint32 *err, MonoError *error)
{
	error_init (error);
	if (!MONO_HANDLE_IS_NULL (name))
	{
		g_assertion_message("Named events are not supported by the Unity platform.");
		return NULL;
	}

	UnityPalEvent* event = UnityPalEventNew(manual, initial);
	*err = UnityPalGetLastError();
	return UnityPalEventHandleNew(event);
}

gboolean
ves_icall_System_Threading_Events_SetEvent_internal (gpointer handle)
{
	UnityPalErrorCode result = UnityPalEventSet(UnityPalEventHandleGet(handle));
	return UnityPalSuccess(result);
}

gboolean
ves_icall_System_Threading_Events_ResetEvent_internal (gpointer handle)
{
	UnityPalErrorCode result = UnityPalEventReset(UnityPalEventHandleGet(handle));
	return UnityPalSuccess(result);
}

void
ves_icall_System_Threading_Events_CloseEvent_internal (gpointer handle)
{
	UnityPalHandleDestroy(handle);
}

gpointer
ves_icall_System_Threading_Events_OpenEvent_internal (MonoStringHandle name, gint32 rights, gint32 *err, MonoError *error)
{
	g_assertion_message("Named events are not supported by the Unity platform.");
	return NULL;
}

MonoW32HandleNamespace*
mono_w32event_get_namespace (MonoW32HandleNamedEvent *event)
{
	g_assertion_message("Named events are not supported by the Unity platform.");
	return NULL;
}
