#include "os-event.h"

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

void
mono_os_event_init (MonoOSEvent *event, gboolean initial)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void
mono_os_event_destroy (MonoOSEvent *event)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void
mono_os_event_set (MonoOSEvent *event)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void
mono_os_event_reset (MonoOSEvent *event)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

MonoOSEventWaitRet
mono_os_event_wait_one (MonoOSEvent *event, guint32 timeout, gboolean alertable)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return MONO_OS_EVENT_WAIT_RET_TIMEOUT;
}

MonoOSEventWaitRet
mono_os_event_wait_multiple (MonoOSEvent **events, gsize nevents, gboolean waitall, guint32 timeout, gboolean alertable)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return MONO_OS_EVENT_WAIT_RET_TIMEOUT;
}

#endif /* PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS */
