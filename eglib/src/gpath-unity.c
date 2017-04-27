#include <glib.h>
#include "Path-c-api.h"

gchar *
g_path_get_dirname(const gchar *filename)
{
    return UnityPalDirectoryName(filename);
}

gchar *
g_path_get_basename(const char *filename)
{
    return UnityPalBasename(filename);
}
