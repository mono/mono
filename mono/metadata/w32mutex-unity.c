#include "w32mutex.h"
#include "Mutex-c-api.h"

void
mono_w32mutex_init (void)
{
}

gpointer
ves_icall_System_Threading_Mutex_CreateMutex_internal (MonoBoolean owned, MonoStringHandle name, MonoBoolean *created, MonoError *error)
{
	UnityPalMutex* mutex = NULL;

	*created = TRUE;

	if (!name) {
		mutex = UnityPalMutexNew (owned);
	} else {
		g_assertion_message ("Named mutexes are not supported by the Unity platform.");
	}

	return UnityPalMutexHandleNew(mutex);
}

MonoBoolean
ves_icall_System_Threading_Mutex_ReleaseMutex_internal (gpointer handle)
{
	UnityPalMutexUnlock(UnityPalMutexHandleGet(handle));
	return TRUE;
}

gpointer
ves_icall_System_Threading_Mutex_OpenMutex_internal (MonoStringHandle name, gint32 rights, gint32 *err, MonoError *error)
{
	g_assertion_message ("Named mutexes are not supported by the Unity platform.");
	return NULL;
}

MonoW32HandleNamespace*
mono_w32mutex_get_namespace (MonoW32HandleNamedMutex *mutex)
{
	g_assertion_message ("Named mutexes are not supported by the Unity platform.");
	return NULL;
}

void
mono_w32mutex_abandon (void)
{
}
