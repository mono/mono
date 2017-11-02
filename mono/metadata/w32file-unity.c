#include <config.h>
#include <glib.h>



#include "Directory-c-api.h"
#include "File-c-api.h"
#include "w32error.h"
#include "w32file.h"
#include "utils/strenc.h"
#include <mono/metadata/w32handle.h>


#ifdef HOST_WIN32

gunichar2
ves_icall_System_IO_MonoIO_get_VolumeSeparatorChar ()
{
	return (gunichar2) ':';	/* colon */
}

gunichar2
ves_icall_System_IO_MonoIO_get_DirectorySeparatorChar ()
{
	return (gunichar2) '\\';	/* backslash */
}

gunichar2
ves_icall_System_IO_MonoIO_get_AltDirectorySeparatorChar ()
{
	return (gunichar2) '/';	/* forward slash */
}

gunichar2
ves_icall_System_IO_MonoIO_get_PathSeparator ()
{
	return (gunichar2) ';';	/* semicolon */
}

void ves_icall_System_IO_MonoIO_DumpHandles (void)
{
	return;
}
#endif /* HOST_WIN32 */

gpointer
mono_w32file_create(const gunichar2 *name, guint32 fileaccess, guint32 sharemode, guint32 createmode, guint32 attrs)
{
	int error = 0;
	gpointer handle;
	gchar* palPath = mono_unicode_to_external(name);
	handle =  UnityPalOpen(palPath, (int) createmode, (int) fileaccess, (int) sharemode, attrs, &error);
	mono_w32error_set_last(error);
	g_free(palPath);

	if (handle == NULL)
		return INVALID_HANDLE_VALUE;

	return handle;
}

gboolean
mono_w32file_close (gpointer handle)
{
	if (handle == NULL)
		return FALSE;
	
	int error = 0;
	gboolean result = UnityPalClose(handle, &error);
	mono_w32error_set_last(error);
	
	return result;
}

gboolean
mono_w32file_read(gpointer handle, gpointer buffer, guint32 numbytes, guint32 *bytesread)
{
	int error = 0;
	
	*bytesread =  UnityPalRead(handle, buffer, numbytes, &error);
	mono_w32error_set_last(error);
	
	return TRUE;
}

gboolean
mono_w32file_write (gpointer handle, gconstpointer buffer, guint32 numbytes, guint32 *byteswritten)
{
	int error = 0;

	*byteswritten = UnityPalWrite(handle, buffer, numbytes, &error);
	mono_w32error_set_last(error);

	return (*byteswritten > 0);
}

gboolean
mono_w32file_flush (gpointer handle)
{
	int error = 0;
	
	gboolean result = UnityPalFlush(handle, &error);
	mono_w32error_set_last(error);
	
	return result;
}

gboolean
mono_w32file_truncate (gpointer handle)
{
	int error = 0;

	gboolean result = UnityPalTruncate(handle, &error);
	mono_w32error_set_last(error);

	return result;
}

guint32
mono_w32file_seek (gpointer handle, gint32 movedistance, gint32 *highmovedistance, guint32 method)
{
	int error = 0;
	
	int32_t result = UnityPalSeek(handle, movedistance, 0, &error);
	mono_w32error_set_last(error);

	return result;
}

gint
mono_w32file_get_type (gpointer handle)
{
	if (handle == NULL)
		return 0;

	return UnityPalGetFileType(handle);
}

gboolean
mono_w32file_get_times (gpointer handle, FILETIME *create_time, FILETIME *access_time, FILETIME *write_time)
{
	/* Not Supported in UnityPAL */
	g_assert_not_reached();
}

gboolean
mono_w32file_set_times (gpointer handle, const FILETIME *create_time, const FILETIME *access_time, const FILETIME *write_time)
{
	int error = 0;

	gboolean result = UnityPalSetFileTime(handle, create_time, access_time, write_time, &error);
	mono_w32error_set_last(error);

	return  result;
}

gpointer
mono_w32file_find_first (const gunichar2 *pattern, WIN32_FIND_DATA *find_data)
{
	gchar* palPath = mono_unicode_to_external(pattern);
	UnityPalFindHandle* findHandle = UnityPalDirectoryFindHandleNew(palPath);
	int32_t resultAttributes = 0;

	int32_t result = 0;
	const char* filename;

	result = UnityPalDirectoryFindFirstFile(findHandle, palPath, &filename, &resultAttributes);

	if (result != 0)
	{
		mono_w32error_set_last(result);
		return INVALID_HANDLE_VALUE;
	}

	find_data->dwFileAttributes = resultAttributes;

	gunichar2 *utf16_basename;
	glong bytes;
	utf16_basename = g_utf8_to_utf16 (filename, -1, NULL, &bytes, NULL);
	
	/* this next section of memset and memcpy is code from mono, the cFileName field is 
	   gunichar2 cFileName [MAX_PATH].  
	   Notes from mono:      
	   Truncating a utf16 string like this might leave the last
	   gchar incomplete
	   utf16 is 2 * utf8
	*/
	bytes *= 2;
	memset (find_data->cFileName, '\0', (MAX_PATH * 2));
	memcpy (find_data->cFileName, utf16_basename, bytes < (MAX_PATH * 2) - 2 ? bytes : (MAX_PATH * 2) - 2);

	g_free(filename);
	g_free(palPath);
	g_free(utf16_basename);

	find_data->dwReserved0 = 0;
	find_data->dwReserved1 = 0;

	find_data->cAlternateFileName [0] = 0;	

	return findHandle;
}

gboolean
mono_w32file_find_next (gpointer handle, WIN32_FIND_DATA *find_data)
{

	int32_t resultAttributes = 0;
	int32_t result;
	const char* filename;

	result = UnityPalDirectoryFindNextFile(handle, &filename, &resultAttributes);

	find_data->dwFileAttributes = resultAttributes;
	gunichar2 *utf16_basename;
	glong bytes;
	utf16_basename = g_utf8_to_utf16 (filename, -1, NULL, &bytes, NULL);
	bytes *= 2;

	memset (find_data->cFileName, '\0', (MAX_PATH * 2));
	memcpy (find_data->cFileName, utf16_basename, bytes < (MAX_PATH * 2) - 2 ? bytes : (MAX_PATH * 2) - 2);

	g_free(filename);
	g_free(utf16_basename);

	find_data->dwReserved0 = 0;
	find_data->dwReserved1 = 0;

	find_data->cAlternateFileName [0] = 0;	

	return (result == 0);
}

gboolean
mono_w32file_find_close (gpointer handle)
{
	gboolean result = UnityPalDirectoryCloseOSHandle(handle);
	UnityPalDirectoryFindHandleDelete(handle);
  
	return result;
}

gboolean
mono_w32file_create_directory (const gunichar2 *name)
{
	int error = 0;

	gchar* palPath = mono_unicode_to_external(name);
	gboolean result = UnityPalDirectoryCreate(palPath, &error);
	mono_w32error_set_last(error);
	g_free(palPath);

	return result;
}

guint32
mono_w32file_get_attributes (const gunichar2 *name)
{
	int error = 0;
	
	gchar* palPath = mono_unicode_to_external(name);
	guint32 result =  UnityPalGetFileAttributes(palPath, &error);
	mono_w32error_set_last(error);
	g_free(palPath);
	
	return result;
}

gboolean
mono_w32file_get_attributes_ex (const gunichar2 *name, MonoIOStat *stat)
{
	gboolean result;
	UnityPalFileStat palStat;
	int error = 0;

	gchar* palPath = mono_unicode_to_external(name);
	result = UnityPalGetFileStat(palPath, &palStat, &error);
	mono_w32error_set_last(error);

	if (result) {
		stat->attributes = palStat.attributes;
		stat->creation_time = palStat.creation_time;
		stat->last_access_time = palStat.last_access_time;
		stat->last_write_time = palStat.last_write_time;
		stat->length = palStat.length;
	}
	g_free(palPath);

	return result;
}

gboolean
mono_w32file_set_attributes (const gunichar2 *name, guint32 attrs)
{
	int error = 0;

	gchar* palPath = mono_unicode_to_external(name);
	gboolean result =  UnityPalSetFileAttributes(palPath, attrs, &error);
	mono_w32error_set_last(error);
	g_free(palPath);

	return result;
}

gboolean
mono_w32file_create_pipe (gpointer *readpipe, gpointer *writepipe, guint32 size)
{
	return UnityPalCreatePipe(*readpipe, *writepipe);
}

gboolean
mono_w32file_get_disk_free_space (const gunichar2 *path_name, guint64 *free_bytes_avail, guint64 *total_number_of_bytes, guint64 *total_number_of_free_bytes)
{
	g_assert_not_reached();
	return FALSE;
}

gboolean
mono_w32file_get_volume_information (const gunichar2 *path, gunichar2 *volumename, gint volumesize, gint *outserial, gint *maxcomp, gint *fsflags, gunichar2 *fsbuffer, gint fsbuffersize)
{
	g_assert_not_reached();
	return FALSE;
}

gboolean
mono_w32file_move (const gunichar2 *path, const gunichar2 *dest, gint32 *error)
{
	gboolean result;
	*error = 0;
	MONO_ENTER_GC_SAFE;
	
	gchar* palPath = mono_unicode_to_external(path);
	gchar* palDest = mono_unicode_to_external(dest);
	result =  UnityPalMoveFile(palPath, palDest, error);
	mono_w32error_set_last(*error);
	g_free(palPath);
	g_free(palDest);

	MONO_EXIT_GC_SAFE;

	return result;
}

gboolean
mono_w32file_replace (const gunichar2 *destination_file_name, const gunichar2 *source_file_name, const gunichar2 *destination_backup_file_name, guint32 flags, gint32 *error)
{
	gboolean result;
	gchar* destPath = NULL;
	gchar* sourcePath = NULL;
	gchar* destBackupPath = NULL;

	if (destination_file_name != NULL)
	{
		destPath = mono_unicode_to_external(destination_file_name);
	}

	if (source_file_name != NULL)
	{
		sourcePath = mono_unicode_to_external(source_file_name);
	}

	if (destination_backup_file_name != NULL)
	{
		destBackupPath = mono_unicode_to_external(destination_backup_file_name);
	}

	MONO_ENTER_GC_SAFE;

	result =  UnityPalReplaceFile(sourcePath, destPath, destBackupPath, 0, error);
	mono_w32error_set_last(*error);

	MONO_EXIT_GC_SAFE;

	g_free(destPath);
	g_free(sourcePath);
	g_free(destBackupPath);

	return result;
}

gboolean
mono_w32file_copy (const gunichar2 *path, const gunichar2 *dest, gboolean overwrite, gint32 *error)
{
	gboolean result;
	*error = 0;

	MONO_ENTER_GC_SAFE;
	
	gchar* palPath = mono_unicode_to_external(path);
	gchar* palDest = mono_unicode_to_external(dest);
	result = UnityPalCopyFile(palPath, palDest, overwrite, error);
	mono_w32error_set_last(*error);
	g_free(palPath);
	g_free(palDest);

	MONO_EXIT_GC_SAFE;

	return result;
}

gboolean
mono_w32file_lock (gpointer handle, gint64 position, gint64 length, gint32 *error)
{
	MONO_ENTER_GC_SAFE;

	UnityPalLock(handle, position, length, error);
	mono_w32error_set_last(*error);

	MONO_EXIT_GC_SAFE;

	return (*error == 0);
}

gboolean
mono_w32file_unlock (gpointer handle, gint64 position, gint64 length, gint32 *error)
{
	MONO_ENTER_GC_SAFE;

	UnityPalUnlock(handle, position, length, error);
	mono_w32error_set_last(*error);

	MONO_EXIT_GC_SAFE;

	return (*error == 0);
}

gpointer
mono_w32file_get_console_input (void)
{
	return UnityPalGetStdInput();
}

gpointer
mono_w32file_get_console_output (void)
{
	return UnityPalGetStdOutput();
}

gpointer
mono_w32file_get_console_error (void)
{
	return UnityPalGetStdError();
}

gint64
mono_w32file_get_file_size (gpointer handle, gint32 *error)
{
	gint64 length;

	MONO_ENTER_GC_SAFE;

	length = UnityPalGetLength(handle, error);
	mono_w32error_set_last(*error);

	MONO_EXIT_GC_SAFE;

	return length;
}

guint32
mono_w32file_get_drive_type (const gunichar2 *root_path_name)
{
	return 0;
}

gint32
mono_w32file_get_logical_drive (guint32 len, gunichar2 *buf)
{
	return -1;
}

gboolean
mono_w32file_remove_directory (const gunichar2 *name)
{
	int error = 0;

	gchar* palPath = mono_unicode_to_external (name);
	gboolean result =  UnityPalDirectoryRemove(palPath, &error);
	mono_w32error_set_last(error);
	g_free(palPath);

	return result;
}

gboolean mono_w32file_delete(const gunichar2 *name)
{
	int error = 0;

	gchar* palPath = mono_unicode_to_external (name);
	gboolean result = UnityPalDeleteFile(palPath, &error);
	mono_w32error_set_last (error);
	g_free(palPath);

	return result;
}

guint32
mono_w32file_get_cwd (guint32 length, gunichar2 *buffer)
{
	/* length is the number of characters in buffer, including the null terminator */
	/* count is the number of characters in the current directory, including the null terminator */
	gunichar2 *utf16_path;
	glong count;
	uintptr_t bytes;
	int error = 0;

	const char* palPath = UnityPalDirectoryGetCurrent(&error);
	mono_w32error_set_last (error);
	utf16_path = mono_unicode_from_external(palPath, &bytes);
	count = (bytes / 2) + 1;

	if (count <= length) {
		/* Add the terminator */
		memset (buffer, '\0', bytes+2);
		memcpy (buffer, utf16_path, bytes);
	}

	g_free(utf16_path);
	g_free(palPath);

	return count;
}

gboolean
mono_w32file_set_cwd (const gunichar2 *path)
{
	int error = 0;

	gchar* palPath = mono_unicode_to_external(path);
	gboolean result = UnityPalDirectorySetCurrent(palPath, &error);
	mono_w32error_set_last (error);
	g_free(palPath);

	return result;
}

gboolean
mono_w32file_set_length (gpointer handle, gint64 length, gint32 *error)
{
	gboolean result = UnityPalSetLength(handle, length, error);
	mono_w32error_set_last(*error);

	return result;
}

void
mono_w32file_cleanup (void)
{
}

void
mono_w32file_init (void)
{
}
