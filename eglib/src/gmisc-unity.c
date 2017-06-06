#include <stdlib.h>
#include <glib.h>

#include "Environment-c-api.h"
#include "Locale-c-api.h"
#include "Path-c-api.h"

gboolean
g_hasenv(const gchar *variable)
{
	return g_getenv(variable) != NULL;
}

gchar *
g_getenv(const gchar *variable)
{
    return UnityPalGetEnvironmentVariable(variable);
}

gboolean
g_setenv(const gchar *variable, const gchar *value, gboolean overwrite)
{
    // This method assumes overwrite is always true.
    UnityPalSetEnvironmentVariable(variable, value);

    // No code in Mono actually checks the return value.
    return TRUE;
}

void
g_unsetenv(const gchar *variable)
{
    UnityPalSetEnvironmentVariable(variable, "");
}

static gboolean locale_initialized = FALSE;

gchar*
g_win32_getlocale(void)
{
    if (locale_initialized == FALSE)
    {
        UnityPalLocaleInitialize();
        locale_initialized = TRUE;
    }

    return UnityPalGetLocale();
}

gboolean
g_path_is_absolute(const char *filename)
{
    return UnityPalIsAbsolutePath(filename);
}

const gchar *
g_get_home_dir(void)
{
    return UnityPalGetHomeDirectory();
}

const char *
g_get_user_name(void)
{
    return UnityPalGetOsUserName();
}

static const char *tmp_dir;

const gchar *
g_get_tmp_dir(void)
{
    if (tmp_dir == NULL)
        tmp_dir = UnityPalGetTempPath();

    return tmp_dir;
}
