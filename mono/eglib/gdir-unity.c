#include <glib.h>

#include "Directory-c-api.h"
#include "Error-c-api.h"

struct _GDir {
    UnityPalFindHandle* handle;
    gchar* current;
    gchar* next;
    const gchar* path_for_rewind;
};

static gboolean
setup_dir_handle(GDir*dir, const gchar* path, GError **error)
{
    gchar* path_search;
    char* result_file_name = NULL;
    gint unused_attributes;
    UnityPalErrorCode result;

    dir->path_for_rewind = g_strdup (path);
    path_search = g_malloc ((strlen(path) + 3)*sizeof(gchar));
    strcpy (path_search, path);
#ifdef G_OS_WIN32
    strcat (path_search, "\\*");
#else
    strcat (path_search, "/*");
#endif

    dir->handle = UnityPalDirectoryFindHandleNew(path_search);
    result = UnityPalDirectoryFindFirstFile(dir->handle, path_search, &result_file_name, &unused_attributes);
    if (!UnityPalSuccess(result)) {
        if (error)
            *error = g_error_new (G_LOG_DOMAIN, g_file_error_from_errno (result), strerror (result));
        g_free (dir);
        return FALSE;
    }

    while ((strcmp (result_file_name, ".") == 0) || (strcmp (result_file_name, "..") == 0)) {
        result = UnityPalDirectoryFindNextFile(dir->handle, &result_file_name, &unused_attributes);
        if (!UnityPalSuccess(result)) {
            result_file_name = NULL;
            break;
        }
    }

    dir->current = NULL;
    dir->next = result_file_name;
    return TRUE;
}

static void close_dir_handle(GDir* dir)
{
    UnityPalDirectoryCloseOSHandle(dir->handle);
    UnityPalDirectoryFindHandleDelete(dir->handle);
    dir->handle = 0; 
}

GDir *
g_dir_open (const gchar *path, guint flags, GError **error)
{
    GDir *dir;
    gboolean success;

    g_return_val_if_fail (path != NULL, NULL);
    g_return_val_if_fail (error == NULL || *error == NULL, NULL);

    dir = g_new0 (GDir, 1);

    success = setup_dir_handle(dir, path, error);
    if (!success)
        return NULL;
    
    return dir;
}

const gchar *
g_dir_read_name (GDir *dir)
{
    char* result_file_name;
    gint unused_attributes;
    UnityPalErrorCode result;

    g_return_val_if_fail (dir != NULL && dir->handle != 0, NULL);

    if (dir->current)
        g_free (dir->current);
    dir->current = NULL;

    dir->current = dir->next;

    if (!dir->current)
        return NULL;

    dir->next = NULL;

    do {
        result = UnityPalDirectoryFindNextFile(dir->handle, &result_file_name, &unused_attributes);
        if (!UnityPalSuccess(result)) {
            dir->next = NULL;
            return dir->current;
        }
    } while ((strcmp (result_file_name, ".") == 0) || (strcmp (result_file_name, "..") == 0));

    dir->next = result_file_name;
    return dir->current;
}

void
g_dir_rewind (GDir *dir)
{
    g_return_if_fail (dir != NULL && dir->handle != NULL);

    close_dir_handle(dir);
    setup_dir_handle(dir, dir->path_for_rewind, NULL);
}

void
g_dir_close (GDir *dir)
{
    g_return_if_fail (dir != NULL && dir->handle != 0);
    
    if (dir->current)
        g_free (dir->current);
    dir->current = NULL;
    if (dir->next)
        g_free (dir->next);
    dir->next = NULL;
    if (dir->path_for_rewind)
        g_free(dir->path_for_rewind);
    dir->path_for_rewind = NULL;
    close_dir_handle(dir);
    g_free (dir);
}
