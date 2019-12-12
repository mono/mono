#include "mono-logger-internals.h"

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

void
mono_log_open_syslog(const char *ident, void *userData)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");

}

void
mono_log_write_syslog(const char *domain, GLogLevelFlags level, mono_bool hdr, const char *message)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void
mono_log_close_syslog()
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

#endif /* PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS */
