#include <glib.h>
#include "mono/utils/mono-dl.h"

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

const char*
mono_dl_get_so_prefix (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

const char**
mono_dl_get_so_suffixes (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

void*
mono_dl_open_file (const char *file, int flags)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

void
mono_dl_close_handle (MonoDl *module)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void*
mono_dl_lookup_symbol_in_process (const char *symbol_name)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

void*
mono_dl_lookup_symbol (MonoDl *module, const char *symbol_name)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

int
mono_dl_convert_flags (int flags)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

char*
mono_dl_current_error_string (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

int
mono_dl_get_executable_path (char *buf, int buflen)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

const char*
mono_dl_get_system_dir (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

#endif /* PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS */
