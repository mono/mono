#include <config.h>
#include "w32error.h"

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

guint32
mono_w32error_get_last (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

void
mono_w32error_set_last (guint32 error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

guint32
mono_w32error_unix_to_win32 (guint32 error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}


#endif /* PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS */
