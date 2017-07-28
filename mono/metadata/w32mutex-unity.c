#include "w32mutex.h"

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

void
mono_w32mutex_init (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gpointer
ves_icall_System_Threading_Mutex_CreateMutex_internal (MonoBoolean owned, MonoString *name, MonoBoolean *created)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

MonoBoolean
ves_icall_System_Threading_Mutex_ReleaseMutex_internal (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gpointer
ves_icall_System_Threading_Mutex_OpenMutex_internal (MonoString *name, gint32 rights, gint32 *error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

MonoW32HandleNamespace*
mono_w32mutex_get_namespace (MonoW32HandleNamedMutex *mutex)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

void
mono_w32mutex_abandon (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

#endif /* PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS */
