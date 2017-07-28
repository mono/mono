#include "w32file.h"

#if defined(PLATFORM_UNITY) && defined(UNITY_USE_PLATFORM_STUBS)

void
mono_w32file_init (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

void
mono_w32file_cleanup (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

gpointer
mono_w32file_create(const gunichar2 *name, guint32 fileaccess, guint32 sharemode, guint32 createmode, guint32 attrs)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

gboolean
mono_w32file_close (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_delete (const gunichar2 *name)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_read(gpointer handle, gpointer buffer, guint32 numbytes, guint32 *bytesread)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_write (gpointer handle, gconstpointer buffer, guint32 numbytes, guint32 *byteswritten)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_flush (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_truncate (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

guint32
mono_w32file_seek (gpointer handle, gint32 movedistance, gint32 *highmovedistance, guint32 method)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gint
mono_w32file_get_type (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gboolean
mono_w32file_get_times (gpointer handle, FILETIME *create_time, FILETIME *access_time, FILETIME *write_time)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_set_times (gpointer handle, const FILETIME *create_time, const FILETIME *access_time, const FILETIME *write_time)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_filetime_to_systemtime (const FILETIME *file_time, SYSTEMTIME *system_time)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gpointer
mono_w32file_find_first (const gunichar2 *pattern, WIN32_FIND_DATA *find_data)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

gboolean
mono_w32file_find_next (gpointer handle, WIN32_FIND_DATA *find_data)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_find_close (gpointer handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_create_directory (const gunichar2 *name)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_remove_directory (const gunichar2 *name)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

guint32
mono_w32file_get_attributes (const gunichar2 *name)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gboolean
mono_w32file_get_attributes_ex (const gunichar2 *name, MonoIOStat *stat)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_set_attributes (const gunichar2 *name, guint32 attrs)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

guint32
mono_w32file_get_cwd (guint32 length, gunichar2 *buffer)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gboolean
mono_w32file_set_cwd (const gunichar2 *path)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_create_pipe (gpointer *readpipe, gpointer *writepipe, guint32 size)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_get_disk_free_space (const gunichar2 *path_name, guint64 *free_bytes_avail, guint64 *total_number_of_bytes, guint64 *total_number_of_free_bytes)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_get_volume_information (const gunichar2 *path, gunichar2 *volumename, gint volumesize, gint *outserial, gint *maxcomp, gint *fsflags, gunichar2 *fsbuffer, gint fsbuffersize)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_move (gunichar2 *path, gunichar2 *dest, gint32 *error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_replace (gunichar2 *destinationFileName, gunichar2 *sourceFileName, gunichar2 *destinationBackupFileName, guint32 flags, gint32 *error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_copy (gunichar2 *path, gunichar2 *dest, gboolean overwrite, gint32 *error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_lock (gpointer handle, gint64 position, gint64 length, gint32 *error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gboolean
mono_w32file_unlock (gpointer handle, gint64 position, gint64 length, gint32 *error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return FALSE;
}

gpointer
mono_w32file_get_console_input (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

gpointer
mono_w32file_get_console_output (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

gpointer
mono_w32file_get_console_error (void)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return NULL;
}

gint64
mono_w32file_get_file_size (gpointer handle, gint32 *error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

guint32
mono_w32file_get_drive_type (const gunichar2 *root_path_name)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

gint32
mono_w32file_get_logical_drive (guint32 len, gunichar2 *buf)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

guint32
mono_w32process_get_fileversion_info_size (gunichar2 *filename, guint32 *handle)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
	return 0;
}

void
mono_w32process_get_fileversion (MonoObject *filever, gunichar2 *filename, MonoError *error)
{
	g_assert(0 && "This function is not yet implemented for the Unity platform.");
}

#endif /* PLATFORM_UNITY && UNITY_USE_PLATFORM_STUBS */
