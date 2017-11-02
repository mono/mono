#include <config.h>
#include <glib.h>
#include <errno.h>

#include "File-c-api.h"
#include "Directory-c-api.h"

gboolean
g_file_get_contents(const gchar *filename, gchar **contents, gsize *length, GError **error)
{
    gchar *str;
    int palError;
    UnityPalFileStat st;
    long offset;
    UnityPalFileHandle* handle = NULL;
    int nread;

    handle = UnityPalOpen(filename, kFileModeOpen, 0, 0, 0, &palError);
    if (handle == NULL)
    {
        if (error != NULL)
            *error = g_error_new(G_LOG_DOMAIN, g_file_error_from_errno(palError), "Error opening file");
		return FALSE;
    }

    if (UnityPalGetFileStat(filename, &st, &palError) == 0)
    {
        if (error != NULL)
            *error = g_error_new(G_LOG_DOMAIN, g_file_error_from_errno(palError), "Error getting file attributes");
        UnityPalClose(handle, &palError);
        return FALSE;
    }

    str = g_malloc(st.length + 1);
    offset = 0;
    do
    {
        nread = UnityPalRead(handle, str + offset, st.length - offset, &palError);
        if (nread > 0)
        {
            offset += nread;
        }
    }
    while ((nread > 0 && offset < st.length) || (nread == -1 && errno == EINTR));

    UnityPalClose(handle, &palError);
    str[st.length] = '\0';
    if (length)
    {
        *length = st.length;
    }
    *contents = str;
    return TRUE;
}

gchar *
g_get_current_dir(void)
{
    int unused;
    return UnityPalDirectoryGetCurrent(&unused);
}

gboolean
g_file_test(const gchar *filename, GFileTest test)
{
    int palError = 0;
    UnityPalFileAttributes attr;

    if (filename == NULL || test == 0)
        return FALSE;

    attr = UnityPalGetFileAttributes(filename, &palError);

    if (palError != 0)
        return FALSE;

    if ((test & G_FILE_TEST_EXISTS) != 0)
    {
        return TRUE;
    }

    if ((test & G_FILE_TEST_IS_EXECUTABLE) != 0)
    {
        return UnityPalIsExecutable(filename) ? TRUE : FALSE;
    }

    if ((test & G_FILE_TEST_IS_REGULAR) != 0)
    {
        if (attr & (kFileAttributeDevice | kFileAttributeDirectory))
            return FALSE;
        return TRUE;
    }

    if ((test & G_FILE_TEST_IS_DIR) != 0)
    {
        if (attr & kFileAttributeDirectory)
            return TRUE;
    }

    /* make this last in case it is OR'd with something else */
    if ((test & G_FILE_TEST_IS_SYMLINK) != 0)
    {
        return FALSE;
    }

    return FALSE;
}
