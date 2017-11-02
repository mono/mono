#include "w32semaphore.h"
#include "Semaphore-c-api.h"
#include "Error-c-api.h"

void
mono_w32semaphore_init (void)
{
}

gpointer
ves_icall_System_Threading_Semaphore_CreateSemaphore_internal (gint32 initialCount, gint32 maximumCount, MonoString *name, gint32 *error)
{
	if (name != NULL)
	{
		g_assertion_message("Named semaphores are not supported by the Unity platform.");
		return NULL;
	}

	UnityPalSemaphore* semaphore = UnityPalSemaphoreNew(initialCount, maximumCount);
	*error = UnityPalGetLastError();
	return UnityPalSemaphoreHandleNew(semaphore);
}

MonoBoolean
ves_icall_System_Threading_Semaphore_ReleaseSemaphore_internal (gpointer handle, gint32 releaseCount, gint32 *prevcount)
{
	return UnityPalSemaphorePost(UnityPalSemaphoreHandleGet(handle), releaseCount, prevcount);
}

gpointer
ves_icall_System_Threading_Semaphore_OpenSemaphore_internal (MonoString *name, gint32 rights, gint32 *error)
{
	g_assertion_message("Named semaphores are not supported by the Unity platform.");
	return NULL;
}

MonoW32HandleNamespace*
mono_w32semaphore_get_namespace (MonoW32HandleNamedSemaphore *semaphore)
{
	g_assertion_message("Named semaphores are not supported by the Unity platform.");
	return NULL;
}
