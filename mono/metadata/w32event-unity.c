#include "w32event.h"

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

void
mono_w32event_init (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gpointer
mono_w32event_create (gboolean manual, gboolean initial)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

gboolean
mono_w32event_close (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

void
mono_w32event_set (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void
mono_w32event_reset (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gpointer
ves_icall_System_Threading_Events_CreateEvent_internal (MonoBoolean manual, MonoBoolean initial, MonoString *name, gint32 *error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

gboolean
ves_icall_System_Threading_Events_SetEvent_internal (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
ves_icall_System_Threading_Events_ResetEvent_internal (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

void
ves_icall_System_Threading_Events_CloseEvent_internal (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gpointer
ves_icall_System_Threading_Events_OpenEvent_internal (MonoString *name, gint32 rights, gint32 *error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

MonoW32HandleNamespace*
mono_w32event_get_namespace (MonoW32HandleNamedEvent *event)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

#endif /* PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS */
