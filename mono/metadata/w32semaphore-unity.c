#include "w32semaphore.h"

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

void
mono_w32semaphore_init (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gpointer
ves_icall_System_Threading_Semaphore_CreateSemaphore_internal (gint32 initialCount, gint32 maximumCount, MonoString *name, gint32 *error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

MonoBoolean
ves_icall_System_Threading_Semaphore_ReleaseSemaphore_internal (gpointer handle, gint32 releaseCount, gint32 *prevcount)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gpointer
ves_icall_System_Threading_Semaphore_OpenSemaphore_internal (MonoString *name, gint32 rights, gint32 *error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

MonoW32HandleNamespace*
mono_w32semaphore_get_namespace (MonoW32HandleNamedSemaphore *semaphore)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

#endif /* PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS */
